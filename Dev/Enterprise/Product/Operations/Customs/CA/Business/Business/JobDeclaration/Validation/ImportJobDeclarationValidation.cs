using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CA.Business.CusBondDetailCollection;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return Declaration.DeclarationValidator; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			var declaration = Declaration;
			if (declaration.IsIID)
			{
				declaration.Manufacturer.Validation.ValidateAll();
				declaration.AdditionalDeliveryAddress.Validation.ValidateAll();
				declaration.AdditionalConsignee.Validation.ValidateAll();
				declaration.OGDProcessInspectionLPCO.Validation.ValidateAll();
				declaration.CFIAPaymentParty.Validation.ValidateAll();
			}
		}

		#region CheckJE_OH_Importer (common)

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			var declaration = Declaration;
			if (!declaration.IsLVS || declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.ConsolidationByImporter)
			{
				MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_OH_ImporterInfo);
				var importer = declaration.Importer;
				if (!(declaration.JE_MessageType == JobMessageTypeList.Codes.Import && declaration.ImporterOfRecordAddress.HasRealAddress))
				{
					CheckHasImporterBusinessNumberForImportExport();
					ValidateImporterDocumentaryAddress(declaration.JE_OH_ImporterInfo);
					if (importer != null)
					{
						ValidateEffectiveImporter(importer, declaration.JE_OH_ImporterInfo);
					}
				}
				else if (declaration.IsCADEnabled)
				{
					CheckHasImporterBusinessNumberForImportExport();
				}

				if (declaration.JE_MessageType == JobMessageTypeList.Codes.Import && !declaration.ImporterOfRecordAddress.HasRealOrganisation && importer != null)
				{
					AuthorityToActValidationHelper.Validate(declaration.JE_OH_ImporterInfo, declaration, () => importer, ImportExportCodeList.Codes.Import, declaration.JE_CustomsOffice);
				}
			}
			var importerAddInfo = declaration.ImporterAddInfo;
			if (importerAddInfo != null)
			{
				if (importerAddInfo.IsAscPasswordNotSpecified)
				{
					declaration.JE_OH_ImporterInfo.AddMessageError(OrgImpAddInfo.AscPasswordNotSpecifiedErrorText);
				}
			}
			OrganisationValidation.ValidateNamesAndAddressesSpecial(declaration.JE_OH_ImporterInfo, Parent.Importer);

			if (declaration.JE_MessageType == JobMessageTypeList.Codes.Import && declaration is IBondDetailsDefault iBondDetailsDefault)
			{
				var bondDetails = iBondDetailsDefault.ImportOrg?.BondDetails;
				if (bondDetails != null)
				{
					var effectiveDate = iBondDetailsDefault.EffectiveDate;
					if (bondDetails.GetBondDetailsStatus(BondTypeList.Codes.NotOnPortal, effectiveDate) == BondDetailsStatus.BondExist)
					{
						ImportAddInfoJobDeclarationValidation.ValidateBondType(declaration, declaration.JE_OH_ImporterInfo, BondTypeList.Codes.NotOnPortal);
					}

					if (bondDetails.GetBondDetailsStatus(BondTypeList.Codes.OnPortal, effectiveDate) == BondDetailsStatus.BondExist)
					{
						ImportAddInfoJobDeclarationValidation.ValidateBondType(declaration, declaration.JE_OH_ImporterInfo, BondTypeList.Codes.OnPortal);
					}
				}
			}
		}

		void CheckHasImporterBusinessNumberForImportExport()
		{
			if (!Declaration.JE_OH_Importer.IsEmpty && Declaration.Importer != null)
			{
				CheckHasImporterBusinessNumberForImportExport(Declaration.Importer, Declaration.JE_OH_ImporterInfo, Declaration.IsCADEnabled, Declaration.IsExistingEffectiveCasualImport, Res.GetString("f423e10a-e264-4bf1-ab55-840c84037781", "importer"), Declaration.ImporterOfRecord);
			}
		}

		#region CheckJE_GS_NKCusAgent

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();
			if (Parent.IsIID)
			{
				var broker = Parent.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, Parent.JE_GS_NKCusAgent));
				if (broker != null)
				{
					if (broker.GS_WorkPhone.IsEmpty)
					{
						Parent.JE_GS_NKCusAgentInfo.AddMessageError(WorkPhoneIsRequired);
					}

					var messageErrorForFullName = ContactNameValidataionHelper.ValidationContactName(broker.GS_FullName, true);
					if (!messageErrorForFullName.IsEmpty)
					{
						Parent.JE_GS_NKCusAgentInfo.AddMessageError(messageErrorForFullName);
					}
				}
			}
		}
		internal static string WorkPhoneIsRequired
		{
			get { return Res.GetString("53EB8417-02AB-4E41-B5C8-0C3BBB1BE1A7", "Work Phone is required for the Broker."); }
		}

		#endregion

		internal static void CheckHasImporterBusinessNumberForImportExport(OrgHeader orgHeader, ZPropertyInfo notificationInfo, bool isCADEnabled, bool isCasual, string orgCaption, OrgHeader orgHeader1 = null)
		{
			var error = ZString.Empty;

			var businessNumberIsRequiredMessage = string.Empty;
			var codeType1 = string.Empty;
			var codeType2 = string.Empty;
			Func<ZString, ZString> getFirstCodeError = null;
			Func<ZString, ZString> getSecondCodeError = null;

			if (isCADEnabled)
			{
				codeType2 = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
				getSecondCodeError = CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError;
				if (isCasual)
				{
					codeType1 = OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial;
					businessNumberIsRequiredMessage = Res.GetString("829D8616-3292-4C2B-B402-953B34B6F0D8", "This {0} does not have a Business Number Importer Non-Commercial ({1}) or business number for import/export ({2}) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.",
						orgCaption, codeType1, codeType2);
					getFirstCodeError = CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError;
				}
				else
				{
					codeType1 = OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial;
					businessNumberIsRequiredMessage = Res.GetString("43E29CED-46FD-412B-9C03-CC5C040BDFAB", "This {0} does not have a Business Number Importer Commercial ({1}) or business number for import/export ({2}) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.",
						orgCaption, codeType1, codeType2);
					getFirstCodeError = CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError;
				}
			}
			else
			{
				codeType1 = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
				codeType2 = OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial;
				businessNumberIsRequiredMessage = Res.GetString("94303449-cf0b-4757-b04e-fbbf0a9202b0", "This {0} does not have a business number for import/export ({1}) or Business Number Importer Commercial ({2}) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.",
					orgCaption, codeType1, codeType2);
				getFirstCodeError = CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError;
				getSecondCodeError = CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError;
			}

			if (orgHeader1 != null)
			{
				error = GetImporterBusinessNumberError(orgHeader1, businessNumberIsRequiredMessage, codeType1, codeType2, getFirstCodeError, getSecondCodeError);
			}

			if (!isCADEnabled || orgHeader1 == null || !error.IsEmpty)
			{
				error = GetImporterBusinessNumberError(orgHeader, businessNumberIsRequiredMessage, codeType1, codeType2, getFirstCodeError, getSecondCodeError);
			}

			if (!error.IsEmpty)
			{
				notificationInfo.AddMessageError(error);
			}
		}

		static ZString GetImporterBusinessNumberError(OrgHeader orgHeader, string businessNumberIsRequiredMessage, string firstCodeType, string secondCodeType, Func<ZString, ZString> getFirstCodeError, Func<ZString, ZString> getSecondCodeError)
		{
			var error = ZString.Empty;
			var registrationNumber = orgHeader.CustomsCodes.GetCustomsRegNoMatching(firstCodeType, secondCodeType);
			if (registrationNumber.IsEmpty)
			{
				error = businessNumberIsRequiredMessage;
			}
			else
			{
				var businessNumber = orgHeader.CustomsCodes.GetCustomsRegNo(firstCodeType);
				error = businessNumber.IsEmpty ? getSecondCodeError(registrationNumber) : getFirstCodeError(registrationNumber);
			}
			return error;
		}

		internal void ValidateImporterDocumentaryAddress(ZPropertyInfo notificationInfo)
		{
			var docAddress = Parent.ImporterDocumentaryAddress;
			var addressCaption = Res.GetString("045c7157-894d-4867-9d4d-5a52d753dd4c", "Importer Documentary");
			if (!Declaration.IsLVS)
			{
				CAAddressValidator.ValidateAddressInCanada(Declaration, docAddress, notificationInfo, addressCaption);
			}
		}

		internal void ValidateEffectiveImporter(OrgHeader importer, ZPropertyInfo notificationInfo)
		{
			if (Declaration.DoesRequireImporterHasPGAContact)
			{
				OrganisationValidation.ValidateCAPAllocatedContact(importer, notificationInfo, GetPGAsRequiringContact(), true, true);
			}
			else if (Declaration.IsIID)
			{
				OrganisationValidation.ValidateCAPContactOrAddressPhone(notificationInfo, importer.MainAddress);
			}

			if (Declaration.DoesRequireImporterHasCFIAAccountNumber)
			{
				var paymentMethod = Declaration.EffectiveCFIAFeePaymentMethod;
				if (paymentMethod == CFIAPaymentMethods.Codes.Other)
				{
					notificationInfo.AddMessageError(OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);
				}
				else if (paymentMethod == CFIAPaymentMethods.Codes.Importer && !DoesOrganizationContainCFIAAccountNumber(importer))
				{
					notificationInfo.AddMessageError(OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
				}
				else if (Declaration.IsCFIAAccountNumberRequired)
				{
					notificationInfo.AddWarning(OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
				}
			}
		}

		ZString GetPGAsRequiringContact()
		{
			ZStringBuilder result = new ZStringBuilder();
			if (Declaration.HasInvoiceLinesWithHCPGA)
			{
				result.Append(PGACodes.Descriptions.HC);
			}
			if (Declaration.HasInvoiceLinesWithCFIAPGA)
			{
				result.Append(PGACodes.Descriptions.CFIA);
			}
			if (Declaration.HasInvoiceLinesWithTCPGA)
			{
				result.Append(PGACodes.Descriptions.TC);
			}
			if (Declaration.HasInvoiceLinesWithPHACPGA)
			{
				result.Append(PGACodes.Descriptions.PHAC);
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		ZBool DoesOrganizationContainCFIAAccountNumber(OrgHeader orgHeader)
		{
			var cfiCode = orgHeader == null ? null : orgHeader.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.CFIAAccountNumber, Core.Constants.CountryCodes.Canada);
			return cfiCode != null && cfiCode.Length > 0;
		}

		#endregion

		#region CheckJE_TotalNoOfPacks (ACROSS)

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();
			if (!Parent.JE_TotalNoOfPacksInfo.HasNotifications())
			{
				DeclarationValidator.MessageErrorIfNotEntered(Parent.JE_TotalNoOfPacksInfo, Res.GetString("a11e6102-9285-4e5b-b5ed-c5dcafd93976", "Total Number of Packages"), ValidateForMessageType.ACROSS);
			}
		}

		#endregion

		#region CheckJE_OH_ShippingLine (ACROSS)

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
			{
				var cnnQuery = new ZQuery(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				var numbers = Parent.AdditionalReferenceNumbers.Find(cnnQuery);
				if (numbers.Length > 0)
				{
					var number = (CusEntryNumber)numbers[0];
					if (number.CE_EntryNum.StartsWith("2ITN"))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ShippingLineInfo, Res.GetString("1986c7e4-da81-4f9f-85c7-d77fb826c09c", "Carrier"));
					}
				}
			}
		}

		#endregion

		#region CheckJE_MessageSubType (B3)

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			var declaration = Declaration;
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
			{
				DeclarationValidator.MessageErrorIfNotEntered(declaration.JE_MessageSubTypeInfo, Res.GetString("7ba956ef-a1f4-4c63-ba9d-309f9b4affe5", "Entry Type"), ValidateForMessageType.B3CUSDEC);
				if (declaration.IsPaperOnlyEntry && !declaration.IsIM2)
				{
					Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("f383d24b-f6ed-4cfc-a0cb-30ae2d0c1dea", "Paper only entry type; electronic entry not valid."));
				}

				if (declaration.NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description).Length == 0)
				{
					if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.CashD ||
						declaration.JE_MessageSubType == B3EntryTypeList.Codes.ConfirmingSight ||
						declaration.JE_MessageSubType == B3EntryTypeList.Codes.Voluntary ||
						declaration.JE_MessageSubType == B3EntryTypeList.Codes.Supplementary)
					{
						declaration.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("0dd270d0-8e3c-4ee9-bc82-1181b76c3ede", "Please enter a message to print on the CAD (Notes)."));
					}
				}
				if (declaration.IsLVSTotalConsolidation)
				{
					CheckHasImporterBusinessNumberForLowValueShipments();
				}
			}

			CheckWHSTransactionExists(Parent.JE_MessageSubTypeInfo, () =>
			{
				var oldValue = Declaration.IsCADEnabled
					? CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.ConvertToLongCode((ZString)declaration.JE_MessageSubTypeInfo.OriginalValue))
					: B3EntryTypeList.GetWarehouseEntryType((ZString)declaration.JE_MessageSubTypeInfo.OriginalValue);
				var currentValue = Declaration.IsCADEnabled
					? CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.ConvertToLongCode((ZString)declaration.JE_MessageSubTypeInfo.Value))
					: B3EntryTypeList.GetWarehouseEntryType((ZString)declaration.JE_MessageSubTypeInfo.Value);
				return oldValue != currentValue;
			});
		}

		void CheckHasImporterBusinessNumberForLowValueShipments()
		{
			if (LowValueShipmentsMessageWrapper.GetBrokerBusinessNumberForLVS(Parent).IsEmpty)
			{
				var lvsBusinessNumberIsRequiredErrorMessage = Res.GetString("E3A8BD12-5459-4BF4-9542-8D9366F11AE9", "The Proxy Organization of the branch of the Customs Broker specified on this job, or the current login branch, must have a Business Number for Low Value Shipments ({0}) defined. Please go to Config > Registration Numbers/Codes of the Organization specified on the appropriate branch.",
					OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments);
				Parent.JE_MessageSubTypeInfo.AddMessageError(lvsBusinessNumberIsRequiredErrorMessage);
			}
		}

		#endregion

		#region CheckJE_EntryAuthorisationDate (common)

		protected override void CheckJE_EntryAuthorisationDate()
		{
			base.CheckJE_EntryAuthorisationDate();
			if (Parent.JE_MessageSubType == (Parent.IsCADEnabled ? CADEntryTypeList.Codes.ExWarehouse201 : B3EntryTypeList.Codes.ExWarehouse20) && DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_EntryAuthorisationDateInfo);
			}

			if (Parent.IsLVS
				&& !CurrentMonthOrPreviousMonth(Parent.JE_EntryAuthorisationDate)
				&& !Parent.JE_EntryAuthorisationDateInfo.HasNotifications())
			{
				var declaration = Parent.IsLVX ? Parent.LVXInvoiceHeader.FirstAdditionalDeclaration : Parent;
				if (declaration == null || !MessageStatusList.IsMessageAccepted(declaration.JE_MessageStatus) && !declaration.CA_LVSCloseDate.IsValid)
				{
					Parent.JE_EntryAuthorisationDateInfo.AddMessageError(Res.GetString("4647030e-5aa9-4322-859a-855b9de95fa7", "Period should be the current, or previous month and year for LVS."));
				}
			}
		}

		bool CurrentMonthOrPreviousMonth(ZDateTime date)
		{
			var result = false;
			var current = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 01);
			if (date.IsValid)
			{
				var entryAuthorisation = new ZDateTime(date.Year, date.Month, 01);
				if (entryAuthorisation == current || entryAuthorisation == current.AddMonths(-1))
				{
					result = true;
				}
			}
			return result;
		}

		#endregion

		#region CheckJE_WarehouseReleaseDate (common)

		protected override void CheckJE_WarehouseReleaseDate()
		{
			base.CheckJE_WarehouseReleaseDate();
			if (Parent.IsPARS)
			{
				DeclarationValidator.MessageErrorIfIsEntered(Parent.JE_WarehouseReleaseDateInfo, string.Empty, ValidateForMessageType.ACROSS);
			}
		}

		#endregion

		#region CheckJE_RL_NKFinalDestination

		protected override void CheckJE_RL_NKFinalDestination()
		{
			if (!Parent.IsConsolidatedLVS)
			{
				base.CheckJE_RL_NKFinalDestination();
			}
		}

		#endregion

		#region CheckJE_RL_NKPortOfArrival

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			if (Parent.ShowShipmentRelatedFieldsOrSea)
			{
				base.CheckJE_RL_NKPortOfArrival();
			}
		}

		#endregion

		#region CheckJE_RL_NKOrigin

		protected override void CheckJE_RL_NKOrigin()
		{
			if (!Parent.IsConsolidatedLVS)
			{
				base.CheckJE_RL_NKOrigin();
			}
		}

		#endregion

		#region CheckJE_RL_NKPortOfLoading

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			if (!Parent.IsConsolidatedLVS && Parent.ShowShipmentRelatedFields)
			{
				base.CheckJE_RL_NKPortOfLoading();
			}
		}

		#endregion

		#region CheckJE_TransportMode

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			Parent.Validation.ValidateJE_CarrierCode();
		}

		#endregion

		#region CheckJE_TotalNoOfPacksPackTypeIsAValidCode

		protected override void CheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			if (Parent.IsIID)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List);
			}
			else
			{
				base.CheckJE_TotalNoOfPacksPackTypeIsAValidCode();
			}
		}

		#endregion

		#region CheckJE_LocationOfGoods (ACROSS)

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();

			if (!Parent.IsLVS)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo, (IBusinessObjectCollection)Parent.Lookups.SubLocationCodes);
				if (!Parent.IsPARS && DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.JE_LocationOfGoods.IsEmpty && Parent.CA_SubLocationName.IsEmpty)
				{
					Parent.JE_LocationOfGoodsInfo.AddMessageError(LocationOfGoodsMustBeEntered);
				}
				if (!Parent.JE_LocationOfGoods.IsEmpty && !Parent.JE_LocationOfGoodsInfo.HasNotifications())
				{
					var subLocation = CACSubLocation.Load(Parent.Factory, Parent.JE_LocationOfGoods);
					if (subLocation != null && subLocation.SL_Port.TrimStart('0') != Parent.JE_CustomsOffice.TrimStart('0'))
					{
						Parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("ae07a08f-00ee-4b06-914a-762a2c27dacd", "The Customs Port of Clearance is not valid for this Sub-Location. Either the Sub-Location code is incorrect or the Port of Clearance should be {0}", subLocation.SL_Port.TrimStart('0')));
					}
				}
			}
		}

		internal static string LocationOfGoodsMustBeEntered
		{
			get
			{
				return Res.GetString("5626e2b5-8c0e-42ac-9efd-2e6eaef9449d", "You must enter either a Sub-Location code or a text description for the location of goods.");
			}
		}

		#endregion

		#region CheckJE_CustomsOffice (common)

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			var declaration = Declaration;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo, declaration.Lookups.CBSAOffices, declaration != null && declaration.IsLVS ? Res.GetString("04c97016-20e8-44ab-947e-ee8c432e2f0f", "Reporting Port") : Res.GetString("6d440524-ed2b-4d26-b67f-623d7314d0c4", "Port Of Clearance"));
		}

		#endregion

		#region CheckPackagesActualPackageCount

		protected override void CheckPackagesActualPackageCount()
		{
			base.CheckPackagesActualPackageCount();
			var parent = Parent;
			if (parent.IsIID && parent.IsEnableACROSSValidation && parent.PackagesActualPackageCount == 0m)
			{
				parent.PackagesActualPackageCountInfo.AddMessageError(Res.GetString("F0A31212-E08E-4897-AF88-68AA10A7C300", "No packages have been associated with a bill of lading, which is required for IID filings"));
			}
		}

		#endregion

		#region CheckJE_CarrierCode

		protected override void CheckJE_CarrierCode()
		{
			base.CheckJE_CarrierCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CarrierCodeInfo, Parent.Lookups.JE_CarrierCodeList);

			var declaration = Declaration;
			if (!declaration.IsLVS && (declaration.IsSea || declaration.IsAir) && declaration.HasUSPlaceOfExportInvoice)
			{
				DeclarationValidator.MessageErrorIfNotEntered(Parent.JE_CarrierCodeInfo, string.Empty, ValidateForMessageType.B3CUSDEC);
			}

			var countOfCCNs = declaration.CargoControlNumbers.Count;
			if (declaration.IsIID)
			{
				if (countOfCCNs == 0)
				{
					Parent.JE_CarrierCodeInfo.AddMessageError(AtLeastOneCCNMustBeEntered);
				}
				if (countOfCCNs > 999)
				{
					Parent.JE_CarrierCodeInfo.AddMessageError(YouHaveEnteredMoreThan999CCNs);
				}
			}
			else
			{
				if (declaration.JE_MessageSubType != B3EntryTypeList.Codes.Voluntary && !declaration.IsOtherWarehouseEntry && !declaration.IsLVS && (!declaration.IsPARS || DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC)))
				{
					if (countOfCCNs == 0)
					{
						Parent.JE_CarrierCodeInfo.AddMessageError(AtLeastOneCCNMustBeEntered);
					}
					else if (countOfCCNs > 10)
					{
						Parent.JE_CarrierCodeInfo.AddMessageError(Only10CCNsMaximumAllowed);
					}
				}
				if (countOfCCNs > 99)
				{
					Parent.JE_CarrierCodeInfo.AddMessageError(YouHaveEnteredMoreThan99CCNs);
				}
			}

			if (declaration.IsPARS && countOfCCNs > 1)
			{
				DeclarationValidator.AddMessageError(Parent.JE_CarrierCodeInfo, Only1CCNIsAllowedForPARS, ValidateForMessageType.ACROSS);
			}

			if (!Parent.JE_CarrierCode.IsEmpty)
			{
				var carrier = new ZZRefCarrierCombined.Loader(Parent.Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, Parent.JE_CarrierCode);
				var transportMode = declaration.JE_TransportMode;
				if (carrier != null && !carrier.TransportModePairList.Any(x => x.Value && x.Description == transportMode) && !carrier.HasMatchingAttributes(new[] { transportMode }))
				{
					Parent.JE_CarrierCodeInfo.AddWarning(Res.GetString("40C7B37B-155E-4D3B-ABDF-12D8C12BF3C2", "Carrier not allowed for mode of transport '{0}'", transportMode));
				}
			}
		}

		internal static string AtLeastOneCCNMustBeEntered
		{
			get
			{
				return Res.GetString("4d81daaf-b779-4056-9e53-703aec051e74", "At least one CCN must be entered on the 'Numbers' tab, or on the 'Cargo Control Numbers' grid on the 'Packing' tab.");
			}
		}

		internal static string Only10CCNsMaximumAllowed
		{
			get
			{
				return Res.GetString("1d142e7d-9265-4cc8-b8b2-3c655012156f", "Only 10 CCNs maximum is allowed.");
			}
		}

		internal static string YouHaveEnteredMoreThan99CCNs
		{
			get
			{
				return Res.GetString("630F6B7B-41FE-4560-99E7-3C2835EF6113", "You have entered more than 99 CCNs.");
			}
		}

		internal static string YouHaveEnteredMoreThan999CCNs
		{
			get
			{
				return Res.GetString("6D60BA1D-4FC4-4DF7-8719-D1E42356EC8B", "You have entered more than 999 CCNs.");
			}
		}

		internal static string Only1CCNIsAllowedForPARS
		{
			get
			{
				return Res.GetString("c747723f-0783-460b-aebd-fb2f9c5fec09", "Only 1 CCN is allowed for PARS shipments.");
			}
		}

		#endregion

		#region Implementation

		protected override bool JE_MergeByRequired
		{
			get
			{
				return !Declaration.IsLVX;
			}
		}

		#endregion

	}
}
