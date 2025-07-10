using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobDraftInvoicePrintingFilter))]
	public class JobDraftInvoicePrintingFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTransactionTypeList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(shipment);

			AssertEquals(2, printingFilter.TransactionTypeList.Count);
			AssertEquals(true, printingFilter.TransactionTypeList.ContainsCode(TransactionTypes.Invoice));
			AssertEquals(true, printingFilter.TransactionTypeList.ContainsCode(TransactionTypes.CreditNote));
		}

		public void TestUseReadOnlyFactory()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(shipment);
			AssertNotEquals(printingFilter.Factory, printingFilter.JobParent.Factory);
		}

		public void TestRefreshInvoiceListWhenNoJobExist()
		{
			var dataFactory = new BusinessObjectFactory();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			dataFactory.NewWithValidTestData<AccDraftInvoiceHeader>();

			dataFactory.Save();

			printingFilter.RefreshInvoiceList();
			AssertEquals("Shouldn't be any transactions in list", 0, printingFilter.Transactions.Count);
		}

		public void TestResetInvoiceList()
		{
			Factory.Save();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			printingFilter.Transactions.RemoveAll();
			AssertEquals("Invoice Count", 0, printingFilter.Transactions.Count);

			printingFilter.ResetInvoiceList();
			AssertEquals("Creditor", ZGuid.Empty, printingFilter.Creditor);
			AssertEquals("Transaction Type", ZString.Empty, printingFilter.TransactionType);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);
		}

		public void TestRefreshInvoiceListWontLoadInvoiceOfOtherCompany()
		{
			Factory.Save();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			var draftInvoice = CreateDraftInvoice(Shipment.PK, Factory.NewWithValidTestData<OrgHeader>().PK, TransactionTypes.Invoice);
			draftInvoice.AIH_GC_Company = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();

			printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Should be 7, not including one of other company.", 7, printingFilter.Transactions.Count);
		}

		public void TestRefreshInvoiceListWontLoadPostedInvoice()
		{
			Factory.Save();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			var draftInvoice = CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.Invoice);
			Factory.Save();

			printingFilter.RefreshInvoiceList();
			AssertEquals("PreRequisite", 8, printingFilter.Transactions.Count);

			var postedInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			postedInvoice.AH_TransactionType = TransactionTypes.Invoice;
			draftInvoice.AIH_AH_PostedTransactionHeader = postedInvoice.PK;

			Factory.Save();

			AssertEquals("PreRequisite", "PST", draftInvoice.PostingStatus);

			printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Should be 7, not including posted one.", 7, printingFilter.Transactions.Count);
		}

		public void TestRefreshInvoiceListWithShipment()
		{
			Factory.Save();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, 5);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, 3);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, 1);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.AdjustmentNote, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.AdjustmentNote, 0);

			AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.AdjustmentNote, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, 7);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, 4);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, 2);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.AdjustmentNote, 1);
		}

		public void TestRefreshInvoiceListWithShipmentAndLinkedConsol()
		{
			Consol.Shipments.Add(Shipment);
			Factory.Save();

			var printingFilter = CreateJobDraftInvoicePrintingFilter(Shipment);
			AssertEquals("Invoice Count", 13, printingFilter.Transactions.Count);

			CombineAssertions("Shipment should also containts related consol's draft invoice.", () =>
			{
				AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, 7);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, 4);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, 2);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.AdjustmentNote, 1);

				AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, 2);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, 1);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, 1);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.AdjustmentNote, 0);

				AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, 4);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, 3);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, 0);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.AdjustmentNote, 1);

				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, 13);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, 8);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, 3);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.AdjustmentNote, 2);
			});
		}

		public void TestRefreshInvoiceListWithConsol()
		{
			Factory.Save();
			var printingFilter = CreateJobDraftInvoicePrintingFilter(Consol);
			AssertEquals("Invoice Count", 6, printingFilter.Transactions.Count);

			CombineAssertions("Org specified Invoice Count", () =>
			{
				AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, 2);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, 1);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, 1);
				AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.AdjustmentNote, 0);

				AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, 0);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, 0);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, 0);
				AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.AdjustmentNote, 0);

				AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, 4);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, 3);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, 0);
				AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.AdjustmentNote, 1);

				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, 6);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, 4);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, 1);
				AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.AdjustmentNote, 1);
			});
		}

		void AssertRefreshInvoiceList(JobDraftInvoicePrintingFilter filter, ZGuid creditorPK, ZString transactionType, int count)
		{
			filter.ResetInvoiceList();
			filter.Creditor = creditorPK;
			filter.TransactionType = transactionType;
			filter.RefreshInvoiceList();
			AssertEquals("Invoice List Count", count, filter.Transactions.Count);
		}

		#region Create Business Objects

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateJobDraftInvoicePrintingFilter(Consol);
		}

		OrgHeader CreateOrgHeader(string code)
		{
			var newFactory = new BusinessObjectFactory();

			var header = newFactory.New<OrgHeader>();
			header.OH_Code = code;
			newFactory.Save();

			return header;
		}

		AccDraftInvoiceHeader CreateDraftInvoice(ZGuid jobParentPK, ZGuid creditorPK, ZString transactionType)
		{
			var header = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var draftInvoiceJobCluster = Factory.NewWithValidTestData<AccDraftInvoiceJobCluster>();
			var draftInvoiceJob = Factory.NewWithValidTestData<AccDraftInvoiceJob>();

			draftInvoiceJobCluster.AIC_AIH_Header = header.PK;

			draftInvoiceJob.AIJ_ParentID = jobParentPK;
			draftInvoiceJob.AIJ_AIC_Cluster = draftInvoiceJobCluster.PK;

			header.AIH_OH_Creditor = creditorPK;
			header.AIH_TransactionType = transactionType;

			return header;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			Consol = TestObjectCreator.CreateConsol();
			Shipment = TestObjectCreator.CreateShipment("S01");

			Org1 = CreateOrgHeader("Creditor1");
			Org2 = CreateOrgHeader("Creditor2");
			Org3 = CreateOrgHeader("Creditor3");

			CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Shipment.PK, Org2.PK, TransactionTypes.CreditNote);
			CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Shipment.PK, Org2.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.CreditNote);
			CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Shipment.PK, Org1.PK, TransactionTypes.AdjustmentNote);

			CreateDraftInvoice(Consol.PK, Org1.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Consol.PK, Org1.PK, TransactionTypes.CreditNote);
			CreateDraftInvoice(Consol.PK, Org3.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Consol.PK, Org3.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Consol.PK, Org3.PK, TransactionTypes.Invoice);
			CreateDraftInvoice(Consol.PK, Org3.PK, TransactionTypes.AdjustmentNote);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		JobDraftInvoicePrintingFilter CreateJobDraftInvoicePrintingFilter(BusinessObject jobParent)
		{
			return new JobDraftInvoicePrintingFilter(jobParent);
		}

		ForwardingConsol Consol;
		ForwardingShipment Shipment;

		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
	}
}
