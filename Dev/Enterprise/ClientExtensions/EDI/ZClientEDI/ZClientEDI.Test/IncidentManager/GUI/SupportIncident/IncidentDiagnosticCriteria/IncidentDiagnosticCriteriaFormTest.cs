using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentDiagnosticCriteriaForm))]
	public class IncidentDiagnosticCriteriaFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var incidentDiagnosticCriteria = Factory.New<IncidentDiagnosticCriteria>();
			return new IncidentDiagnosticCriteriaForm(incidentDiagnosticCriteria);
		}

		public void TestFormHasPlugIns()
		{
			var incidentDiagnosticCriteria = Factory.New<IncidentDiagnosticCriteria>();
			using (var form = new IncidentDiagnosticCriteriaForm(incidentDiagnosticCriteria))
			{
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}
	}
}
