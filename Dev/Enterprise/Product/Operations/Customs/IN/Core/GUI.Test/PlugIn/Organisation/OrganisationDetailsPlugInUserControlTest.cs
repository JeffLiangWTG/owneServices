using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(OrganisationDetailsPlugInUserControl))]
sealed class OrganisationDetailsPlugInUserControlTest : TestCaseWithFactory
{
	public void TestIsDiplomatCheckBox()
	{
		using (var control = new OrganisationDetailsPlugInUserControl())
		{
			var checkBox = control.IsDiplomatCheckBox;
			AssertNotNull(checkBox);
			AssertEquals("ZO_IsDiplomat", checkBox.GetBindingMember());
		}
	}
}
