using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7ItemWrapper))]
public sealed class H7ItemWrapperTest : DataProviderTestCase<H7ItemWrapper>
{
	public void TestAdditionalInformation()
	{
		SetUpTests();
		var addInfo1 = item.AdditionalInfos.AddNew();
		addInfo1.CSI_Code = "00100";
		addInfo1.CSI_Description = "Description1";
		addInfo1.CSI_SubType = "INF";
		var addInfo2 = item.AdditionalInfos.AddNew();
		addInfo2.CSI_Code = "00200";
		addInfo2.CSI_Description = "Description2";
		addInfo2.CSI_SubType = "INF";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.AdditionalInformation);
			AssertEquals(2, wrapper.AdditionalInformation.Count);
			var wrapperAddInfo1 = wrapper.AdditionalInformation.First();
			AssertEquals("00100", wrapperAddInfo1.Code);
			AssertEquals("Description1", wrapperAddInfo1.Description);
			var wrapperAddInfo2 = wrapper.AdditionalInformation.Skip(1).First();
			AssertEquals("00200", wrapperAddInfo2.Code);
			AssertEquals("Description2", wrapperAddInfo2.Description);
		});
	}

	public void TestAdditionalReferences()
	{
		SetUpTests();
		var addRef1 = item.AdditionalInfos.AddNew();
		addRef1.CSI_Code = "00100";
		addRef1.CSI_ReferenceNumber = "Ref1";
		addRef1.CSI_SubType = "REF";
		var addRef2 = item.AdditionalInfos.AddNew();
		addRef2.CSI_Code = "00200";
		addRef2.CSI_ReferenceNumber = "Ref2";
		addRef2.CSI_SubType = "REF";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.AdditionalReferences);
			AssertEquals(2, wrapper.AdditionalReferences.Count);
			var wrapperAddRef1 = wrapper.AdditionalReferences.First();
			AssertEquals("00100", wrapperAddRef1.ReferenceType);
			AssertEquals("Ref1", wrapperAddRef1.ReferenceNumber);
			var wrapperAddRef2 = wrapper.AdditionalReferences.Skip(1).First();
			AssertEquals("00200", wrapperAddRef2.ReferenceType);
			AssertEquals("Ref2", wrapperAddRef2.ReferenceNumber);
		});
	}

	public void TestExporter()
	{
		SetUpTests();
		bill.ABL_ShipperName = "Shipper Name";
		bill.ABL_ShipperStreet1 = "Shipper Street 1";
		bill.ABL_ShipperStreet2 = "Street 2 ";
		bill.ABL_RN_NKShipperCountry = "AU";
		bill.ABL_ShipperPostcode = "2000";
		bill.ABL_ShipperCity = "ShipperSydney";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			var exporter = wrapper.Exporter;
			AssertNotNull(exporter);
			AssertSame("Cached", exporter, wrapper.Exporter);
			AssertNull(exporter.IdentificationNumber);
			AssertNotNull(exporter.Address);
			AssertEquals("Shipper Name", exporter.Address.Name);
			AssertEquals("Shipper Street 1 Street 2", exporter.Address.StreetAndNumber);
			AssertEquals("AU", exporter.Address.Country);
			AssertEquals("2000", exporter.Address.ZipCode);
			AssertEquals("ShipperSydney", exporter.Address.City);
		});
	}

	public void TestGoodsDescription()
	{
		SetUpTests();
		item.API_GoodsDescription = "Description of the goods";
		var wrapper = new H7ItemWrapper(item);
		AssertEquals("Description of the goods", wrapper.GoodsDescription);
	}

	public void TestGrossMass()
	{
		SetUpTests();
		item.API_GrossWeight = 980;
		item.API_GrossWeightUQ = "KG";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(H7ItemWrapper.GrossMass), 980m, wrapper.GrossMass);

			item.API_GrossWeightUQ = "G";
			AssertEquals(nameof(H7ItemWrapper.GrossMass), 0.98m, wrapper.GrossMass);
		});
	}

	public void TestHSCode()
	{
		SetUpTests();
		item.API_Tariff = "123456789";
		var wrapper = new H7ItemWrapper(item);
		AssertEquals("123456", wrapper.HSCode);
	}

	public void TestIntrinsicValue()
	{
		SetUpTests();
		item.API_GoodsValue = 123.45;
		item.API_RX_NKGoodsValueCurrency = "EUR";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.IntrinsicValue);
			var intrinsicValue = wrapper.IntrinsicValue;
			AssertSame("Cached", intrinsicValue, wrapper.IntrinsicValue);

			AssertEquals(123.45m, wrapper.IntrinsicValue.Amount);
			AssertEquals("EUR", wrapper.IntrinsicValue.Currency);
		});
	}

	public void TestItemNumber()
	{
		SetUpTests();
		item.SequenceNumber = 1;
		var wrapper = new H7ItemWrapper(item);
		AssertEquals(1, wrapper.ItemNumber);
	}

	public void TestMethodOfPayment()
	{
		SetUpTests();
		header.AMA_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
		var wrapper = new H7ItemWrapper(item);
		AssertEquals("CSH", wrapper.MethodOfPayment);
	}

	public void TestNumberOfPacks()
	{
		SetUpTests();
		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackQty = 7;
		pack1.APA_PackUQ = "G";
		pack1.APA_MarksAndNumbers = "pack1";
		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackQty = 1;
		pack2.APA_PackUQ = "KG";
		pack2.APA_MarksAndNumbers = "pack2";

		var packedItem = bill.PackedItems.AddNew();
		foreach (var link in packedItem.AsycudaPackPackedItemLinks)
		{
			link.IsLinked = true;
		}

		var pack3 = bill.Packs.AddNew();
		pack3.APA_PackQty = 6;
		pack3.APA_PackUQ = "G";
		pack3.APA_MarksAndNumbers = "unlinked pack";

		AssertEquals("Pre-condition: 3 packs in total", 3, packedItem.AsycudaPackPackedItemLinks.Count);

		var wrapper = new H7ItemWrapper(packedItem);

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.NumberOfPacks);
			var numberOfPacks = wrapper.NumberOfPacks;
			AssertSame("Cached", numberOfPacks, wrapper.NumberOfPacks);
			AssertEquals("Only linked pack should be included", 2, wrapper.NumberOfPacks.Count);
			AssertContainsExactElementsInAnyOrder("Packaging data", [7, 1], numberOfPacks);
		});
	}

	public void TestPreviousDocuments()
	{
		SetUpTests();

		var previousDoc1 = item.PreviousDocuments.AddNew();
		previousDoc1.CSI_Code = "123";
		previousDoc1.CSI_ReferenceNumber = "Ref1";
		var previousDoc2 = item.PreviousDocuments.AddNew();
		previousDoc2.CSI_Code = "234";
		previousDoc2.CSI_ReferenceNumber = "Ref2";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.PreviousDocuments);
			AssertEquals(2, wrapper.PreviousDocuments.Count);
			var wrapperPreviousDoc1 = wrapper.PreviousDocuments.First();
			AssertEquals("123", wrapperPreviousDoc1.DocumentType);
			AssertEquals("Ref1", wrapperPreviousDoc1.ReferenceNumber);
			var wrapperPreviousDoc2 = wrapper.PreviousDocuments.Skip(1).First();
			AssertEquals("234", wrapperPreviousDoc2.DocumentType);
			AssertEquals("Ref2", wrapperPreviousDoc2.ReferenceNumber);
		});
	}

	public void TestProcedure()
	{
		SetUpTests();
		bill.ABL_Procedure = "C07++F48+";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNull(wrapper.Procedure.Procedure);
			AssertNull(wrapper.Procedure.PreviousProcedure);
			AssertEquals("AdditionalProcedures count", 2, wrapper.Procedure.AdditionalProcedures.Count);
			AssertEquals("First AdditionalProcedure", "C07", wrapper.Procedure.AdditionalProcedures.First());
			AssertEquals("Second AdditionalProcedure", "F48", wrapper.Procedure.AdditionalProcedures.Skip(1).First());
		});
	}

	public void TestSupplementaryUnit()
	{
		SetUpTests();
		item.API_CustomsQty2 = 38;

		var wrapper = new H7ItemWrapper(item);
		AssertEquals(38m, wrapper.SupplementaryUnit);
	}

	public void TestSupportingDocuments()
	{
		SetUpTests();

		var supportingDoc1 = item.SupportingDocuments.AddNew();
		supportingDoc1.CSI_Code = "YYY";
		supportingDoc1.CSI_ReferenceNumber = "Ref1";
		var supportingDoc2 = item.SupportingDocuments.AddNew();
		supportingDoc2.CSI_Code = "ZZZ";
		supportingDoc2.CSI_ReferenceNumber = "Ref2";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.SupportingDocuments);
			AssertEquals(2, wrapper.SupportingDocuments.Count);
			var wrapperSupportingDoc1 = wrapper.SupportingDocuments.First();
			AssertEquals("YYY", wrapperSupportingDoc1.Code);
			AssertEquals("Ref1", wrapperSupportingDoc1.ReferenceNumber);
			var wrapperSupportingDoc2 = wrapper.SupportingDocuments.Skip(1).First();
			AssertEquals("ZZZ", wrapperSupportingDoc2.Code);
			AssertEquals("Ref2", wrapperSupportingDoc2.ReferenceNumber);
		});
	}

	public void TestTransportCosts()
	{
		SetUpTests();
		bill.ABL_InsuranceValue = 1200;
		bill.ABL_TransportValue = 34;
		bill.ABL_RX_NKTransportValueCurrency = "EUR";

		bill.PackedItems.AddNew();

		var expectedCostPerItem = 1234m / 2;
		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(wrapper.TransportCosts.Amount), expectedCostPerItem, wrapper.TransportCosts.Amount);
			AssertEquals(nameof(wrapper.TransportCosts.Currency), "EUR", wrapper.TransportCosts.Currency);
		});
	}

	public void TestTransportDocuments()
	{
		SetUpTests();

		var transportDoc1 = item.AdditionalInfos.AddNew();
		transportDoc1.CSI_Code = "00100";
		transportDoc1.CSI_ReferenceNumber = "Ref1";
		transportDoc1.CSI_SubType = "TRA";
		var transportDoc2 = item.AdditionalInfos.AddNew();
		transportDoc2.CSI_Code = "00200";
		transportDoc2.CSI_ReferenceNumber = "Ref2";
		transportDoc2.CSI_SubType = "TRA";

		var wrapper = new H7ItemWrapper(item);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TransportDocuments);
			AssertEquals(2, wrapper.TransportDocuments.Count);
			var wrapperTransportDoc1 = wrapper.TransportDocuments.First();
			AssertEquals("00100", wrapperTransportDoc1.DocumentType);
			AssertEquals("Ref1", wrapperTransportDoc1.ReferenceNumber);
			var wrapperTransportDoc2 = wrapper.TransportDocuments.Skip(1).First();
			AssertEquals("00200", wrapperTransportDoc2.DocumentType);
			AssertEquals("Ref2", wrapperTransportDoc2.ReferenceNumber);
		});
	}

	public void TestUcr()
	{
		SetUpTests();
		bill.ABL_UCRNumber = "UCR001";

		var wrapper = new H7ItemWrapper(item);
		AssertEquals("UCR001", wrapper.Ucr);
	}

	void SetUpTests()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		item = bill.PackedItems.AddNew();
	}

	protected override H7ItemWrapper GetProvider()
	{
		var pHeader = Factory.New<AsycudaManifestHeader>();
		var pBill = pHeader.Bills.AddNew();
		var pItem = pBill.PackedItems.AddNew();
		return new H7ItemWrapper(pItem);
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
	AsycudaPackedItem item;
}
