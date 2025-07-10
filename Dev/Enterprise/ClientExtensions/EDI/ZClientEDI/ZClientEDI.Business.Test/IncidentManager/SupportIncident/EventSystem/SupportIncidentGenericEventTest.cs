namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentGenericEventTest : SupportIncidentEventTestCase
	{
		protected override void AssertTriggerLastEventResult(bool result, int taskCountBeforeTrigger, int taskCountAfterTrigger)
		{
			Assert("Last event is re-triggered", result);
			AssertEquals("Task count has no change because event code DDD is not included in event factory", taskCountBeforeTrigger, taskCountAfterTrigger);
		}

		protected override IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident)
		{
			return new SupportIncidentGenericEvent((SupportIncident)incident, "DDD");
		}
	}
}