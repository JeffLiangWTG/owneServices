
namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public static class Constants
	{
		public static class AddressType
		{
			public const string Consignee = "Consignee";
			public const string Exporter = "Exporter";
			public const string Shipper = "Shipper";
			public const string Manufacturer = "Manufacturer";
			public const string Originator = "Originator";
			public const string MailTo = "MailTo";
			public const string FinalConsigneeAddress = "FinalConsigneeAddress";
			public const string CustomsBroker = "CustomsBroker";
			public const string CFIAPaymentParty = "CFIAPaymentParty";
			public const string UltimateConsignee = "UltimateConsignee";
			public const string CFIAAccountOwner = "CFIAAccountOwner";
			public const string HarvestingParty = "HarvestingParty";
			public const string FoodProcessor = "FoodProcessor";
			public const string LPCOApplicant = "LPCOApplicant";
			public const string LPCOHolder = "LPCOHolder";
			public const string ECCCMachineManufacturer = "ECCCMachineManufacturer";
			public const string ECCCEngineLocation = "ECCCEngineLocation";
			public const string ECCCEvidenceOfConfirmityLocation = "ECCCEvidenceOfConfirmityLocation";
		}

		public static class AddInfoKeys
		{
			public static class Declaration
			{
				public const string PortOfClearance = "PortOfClearance";
				public const string SubLocationCode = "SubLocationCode";
				public const string ExamLocationName = "ExamLocationName";
				public const string PARSETA = "PARSETA";
				public const string CarrierCode = "CarrierCode";
			}

			public static class InvoiceHeader
			{
				public const string NetWeight = "NetWeight";
				public const string NetWeightUQ = "NetWeightUQ";
				public const string ValuationDateOverride = "ValuationDateOverride";
				public const string CountryOfOrigin = "CountryOfOrigin";
				public const string ProvinceOfOrigin = "ProvinceOfOrigin";
			}

			public static class InvoiceLine
			{
				public const string ValueForDutyCode = "ValueForDutyCode";
				public const string AuthorityNumber = "AuthorityNumber";
				public const string TRSNumber = "TRSNumber";
				public const string ProvinceOfOrigin = "ProvinceOfOrigin";
				public const string IsCompliantCompletion = "CompliantCompletion";
				public const string IsImportDateCompliant = "CompliantImportDate";
				public const string ImporterDeclarationCode = "ImporterDeclarationCode";
				public const string ImporterDeclarationCode2 = "ImporterDeclarationCode2";
				public const string ProductClassDescription = "ProductClassDescription";
				public const string VehicleConditionDescription = "VehicleConditionDescription";
				public const string CustomsValueInUSD = "CustomsValueInUSD";
				public const string Qty2 = "Qty2";
				public const string Qty2UM = "Qty2UM";
				public const string Qty3 = "Qty3";
				public const string Qty3UM = "Qty3UM";
			}

			public static class EntryLine
			{
				public const string GoodsShipmentSequence = "GoodsShipmentSequence";
				public const string CommoditySequence = "CommoditySequence";
			}

			public static class HCPGAHeader
			{
				public const string Category = "Category";
				public const string IntendedUseCode = "IntendedUseCode";
			}

			public static class CargoControlNumber
			{
				public const string Type = "CCN";
				public const string CCNumber = "CargoControlNumber";
				public const string BillType = "BillType";
				public const string BillNumber = "BillNumber";
			}

			public static class CusCALPCO
			{
				public const string CusAddInfoType = "CLP";
				public const string Description = "LPCO";
				public const string Type = "Type";
				public const string RefNo = "RefNo";
				public const string SecondaryRefNo = "SecondaryRefNo";
				public const string DIFRefNumberOrLocation = "DIFRefNumberOrLocation";
				public const string LPCOStartDate = "LPCOStartDate";
				public const string LPCOEndDate = "LPCOEndDate";
				public const string LPCOIssueDate = "LPCOIssueDate";
				public const string LPCOHolderName = "LPCOHolderName";
				public const string LPCOHolderType = "LPCOHolderType";
				public const string CountryOfIssuance = "CountryOfIssuance";
				public const string CountryOfOrigin = "CountryOfOrigin";
				public const string AuthorizationCountry = "AuthorizationCountry";
				public const string IsMixedCountryOfOrigin = "IsMixedCountryOfOrigin";
				public const string CommodityTypeCode = "CommodityTypeCode";
				public const string Qty = "Qty";
				public const string UQ = "UQ";
				public const string AuthorizedPartyContactName = "AuthorizedPartyContactName";
				public const string AuthorizedPartyContactEmail = "AuthorizedPartyContactEmail";
				public const string AuthorizedPartyContactPhone = "AuthorizedPartyContactPhone";
				public const string ApplicantContactName = "ApplicantContactName";
				public const string ApplicantContactEmail = "ApplicantContactEmail";
				public const string ApplicantContactPhone = "ApplicantContactPhone";
				public const string LPCOApplicant = "LPCOApplicant";
				public const string LPCOApplicantName = "LPCOApplicantName";
				public const string IsHolderOverridden = "IsHolderOverridden";
				public const string IsApplicantOverridden = "IsApplicantOverridden";
				public const string RN_NKSmeltAndPourCountryCode = "RN_NKSmeltAndPourCountryCode";
			}

			public const string FrenchPreferred = "FrenchPreferred";
			public const string LinkedBill = "LinkedBill";
			public const string DirectShipmentDate = "DirectShipmentDate";
			public const string DirectShipmentCountry = "DirectShipmentCountry";
			public const string DirectShipmentState = "DirectShipmentState";
			public const string MessageRefNumber = "MessageRefNumber";
			public const string AccountSecurityNo = "AccountSecurityNo";
			public const string EffectiveBill = "EffectiveBill";
		}

		public static class AddInfoBoolValues
		{
			public const string True = "Y";
			public const string False = "N";
		}
	}
}
