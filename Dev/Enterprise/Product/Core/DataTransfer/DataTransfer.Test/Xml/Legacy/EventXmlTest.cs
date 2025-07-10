using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class EventXmlTest : TestCaseWithFactory
	{
		public void TestToStringWithoutAdditionalXmlForLog()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "LoginName";
			staff.GS_FullName = "Test";
			staff.GS_Code = "EYL";

			var log = Factory.New<BaseStmALog>();
			log.SL_Table = "Table";
			log.SL_Parent = ZGuid.NewZGuid();
			log.SL_IsEstimate = false;
			log.SL_Reference = "";
			log.SL_EventTime = new ZDateTime(2001, 12, 17, 9, 30, 47);
			log.SL_GS_NKUser = "EYL";
			log.SL_SE_NKEvent = Events.EditedARecord.Code;
			Factory.Save();
			using (log.GetValidationSuspender())
			{
				log.SL_SE_NKEvent = "XYY";

				var baseEventXml = new EventXml();
				baseEventXml.Add(log);

				var expectedPostedDate = log.SL_PostedTimeUtc.ToString("s");
				var expectedResult =
					"<Events><Event>" +
					"<Source>Table</Source>" +
					"<Code>XYY</Code>" +
					"<DateTime>2001-12-17T09:30:47</DateTime>" +
					"<PostedDateTime>" + expectedPostedDate + "</PostedDateTime>" +
					"<User>LoginName</User>" +
					"<IsEstimatedDate>false</IsEstimatedDate>" +
					"</Event></Events>";

				var result = baseEventXml.ToString();
				AssertEquals("Shouldn't contain the additional xml", expectedResult, result);
			}
		}

		public void TestToStringWithAdditionalXmlForLog()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "LoginName";
			staff.GS_FullName = "Test";
			staff.GS_Code = "EYL";

			var log = Factory.New<BaseStmALog>();
			log.SL_Table = "Table";
			log.SL_Parent = ZGuid.NewZGuid();
			log.SL_IsEstimate = false;
			log.SL_Reference = "";
			log.SL_EventTime = new ZDateTime(2001, 12, 17, 9, 30, 47);
			log.SL_GS_NKUser = "EYL";
			log.SL_SE_NKEvent = Events.EditedARecord.Code;
			Factory.Save();
			using (log.GetValidationSuspender())
			{
				log.SL_SE_NKEvent = "XYY";

				EventXml.Add(log);

				var expectedXml = "<extra>some additional xml</extra>";

				var result = EventXml.ToString();

				Assert("Should contain the additional xml", result.Contains(expectedXml));
			}
		}

		public void TestToStringAndADD()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "LoginName";
			staff.GS_FullName = "Test";
			staff.GS_Code = "EYL";

			var log = Factory.New<BaseStmALog>();

			log.SL_Table = "Table";
			log.SL_Parent = ZGuid.NewZGuid();
			log.SL_IsEstimate = false;
			log.SL_Reference = "";
			log.SL_EventTime = new ZDateTime(2001, 12, 17, 9, 30, 47);
			log.SL_GS_NKUser = "EYL";
			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;

			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_Table = "Table1";
				log1.SL_Parent = ZGuid.NewZGuid();
				log1.SL_IsEstimate = false;
				log1.SL_Reference = "";
				log1.SL_EventTime = new ZDateTime(2003, 12, 17, 9, 30, 47);
				log1.SL_GS_NKUser = "EYX";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_SE_NKEvent = Events.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}
			Factory.Save();

			#region ExpectedXml

			var event1 =
				"<Event>" +
				"<Source>Table</Source>" +
				"<Code>ADD</Code>" +
				"<CodeDescription>Added a record to the system</CodeDescription>" +
				"<DateTime>2001-12-17T09:30:47</DateTime>" +
				"<PostedDateTime>" + log.SL_PostedTimeUtc.ToString("s") + "</PostedDateTime>" +
				"<User>LoginName</User>" +
				"<IsEstimatedDate>false</IsEstimatedDate>" +
				"<extra>some additional xml</extra>" +
				"</Event>";

			var event2 =
				"<Event>" +
				"<Source>Table1</Source>" +
				"<Code>EDT</Code>" +
				"<CodeDescription>Edited a record</CodeDescription>" +
				"<DateTime>2003-12-17T09:30:47</DateTime>" +
				"<PostedDateTime>" + log1.SL_PostedTimeUtc.ToString("s") + "</PostedDateTime>" +
				"<User>EYX</User>" +
				"<IsEstimatedDate>false</IsEstimatedDate>" +
				"<extra>some additional xml</extra>" +
				"</Event>";

			#endregion

			EventXml.Add(log);
			EventXml.Add(log1);

			var eventXmlString = EventXml.ToString();
			Assert("Event", eventXmlString.Contains(event1));
			Assert("Event", eventXmlString.Contains(event2));
		}

		public void TestToStringAndADDInvalidEvent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "LoginName";
			staff.GS_FullName = "Test";
			staff.GS_Code = "EYL";

			var log = Factory.New<BaseStmALog>();

			log.SL_Table = "Table";
			log.SL_Parent = ZGuid.NewZGuid();
			log.SL_IsEstimate = false;
			log.SL_Reference = "";
			log.SL_EventTime = new ZDateTime(2001, 12, 17, 9, 30, 47);
			log.SL_GS_NKUser = "EYL";
			log.SL_SE_NKEvent = Events.EditedARecord.Code;
			Factory.Save();
			using (log.GetValidationSuspender())
			{
				log.SL_SE_NKEvent = "XYY";

				EventXml.Add(log);

				AssertNotNullOrEmpty("Expect no exception", EventXml.ToString());
			}
		}

		public void TestToADDWithPayload()
		{
			var log = Factory.New<BaseStmALog>();

			log.SL_Table = "Table AMP: & GT: > LT: <";
			log.SL_Parent = ZGuid.NewZGuid();
			log.SL_IsEstimate = false;
			log.SL_Reference = "";
			log.SL_EventTime = new ZDateTime(2001, 12, 17, 9, 30, 47);
			log.SL_GS_NKUser = "EY";
			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;

			Factory.Save();

			#region ExpectedXml

			var expectedXml =
				"<Events>" +
				"<Event>" +
				"<Source>Table AMP: &amp; GT: &gt; LT: &lt;</Source>" +
				"<Code>ADD</Code>" +
				"<CodeDescription>Added a record to the system</CodeDescription>" +
				"<DateTime>2001-12-17T09:30:47</DateTime>" +
				"<PostedDateTime>" + log.SL_PostedTimeUtc.ToString("s") + "</PostedDateTime>" +
				"<User>EY</User>" +
				"<IsEstimatedDate>false</IsEstimatedDate>" +
				"<Payload><Test><Test1>Test1</Test1></Test></Payload>" +
				"<extra>some additional xml</extra>" +
				"</Event>" +
				"</Events>";

			#endregion

			EventXml.Add(log, "<Test><Test1>Test1</Test1></Test>");

			AssertMultilineASCIIEquals("", expectedXml, EventXml.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EventXml = new EventXmlForTesting();
		}

		EventXmlForTesting EventXml;

		class EventXmlForTesting : EventXml
		{
			protected override ZString GetAdditionalXmlForLog(BaseStmALog log)
			{
				return "<extra>some additional xml</extra>";
			}
		}

		#endregion
	}
}
