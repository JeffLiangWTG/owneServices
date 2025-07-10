
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodAdditionalDutyCalculation : AutoCMRTreatmentRatePeriodAdditionalDutyCalculation, ICMRDutyRate, IFourRates
	{
		public CMRTreatmentRatePeriodAdditionalDutyCalculation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentRatePeriodAdditionalDutyCalculation New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentRatePeriodAdditionalDutyCalculation>();
		}

		public static CMRTreatmentRatePeriodAdditionalDutyCalculation Load(CMRTreatmentRatePeriodSnapshot treatment)
		{
			ZQuery filter = new ZQuery(CMRTreatmentRatePeriodAdditionalDutyCalculationSchema.TD_TreatmentRatePeriodSnapshotCode, treatment.TP_Code);
			filter.AddToFilter(CMRTreatmentRatePeriodAdditionalDutyCalculationSchema.TD_TreatmentRatePeriodSnapshotPeriodIdentifier, treatment.TP_PeriodIdentifier);
			filter.AddToFilter(CMRTreatmentRatePeriodAdditionalDutyCalculationSchema.TD_TreatmentRatePeriodSnapshotPreferenceSchemeType, treatment.TP_PreferenceSchemeType);
			filter.AddToFilter(CMRTreatmentRatePeriodAdditionalDutyCalculationSchema.TD_TreatmentRatePeriodSnapshotRateNumber, treatment.TP_RateNumber);

			return treatment.Factory.LoadTop1<CMRTreatmentRatePeriodAdditionalDutyCalculation>(filter);
		}

		#region ICMRDutyRate Members

		ZString ICMRDutyRate.CalculationType
		{
			get { return TreatmentRate == null ? ZString.Empty : ((ICMRDutyRate)TreatmentRate).CalculationType; }
		}

		RateInfo[] ICMRDutyRate.RatesApplicable
		{
			get { return RateInfoCalculator.GetRateApplicable(this); }
		}

		#endregion

		#region Related BizO

		internal CMRTreatmentRatePeriodSnapshot TreatmentRate
		{
			get
			{
				if (fTreatmentRate == null)
				{
					fTreatmentRate = CMRTreatmentRatePeriodSnapshot.Load(Factory,
						TD_TreatmentRatePeriodSnapshotCode,
						TD_TreatmentRatePeriodSnapshotRateNumber,
						TD_TreatmentRatePeriodSnapshotPreferenceSchemeType,
						TD_TreatmentRatePeriodSnapshotPeriodIdentifier);
				}
				return fTreatmentRate;
			}
		}
		CMRTreatmentRatePeriodSnapshot fTreatmentRate;

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

		#region IFourRates Members

		ZDecimal IFourRates.CustomsRate
		{
			get
			{
				return TD_CustomsValueRate;
			}
		}

		ZDecimal IFourRates.FirstQtyRate
		{
			get
			{
				return TD_QuantityRate;
			}
		}

		ZString IFourRates.FirstUQ
		{
			get
			{
				return TreatmentRate == null ? ZString.Empty : ((IFourRates)TreatmentRate).FirstUQ;
			}
		}

		ZDecimal IFourRates.SecondQtyRate
		{
			get
			{
				return TD_SecondQuantityRate;
			}
		}

		ZString IFourRates.SecondUQ
		{
			get
			{
				return TreatmentRate == null ? ZString.Empty : ((IFourRates)TreatmentRate).SecondUQ;
			}
		}

		ZDecimal IFourRates.OtherDutyFactorRate
		{
			get
			{
				return TD_OtherDutyFactorRate;
			}
		}

		#endregion
	}
}
