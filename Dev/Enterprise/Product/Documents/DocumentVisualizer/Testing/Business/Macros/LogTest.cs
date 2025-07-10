using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class LogTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var stmLog = dummy.Logs.AddNew(Events.Authorised,
				"free text reference",
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "AAA"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, "log"));

			Factory.Save();

			var log = new Log(stmLog);

			AssertEquals("EventTime", stmLog.SL_EventTime.ToDateTime(), log.EventTime);
			AssertEquals("PostedTime", stmLog.SL_PostedTimeUtc.ToDateTime(), log.PostedTime);
			AssertEquals("Reference", stmLog.ReferenceFreeText, log.Reference);
			AssertEquals("EventDetails", stmLog.DisplayEventReference, log.EventDetails);
			AssertEquals("Event.Code", Events.Authorised.Code, log.Event.Code);
			AssertEquals("User.Name", GlbStaff.CurrentUser.GS_FullName, log.User.Name);
		}
	}
}