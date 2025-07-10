using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ShippingBillOverrideUserControl))]
public class ShippingBillOverrideUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new ShippingBillOverrideUserControl();
		AssertEquals("DataSourceType", typeof(CusEntryInstruction), control.DataSourceType);
	}
}