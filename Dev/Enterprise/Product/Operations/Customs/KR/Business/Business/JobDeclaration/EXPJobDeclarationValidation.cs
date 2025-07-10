using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class EXPJobDeclarationValidation : JobDeclarationValidation
	{
		public EXPJobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();

			Declaration.SetJobDocAddressParentAsNeedingValidation();
			Declaration.SetUDMEntryNumberParentAsNeedingValidation();
			Declaration.SetExtraInspectionDataParentAsNeedingValidation();

			ValidateIndustrialParkCode();
			ValidateFinalBondedWarehouse();
			ValidateInspectionDate();
			ValidateUnderbondMovementArrivalDate();
			ValidateUnderbondMovementDepartureDate();
		}

		protected override void CheckJE_ExportGoodsType()
		{
			base.CheckJE_ExportGoodsType();

			if (Parent.IsDeclarationProcedureTypeM)
			{
				if (!Parent.JE_ProcedureTypeSetM)
				{
					Parent.JE_ExportGoodsTypeInfo.AddMessageError(Res.GetString("6C58DB68-2929-4F96-9CBE-4A308B9361AC", "If Declaration Type is 'M', then Transaction Type should be 71, 78, or 79."));
				}
			}

			if (Parent.JE_ExportGoodsType != TransactionTypeCodeList.Codes._11 && Parent.JE_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD)
			{
				Parent.JE_ExportGoodsTypeInfo.AddMessageError(Res.GetString("C2E19A4B-1D73-408A-8EBB-7E32E9D59B5A", "If Auto Drawback is ‘AD’, then Transaction Type must be ‘11’."));
			}

			if (Parent.IsDeclarationProcedureTypeE)
			{
				if (Parent.JE_ExportGoodsType != TransactionTypeCodeList.Codes._15 && Parent.JE_ExportGoodsType != TransactionTypeCodeList.Codes._17 && Parent.JE_ExportGoodsType != TransactionTypeCodeList.Codes._103)
				{
					Parent.JE_ExportGoodsTypeInfo.AddMessageError(ExportGoodsTypeCheckError);
				}
			}
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ExportGoodsTypeInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			if (Parent.JE_ExportGoodsType == TransactionTypeCodeList.Codes._80 && Parent.JE_MessageSubType != ExportTypeCodeList.Codes.B && Parent.JE_MessageSubType != ExportTypeCodeList.Codes.H)
			{
				Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("08F06EFC-DA14-4FA7-A32F-F6350D4731E5", "If Transaction Type is '80', then Export Type must be ‘B’ or ‘H’."));
			}
			if (!Parent.IsDeclarationProcedureTypeE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MessageSubTypeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageSubTypeInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			if (!Parent.IsDeclarationProcedureTypeE || !Parent.JE_CustomsDivision.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsDestinationInfo);
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			base.CheckJE_SubLocationOfGoods();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SubLocationOfGoodsInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			base.CheckJE_LocationOtherInformation();
			if (Parent.IsDeclarationProcedureTypeB || Parent.IsDeclarationProcedureTypeM)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOtherInformationInfo);
			}
		}

		protected override void CheckJE_TransportModeMandatory()
		{
			if (!Parent.IsDeclarationProcedureTypeE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInfo);
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportModeInfo);
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TotalNoOfPacksPackTypeInfo);
		}

		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();
			if (Parent.BrokerAddress != null)
			{
				if (Parent.BrokerAddress.CompanyName.IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(GetMissingCompanyMessage((NoResString)"company name"));
				}
				if (Parent.BrokerAddress.Header.GetRepresentativeName().IsEmpty)
				{
					Parent.JE_GBInfo.AddMessageError(Res.GetString("62BEAE3E-13F2-45EE-B2ED-2EFA3E425577", "The name of this company's representative is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact."));
				}
			}
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			if (Parent.IsDeclarationProcedureTypeM)
			{
				if (Parent.Forwarder != null)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(Res.GetString("29CBD4F0-6699-4121-B021-BF9F074343DD", "When Declaration Type is 'M', you should not enter Forwarder"));
				}
			}
			else if (IsUnderbondPeriodRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ForwarderInfo);
				if (Parent.Forwarder != null)
				{
					if (Parent.Forwarder.GetRepresentativeName().IsEmpty)
					{
						Parent.JE_OH_ForwarderInfo.AddMessageError(MissingRepresentativeMessage);
					}
				}
			}
		}

		protected override void CheckJE_OA_SellerAddress()
		{
			base.CheckJE_OA_SellerAddress();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_SellerAddressInfo);
			if (!Parent.SellerOrgPK.IsEmpty && Parent.JE_OA_SellerAddress.IsEmpty)
			{
				Parent.JE_OA_SellerAddressInfo.AddError(NotEnteredOrgAddressMessage);
			}
			if (Parent.SellerAddress != null)
			{
				if (Parent.SellerAddress.CompanyName.IsEmpty)
				{
					Parent.JE_OA_SellerAddressInfo.AddMessageError(MissingCompanyNameMessage);
				}
				if (!Parent.IsDeclarationProcedureTypeE)
				{
					if (Parent.SellerAddress.Header.GetRegistrationNumber(Constants.IdentificationType.UnipassIDForOrganization).IsEmpty)
					{
						Parent.JE_OA_SellerAddressInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Unipass ID", Constants.IdentificationType.UnipassIDForOrganization));
					}
				}
			}
		}

		protected override void CheckJE_OA_ManufacturerAddress()
		{
			base.CheckJE_OA_ManufacturerAddress();
			if (!Parent.ManufacturerOrgPK.IsEmpty && Parent.JE_OA_ManufacturerAddress.IsEmpty)
			{
				Parent.JE_OA_ManufacturerAddressInfo.AddError(NotEnteredOrgAddressMessage);
			}
		}

		protected override void CheckJE_OA_SupplierAddress()
		{
			base.CheckJE_OA_SupplierAddress();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_SupplierAddressInfo);
			if (!Parent.JE_OH_Supplier.IsEmpty && Parent.JE_OA_SupplierAddress.IsEmpty)
			{
				Parent.JE_OA_SupplierAddressInfo.AddError(NotEnteredOrgAddressMessage);
			}
			if (Parent.SupplierAddress != null)
			{
				if (Parent.SupplierAddress.CompanyName.IsEmpty)
				{
					Parent.JE_OA_SupplierAddressInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
				}
				if (Parent.SupplierAddress.Postcode.IsEmpty)
				{
					Parent.JE_OA_SupplierAddressInfo.AddMessageError(EXPJobComInvoiceHeaderValidation.MissingPostCodeMessage);
				}
				else if (Parent.SupplierAddress.Postcode.Length > 5)
				{
					Parent.JE_OA_SupplierAddressInfo.AddMessageError(EXPJobComInvoiceHeaderValidation.MaxLengthPostCodeMessage);
				}

				if (Parent.SupplierAddress.Header != null)
				{
					if (Parent.SupplierAddress.Header.GetRepresentativeName().IsEmpty)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingRepresentativeMessage);
					}
					if (Parent.SupplierAddress.Header.GetRegistrationFirstMatchedBusinessOrIndividualID().IsEmpty)
					{
						if (Parent.SupplierAddress.Header.GetIsIndividual())
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(Res.GetString("E601208B-9DD3-4BAB-8046-7C67E50ED975", "There is no Individual Registration Number for this organization. Please press F3 here and add a number of type '01' or 'PAS' or '03' or '05' in Config > Registration Numbers/Codes on the Organization form."));
						}
						else
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", IdentificationType.BusinessRegNo));
						}
					}
					if (!Parent.IsDeclarationProcedureTypeE)
					{
						if (Parent.SupplierAddress.Header.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization).IsEmpty)
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage((NoResString)"Unipass ID", IdentificationType.UnipassIDForOrganization));
						}
					}
				}

				if (Parent.JE_ExporterType == ExporterTypeCodeList.Codes.B)
				{
					if (Parent.SellerAddress == Parent.SupplierAddress)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(Res.GetString("437B5E53-3515-4F53-AD6E-DE10E18C7F96", "You have indicated that the exporter type is 'B - Agent'. However the supplier and the exporter agent are set to the same organization. Please check."));
					}
				}
				if (Parent.IsSelfDeclaringOwner)
				{
					var declarantUnipassID = Parent.BrokerAddress?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization) ?? ZString.Empty;
					var supplierUnipassID = Parent.SupplierAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
					if (declarantUnipassID != supplierUnipassID)
					{
						Parent.JE_OA_SupplierAddressInfo.AddMessageError(string.Format(SelfDeclaringSupplierUnipassIDErrMsg, supplierUnipassID, declarantUnipassID));
					}
				}
				if (Parent.JE_ExporterType == ExporterTypeCodeList.Codes.A || Parent.JE_ExporterType == ExporterTypeCodeList.Codes.C)
				{
					if (Parent.SellerAddress != null)
					{
						var exporterUnipassID = Parent.SellerAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
						var exporterCompanyName = Parent.SellerAddress.CompanyName;
						var supplierUnipassID = Parent.SupplierAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
						var supplierCompanyName = Parent.SupplierAddress.CompanyName;
						if ((exporterUnipassID != supplierUnipassID) || (exporterCompanyName != supplierCompanyName))
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(Res.GetString("8088F440-69C2-41F7-8864-255C2E7464A1", "If exporter type is 'A' or 'C', the exporter company name and Unipass ID must be the same as the supplier company name and Unipass ID. Please press F3 here and edit Details > Full Name or Config > Registration Numbers/Codes type of '06' on the Organization form."));
						}
					}
				}

				if (Parent.JE_ExporterType == ExporterTypeCodeList.Codes.D)
				{
					if (Parent.SellerAddress != null)
					{
						if (Parent.SellerAddress.CompanyName != Parent.SupplierAddress.CompanyName || Parent.SellerAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization) != Parent.SupplierAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization))
						{
							Parent.JE_OA_SupplierAddressInfo.AddMessageError(SameSellerAndSupplierMessage);
						}
					}
				}
			}
		}

		protected override void CheckJE_LocationQualifier()
		{
			base.CheckJE_LocationQualifier();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationQualifierInfo);
		}

		public void ValidateIndustrialParkCode()
		{
			ValidateCalculatedProperty(Parent.IndustrialParkCodeInfo);
		}

		protected void CheckIndustrialParkCode()
		{
			if (Parent.IndustrialParkCode != Constants.ManufacturerDefaultCode.IndustrialParkCode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.IndustrialParkCodeInfo);
			}
		}

		public void ValidateFinalBondedWarehouse()
		{
			ValidateCalculatedProperty(Parent.FinalBondedWarehouseInfo);
		}

		protected void CheckFinalBondedWarehouse()
		{
			if (Parent.FinalBondedWarehouse == Constants.ContainerTerminalOperatorDefaultCode.GetFinalBondedAreaCode(Parent.JE_CustomsOffice))
			{
				if (!Parent.IsDeclarationProcedureTypeE)
				{
					Parent.FinalBondedWarehouseInfo.AddWarning(JobComInvoiceHeaderValidation.GetWarningMessageAboutToSendDefaultValue("CTO", (NoResString)"Final Bonded Warehouse", (NoResString)"Customs Office + 99999"));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.FinalBondedWarehouseInfo);
			}
		}

		public void ValidateInspectionDate()
		{
			ValidateCalculatedProperty(Parent.InspectionDateInfo);
		}

		protected void CheckInspectionDate()
		{
			if (Parent.JE_MessageSubType != ExportTypeCodeList.Codes.G)
			{
				if (!Parent.IsDeclarationProcedureTypeE)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.InspectionDateInfo);
				}

				ZDateTime possibleInspectionDate = Parent.LastestCustomsEntryIssueDate.IsEmpty ? ZDateTime.Today : Parent.LastestCustomsEntryIssueDate;

				if (Parent.InspectionDate.IsValid)
				{
					if ((ZDate)Parent.InspectionDate < (ZDate)possibleInspectionDate)
					{
						Parent.InspectionDateInfo.AddMessageError(Res.GetString("FD0F2763-B51F-4659-917C-E097A73A4BA1", "The Preferred Inspection Date must be greater than or equal to the Declaration Date."));
					}
				}
			}
			Parent.MarkAsNeedingValidation();
		}

		public void ValidateUnderbondMovementArrivalDate()
		{
			ValidateCalculatedProperty(Parent.UnderbondMovementArrivalDateInfo);
		}

		protected void CheckUnderbondMovementArrivalDate()
		{
			if (IsUnderbondPeriodRequired)
			{
				if (Parent.UnderbondMovementArrivalDate.IsEmpty)
				{
					Parent.UnderbondMovementArrivalDateInfo.AddMessageError(UnderbondPeriodCheckExportTypeError);
				}

				if (!Parent.UnderbondMovementDepartureDate.IsEmpty)
				{
					if (Parent.UnderbondMovementArrivalDate.IsEmpty)
					{
						Parent.UnderbondMovementArrivalDateInfo.AddMessageError(IssueDateMessageError);
					}
					else if (Parent.UnderbondMovementArrivalDate > Parent.UnderbondMovementDepartureDate)
					{
						Parent.UnderbondMovementArrivalDateInfo.AddMessageError(IssueDateMessageError);
					}
				}
			}
			else if (Parent.UnderbondMovementArrivalDate.IsValid)
			{
				if (Parent.IsDeclarationProcedureTypeM)
				{
					Parent.UnderbondMovementArrivalDateInfo.AddMessageError(UnderbondPeriodCheckDeclarationTypeError);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.UnderbondMovementArrivalDateInfo);
				}
			}
		}

		public void ValidateUnderbondMovementDepartureDate()
		{
			ValidateCalculatedProperty(Parent.UnderbondMovementDepartureDateInfo);
		}

		protected void CheckUnderbondMovementDepartureDate()
		{
			if (IsUnderbondPeriodRequired)
			{
				if (Parent.UnderbondMovementDepartureDate.IsEmpty)
				{
					Parent.UnderbondMovementDepartureDateInfo.AddMessageError(UnderbondPeriodCheckExportTypeError);
				}

				if (!Parent.UnderbondMovementArrivalDate.IsEmpty)
				{
					if (Parent.UnderbondMovementDepartureDate.IsEmpty)
					{
						Parent.UnderbondMovementDepartureDateInfo.AddMessageError(ExpiryDateMessageError);
					}
					else if (Parent.UnderbondMovementDepartureDate < Parent.UnderbondMovementArrivalDate)
					{
						Parent.UnderbondMovementDepartureDateInfo.AddMessageError(ExpiryDateMessageError);
					}
				}
			}
			else if (Parent.UnderbondMovementDepartureDate.IsValid)
			{
				if (Parent.IsDeclarationProcedureTypeM)
				{
					Parent.UnderbondMovementDepartureDateInfo.AddMessageError(UnderbondPeriodCheckDeclarationTypeError);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.UnderbondMovementDepartureDateInfo);
				}
			}
		}

		protected override void CheckJE_CarrierCode()
		{
			base.CheckJE_CarrierCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CarrierCodeInfo);
		}

		public bool IsUnderbondPeriodRequired
		{
			get
			{
				return ExportTypeCodeList.IsUnderbondPeriodRequired(Parent.JE_MessageSubType) && Parent.JE_ProcedureType != DeclarationProcedureTypeList.Codes.M;
			}
		}

		protected override void CheckJE_CustomsDivision()
		{
			base.CheckJE_CustomsDivision();
			if (!Declaration.IsDeclarationProcedureTypeE || !Declaration.JE_CustomsOffice.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_CustomsDivisionInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_CustomsDivisionInfo);
		}

		protected override void CheckJE_ExporterType()
		{
			base.CheckJE_ExporterType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Declaration.JE_ExporterTypeInfo);
		}

		protected override void CheckJE_TradeIDWithKP()
		{
			base.CheckJE_TradeIDWithKP();
			if (Declaration.JE_GoodsDestination == Core.Constants.CountryCodes.KoreaNorth)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TradeIDWithKPInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TradeIDWithKPInfo);
		}

		protected override void CheckJE_ReturnReason()
		{
			base.CheckJE_ReturnReason();
			if (!Declaration.IsDeclarationProcedureTypeM)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Declaration.JE_ReturnReasonInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Declaration.JE_ReturnReasonInfo);
			}
		}

		protected override void CheckJE_ReturnType()
		{
			base.CheckJE_ReturnType();
			if (!Declaration.IsDeclarationProcedureTypeM)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Declaration.JE_ReturnTypeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Declaration.JE_ReturnTypeInfo);
			}
		}

		protected override void CheckJE_GoodsCondition()
		{
			base.CheckJE_GoodsCondition();
			if (!Declaration.IsDeclarationProcedureTypeE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_GoodsConditionInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_GoodsConditionInfo);
		}

		protected override void CheckJE_TradeIndicatorWithKP()
		{
			base.CheckJE_TradeIndicatorWithKP();
			if (Declaration.JE_GoodsDestination == Core.Constants.CountryCodes.KoreaNorth)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TradeIndicatorWithKPInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TradeIndicatorWithKPInfo);
		}

		protected override void CheckJE_SimpleDRWApp()
		{
			base.CheckJE_SimpleDRWApp();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Declaration.JE_SimpleDRWAppInfo);
			if (Declaration.JE_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD && Declaration.InvoiceLines.Count > 0 && !Declaration.HasInvoiceLinesEligibleForSimpleDrawback)
			{
				Declaration.JE_SimpleDRWAppInfo.AddMessageError(Res.GetString("35CF08AE-1B6C-45CD-9415-B2609F464875", "When a simple drawback is applied, there must be at least one HS code which is eligible for the process, but there are currently none."));
			}
		}

		protected override void CheckJE_ProcedureType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Declaration.JE_ProcedureTypeInfo);
			if (Declaration.JE_ExportGoodsType == TransactionTypeCodeList.Codes._78 || Declaration.JE_ExportGoodsType == TransactionTypeCodeList.Codes._79)
			{
				if (!Declaration.IsDeclarationProcedureTypeM)
				{
					Declaration.JE_ProcedureTypeInfo.AddMessageError(Res.GetString("5779840A-FEA1-48B8-8786-D09AA35E73F6", "If Transaction Type Code is '78' or '79' then, Declaration Procedure Type must be 'M'."));
				}
			}

			if (Declaration.JE_MessageSubType == ExportTypeCodeList.Codes.F)
			{
				if (Declaration.JE_ProcedureType != DeclarationProcedureTypeList.Codes.L)
				{
					Declaration.JE_ProcedureTypeInfo.AddMessageError(ExportTypeCheckDeclarationTypeError);
				}
			}
		}

		protected override void CheckJE_ContainerPackMode()
		{
			base.CheckJE_ContainerPackMode();
			if (Declaration.IsDeclarationProcedureTypeB)
			{
				if (Declaration.JE_ContainerPackMode != ContainerPackModeCodeList.Codes.LC && Declaration.JE_ContainerPackMode != ContainerPackModeCodeList.Codes.FC)
				{
					Declaration.JE_ContainerPackModeInfo.AddMessageError(Res.GetString("C6BFF55F-2538-42A3-BEE7-CD740A9D34A5", "If 'Declaration Type' is 'B' then 'Container Pack' must be 'FC' or 'LC'."));
				}
			}
		}

		protected override void CheckJE_LocationIDInBondedArea()
		{
			base.CheckJE_LocationIDInBondedArea();
			if (Parent.JE_ProcedureType == DeclarationProcedureTypeList.Codes.B && Parent.JE_LocationIDInBondedArea.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationIDInBondedAreaInfo);
			}
		}

		protected override bool IsJE_ContainerPackModeMandatory => !Declaration.IsDeclarationProcedureTypeE;

		public static string IssueDateMessageError => Res.GetString("2469AC2C-FC68-4C13-8145-885C17E12C14", "Please enter an issue date that is earlier than or the same as the expiration date.");

		public static string ExpiryDateMessageError => Res.GetString("3A660E02-D7D3-4D49-A7EF-411A6CCA1504", "Please enter an expiry date that is later than or the same as the issue date.");

		public static string UnderbondPeriodCheckDeclarationTypeError => Res.GetString("7F91125E-C7EE-45AE-A41F-8CC0E0C00DC1", "Bonded Period must be empty when Declaration Type is 'M'.");

		public static string UnderbondPeriodCheckExportTypeError => Res.GetString("A2B74068-9E21-46A6-9E65-619D6DBE1F5B", "If Export Type is 'B', 'D' or 'E' then Bonded Period is mandatory.");

		public static string ExportGoodsTypeCheckError => Res.GetString("486C5F42-4AF8-4C59-8235-973E94447F2F", "Transaction type must be '15', '17' or '103' If Declaration is 'E'.");

		public static string SameSellerAndSupplierMessage => Res.GetString("4A02FC07-DC9B-490A-8025-6CE923017CFE", "If the Export Type is 'D', the Supplier and Exporter must have the same Unipass ID and company name.");

		public static string SelfDeclaringSupplierUnipassIDErrMsg => Res.GetString("1B5EF111-FB5A-4D1F-819C-760EA382BD63", "For the self - declaring suppliers, the declarant's and supplier's Unipass IDs should be the same, but they are different. The supplier has '{0}' and the declarant has '{1}'.You can see the supplier's Unipass ID (type '06') if you press F3 and go to Config > Registration Numbers/Codes on the Organization form. You can see the declarant's Unipass ID in the Organization Proxy of the branch(if there is a Customs Address of Record) or the Company profile.");

		public static string ExportTypeCheckDeclarationTypeError => Res.GetString("A75440F3-F5CE-4550-BAEA-E55AE4880CF2", "If Export Type is ‘F’ then Declaration Type must be 'L'.");
	}
}
