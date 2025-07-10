using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CNOrgAdditionalCustomsDefaultsControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = new CNOrgAdditionalCustomsDefaultsControl())
			{
				AssertEquals("CNOrgAdditionalCustomsDefaultsControl", control.Name);
				AssertNotNull(control.FindSingleOrDefault<Control>("CustomsProcedureDropEdit"));
				AssertNotNull(control.FindSingleOrDefault<Control>("LevyTypeDropEdit"));
				AssertNotNull(control.FindSingleOrDefault<Control>("ManualNoTextBox"));
			}
		}
	}
}
