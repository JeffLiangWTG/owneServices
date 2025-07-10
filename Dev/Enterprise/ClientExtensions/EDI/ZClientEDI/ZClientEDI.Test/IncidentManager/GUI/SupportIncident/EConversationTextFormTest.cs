using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(EConversationTextForm))]
	public class EConversationTextFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentResolutionWizardAction(incident);
			return new EConversationTextForm(incident);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "allMessagesRadioButton" || control.Name == "userMessagesRadioButton")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
