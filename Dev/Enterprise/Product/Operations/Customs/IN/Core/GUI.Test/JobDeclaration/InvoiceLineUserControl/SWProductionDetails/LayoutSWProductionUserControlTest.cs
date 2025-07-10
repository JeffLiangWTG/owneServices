using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(LayoutSWProductionUserControl))]
sealed class LayoutSWProductionUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new LayoutSWProductionUserControl();
		AssertEquals("DataSourceType", typeof(SWProduction), control.DataSourceType);
	}
}
