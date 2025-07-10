using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Business
{
	public static class UniversalReferenceConstants
	{
		public static ZBool SLFCALCEXSIsValid => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(UniversalReferenceConstants.RefCusCodeListType.Codes.SLFCALCEXS, Core.Constants.CountryCodes.Canada, ZDateTime.Today);

		public static bool IsCAMQWAR => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true);

		public static bool IsCarmR2 => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true);

		public static bool IsRPPGrace => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.RPPGrace, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true);

		public static bool IsCBSABOValid(ZDateTime effectiveDate)
		{
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, effectiveDate, true);
		}

		public static ZDecimal GetCLVSThreshold(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString taxOrFeeCode)
		{
			return factory.GetCachedValue(string.Join("_", "RefCusTaxOrFee.Loader.LoadTaxOrFeeFromCodeDate", taxOrFeeCode, effectiveDate.ToString("yyMMdd", CultureInfo.InvariantCulture)), () =>
			{
				return new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Canada, taxOrFeeCode, effectiveDate)?.ZZF_Threshold ?? ZDecimal.Zero;
			});
		}

		public static readonly ZDateTime SyncCAReferenceCutOffDate = new ZDateTime(2024, 10, 04);

		public static readonly ZString TempAreaCode = "XXX";

		public static readonly ZDateTime UNPackTypeStartDate = new ZDateTime(1994, 1, 1);

		public static int CBSAOfficeCodeMaxLength => 4;

		[ThreadSafe]
		public static string[] ExciseTaxCodesRequireSelfCalculation = { "C01", "C02", "C03", "C04", "C08", "C10", "C12", "C14", "E14", "E15", "E22", "E37", "E38", "E39", "E40" };

		public static class RefCusCodeListType
		{
			public static class Codes
			{
				public const string SubLocation = "SUBLC";
				public const string CFIAMiscCodes = "CFIAM";
				public const string CBSAErrorCodes = "CAERR";
				public const string SLFCALCEXS = "SLFCALCEXS";
				public const string CasualImportCommodity = "CASCM";
				public const string CACTradeZone = "USFTZ";
			}
		}

		public static class RefCusCodeListAttributes
		{
			public static class Names
			{
				public const string Division = "Division";
				public const string Port = "PORT";
				public const string Street = "Street";
				public const string City = "CITY";
				public const string Province = "Province";
				public const string PostCode = "PostCode";
				public const string Type = "Type";
			}
		}

		public static class RefSysConfigType
		{
			public static class Codes
			{
				public const string CAAIRSUriFrench = "CAAIRSUriF";
				public const string CAAIRSUriEnglish = "CAAIRSUriE";
				public const string CAAIRSEndUseTextID = "CAAIRSEUID";
				public const string CAAIRSExtensionTextID = "CAAIRSETID";
				public const string CAAIRSMiscTextID = "CAAIRSMSID";
				public const string CAAIRSIIDTableID = "CAAIRSITID";
				public const string CAAIRSPostDataTemplate = "CAAIRSPDTA";
				public const string CAAIRSIIDWebPageTitleFrench = "CAAIRSIWTF";
				public const string CAAIRSIIDWebPageTitleEnglish = "CAAIRSIWTE";
				public const string CAAIRSMaterializedGridTitleFrench = "CAAIRSMGTF";
				public const string CAAIRSMaterializedGridTitleEnglish = "CAAIRSMGTE";
				public const string CAAIRSDeMaterializedGridTitleFrench = "CAAIRSDMTF";
				public const string CAAIRSDeMaterializedGridTitleEnglish = "CAAIRSDMTE";
				public const string CAAIRSRegistrationGridTitleFrench = "CAAIRSREGF";
				public const string CAAIRSRegistrationGridTitleEnglish = "CAAIRSREGE";
				public const string CAAIRSCodeTextFrench = "CAAIRSCDTF";
				public const string CAAIRSCodeTextEnglish = "CAAIRSCDTE";
				public const string CAAIRSRegistrationTextFrench = "CAAIRSRTTF";
				public const string CAAIRSRegistrationTextEnglish = "CAAIRSRTTE";
				public const string CAAIRSOrTextFrench = "CAAIRSORTF";
				public const string CAAIRSOrTextEnglish = "CAAIRSORTE";
				public const string CAAIRSLPCOCodeAttributeKey = "CAAIRSLAKY";
				public const string CAAIRSLPCOCodeAttributeCode = "CAAIRSLACD";
				public const string CAAIRSLPCOCodeAttributeDescription = "CAAIRSLADS";
			}
		}

		public static class RefCusRate
		{
			public static class Codes
			{
				public const string EXS = "EXS";
				public const string DTY = "DTY";
			}
		}

		public static class CusConditionType
		{
			public static class Codes
			{
				public const string PGA = "PGA";
			}
		}
	}
}
