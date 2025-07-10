using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class EInvoicingBatchStatusUpdateExtensionsTest : TestCaseWithFactory
	{
		public void TestUpdateBatchAndPivotStatusAndErrorDescription_BatchStatusUpdated()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Constants.EInvoicingPivotState.Batched);

			var transactions = new List<InvoicingBase>();
			transactions.Add(arInvoice);
			var errorMessage = "error111";

			batch.UpdateBatchAndPivotStatusAndErrorDescription(transactions.Select(x => x.PK), errorMessage, Constants.EInvoicingPivotState.Failed);
			AssertEquals("Batch status not modified.", Constants.EInvoicingBatchState.Ready, batch.AIB_Status);
			AssertEquals("Pivot status modified.", Constants.EInvoicingPivotState.Failed, pivot1.AIP_Status);
			AssertEquals("Pivot status not modified.", Constants.EInvoicingPivotState.Batched, pivot2.AIP_Status);
			AssertEquals("Pivot error message modified.", errorMessage, pivot1.AIP_ErrorDescription);
			AssertEquals("Pivot error message not modified.", ZString.Empty, pivot2.AIP_ErrorDescription);
			Assert("Not in DB.", !batch.IsInDatabase);
			Assert("Not in DB.", !pivot1.IsInDatabase);
			Assert("Not in DB.", !pivot2.IsInDatabase);
		}

		public void TestUpdateBatchAndPivotStatusAndErrorDescription_BatchStatusNotUpdated()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Constants.EInvoicingPivotState.Batched);

			var transactions = new List<InvoicingBase>();
			transactions.Add(arCreditNote);
			var errorMessage = "error222";

			batch.UpdateBatchAndPivotStatusAndErrorDescription(transactions.Select(x => x.PK), errorMessage, Constants.EInvoicingPivotState.Succeed, Constants.EInvoicingBatchState.Sent);
			AssertEquals("Batch status modified.", Constants.EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Pivot status not modified.", Constants.EInvoicingPivotState.Batched, pivot1.AIP_Status);
			AssertEquals("Pivot status modified.", Constants.EInvoicingPivotState.Succeed, pivot2.AIP_Status);
			AssertEquals("Pivot error message not modified.", ZString.Empty, pivot1.AIP_ErrorDescription);
			AssertEquals("Pivot error message modified.", errorMessage, pivot2.AIP_ErrorDescription);
			Assert("Not in DB.", !batch.IsInDatabase);
			Assert("Not in DB.", !pivot1.IsInDatabase);
			Assert("Not in DB.", !pivot2.IsInDatabase);
		}

		public void TestUpdateBatchAndPivotStatusAndErrorDescription_ForLongErrorMessage()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "123456789123456";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var transactions = new List<InvoicingBase>();
				transactions.Add(arInvoice);
				var errorMessage = "abcdefghijklmnopqrstuvwxyz";
				var longErrorMessages = new List<ZString>();
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);
				longErrorMessages.Add(errorMessage);

				batch.UpdateBatchAndPivotStatusAndErrorDescription(transactions.Select(x => x.PK), longErrorMessages, Constants.EInvoicingPivotState.Failed);
				var first230charOfErrorDescription = "abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz. abcdefghi";
				var restOfTheErrorDescription = $"...Details in notification email sent to user group {group.GG_Code}";
				AssertEquals("Pivot error message truncated and modified as expected.", $"{first230charOfErrorDescription}{restOfTheErrorDescription}", pivot.AIP_ErrorDescription);
				AssertEquals("Character count.", AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength, pivot.AIP_ErrorDescription.Length);
			}
		}

		public void TestUpdateBatchWarningDescription()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "123456789123456";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var transactions = new List<InvoicingBase>();
				transactions.Add(arInvoice);
				var warningMessage = "WARNING: FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may be rejected. You can add CIG and CUP as additional references on the operational job record.";

				batch.MarkBatchAndTransactionPivotsAsSent(transactions.Select(x => x.PK), EventProcessorHelper.CheckLengthAndTruncIfNeeded(batch.Company, warningMessage));
				var first230charOfWarningDescription = "WARNING: FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto <IdDocumento>, <CodiceCUP> and <CodiceCIG> Job should contain Order Reference, CIG and CUP when Account is a GOV Organization. Without these references, the invoice may ";
				var restOfTheWarningDescription = $"...Details in notification email sent to user group {group.GG_Code}";
				AssertEquals("Pivot error message truncated and modified as expected.", $"{first230charOfWarningDescription}{restOfTheWarningDescription}", pivot.AIP_ErrorDescription);
				AssertEquals("Character count.", AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength, pivot.AIP_ErrorDescription.Length);
			}
		}

		public void TestMarkBatchAndTransactionPivotsAsSent()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Constants.EInvoicingPivotState.Batched);

			var transactions = new List<InvoicingBase>();
			transactions.Add(arInvoice);

			batch.MarkBatchAndTransactionPivotsAsSent(transactions.Select(x => x.PK));
			AssertEquals("Batch status modified.", Constants.EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals("Pivot status modified.", Constants.EInvoicingPivotState.Sent, pivot1.AIP_Status);
			AssertEquals("Pivot status not modified.", Constants.EInvoicingPivotState.Batched, pivot2.AIP_Status);
			AssertDateTimeWithinOneSecond("Pivot last sent time modified.", ZDateTime.UtcNow.ToDateTime(), pivot1.AIP_LastSentTimeUtc.ToDateTime());
			AssertEquals("Pivot last sent time not modified.", ZDateTime.Empty, pivot2.AIP_LastSentTimeUtc);
			Assert("Not in DB.", !batch.IsInDatabase);
			Assert("Not in DB.", !pivot1.IsInDatabase);
			Assert("Not in DB.", !pivot2.IsInDatabase);
		}

		public void TestLoadInvoicingBasesReadyToBeSent()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Constants.EInvoicingPivotState.BatchedWithError);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var transactionPKs = batch.LoadEInvoicingPKsToBeSent(AccTransactionHeaderSchema.Constants.Prefix);
				AssertEquals(1, transactionPKs.Length);
				AssertCollectionContains(arInvoice.PK, transactionPKs);
			}

			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var transactionPKs = batch.LoadEInvoicingPKsToBeSent(AccTransactionHeaderSchema.Constants.Prefix);
				AssertEquals(2, transactionPKs.Length);
				AssertCollectionContains(arInvoice.PK, transactionPKs);
				AssertCollectionContains(arCreditNote.PK, transactionPKs);
			}
		}

		public void TestLoadComplianceDocumentsReadyToBeSent()
		{
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice1.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			arInvoice2.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			var complianceDocument1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice1.Lines[0], TestObjectCreator.Debtor);
			var complianceDocument2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA002", "TXE", "desc", arInvoice2.Lines[0], TestObjectCreator.Debtor);
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.BatchedWithError);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var complianceDocumentPKs = batch.LoadEInvoicingPKsToBeSent(AccComplianceDocumentHeaderSchema.Constants.Prefix);
				AssertEquals(1, complianceDocumentPKs.Length);
				AssertCollectionContains(complianceDocument1.PK, complianceDocumentPKs);
			}

			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var complianceDocumentPKs = batch.LoadEInvoicingPKsToBeSent(AccComplianceDocumentHeaderSchema.Constants.Prefix);
				AssertEquals(2, complianceDocumentPKs.Length);
				AssertCollectionContains(complianceDocument1.PK, complianceDocumentPKs);
				AssertCollectionContains(complianceDocument2.PK, complianceDocumentPKs);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}