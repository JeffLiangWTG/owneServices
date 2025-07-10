using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDeclarationChargeCalculator
	{
		public CMRDeclarationChargeCalculator(IDeclarationChargeProvider entry)
		{
			this.entry = entry;
		}

		readonly IDeclarationChargeProvider entry;

		BusinessObjectFactory Factory => factory ?? (factory = entry is CusEntryHeader entryHeader ? entryHeader.Factory : new BusinessObjectFactory());
		BusinessObjectFactory factory;

		protected virtual RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader => refCusTaxOrFeeLoader ?? (refCusTaxOrFeeLoader = new RefCusTaxOrFee.Loader(Factory));
		RefCusTaxOrFee.Loader refCusTaxOrFeeLoader;

		ZDecimal Deminimus => UniversalReferenceHelper.GetDeminimus(Factory);

		public ZDecimal DeclarationProcessingCharge
		{
			get
			{
				if (!declarationProcessingCharge.HasValue)
				{
					ZDecimal result = 0m;
					if (!entry.IsS162ATemporaryImport && !entry.IsSOFADeclaration)
					{
						var currentCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
						var effectiveDutyDate = entry.EffectiveDutyDate;
						var loader = RefCusTaxOrFeeLoader;

						if (entry.N10CustomsValue > Deminimus)
						{
							switch (entry.TransportMode)
							{
								case TransportModeEnum.Sea:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DSN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DSH, effectiveDutyDate), entry.N10CustomsValue);
									break;
								case TransportModeEnum.Air:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DAN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DAH, effectiveDutyDate), entry.N10CustomsValue);
									break;
								case TransportModeEnum.Post:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DPN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DPH, effectiveDutyDate), entry.N10CustomsValue);
									break;
							}
						}

						if (entry.N20CustomsValue > Deminimus)
						{
							switch (entry.TransportMode)
							{
								case TransportModeEnum.Sea:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DSN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DSH, effectiveDutyDate), entry.N20CustomsValue);
									break;
								case TransportModeEnum.Air:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DAN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DAH, effectiveDutyDate), entry.N20CustomsValue);
									break;
								case TransportModeEnum.Post:
									result += GetDeclarationProcessingCharge(loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DPN, effectiveDutyDate), loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DPH, effectiveDutyDate), entry.N20CustomsValue);
									break;
							}
						}

						if (entry.N30CustomsValue > ZDecimal.Zero)
						{
							result += loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.DNW, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero;
						}
					}

					declarationProcessingCharge = result;
				}

				return declarationProcessingCharge.Value;
			}
		}
		ZDecimal? declarationProcessingCharge;

		ZDecimal GetDeclarationProcessingCharge(RefCusTaxOrFee lowValueRefCusTaxOrFee, RefCusTaxOrFee highValueRefCusTaxOrFee, ZDecimal customsValue)
		{
			var result = ZDecimal.Zero;
			if (lowValueRefCusTaxOrFee != null && highValueRefCusTaxOrFee != null)
			{
				var highValueThreshold = lowValueRefCusTaxOrFee.ZZF_Threshold;
				result = highValueThreshold == 0m || customsValue < highValueThreshold ? lowValueRefCusTaxOrFee.ZZF_Value : highValueRefCusTaxOrFee.ZZF_Value;
			}
			else if (lowValueRefCusTaxOrFee != null && highValueRefCusTaxOrFee == null)
			{
				result = lowValueRefCusTaxOrFee.ZZF_Value;
			}
			else if (lowValueRefCusTaxOrFee == null && highValueRefCusTaxOrFee != null)
			{
				result = highValueRefCusTaxOrFee.ZZF_Value;
			}

			return result;
		}

		public ZDecimal AQISProcessingCharge
		{
			get
			{
				ZDecimal result = 0m;
				var currentCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var effectiveDutyDate = entry.EffectiveDutyDate;
				var loader = RefCusTaxOrFeeLoader;
				if (entry.N10CustomsValue > Deminimus)
				{
					switch (entry.TransportMode)
					{
						case TransportModeEnum.Sea:
							result += loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q1S, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero;
							break;
						case TransportModeEnum.Air:
							result += loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q1A, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero;
							break;
					}
				}
				if (entry.N20CustomsValue > Deminimus)
				{
					switch (entry.TransportMode)
					{
						case TransportModeEnum.Sea:
							result += loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q2S, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero;
							break;
						case TransportModeEnum.Air:
							result += loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q2A, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero;
							break;
					}
				}
				return result;
			}
		}

		public ZDecimal AQISContainerCharge
		{
			get
			{
				ZDecimal result = 0m;
				var currentCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var effectiveDutyDate = entry.EffectiveDutyDate;
				var loader = RefCusTaxOrFeeLoader;
				if (entry.TransportMode == TransportModeEnum.Sea)
				{
					if (entry.N10CustomsValue > Deminimus)
					{
						result += (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q1F, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfFCLContainers
							+ (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q1X, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfFCXContainers
							+ (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q1L, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfLCLContainers;
					}
					if (entry.N20CustomsValue > Deminimus)
					{
						result += (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q2F, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfFCLContainers
							+ (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q2X, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfFCXContainers
							+ (loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(currentCompany, Constants.Q2L, effectiveDutyDate)?.ZZF_Value ?? ZDecimal.Zero) * entry.NumberOfLCLContainers;
					}
				}
				return result;
			}
		}

		public static class Constants
		{
			public const string Q1F = "Q1F";
			public const string Q1X = "Q1X";
			public const string Q1L = "Q1L";
			public const string Q2F = "Q2F";
			public const string Q2X = "Q2X";
			public const string Q2L = "Q2L";
			public const string Q1A = "Q1A";
			public const string Q1S = "Q1S";
			public const string Q2A = "Q2A";
			public const string Q2S = "Q2S";
			public const string DAN = "DAN";
			public const string DAH = "DAH";
			public const string DPH = "DPH";
			public const string DSH = "DSH";
			public const string DNW = "DNW";
			public const string DPN = "DPN";
			public const string DSN = "DSN";
		}
	}
}
