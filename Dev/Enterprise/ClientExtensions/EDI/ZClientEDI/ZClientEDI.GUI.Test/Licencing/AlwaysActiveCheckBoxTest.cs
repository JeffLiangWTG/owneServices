using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	class AlwaysActiveCheckBoxTest : TestCaseWithFactory
	{
		public void TestAlwaysActive()
		{
			using (var control = new AlwaysActiveCheckBoxForTest())
			{
				control.ReadOnlyForBindingProperty_Exposed.ReadOnlyForBinding = true;
				AssertEquals(false, control.ReadOnly);
				control.ReadOnly = true;
				AssertEquals(false, control.ReadOnly);
			}
		}
	}

	class AlwaysActiveCheckBoxForTest : AlwaysActiveCheckBox
	{
		public ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty_Exposed => base.ReadOnlyForBindingProperty;
	}
}
