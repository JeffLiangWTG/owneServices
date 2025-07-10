using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMBillUserControl))]
sealed class CGMBillUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new CGMBillUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaBill), control.DataSourceType);
		}
	}
}
