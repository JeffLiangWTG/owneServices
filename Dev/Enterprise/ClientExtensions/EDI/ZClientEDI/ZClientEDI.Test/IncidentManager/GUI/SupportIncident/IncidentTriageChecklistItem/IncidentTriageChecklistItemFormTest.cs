using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentTriageChecklistItemForm))]
	public class IncidentTriageChecklistItemFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var checklistItem = Factory.New<IncidentTriageChecklistItem>();
			return new IncidentTriageChecklistItemForm(checklistItem);
		}
	}
}
