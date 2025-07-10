using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Chile.Testing
{
	public class ChileEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		#region TransactionPivot

		public void TestTransactionPivotIsNull()
		{
			var batch = Factory.New<AccEInvoicingBatch>();

			var additionalDataItemsProvider = new ChileEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 0, additionalDataItems.Count);
		}

		public void TestTransactionPivotHasMoreThanOnePivot_MustTakeFirstOne()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var invoice = Factory.New<ARInvoice>();
			var invoice2 = Factory.New<ARInvoice>();

			Factory.Save();

			var batch = CreateInvoicePivotAndBatch(invoice.PK, companyPK);

			AddPivotToBatch(batch, invoice2.PK);

			AssertAdditionalDataItemValue(batch, invoice.InvoiceTransactionReference);
		}

		#endregion

		public void TestParentTransactionHeaderIsNull()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = companyPK;

			var pivot = batch.TransactionPivots.AddNew();
			pivot.SetCompanyAndCountryCode(batch.Company);

			var additionalDataItemsProvider = new ChileEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 0, additionalDataItems.Count);
		}

		#region FolioCliente

		public void TestGetFolioCliente_WhenInvoiceTransactionReferenceIsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var invoice = Factory.New<ARInvoice>();

			var batch = CreateInvoicePivotAndBatch(invoice.PK, companyPK);

			var additionalDataItemsProvider = new ChileEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("Number Of Elements", 0, additionalDataItems.Count);
		}

		public void TestGetFolioCliente_WhenInvoiceTransactionReferenceNotIsEmpty()
		{
			var companyPK = Factory.New<GlbCompany>().PK;

			var invoice = Factory.New<ARInvoice>();
			Factory.Save();

			var batch = CreateInvoicePivotAndBatch(invoice.PK, companyPK);

			AssertAdditionalDataItemValue(batch, invoice.InvoiceTransactionReference);
		}

		#endregion

		#region Implementation 

		void AssertAdditionalDataItemValue(AccEInvoicingBatch batch, string expectedFolioCliente)
		{
			var additionalDataItemsProvider = new ChileEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, Factory.New<GlbBranch>(), new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Common.Logger());

			AssertEquals("PreCondicion: Number Of Elements", 1, additionalDataItems.Count);

			AssertEquals("FolioCliente Key", "FolioCliente", additionalDataItems[0].Key);
			AssertEquals("FolioCliente Value", expectedFolioCliente, additionalDataItems[0].Value);
		}

		AccEInvoicingBatch CreateInvoicePivotAndBatch(ZGuid invoicePk, ZGuid company)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = company;

			AddPivotToBatch(batch, invoicePk);

			return batch;
		}

		void AddPivotToBatch(AccEInvoicingBatch batch, ZGuid invoicePk)
		{
			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoicePk;
			pivot.SetCompanyAndCountryCode(batch.Company);
		}

		#endregion
	}
}
