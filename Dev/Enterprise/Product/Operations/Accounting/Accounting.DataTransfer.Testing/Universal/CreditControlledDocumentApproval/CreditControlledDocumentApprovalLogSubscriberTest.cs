using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.CreditControlledDocumentApproval;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.CreditControlledDocumentApproval
{
	[TestedType(typeof(CreditControlledDocumentApprovalLogSubscriber))]
	class CreditControlledDocumentApprovalLogSubscriberTest : LogSubscriberTest<CreditControlledDocumentApprovalLogSubscriber>
	{
		public void TestNoBonusFactory()
		{
			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			{
				Factory.NameForDebugging = "Test Factory";
				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.CreateEdiCommunication(GlbCompany.CurrentCompany.OrgProxy, EDICommunicationsMode.Modules.CreditControlledDocumentApproval, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "23434");
				Factory.Save();

				var request = Factory.New<CreditControlledDocumentsApproval>();
				request.Initialize(shipment);
				Factory.Save();
				request.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, false);
				Factory.Save();

				var requestPK = request.PK;
				using (var disposable = PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
				{
					RunLogWalkerCycleForTest();

					var threadOwnerfactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(x => x.ThreadSentry.IsOwner);
					var ediMessageFactory = threadOwnerfactories.FirstOrDefault(f => GetEdiMessges(f, requestPK, true).Any());
					AssertEquals(FormattableString.Invariant($"Thread Owner Factories - {string.Join("\r\n", threadOwnerfactories.Select(x => GetFactoryInfo(x)))} \r\n EDI Message Factories: {GetFactoryInfo(ediMessageFactory)}")
								, 1
								, threadOwnerfactories.Count(f => GetEdiMessges(f, request.PK, true).Any()));
				}

				AssertMessageQueued("Two message for two log", request.PK, 2);
			}

			string GetFactoryInfo(BusinessObjectFactory factory) => factory != null ? FormattableString.Invariant($"{(factory.NameForDebugging.IsNullOrEmpty() ? "<No Factory Name>" : factory.NameForDebugging)}") : "Null";
		}

		public void TestQueueEDIMessages()
		{
			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			{
				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				TestObjectCreator.AddEdiCommunication(GlbCompany.CurrentCompany.OrgProxy, EDICommunicationsMode.Modules.CreditControlledDocumentApproval, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "23434");
				Factory.Save();

				var request1 = Factory.New<CreditControlledDocumentsApproval>();
				request1.Initialize(shipment);
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertMessageQueued("Message should be queued - request in REQ status", request1.PK, 1);

				var request2 = Factory.New<CreditControlledDocumentsApproval>();
				request2.Initialize(shipment);
				request2.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertNoMessageQueued("Message should not be queued after request is approved", request2.PK);

				var request3 = Factory.New<CreditControlledDocumentsApproval>();
				request3.Initialize(shipment);
				Factory.Save();
				request3.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, false);
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertMessageQueued("2 Messages should be queued for request3, one for REQ and another for CAN", request3.PK, 2);
			}
		}

		void AssertMessageQueued(string message, ZGuid requestPK, int numberOfMessages)
		{
			AssertEquals(message, numberOfMessages, GetEdiMessges(Factory, requestPK).Length);
		}

		void AssertNoMessageQueued(string message, ZGuid requestPK)
		{
			AssertEquals(message, 0, GetEdiMessges(Factory, requestPK).Length);
		}

		EDIMessage[] GetEdiMessges(BusinessObjectFactory factory, ZGuid requestPK, bool isFromLocalCacheOnly = false)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageSubType, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment) { FetchOnlyFromLocalCache = isFromLocalCacheOnly };
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, requestPK.ToGuid());
			return factory.Load<EDIMessage>(query);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
