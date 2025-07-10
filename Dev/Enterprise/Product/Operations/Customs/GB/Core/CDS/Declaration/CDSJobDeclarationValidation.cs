using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.MessagingRules;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public partial class CDSJobDeclarationValidation : Business.Declaration.JobDeclarationValidation
	{
		public CDSJobDeclarationValidation(JobDeclaration parent)
		: base(parent)
		{
		}

		public new JobDeclaration Parent
		{
			get { return base.Parent; }
		}

		protected override void CheckJE_CustomsProfile()
		{
			const string Phase1ExportCDSInventoryError = "CDS Export Inventory Dual Running is in Phase One. Do not send an inventory-linked declaration to CDS. Instead either make this declaration non-inventory or select a CHIEF profile/badge.";
			const string BadgeHasNoDuns = "Your branch does not have a DUNS account number, which is needed for communication with Pentant; please refer to 1BGB059";
			const string MessageErrorBadgeCodeNotFound = "No badge code was found in the registry for the specified criteria. Check the port codes and the direction. If these are correct then have your administrator check the badges in the registry.";
			var messageErrorBadgeCodeNotInList = $"You must have a valid Badge Code. Badge Codes are setup under Admin -> System -> Registry -> {GBCustomsDataRegistry.Instance.BadgeCodes.Inner.Location}";

			base.CheckJE_CustomsProfile();

			if (Parent.JE_CustomsProfile.IsEmpty)
			{
				Parent.JE_CustomsProfileInfo.AddMessageError(MessageErrorBadgeCodeNotFound);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsProfileInfo, (NoResString)messageErrorBadgeCodeNotInList);
			}

			if (!Parent.JE_CustomsProfile.IsEmpty)
			{
				if (Parent.ZG_Gateway == GatewayList.Codes.CDS)
				{
					var notificationCollection = new MessageSendingNotificationCollection();
					var checker = new CdsGlbExternalPasswordCheckerWithNotifications(Parent, notificationCollection);
					_ = checker.PasswordExistsAndOkToSendToCds;
					var warnings = notificationCollection.WarningNotificationsAsString();
					var errors = notificationCollection.ErrorNotificationsAsString();

					if (!warnings.IsEmpty)
					{
						Parent.JE_CustomsProfileInfo.AddWarning(warnings);
					}

					if (!errors.IsEmpty)
					{
						Parent.JE_CustomsProfileInfo.AddMessageError(errors);
					}

					if (GBExtensions.IsPhase1Active && !Parent.JE_MasterUCR.IsEmpty && Parent.IsExport())
					{
						Parent.JE_CustomsProfileInfo.AddMessageError(Phase1ExportCDSInventoryError);
					}
				}
				else if (Parent.ZG_Gateway == GatewayList.Codes.Pentant)
				{
					if (Parent.DunsForBranchOrgProxy == ZString.Empty) // DUNS
					{
						Parent.JE_CustomsProfileInfo.AddMessageError(BadgeHasNoDuns);
					}
				}
			}
		}

		protected override void CheckJE_GoodsLocation()
		{
			var declaration = Parent;
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(declaration.JE_GoodsLocationInfo);

			var list = declaration.Lookups.LocationOfGoods;
			if (list != null)
			{
				ListValidation.MessageErrorIfInvalidCode(declaration.JE_GoodsLocationInfo, list);
			}

			var goodsLocation = declaration.JE_GoodsLocation;
			if (!goodsLocation.IsEmpty)
			{
				var locationQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, goodsLocation);
				var groupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;
				var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port;
				var codelist = ZZRefCusCodeListCombined.Loader.Load(declaration.Factory, groupingCode, codeType, declaration.DateOfValuation, locationQuery);
				if (codelist.Length == 1)
				{
					var attributeValue = codelist[0].Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility);
					var locationOtherInformation = declaration.JE_Calc_LocationOtherInformationType;
					if (attributeValue != locationOtherInformation)
					{
						declaration.JE_GoodsLocationInfo.AddMessageError(string.Format(LocationTypeCodeMismatch, attributeValue, locationOtherInformation));
					}
				}
			}
		}

		public const string LocationTypeCodeMismatch = "This location code requires location type code '{0}' but you have supplied '{1}'";

		protected override void CheckJE_SubLocationOfGoods()
		{
			base.CheckJE_SubLocationOfGoods();

			var declaration = Parent;
			if (declaration.IsInventoryControlledAirImport && declaration.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW)
			{
				var codelist = ZZRefCusCodeListCombined.Loader.Load(declaration.Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
				if (!codelist.Select(x => x.ZZD_Code).Contains(declaration.JE_SubLocationOfGoods))
				{
					declaration.JE_SubLocationOfGoodsInfo.AddMessageError(Res.GetString("A01D6514-95C9-4AB7-BC8D-C8FD44A4E0B6", "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character)."));
				}
			}
		}

		protected override void CheckJE_LocationQualifier()
		{
			base.CheckJE_LocationQualifier();
			if (Parent.JE_LocationQualifier == Business.CodeDescriptionPairLists.LocationQualifierList.Codes.CustomsWarehouse)
			{
				Parent.JE_LocationQualifierInfo.AddMessageError(DoNotUseCwHere);
			}
		}
		public const string DoNotUseCwHere = "Use the warehouse fields on the Entry Instruction tab to detail warehouses.  Do not select CW here.";

		protected override void ValidateSupervisingOfficeDocAddressCore()
		{
			ValidateSupervisingOfficeHasSpoffCode(Parent, Parent.SupervisingOfficeDocAddress);
		}

		public static void ValidateSupervisingOfficeHasSpoffCode(JobDeclaration declaration, JobDocAddress address)
		{
			if (address.IsValidAddress)
			{
				var customsRegNo = address.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.UnitedKingdom, address.E2_OA_Address);
				if (customsRegNo.IsEmpty)
				{
					address.E2_OA_AddressInfo.AddMessageError(InvalidSupervisingOffice_ErrorMessage);
				}
				else if (!declaration.Lookups.CustomsSupervisingOfficeList.Where(x => x.ZZD_Code == customsRegNo).Any())
				{
					address.E2_OA_AddressInfo.AddMessageError(SupervisingOfficeDoesNotExist_ErrorMessage);
				}
			}
		}

		protected override void CheckJE_EntryStyle()
		{
			base.CheckJE_EntryStyle();
			ValidateJE_EntrySubStyle();

			GetEntryStyleValidator().Validate();
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			if (RequiresTransportDetails(Parent) && Parent.JE_RN_NKTransportNationality.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();

			if (Parent.IsRoRoOnlyLocation && Parent.JE_TransportMode != GBTransportTypeList.Codes.ROR)
			{
				Parent.JE_TransportModeInfo.AddMessageError("Transport type must be \"ROR\" when Location of goods is a RoRo-Only location.");
			}

			CheckJE_TransportModeMandatory();
			ValidateJE_RN_NKTransportNationality();
		}

		protected override void CheckJE_TransportModeMandatory()
		{
			base.CheckJE_TransportModeMandatory();
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			if (Parent.JE_VesselName.IsEmpty && Parent.IsRoRoLocation)
			{
				Parent.JE_VesselNameInfo.AddMessageError("The transport ID must be completed for a RORO location.");
			}

			if ((Parent.IsExport || Parent.IsImport) && Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if ((Parent.IsExport || Parent.IsImport) && Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
			}
		}

		protected override void CheckJE_DeclarantType()
		{
			if (Parent.JE_DeclarantType == RepresentationTypeList.Codes._1Self)
			{
				var msg = "Under CDS, self representation is not declared using this field. For self representation, set the declarant and importer to be the same party (compared using EORI), and clear this field.";
				Parent.JE_DeclarantTypeInfo.AddMessageError(msg);
			}
			else if (!Parent.JE_DeclarantType.IsEmpty)
			{
				base.CheckJE_DeclarantType();
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();

			if (!Parent.JE_OA_DeclarantAddress.IsEmpty)
			{
				EoriCodeValidationHelper.CheckNoEoriCodeInPropertyInfo(Parent.DeclarantTraderId, Parent.JE_OA_DeclarantAddressInfo);
			}

			if (Parent.IsDeclarantSameAsLocalClientBasedOnEori())
			{
				var representativeDocAddress = Parent.DocAddresses.FindByDocAddressType(DocAddressType.Representative);
				if (!Parent.JE_OA_Representative.IsEmpty || (representativeDocAddress != null && !representativeDocAddress.IsEmpty && !representativeDocAddress.IsDeleted))
				{
					var msg = "When the declarant is the importer, a representative is not allowed. Remove the representative, or select a different importer or declarant.";
					Parent.JE_OA_DeclarantAddressInfo.AddMessageError(msg);
				}
			}
		}

		// FIR-DAN-PFX (other)
		protected override void CheckJE_PaymentMethod()
		{
			SetUpSharedProps();
			base.CheckJE_PaymentMethod();
			AccountNumbersValidationHelper.CheckFirstIsEmptySecondNot(firstDan, secondDan, firstDanPrefix);
			AccountNumbersValidationHelper.CheckBothAccountNoSame(firstDan, secondDan, firstDanPrefix);
			AccountNumbersValidationHelper.CheckHasAccountNumberNoPrefix(firstDan, firstDanPrefix, firstDanPrefix);
			AccountNumbersValidationHelper.CheckNoAccountNumberHasPrefix(firstDan, firstDanPrefix, firstDanPrefix);
			ValidateJE_DefermentAccountNumber();
			Parent.AddInfoValidation.ValidateZG_VATDeferNumber();
		}

		protected override void CheckJE_DefermentAccountNumber()
		{
			SetUpSharedProps();
			base.CheckJE_DefermentAccountNumber();
			AccountNumbersValidationHelper.CheckFirstIsEmptySecondNot(firstDan, secondDan, firstDan);
			AccountNumbersValidationHelper.CheckBothAccountNoSame(firstDan, secondDan, firstDan);
			AccountNumbersValidationHelper.CheckHasAccountNumberNoPrefix(firstDan, firstDanPrefix, firstDan);
			AccountNumbersValidationHelper.CheckNoAccountNumberHasPrefix(firstDan, firstDanPrefix, firstDan);
			ValidateJE_PaymentMethod();
		}

		protected override void CheckJE_ShipmentIncoTermPlace()
		{
			base.CheckJE_ShipmentIncoTermPlace();
			CDSIncoTermPlaceValidator.Validate(Parent.Factory, Parent.JE_ShipmentIncoTermPlace, Parent.JE_ShipmentIncoTermPlaceInfo);
		}

		EntryStyleValidator GetEntryStyleValidator()
		{
			return new EntryStyleValidator(this);
		}

		bool RequiresTransportDetails(JobDeclaration declaration)
		{
			var transportMode = declaration.TransportMode;
			if (transportMode == TransportTypeList.Codes.Rail
				|| transportMode == TransportTypeList.Codes.Mail
				|| transportMode == TransportTypeList.Codes.FixedTransportInstallations)
			{
				return false;
			}

			if (declaration.CusEntryInstruction != null)
			{
				var style = declaration.CusEntryInstruction.CEI_Style;
				return style == ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse
					|| style == ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission
					|| style == ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing
					|| style == ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			}

			return false;
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			var bRRS = Parent.AdditionalInfos.Find(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.RRS01).Any();
			if (Parent.JE_OH_ShippingLine.IsEmpty && !bRRS && Parent.JE_IsGvmsPort)
			{
				Parent.JE_OH_ShippingLineInfo.AddMessageError(ShippingLineErrorMsg);
			}
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			if (Parent.JE_GoodsOrigin.IsEmpty)
			{
				Parent.JE_GoodsOriginInfo.AddMessageError("Port of origin is required for transfer to HMRC");
			}
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			if (Parent.JE_GoodsDestination.IsEmpty)
			{
				Parent.JE_GoodsDestinationInfo.AddMessageError("Port of final destination is required for transfer to HMRC");
			}
		}

		public override void ValidateImporterDocumentaryAddress(JobDocAddressValidation validation)
		{
			base.ValidateImporterDocumentaryAddress(validation);
			if (Parent.ImporterDocumentaryAddress.Organisation != null)
			{
				AddressValidationHelper.CheckConsigneePostcode(Parent);
				if (Parent.IsImport)
				{
					EoriCodeValidationHelper.CheckNoEoriCodeInPropertyInfo(Parent.ImporterDocumentaryAddress, Parent.ImporterDocumentaryAddress.E2_OA_AddressInfo);
				}
			}
			if (Parent.ImporterDocumentaryAddress.Organisation == null && !Parent.Invoices.AtLeastOneInvoiceHasABuyer)
			{
				Parent.ImporterDocumentaryAddress.E2_OA_AddressInfo.AddWarning("You have not entered an importer. Please ensure that you select item-level importers against the invoice headers.");
			}
		}

		public override void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
		{
			base.ValidateSupplierDocumentaryAddress(validation);
			if (Parent.SupplierDocumentaryAddress.Organisation != null && Parent.IsExport)
			{
				EoriCodeValidationHelper.CheckNoEoriCodeInPropertyInfo(Parent.SupplierDocumentaryAddress, Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo);
			}

			if (Parent.SupplierDocumentaryAddress.Organisation == null && !Parent.Invoices.AtLeastOneInvoiceHasASupplier)
			{
				Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo.AddWarning("You have not entered a supplier. Please ensure that you select item-level suppliers against the invoice headers.");
			}

			// Check postcode:
			if (Parent.SupplierDocumentaryAddress.Organisation != null && Parent.IsImport)
			{
				AddressValidationHelper.CheckAddressAndNameEmpty(Parent.SupplierDocumentaryAddress, Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo);
			}
		}

		const string ShippingLineErrorMsg = "This job moves through a GVMS-enabled port (as identified from the 5/23 location of goods code), and so requires an RRS01 statement. This can be set manually on the declaration's or instruction's Additional Information grid, or can be automated by completing this carrier field.";

		protected override string GetUnnecessaryContainerWarningCore(int counter)
		{
			return Enterprise.Customs.GB.CDS.Res.GetString("4477461-c4e5-409f-96c3-abb0861d46a0", "You have entered {0} container(s) for a mode that doesn't require them.", counter);
		}

		ZPropertyInfo firstDan;
		ZPropertyInfo firstDanPrefix;
		ZPropertyInfo secondDan;

		void SetUpSharedProps()
		{
			firstDan = Parent.JE_DefermentAccountNumberInfo;
			firstDanPrefix = Parent.JE_PaymentMethodInfo;
			secondDan = Parent.ZG_VATDeferNumberInfo;
		}
	}
}
