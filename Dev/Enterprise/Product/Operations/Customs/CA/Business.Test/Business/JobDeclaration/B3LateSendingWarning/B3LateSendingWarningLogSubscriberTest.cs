using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3LateSendingWarningLogSubscriber))]
	sealed class B3LateSendingWarningLogSubscriberTest : LogSubscriberTest<B3LateSendingWarningLogSubscriber>
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		[TestDate(2015, 1, 1)]
		public void TestB3LateSendingWarningLogSubscriber_SendWarningEmail()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T";
			staff.GS_LoginName = "T";
			staff.GS_EmailAddress = "t@wisetechglobal.com";

			var declaration = JobDeclarationTest.CreateCONJobDeclarationWithB3LateSendingWarningEvent(Factory);
			declaration.JE_SystemCreateUser = staff.GS_Code;
			declaration.JE_EntryAuthorisationDate = new DateTime(2015, 1, 1);
			Factory.Save();

			var log = declaration.Logs.MostRecentLogByEventTime(Events.CanadianCADLateSendingWarning);
			Assert("Precondition: B3LateSendingWarning event has been added", !log.IsCancelled);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TestDateAttribute.Date = new DateTime(2015, 2, 3); // Go into the future. (Note: IsDelayedFired on the CB3 event makes this necessary)
			AssertNoExceptionThrown(() => RunLogWalkerCycleForTest());
			Assert(NotifiedEventList.Contains(string.Format("[Canadian CAD Late Sending Warning] CAD Late Sending Warning, scheduled by ~BP at {0} UTC", log.SL_EventTimeUtc.ToString("dd-MMM-yyyy HH:mm"))));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CAD Late Sending Warning");
			AssertNotNull("CAD Late Sending Warning should be created", email);

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CanadianCADLateSendingWarning.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			query.AddToFilter(StmALogSchema.SL_EventTime, log.SL_EventTime);
			Assert("B3LateSendingWarning event should be cancelled", newFactory.LoadTop1<StmALog>(query).IsCancelled);
		}

		[TestDate(2015, 12, 12)]
		public void TestB3LateSendingWarningLogSubscriber_HadAB3Accepted()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T";
			staff.GS_LoginName = "T";
			staff.GS_EmailAddress = "t@wisetechglobal.com";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_SystemCreateUser = staff.GS_Code;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			Factory.Save();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CanadianCADLateSendingWarning.Code;
				log.SL_Parent = declaration.PK;
				log.SL_Table = "JobDeclaration";
				log.SL_EventTime = new DateTime(2015, 2, 11);
				log.SL_GS_NKUser = User.ServiceUserCode;
				log.SL_Reference = B3LateSendingWarningLogSubscriber.B3WarningComment;
				Factory.Save();
			}

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains(string.Format("[Canadian CAD Late Sending Warning] CAD Late Sending Warning, scheduled by ~BP at {0} UTC", log.SL_EventTimeUtc.ToString("dd-MMM-yyyy HH:mm"))));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CAD Late Sending Warning");
			AssertNull("CAD Late Sending Warning should not be created", email);
			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CanadianCADLateSendingWarning.Code);
			query.AddToFilter(StmALogSchema.SL_EventTime, log.SL_EventTime);
			Assert("B3LateSendingWarning event should be cancelled", newFactory.LoadTop1<StmALog>(query).IsCancelled);
		}
	}
}
