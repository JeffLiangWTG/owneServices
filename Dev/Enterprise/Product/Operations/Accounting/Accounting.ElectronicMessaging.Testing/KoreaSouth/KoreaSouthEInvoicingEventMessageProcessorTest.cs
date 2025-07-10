using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class KoreaSouthEInvoicingEventMessageProcessorTest : TestCaseWithFactory
	{
		#region AttachTaxInvoiceXMLToEDocs

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_SingleInvoices_WhenIssueIDAndIssueDateTimeIsEmpty()
		{
			var (pivotGen, pivotGeq, invoice) = CreateSample("00001001", "202203271234567800000001");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq);

			Factory.Save();

			var docTaxInvoices = new List<string>();
			docTaxInvoices.Add(Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes($@"
<TaxInvoice>
    <TaxInvoiceDocument>
        <IssueID/>
        <IssueDateTime/>
    </TaxInvoiceDocument>
</TaxInvoice>")));

			LinkEDIMessageToInvoice(new[] { invoice }, docTaxInvoices);
			AssertContains("Pre-condition:", EInvoicingKoreaSouthConstants.DataContext.KoreaDocTaxInvoice, EDIMessage.EM_MessageText);
			ediMessage = null;

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] { "202203271234567800000001-Success-" }
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(0, invoice.DocManagerInfo.Files.Count);

			var expectedError = "Unable to attach the tax invoice XML of Invoice [00001001] to eDoc.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_SingleInvoices_WhenContextDoNotExistKoreaDocTaxInvoice()
		{
			var (pivotGen, pivotGeq, invoice) = CreateSample("00001001", "202203271234567800000001");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq);

			Factory.Save();

			LinkEDIMessageToInvoice(new[] { invoice }, null, "20220621");
			AssertNotContains("Pre-condition:", EInvoicingKoreaSouthConstants.DataContext.KoreaDocTaxInvoice, EDIMessage.EM_MessageText);
			ediMessage = null;

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] { "202203271234567800000001-Success-" }
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(0, invoice.DocManagerInfo.Files.Count);

			var expectedError = "Unable to attach the tax invoice XML of Invoice [00001001] to eDoc.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_SingleInvoices_WhenExistMultipleSubmitSuccessLogs()
		{
			var (pivotGen, pivotGeq, invoice) = CreateSample("00001001", "202203271234567800000001");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq);

			Factory.Save();

			LinkEDIMessageToInvoice(new[] { invoice }, "20220621");
			LinkEDIMessageToInvoice(new[] { invoice }, "20220622");

			var logs = batchGen.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode
							&& l.SourceInfoItems.Any(x => x.Key.Replace(" ", "") == EInvoicingKoreaSouthConstants.DataContext.KoreaStatusCode && x.Data == EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess)).OrderBy(l => l.SL_EventTime);
			AssertEquals("Pre-condition:", 2, logs.Count());
			AssertEquals("Pre-condition:", false, logs.AllSame(x => x.SL_EventTime));
			AssertContains("Pre-condition:", "20220621", logs.First().ReferenceFreeText);
			AssertContains("Pre-condition:", "20220622", logs.Last().ReferenceFreeText);

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] { "202203271234567800000001-Success-" }
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(1, invoice.DocManagerInfo.Files.Count);
			AssertEquals("TaxInvoice_202203271234567800000001_20220622.xml", invoice.DocManagerInfo.Files[0].FileName);

			var xml = @"
<TaxInvoice>
    <TaxInvoiceDocument>
        <IssueID>202203271234567800000001</IssueID>
        <IssueDateTime>20220622</IssueDateTime>
    </TaxInvoiceDocument>
