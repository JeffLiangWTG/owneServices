using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(InvestigationItemForm))]
	public class InvestigationItemFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var investigationItem = Factory.New<InvestigationItem>();
			return new InvestigationItemForm(investigationItem);
		}
	}
}
