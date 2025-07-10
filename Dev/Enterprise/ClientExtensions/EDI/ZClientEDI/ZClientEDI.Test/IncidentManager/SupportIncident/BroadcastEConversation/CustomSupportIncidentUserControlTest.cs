using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	public class CustomSupportIncidentUserControlTest : TestCaseWithFactory
	{
		public void TestSelectedItems()
		{
			var incidents = new List<SupportIncident>() { Factory.NewWithValidTestData<SupportIncident>(), Factory.NewWithValidTestData<SupportIncident>() };
			using (var form = new ZForm())
			{
				form.Controls.Add(new CustomSupportIncidentUserControl(incidents));
				form.Show();

				var control = (CustomSupportIncidentUserControl)form.Controls.Find("CustomSupportIncidentUserControl", true)[0];
				AssertEquals(0, control.SelectedItems.Count());

				control.SelectAllForTest();
				AssertEquals(2, control.SelectedItems.Count());
			}
		}
	}
}
