
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodSnapshot : AutoCMRTreatmentRatePeriodSnapshot, ICMRDutyRate, IFourRates, ICompositeDutyRate, ICodeDescription
	{
		public CMRTreatmentRatePeriodSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentRatePeriodSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentRatePeriodSnapshot>();
		}

		/// <summary>
		/// The following is because of a bug in the ACS CMR reference file generator which affects just the one treatment code,
		/// Customs are aware of it, and are not going to fix it
		/// They said it was too much work.
		/// </summary>
		public override ZDecimal TP_CustomsValueRate
		{
			get
			{
				return TP_Code == "462" && TP_RateNumber == "61A" && base.TP_CustomsValueRate > 0 ? (ZDecimal)(-base.TP_CustomsValueRate) : base.TP_CustomsValueRate;
			}
		}

		public static CMRTreatmentRatePeriodSnapshot Load(ICMRDutyData dutyData, ZString treatmentCode)
		{
			DutyDataFromInvoiceLine randomLineDutyData = dutyData.RandomLineDutyData;

			CMRTreatmentRatePeriodSnapshot result = null;
			if (randomLineDutyData.EffectiveDutyDate.IsValid && !treatmentCode.IsEmpty)
			{
				ZQuery finalFilter = new ZQuery();
				if (randomLineDutyData.Preference.IsEmpty || randomLineDutyData.Preference == AUAddInfo.GeneralPreferenceRate)
				{
					finalFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, SQLComparisonOperator.Equal, AUAddInfo.GeneralPreferenceRate);
				}
				else
				{
					ZQuery preferenceFilter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, SQLComparisonOperator.Equal, randomLineDutyData.Preference);
					ZQuery generalPreferenceFilter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, SQLComparisonOperator.Equal, AUAddInfo.GeneralPreferenceRate);
					generalPreferenceFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_CalculationType, SQLComparisonOperator.Equal, Constants.DutyCalcTypes.Info);
					preferenceFilter.AddToFilter(generalPreferenceFilter, JoinCondition.Or);
					finalFilter.AddToFilter(preferenceFilter, JoinCondition.And);
				}
				finalFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_Code, SQLComparisonOperator.Equal, treatmentCode);
				ZString rateNumber = randomLineDutyData.TreatmentRateNumber.IsEmpty ? new ZString("001") : randomLineDutyData.TreatmentRateNumber.PadLeft(3, '0');
				finalFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_RateNumber, SQLComparisonOperator.Equal, rateNumber);
				finalFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, randomLineDutyData.EffectiveDutyDate);

				ZQuery endDateFilter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, randomLineDutyData.EffectiveDutyDate);
				endDateFilter.AddToFilter(JoinCondition.Or, CMRTreatmentRatePeriodSnapshotSchema.TP_EndDate, SQLComparisonOperator.Equal, null);

				finalFilter.AddToFilter(endDateFilter, JoinCondition.And);

				CMRTreatmentRatePeriodSnapshot[] possibleResults = dutyData.Factory.Load<CMRTreatmentRatePeriodSnapshot>(finalFilter);
				if (possibleResults.Length > 1)
				{
					foreach (CMRTreatmentRatePeriodSnapshot possibleResult in possibleResults)
					{
						if (possibleResult.TP_PreferenceSchemeType == randomLineDutyData.Preference)
						{
							result = possibleResult;
							break;
						}
					}
				}
				if (result == null && possibleResults.Length > 0)
				{
					result = possibleResults[0];
				}
			}
			return result;
		}

		public static CMRTreatmentRatePeriodSnapshot Load(BusinessObjectFactory factory, ZString treatmentCode, ZString rateNumber, ZString preference, ZShort periodID)
		{
			ZQuery filter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_Code, treatmentCode);
			filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_RateNumber, rateNumber);
			filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, preference);
			filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_PeriodIdentifier, periodID);

			return factory.LoadTop1<CMRTreatmentRatePeriodSnapshot>(filter);
		}

		public bool IsInformationOnly
		{
			get { return TP_CalculationType == Constants.DutyCalcTypes.Info; }
		}

		public bool IsCalculable
		{
			get { return TP_CalculationType == Constants.DutyCalcTypes.Calc || TP_CalculationType == Constants.DutyCalcTypes.Higher || TP_CalculationType == Constants.DutyCalcTypes.Lower; }
		}

		public ZString GetDutyRateDescription()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("Duty (");
			result.Append(TP_PreferenceSchemeType);
			result.Append(", ");
			result.Append(TP_RateNumber + ")");
			result.Append(":" + DutyRateDescriptor.GetDutyRateDescription(this));
			return result.ToString();
		}

		#region ICMRDutyRate Members

		ZString ICMRDutyRate.CalculationType
		{
			get { return TP_CalculationType; }
		}

		RateInfo[] ICMRDutyRate.RatesApplicable
		{
			get { return RateInfoCalculator.GetRateApplicable(this); }
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
			get { return TP_CustomsValueRate; }
		}

		ZDecimal IFourRates.FirstQtyRate
		{
			get { return TP_QuantityRate; }
		}

		ZString IFourRates.FirstUQ
		{
			get { return TP_QuantityUnit; }
		}

		ZDecimal IFourRates.SecondQtyRate
		{
			get { return TP_SecondQuantityRate; }
		}

		ZString IFourRates.SecondUQ
		{
			get { return TP_SecondQuantityUnit; }
		}

		ZDecimal IFourRates.OtherDutyFactorRate
		{
			get { return TP_OtherDutyFactorRate; }
		}

		#endregion

		#region ICompositeDutyRate Members

		IFourRates ICompositeDutyRate.AdditionalDutyRate
		{
			get { return CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(this); }
		}

		ZString ICompositeDutyRate.CalculationType
		{
			get { return TP_CalculationType.ToUpper(); }
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Description
		{
			get { return GetDutyRateDescription(); }
		}

		string ICodeDescription.Code
		{
			get { return TP_Code; }
		}

		#endregion
	}
}
