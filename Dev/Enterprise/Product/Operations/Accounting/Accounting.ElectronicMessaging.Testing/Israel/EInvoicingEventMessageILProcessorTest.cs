using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Israel.Testing
{
	public class EInvoicingEventMessageILProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_WhenSubmissionTransactionReturnAcknowledged()
		{
			var govAllocNr = "456";
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeAcknowledgedCode, govermentAllocatedID: govAllocNr);
			AssertResult(arInvoice, batchSubmit, pivotSubmit, EInvoicingPivotState.Succeed, govermentAllocatedID: govAllocNr);
		}

		public void TestProcess_WhenSubmissionTransactionReturnRejected()
		{
			var rejectMessage = "Submit is Rejected";
			var (arInvoice, pivotSubmit, batchSubmit) = ProcessSubmit(AutoEvents.InterchangeRejectedCode, error: rejectMessage);
			AssertResult(arInvoice, batchSubmit, pivotSubmit, EInvoicingPivotState.Failed, error: rejectMessage);
		}

		void AssertResult(TransactionHeader arInvoice, AccEInvoicingBatch submitBatch, AccEInvoicingTransactionPivot submitPivot, string submitPivotExpectStatus,
			string error = "", string govermentAllocatedID = "")
		{
			AssertEquals(EInvoicingBatchState.Sent, submitBatch.AIB_Status);

			AssertEquals(submitPivotExpectStatus, submitPivot.AIP_Status);
			AssertEquals(error, submitPivot.AIP_ErrorDescription);
			AssertEquals(EInvoicingPivotActionType.Submit, submitPivot.AIP_ActionType);
			AssertEquals(CountryCodes.Israel, submitPivot.AIP_RN_NKCountryCode);

			AssertEquals(govermentAllocatedID, arInvoice.AH_GovernmentAllocatedID);
		}

		(TransactionHeader, AccEInvoicingTransactionPivot, AccEInvoicingBatch) ProcessSubmit(string eventType, string error = "", string govermentAllocatedID = "")
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.USD/*ILS*/, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 5.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

				var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
				var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, EInvoicingBatchState.Sent);

				Factory.Save();

				Process(batchSubmit, eventType, error, govermentAllocatedID);
				return (arInvoice, pivotSubmit, batchSubmit);
			}
		}

		void Process(AccEInvoicingBatch batch, string eventType, string reason = "", string govermentAllocatedID = "")
		{
			var universalEvent = GetUniversalEvent(eventType, reason, govermentAllocatedID);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, EDIMessage, universalEvent, batch);
			var processor = new EInvoicingEventMessageILProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
			processor.Process();
		}

		UniversalEvent GetUniversalEvent(string eventType, string reason, string governmentAllocatedID)
		{
			var governmentAllocatedIDContext = string.IsNullOrEmpty(governmentAllocatedID) ? string.Empty : $@"
<Context>
	<Type>EINV_GovtAllocatedTransactionRefNumber</Type>
	<Value>{governmentAllocatedID}</Value>
</Context>
";

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
		<EventTime>2024-05-10T09:30:10</EventTime>
		<EventType>{eventType}</EventType>
		<EventParameters>
			<MessageType>IL</MessageType>
			<MessageSubType>GEN</MessageSubType>
			<Reason>{reason}</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DIL</Value>
			</Context>
			<Context>
				<Type>EINV_RefreshTokenExpireDateUTC</Type>
				<Value>2024-07-01</Value>
			</Context>
			{governmentAllocatedIDContext}
		</ContextCollection>
	</Event>
</UniversalEvent>";
			return EDIMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
		}

		IXmlSessionTracker Logger => logger ??= new XmlSessionTracker(new ServiceTaskLogForTesting());
		IXmlSessionTracker logger;

		EDIMessage EDIMessage => ediMessage ??= EDIMessageTestFactory.New(Factory);
		EDIMessage ediMessage;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;

		CountryEInvoicingObjectFactory CountryEInvoicingObjectFactory => countryEInvoicingObjectFactory ??= new MalaysiaEInvoicingObjectFactory();
		CountryEInvoicingObjectFactory countryEInvoicingObjectFactory;
	}
}
