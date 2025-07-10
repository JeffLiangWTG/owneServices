using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoFRJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public void ValidateChargePaymentOrDestinationID()
		{
			ValidateCalculatedProperty(Parent.ChargePaymentOrDestinationIDInfo);
		}

		protected virtual void CheckChargePaymentOrDestinationID()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.ChargePaymentOrDestinationIDInfo);
			if (parent.ChargePaymentOrDestinationID.IsEmpty)
			{
				if (!UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(Parent.Factory, Parent).IsEmpty())
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ChargePaymentOrDestinationIDInfo);
				}
			}
		}

		public override void ValidateWarehouseDocAddressForeignKey()
		{
		}

		protected override bool IsIncotermRequired => true;

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInlandInfo);
		}

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();
			var declaration = Parent;

			if (!declaration.JE_OA_Representative.IsEmpty)
			{
				CheckRuleNat_020(declaration.Representative, declaration.JE_OA_RepresentativeInfo);
			}
		}

		protected override void CheckJE_OH_ControllingCustomer()
		{
			base.CheckJE_OH_ControllingCustomer();

			if (Parent.ControllingCustomer != null)
			{
				CheckRuleNat_020(Parent.ControllingCustomer.MainAddress, Parent.JE_OH_ControllingCustomerInfo);
			}
		}

		void CheckRuleNat_020(OrgAddress address, ZPropertyInfo propertyInfo)
		{
			if (ValidationDecider is IDeclarationValidationDecider { IsRuleNAT_020Active: true })
			{
				string ruleCode = "NAT_020";
				DeltaIEDeclarationValidationHelper.CheckEORI(propertyInfo, address, ruleCode);
			}
		}

		void CheckRuleNat_021(ZPropertyInfo propertyInfo)
		{
			var declaration = Parent;
			if (ValidationDecider is IDeclarationValidationDecider { IsRuleNAT_021Active: true }
				&& !declaration.JE_CustomsOffice.StartsWith(Core.Constants.CountryCodes.France)
				&& declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48))
			{
				propertyInfo.AddMessageError(Res.GetString("BA3F79A2-44EE-4AC5-B198-6DED339C6FBE", "You have entered a concession F48, the declaration office must be located in metropolitan France."));
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsProfileInfo);
			CheckAuthorizationOwnerEORI();
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			CheckRuleNat_021(Parent.JE_CustomsOfficeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo, ResString.GetMultilingualString("DD12258B-49A7-4293-84FC-D50717328190", "Entered office code is not a valid office."));
		}

		protected void CheckAuthorizationOwnerEORI()
		{
			var deltaGAccountOwner = Parent.DeltaAccountOrgHeader;
			if (deltaGAccountOwner == null)
			{
				Parent.JE_CustomsProfileInfo.AddMessageError(Res.GetString("7BAABD3E-7F9F-435D-938E-3DD700C14BB4", "No authorization owner could be inferred from declaration actual data. Please fix this or the declaration will be rejected by Customs."));
			}
			else if (Parent.EoriCode.IsEmpty)
			{
				Parent.JE_CustomsProfileInfo.AddMessageError(Res.GetString("7D36B6E6-C2FE-478F-AC39-12E946F71612", "{0} EORI code not found", deltaGAccountOwner.OH_FullName));
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			var parent = Parent;
			if (parent.Lookups.GoodsLocations.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.JE_LocationOfGoodsInfo);
				MandatoryValidation.WarnIfNotEntered(parent.JE_LocationOfGoodsInfo, Res.GetString("606F5633-1DCB-46CB-9A92-E795DF71F23A", "Authorization when there is at least one available for this Profile"));
			}
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			if (Parent.Lookups.SubGoodsLocations.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_SubLocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_PaymentMethodLogicForEU()
		{
		}

		public void ValidateJE_DeltaMode()
		{
			ValidateCalculatedProperty(Parent.JE_DeltaModeInfo);
			ValidateCalculatedProperty(Parent.JE_CustomsProfileInfo);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKOriginInfo);
		}

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			if (Parent.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Export)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_PaymentMethodInfo);

			if (Parent.Validation.ValidationDecider is IDeclarationValidationDecider { IsRuleNAT_130BisActive: true }
				&& Parent.IsDeclarationStandard
				&& Parent.JE_PaymentMethod != MethodOfPaymentList.Codes.A)
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("305BBD7E-B57C-4BDB-9BA6-0994747BF53E", "[NAT_130_Bis] Cash is the only suitable Method of Payment when the declaration has E0001 Additional Information."));
			}
		}

		protected override void CheckJE_DefermentAccountNumber()
		{
			base.CheckJE_DefermentAccountNumber();
			var parent = Parent;

			if (parent.Validation.ValidationDecider is IDeclarationValidationDecider { IsRuleNAT_130BisActive: true }
				&& parent.IsDeclarationStandard)
			{
				if (!parent.JE_DefermentAccountNumber.IsEmpty)
				{
					parent.JE_DefermentAccountNumberInfo.AddMessageError(Res.GetString("7E53C34F-6CDF-4114-A6E4-91FC8C2F7D33", "[NAT_130_Bis] The deferred payment number should be empty when the declaration has E0001 Additional Information."));
				}
			}
			else if (parent.JE_PaymentMethod == MethodOfPaymentList.Codes.A)
			{
				if (!parent.JE_DefermentAccountNumber.IsEmpty)
				{
					parent.JE_DefermentAccountNumberInfo.AddMessageError(Res.GetString("6259EFB3-D670-4578-9C1E-A9B8848193FB", "The deferred payment number should be empty when the Method of Payment is Cash."));
				}
			}
			else
			{
				CheckJE_DefermentAccountNumberLength();
			}
		}

		protected virtual void CheckJE_DefermentAccountNumberLength()
		{
			if (Parent.JE_PaymentMethod == MethodOfPaymentList.Codes.R)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DefermentAccountNumberInfo);
				var accountNo = Parent.JE_DefermentAccountNumber;
				if (accountNo.Length != 4 || !accountNo.IsLettersOnlyOrEmpty)
				{
					Parent.JE_DefermentAccountNumberInfo.AddWarning(Res.GetString("7DACBBA5-E6F0-4126-9976-FA19BA828A65", "An account number must be 4 letters long"));
				}
			}
		}

		protected override void CheckJE_LandedPieces()
		{
			base.CheckJE_LandedPieces();
			var landedPieces = Parent.JE_LandedPieces;
			if (Parent.IsImport && Parent.IsAir && ZInt.Zero < landedPieces && landedPieces < Parent.JE_TotalNoOfPacks)
			{
				Parent.JE_LandedPiecesInfo.AddMessageError(Res.GetString("43EB2CF1-7BF7-4FC3-A562-5DC1720D9E39", "Not all pieces are received yet."));
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			var parent = Parent;
			if (parent.JE_DeclarantType == EU.Business.RepresentationTypeList.Codes._3Indirect)
			{
				if (parent.DeclarantAddress?.Header == null)
				{
					parent.JE_OA_DeclarantAddressInfo.AddMessageError(ErrorCollectorHelper.DeclarantIsRequired);
				}
			}

			var declarant = parent.DeclarantAddress?.Header;
			var representative = parent.Representative?.Header;
			if (declarant.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.BrokerageRegistration).IsEmpty && representative.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.BrokerageRegistration).IsEmpty)
			{
				parent.JE_OA_DeclarantAddressInfo.AddMessageError(ErrorCollectorHelper.CBRNotConfigured);
			}

			var customsProfile = parent.JE_CustomsProfile;
			if (!customsProfile.IsEmpty)
			{
				if (IsPartnerIdNotConfigured(declarant, customsProfile) && IsPartnerIdNotConfigured(parent.IsImport ? parent.Importer : parent.IsExport ? parent.Supplier : null, customsProfile))
				{
					parent.JE_OA_DeclarantAddressInfo.AddMessageError(MessageBuilderHelper.PartnerIdNotConfigured);
				}
			}

			CheckMandatoryPhoneAndEmail(parent.JE_OA_DeclarantAddressInfo);
		}

		bool IsPartnerIdNotConfigured(OrgHeader header, ZString customsProfile)
		{
			var result = false;
			var parent = Parent;
			var accountCollection = header?.DeltaAgreementNumberCollection.Cast<OrgCusAccount>();

			if (accountCollection == null || !accountCollection.Any(x => x.CZ_Account == customsProfile && (!IsAccountDeltaModeMandatory || x.CZ_Type == parent.JE_DeltaMode) && !x.CZ_RepresentativeID.IsEmpty))
			{
				result = true;
			}

			return result;
		}

		protected virtual bool IsAccountDeltaModeMandatory => true;

		protected override void CheckPackagesActualPackageCount()
		{
			base.CheckPackagesActualPackageCount();

			var parent = Parent;
			if (parent.ActiveEntryHeaders.Count > 0)
			{
				foreach (Package package in parent.Packages)
				{
					int usedPackQuantity = 0;

					foreach (CusEntryHeader entryHeader in parent.ActiveEntryHeaders)
					{
						foreach (var entryLine in entryHeader.MergedLines)
						{
							usedPackQuantity += entryLine.PackagingDetails.Where(x => x.CHC_CW == package.PK).Sum(y => y.CHC_NumberOfPacks);
						}
					}

					if (usedPackQuantity < package.CW_PackQty)
					{
						var message = string.Format(CultureInfo.CurrentCulture, NotAllPacksAllocated, package.CW_PackType);
						if (!parent.PackagesActualPackageCountInfo.HasMessageError(message))
						{
							parent.PackagesActualPackageCountInfo.AddMessageError(message);
						}
					}
				}
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			base.CheckJE_DateOfFirstArrival();
			if (Parent.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Import)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfFirstArrivalInfo);
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (Parent.JE_MessageType == Customs.Business.JobMessageTypeList.Codes.Export)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo);
			}
		}

		public static string NotAllPacksAllocated => Res.GetString("489E38FC-1E4C-4B42-8739-7616DFC199C9", "Not all packs of type {0} are allocated to entry lines. Consider adding a new invoice line and set it the remaining pack quantity.");

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			var declaration = Parent;

			CheckRuleNat_145bis(declaration.JE_ApplicationCodeInfo);

			var info = declaration.JE_ApplicationCodeInfo;
			if (declaration.IsInDatabase)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
			}
			else
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);
			}
			if ((ZString)info.OriginalValue != declaration.JE_ApplicationCode && declaration.DeclarationMessagesHaveBeenSent(reloadMessages: true))
			{
				declaration.JE_ApplicationCodeInfo.AddError(Res.GetString("884e8891-0c50-4096-b9fe-9cda51e55cd7", "You may not change the message because messages have been sent."));
			}
		}

		void CheckRuleNat_145bis(ZPropertyInfo propertyInfo)
		{
			var declaration = Parent;
			var hasMandatoryInfo = DeltaIEDeclarationValidationHelper.HasMandatoryAdditionalInfos(declaration.AdditionalInfos);
			if (ValidationDecider is IDeclarationValidationDecider { IsRuleNat_145BisActive: true } && !hasMandatoryInfo)
			{
				propertyInfo.AddMessageError(Res.GetString("05fcbade-f008-40dc-a503-b95f5c994a7a", "[NAT_145BIS] A0010 Additional Information is mandatory for Delta Import declarations."));
			}
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();
			var parent = Parent;
			if (parent.IsUCC6AndIsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JE_GS_NKCusAgentInfo);
			}

			CheckMandatoryPhoneAndEmail(parent.JE_GS_NKCusAgentInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJE_DeltaMode();
			ValidateChargePaymentOrDestinationID();
		}

		void CheckMandatoryPhoneAndEmail(ZPropertyInfo info)
		{
			var parent = Parent;

			if (parent.IsUCC6AndIsImport && !parent.JE_GS_NKCusAgent.IsEmpty && parent.CusAgent != null)
			{
				if ((parent.CusAgent.GS_WorkPhone.IsEmpty || parent.CusAgent.EmailAddresses.All(e => e.GSE_EmailAddress.IsEmpty)) && (parent.Branch.GB_Phone_Wrapper.FormattedForBinding.IsEmpty || parent.Branch.GB_Email.IsEmpty))
				{
					info.AddMessageError(Res.GetString("C128AC15-6221-4744-8C60-8CE5F1EC3AA9", "Phone and email are mandatory in customs message, please enter both the data for Broker staff or have it configured in current Branch information."));
				}
			}
		}

		protected override void CheckJE_UCR()
		{
			base.CheckJE_UCR();
			CheckJE_UCRLength();
		}

		void CheckJE_UCRLength()
		{
			var parent = Parent;
			List<SupportingDocument> foundDocuments = new List<SupportingDocument>();

			foundDocuments.AddRange(parent.SupportingDocuments.Cast<SupportingDocument>().Where(y => y.IsODS));

			foreach (var invoice in parent.Invoices.Cast<JobComInvoiceHeader>())
			{
				foundDocuments.AddRange(invoice.SupportingDocuments.Cast<SupportingDocument>().Where(y => y.IsODS));

				foreach (var invoiceLine in invoice.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					foundDocuments.AddRange(invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Where(y => y.IsODS));
				}
			}

			if (foundDocuments.Any() && parent.JE_UCR.Length > MaxUCRLengthWithODSDocs)
			{
				string foundCSICodesString = string.Join(", ", foundDocuments.Select(y => y.CSI_Code).Distinct());
				string errorMsg =
					string.Format(
						Res.GetString("FE7A05AC-C256-4790-ACE1-8F16015085D2", "The presence of documents of type {0} limits the local reference to {1} chars max."), foundCSICodesString, MaxUCRLengthWithODSDocs);

				parent.JE_UCRInfo.AddMessageError(errorMsg);
			}
		}

		public const int MaxUCRLengthWithODSDocs = 22;

		protected override void CheckJE_CustomsGuaranteeNumber()
		{
			var parent = Parent;
			if (parent.CustomsEntryHeaders.Cast<CusEntryHeader>().Any(ceh => ceh.IsGuaranteeConsumed))
			{
				if (parent.JE_CustomsGuaranteeNumber.IsEmpty || parent.CustomsGuarantee == null || parent.CustomsGuarantee.GetApplicationSpecificReference(GuaranteeTypeList.Codes.COD).IsEmpty)
				{
					var message = Res.GetString("D0E87498-C47D-48E9-B7AB-CD80D9AC2970", "No guarantee was set, although one is required. Check that the {0} organization or the Declarant owns a valid guarantee of type COD", parent.IsImport ? Res.GetString("72fa58b9-cfa6-4839-9d2a-ae36869479ab", "Importer") : Res.GetString("5e36f4eb-63bf-47d8-bf31-7636217139e5", "Supplier"));
					parent.JE_CustomsGuaranteeNumberInfo.AddMessageError(message + AdditionalSpecificErrorMessageForNeededACODGuarantee + ".");
				}
			}
		}

		protected virtual string AdditionalSpecificErrorMessageForNeededACODGuarantee => ZString.Empty;

		protected override void CheckJE_AirRouteType()
		{
			var parent = Parent;
			if (parent.IsImport && parent.IsAir)
			{
				base.CheckJE_AirRouteType();
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_AirRouteTypeInfo, parent.Lookups.AirRouteTypeList);
			}
		}

		protected override void CheckJE_ExportExitType()
		{
			base.CheckJE_ExportExitType();

			var parent = Parent;
			if (!parent.IsOfficeOfLodgementDifferentFromOfficeOfExit)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.JE_ExportExitTypeInfo, parent.Lookups.ExportExitTypeList, ListValidation.InvalidCodeMessageError);

				if (parent.JE_ExportExitType.IsEmpty && parent.IsOfficeOfExitCodeTheSameAsOfficeOfDeclaration)
				{
					parent.JE_ExportExitTypeInfo.AddMessageError(ExportExitTypeRequiredError);
				}

				if (!parent.JE_ExportExitType.IsEmpty && !parent.HasValidPreviousDocumentForExportExitType)
				{
					var msgError = parent.JE_ExportExitType == ExportExitTypeList.Codes.TRA
						? string.Format(CultureInfo.InvariantCulture, ExportExitTypePreviousDocumentRequiredTRAError.ToString(), parent.JE_ExportExitType)
						: string.Format(CultureInfo.InvariantCulture, ExportExitTypePreviousDocumentRequiredError.ToString(), parent.JE_ExportExitType, string.Join(",", parent.GetAllowedPreviousDocsForExportExitType()));
					parent.JE_ExportExitTypeInfo.AddMessageError(msgError);
				}

				if (!parent.JE_ExportExitType.IsEmpty && !parent.HasValidExportExitTypeCTStatusCombination)
				{
					parent.JE_ExportExitTypeInfo.AddMessageError(ExportExitTypeCTStatusError.ToString());
				}
			}
		}

		protected override void CheckJE_ExportExitTypeReason()
		{
			base.CheckJE_ExportExitTypeReason();

			var parent = Parent;
			if (!parent.IsOfficeOfLodgementDifferentFromOfficeOfExit)
			{
				if (parent.JE_ExportExitType == ExportExitTypeList.Codes.OTH && parent.JE_ExportExitTypeReason.IsEmpty)
				{
					parent.JE_ExportExitTypeReasonInfo.AddMessageError(ExportExitTypeReasonRequiredError);
				}
			}
		}

		protected override void CheckJE_RegionOrTerritoryOfDestination()
		{
			base.CheckJE_RegionOrTerritoryOfDestination();
			var parent = Parent;
			if (parent.Lookups.RegionOrTerritoryOfDestinationList.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.JE_RegionOrTerritoryOfDestinationInfo);
			}
		}

		public static string ExportExitTypeRequiredError => Res.GetString("98DACC4D-E46D-4A84-9BAE-BC47F9B6C4B2", "Export Exit Type is required if the Office of Exit is the same as the Office of Export.");

		public static string ExportExitTypeReasonRequiredError => Res.GetString("D53F7C50-BFFA-4110-9FDA-005CFF9AB99A", "Export Exit Type Reason is required if the Export Exit Type is OTH.");

		static IMultilingualString ExportExitTypePreviousDocumentRequiredError => ResString.GetMultilingualString("770F35B7-21BD-4F24-BBB9-EEEBD9C4878A", "Export Exit Type {0} requires a previous document but none was found. Expected document type: {1}");
		static IMultilingualString ExportExitTypePreviousDocumentRequiredTRAError => ResString.GetMultilingualString("97CC3140-F04F-4CAB-88B9-2AC7BF44CB67", "Export Exit Type {0} requires a previous document but none was found.");

		public static IMultilingualString ExportExitTypeCTStatusError => ResString.GetMultilingualString("73D08329-463F-4478-BDA6-E6F1D1EC816C", "Export Exit Type TRA should only be used when CT Status is T1, T2 or T-");

		internal new EU.Business.Declaration.IDeclarationValidationDecider ValidationDecider => base.ValidationDecider;
	}
}
