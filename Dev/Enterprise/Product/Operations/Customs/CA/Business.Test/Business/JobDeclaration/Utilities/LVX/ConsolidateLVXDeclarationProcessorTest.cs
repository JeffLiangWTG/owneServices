using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateLVXDeclarationProcessorTest : TestCaseWithFactory
	{
		sealed class FactorySaveAlerterForTest : Disposable, ITransactionParticipantListener
		{
			public FactorySaveAlerterForTest(string context)
			{
				BusinessObjectFactory.RegisterListener(this);
				this.context = context;
			}

			readonly string context;

			void ITransactionParticipantListener.FactorySaveBeginning(ITransactionParticipant[] factories)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Save in [{context}]"), FormattableString.Invariant($@"It is not valid to call Factory.Save in {context}.
It is never valid or safe to call Factory.Save within a LogSubscriber of the LWK service task. (And it never has been)
There reason for this is that we cannot guarantee that the same StmJobQueue will not be processed multiple times if something goes wrong after this save but before the NewsTransmitter calls save.
In order to fix this issue you should be deleting the instance of the save being reported here."));
			}

			void ITransactionParticipantListener.FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
			{
				// Don't care.
			}

			protected override void Dispose(bool isDisposing) => BusinessObjectFactory.UnRegisterListener(this);
		}

		[Serializable]
		class LogSubscriberForTest : LogSubscriber
		{
			public override string Name => ProcessTask.WorkflowEventTriggerJobQueueName;

			public override string[] EventTypes => new string[] { Events.WorkflowTriggerEvent.Code };

			public override string[] TableNames => new[] { ProcessTasksSchema.Constants.TableName, ProcessJobTriggerLinkSchema.Constants.TableName };

			protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
			{
				foreach (var queuedLog in queuedLogs)
				{
					var declaration = queuedLog.Factory.Load<JobDeclaration>(queuedLog.SJ_ParentID);
					if (declaration != null)
					{
						var processor = new ConsolidateLVXDeclarationProcessor(declaration);
						processor.Process(new Notifications());
					}
				}
			}

			public void Process(IQueuedLog[] queuedLogs)
			{
				ProcessLogQueueItems(queuedLogs);
			}

			public override bool EnableFactorySaveAlerterInTesting => true;
		}

		public void TestProcessThroughWorkFlow()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_TransportMode = "ROA";
			Factory.Save();

			var log = Factory.New<IQueuedLog>();
			((BusinessObject)log)[StmJobQueueSchema.SJ_ParentID] = declaration.PK;

			var processor = new LogSubscriberForTest();
			using (new FactorySaveAlerterForTest("Log Walker"))
			{
				AssertNoExceptionThrown(() =>
				{
					processor.Process(new IQueuedLog[] { log });
				});
			}
		}

		public void TestProcess()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "Importer1";
				var lvx = Factory.New<JobDeclaration>();
				lvx.JE_DeclarationReference = "B000000X0";
				lvx.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				lvx.JE_EntryAuthorisationDate = new ZDateTime(2016, 1, 1);
				lvx.JE_OH_Importer = importer.PK;
				var invoice = lvx.LVXInvoiceHeader;
				Factory.Save();

				invoice.CA_ReadyForConsolidation = true;
				var lvs = Factory.New<JobDeclaration>();
				lvs.JE_DeclarationReference = "B000000S0";
				lvs.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				lvs.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				lvs.JE_EntryAuthorisationDate = new ZDateTime(2016, 1, 1);
				lvs.JE_OH_Importer = importer.PK;
				Factory.Save();
				var processor = new ConsolidateLVXDeclarationProcessor(lvx);
				processor.Process(new Notifications());
				AssertNull("Did not consolidate", lvx.LVXInvoiceHeader.FirstAdditionalDeclaration);
				lvx.ActiveEntryHeaders.AddNew();
				Factory.Save();
				processor.Process(new Notifications());
				Assert(!invoice.JZ_JE.IsEmpty);
				Assert(invoice.JobDeclaration.IsPersistent);
				Assert(invoice.IsAttachedToPersistentDeclaration);
				Assert(invoice.JobDeclaration.IsLVX);
				Assert(invoice.IsAttachedToPersistentLVXDeclaration);
				Assert(invoice.SupportAdditionalDeclarations);
				AssertNotNull(lvx.LVXInvoiceHeader.FirstAdditionalDeclaration);
				AssertEquals($"{lvx.HumanReadableName} should be attached to {lvs.HumanReadableName}", lvs.PK, lvx.LVXInvoiceHeader.FirstAdditionalDeclaration.PK);
			}
		}
	}
}
