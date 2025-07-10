using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRStatisticalClassificationPeriodCharacteristic))]
	sealed class CMRStatisticalClassificationPeriodCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadWithKey()
		{
			var tariffNumber = "00000000";
			var statCode = "00";
			short periodID = 0;

			var result = CMRStatisticalClassificationPeriodCharacteristic.Load(Factory, tariffNumber, statCode, periodID);
			AssertEquals("No record found", 0, result.Length);

			var record = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			result = CMRStatisticalClassificationPeriodCharacteristic.Load(Factory, tariffNumber, statCode, periodID);
			AssertEquals("No record found", 0, result.Length);

			record.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = periodID;
			result = CMRStatisticalClassificationPeriodCharacteristic.Load(Factory, tariffNumber, statCode, periodID);
			AssertEquals("No record found", 0, result.Length);

			record.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = statCode;
			result = CMRStatisticalClassificationPeriodCharacteristic.Load(Factory, tariffNumber, statCode, periodID);
			AssertEquals("No record found", 0, result.Length);

			record.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = tariffNumber;
			result = CMRStatisticalClassificationPeriodCharacteristic.Load(Factory, tariffNumber, statCode, periodID);
			AssertEquals("One record found", 1, result.Length);
			AssertEquals("One record found", record, result[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
	}
}
