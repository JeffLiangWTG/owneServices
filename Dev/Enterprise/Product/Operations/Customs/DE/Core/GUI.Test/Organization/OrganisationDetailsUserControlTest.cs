
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class OrganisationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsArePresent()
		{
			using (var control = new OrganisationDetailsUserControl())
			{
				AssertEquals("OrgCusAccountCollectionUserControl Count", 1, control.Controls.Find("OrgCusAccountCollectionUserControl", true).Length);
			}
		}
	}
}
