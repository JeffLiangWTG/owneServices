using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRStatisticalClassificationPeriodSnapshot))]
	sealed class CMRStatisticalClassificationPeriodSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoad()
		{
			var tariffNumber = "00000000";
			var statCode = "00";
			var effectiveDutyDate = new ZDateTime(2005, 1, 1);

			AssertNull(CMRStatisticalClassificationPeriodSnapshot.Load(Factory, tariffNumber, statCode, effectiveDutyDate));

			var record = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			record.SC_TariffClassificationNumber = tariffNumber;
			AssertNull(CMRStatisticalClassificationPeriodSnapshot.Load(Factory, tariffNumber, statCode, effectiveDutyDate));

			record.SC_StatisticalClassificationCode = statCode;
			AssertNull(CMRStatisticalClassificationPeriodSnapshot.Load(Factory, tariffNumber, statCode, effectiveDutyDate));

			record.SC_StartDate = effectiveDutyDate;
			AssertEquals(record, CMRStatisticalClassificationPeriodSnapshot.Load(Factory, tariffNumber, statCode, effectiveDutyDate));
		}

		protected override BusinessObject GetNewBusinessObject() => CMRStatisticalClassificationPeriodSnapshot.New(Factory);
	}
}
