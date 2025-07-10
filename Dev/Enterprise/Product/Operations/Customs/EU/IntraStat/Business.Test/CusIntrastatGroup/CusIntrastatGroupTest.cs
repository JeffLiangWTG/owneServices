using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static CargoWise.EntityFramework.DataBoundResourceStrings;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatGroup))]
	sealed class CusIntrastatGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCaptions() => CombineAssertions(() =>
		{
			AssertEquals("Flow", GetDataForProperty(typeof(CusIntrastatGroup), nameof(CusIntrastatGroup.CIG_Flow)).Caption);
			AssertEquals("Group Number", GetDataForProperty(typeof(CusIntrastatGroup), nameof(CusIntrastatGroup.CIG_GroupNumber)).Caption);
			AssertEquals("Reporter", GetDataForProperty(typeof(CusIntrastatGroup), nameof(CusIntrastatGroup.ReporterCode)).Caption);
			AssertEquals("Reporting Period", GetDataForProperty(typeof(CusIntrastatGroup), nameof(CusIntrastatGroup.CIG_Period)).Caption);
			AssertEquals("Status", GetDataForProperty(typeof(CusIntrastatGroup), nameof(CusIntrastatGroup.CIG_Status)).Caption);
		});

		public void TestHumanReadableName()
		{
			AssertEquals("Intrastat - Report", report.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => IntrastatTestDataHelper.New(factory).NewCusIntrastatGroupWithValidData();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => report;

		protected override void SetUp()
		{
			report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
		}
		CusIntrastatGroup report;
	}

	[TestedType(typeof(CusIntrastatGroup))]
	sealed class CusIntrastatGroupClusterKeyTest : ClusterKeyMasterMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
			return report;
		}
	}
}
