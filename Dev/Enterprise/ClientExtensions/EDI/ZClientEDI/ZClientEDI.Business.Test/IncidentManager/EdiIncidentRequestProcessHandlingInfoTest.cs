using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class EdiIncidentRequestProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateCascadingTargets()
		{
			var request = Factory.NewWithValidTestData<EdiIncidentRequest>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_INC_Request = request.PK;
			Factory.Save();

			var trigger1 = incident.WorkflowItems.Triggers.AddNew();
			trigger1.P9_RespondToCascadedEvents = true;
			trigger1.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var trigger2 = incident.WorkflowItems.Triggers.AddNew();
			trigger2.P9_RespondToCascadedEvents = true;
			trigger2.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var trigger3 = incident.WorkflowItems.Triggers.AddNew();
			trigger3.P9_RespondToCascadedEvents = false;
			trigger3.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var trigger4 = incident.WorkflowItems.Triggers.AddNew();
			trigger4.P9_RespondToCascadedEvents = false;
			trigger4.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;
			Factory.Save();

			var documentImportedEventLog = request.Logs.AddNew(Events.DocumentImported);

			var info = new EdiIncidentRequestProcessHandlingInfo(request);
			var cascadingLinks = info.GetCascadingTargets(documentImportedEventLog);
			AssertEquals(1, cascadingLinks.Count());

			var triggers = cascadingLinks.Single().Triggers;
			AssertEquals("Should include triggers with DDI event and allow cascaded events", 2, triggers.Length);
			Assert(triggers.Contains(trigger1));
			Assert(triggers.Contains(trigger2));
			Assert(!triggers.Contains(trigger3));
			Assert(!triggers.Contains(trigger4));
		}
	}
}
