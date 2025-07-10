using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Shared;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Test
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(XtMessageEvent))]
	sealed class XtMessageEventTest : NonPersistentBusinessObjectTestCase
	{
		public void TestXtMessageEventTest_Success()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_InterchangeNum = "100";

			var obj = new XtMessageEvent(interchange, new XtMessageEventData(1, 101, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21Z\",\"logtext\":\"test logtext\"}"));
			AssertEquals(2, obj.LogEvent);
			AssertEquals(new ZDateTime("20-Oct-23 22:32:21", DateTimeKind.Local), obj.LogTime);
			AssertEquals("Sent to application: test logtext", obj.LogText);
			AssertEquals("101", obj.XtMsgId);
			AssertEquals(interchange.EI_InterchangeNum, obj.InterchangeId);
			AssertEquals(interchange.PK, obj.InterchangeGuid);
		}

		public void TestXtMessageEventTest_UseLogEventDescWhenEmptyLogText()
		{
			var obj = new XtMessageEvent(null, new XtMessageEventData(1, 101, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21Z\",\"logtext\":\"\"}"));
			AssertEquals(2, obj.LogEvent);
			AssertEquals(new ZDateTime("20-Oct-23 22:32:21", DateTimeKind.Local), obj.LogTime);
			AssertEquals("Sent to application", obj.LogText);
		}

		public void TestXtMessageEventTest_InvalidEvent()
		{
			//default test env time zone: +10, no daylight saving time.
			var obj = new XtMessageEvent(null, new XtMessageEventData(1, 101, "{\"logevent\":\"ab\",\"time\":\"2023-10-20T12:32:21Z\",\"logtext\":\"\"}"));
			AssertEquals(0, obj.LogEvent);
			AssertEquals(new ZDateTime("20-Oct-23 22:32:21", DateTimeKind.Local), obj.LogTime);
			AssertEquals("", obj.LogText);
		}

		public void TestXtMessageEventTest_HKBranch()
		{
			// timezone: +8, without daylight saving time.
			AssertXtMessageEventTest_Branch("HKHKG", "20-Oct-23 20:32:21");
		}

		public void TestXtMessageEventTest_SydBranch()
		{
			// timezone: +10, with daylight saving time.
			AssertXtMessageEventTest_Branch("AUSYD", "20-Oct-23 23:32:21");
		}

		void AssertXtMessageEventTest_Branch(string homePort, string expectedLocalTimeString)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CM1";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = homePort;
			var depart = Factory.New<GlbDepartment>();
			depart.GE_Code = "DE1";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var obj = new XtMessageEvent(null, new XtMessageEventData(1, 101, "{\"logevent\":\"ab\",\"time\":\"2023-10-20T12:32:21Z\",\"logtext\":\"\"}"));
				AssertEquals(0, obj.LogEvent);
				AssertEquals(new ZDateTime(expectedLocalTimeString, DateTimeKind.Local), obj.LogTime);
				AssertEquals("", obj.LogText);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new XtMessageEvent(null, new XtMessageEventData(1, 101, "{}"));
		}
	}
}
