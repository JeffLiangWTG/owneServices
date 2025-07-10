namespace Enterprise.Accounting.Business.Riba.Testing
{
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(AccCollectionOrderDocumentSupporter))]
	public class AccCollectionOrderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetContactOrganisation()
		{
			var supporter = new AccCollectionOrderDocumentSupporter(order1);
			order1.ACO_OH_Debtor = ZGuid.Empty;

			AssertNull("No contact org yet", supporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ANY).OrgHeader);
			order1.ACO_OH_Debtor = TestObjectCreator.Debtor.PK;

			AssertEquals("Contact Org for Document not null", TestObjectCreator.Debtor.PK, supporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestDocumentSupporter()
		{
			AssertEquals("Document Supporter should be of type", typeof(AccCollectionOrderDocumentSupporter), order1.DocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext CollectionOrder is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.CollectionOrder))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(CargoWise.Definitions.BusinessContext.CollectionOrder, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<AccCollectionOrder>();
		}

		AccCollectionOrderDocumentSupporter DocumentSupporter
		{
			get
			{
				var order = Factory.NewWithValidTestData<AccCollectionOrder>();
				return new AccCollectionOrderDocumentSupporter(order);
			}
		}

		#endregion

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccBankAccount bankAccount;
		ARInvoice invoice1;
		protected override void SetUp()
		{
			base.SetUp();
			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 100m, false);
			invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			order1 = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "0000001", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order1, invoice1, false);
			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
	}
}
