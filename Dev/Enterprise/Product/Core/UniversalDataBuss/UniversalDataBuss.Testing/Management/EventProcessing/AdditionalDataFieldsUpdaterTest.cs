using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Testing.Management.EventProcessing
{
	class AdditionalDataFieldsUpdaterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestFieldUpdaterUpdate()
		{
			// Arrange
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001238";
			Factory.SaveForTesting();

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(GetEvent("EDCBA"));
			logParent.Z0_Code = "ABCDE";

			// Act
			manager.Process(message);
			// Assert
			AssertEquals("Linked Event to Dummy Business Object XXX00001238.\r\nField [DummyWithWorkflow.Z0_Code] has been updated to value [EDCBA] on Dummy Business Object XXX00001238.", string.Join("\r\n", manager.Logger.Logs));
			AssertEquals("EDCBA", logParent.Z0_Code);
		}

		public void TestFieldUpdaterUpdate_ValueOverLength()
		{
			// Arrange
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001238";
			Factory.SaveForTesting();

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(GetEvent("FEDCBA"));
			logParent.Z0_Code = "ABCDE";

			// Act
			manager.Process(message);
			// Assert
			AssertEquals("Linked Event to Dummy Business Object XXX00001238.\r\nWarning - The maximum length of 'Z0_Code' has been exceeded.\n The maximum length of this property is 5 characters, but 6 were entered. New value: FEDCBA. Old value: ABCDE\r\nValue [FEDCBA] has been truncated to the limit of 5 characters for Field <DummyWithWorkflow.Z0_Code>.", string.Join("\r\n", manager.Logger.Logs));
			AssertEquals("FEDCB", logParent.Z0_Code);
		}

		public void TestFieldUpdaterUpdate_UpdateTransactionNumberForAccountingInvoice()
		{
			var invoice = ObjectFactory.Get<ITransactionCreator>().CreateTransaction(Factory.BOFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var company = Factory.Load<GlbCompany>(invoice.AH_GC);
			Factory.SaveForTesting();
			var transactionNum = invoice.AH_TransactionNum;

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice.AH_Ledger, invoice.AH_TransactionType, transactionNum));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>
			{
				new() { Type = "ARInvoice.AH_TransactionNum", Value = "AAAA999" }
			};
			eventDataObject.EventType = Events.DataImportCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var logger = manager.Process(message);

			AssertNoExceptionThrown(() => { Factory.SaveForTesting(); });

			AssertEquals("AH_TransactionNum should not be changed.", transactionNum, invoice.AH_TransactionNum);

			var expectedMessage = "Field [ARInvoice.AH_TransactionNum] in Accounts Receivable Invoice could not be modified because it's already saved in database.";
			AssertEquals(string.Format("The logger should log the warning message: {0}", expectedMessage), true, logger.Logs.Any((l) => l.Type == Enterprise.Integration.LogType.Warning && l.Message == expectedMessage));
		}

		public void TestFieldUpdaterUpdate_WhenUpdatingJobDecWithValidValues_ShouldSaveSuccessfully()
		{
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var company = Factory.Load<GlbCompany>(jobDeclaration.JE_GC);
			var newStatus = ScreeningStatusesList.Codes.JobClearedExternal;

			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, jobDeclaration.JE_DeclarationReference);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>
			{
				new() { Type = "JobDeclaration.JE_ScreeningStatus", Value = newStatus }
			};
			eventDataObject.EventType = Events.DataImportCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			IXmlSessionTracker logger;
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				var message = GetQueuedUniversalEventMessage(eventDataObject);
				logger = manager.Process(message);
			}

			AssertNoExceptionThrown(() => { Factory.SaveForTesting(); });
			var expectedMessage = @$"Field [JobDeclaration.JE_ScreeningStatus] has been updated to value [{newStatus}] on Declaration {jobDeclaration.JE_DeclarationReference}.";
			AssertEquals(string.Format("The logger should log the info message: {0}", expectedMessage), true, logger.Logs.Any((l) => l.Type == Enterprise.Integration.LogType.Information && l.Message == expectedMessage));
		}

		public void TestFieldUpdaterUpdate_WhenUpdatingJobDecWithRegistryOff_ShouldNotUpdateValues()
		{
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var company = Factory.Load<GlbCompany>(jobDeclaration.JE_GC);
			var initialStatus = ScreeningStatusesList.Codes.Clear;

			jobDeclaration.JE_ScreeningStatus = initialStatus;
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, jobDeclaration.JE_DeclarationReference);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>
			{
				new() { Type = "JobDeclaration.JE_ScreeningStatus", Value = ScreeningStatusesList.Codes.JobBlockedExternal }
			};
			eventDataObject.EventType = Events.DataImportCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			IXmlSessionTracker logger;
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				var message = GetQueuedUniversalEventMessage(eventDataObject);
				logger = manager.Process(message);
			}

			AssertNoExceptionThrown(() => { Factory.SaveForTesting(); });
			AssertEquals("Job Declaration should not be updated", initialStatus, jobDeclaration.JE_ScreeningStatus);
			var expectedMessage = "The provided screening status value is invalid. Ensure it matches one of the accepted statuses.";
			AssertEquals(string.Format("The logger should log the warning message: {0}", expectedMessage), true, logger.Logs.Any((l) => l.Type == Enterprise.Integration.LogType.Warning && l.Message == expectedMessage));
		}

		public void TestFieldUpdaterGeneratesWarningForZGuid()
		{
			// Arrange
			var logParent = Factory.BOFactory.New<IDummyWithWorkflow>();
			logParent.Z0_Description = "XXX00001238";
			logParent.Z0_Code = "ABCDE";
			Factory.SaveForTesting();

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var messageText = GetEvent(new Dictionary<string, string>
			{
				{ nameof(DummyWithWorkflow.Z0_Guid), "1EC95B12-9C44-4BE1-A8AB-4FE6CFDBC281" },
				{ nameof(DummyWithWorkflow.Z0_Code), "EDCBA" },
			});
			var message = GetQueuedUniversalEventMessage(messageText);

			// Act
			manager.Process(message);
			// Assert
			AssertEquals(@"Linked Event to Dummy Business Object XXX00001238.
Warning - Invalid property type [CargoWise.Types.ZGuid]. The property [Z0_Guid] can't be updated.
Field [DummyWithWorkflow.Z0_Code] has been updated to value [EDCBA] on Dummy Business Object XXX00001238.", string.Join("\r\n", manager.Logger.Logs));
			AssertEquals("EDCBA", logParent.Z0_Code);
			AssertEquals(ZGuid.Empty, logParent.Z0_Guid);
		}

		string GetEvent(string value, string fieldName = nameof(DummyWithWorkflow.Z0_Code))
		{
			return GetEvent(new Dictionary<string, string> { { fieldName, value } });
		}
		string GetEvent(Dictionary<string, string> fieldValuePairs)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>XXX00001238</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <Company>
        <Code>{Env.CurrentCompany.Code}</Code>
        <Name>Fat Old Olgas</Name>
      </Company>
      <EnterpriseID>WAG</EnterpriseID>
      <EventType>
        <Code>FUL</Code>
        <Description>Freight Unloaded</Description>
      </EventType>
      <ServerID>BOT</ServerID>
      <DataProvider>CargoWise One</DataProvider>
    </DataContext>
    <EventTime>2011-07-12T00:00:00.000</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USCIT</Value>
      </Context>
    </ContextCollection>
    <AdditionalFieldsToUpdateCollection>
{string.Join("\r\n",
	fieldValuePairs.Select(fv =>
	$@"<AdditionalFieldsToUpdate>
        <Type>DummyWithWorkflow.{fv.Key}</Type>
        <Value>{fv.Value}</Value>
      </AdditionalFieldsToUpdate>")
	)}
    </AdditionalFieldsToUpdateCollection>
  </Event>
</UniversalEvent>";
		}
	}
}
