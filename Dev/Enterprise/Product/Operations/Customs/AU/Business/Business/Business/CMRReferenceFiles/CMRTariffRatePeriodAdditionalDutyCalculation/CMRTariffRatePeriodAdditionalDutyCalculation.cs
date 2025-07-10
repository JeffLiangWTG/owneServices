
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodAdditionalDutyCalculation : AutoCMRTariffRatePeriodAdditionalDutyCalculation, ICMRDutyRate, IFourRates
	{
		public CMRTariffRatePeriodAdditionalDutyCalculation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffRatePeriodAdditionalDutyCalculation New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffRatePeriodAdditionalDutyCalculation>();
		}

		public static CMRTariffRatePeriodAdditionalDutyCalculation Load(CMRTariffRatePeriodSnapshot tariffRate)
		{
			ZQuery filter = new ZQuery(CMRTariffRatePeriodAdditionalDutyCalculationSchema.TA_TariffRatePeriodSnapshotTariffClassificationNumber, tariffRate.TT_TariffClassificationNumber);
			filter.AddToFilter(CMRTariffRatePeriodAdditionalDutyCalculationSchema.TA_TariffRatePeriodSnapshotRateNumber, tariffRate.TT_RateNumber);
			filter.AddToFilter(CMRTariffRatePeriodAdditionalDutyCalculationSchema.TA_TariffRatePeriodSnapshotPeriodIdentifier, tariffRate.TT_PeriodIdentifier);
			filter.AddToFilter(CMRTariffRatePeriodAdditionalDutyCalculationSchema.TA_TariffRatePeriodSnapshotPreferenceSchemeType, tariffRate.TT_PreferenceSchemeType);

			return tariffRate.Factory.LoadTop1<CMRTariffRatePeriodAdditionalDutyCalculation>(filter);
		}

		#region ICMRDutyRate Members

		ZString ICMRDutyRate.CalculationType
		{
			get
			{
				return TariffRate == null ? ZString.Empty : ((ICMRDutyRate)TariffRate).CalculationType;
			}
		}

		RateInfo[] ICMRDutyRate.RatesApplicable
		{
			get
			{
				return RateInfoCalculator.GetRateApplicable(this);
			}
		}

		#endregion

		#region IFourRates Members

		ZDecimal IFourRates.CustomsRate
		{
			get
			{
				return TA_CustomsValueRate;
			}
		}

		ZDecimal IFourRates.FirstQtyRate
		{
			get
			{
				return TA_QuantityRate;
			}
		}

		ZString IFourRates.FirstUQ
		{
			get
			{
				return TariffRate == null ? ZString.Empty : ((IFourRates)TariffRate).FirstUQ;
			}
		}

		ZDecimal IFourRates.SecondQtyRate
		{
			get
			{
				return TA_SecondQuantityRate;
			}
		}

		ZString IFourRates.SecondUQ
		{
			get
			{
				return TariffRate == null ? ZString.Empty : ((IFourRates)TariffRate).SecondUQ;
			}
		}

		ZDecimal IFourRates.OtherDutyFactorRate
		{
			get
			{
				return TA_OtherDutyFactorRate;
			}
		}

		#endregion

		#region Related Object

		internal CMRTariffRatePeriodSnapshot TariffRate
		{
			get
			{
				if (fTariffRate == null)
				{
					fTariffRate = CMRTariffRatePeriodSnapshot.Load(Factory,
						TA_TariffRatePeriodSnapshotTariffClassificationNumber,
						TA_TariffRatePeriodSnapshotRateNumber,
						TA_TariffRatePeriodSnapshotPreferenceSchemeType,
						TA_TariffRatePeriodSnapshotPeriodIdentifier);
				}
				return fTariffRate;
			}
		}
		CMRTariffRatePeriodSnapshot fTariffRate;

		internal RateInfoCalculator RateInfoCalculator
		{
			get
			{
				if (fRateInfoCalculator == null)
				{
					fRateInfoCalculator = new RateInfoCalculator();
				}
				return fRateInfoCalculator;
			}
		}
		RateInfoCalculator fRateInfoCalculator;

		#endregion
	}
}
