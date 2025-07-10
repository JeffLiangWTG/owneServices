using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(MessageAndCustomsStatusWithOverrideUserControl))]
sealed class MessageAndCustomsStatusWithOverrideUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new MessageAndCustomsStatusWithOverrideUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaManifestHeader), control.DataSourceType);
		}
	}
}
