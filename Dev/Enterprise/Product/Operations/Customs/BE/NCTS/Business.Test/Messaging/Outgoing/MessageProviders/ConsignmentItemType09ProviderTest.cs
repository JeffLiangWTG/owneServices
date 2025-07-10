using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class ConsignmentItemType09ProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentItemType09Provider>
	{
		public void TestGoodsItemNumber()
		{
			item.BY_LineNo = 3;
			AssertEquals(3, Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			item.BY_DeclarationGoodsItemNumber = 4;
			AssertEquals(4, Provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationType()
		{
			item.BY_Type = "D";
			AssertEquals("D", Provider.DeclarationType);
		}

		public void TestCountryOfDispatch()
		{
			item.BY_RN_NKCountryOfDispatch = "BE";
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			item.BY_RN_NKCountryOfDestination = "DE";
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.CountryOfDestination);
		}

		public void TestReferenceNumberUCR()
		{
			item.BY_CommercialReferenceNumber = "ref";
			AssertEquals("ref", Provider.ReferenceNumberUCR);
		}

		public void TestConsignee()
		{
			Factory.CreateJobDocAddress("CEA", parent: item);
			AssertNotNull(Provider.Consignee);
		}

		public void TestAdditionalSupplyChainActors()
		{
			Factory.CreateCusReference<CusSupplyChainActorReference>("", "SCA", parent: item);
			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestCommodity()
		{
			AssertNotNull(Provider.Commodity);
		}

		public void TestPreviousDocuments()
		{
			Factory.CreateCusSupportingInfo("PRE", null, parent: item);
			AssertEquals(1, Provider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			Factory.CreateCusSupportingInfo("SUP", null, parent: item);
			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: item, reference: "Additional Reference");
			Factory.CreateCusSupportingInfo("OTH", "INF", parent: item, reference: "Additional Information");
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: item, reference: "Transport Document");
			AssertEquals("Additional Reference", Provider.AdditionalReferences.Single().ReferenceNumber);
		}

		public void TestAdditionalInformation()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: item, reference: "Additional Reference");
			var supportingInfo = Factory.CreateCusSupportingInfo("OTH", "INF", parent: item, reference: "Additional Information");
			supportingInfo.CSI_Description = "Additional Information Desc";
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: item, reference: "Transport Document");
			AssertEquals("Additional Information Desc", Provider.AdditionalInformation.Single().Text);
		}

		public void TestPackagings()
		{
			item.Packages.AddNew();
			AssertEquals(1, Provider.Packagings.Count);
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: item, reference: "Additional Reference");
			Factory.CreateCusSupportingInfo("OTH", "INF", parent: item, reference: "Additional Information");
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: item, reference: "Transport Document");
			AssertEquals("Transport Document", Provider.TransportDocuments.Single().ReferenceNumber);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			item.BY_TransportChargesMethodOfPayment = "C";
			AssertEquals("C", Provider.TransportChargesMethodOfPayment);
		}

		protected override ConsignmentItemType09Provider GetProvider() => provider;
		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			item = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			provider = new ConsignmentItemType09Provider(item);
		}

		NctsDepartureCargoDesc item;
		ConsignmentItemType09Provider provider;
	}
}