</TaxInvoice>";
			AssertAuthorisationRecord(invoice, xml);
		}

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_SingleInvoices_WhenNotExistSubmitSuccessLog()
		{
			var (pivotGen, pivotGeq, invoice) = CreateSample("00001001", "202203271234567800000001");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq);

			Factory.Save();

			AssertEquals("Pre-condition:", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] { "202203271234567800000001-Success-" }
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(0, invoice.DocManagerInfo.Files.Count);

			var expectedError = "Unable to attach the tax invoice XML of Invoice [00001001] to eDoc.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_SingleInvoices_WhenIssueIDDoNotMatch()
		{
			var (pivotGen, pivotGeq, invoice) = CreateSample("00001001", "202203271234567800000001");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq);

			Factory.Save();

			LinkEDIMessageToInvoice(new[] { invoice });

			invoice.GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(AccTransactionHeaderReferenceTypes.KRI).AH1_Reference = "202203271234567800000002";
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] { "202203271234567800000002-Success-" }
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(0, invoice.DocManagerInfo.Files.Count);

			var expectedError = "Unable to attach the tax invoice XML of Invoice [00001001] to eDoc.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcessIAK_QuerySuccess_AttachTaxInvoiceXMLToEDocs_MultipleInvoices()
		{
			var (pivotGen1, pivotGeq1, invoice_Success1) = CreateSample("00001001", "202203271234567800000001");
			var (pivotGen2, pivotGeq2, invoice_Success2) = CreateSample("00001002", "202203271234567800000002");
			var (pivotGen3, pivotGeq3, invoice_Failed)   = CreateSample("00001003", "202203271234567800000003");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen1, pivotGen2, pivotGen3);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq1, pivotGeq2, pivotGeq3);

			Factory.Save();

			LinkEDIMessageToInvoice(new[] { invoice_Success1, invoice_Success2, invoice_Failed });

			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, documentStatusCodes: new[] {
						"202203271234567800000001-Success-" ,
						"202203271234567800000002-Success-" ,
						"202203271234567800000003-Fail-MessageWillBeRecorded",
					}
				);
			var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
			processorQuery.Process();

			AssertEquals(EInvoicingPivotState.Succeed, invoice_Success1.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(EInvoicingPivotState.Succeed, invoice_Success2.EInvoicingTransactionPivotSubmitted.AIP_Status);
			AssertEquals(EInvoicingPivotState.Failed, invoice_Failed.EInvoicingTransactionPivotSubmitted.AIP_Status);

			AssertEquals(1, invoice_Success1.DocManagerInfo.Files.Count);
			AssertEquals(1, invoice_Success2.DocManagerInfo.Files.Count);
			AssertEquals(0, invoice_Failed.DocManagerInfo.Files.Count);

			AssertEquals(ReferenceTypes.Accounting, invoice_Success1.DocManagerInfo.Files[0].DocType);
			AssertEquals(ReferenceTypes.Accounting, invoice_Success2.DocManagerInfo.Files[0].DocType);

			AssertEquals("TaxInvoice_202203271234567800000001_20220620.xml", invoice_Success1.DocManagerInfo.Files[0].FileName);
			AssertEquals("TaxInvoice_202203271234567800000002_20220620.xml", invoice_Success2.DocManagerInfo.Files[0].FileName);

			var xml1 = @"
<TaxInvoice>
    <TaxInvoiceDocument>
        <IssueID>202203271234567800000001</IssueID>
        <IssueDateTime>20220620</IssueDateTime>
    </TaxInvoiceDocument>
</TaxInvoice>";

			var xml2 = @"
<TaxInvoice>
    <TaxInvoiceDocument>
        <IssueID>202203271234567800000002</IssueID>
        <IssueDateTime>20220620</IssueDateTime>
    </TaxInvoiceDocument>
</TaxInvoice>";

			AssertEquals(xml1, invoice_Success1.DocManagerInfo.Files[0].ImageData.ToUTF8());
			AssertEquals(xml2, invoice_Success2.DocManagerInfo.Files[0].ImageData.ToUTF8());

			AssertAuthorisationRecord(invoice_Success1, xml1);
			AssertAuthorisationRecord(invoice_Success2, xml2);
		}

		void AssertAuthorisationRecord(InvoicingBase transaction, string taxInvoiceXml)
		{
			var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, transaction.PK);
			var authRecords = Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);
			AssertEquals(1, authRecords.Length);
			AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, authRecords[0].AHF_ParentTableCode);
			AssertEquals(AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth, authRecords[0].AHF_RecordType);
			AssertEquals(new KoreaSouthEInvoicingDataFinder().GetSubmitId(transaction.GetMostRecentEInvoicingTransactionPivot().Batch), authRecords[0].AHF_Number);
			AssertEquals(taxInvoiceXml, authRecords[0].AHF_AuthorisationData.ToUTF8());
		}

		#endregion

		public void TestProcessIAK_QuerySuccess_UpdateEInvoicingPivotForLinkedCreditNote_Reverse()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			{
				var (pivotGen1, pivotGeq1, arInvoice) = CreateSample("00001001", "202203271234567800000001");
				Factory.Save();
				AssertEquals(EInvoicingPivotState.Delivered, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				new ReversingFactory().NewReversing(arInvoice).Reverse();
				var arCreditNote = arInvoice.ReverseInvoice;
				Factory.Save();
				AssertEquals(EInvoicingPivotState.Pending, arCreditNote.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchGen, pivotGen1);

				var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchGeq, pivotGeq1);

				Factory.Save();

				LinkEDIMessageToInvoice(new[] { arInvoice });

				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
						, AutoEvents.InterchangeAcknowledgedCode
						, ""
						, documentStatusCodes: new[] {
						"202203271234567800000001-Success-" ,
						}
					);
				var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
				processorQuery.Process();

				AssertEquals(EInvoicingPivotState.Succeed, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals(EInvoicingPivotState.Queued, arCreditNote.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
			}
		}

		public void TestProcessIAK_QuerySuccess_UpdateEInvoicingPivotForLinkedCreditNote_Amend()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			{
				var (pivotGen1, pivotGeq1, arInvoice) = CreateSample("00001001", "202203271234567800000001");
				Factory.Save();
				AssertEquals(EInvoicingPivotState.Delivered, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				var arCreditNote1 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction as ARCreditNote;
				var arCreditNote2 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction as ARCreditNote;
				Factory.Save();
				AssertEquals(EInvoicingPivotState.Pending, arCreditNote1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals(EInvoicingPivotState.Pending, arCreditNote2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchGen, pivotGen1);

				var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchGeq, pivotGeq1);

				Factory.Save();

				LinkEDIMessageToInvoice(new[] { arInvoice });

				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
						, AutoEvents.InterchangeAcknowledgedCode
						, ""
						, documentStatusCodes: new[] {
						"202203271234567800000001-Success-" ,
						}
					);
				var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchGeq);
				processorQuery.Process();

				AssertEquals(EInvoicingPivotState.Succeed, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals(EInvoicingPivotState.Queued, arCreditNote1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals(EInvoicingPivotState.Queued, arCreditNote2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
			}
		}

		public void TestProcessIAK_QuerySuccess_UpdateEInvoicingPivotForLinkedInvoice_Amend()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			{
				var (pivotSubmit, pivotCheckStatus, arInvoice) = CreateSample("00001001", "202203271234567800000001");
				Factory.Save();
				AssertEquals(EInvoicingPivotState.Delivered, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				var amendmentInvoice1 = testObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice).amendTransaction as ARInvoice;
				var amendmentInvoice2 = testObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice).amendTransaction as ARInvoice;
				amendmentInvoice1.AH_TransactionNum = "TEST001";
				amendmentInvoice2.AH_TransactionNum = "TEST002";
				amendmentInvoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
				amendmentInvoice2.IsManuallySetTransactionNumber_ForTestOnly = true;
				Factory.Save();
				AssertEquals("Amendment invoice1 should have pivot status Pending, because the original transaction is being E-Invoicing.", EInvoicingPivotState.Pending, amendmentInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals("Amendment invoice2 should have pivot status Pending, because the original transaction is being E-Invoicing.", EInvoicingPivotState.Pending, amendmentInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);

				var batchSubmit = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchSubmit, pivotSubmit);
				var batchCheckStatus = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				BindPivotTobatch(batchCheckStatus, pivotCheckStatus);
				Factory.Save();

				LinkEDIMessageToInvoice(new[] { arInvoice });

				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess, AutoEvents.InterchangeAcknowledgedCode, "", documentStatusCodes: new[] { "202203271234567800000001-Success-" });
				var processorQuery = new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, batchCheckStatus);
				processorQuery.Process();

				AssertEquals(EInvoicingPivotState.Succeed, arInvoice.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals("Amendment invoice1 should have pivot status Queued, because the original transaction is successfully E-Invoiced.", EInvoicingPivotState.Queued, amendmentInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
				AssertEquals("Amendment invoice2 should have pivot status Queued, because the original transaction is successfully E-Invoiced.", EInvoicingPivotState.Queued, amendmentInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status);
			}
		}

		public void TestProcessIAK_QuerySuccess_SimpleCase()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			var batchGen = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1233, EInvoicingBatchState.Sent);

			var queryTimesAddOn = CreateQueryTimesAddOnColumn(batchGen);
			queryTimesAddOn.XA_Data = "0";

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);

			Factory.Save();

			LinkEDIMessageToInvoice(new[] { arInvoice });

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			AssertEquals("PreCondition", true, arInvoice.AH_ComplianceDocumentDate.IsEmpty);
			AssertEquals("PreCondition", false, queryTimesAddOn.IsDeleted);

			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, ""
					, complianceDate: "05/10/2022 11:03:56"
					, documentStatusCodes: new[] { "202203271234567800000001-Success-MessageNotUsed" }
				)
				, batch);
			processor.Process();

			AssertEquals(true, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Succeed, pivotGen.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotGen.AIP_ErrorDescription);
			AssertEquals(true, logger.Logs.IsNullOrEmpty());
			AssertEquals(true, queryTimesAddOn.IsDeleted);

			AssertEquals(ZDate.Empty, arInvoice.AH_ComplianceDocumentDate);

			AssertEquals(1, pivotGen.ParentTransactionHeader.DocManagerInfo.Files.Count);
			AssertContains($"TaxInvoice_{GetIssueIDFormInvoice(pivotGen.ParentTransactionHeader)}_20220620.xml", pivotGen.ParentTransactionHeader.DocManagerInfo.Files[0].FileName);
		}

		public void TestProcessIAK_QuerySuccess_AllCases()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var (pivotGen_Success1, pivotGeq_Success1, invoice_Success1) = CreateSample("00001001", "202203271234567800000001");
			var (pivotGen_Success2, pivotGeq_Success2, invoice_Success2) = CreateSample("00001002", "202203271234567800000002");
			var (pivotGen_Fail, pivotGeq_Fail, invoice_Fail) = CreateSample("00001011", "202203271234567800000011");
			var (pivotGen_GotWrong1, pivotGeq_GotWrong1, invoice_GotWrong1) = CreateSample("00002001", "202203271234567800002001");
			var (pivotGen_GotWrong2, pivotGeq_GotWrong2, invoice_GotWrong2) = CreateSample("00002002", "202203271234567800002002");

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen_Success1, pivotGen_Success2, pivotGen_Fail, pivotGen_GotWrong1, pivotGen_GotWrong2);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq_Success1, pivotGeq_Success2, pivotGeq_Fail, pivotGeq_GotWrong1, pivotGeq_GotWrong2);

			Factory.Save();

			LinkEDIMessageToInvoice(new [] { invoice_Success1, invoice_Success2 });

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess
					, AutoEvents.InterchangeAcknowledgedCode
					, "ReasonMsgWillNotBeUsedHere"
					, documentStatusCodes: new[] {
						"202203271234567800000001-Success-MessageNotUsed" ,
						"202203271234567800000002-Success-" ,
						"202203271234567800000011-Fail-MessageWillBeRecorded",
						"202203271234567800002001-AA-MessageNotUsed",
					}
				)
				, batchGeq);
			processor.Process();

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertInvoiceSucess(pivotGen_Success1, pivotGeq_Success1);
			AssertInvoiceSucess(pivotGen_Success2, pivotGeq_Success2);
			AssertInvoiceFail(pivotGen_Fail, pivotGeq_Fail, invoice_Fail, "MessageWillBeRecorded");
			AssertInvoiceGotWrong(pivotGen_GotWrong1, pivotGeq_GotWrong1, "Invalid Issue-ID mapping result [202203271234567800002001-AA-MessageNotUsed].", $"Invalid Issue-ID mapping result [202203271234567800002001-AA-MessageNotUsed] for [{invoice_GotWrong1.AH_Ledger} {invoice_GotWrong1.AH_TransactionType} {invoice_GotWrong1.AH_TransactionNum}].");
			AssertInvoiceGotWrong(pivotGen_GotWrong2, pivotGeq_GotWrong2, "Did not find Issue-ID mapping result.", $"Invalid Issue-ID mapping result [] for [{invoice_GotWrong2.AH_Ledger} {invoice_GotWrong2.AH_TransactionType} {invoice_GotWrong2.AH_TransactionNum}].");

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			Assert(logger.Logs.Any(x => x.Message.Trim() == @"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed."));
			AssertEmailSubjectAndBody(batchGeq, "Following invoices were failed."
				, new (InvoicingBase invoice, string invoiceErrorMsg)[] {
					(invoice_Fail, "MessageWillBeRecorded"),
					(invoice_GotWrong1, "Invalid Issue-ID mapping result [202203271234567800002001-AA-MessageNotUsed]."),
					(invoice_GotWrong2, "Did not find Issue-ID mapping result."),
			});

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			void AssertInvoiceSucess(AccEInvoicingTransactionPivot pivotGen, AccEInvoicingTransactionPivot pivotGeq)
			{
				AssertEquals(true, pivotGeq.IsDeleted);
				AssertEquals(EInvoicingPivotState.Succeed, pivotGen.AIP_Status);
				AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
				AssertEquals("", pivotGen.AIP_ErrorDescription);

				AssertEquals(1, pivotGen.ParentTransactionHeader.DocManagerInfo.Files.Count);
				AssertContains($"TaxInvoice_{GetIssueIDFormInvoice(pivotGen.ParentTransactionHeader)}_20220620.xml", pivotGen.ParentTransactionHeader.DocManagerInfo.Files[0].FileName);
			}

			void AssertInvoiceGotWrong(AccEInvoicingTransactionPivot pivotGen, AccEInvoicingTransactionPivot pivotGeq, string expectedErrorMsgForInvoice, string expectedErrorMsgForLog)
			{
				AssertEquals(true, pivotGeq.IsDeleted);
				AssertEquals(false, pivotGen.IsDeleted);
				AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
				AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
				AssertEquals(expectedErrorMsgForInvoice, pivotGen.AIP_ErrorDescription);

				AssertEquals(1, logger.Logs.Count(x => x.Message.Contains(expectedErrorMsgForLog)));
			}

			void AssertInvoiceFail(AccEInvoicingTransactionPivot pivotGen, AccEInvoicingTransactionPivot pivotGeq, InvoicingBase invoice, string expectedErrorDescription)
			{
				AssertEquals(true, pivotGeq.IsDeleted);
				AssertEquals(false, pivotGen.IsDeleted);
				AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
				AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
				AssertEquals(expectedErrorDescription, pivotGen.AIP_ErrorDescription);
			}
		}

		public void TestProcessIAK_SubmitFail()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1233, EInvoicingBatchState.Sent);

			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.SubmitFail, AutoEvents.InterchangeAcknowledgedCode, "SubmitFailReason")
				, batch);
			processor.Process();

			AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("SubmitFailReason", pivotGen.AIP_ErrorDescription);

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertContains(@"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed.", logger.Logs.First().Message);
			AssertEmailSubjectAndBody(batch, "SubmitFailReason"
				, new (InvoicingBase invoice, string invoiceErrorMsg)[] { (arInvoice, null) });
		}

		public void TestProcessIAK_QueryFail()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			var batchGen = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1233, EInvoicingBatchState.Sent);
			var queryTimesAddOn = CreateQueryTimesAddOnColumn(batchGen);
			queryTimesAddOn.XA_Data = "0";

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			AssertEquals("PreCondition", true, queryTimesAddOn.IsInDatabase);
			AssertEquals("PreCondition", false, queryTimesAddOn.IsDeleted);

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QueryFail, AutoEvents.InterchangeAcknowledgedCode, "QueryFailReason")
				, batch);
			processor.Process();

			AssertEquals(true, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("QueryFailReason", pivotGen.AIP_ErrorDescription);
			AssertEquals(true, queryTimesAddOn.IsDeleted);

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertContains(@"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed.", logger.Logs.First().Message);
			AssertEmailSubjectAndBody(batch, "QueryFailReason"
				, new (InvoicingBase invoice, string invoiceErrorMsg)[] { (arInvoice, null) });
		}

		public void TestProcessIAK_QueryWaiting()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_ActionType = EInvoicingPivotActionType.Submit;
			pivotGen.AIP_Status = EInvoicingPivotState.Delivered;
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotQuery.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotQuery.AIP_ErrorDescription = ZString.Empty;

			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QueryProcessing, AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batch);
			processor.Process();

			AssertEquals(false, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotQuery.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotQuery.AIP_ErrorDescription);

			AssertEquals(EInvoicingPivotState.Delivered, pivotGen.AIP_Status);
			AssertEquals(ZDateTime.Empty, pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotGen.AIP_ErrorDescription);

			AssertEquals(true, logger.Logs.IsNullOrEmpty());
		}

		public void TestProcessIAK_QueryWaiting_ReachMaxQueryTimes()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");

			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			var batchGen = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1233, EInvoicingBatchState.Sent);

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);

			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.QueryProcessing, AutoEvents.InterchangeAcknowledgedCode, "AAAAAABQQQQ")
				, batch);
			AssertNull(FindQueryTimesAddOnColumn(batchGen));

			processor.Process();
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals("1", FindQueryTimesAddOnColumn(batchGen).XA_Data);
			processor.Process();
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals("2", FindQueryTimesAddOnColumn(batchGen).XA_Data);
			processor.Process();
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals("3", FindQueryTimesAddOnColumn(batchGen).XA_Data);
			processor.Process();
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals("4", FindQueryTimesAddOnColumn(batchGen).XA_Data);
			processor.Process();
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals("5", FindQueryTimesAddOnColumn(batchGen).XA_Data);
			AssertEquals(false, FindQueryTimesAddOnColumn(batchGen).IsDeleted);

			processor.Process();

			var expectedErrorMessage = "Exceeded the maximum times of automatic Query Status. Please retrieve the e-Invoice status manually via Action > Request e-Invoice Status, and take the appropriate action based on the status returned.";

			AssertEquals(true, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals(expectedErrorMessage, pivotGen.AIP_ErrorDescription);
			AssertNull(FindQueryTimesAddOnColumn(batchGen));

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertContains(@"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed.", logger.Logs.First().Message);
			AssertEmailSubjectAndBody(batch, expectedErrorMessage, new (InvoicingBase invoice, string invoiceErrorMsg)[] { (arInvoice, null) });
		}

		GenAddOnColumn FindQueryTimesAddOnColumn(BusinessObject parentBizO)
		{
			var addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentBizO.PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, parentBizO.TablePrefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, AccEInvoicingBatch.Schema.AIB_QueryTimes);
			return parentBizO.Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
		}

		GenAddOnColumn CreateQueryTimesAddOnColumn(BusinessObject parentBizO)
		{
			var addOnColumn = Factory.New<GenAddOnColumn>();
			addOnColumn.XA_ParentID = parentBizO.PK;
			addOnColumn.XA_ParentTableCode = parentBizO.TablePrefix;
			addOnColumn.XA_Name = AccEInvoicingBatch.Schema.AIB_QueryTimes;
			addOnColumn.XA_Type = AddOnColumnDataType.Codes.Integer;

			return addOnColumn;
		}

		public void TestProcessIAK_SubmitSuccess()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;

			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess, AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batch);
			processor.Process();

			var pivotQuery = Factory.LoadTop1<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
			);
			AssertNotNull(pivotQuery);
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotQuery.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotQuery.AIP_ErrorDescription);

			AssertEquals(EInvoicingPivotState.Delivered, pivotGen.AIP_Status);
			AssertEquals(ZDateTime.Empty, pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotGen.AIP_ErrorDescription);

			AssertEquals(true, logger.Logs.IsNullOrEmpty());
		}

		public void TestProcessIAK_UnkownStatus()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, "202203271234567800000001");
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			var batchGen = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1233, EInvoicingBatchState.Sent);

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotQuery.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotQuery.AIP_ErrorDescription = ZString.Empty;
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batch);
			processor.Process();

			AssertEquals(false, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Sent, pivotQuery.AIP_Status);
			AssertEquals(ZDateTime.Empty, pivotQuery.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotQuery.AIP_ErrorDescription);

			AssertEquals(false, pivotGen.IsDeleted);
			AssertEquals(EInvoicingPivotState.Delivered, pivotGen.AIP_Status);
			AssertEquals(ZDateTime.Empty, pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("", pivotGen.AIP_ErrorDescription);

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			var expectedError = "No update performed due to invalid status code [A].";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcess_AllSuccessOrFail()
		{
			var (pivotGen_Success, pivotGeq_Sent1, _) = CreateSample("00001001", "202203271234567800000001");
			pivotGen_Success.AIP_Status = EInvoicingPivotState.Succeed;
			pivotGeq_Sent1.AIP_Status = EInvoicingPivotState.Sent;

			var (pivotGen_Fail, pivotGeq_Sent2, _) = CreateSample("00001002", "202203271234567800000002");
			pivotGen_Fail.AIP_Status = EInvoicingPivotState.Failed;
			pivotGeq_Sent2.AIP_Status = EInvoicingPivotState.Sent;

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen_Success, pivotGen_Fail);

			var batchGeq = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGeq, pivotGeq_Sent1, pivotGeq_Sent2);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batchGeq);
			processor.Process();

			AssertPivot(pivotGeq_Sent1);
			AssertPivot(pivotGeq_Sent2);
			AssertPivot(pivotGen_Success);
			AssertPivot(pivotGen_Fail);

			var log = logger.Logs.First();
			AssertEquals("Log type", Enterprise.Integration.LogType.Warning, log.Type);
			var expectedWarning = $"No update performed due to transaction pivot having 'SUC' or 'FAL' status for invoice submit batch 10001 in {GlbCompany.CurrentCompany.GC_Name}.";
			AssertContains(expectedWarning, log.Message);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			void AssertPivot(AccEInvoicingTransactionPivot pivot)
			{
				AssertEquals(false, pivot.IsDeleted);
				AssertEquals(false, pivot.AIP_StatusInfo.HasChanges);
				AssertEquals(ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("", pivot.AIP_ErrorDescription);
			}
		}

		public void TestProcess_BatchDiscard()
		{
			var (pivotGen, pivotGeq, _) = CreateSample("00001001", "202203271234567800000001");
			pivotGen.AIP_Status = EInvoicingPivotState.Delivered;
			pivotGeq.AIP_Status = EInvoicingPivotState.Sent;

			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(batchGen, pivotGen);
			batchGen.AIB_Status = EInvoicingBatchState.Discarded;

			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batchGen);
			processor.Process();

			AssertEquals(false, pivotGen.HasChanges);
			AssertEquals(false, pivotGeq.HasChanges);
			AssertEquals(true, logger.Logs.IsNullOrEmpty());
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestProcess_NotFoundAnyPivotForQueryStatusBatch()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA", KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest)
				, batch);

			var exp = AssertExceptionThrown<NullReferenceException>(() => processor.Process());
			AssertEquals("Object reference not set to an instance of an object.", exp.Message);
			AssertContains("at Enterprise.Accounting.ElectronicMessaging.KoreaSouth.KoreaSouthEInvoicingEventMessageProcessor.GetSubmitBatch()", exp.StackTrace);
		}

		public void TestProcess_NotFoundSubmitBatch()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotQuery.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotQuery.AIP_ErrorDescription = ZString.Empty;
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA")
				, batch);
			processor.Process();

			AssertEquals(false, logger.Logs.IsNullOrEmpty());

			var expectedError = "Submission batch is not found.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcess_NotFoundAnyPivotForSubmitBatch()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeAcknowledgedCode, "AAAAAA", KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest)
				, batch);
			processor.Process();

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			var expectedError = $"No update performed due to transaction pivot not found for invoice submit batch {batch.AIB_BatchNumber} in {GlbCompany.CurrentCompany.GC_Name}.";
			AssertContains(expectedError, logger.Logs.First().Message);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestProcess_IRJ_GEN()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotQuery.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
			pivotQuery.AIP_Status = EInvoicingPivotState.Sent;
			pivotQuery.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotQuery.AIP_ErrorDescription = ZString.Empty;

			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;

			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotGen, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeRejectedCode, "AAAAAA", KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest)
				, batch);
			processor.Process();

			AssertEquals(true, pivotQuery.IsDeleted);
			AssertEquals(false, pivotGen.IsDeleted);
			AssertEquals(EInvoicingPivotState.Failed, pivotGen.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotGen.AIP_LastResponseReceivedUtc);
			AssertEquals("AAAAAA", pivotGen.AIP_ErrorDescription);

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertContains(@"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed.", logger.Logs.First().Message);
			AssertEmailSubjectAndBody(batch, "AAAAAA"
				, new (InvoicingBase invoice, string invoiceErrorMsg)[] { (arInvoice, null) });
		}

		public void TestProcess_IRJ_GEQ()
		{
			SetDummyEInvoicingErrorNotificationGroup();
			var batchGen = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;
			pivotGen.AIP_AIB = batchGen.PK;

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("1234568", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice2.IsManuallySetTransactionNumber_ForTestOnly = true;
			var pivotGen2 = arInvoice2.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen2.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen2.AIP_ErrorDescription = ZString.Empty;
			pivotGen2.AIP_AIB = batchGen.PK;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotQuery.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotQuery.AIP_ErrorDescription = ZString.Empty;

			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1234, EInvoicingBatchState.Sent);
			Factory.Save();

			IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var processor = new KoreaSouthEInvoicingEventMessageProcessor(logger
				, EDIMessage
				, GetUniversalEvent("A", AutoEvents.InterchangeRejectedCode, "AAAAAA", KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest)
				, batch);
			processor.Process();

			AssertEquals(false, pivotQuery.IsDeleted);
			AssertEquals(EInvoicingPivotState.Queued, pivotQuery.AIP_Status);
			AssertEquals(new ZDateTime(2018, 1, 9, 9, 30, 10), pivotQuery.AIP_LastResponseReceivedUtc);
			AssertEquals("AAAAAA", pivotQuery.AIP_ErrorDescription);

			AssertPivotGen(pivotGen);
			AssertPivotGen(pivotGen2);

			AssertEquals(false, logger.Logs.IsNullOrEmpty());
			AssertContains(@"Email Notification was sent successfully for Eagle Datamation International.
E-Reporting Email Notification task completed.", logger.Logs.First().Message);
			AssertEmailSubjectAndBody(batch, @"All invoices in this batch have re-queuqed automatically due to following reason:
AAAAAA"
				, new (InvoicingBase invoice, string invoiceErrorMsg)[] { (arInvoice, null), (arInvoice2, null) });

			void AssertPivotGen(AccEInvoicingTransactionPivot pivot)
			{
				AssertEquals(false, pivot.IsDeleted);
				AssertEquals(EInvoicingPivotState.Delivered, pivot.AIP_Status);
				AssertEquals(ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
				AssertEquals("", pivot.AIP_ErrorDescription);
			}
		}

		public void TestRecordReceiptIdToInvoice()
		{
			var batchSeq = 1;
			foreach (var statusCode in typeof(EInvoicingKoreaSouthConstants.StatusCodes).GetConstantValues().Except(EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess))
			{
				AssertRecordReceiptIdToInvoice(batchSeq, $"0000{batchSeq}0", statusCode, ReadyKoreaConstants.ValidationDocumentStatusCodes.Fail
					, expectedHavingReciptId: false);
				batchSeq++;

				AssertRecordReceiptIdToInvoice(batchSeq, $"0000{batchSeq}0", statusCode, ReadyKoreaConstants.ValidationDocumentStatusCodes.Success
					, expectedHavingReciptId: false);
				batchSeq++;
			}

			AssertRecordReceiptIdToInvoice(batchSeq, $"0000{batchSeq}0", EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess, ReadyKoreaConstants.ValidationDocumentStatusCodes.Fail
				, expectedHavingReciptId: false);
			batchSeq++;

			AssertRecordReceiptIdToInvoice(batchSeq, $"0000{batchSeq}0", EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess, ReadyKoreaConstants.ValidationDocumentStatusCodes.Success
				, expectedHavingReciptId: true);
		}

		void AssertRecordReceiptIdToInvoice(int batchSeq, string invoiceNum, string statusCode, string invocieProcessingStatus, bool expectedHavingReciptId)
		{
			ediMessage = null;
			EDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			invoiceNum = invoiceNum.PadLeft(8, '0');
			invoiceNum = invoiceNum.Substring(invoiceNum.Length - 8, 8);
			var issueID = $"2022032712345678{invoiceNum}";
			var mockReceiptID = $"ReceiptID_{invoiceNum}";

			var (pivotGen, _, invoice) = CreateSample(invoiceNum, issueID);
			var submitBatch = TestObjectCreator.CreateEInvoicingBatch(10000 + batchSeq, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			BindPivotTobatch(submitBatch, pivotGen);
			Factory.Save();

			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var universalEvent = GetUniversalEvent(statusCode
				, AutoEvents.InterchangeAcknowledgedCode
				, ""
				, receiptId: mockReceiptID
				, documentStatusCodes: new[] { $"{issueID}-{invocieProcessingStatus}-" }
			);
			new KoreaSouthEInvoicingEventMessageProcessor(logger, EDIMessage, universalEvent, submitBatch).Process();
			Factory.Save();

			var recriptIdRef = invoice.GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(AccTransactionHeaderReferenceTypes.RED);
			if (expectedHavingReciptId)
			{
				AssertNotNull("Receipt ID should be recored.", recriptIdRef);
				AssertEquals("Receipt ID", mockReceiptID, recriptIdRef.AH1_Reference);
			}
			else
			{
				AssertNull("Receipt ID should not be recored.", recriptIdRef);
			}

			ClearUnimportantReportingError();

			void ClearUnimportantReportingError()
			{
				if (ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastMessageReported == $"Unable to attach the tax invoice XML of Invoice [{invoiceNum}] to eDoc.")
				{
					ErrorReporter.Clear();
				}
			}
		}

		string GetIssueIDFormInvoice(TransactionHeader invoice)
		{
			return invoice.GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(AccTransactionHeaderReferenceTypes.KRI).AH1_Reference;
		}

		void LinkEDIMessageToInvoice(InvoicingBase[] invoices, string issueDateTime = null)
		{
			var docTaxInvoices = new List<string>();
			foreach (var invoice in invoices)
			{
				docTaxInvoices.Add(Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes($@"
<TaxInvoice>
    <TaxInvoiceDocument>
        <IssueID>{GetIssueIDFormInvoice(invoice)}</IssueID>
        <IssueDateTime>{issueDateTime ?? "20220620"}</IssueDateTime>
    </TaxInvoiceDocument>
</TaxInvoice>")));
			}

			LinkEDIMessageToInvoice(invoices, docTaxInvoices, issueDateTime);
			ediMessage = null;
		}

		void LinkEDIMessageToInvoice(InvoicingBase[] invoices, List<string> docTaxInvoices, string issueDateTime = null)
		{
			var universalEvent = GetUniversalEvent(EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess, AutoEvents.InterchangeAcknowledgedCode, "AAAAAA", KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, docTaxInvoices: docTaxInvoices);

			EDIMessage.EM_Status = EDIMessage.Status.Queued;
			EDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			EDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			EDIMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			EDIMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var linker = new MessageDataLogLinker(Events.InterchangeAcknowledged, Factory, $"Reference with {issueDateTime}");
			invoices.ForEach(x => linker.LinkMessageToParentBOLogs(EDIMessage, x.EInvoicingProxy.MostRecentPivot.Batch));
			Factory.Save();

			AssertEquals("Pre-condition: no eDocs", true, invoices.All(x => x.DocManagerInfo.Files.Count == 0));
		}

		(AccEInvoicingTransactionPivot PivotSubmit, AccEInvoicingTransactionPivot PivotCheckStatus, InvoicingBase Invoice) CreateSample(string invoiceNum, string issueId)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(invoiceNum, testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			StoreIssueId(arInvoice, issueId);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivotGen = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			pivotGen.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGen.AIP_ErrorDescription = ZString.Empty;

			var pivotGeq = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
			pivotGeq.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			pivotGeq.AIP_ErrorDescription = ZString.Empty;

			return (pivotGen, pivotGeq, arInvoice);
		}

		void BindPivotTobatch(AccEInvoicingBatch bindBatch, params AccEInvoicingTransactionPivot[] pivots)
		{
			foreach (var pivot in pivots)
			{
				pivot.AIP_AIB = bindBatch.PK;
			}
		}

		void AssertEmailSubjectAndBody(AccEInvoicingBatch expectedBatch, string expectedBatchErrorMsg, IEnumerable<(InvoicingBase invoice, string invoiceErrorMsg)> invoiceErrorMsgs)
		{
			var mails = EnvProxy.Instance.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, mails.Count);

			var email = mails[0];
			AssertEquals($"E-Reporting error notification for Electronic Invoicing Batch {expectedBatch.AIB_BatchNumber} [EDI]", email.Subject);

			foreach (var part in GetExpectedBodyParts())
			{
				AssertContains(part, email.Body);
			}

			IEnumerable<ZString> GetExpectedBodyParts()
			{
				var parts = new List<ZString>();
				parts.Add(@"<html><body>
The following items were not successfully submitted to E-Reporting authority:
<br/><br/>");
				parts.Add($@"<span style='text-decoration: underline;'>Electronic Invoicing Batch {expectedBatch.AIB_BatchNumber}</span><br/>
<b>
&nbsp;&nbsp;
Error Details:
</b>
<div style='width:1200px; margin-left:30px;'>
{expectedBatchErrorMsg}
</div>");
				foreach (var (invoice, invoiceErrorMsg) in invoiceErrorMsgs)
				{
					parts.Add($@"<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARInvoice&BusinessEntityPK={invoice.PK}");
					if (!string.IsNullOrWhiteSpace(invoiceErrorMsg))
					{
						parts.Add($@">Transaction AR INV {invoice.AH_TransactionNum}</a><br/>
<b>
&nbsp;&nbsp;
Error Details:
</b>
<div style='width:1200px; margin-left:30px;'>
{invoiceErrorMsg}
</div>");
					}
					else
					{
						parts.Add($@">Transaction AR INV {invoice.AH_TransactionNum}</a><br/>");
					}
				}
				parts.Add($@"</br>
If you wish to re-submit the items, please reset the status of each item to Queued in the appropriate module.
<br/><br/>
<b>
Related EDI Message:
</b><a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=EDIMessage&BusinessEntityPK=");
				parts.Add(@"'></a><br/>
<b>
Company:
</b>
EDI - Eagle Datamation International
<br/>
</body></html>");
				return parts;
			}
		}

		UniversalEvent GetUniversalEvent(
			string statusCode,
			string eventType,
			string reason,
			string messageType = null,
			string complianceDate = "",
			string receiptId = "",
			IEnumerable<string> documentStatusCodes = null,
			IEnumerable<string> docTaxInvoices = null)
		{
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
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>{eventType}</EventType>
		<EventParameters>
			<MessageType>KR</MessageType>
			<MessageSubType>{messageType}</MessageSubType>
			<Reason>{reason}</Reason>
		</EventParameters>
		<ContextCollection>
 			<Context>
				<Type>EINV_ComplianceDate</Type>
				<Value>{complianceDate}</Value>
			</Context>
			<Context>
				<Type>EINV_KoreaStatusCode</Type>
				<Value>{statusCode}</Value>
			</Context>
			<Context>
				<Type>EINV_ReceiptID</Type>
				<Value>{receiptId}</Value>
			</Context>
{GetDocumentStatusCodeTags()}
{GetDocTaxInvoicesTags()}
		</ContextCollection>
	</Event>
</UniversalEvent>";
			return EDIMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();

			string GetDocumentStatusCodeTags()
			{
				return documentStatusCodes == null
					? ""
					: string.Join(System.Environment.NewLine, documentStatusCodes.Select(documentStatusCode => $"<Context><Type>EINV_KoreaDocumentStatus</Type><Value>{documentStatusCode}</Value></Context>"));
			}

			string GetDocTaxInvoicesTags()
			{
				return docTaxInvoices == null
					? ""
					: string.Join(System.Environment.NewLine, docTaxInvoices.Select(docTaxInvoice => $"<Context><Type>EINV_DocTaxInvoice</Type><Value>{docTaxInvoice}</Value></Context>"))
					+ $"<Context><Type>EINV_DocTaxInvoiceCount</Type><Value>{docTaxInvoices.Count()}</Value></Context>";
			}
		}

		void StoreIssueId(AccTransactionHeader transaction, string issueId)
		{
			var reference = transaction.Factory.New<AccTransactionHeaderReference>();
			reference.AH1_AH = transaction.PK;
			reference.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			reference.AH1_Reference = issueId;
		}

		void SetDummyEInvoicingErrorNotificationGroup()
		{
			var helper = new EInvoicingTestHelper(TestObjectCreator);
			var notificationGroupPK = helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid());
		}

		EDIMessage EDIMessage => ediMessage ?? (ediMessage = EDIMessageTestFactory.New(Factory));
		EDIMessage ediMessage;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
