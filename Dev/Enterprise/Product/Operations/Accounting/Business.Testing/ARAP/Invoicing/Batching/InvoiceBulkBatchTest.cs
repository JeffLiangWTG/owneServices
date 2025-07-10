using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBulkBatch))]
	public class InvoiceBulkBatchTest : InvoiceBatchHeaderTest
	{
		public override void TestFetchHints()
		{
			Assert(true);
		}

		public void TestNonPersistentStatus()
		{
			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);

			AssertEquals("IsInDatabase is always false.", false, batch.IsInDatabase);
			AssertEquals("IsSavedByFactory is always false.", false, batch.IsSavedByFactory);
		}

		public void TestAH_OH()
		{
			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);

			AssertEquals("AH_OH is always empty.", ZGuid.Empty, batch.AH_OH);
			batch.AH_OH = ZGuid.NewZGuid();
			AssertEquals("AH_OH is always empty.", ZGuid.Empty, batch.AH_OH);
		}

		public void TestClearBatches()
		{
			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);

			InvoiceBatchHeader batch1 = GetTestInvoiceBatch();
			batch.InvoiceBatchHeaders.Add(batch1);
			InvoiceBatchHeader batch2 = GetTestInvoiceBatch();
			batch.InvoiceBatchHeaders.Add(batch2);

			AssertEquals("Batches should be added.", 2, batch.InvoiceBatchHeaders.Count);

			batch.ClearBatches();
			AssertEquals("Batches should be removed.", 0, batch.InvoiceBatchHeaders.Count);
			AssertEquals("A batch should be deleted.", true, batch1.IsDeleted);
			AssertEquals("A batch should be deleted.", true, batch2.IsDeleted);

			InvoicingBase line_0 = Factory.New<ARInvoice>();
			line_0.AH_OH = TestObjectCreator.ABIGAS.PK;
			line_0.AH_OSTotal = 100m;
			line_0.AH_GSTAmount = 10m;
			line_0.AH_InvoiceAmount = 5m;

			AssertBatchTotals(batch, 0m, 0m, 0m);

			batch.CreateBatches(new InvoicingBase[] { line_0 });

			AssertBatchTotals(batch, 100m, 10m, 5m);

			batch.ClearBatches();

			AssertBatchTotals(batch, 0m, 0m, 0m);
		}

		public void TestCreateBatches()
		{
			InvoicingBase line_0 = Factory.New<ARInvoice>();
			line_0.AH_OH = TestObjectCreator.ABIGAS.PK;
			line_0.AH_OSTotal = 100m;
			line_0.AH_GSTAmount = 10m;
			line_0.AH_InvoiceAmount = 5m;
			InvoicingBase line_1 = Factory.New<ARCreditNote>();
			line_1.AH_OH = TestObjectCreator.ABIGAS.PK;
			line_1.AH_OSTotal = 110m;
			line_1.AH_GSTAmount = 20m;
			line_1.AH_InvoiceAmount = 10m;

			InvoicingBase line_2 = Factory.New<ARInvoice>();
			line_2.AH_OH = TestObjectCreator.AALSHI.PK;
			line_2.AH_OSTotal = 10m;
			line_2.AH_GSTAmount = 2m;
			line_2.AH_InvoiceAmount = 1m;
			InvoicingBase line_3 = Factory.New<ARCreditNote>();
			line_3.AH_OH = TestObjectCreator.AALSHI.PK;
			line_3.AH_OSTotal = 11m;
			line_3.AH_GSTAmount = 4m;
			line_3.AH_InvoiceAmount = 2m;
			InvoicingBase line_4 = Factory.New<ARInvoice>();
			line_4.AH_OH = TestObjectCreator.AALSHI.PK;
			line_4.AH_OSTotal = 12m;
			line_4.AH_GSTAmount = 6m;
			line_4.AH_InvoiceAmount = 3m;

			InvoicingBase[] lines = new InvoicingBase[] { line_0, line_1, line_2, line_3, line_4 };

			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS)].Value = true;
			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;
			batch.CreateBatches(lines);

			AssertEquals("InvoiceBulkBatch should contain 2 batches.", 2, batch.InvoiceBatchHeaders.Count);
			AssertEquals("Batch 1 should contain 2 lines.", 2, batch.InvoiceBatchHeaders[0].Line.Count);
			AssertEquals("Batch 2 should contain 3 lines.", 3, batch.InvoiceBatchHeaders[1].Line.Count);

			AssertBatchTotals(batch.InvoiceBatchHeaders[0], 210m, 30m, 15m);
			AssertCollectionContains("Batch 1 should contain the line:", line_0, batch.InvoiceBatchHeaders[0].Line);
			AssertCollectionContains("Batch 1 should contain the line:", line_1, batch.InvoiceBatchHeaders[0].Line);
			AssertEquals("Batch 1 should have setted AH_OH.", TestObjectCreator.ABIGAS.PK, batch.InvoiceBatchHeaders[0].AH_OH);

			AssertBatchTotals(batch.InvoiceBatchHeaders[1], 33m, 12m, 6m);
			AssertCollectionContains("Batch 2 should contain the line:", line_2, batch.InvoiceBatchHeaders[1].Line);
			AssertCollectionContains("Batch 2 should contain the line:", line_3, batch.InvoiceBatchHeaders[1].Line);
			AssertCollectionContains("Batch 2 should contain the line:", line_4, batch.InvoiceBatchHeaders[1].Line);
			AssertEquals("Batch 2 should have setted AH_OH.", TestObjectCreator.AALSHI.PK, batch.InvoiceBatchHeaders[1].AH_OH);

			AssertBatchTotals(batch, 210m + 33m, 30m + 12m, 15m + 6m);

			AssertNoError("batches should be validated to check - selected job types should have only one layout on header", batch.InvoiceBatchHeaders[0].AH_OHInfo, "This debtor has different layout setup information for the selected job types. Review the 'Invoice Batching' configuration for this debtor");
			AssertHasError("batches should be validated to check - selected job types should have only one layout on header", batch.InvoiceBatchHeaders[1].AH_OHInfo, "This debtor has different layout setup information for the selected job types. Review the 'Invoice Batching' configuration for this debtor");
		}

		public void TestInvoiceBulkBatchInitialization()
		{
			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);
			AssertBatchTotals(batch, 0m, 0m, 0m);
		}

		public void TestTogglingBatchInvoiceListItemsUpdatesBatchTotalsCorrectly()
		{
			InvoicingBase line_0 = Factory.New<ARInvoice>();
			line_0.AH_OH = TestObjectCreator.ABIGAS.PK;
			line_0.AH_OSTotal = 100m;
			line_0.AH_GSTAmount = 10m;
			line_0.AH_InvoiceAmount = 5m;

			InvoicingBase line_1 = Factory.New<ARInvoice>();
			line_1.AH_OH = TestObjectCreator.AALSHI.PK;
			line_1.AH_OSTotal = 10m;
			line_1.AH_GSTAmount = 2m;
			line_1.AH_InvoiceAmount = 1m;

			InvoicingBase[] lines = new InvoicingBase[] { line_0, line_1 };

			InvoiceBulkBatch batch = GetNewBusinessObject() as InvoiceBulkBatch;
			AssertNotNull(batch);

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS)].Value = true;
			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;
			batch.CreateBatches(lines);

			AssertBatchTotals(batch, 110m, 12m, 6m);

			line_0.IncludeInTheBatch = ZBool.False;

			AssertBatchTotals(batch, 10m, 2m, 1m);

			line_1.IncludeInTheBatch = ZBool.False;

			AssertBatchTotals(batch, 0m, 0m, 0m);

			line_0.IncludeInTheBatch = ZBool.True;

			AssertBatchTotals(batch, 100m, 10m, 5m);

			line_1.IncludeInTheBatch = ZBool.True;

			AssertBatchTotals(batch, 110m, 12m, 6m);
		}

		public new void TestShouldNotSetTransactionNumberToEmptyWhenTransactionIsInDB()
		{
			Assert("InvoiceBatchHeader.IsSavedByFactory is false. It serves as an non-persistent bizO.", true);
		}

		#region Not Valid Inherited Tests

		public override void TestShouldApplyDataRefreshBusUpdate() => Assert("This transaction can not be saved ever", true);

		public override void TestCanApplyDataRefresh_SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged_BusinessContext() => Assert("This transaction can not be saved ever", true);

		public override void TestUnmanagedRegisteredEditableChildObjectForDataRefresh_DependencyOn_ShouldApplyDataRefreshBusUpdate() => Assert("This transaction can not be saved ever", true);

		public override void TestUnmanagedRegisteredEditableChildObjectForDataRefresh_Context_SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged() => Assert("This transaction can not be saved ever", true);

		public new void TestAH_RX_NKTransactionCurrency()
		{
			Assert(true);
		}

		public new void TestConcurrency()
		{
			Assert(true);
		}

		public new void TestCreateDate()
		{
			Assert(true);
		}

		public new void TestCreatingUser()
		{
			Assert(true);
		}

		public new void TestCreatingUserID()
		{
			Assert(true);
		}

		public new void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			Assert(true);
		}

		public new void TestTransactionNumberOnSave()
		{
			Assert(true);
		}

		public new void TestDeleteTransactionInDBCausesException()
		{
			Assert(true);
		}

		public new void TestAH_OHSFilterResetJobTypeAndCurrency()
		{
			Assert(true);
		}

		public new void TestBatchInvoiceJobType()
		{
			Assert(true);
		}

		public new void TestCurrencySet()
		{
			Assert(true);
		}

		public new void TestDefaultCurrencySetByDebtor()
		{
			Assert(true);
		}

		public new void TestOrganisationSet()
		{
			Assert(true);
		}

		public new void TestReverseAmount()
		{
			Assert(true);
		}

		public new void TestSavingNewRecord()
		{
			Assert(true);
		}

		public new void TestTermsAndInvoiceTermsDaysSetFromOrgHeader()
		{
			Assert(true);
		}

		public new void TestInvoiceAndDueDateSetForIndividualInvoices()
		{
			Assert(true);
		}

		public new void TestDueDateSet()
		{
			Assert(true);
		}

		public new void TestPropertiesSetToReadOnlyOnSaved()
		{
			Assert(true);
		}

		public new void TestPropertiesSetToReadOnlyOnLoaded()
		{
			Assert(true);
		}

		public new void TestDefaultOrgAddressPK()
		{
			Assert(true);
		}

		public new void TestDefaultOrgAddressPK_LocalClient()
		{
			Assert(true);
		}

		public new void TestDefaultOrgAddressPK_OverseasAgent()
		{
			Assert(true);
		}

		public new void TestDisplayInvoiceAddressOverrides()
		{
			Assert(true);
		}

		public new void TestAH_OA_InvoiceAddressOverride()
		{
			Assert(true);
		}

		public new void TestDisplayInvoiceContactOverrides()
		{
			Assert(true);
		}

		public new void TestWhenOnSavingUpdateInvoiceDateOfInvoiceWithoutUpdateExRate()
		{
			Assert(true);
		}

		public new void TestAH_ExchangeRateWithOtherTaxes()
		{
			Assert(true);
		}

		public new void TestTransactionHeaderAuthorizationNumberReference()
		{
			Assert(true);
		}

		#endregion

		#region Implementation

		protected override Type TypeOfValidation
		{
			get { return typeof(InvoiceBulkBatchValidation); }
		}

		protected override Type TypeOfFilterObject
		{
			get { return typeof(InvoiceBulkBatchFilterBusinessObject); }
		}

		#endregion

		#region Assertion Helper Methods

		public void AssertBatchTotals(InvoiceBatchHeader batch, ZDecimal osTotal, ZDecimal gstAmount, ZDecimal invoiceAmount)
		{
			AssertEquals("Batch should have OSTotal: ", osTotal, batch.AH_OSTotal);
			AssertEquals("Batch should have GSTAmount: ", gstAmount, batch.AH_GSTAmount);
			AssertEquals("Batch should have InvoiceAmount: ", invoiceAmount, batch.AH_InvoiceAmount);
		}

		#endregion
	}
}
