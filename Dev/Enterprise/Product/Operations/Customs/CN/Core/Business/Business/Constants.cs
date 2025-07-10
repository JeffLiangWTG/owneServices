using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public static class Constants
	{
		public static class CusCodeDataCode
		{
			public const string BatchNumber = "BN";
		}

		public static class CusCodeDataTypes
		{
			public static class Codes
			{
				public const string AdditionalInformation = "ADI";
				public const string MergingRule = "MGR";
				public const string CIQ = "CIQ";
				public const string CustomsOffice = "COF";
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule")]
				public const string EnterpriseQualification = "EPQ";
				public const string SpecialBusinessIdentifier = "SBI";
				public const string CargoAttribute = "CAT";
				public const string Package = "PKG";
				public const string OperationMatter = "OPM";
				public const string CusAttachment = "ATH";
			}

			public static class Descriptions
			{
				public static MultilingualString AdditionalInformation => ResString.GetMultilingualString("58F61508-792B-46D7-984E-D6C42533848A", "Additional Information");
				public static MultilingualString MergingRule => ResString.GetMultilingualString("366F3403-FBB9-422C-A5CA-3A7EE6C8290C", "Merging Rule");
				public static MultilingualString CIQ => ResString.GetMultilingualString("08BB9041-E546-4D56-80D5-0D6A2B9CB2A0", "CIQ");
				public static MultilingualString CustomsOffice => ResString.GetMultilingualString("B3601BB0-EA8C-4AA8-B59B-2EBAE9972B11", "Customs Office");

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule")]
				public static MultilingualString EnterpriseQualification => ResString.GetMultilingualString("60C29C3D-E825-4AF0-9583-B0358CC9A488", "Enterprise Qualification");

				public static MultilingualString SpecialBusinessIdentifier => ResString.GetMultilingualString("3B12B114-B280-4D7C-AF60-2501E022CC38", "Special Business Identifier");
				public static MultilingualString CargoAttribute => ResString.GetMultilingualString("EFA636E1-6144-477C-B7E2-FC3F389C86AF", "Cargo Attribute");
				public static MultilingualString Package => ResString.GetMultilingualString("7ADB366A-DFAE-4F2B-BE62-F63537A1CBA8", "Package");
				public static MultilingualString OperationMatter => ResString.GetMultilingualString("2EB4BB02-19D1-43F4-99E7-3CD933B1D518", "Operation Matter");
				public static MultilingualString CusAttachment => ResString.GetMultilingualString("FD0C3273-41BC-4C82-8851-644A8DBCF3B5", "Attachment");
			}
		}

		public static class EntryChargeTypes
		{
			public const string CustomsDuty = "DTY";
			public const string Vat = "VAT";
		}

		public static class CNSWClient
		{
			public const string EHubClientStatusOK = "OK";
			public const string ConfigNameForeService = "CNCustomsSW";
		}

		#region SuppressResourceStringsCheckRegion
		public static class UniversalReferenceConstants
		{
			public static class CusTariffTypes
			{
				public const string ChinaCIQTariff = "CIQ";
			}

			public static class CusTariffAttributeName
			{
				public const string AdditionalInfomation = "AdditionalInfo";
				public const string ImportCIQRequirement = "ImportCIQRequirement";
				public const string ExportCIQRequirement = "ExportCIQRequirement";
				public const string ImportCUSRequirement = "ImportCUSRequirement";
				public const string ExportCUSRequirement = "ExportCUSRequirement";
				public const string CommodityType = "COMMODITYTYPE";
				public const string SupportsTSD = "SupportsTSD";
			}

			public static class CusTariffAttributeValue
			{
				public const char StandardCIQImportRequirement = 'M';
				public const char StandardCIQExportRequirement = 'N';
				public const string CommodityTypeMED = "MED";
				public const string CommodityTypeCFCS = "CFCS";
				public const string CommodityTypeUME = "UME";
				public const string CommodityTypeATP = "ATP";
				public const string CommodityTypeDGC = "DGC";
				public const char CUSExportRequirementB = 'B';
				public const char CUSImportRequirementA = 'A';
				public const char CIQImportRequirementL = 'L';
				public const string NotSupportsTSD = "N";
			}

			public static class CusCodeListAttributeName
			{
				public const string DisplayCode = "DisplayCode";
				public const string RequiresLineNumber = "RequiresLineNumber";
				public const string ApplicableCountry = "ApplicableCountry";
				public const string Alias = "Alias";
				public const string CustomsOffice = "CustomsOffice";
				public const string SupportsDeclarationOfOrigin = "SupportsDeclarationOfOrigin";
				public const string TSDSupported = "TSDSupported";
			}

			public static class RequiresLineNumberAttributeValue
			{
				public const string Mandatory = "Mandatory";
				public const string Optional = "Optional";
			}

			public static class RefCusRateTypes
			{
				public const string CustomsDuty = "DTY";
				public const string ExportDuty = "EXP";
				public const string Excise = "EXC";
				public const string VAT = "VAT";
			}

			public static class RefCusRateCodes
			{
				public const string CustomsDuty = "DTY";
			}

			public static class RateFormulaCountrySpecificValue
			{
				public const string CVInUSD = "CVINUSD";
			}

			public static class RefCusTaxOrFee
			{
				public const string DelayedFeeRate = "\u6EDE\u62A5\u8D39\u7387";
				public const string DDF = "DDF";
				public const string OTH = "OTH";
			}
		}
		#endregion

		public static class CusSupportingInfoTypes
		{
			public const string CIQProductQualification = "PQD";
			public const string CusSupportingDocument = "SUP";
		}

		public static class JobDeclarationUniversalMessagingEventTypes
		{
			public const string SingleWindow = "SW";
		}

		public static class PrimaryPreferenceCodes
		{
			public const string Normal = "NORMAL";
			public const string FreeTradeAgreement = "FTA";
			public const string MostFavouredNations = "MFN";
			public const string LeastDevelopedCountries = "LDC";
		}

		public static class DocumentCodes
		{
			public const string CertificateOfOrigin = "1Y";
			public const string ImportLicense = "01";
		}

		public static class RefCusCodeListTypes
		{
			public const string CNDesignatedSitesUnderSupervisionForImportedMeat = "DSSMT";
			public const string CNDesignatedSitesUnderSupervisionForImportedFrozenandFreshSeafoodProducts = "DSSSF";
			public const string CNDesignatedSitesUnderSupervisionForImportedGrain = "DSSGN";
			public const string CNDesignatedSitesUnderSupervisionForImportedFruit = "DSSFT";
			public const string CNDesignatedSitesUnderSupervisionForImportedEdibleAquaticAnimals = "DSSAA";
			public const string CNDesignatedSitesUnderSupervisionForImportedPlantSeedlings = "DSSPS";
			public const string CNDesignatedSitesUnderSupervisionForImportedLogs = "DSSLG";
			public const string CNIsolationSitesforQuarantineofImportedAnimals = "ISQAN";
		}

		public static class TradeAgreementCodes
		{
			public static class Codes
			{
				public const string LDC = "13";
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Chinese")]
			public static class Descriptions
			{
				public const string LDC = "最不发达国家特别优惠关税待遇";
			}
		}

		public static class CNCustomsUNDGPackingGroups
		{
			public const string HighDanger = "1";
			public const string MediumDanger = "2";
			public const string LowDanger = "3";
			public const string Unknown = "4";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html interpretation.")]
		public static class MessageHtmlInterpretation
		{
			public const string HtmlInterpretationHeader = "<table cellpadding=\"3\" cellspacing=\"0\" width=\"100% \" border=\"1\" style=\"font-size: 14px; border: 1px solid gray; border-collapse: collapse; font-family: Arial, sans-serif;\">";
			public const string HtmlInterpretationTail = "</table>";
			public const string SegmentHtmlTemplate = @"<tr><td width=""75px"">{0}</td><td>{1}</td></tr>";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html interpretation.")]
		public static class MessageDescriptions
		{
			public const string AcdAgrResponse = "报关单代理委托导入";
			public const string CSWResponseMessage = "报关单导入单一窗口";
		}

		public static class JobComInvLineRefType
		{
			public const string FormulaPricingRecordNumber = "FPRN";
		}
	}
}
