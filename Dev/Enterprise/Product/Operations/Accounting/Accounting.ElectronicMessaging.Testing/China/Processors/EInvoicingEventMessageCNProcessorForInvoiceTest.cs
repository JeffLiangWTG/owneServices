using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ElectronicMessaging.Testing
{
	public class EInvoicingEventMessageCNProcessorForInvoiceTest : TestCaseWithFactory
	{
		[TestDate(2019, 8, 1, 17, 51, 31)]
		public void TestIAKMessageProcessingForSIU()
		{
			var base64EncodedResponseMessage1 = new ZString("base64EncodedResponseMessage");

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1001", TestObjectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1002", TestObjectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var message = UOFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = string.Format(IAKMessage, string.Join("|", invoice1.PK.ToString(), invoice2.PK.ToString()), base64EncodedResponseMessage1, "CDV");

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			var query1 = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);
			query1.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, invoice1.PK);
			var reference1 = Factory.LoadTop1<AccTransactionHeaderReference>(query1);

			var query2 = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);
			query2.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, invoice2.PK);
			var reference2 = Factory.LoadTop1<AccTransactionHeaderReference>(query2);

			AssertEquals("Precondition", 0, logger.Count);
			AssertPrecondition(invoice1, reference1);
			AssertPrecondition(invoice2, reference2);

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var postServiceInvoice1 = newFactory.Load<AccTransactionHeader>(invoice1.PK);
			reference1 = newFactory.LoadTop1<AccTransactionHeaderReference>(query1);

			var postServiceInvoice2 = newFactory.Load<AccTransactionHeader>(invoice2.PK);
			reference2 = newFactory.LoadTop1<AccTransactionHeaderReference>(query2);

			var codedContext1 = base64EncodedResponseMessage1.ToUTF8FromBase64();

			AssertEquals("PostCondition", "", ErrorReporter.LastKeyReported);
			AssertEquals("PostCondition", 14, logger.Count);
			AssertProcessingResult(postServiceInvoice1, reference1);
			AssertProcessingResult(postServiceInvoice2, reference2);

			void AssertPrecondition(InvoicingBase invoice, AccTransactionHeaderReference reference)
			{
				AssertEquals("Precondition", 0, logger.Count);
				AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Precondition", ZString.Empty, invoice.AH_TransactionReference);
				AssertEquals("Precondition", ZString.Empty, invoice.AH_ComplianceSubType);
				AssertNull("Precondition", reference);
			}
			void AssertProcessingResult(AccTransactionHeader postServiceInvoice, AccTransactionHeaderReference reference)
			{
				var fileName = $"VAT_{postServiceInvoice.AH_TransactionNum}_ETB_152000186357_2022-03-22";
				CombineAssertions(() =>
				{
					AssertEquals("PostCondition", 1, postServiceInvoice.DocManagerInfo.AllEDocs.Count);
					AssertEquals("PostCondition", $"{fileName}.pdf", postServiceInvoice.DocManagerInfo.AllEDocs[0].FileName);
					AssertEquals("PostCondition", "MSC", postServiceInvoice.DocManagerInfo.AllEDocs[0].DocType);
					AssertEquals("PostCondition", codedContext1, postServiceInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
					AssertEquals("PostCondition", new ZDate(2019, 8, 2), postServiceInvoice.AH_ComplianceDocumentDate);
					AssertEquals("PostCondition", "15200018635723479938", postServiceInvoice.AH_TransactionReference);
					AssertEquals("PostCondition", "TXA", postServiceInvoice.AH_ComplianceSubType);
					Assert("PostCondition", postServiceInvoice.Logs.GetAllLogs().Cast<StmALog>().Any(x =>
						x.SL_SE_NKEvent == AutoEvents.DocumentImportedCode && x.SL_Reference == fileName));
					AssertNotNull("PostCondition", reference);
					AssertEquals("PostCondition", "CDV", reference.AH1_Reference);
					AssertEquals("PostCondition", 10m, reference.AH1_Amount);
				});
			}
		}

		public void TestRefreshComplianceNumberAndComplianceDateForSIU()
		{
			var base64EncodedResponseMessage1 = new ZString("base64EncodedResponseMessage");

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoice.AH_ComplianceDocumentDate = new ZDate(2019, 08, 01);
			invoice.AH_TransactionReference = "1111";

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);

			var reference = UOFactory.BOFactory.New<AccTransactionHeaderReference>();
			reference.AH1_AH = invoice.PK;
			reference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
			reference.AH1_Reference = ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code;

			var message = UOFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = string.Format(IAKMessage, invoice.PK.ToString(), base64EncodedResponseMessage1, "CDI");

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			reference = newFactory.Load<AccTransactionHeaderReference>(reference.PK);
			invoice = newFactory.Load<ARInvoice>(invoice.PK);

			AssertNotNull("PostCondition", reference);
			AssertEquals("PostCondition", ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code, reference.AH1_Reference);
			AssertEquals("PostCondition", "2019-08-02", invoice.AH_ComplianceDocumentDate.ToString("yyyy-MM-dd"));
			AssertEquals("PostCondition", "15200018635723479938", invoice.AH_TransactionReference);
		}

		readonly string IAKMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccountingInvoice</Type>
					<Key/>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-08-01T17:51:00</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageSubType>SIU</MessageSubType>
			<MessageType>CN</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>DCN</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceNumber</Type>
				<Value>15200018635723479938</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceDate</Type>
				<Value>2019-08-02 20:21:26</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceSubType</Type>
				<Value>TXA</Value>
			</Context>
			<Context>
				<Type>EINV_CN_SerialNumber</Type>
				<Value>{0}|EDIEDIDAT</Value>
			</Context>
			<Context>
				<Type>EINV_ComplianceDocumentStatus</Type>
				<Value>{2}</Value>
			</Context>
			<Context>
				<Type>EINV_CN_VoidedAndCreditedAmount</Type>
				<Value>10.00</Value>
			</Context>
		</ContextCollection>
		<AttachedDocumentCollection>
			<AttachedDocument>
				<FileName>ETB_152000186357_2022-03-22.pdf</FileName>
				<ImageData>{1}</ImageData>
				<Type>
					<Code>MSC</Code>
					<Description>Miscellaneous Document</Description>
				</Type>
				<IsPublished>true</IsPublished>
			</AttachedDocument>
		</AttachedDocumentCollection>
	</Event>
</UniversalEvent>
";

		public void TestIRJMessageProcessingForSIU()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var message = UOFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = string.Format(IRJMessage, invoice.PK.ToString());

			UOFactory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertEquals("Precondition", 0, logger.Count);

			serviceTask.RunTask();

			var emailSendingUnsuccessfulMessage = $@"Information|Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Transaction AR INV 00001000 [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
			Assert("Postcondition", logger.ToString().Contains(emailSendingUnsuccessfulMessage));
		}

		readonly string IRJMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccountingInvoice</Type>
					<Key/>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-08-01T17:51:00</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageSubType>SIU</MessageSubType>
			<MessageType>CN</MessageType>
			<Reason>Failed to upload for some reason</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>EINV_CN_SerialNumber</Type>
				<Value>{0}|EDIEDIDAT</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(UOFactory.BOFactory);
		}

		TestObjectCreator TestObjectCreator;

		UniversalObjectFactory UOFactory => uoFactory ?? (uoFactory = new UniversalObjectFactory());
		UniversalObjectFactory uoFactory;
	}
}
