using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(EventPseudoApplicator))]
	internal sealed class EventPseudoApplicatorTest : PseudoApplicatorTest<EventPseudoApplicator>
	{
		public void TestRunAction_AddEvent()
		{
			AssertNotNull(Dummy1);
			AssertNotNull(Dummy2);
			AssertNotNull(Dummy3);
			Factory.Save();
			AssertEquals("precondition:", 0, CountEvents(Dummy1, Events.Delivered));
			AssertEquals("precondition:", 0, CountEvents(Dummy2, Events.Delivered));
			AssertEquals("precondition:", 0, CountEvents(Dummy3, Events.Delivered));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { Dummy1.PK, Dummy3.PK } };
			var runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			runner.Printer = ZGuid.NewZGuid();
			RunRunner(runner, "");
			AssertEquals("no event to add", 0, CountEvents(Dummy1, Events.Delivered));
			AssertEquals("no event to add", 0, CountEvents(Dummy2, Events.Delivered));
			AssertEquals("no event to add", 0, CountEvents(Dummy3, Events.Delivered));
			Action.SU_SE_NKDocumentEvent = Events.Delivered.Code;
			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			runner.Printer = ZGuid.NewZGuid();
			RunRunner(runner, "INFO: Starting Section: Events ...");
			AssertEquals("should have event added", 1, CountEvents(Dummy1, Events.Delivered));
			AssertEquals("should not have event added", 0, CountEvents(Dummy2, Events.Delivered));
			AssertEquals("should have event added", 1, CountEvents(Dummy3, Events.Delivered));
		}

		#region Implementation
		static int CountEvents(BusinessObject bizObj, Event eventType)
		{
			StmALog[] logs = bizObj.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code));
			return logs.Length;
		}

		protected override void ConfigureForInclusion(OperationalAction action)
		{
			action.SU_SE_NKDocumentEvent = Events.StaffVerbalWarningIssued.Code;
		}

		protected override void ConfigureForExclusion(OperationalAction action)
		{
			action.SU_SE_NKDocumentEvent = "";
		}

		protected override EventPseudoApplicator NewApplicator(OperationalActionRunner runner)
		{
			return new EventPseudoApplicator(runner);
		}
		#endregion
	}
}
