using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class EInvoicingEventMessageVNProcessorTest : TestCaseWithFactory
	{
		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForSRN()
		{
			AssertIAKMessageProcessing(VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice);
			AssertNotNull("Document Request action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(pivot, EInvoicingPivotActionType.DocumentAction));
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForCRN()
		{
			AssertIAKMessageProcessing(VietnamEInvoiceAPICommandList.Codes.CancelReceivablesInvoice);
			AssertNotNull("Document Request action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(pivot, EInvoicingPivotActionType.DocumentAction));
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForCRC()
		{
			AssertIAKMessageProcessing(VietnamEInvoiceAPICommandList.Codes.CancelReceivablesCircular78Invoice);
			AssertNotNull("Document Request action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(pivot, EInvoicingPivotActionType.DocumentAction));
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForRAJ()
		{
			AssertIAKMessageProcessing(VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice);
			AssertNotNull("Document Request action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(pivot, EInvoicingPivotActionType.Approve));
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForRAP()
		{
			AssertIAKMessageProcessing(VietnamEInvoiceAPICommandList.Codes.ApproveReceivablesInvoice);
			AssertNotNull("Document Request action pivot is created after successful process.", GetPivotCreatedAfterSubmitActionProcess(pivot, EInvoicingPivotActionType.DocumentAction));
		}

		void AssertIAKMessageProcessing(string messageSubType)
		{
			EventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCodes.VietNam,
				MessageSubType = messageSubType,
			};
			var processor = new EInvoicingEventMessageVNProcessor(logger, EDIMessage, EventDataObject, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForRDN()
		{
			invoice.AH_ComplianceDocumentDate = new ZDate(2019, 8, 1);
			invoice.AH_TransactionReference = "GV19T0007168";

			var base64EncodedResponseMessage = new ZString("WwogICAgewogICAgICAgICJwZGYiOiAiZGF0YTphcHBsaWNhdGlvbi9wZGY7YmFzZTY0LEpWQkVSaTB4TGpZS0pmLy8vLzhLQ2pVZ01DQnZZbW9LUER3S0NTOWoiCiAgICB9Cl0=");

			var message = UOFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = string.Format(IAKMessageForRDN, 100, base64EncodedResponseMessage);

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertEquals("Precondition", 0, logger.Count);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var postServicePivot = newFactory.Load<AccEInvoicingTransactionPivot>(pivot.PK);
			var postServiceInvoice = newFactory.Load<AccTransactionHeader>(invoice.PK);

			var expectedFileName = "EInvoice_" + postServiceInvoice.AH_TransactionReference + "_" + postServiceInvoice.AH_ComplianceDocumentDate.ToString("yyyyMMdd") + ".pdf";
			var codedContext = base64EncodedResponseMessage.ToUTF8FromBase64().Trim(new char[] { '[', ']' });
			var eInvoiceDocumentResponse = JsonConvert.DeserializeObject<VietnamEInvoiceDocumentResponse>(codedContext);
			var eInvoiceDocumentContext = Convert.FromBase64String(eInvoiceDocumentResponse.pdf.Replace("data:application/pdf;base64,", ""));

			CombineAssertions(() =>
			{
				AssertEquals("PostCondition", "", ErrorReporter.LastKeyReported);
				AssertEquals("Postcondition", 14, logger.Count);
				AssertEquals("Postcondition", new ZDateTime(2019, 8, 1, 17, 51, 00), postServicePivot.AIP_LastResponseReceivedUtc);
				AssertEquals("Postcondition", EInvoicingPivotState.Succeed, postServicePivot.AIP_Status);
				AssertEquals("Postcondition", ZString.Empty, postServicePivot.AIP_ErrorDescription);
				AssertEquals("Precondition", 1, postServiceInvoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals("PostCondition", expectedFileName, postServiceInvoice.DocManagerInfo.AllEDocs[0].FileName);
				AssertEquals("PostCondition", "ACC", postServiceInvoice.DocManagerInfo.AllEDocs[0].DocType);
				AssertEquals("PostCondition", new ZBlob(eInvoiceDocumentContext), postServiceInvoice.DocManagerInfo.AllEDocs[0].ImageData);
			});
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForRDN_FileNameofCancellationRequest()
		{
			invoice.AH_ComplianceDocumentDate = new ZDate(2019, 8, 1);
			invoice.AH_TransactionReference = "GV19T0007168";
			pivot.AIP_Status = EInvoicingPivotState.Succeed;

			var helper = new EInvoicingTestHelper(objectCreator);
			var arCreditNote = (ARCreditNote)helper.ObjectCreator.ReverseTransaction(invoice, out _);
			var batch = objectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivotArCreditNote = objectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, EInvoicingPivotState.Sent);
			Factory.Save();

			var base64EncodedResponseMessage = new ZString("WwogICAgewogICAgICAgICJwZGYiOiAiZGF0YTphcHBsaWNhdGlvbi9wZGY7YmFzZTY0LEpWQkVSaTB4TGpZS0pmLy8vLzhLQ2pVZ01DQnZZbW9LUER3S0NTOWoiCiAgICB9Cl0=");

			var message = UOFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = string.Format(IAKMessageForRDN, 101, base64EncodedResponseMessage);

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertEquals("Precondition", 0, logger.Count);
			AssertEquals("Precondition", ZDateTime.Empty, pivotArCreditNote.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivotArCreditNote.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivotArCreditNote.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, arCreditNote.DocManagerInfo.AllEDocs.Count);

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var postServiceInvoice = newFactory.Load<AccTransactionHeader>(arCreditNote.PK);
			AssertEquals("PostCondition", "EInvoice_GV19T0007168_20190801.pdf", postServiceInvoice.DocManagerInfo.AllEDocs[0].FileName);
		}

		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIRJMessageProcessing()
		{
			EventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			EventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCodes.VietNam,
				Reason = "This message is rejected for test",
			};
			var processor = new EInvoicingEventMessageVNProcessor(logger, EDIMessage, EventDataObject, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);

			processor.Process();

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, logger.Logs.ElementAt(0).Message);
			AssertEquals("Postcondition", ZDateTime.Now, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", "This message is rejected for test", pivot.AIP_ErrorDescription);
		}

		AccEInvoicingTransactionPivot GetPivotCreatedAfterSubmitActionProcess(AccEInvoicingTransactionPivot submitPivot, ZString actionType)
			=> submitPivot.Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, submitPivot.AIP_ParentID)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued));

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
			objectCreator = new TestObjectCreator(UOFactory.BOFactory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				batch = objectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);
				Factory.Save();
			}

			EventDataObject = new UniversalEvent();
			EventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			EventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			EventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			EventDataObject.EventTime = ZDateTimeOffset.Now;
		}

		#region IAKMessageForRDN

		readonly string IAKMessageForRDN = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Name>Eagle Datamation International</Name>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-08-01T17:51:00</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageSubType>RDN</MessageSubType>
			<MessageType>VN</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DVN</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
		#endregion

		readonly string emailSendingUnsuccessfulMessage = $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Transaction AR INV 00001000 [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		InvoicingBase invoice;
		AccEInvoicingBatch batch;
		AccEInvoicingTransactionPivot pivot;
		IXmlSessionTracker logger;
		TestObjectCreator objectCreator;
		UniversalEvent EventDataObject;
		UniversalObjectFactory UOFactory => uoFactory ?? (uoFactory = new UniversalObjectFactory());
		UniversalObjectFactory uoFactory;
	}
}
