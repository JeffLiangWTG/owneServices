
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodSnapshot : AutoCMRTariffRatePeriodSnapshot, ICMRDutyRate, IFourRates, ICompositeDutyRate, ICodeDescription
	{
		public CMRTariffRatePeriodSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffRatePeriodSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffRatePeriodSnapshot>();
		}

		public static CMRTariffRatePeriodSnapshot Load(ICMRDutyData dutyData, ZString tariffNumber)
		{
			CMRTariffRatePeriodSnapshot result = null;

			DutyDataFromInvoiceLine randomLineDutyData = dutyData.RandomLineDutyData;
			if (randomLineDutyData.EffectiveDutyDate.IsValid && !tariffNumber.IsEmpty)
			{
				var preferenceScheme = randomLineDutyData.Preference.IsEmpty ? AUAddInfo.GeneralPreferenceRate : randomLineDutyData.Preference.ToString();
				ZQuery finalFilter = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_PreferenceSchemeType, SQLComparisonOperator.Equal, preferenceScheme);
				finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, SQLComparisonOperator.Equal, tariffNumber.Replace(".", ""));

				ZString rateNumber = dutyData.RandomLineDutyData.RateNumber.IsEmpty ? new ZString("001") : randomLineDutyData.RateNumber.PadLeft(3, '0');
				finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_RateNumber, SQLComparisonOperator.Equal, rateNumber);
				finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, randomLineDutyData.EffectiveDutyDate);

				ZQuery endDateFilter = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, randomLineDutyData.EffectiveDutyDate);
				endDateFilter.AddToFilter(JoinCondition.Or, CMRTariffRatePeriodSnapshotSchema.TT_EndDate, SQLComparisonOperator.Equal, null);

				finalFilter.AddToFilter(endDateFilter, JoinCondition.And);

				result = dutyData.Factory.LoadTop1<CMRTariffRatePeriodSnapshot>(finalFilter);
			}
			return result;
		}

		public static CMRTariffRatePeriodSnapshot Load(BusinessObjectFactory factory, ZString tariffNumber, ZString rateNumber, ZString preference, ZShort periodID)
		{
			var preferenceScheme = preference.IsEmpty ? AUAddInfo.GeneralPreferenceRate : preference.ToString();
			ZQuery filter = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, tariffNumber);
			filter.AddToFilter(CMRTariffRatePeriodSnapshotSchema.TT_RateNumber, rateNumber);
			filter.AddToFilter(CMRTariffRatePeriodSnapshotSchema.TT_PreferenceSchemeType, preferenceScheme);
			filter.AddToFilter(CMRTariffRatePeriodSnapshotSchema.TT_PeriodIdentifier, periodID);
			return factory.LoadTop1<CMRTariffRatePeriodSnapshot>(filter);
		}

		public bool HasCustomsRateOnly
		{
			get
			{
				return !TT_CustomsValueRate.IsEmpty && TT_OtherDutyFactorRate.IsEmpty && TT_QuantityRate.IsEmpty && TT_SecondQuantityRate.IsEmpty;
			}
		}

		public ZString GetDutyRateDescription()
		{
			return "Duty: " + DutyRateDescriptor.GetDutyRateDescription(this);
		}

		#region ICMRDutyRate Members

		ZString ICMRDutyRate.CalculationType
		{
			get
			{
				return TT_CalculationType;
			}
		}

		RateInfo[] ICMRDutyRate.RatesApplicable
		{
			get
			{
				return RateInfoCalculator.GetRateApplicable(this);
			}
		}

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
				return TT_CustomsValueRate;
			}
		}

		ZDecimal IFourRates.FirstQtyRate
		{
			get
			{
				return TT_QuantityRate;
			}
		}

		ZString IFourRates.FirstUQ
		{
			get
			{
				return TT_QuantityUnit;
			}
		}

		ZDecimal IFourRates.SecondQtyRate
		{
			get
			{
				return TT_SecondQuantityRate;
			}
		}

		ZString IFourRates.SecondUQ
		{
			get
			{
				return TT_SecondQuantityUnit;
			}
		}

		ZDecimal IFourRates.OtherDutyFactorRate
		{
			get
			{
				return TT_OtherDutyFactorRate;
			}
		}

		#endregion

		#region ICompositeDutyRate Members

		IFourRates ICompositeDutyRate.AdditionalDutyRate
		{
			get { return CMRTariffRatePeriodAdditionalDutyCalculation.Load(this); }
		}

		ZString ICompositeDutyRate.CalculationType
		{
			get { return TT_CalculationType.ToUpper(); }
		}

		#endregion
		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Description
		{
			get { return SchemeDescriptor.GetDescriptionIncludingDutyRate(SchemeList, this); }
		}

		string ICodeDescription.Code
		{
			get { return TT_PreferenceSchemeType; }
		}

		#endregion

		CMRSchemeList SchemeList
		{
			get
			{
				if (fSchemeList == null)
				{
					fSchemeList = new CMRSchemeList();
				}
				return fSchemeList;
			}
		}
		CMRSchemeList fSchemeList;
	}
}
