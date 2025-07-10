using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMTransshipmentDetailsUserControl))]
sealed class CGMTransshipmentDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new CGMTransshipmentDetailsUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaBill), control.DataSourceType);
		}
	}
}
