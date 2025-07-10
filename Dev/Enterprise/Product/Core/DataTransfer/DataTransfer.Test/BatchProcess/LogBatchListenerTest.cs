using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	sealed class LogBatchListenerTest : TestCaseWithFactory
	{
		public void TestFoundObjects()
		{
			var listener = new LogBatchListenerTestClass();
			listener.Match(OrganisationAddLog, Notify);

			AssertEquals("Process was called", true, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, true, Notify.AsString.IndexOf(FoundMessage) >= 0);
		}

		public void TestLogWithInvalidFK()
		{
			var logEntry = Factory.New<StmALog>();
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			using (logEntry.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry.SL_Table = parent.TableName;
				logEntry.SL_Parent = parent.PK;
			}
			Factory.Save();
			parent.Delete();
			Factory.Save();
			var listener = new LogBatchListenerTestClass();
			listener.Match(logEntry, Notify);

			var notFoundMessage = string.Format(CultureInfo.InvariantCulture, "{0} is missing record with PK '{1}'", logEntry.SL_Table, logEntry.SL_Parent);
			AssertEquals("Process was not called", false, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, true, Notify.AsString.IndexOf(notFoundMessage) >= 0);
		}

		public void TestAdditionalMatching()
		{
			var mockListener = new Mock<LogBatchListenerTestClass>();
			mockListener.CallBase = true;
			mockListener.Protected()
				.Setup<bool>("AdditionalMatching", OrganisationAddLog)
				.Returns(false);

			var listener = mockListener.Object;
			listener.Match(OrganisationAddLog, Notify);
			AssertEquals("Process was not called", false, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, false, Notify.AsString.IndexOf(NotFoundMessage) >= 0);
			mockListener.VerifyAll();

			mockListener.Protected()
				.Setup<bool>("AdditionalMatching", OrganisationAddLog)
				.Returns(true);

			Notify = new NotificationBuffer();
			listener.Match(OrganisationAddLog, Notify);
			AssertEquals("Process was called", true, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, true, Notify.AsString.IndexOf(FoundMessage) >= 0);
			mockListener.VerifyAll();
		}

		public void TestMatchTableName()
		{
			var mockListener = new Mock<LogBatchListenerTestClass>();
			mockListener.CallBase = true;

			mockListener.Protected()
				.Setup<bool>("MatchTableName", OrganisationAddLog)
				.Returns(true);

			var listener = mockListener.Object;

			listener.Match(OrganisationAddLog, Notify);
			AssertEquals("Process was called", true, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, true, Notify.AsString.IndexOf(FoundMessage) >= 0);
			mockListener.VerifyAll();
		}

		public void TestLoadBusinessObject()
		{
			var mockListener = new Mock<LogBatchListenerTestClass>();
			mockListener.CallBase = true;

			mockListener.Protected()
				.Setup<BusinessObject>("LoadBusinessObject", OrganisationAddLog)
				.Returns(Organisation);

			var listener = mockListener.Object;
			listener.Match(OrganisationAddLog, Notify);
			AssertEquals("Process was called", true, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, true, Notify.AsString.IndexOf(FoundMessage) >= 0);
			mockListener.VerifyAll();
		}

		public void TestLoadJobDeclarationIsLoadedInCorrectCountry()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Guid departmentGuid = GlbDepartment.CurrentDepartment.PK.ToGuid();

			JobDecBatchListenerTestClass listener = new JobDecBatchListenerTestClass();
			listener.Factory = Factory;

			BusinessObject bizO = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			StmALog log = bizO.GetLogs().AddNew();

			listener.Match(log, Notify);
			Assert("Process should be called", listener.ProcessWasCalled);

			GlbBranch aEBranch = CreateAEBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, aEBranch.PK.ToGuid(), departmentGuid))
			{
				bizO = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AE.IJobDeclaration>();
				log = bizO.GetLogs().AddNew();
				Factory.Save();
			}

			listener.ProcessWasCalled = false;
			listener.Match(log, Notify);
			Assert("Process should not be called, JobDec should be loaded because it was created in a different country", !listener.ProcessWasCalled);
		}

		public void TestTableNameDefinedDoesNotMatchOnBusinessObject()
		{
			var mockListener = new Mock<LogBatchListenerTestClass>();
			mockListener.CallBase = true;
			mockListener.Setup(m => m.BusinessObjectTableName)
				.Returns("SomeInvalidTableName");
			var listener = mockListener.Object;

			listener.Match(OrganisationAddLog, Notify);
			AssertEquals("Process was not called", false, listener.ProcessWasCalled);
			AssertEquals("Notify: " + System.Environment.NewLine + Notify.AsString, false, Notify.AsString.IndexOf(NotFoundMessage) >= 0);
			mockListener.VerifyAll();
		}

		public void TestSendEmailToNotificationGroup()
		{
			LogBatchListenerTestClass listener = new LogBatchListenerTestClass();

			GlbGroup notifyGroup = Factory.New<GlbGroup>();
			notifyGroup.GG_Code = "test";
			GlbStaff staff = notifyGroup.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "test@test.com";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			NotificationBuffer notification = new NotificationBuffer(Notify);
			listener.Factory = Factory;
			listener.SendEmailToNotificationGroup(notification, notifyGroup.PK, "Test");
			AssertEquals("No email should have been sent because the Loaded Notification Group should have been null", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			listener.SendEmailToNotificationGroup(notification, notifyGroup.PK, "Test");
			AssertEquals("1 Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation

		GlbBranch CreateAEBranch()
		{
			GlbCompany aECompany = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			aECompany.GC_Code = "AEC";
			aECompany.GC_City = "Dubai";
			aECompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
			aECompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;

			GlbBranch aEBranch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			aEBranch.GB_BranchName = "Dubai Branch";
			aEBranch.GB_Code = "DXB";
			aEBranch.GB_RL_NKHomePort = "AEDXB";
			aEBranch.GB_GC = aECompany.PK;

			Factory.Save();

			return aEBranch;
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Notify = new BatchProcessorNotificationBufferBridge(new LoggingInformation());

			Organisation = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, Organisation.PK);
			filter.AddToFilter(StmALogSchema.SL_Table, OrgHeaderSchema.Constants.TableName);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			OrganisationAddLog = Factory.LoadTop1<StmALog>(filter);
			Factory.Save();

			FoundMessage = string.Format("LogBatchListenerTestClass: Found Event {0} for {1} posted at {2}", OrganisationAddLog.SL_SE_NKEvent, OrganisationAddLog.SL_Table, OrganisationAddLog.SL_PostedTimeUtc.ToISO8601String());
			NotFoundMessage = string.Format("{0} is missing record with PK '{1}'", OrganisationAddLog.SL_Table, OrganisationAddLog.SL_Parent.ToString());
		}

		NotificationBuffer Notify;
		OrgHeader Organisation;
		StmALog OrganisationAddLog;
		string FoundMessage;
		string NotFoundMessage;

		#endregion
	}
}
