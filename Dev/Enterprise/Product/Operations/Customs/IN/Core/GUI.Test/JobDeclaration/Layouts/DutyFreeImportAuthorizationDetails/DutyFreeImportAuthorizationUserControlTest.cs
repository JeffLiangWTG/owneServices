using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DutyFreeImportAuthorizationUserControl))]
sealed class DutyFreeImportAuthorizationUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new DutyFreeImportAuthorizationUserControl();
		AssertEquals("DataSourceType", typeof(DfiaExportItemDetail), control.DataSourceType);
	}
}
