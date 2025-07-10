using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentTriageForm))]
	public class IncidentTriageFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var triage = Factory.New<IncidentTriage>();
			return new IncidentTriageForm(triage);
		}

		public void TestShowMenuItemsLinkLabel()
		{
			var triage = Factory.New<IncidentTriage>();
			using (var form = new IncidentTriageForm(triage))
			{
				form.Show();
				var linkLabel = form.Controls.Find("showMenuItemsLinkLabel", true).Single() as ZLinkLabel;
				linkLabel.OnLinkClicked_Exposed(null);
				AssertEquals(typeof(SourceModuleFinderForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestWorkflowTabPage()
		{
			var triage = Factory.New<IncidentTriage>();
			using (var form = new IncidentTriageForm(triage))
			{
				form.Show();
				var workflowTabPage = form.ExposedWorkflowTabPageForTest;
				Assert(workflowTabPage.TabRelevant);
			}
		}
	}
}
