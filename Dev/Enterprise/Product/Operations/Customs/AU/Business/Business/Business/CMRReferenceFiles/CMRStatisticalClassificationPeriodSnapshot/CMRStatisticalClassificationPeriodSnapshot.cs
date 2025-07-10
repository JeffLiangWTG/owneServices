
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodSnapshot : AutoCMRStatisticalClassificationPeriodSnapshot
	{
		public CMRStatisticalClassificationPeriodSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRStatisticalClassificationPeriodSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRStatisticalClassificationPeriodSnapshot>();
		}

		public static CMRStatisticalClassificationPeriodSnapshot Load(BusinessObjectFactory factory, ZString tariffNumber, ZString statCode, ZDateTime effectiveDutyDate)
		{
			CMRStatisticalClassificationPeriodSnapshot result = null;
			if (effectiveDutyDate.IsValid)
			{
				ZQuery filter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, tariffNumber.Replace(" ", "").Replace(".", ""));
				filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StatisticalClassificationCode, SQLComparisonOperator.Equal, statCode.Replace(" ", ""));
				filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDutyDate);

				ZQuery endDateFilter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, null);
				endDateFilter.AddToFilter(JoinCondition.Or, CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDutyDate);

				filter.AddToFilter(endDateFilter, JoinCondition.And);

				result = factory.LoadTop1<CMRStatisticalClassificationPeriodSnapshot>(filter);
			}
			return result;
		}
	}
}
