using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
#if !WINZOR
	[TestedType(typeof(TriageAssistTreeModelView.TriageAssistTreeModel))]
	public class TriageAssistTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TriageAssistTreeModelView.TriageAssistTreeModel(Assist);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
	}
#endif
}
