using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TSGoodsShipmentTypeDocumentsAuthorisationsProviderTest : DataProviderTestCase<TSGoodsShipmentTypeDocumentsAuthorisationsProvider>
	{
		public void TestUCR()
		{
			header.MasterBill.ABL_UCRNumber = "QW918";
			AssertEquals("QW918", Provider.UCR);
		}

		public void TestSimplifiedDeclarationDocuments()
		{
			var docs = Provider.SimplifiedDeclarationDocuments;
			AssertEquals(0, docs.Count);

			var headerPreviousDoc1 = header.PreviousDocuments.AddNew();
			headerPreviousDoc1.CSI_Code = "100";
			docs = GetProvider().SimplifiedDeclarationDocuments;
			AssertEquals("Single previous document added to header", 1, docs.Count);
			Assert(docs.First() is ISimplifiedDeclarationDocumentWritingOff);
			AssertEquals("Header Previous Document 1 Type", "100", docs.First().PreviousDocumentType);

			var headerPreviousDoc2 = header.PreviousDocuments.AddNew();
			headerPreviousDoc2.CSI_Code = "200";
			docs = GetProvider().SimplifiedDeclarationDocuments;
			AssertEquals("2 previous documents added to header", 2, docs.Count);
			Assert(docs.Last() is ISimplifiedDeclarationDocumentWritingOff);
			AssertEquals("Header Previous Document 2 Type", "200", docs.Last().PreviousDocumentType);

			var billPreviousDoc1 = header.MasterBill.PreviousDocuments.AddNew();
			billPreviousDoc1.CSI_Code = "370";
			docs = GetProvider().SimplifiedDeclarationDocuments;
			AssertEquals("If previous documents are added to bill and header, only bill docs will be sent", 1, docs.Count);
			Assert(docs.First() is ISimplifiedDeclarationDocumentWritingOff);
			AssertEquals("Bill Previous Document 1 Type", "370", docs.First().PreviousDocumentType);

			var billPreviousDoc2 = header.MasterBill.PreviousDocuments.AddNew();
			billPreviousDoc2.CSI_Code = "888";
			docs = GetProvider().SimplifiedDeclarationDocuments;
			AssertEquals("Two previous documents added to bill", 2, docs.Count);
			Assert(docs.Last() is ISimplifiedDeclarationDocumentWritingOff);
			AssertEquals("Bill Previous Document 2 Type", "888", docs.Last().PreviousDocumentType);
		}

		public void TestAdditionalInformations()
		{
			_ = header.MasterBill.AdditionalInfos.AddNew();
			_ = header.MasterBill.AdditionalInfos.AddNew();

			AssertEquals(2, Provider.AdditionalInformations.Count);
			Assert(Provider.AdditionalInformations.First() is IAdditionalInformation);
		}

		public void TestProducedDocuments()
		{
			_ = header.MasterBill.SupportingDocuments.AddNew();
			_ = header.MasterBill.SupportingDocuments.AddNew();

			AssertEquals(2, Provider.ProducedDocuments.Count);
			Assert(Provider.ProducedDocuments.First() is IIdType);
		}

		public void TestWarehouse()
		{
			var authorizationUsage = Factory.New<CusAuthorizationUsage>();
			authorizationUsage.Parent = header;
			AssertType<WarehouseIdentificationProvider>(Provider.Warehouse);
		}

		protected override TSGoodsShipmentTypeDocumentsAuthorisationsProvider GetProvider() => new TSGoodsShipmentTypeDocumentsAuthorisationsProvider(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TemporaryStorageHeader>();
		}

		TemporaryStorageHeader header;
	}
}
