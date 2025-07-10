using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	public class EInvoicingEventMessageTWProcessorTest : TestCaseWithFactory
	{
		public void TestIAKMessageProcessing()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
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
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TW</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTW</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageTWProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
		}

		public void TestIAKMessageProcessing_UpdateThroughResponseFileResult()
		{
			AssertEquals(false, EnvProxy.Instance.IsProductionSystem);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				complianceDocument.ADH_InternalReference = "00001087";

				var arCreditNote = objectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var complianceDocument2 = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arCreditNote.Lines[0], objectCreator.Debtor);
				complianceDocument2.ADH_InternalReference = "00001088";
				var pivot2 = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.Sent);

				var arInvoice2 = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var complianceDocument3 = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA003", "TXE", "desc", arInvoice2.Lines[0], objectCreator.Debtor);
				complianceDocument3.ADH_InternalReference = "00001089";
				var pivot3 = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument3, Core.Constants.EInvoicingPivotState.Sent);
				Factory.Save();

				var inMessage = EDIMessageTestFactory.New(Factory);
				inMessage.EM_MessageText = $@"
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
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>TW</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTW</Value>
			</Context>
			<Context>
				<Type>ResponseFileResult</Type>
				<Value>MjAyMTEyMDUwMDAwMTA4N3xPfFdNMDAwMDE4MHw1Mjg4OTMxN3w4MDE2ODQ2NnxOfOeZvOelqOiZn+eivOagvOW8j+mMr+iqpCznmbznpajomZ/norw6V00wMDAwMTgwCjIwMjExMjA1MDAwMDEwODh8T3xWQzAwMDAwNjAwMXw1Mjg4OTMxN3w5Njk0NDQ5MHxOfOWtl+S4sumVt+W6puWkp+aWvOmZkOWItumVt+W6piAxMAoyMDIxMTIwNTAwMDAxMDg5fE98VkMwMDAwMDYwMDF8NTI4ODkzMTd8OTY5NDQ0OTB8WXw=</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
				var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();

				var processor = new EInvoicingEventMessageTWProcessor(logger, EDIMessage, universalEvent, batch);

				AssertEquals(EInvoicingPivotState.Sent, pivot.AIP_Status);
				AssertEquals(string.Empty, pivot.AIP_ErrorDescription);
				AssertEquals(EInvoicingPivotState.Sent, pivot2.AIP_Status);
				AssertEquals(string.Empty, pivot2.AIP_ErrorDescription);
				AssertEquals(EInvoicingPivotState.Sent, pivot3.AIP_Status);
				AssertEquals(string.Empty, pivot3.AIP_ErrorDescription);

				processor.Process();

				AssertEquals(EInvoicingPivotState.Failed, pivot.AIP_Status);
				AssertEquals("發票號碼格式錯誤,發票號碼:WM0000180", pivot.AIP_ErrorDescription);
				AssertEquals(EInvoicingPivotState.Failed, pivot2.AIP_Status);
				AssertEquals("字串長度大於限制長度 10", pivot2.AIP_ErrorDescription);
				AssertEquals(EInvoicingPivotState.Succeed, pivot3.AIP_Status);
				AssertEquals(string.Empty, pivot3.AIP_ErrorDescription);
			}
		}

		public void TestIRJMessageProcessing()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
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
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>TW</MessageType>
			<Reason>This message is rejected for test</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTW</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageTWProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, logger.Logs.ElementAt(0).Message);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", "This message is rejected for test", pivot.AIP_ErrorDescription);
		}

		public void TestISNMessageProcessing()
		{
			var arCreditNote2 = objectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument2 = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arCreditNote2.Lines[0], objectCreator.Debtor);
			var pivot2 = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.Delivered);

			var arCreditNote3 = objectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument3 = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arCreditNote3.Lines[0], objectCreator.Debtor);
			var pivot3 = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument3, Core.Constants.EInvoicingPivotState.Failed);

			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
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
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>ISN</EventType>
		<EventParameters>
			<Department>TPE</Department>
			<MessageType>TW</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DTW</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageTWProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Delivered, pivot2.AIP_Status);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Failed, pivot3.AIP_Status);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Delivered, pivot.AIP_Status);
			AssertEquals("Postcondition: No change will happen when AIP_Status is not SNT", Core.Constants.EInvoicingPivotState.Delivered, pivot2.AIP_Status);
			AssertEquals("Postcondition: No change will happen when AIP_Status is not SNT", Core.Constants.EInvoicingPivotState.Failed, pivot3.AIP_Status);
		}

		EDIMessage EDIMessage
		{
			get
			{
				if (ediMessage == null)
				{
					ediMessage = EDIMessageTestFactory.New(Factory);
				}
				return ediMessage;
			}
		}
		EDIMessage ediMessage;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			objectCreator = new TestObjectCreator(Factory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
				batch = objectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
				Factory.Save();
			}
		}

		readonly string emailSendingUnsuccessfulMessage = $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Compliance Document AA001 [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		AccComplianceDocumentHeader complianceDocument;
		AccEInvoicingBatch batch;
		AccEInvoicingTransactionPivot pivot;
		IXmlSessionTracker logger;
		TestObjectCreator objectCreator;
	}
}
