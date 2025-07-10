using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015HouseConsignmentTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015HouseConsignment>
	{
		protected override IE013AndIE015HouseConsignment GetProvider() => new IE013AndIE015HouseConsignment(bill);

		public void TestCountryOfDispatch()
		{
			bill.B0_RN_NKCountryOfExport = "IE";
			AssertEquals("CountryOfDispatch", "IE", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			bill.B0_RN_NKCountryOfDestination = "FR";
			AssertEquals("CountryOfDispatch", "FR", Provider.CountryOfDestination);
		}

		public void TestGrossMass()
		{
			bill.B0_Weight = 1;
			AssertEquals("GrossMass", new decimal(1), Provider.GrossMass);
		}

		public void TestReferenceNumberUCR()
		{
			bill.B0_ReferenceID = "REF";
			AssertEquals("ReferenceNumberUCR", "REF", Provider.ReferenceNumberUCR);
		}

		public void TestConsignor()
		{
			SetupConsignor();
			AssertEquals("Consignor", "Org1", Provider.Consignor.Name);
		}

		void SetupConsignor()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Org1";
			var consignor = bill.Consignor;
			consignor.E2_OA_Address = org.MainAddress.PK;
		}

		public void TestConsignee()
		{
			SetupConsignee();
			AssertEquals("Consignee", "Org2", Provider.Consignee.Name);
		}

		void SetupConsignee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Org2";
			var consignee = bill.Consignee;
			consignee.E2_OA_Address = org.MainAddress.PK;
		}

		public void TestAdditionalSupplyChainActors()
		{
			var cusSupplyChainActorReference = bill.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference.CFR_Code = "CFR";
			cusSupplyChainActorReference.CFR_Reference = "IE123456789";

			AssertEquals("Role", "CFR", Provider.AdditionalSupplyChainActors.First().Role);
			AssertEquals("ID", "IE123456789", Provider.AdditionalSupplyChainActors.First().ID);
		}

		public void TestDepartureTransportMeans()
		{
			AssertType<ITransportMeans[]>("Departure Transport Means", Provider.DepartureTransportMeans);
		}

		public void TestPreviousDocuments()
		{
			AssertNull("Previous Documents", Provider.PreviousDocuments.FirstOrDefault());

			bill.PreviousDocuments.AddNew();
			AssertType<PreviousDocumentProvider>("Previous Documents", GetProvider().PreviousDocuments.FirstOrDefault());
		}

		public void TestSupportingDocuments()
		{
			AssertNull("Supporting Documents", Provider.SupportingDocuments.FirstOrDefault());

			bill.SupportingDocuments.AddNew();
			AssertType<SupportingDocumentProvider>("Supporting Documents", GetProvider().SupportingDocuments.FirstOrDefault());
		}

		public void TestAdditionalReferences()
		{
			AssertNull("Additional References", Provider.AdditionalReferences.FirstOrDefault());
			SetupAdditionalDocuments(bill);
			var additionalReference = GetProvider().AdditionalReferences.FirstOrDefault();
			AssertEquals("Type", "REF1", additionalReference.Type);
			AssertEquals("Reference", "REF_REF", additionalReference.Reference);
		}

		public void TestTransportDocuments()
		{
			AssertNull("Transport Documents", Provider.TransportDocuments.FirstOrDefault());

			SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA1", "TRA_REF");
			SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
			SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
			SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA3", "TRA_REF");

			var transportDocuments = GetProvider().TransportDocuments.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("count", 3, transportDocuments.Length);
				AssertDocument(transportDocuments[0], "TRA1", "TRA_REF");
				AssertDocument(transportDocuments[1], "TRA2", "TRA_REF");
				AssertDocument(transportDocuments[2], "TRA3", "TRA_REF");
			});
		}

		void AssertDocument(IDocument document, string expectedType, string expectedReference)
		{
			AssertEquals("Type", expectedType, document.Type);
			AssertEquals("Reference", expectedReference, document.Reference);
		}

		void SetupTransportDocument(AdditionalInfo info, ZString code, ZString referenceNumber)
		{
			info.CSI_SubType = "TRA";
			info.CSI_Code = code;
			info.CSI_ReferenceNumber = referenceNumber;
		}

		public void TestAdditionalInformations()
		{
			AssertNull("Additinoal Informations", Provider.AdditionalInformations.FirstOrDefault());
			SetupAdditionalDocuments(bill);
			var additionalInformation = GetProvider().AdditionalInformations.FirstOrDefault();
			AssertEquals("Code", "INF1", additionalInformation.Code);
			AssertEquals("Text", "INF_REF", additionalInformation.Text);
		}

		void SetupAdditionalDocuments(NctsBill bill)
		{
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = "INF";
			additionalDocument.CSI_Code = "INF1";
			additionalDocument.CSI_Description = "INF_REF";
			var transportDocument = bill.AdditionalDocuments.AddNew();
			transportDocument.CSI_SubType = "TRA";
			transportDocument.CSI_Code = "TRA1";
			transportDocument.CSI_ReferenceNumber = "TRA_REF";
			var additionalReference = bill.AdditionalDocuments.AddNew();
			additionalReference.CSI_SubType = "REF";
			additionalReference.CSI_Code = "REF1";
			additionalReference.CSI_ReferenceNumber = "REF_REF";
		}

		public void TestMethodOfPayment()
		{
			bill.B0_TransportPaymentMethod = "1";
			AssertEquals("MethodOfPayment", "1", Provider.MethodOfPayment);
		}

		public void TestConsignmentItems()
		{
			bill.GoodsItems.AddNew().BY_LineNo = 1;
			bill.GoodsItems.AddNew().BY_LineNo = 2;
			AssertEquals("ConsignmentItems", 2, Provider.ConsignmentItems.Count);
			var items = Provider.ConsignmentItems.ToArray();
			AssertEquals("ConsignmentItems", (short)1, items[0].GoodsItemNumber);
			AssertEquals("ConsignmentItems", (short)2, items[1].GoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
		}
		NctsBill bill;
		NctsHeader nctsHeader;
	}
}
