using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRMessageManagerAbstractTest : TestCaseWithFactory
	{
		public void TestGetCommonNotificationsForSendingHasErrorForMissingRegistrationNumber()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12345";
			AssertErrorCountReduced();
		}

		public void TestNotificationsForMissingEncryptionCertificates()
		{
			var certificatesHelper = ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory);
			certificatesHelper.CreateCustomsCertificates();
			Factory.Save();

			var errors = Manager.GetNotificationsForSendingAnOriginal();
			AssertEquals($"Notification Count to Reduce when certs added to DB.  The Errors reported were:\r\n{errors.ErrorNotificationsAsString()}", 2, preErrorCount - errors.ErrorCount);
		}

		public void TestNotificationsForMissingPrivateKey()
		{
			Env.Registry.AUCCompanyCertificatePassword = "password";
			Env.Registry.AUCCompanyCertificateData = new byte[] { 1, 2, 3 };
			AssertErrorCountReduced();
		}

		public void TestNotificationForServiceTaskIsNotActive()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1111111111");
			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.S5_IsActive = false;
			serviceTask.S5_ScheduleType = "AUS";
			serviceTask.S5_TypeOfDocument = "AUC";
			var serviceTask1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask1.S5_IsActive = false;
			serviceTask1.S5_ScheduleType = "AUP";
			serviceTask1.S5_TypeOfDocument = "AUC";
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.EnableAUServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var notifications = Manager.GetNotificationsForSendingAnOriginal();
				var warnings = string.Join("\r\n", notifications.Where(x => x.IsWarning).Select(x => x.Message));
				AssertNotContains("Please have your Administrator check the tasks with these codes. AUS: ServiceTaskIsInactive", warnings);
			}

			using (AUCustomsDataRegistry.Instance.EnableAUServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = Manager.GetNotificationsForSendingAnOriginal();
				var warnings = string.Join("\r\n", notifications.Where(x => x.IsWarning).Select(x => x.Message));
				AssertContains("Please have your Administrator check the tasks with these codes. AUS: ServiceTaskIsInactive", warnings);

				serviceTask.S5_IsActive = true;
				Factory.Save();

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(m => m.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					notifications = Manager.GetNotificationsForSendingAnOriginal();
					warnings = string.Join("\r\n", notifications.Where(x => x.IsWarning).Select(x => x.Message));
					AssertNotContains("Please have your Administrator check the tasks with these codes. AUS: ServiceTaskIsInactive", warnings);
				}
			}
		}

		public void TestResetToOriginalCancelsEvents()
		{
			var house = Factory.New<CusSCAHouse>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Logs.AddNew(((IParentForCargoReporter)shipment).CargoReportSentEvent);
			shipment.Logs.AddNew(((IParentForCargoReporter)shipment).CargoReportAcceptedEvent);
			shipment.Logs.AddNew(((IParentForCargoReporter)shipment).CargoReportRejectedEvent);
			shipment.Logs.AddNew(((IParentForCargoReporter)shipment).CargoReportWithdrawEvent);
			shipment.Logs.AddNew(Events.Attached);
			var collection = new StmALogDependentCollection(shipment);
			collection.Load(new ZQuery(StmALogSchema.SL_IsCancelled, "N"));
			Assert("sent event", CollectionContainsEvent(collection, ((IParentForCargoReporter)shipment).CargoReportSentEvent));
			Assert("accepted event", CollectionContainsEvent(collection, ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent));
			Assert("rejected event", CollectionContainsEvent(collection, ((IParentForCargoReporter)shipment).CargoReportRejectedEvent));
			Assert("withdraw event", CollectionContainsEvent(collection, ((IParentForCargoReporter)shipment).CargoReportWithdrawEvent));
			Assert("other event", CollectionContainsEvent(collection, Events.Attached));
			house.CA_JS = shipment.PK;
			var messageManager = new CusSCAHouseSEACRManager(house);
			messageManager.ResetToOriginal();
			var collection2 = new StmALogDependentCollection(shipment);
			collection2.Load(new ZQuery(StmALogSchema.SL_IsCancelled, "N"));
			Assert("no sent event", !CollectionContainsEvent(collection2, ((IParentForCargoReporter)shipment).CargoReportSentEvent));
			Assert("no accepted event", !CollectionContainsEvent(collection2, ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent));
			Assert("no rejected event", !CollectionContainsEvent(collection2, ((IParentForCargoReporter)shipment).CargoReportRejectedEvent));
			Assert("no withdraw event", !CollectionContainsEvent(collection2, ((IParentForCargoReporter)shipment).CargoReportWithdrawEvent));
			Assert("other event", CollectionContainsEvent(collection2, Events.Attached));
			Assert("cancelled event", CollectionContainsEvent(collection2, Events.Cancelled));
		}

		public void TestNotificationsWhenWaitingForResponse()
		{
			AssertNotificationsWhenWaitingForResponse(CMRBaseStatuses.Codes.AwaitingResponseToAmendment, true);
			AssertNotificationsWhenWaitingForResponse(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, true);
			AssertNotificationsWhenWaitingForResponse(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, true);
			AssertNotificationsWhenWaitingForResponse(CMRBaseStatuses.Codes.NotSent, false);
		}

		public void TestCanSendOriginal()
		{
			foreach (string statusWeCanSendOriginalFrom in StatusesWeCanSendOriginalFrom)
			{
				AssertCanSendOriginal(statusWeCanSendOriginalFrom, true);
			}
			foreach (string statusWeCantSendOriginalFrom in StatusesWeCantSendOriginalFrom)
			{
				AssertCanSendOriginal(statusWeCantSendOriginalFrom, false);
			}
		}

		public void TestCanSendWithdrawal()
		{
			foreach (string statusWeCanSendWithdrawalFrom in StatusesWeCanSendWithdrawalFrom)
			{
				AssertCanSendWithdrawal(statusWeCanSendWithdrawalFrom, true);
			}
			foreach (string statusWeCantSendWithdrawalFrom in StatusesWeCantSendWithdrawalFrom)
			{
				AssertCanSendWithdrawal(statusWeCantSendWithdrawalFrom, false);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestSaveOriginalMessage()
		{
			GenerateOriginalMessageForSaving();
			Factory.Save();
		}

		protected virtual void GenerateOriginalMessageForSaving()
		{
			Manager.GenerateOriginalMessages(Manager.BusinessObject);
		}

		protected abstract void SetStatus(ZString status);

		void AssertCanSendOriginal(string status, bool canSendOriginal)
		{
			SetStatus(status);
			AssertEquals("CanSendOriginal from status " + status, canSendOriginal, Manager.CanSendOriginal);
		}

		void AssertCanSendWithdrawal(string status, bool canSendWithdrawal)
		{
			SetStatus(status);
			AssertEquals("CanSendWithdrawal from status " + status, canSendWithdrawal, Manager.CanSendWithdrawal);
		}

		bool CollectionContainsEvent(StmALogDependentCollection logCollection, Event cargoEvent)
		{
			bool result = false;
			foreach (StmALog aLog in logCollection)
			{
				if (aLog.SL_SE_NKEvent == cargoEvent.Code)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		void AssertNotificationsWhenWaitingForResponse(ZString status, bool expectError)
		{
			SetStatus(status);
			AssertEquals("Awaiting for Response Error", expectError, Manager.GetNotificationsForSendingAnOriginal().ContainsError(SingleMessageManager.PendingMessagesErrorMessage));
		}

		void AssertErrorCountReduced()
		{
			var postErrorCount = Manager.GetNotificationsForSendingAnOriginal().ErrorCount;
			AssertEquals("NotificationCountReduced", true, preErrorCount > postErrorCount);
		}

		string[] StatusesWeCanSendOriginalFrom
		{
			get
			{
				return new string[]
				{
					CMRBaseStatuses.Codes.NotSent,
					CMRBaseStatuses.Codes.OriginalRejected,
					CMRBaseStatuses.Codes.WithdrawalAccepted
				};
			}
		}

		string[] StatusesWeCantSendOriginalFrom
		{
			get
			{
				return new string[]
				{
					CMRBaseStatuses.Codes.AmendmentAccepted,
					CMRBaseStatuses.Codes.AmendmentRejected,
					CMRBaseStatuses.Codes.AwaitingResponseToAmendment,
					CMRBaseStatuses.Codes.AwaitingResponseToOriginal,
					CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal,
					CMRBaseStatuses.Codes.OriginalAccepted,
					CMRBaseStatuses.Codes.WithdrawalRejected
				};
			}
		}

		string[] StatusesWeCanSendWithdrawalFrom
		{
			get
			{
				return new string[]
				{
					CMRBaseStatuses.Codes.AmendmentAccepted,
					CMRBaseStatuses.Codes.AmendmentRejected,
					CMRBaseStatuses.Codes.OriginalAccepted,
					CMRBaseStatuses.Codes.WithdrawalRejected
				};
			}
		}

		string[] StatusesWeCantSendWithdrawalFrom
		{
			get
			{
				return new string[]
				{
					CMRBaseStatuses.Codes.AwaitingResponseToAmendment,
					CMRBaseStatuses.Codes.AwaitingResponseToOriginal,
					CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal,
					CMRBaseStatuses.Codes.NotSent,
					CMRBaseStatuses.Codes.OriginalRejected,
					CMRBaseStatuses.Codes.WithdrawalAccepted
				};
			}
		}

		protected abstract CMRMessageManager GetManager();

		CMRMessageManager manger;
		protected CMRMessageManager Manager => manger ?? (manger = GetManager());

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			Env.Registry.AUCCompanyCertificateData = Array.Empty<byte>();
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			var collection = Manager.GetNotificationsForSendingAnOriginal();
			preErrorCount = collection.ErrorCount;
		}

		int preErrorCount;
	}
}
