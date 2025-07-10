using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBulkBatchFilterBusinessObject))]
	public class InvoiceBulkBatchFilterBusinessObjectTest : InvoiceBatchHeaderFilterBusinessObjectTest
	{
		#region Order By

		public void TestOrderBy()
		{
			AssertEquals("Query should containd OrderBy clause", "AH_OH", TestARFilterBizO.Filter.OrderBy);
		}

		#endregion

		#region Job Type

		public void TestFilterByJobType_OrgModuleType()
		{
			Factory.Save();

			InvoiceTobeIncluded = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceTobeExcluded = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceTobeIncluded.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			InvoiceTobeExcluded.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;

			OrgInvoiceType invoiceTypeMSC = TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			invoiceTypeMSC.PI_Module = InvoiceTypeModuleList.Codes.MSC;
			InvoiceTobeIncluded.AH_OH = TestObjectCreator.AALSHI.PK;

			OrgInvoiceType invoiceTypeFWD = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceTypeFWD.PI_Module = InvoiceTypeModuleList.Codes.FWD;
			InvoiceTobeExcluded.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			InvoiceBulkBatch parentBulkBatchHeader = Factory.NewWithValidTestData<InvoiceBulkBatch>();
			parentBulkBatchHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			TestARFilterBizO.SetParent(parentBulkBatchHeader);
			parentBulkBatchHeader.JobTypeList[parentBulkBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			AssertJobTypeFilterResult();
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoiceBulkBatchFilterBusinessObject();
		}

		#endregion
	}
}
