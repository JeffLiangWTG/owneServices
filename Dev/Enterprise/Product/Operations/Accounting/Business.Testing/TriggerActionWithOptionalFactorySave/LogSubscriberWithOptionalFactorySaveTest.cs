using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	public class LogSubscriberWithOptionalFactorySaveTest : TestCaseWithFactory
	{
		public void TestSuccessProcessing()
		{
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			AssertProcessing(true, true);
		}

		public void TestTryHanddleExceptionWithSaveAsSuccessfulInNewFactoryResult()
		{
			Dummy.Logs.AddNew(Events.CustomisableEvent11);
			Factory.Save();

			AssertProcessing(false, true);
		}

		public void TestTryHanddleExceptionWithUnhandledResult()
		{
			Dummy.Logs.AddNew(Events.CustomisableEvent22);
			Factory.Save();

			AssertProcessing(false, false);
		}

		void AssertProcessing(bool isSuccessfulSaving, bool isSuccessfulProcessing)
		{
			var subscriber = new DummyLogSubscriberWithOptionalFactorySave();
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });
			var queueLogs = Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, Dummy.PK));
			AssertEquals("Precondition: Number of queued logs created", 1, queueLogs.Length);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Queued log status", JobQueueStatus.StatusQueued, queueLogs[0].SJ_Status);
				AssertEquals("Precondition: Queued log filter name", nameof(DummyLogSubscriberWithOptionalFactorySave), ((BusinessObject)queueLogs[0])[StmJobQueueSchema.SJ_FilterName.Name]);
			});

			var logger = new LoggerForTest();
			var transmitter = new MockNewsTransmitter(subscriber, new[] { subscriber }, new SubscriberParameters() { Logger = logger });
			transmitter.ProcessLogs(queueLogs);

			var newTestFactory = new BusinessObjectFactory();
			queueLogs = newTestFactory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, Dummy.PK));
			AssertEquals("Number of queued logs loaded", 1, queueLogs.Length);
			CombineAssertions(() =>
			{
				if (isSuccessfulProcessing)
				{
					AssertEquals("Queued log status", JobQueueStatus.StatusProcessed, queueLogs[0].SJ_Status);
					AssertEquals("LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				}
				else
				{
					AssertEquals("Queued log status", JobQueueStatus.StatusFailed, queueLogs[0].SJ_Status);
					AssertNotEquals("LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});

			var dummyObjectsSaved = newTestFactory.Load<DummyWithWorkflow>(new ZQuery());
			AssertEquals("Number of dummyObjectsSaved", isSuccessfulSaving ? 2 : 1, dummyObjectsSaved.Length);
			AssertEquals("Is main object saved", 1, dummyObjectsSaved.Where(x => x.Z0_Description == MainObjectDescription).Count());
			if (isSuccessfulSaving)
			{
				AssertEquals("Is optional object saved", 1, dummyObjectsSaved.Where(x => x.Z0_Description == OptionalObjectDescription).Count());
			}
			if (isSuccessfulProcessing)
			{
				AssertEquals("Emails created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));

			Dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Dummy.Z0_Description = MainObjectDescription;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);
		}

		DummyWithWorkflow Dummy;
		public const string MainObjectDescription = "Main object";
		public const string OptionalObjectDescription = "Optional object";

		[Serializable]
		public class DummyLogSubscriberWithOptionalFactorySave : LogSubscriberWithOptionalFactorySave
		{
			public override string Name => nameof(DummyLogSubscriberWithOptionalFactorySave);

			public override string[] EventTypes => new[] { Events.CustomisableEvent00Code, Events.CustomisableEvent11Code, Events.CustomisableEvent22Code };

			public override string[] TableNames => new string[] { DummyBizoSchema.Constants.TableName };

			protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
			{
				var factory = queuedLogs[0].Factory;
				var bizo = factory.NewWithValidTestData<DummyWithWorkflow>();
				bizo.Z0_Description = OptionalObjectDescription;

				var accountingEmailDef1 = new AccountingEmailDef_ForTest();
				var accountingEmailDef2 = new AccountingEmailDef_ForTest();

				if (queuedLogs[0].SJ_SE_NKEvent == Events.CustomisableEvent11Code)
				{
					throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("My exception", accountingEmailDef1, accountingEmailDef2);
				}
				else if (queuedLogs[0].SJ_SE_NKEvent == Events.CustomisableEvent22Code)
				{
					throw new NullReferenceException();
				}
				else
				{
					accountingEmailDef1.Create(factory);
					accountingEmailDef2.Create(factory);
				}
			}
		}
	}
}
