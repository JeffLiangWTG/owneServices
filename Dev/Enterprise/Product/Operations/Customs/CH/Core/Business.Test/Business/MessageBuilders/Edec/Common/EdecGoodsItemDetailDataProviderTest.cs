using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecGoodsItemDetailDataProviderTest : TestCaseWithFactory
{
	public void TestDetailsForVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_ModelName = "879";
		vehicle.CVH_VehicleIdentificationNumber = "UU6JA69691D713820";
		vehicle.CVH_RegistrationNumber = "672141217";

		var goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 3, goodsItemDetails.Count());
			AssertEquals(true, goodsItemDetails.FirstOrDefault() is IEdecGoodsItemDetail);
			AssertEquals("Detail Name should be: ", "1", goodsItemDetails.ElementAt(0).Name);
			AssertEquals("Detail Value should be: ", "879", goodsItemDetails.ElementAt(0).Value);
			AssertEquals("Detail Name should be: ", "2", goodsItemDetails.ElementAt(1).Name);
			AssertEquals("Detail Value should be: ", "UU6JA69691D713820", goodsItemDetails.ElementAt(1).Value);
			AssertEquals("Detail Name should be: ", "3", goodsItemDetails.ElementAt(2).Name);
			AssertEquals("Detail Value should be: ", "672141217", goodsItemDetails.ElementAt(2).Value);
		});
	}

	public void TestDetailsForTobacco()
	{
		var tobacco = invoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigars;
		tobacco.CSI_SubType = "01";
		tobacco.CSI_Description = nameof(TobaccoMainGroupCodes.Cigars);
		tobacco.CSI_ItemNumber = 1;
		tobacco.CSI_Value = 100d;
		tobacco.CSI_AdditionalDescription = "5";

		var goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 6, goodsItemDetails.Count());
			AssertEquals(true, goodsItemDetails.FirstOrDefault() is IEdecGoodsItemDetail);
			AssertEquals("Detail Name should be: ", "7", goodsItemDetails.ElementAt(0).Name);
			AssertEquals("Detail Value should be: ", TobaccoMainGroupCodes.Cigars, goodsItemDetails.ElementAt(0).Value);
			AssertEquals("Detail Name should be: ", "8", goodsItemDetails.ElementAt(1).Name);
			AssertEquals("Detail Value should be: ", "01", goodsItemDetails.ElementAt(1).Value);
			AssertEquals("Detail Name should be: ", "9", goodsItemDetails.ElementAt(2).Name);
			AssertEquals("Detail Value should be: ", nameof(TobaccoMainGroupCodes.Cigars), goodsItemDetails.ElementAt(2).Value);
			AssertEquals("Detail Name should be: ", "10", goodsItemDetails.ElementAt(3).Name);
			AssertEquals("Detail Value should be: ", "1", goodsItemDetails.ElementAt(3).Value);
			AssertEquals("Detail Name should be: ", "24", goodsItemDetails.ElementAt(4).Name);
			AssertEquals("Detail Value should be: ", "100", goodsItemDetails.ElementAt(4).Value);
			AssertEquals("Detail Name should be: ", "25", goodsItemDetails.ElementAt(5).Name);
			AssertEquals("Detail Value should be: ", "5", goodsItemDetails.ElementAt(5).Value);
		});
	}

	public void TestDetailsForGeneralDataEntry()
	{
		var additionalInfo = invoiceLine.AdditionalInformations.AddNew();
		additionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
		additionalInfo.CSI_ReferenceNumber = RefCusCodeTestHelper.ValidFreeZoneTrafficCode;

		var goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 1, goodsItemDetails.Count());

			var goodsItemDetail = goodsItemDetails.First();
			AssertEquals(true, goodsItemDetail is IEdecGoodsItemDetail);
			AssertEquals("Detail Name should be: ", UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic, goodsItemDetail.Name);
			AssertEquals("Detail Value should be: ", RefCusCodeTestHelper.ValidFreeZoneTrafficCode, goodsItemDetail.Value);
		});
	}

	public void TestEmptyDetail()
	{
		var additionalInfo = invoiceLine.AdditionalInformations.AddNew();
		additionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
		additionalInfo.CSI_ReferenceNumber = "";

		var goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine);
		AssertEquals("Detail list should be empty", 0, goodsItemDetails.Count());

		additionalInfo.CSI_ReferenceNumber = RefCusCodeTestHelper.ValidFreeZoneTrafficCode;
		goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine);
		AssertEquals("Detail list shouldn't be empty", 1, goodsItemDetails.Count());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
}
