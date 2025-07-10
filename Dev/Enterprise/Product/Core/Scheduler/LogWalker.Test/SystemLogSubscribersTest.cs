using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	sealed class SystemLogSubscribersTest : TestCase
	{
		public void TestEnumerator()
		{
			SystemLogSubscribers subscribers = new SystemLogSubscribers();
			List<LogSubscriber> subscribersList = new List<LogSubscriber>(subscribers);
			List<string> subscriberNames = subscribersList.ConvertAll<string>((l) => l.Name);
			AssertEquals("Add your LogSubscriber to the ExpectedSystemSubscribersOrderedByPriority property below", ExpectedSystemSubscribersOrderedByPriority.Length, subscriberNames.Count);
			AssertEquals("Check the priority ordering", string.Join("\\n", ExpectedSystemSubscribersOrderedByPriority), string.Join("\\n", subscriberNames.ToArray()));
		}

		string[] ExpectedSystemSubscribersOrderedByPriority
		{
			get
			{
				return new string[]
				{
					"ConsolTrackingUpdater",
					"CADeclarationSubscriptionUpdater",
					"VoyOriginSubscriptionUpdater",
					"VoyDestinationSubscriptionUpdater",
					"FlightJobSailingSubscriptionUpdater",
					"FlightConsolSubscriptionUpdater",
					"DeclarationSubscriptionUpdater",
					"WorkflowEventTrigger",
					"ShipmentCargoReporterWorkflow",
					"UnapprovedTransactionsConverter",
					"TransactionNettingTransmitter",
					"MatchTransactionNettingTransmitter",
					"eNettTransactionSender",
					"CMRCARSTMessage",
					"AuQrpDel",
					"OrderVoyageEventsCopier",
					"JobVoyageEventLogSynchronizer",
					"InvoicePDFExport",
					"DSMLogSubscriber",
					"CVDEventRaiser",
					"B3LogSubscriber",
					"TasksAndMilestonesLoader",
					"WorkflowEventPublish",
					"HVLVItemStatusLogSubscriber",
					"HVLVReadyLogSubscriber",
					"HVLVDeclarationCSHLogSubscriber",
					"GlbCompanyDataImportLogSubscriber",
					"TMCEventLogSubscriber",
					"JobConversationMessageLogSubscriber",
					"PatternMatchingSubscriber",
					"ARCreditNoteApprovalSubscriber",
					"CreditApprovallogSubscriber",
					"DeclarationLockLogSubscriber",
					"NctsDeclarationLockLogSubscriber",
					"CustomsStatusLogSubscriber",
					"RejectionEmailLogSubscriber",
					"AgencyBookingUpdatedLogSubscriber",
					"ShipmentStatusUpdatedLogSubscriber",
					"USLVSToDeclarationLogSubscriber",
					"UpdateRelatedJobSubscriber",
					"ContainerLoadListShipmentCreation",
					"AccTriggerWithOptSaveLogSubscriber",
					"EmploymentChangeLogSubscriber",
					"CancelChangeRequestLogSubscriber",
					"RecordAuditedLogSubscriber",
					"OpportunityCalendarLogSubscriber",
					"SupplierBookingShipmentCreator",
					"CNActionPUPDLVLogSubscriber",
					"PlGlbStaffSubscriber",
					"PlGlbExtPasswSubscriber",
					"ShipmentEventLogSubscriber",
					"ForwardingConsolToTWHLogSubscriber"
				};
			}
		}

		public void TestEnumerator_DoesNotIncludeNonRequiredOnes()
		{
			List<LogSubscriber> dummyList = new List<LogSubscriber>();
			dummyList.Add(new DummyLogSubscriber("Included", true));
			dummyList.Add(new DummyLogSubscriber("NotIncluded", false));

			using (ObjectFactory.Substitute("SystemLogSubscribers", dummyList))
			{
				SystemLogSubscribers subscribers = new SystemLogSubscribers();
				List<LogSubscriber> subscribersList = new List<LogSubscriber>(subscribers);
				AssertEquals(1, subscribersList.Count);
				AssertEquals("Included", subscribersList[0].Name);
			}
		}

		[Serializable]
		class DummyLogSubscriber : LogSubscriber
		{
			public DummyLogSubscriber(string name, bool isRequired)
			{
				this.name = name;
				this.isRequired = isRequired;
			}

			public override string Name
			{
				get { return name; }
			}

			public override bool IsRequired
			{
				get { return isRequired; }
			}

			readonly string name;
			readonly bool isRequired;

			public override string[] EventTypes
			{
				get { throw new System.NotImplementedException(); }
			}

			public override string[] TableNames
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
