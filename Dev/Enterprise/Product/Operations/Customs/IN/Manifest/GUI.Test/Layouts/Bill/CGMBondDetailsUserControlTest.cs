using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMBondDetailsUserControl))]
sealed class CGMBondDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new CGMBondDetailsUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaBill), control.DataSourceType);
		}
	}
}
