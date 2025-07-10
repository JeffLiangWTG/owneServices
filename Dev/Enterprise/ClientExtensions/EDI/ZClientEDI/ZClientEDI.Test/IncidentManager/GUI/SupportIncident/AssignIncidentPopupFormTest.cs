using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(AssignIncidentPopupForm))]
	public class AssignIncidentPopupFormTest : BaseIncidentPopupFormTest
	{
		public void TestAction()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			using (AssignIncidentPopupForm form = new AssignIncidentPopupForm(action))
			{
				AssertEquals(true, ((SupportIncidentAssignStaffAction)form.BusinessEntity).AssignToOther);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			return new AssignIncidentPopupForm(action);
		}
	}
}
