using System;
using System.Linq;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(MexicoGlobalXUEFunctionalityProvider))]
	class MexicoGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new MexicoEInvoicingObjectFactory();

		const int DEFAULT_CONFIGURATION_REMAINING_TIMBRES = 100;
		const int DEFAULT_CONFIGURATION_INTERVAL = 10;
		const int RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE = 77;

		public void TestAfterEventMessageProcessed_LogAWarningWhenUniversalEventRemainingStamps_WasNotParsed()
		{
			CreateStaffInAllGroup();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = 33, Interval = 10 };
			var expectedWarningMessage = "Remaining Stamps from ContextCollection could not be parsed, so no notification mail should be sent.";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("Precondition", 0, Logger.Logs.Count());

			var emailCreated = SetupAndExecuteAfterEventMessageProcessed(expectedRemainingTimbres: "ABC", xueFunctionalityProvider, remainingTimbresConfig, Logger);

			AssertEquals("Postcondition", 1, Logger.Logs.Count());
			Assert("Should have logged the warning", Logger.Logs.Select(l => l.Message).Contains(expectedWarningMessage));
			AssertEquals("No emails created", 0, emailCreated);
		}

		public void TestAfterEventMessageProcessedDoNotSendEmail_BecauseRemainingTimbresIsZeroButModFromConfigIsNotZero()
		{
			CreateStaffInAllGroup();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = 33, Interval = 10 };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emailCreated = SetupAndExecuteAfterEventMessageProcessed(expectedRemainingTimbres: "0", xueFunctionalityProvider, remainingTimbresConfig);
			AssertEquals("No emails created", 0, emailCreated);
		}

		public void TestAfterEventMessageProcessedSendEmail_InEveryCountdownStepFromTenToZero()
		{
			CreateStaffInAllGroup();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = 10, Interval = 1 };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			foreach (var emailsToCreate in Enumerable.Range(10, 1).Reverse().Select((v, i) => new { Value = v.ToString(), Index = i + 1 }).ToList())
			{
				var emailCreated = SetupAndExecuteAfterEventMessageProcessed(expectedRemainingTimbres: emailsToCreate.Value, xueFunctionalityProvider, remainingTimbresConfig);
				AssertEquals($"{emailsToCreate.Index} email created", emailsToCreate.Index, emailCreated);
			}
		}

		public void TestAfterEventMessageProcessedSendEmail_Because_MexicoRemainingFolioNotificationDefaultValueIsReachedAlthoughItIsNotOverridden()
		{
			CreateStaffInAllGroup();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: DEFAULT_CONFIGURATION_REMAINING_TIMBRES.ToString());

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(InvoiceBatch.AIB_GC.ToGuid(), Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}
		}

		public void TestAfterEventMessageProcessedSendEmailWithCorrectData()
		{
			var expectedReceiptEmail = "mexico@cargowiseone";
			var expectedRemainingTimbres = RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE;
			var companyPk = InvoiceBatch.AIB_GC.ToGuid();

			CreateStaffInAllGroup(expectedReceiptEmail);

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: expectedRemainingTimbres.ToString());
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = expectedRemainingTimbres, Interval = DEFAULT_CONFIGURATION_INTERVAL };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
				Assert(emailsCreatedList.Single().Recipients.Contains(expectedReceiptEmail));
			}
		}

		public void TestAfterEventMessageProcessedWillNotSendEmail_Because_RemainingStampsDoesNotMatchWithEINV_MX_RemainingStampsNode()
		{
			CreateStaffInAllGroup();
			var expectedRemainingTimbres = RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE;
			var companyPk = InvoiceBatch.AIB_GC.ToGuid();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: expectedRemainingTimbres.ToString());
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = expectedRemainingTimbres, Interval = DEFAULT_CONFIGURATION_INTERVAL };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = 150, Interval = 49 }))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}
		}

		public void TestAfterEventMessageProcessedWillNotSendEmail_Because_UniversalEventsDoesNotContainEINV_MX_RemainingStampsElement()
		{
			CreateStaffInAllGroup();
			var expectedRemainingTimbres = RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE;
			var companyPk = InvoiceBatch.AIB_GC.ToGuid();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: expectedRemainingTimbres.ToString());
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE, Interval = DEFAULT_CONFIGURATION_INTERVAL };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}

			universalEvent = GetUniversalEvent(includeRemainingTimbresContext: false);
			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}
		}

		public void TestAfterEventMessageProcessedWillNotSendEmail_Because_MexicoRemainingFolioNotificationIsNotOverriden()
		{
			CreateStaffInAllGroup();
			var expectedRemainingTimbres = RANDOM_VALUE_NOTEQUAL_TODEFAULTVALUE;
			var companyPk = InvoiceBatch.AIB_GC.ToGuid();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: expectedRemainingTimbres.ToString());
			var remainingTimbresConfig = new MexicoNotificationRemainingFolioConfiguration() { FoliosQuantity = expectedRemainingTimbres, Interval = DEFAULT_CONFIGURATION_INTERVAL };

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("1 email created", 1, emailsCreatedList.Count);
			}

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, Logger);
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				AssertEquals("2 emails created", 2, emailsCreatedList.Count);
			}
		}

		public void TestAfterEventMessageProcessed_Throws_WhenNullArguments()
		{
			var universalEvent = GetUniversalEvent();

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var xueFunctionalityProvider = countryFactory.GetGlobalXUEFunctionalityProvider();

			AssertExceptionThrown<ArgumentNullException>("Universal Event can't be null.", () => xueFunctionalityProvider.AfterEventMessageProcessed(null, InvoiceBatch, Logger));
			AssertExceptionThrown<ArgumentNullException>("InvoiceBatch can't be null.", () => xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, null, Logger));
			AssertExceptionThrown<ArgumentNullException>("Logger can't be null.", () => xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, null));
		}

		int SetupAndExecuteAfterEventMessageProcessed(string expectedRemainingTimbres, IGlobalXUEFunctionalityProvider xueFunctionalityProvider, MexicoNotificationRemainingFolioConfiguration remainingTimbresConfig, IXmlSessionTracker logger = null)
		{
			var universalEvent = GetUniversalEvent(expectedRemainingTimbres: expectedRemainingTimbres);
			var companyPk = InvoiceBatch.AIB_GC.ToGuid();

			using (AccountingConfigurationRegistry.Instance.MexicoRemainingFolioNotification.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, remainingTimbresConfig))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				xueFunctionalityProvider.AfterEventMessageProcessed(universalEvent, InvoiceBatch, logger ?? new XmlSessionTracker(new ServiceTaskLogForTesting()));
				var emailsCreatedList = Env.OutgoingMailManager.EmailsCreated;

				return emailsCreatedList.Count;
			}
		}

		UniversalEvent GetUniversalEvent(bool includeRemainingTimbresContext = true, string expectedRemainingTimbres = "0")
		{
			var remainingTimbresContext = includeRemainingTimbresContext
				? $@"
<Context>
	<Type>EINV_MX_RemainingStamps</Type>
	<Value>{expectedRemainingTimbres}</Value>
</Context>
" : string.Empty;

			EDIMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-05-16T09:30:10</EventTime>
		<EventType>ANY</EventType>
		<EventParameters>
			<MessageType>MX</MessageType>
			<MessageSubType>MSG</MessageSubType>
			<Reason>Reason</Reason>
		</EventParameters>
		<ContextCollection>
			{remainingTimbresContext}
		</ContextCollection>
	</Event>
</UniversalEvent>";
			return ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
		}

		void CreateStaffInAllGroup(string emailAddress = null)
		{
			var staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = "someone";
			staffPluto.GS_Code = "SOM";
			staffPluto.GS_EmailAddress = emailAddress ?? "someone@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Groups.AllPK));
			Factory.Save();
		}

		protected IXmlSessionTracker Logger;

		protected override void SetUp()
		{
			base.SetUp();

			Logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
		}

		EDIMessage EDIMessage => ediMessage ??= EDIMessageTestFactory.New(Factory);
		EDIMessage ediMessage;

		AccEInvoicingBatch InvoiceBatch => invoiceBatch ??= new TestObjectCreator(Factory).CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
		AccEInvoicingBatch invoiceBatch;
	}
}
