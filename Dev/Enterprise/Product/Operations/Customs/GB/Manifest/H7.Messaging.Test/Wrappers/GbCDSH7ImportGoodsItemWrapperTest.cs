using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public class GbCDSH7ImportGoodsItemWrapperTest : TestCaseWithFactory
	{
		public void TestGovernmentProcedures_NonBIRDS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "1H7", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "C07", ZString.Empty, ZString.Empty);
			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			var packedItem = bill.PackedItems.AddNew();
			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);

			var code = bill.AdditionalProcedureCodes.AddNew();
			code.CY_Code = "4000C07";

			var procedures = wrapper.GovernmentProcedures.ToList();

			CombineAssertions("GovernmentProcedures", () =>
			{
				AssertEquals(3, wrapper.GovernmentProcedures.Count());
				AssertEquals("CurrentCode", "40", procedures[0].CurrentCode);
				AssertEquals("PreviousCode", "00", procedures[0].PreviousCode);
				AssertEquals("1H7", procedures[1].CurrentCode);
				AssertEquals("C07", procedures[2].CurrentCode);
			});
		}

		public void TestGovernmentProcedures_BIRDS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "1H7", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "C07", ZString.Empty, ZString.Empty);
			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			var packedItem = bill.PackedItems.AddNew();
			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: true);

			var code = bill.AdditionalProcedureCodes.AddNew();
			code.CY_Code = "4000C07";

			CombineAssertions("GovernmentProcedures for BIRDS message is hard-coded", () =>
			{
				bill.ABL_RL_NKOrigin = "JE123";
				AssertContainsExactElementsInAnyOrder("15F code is appended for JE origin",
					new[] { ("00", "20"), ("21V", ""), ("15F", "") },
					wrapper.GovernmentProcedures.Select(p => (p.CurrentCode.ToString(), p.PreviousCode.ToString())));

				bill.ABL_RL_NKOrigin = "GG456";
				AssertContainsExactElementsInAnyOrder("15F code is appended for GG origin",
					new[] { ("00", "20"), ("21V", ""), ("15F", "") },
					wrapper.GovernmentProcedures.Select(p => (p.CurrentCode.ToString(), p.PreviousCode.ToString())));

				bill.ABL_RL_NKOrigin = "AU789";
				AssertContainsExactElementsInAnyOrder("No 15F code in other country",
					new[] { ("00", "20"), ("21V", ""), },
					wrapper.GovernmentProcedures.Select(p => (p.CurrentCode.ToString(), p.PreviousCode.ToString())));
			});
		}

		public void TestSeller()
		{
			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "UK");

			var shipper = shipperOrg.Addresses.AddNew();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Shipper = shipper.PK;
			bill.ABL_ShipperStreet1 = "Address1";
			bill.ABL_ShipperStreet2 = "Address2";
			bill.ABL_ShipperCity = "City";
			bill.ABL_RN_NKShipperCountry = "AU";
			bill.ABL_ShipperPostcode = "12345678";
			bill.ABL_ShipperName = "Shipper";

			var packedItem = bill.PackedItems.AddNew();
			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			var seller = wrapper.Seller;
			CombineAssertions("Seller", () =>
			{
				AssertEquals("Id", "UK001", seller.ID);
				AssertEquals("Name", "Shipper", seller.Name);
				AssertEquals("Country code", "AU", seller.Address.CountryCode);
				AssertEquals("Address line", "Address1Address2", seller.Address.Line);
				AssertEquals("City", "City", seller.Address.CityName);
				AssertEquals("Postcode ID", "12345678", seller.Address.PostcodeID);
			});
		}

		public void TestPackagings_NonBIRDSMessage()
		{
			var bill = Factory.New<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = 7;
			pack1.APA_PackUQ = "G";
			pack1.APA_MarksAndNumbers = "pack 1";
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 1;
			pack2.APA_PackUQ = "KG";
			pack2.APA_MarksAndNumbers = "pack 2";

			var packedItem = bill.PackedItems.AddNew();

			foreach (var link in packedItem.AsycudaPackPackedItemLinks)
			{
				link.IsLinked = true;
			}

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);

			AssertNull("Should not provide packagings collection for non BIRDS message", wrapper.Packagings);
		}

		public void TestPackagings_BIRDSMessage()
		{
			var bill = Factory.New<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = 7;
			pack1.APA_PackUQ = "G";
			pack1.APA_MarksAndNumbers = "pack 1";
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 1;
			pack2.APA_PackUQ = "KG";
			pack2.APA_MarksAndNumbers = "pack 2";

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

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: true);
			var packagings = wrapper.Packagings;

			CombineAssertions("Should provide packagings collection for BIRDS message", () =>
			{
				AssertEquals("Only linked pack should be included", 2, packagings.Count());
				AssertContainsExactElementsInAnyOrder("Packaging data",
					new[] { ("G", 7m, "pack 1"), ("KG", 1m, "pack 2") },
					packagings.Select(p => ((string)p.TypeCode, (decimal)p.Quantity, (string)p.MarksNumbersID)));
			});
		}

		public void TestDescription()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_GoodsDescription = "GoodsDescription";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			AssertEquals("GoodsDescription", wrapper.Description);
		}

		public void TestClassifications()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_Tariff = "1234567";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			CombineAssertions("Classifications", () =>
			{
				AssertEquals(1, wrapper.Classifications.Count());
				AssertEquals("ID", "123456", wrapper.Classifications.First().ID);
				AssertEquals("TypeCode", "TSP", wrapper.Classifications.First().TypeCode);
			});
		}

		public void TestGrossWeight()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_GrossWeight = 1000300;
			packedItem.API_GrossWeightUQ = "G";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			AssertEquals(1000.3m, wrapper.GrossWeight);
		}

		public void TestNetWeight()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_NetWeight = 600;
			packedItem.API_NetWeightUQ = "G";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			AssertEquals(0.6m, wrapper.NetWeight);
		}

		public void TestTariffQuantity()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_CustomsQty2 = 10.000000;

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			AssertEquals(10m, wrapper.TariffQuantity);
		}

		public void TestInvoiceLineItemCharge()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_CustomsValue = 10;
			bill.ABL_RX_NKCustomsValueCurrency = "USD";

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_GoodsValue = 30;
			packedItem.API_RX_NKGoodsValueCurrency = "AUD";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			CombineAssertions("InvoiceLineItemCharge", () =>
			{
				AssertEquals("Amount", 30m, wrapper.InvoiceLineItemCharge.Amount);
				AssertEquals("Currency", "AUD", wrapper.InvoiceLineItemCharge.Currency);
			});
		}

		public void TestPreviousDocuments()
		{
			var bill = Factory.New<AsycudaBill>();
			var previousDocument0 = bill.PreviousDocuments.AddNew();
			previousDocument0.CSI_SubType = "A";
			previousDocument0.CSI_Code = "333";
			previousDocument0.CSI_ReferenceNumber = "654321";

			var packedItem = bill.PackedItems.AddNew();
			var previousDocument1 = packedItem.PreviousDocuments.AddNew();
			previousDocument1.CSI_SubType = "Z";
			previousDocument1.CSI_Code = "380";
			previousDocument1.CSI_ReferenceNumber = "123456";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);

			CombineAssertions("PreviousDocument", () =>
			{
				AssertEquals(2, wrapper.PreviousDocuments.Count());
				AssertEquals("CategoryCode", "A", wrapper.PreviousDocuments.ElementAtOrDefault(0).CategoryCode);
				AssertEquals("TypeCode", "333", wrapper.PreviousDocuments.ElementAtOrDefault(0).TypeCode);
				AssertEquals("ID", "654321", wrapper.PreviousDocuments.ElementAtOrDefault(0).ID);
				AssertEquals("CategoryCode", "Z", wrapper.PreviousDocuments.ElementAtOrDefault(1).CategoryCode);
				AssertEquals("TypeCode", "380", wrapper.PreviousDocuments.ElementAtOrDefault(1).TypeCode);
				AssertEquals("ID", "123456", wrapper.PreviousDocuments.ElementAtOrDefault(1).ID);
			});
		}

		public void TestAdditionalInfo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			var billAdditionalInfo = bill.AdditionalInfos.AddNew();
			billAdditionalInfo.CSI_Code = "390";
			billAdditionalInfo.CSI_Description = "My Description";

			var packItemAdditionalInfo = packedItem.AdditionalInfos.AddNew();
			packItemAdditionalInfo.CSI_Code = "380";
			packItemAdditionalInfo.CSI_Description = "An intense dissatisfaction with the world";

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);

			var statements = wrapper.AdditionalInformations.ToList();
			CombineAssertions("AdditionalInfo", () =>
			{
				AssertEquals(2, wrapper.AdditionalInformations.Count());
				AssertEquals("Statement", "380", statements[0].Statement);
				AssertEquals("StatementText", "An intense dissatisfaction with the world", statements[0].StatementText);
				AssertEquals("Statement", "390", statements[1].Statement);
				AssertEquals("StatementText", "My Description", statements[1].StatementText);
			});
		}

		public void TestAdditionalDocuments()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var itemSupportingDocument = packedItem.SupportingDocuments.AddNew();
			var billSupportingDocument = bill.SupportingDocuments.AddNew();

			billSupportingDocument.CSI_Code = "109C";
			billSupportingDocument.CSI_ReferenceNumber = "My Reference";
			billSupportingDocument.CSI_ReferenceNumber2 = "Auth";
			billSupportingDocument.CSI_Availability = "A";
			billSupportingDocument.CSI_Actions = "C";
			billSupportingDocument.CSI_Description = "My Description";
			billSupportingDocument.CSI_DateOfIssue = new ZDateTime(2013, 01, 02);

			itemSupportingDocument.CSI_Code = "108C";
			itemSupportingDocument.CSI_ReferenceNumber = "An intense dissatisfaction with the world";
			itemSupportingDocument.CSI_ReferenceNumber2 = "about it";
			itemSupportingDocument.CSI_Availability = "A";
			itemSupportingDocument.CSI_Actions = "C";
			itemSupportingDocument.CSI_Description = "And a compulsion to do something";
			itemSupportingDocument.CSI_DateOfIssue = new ZDateTime(2013, 01, 01);

			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);
			var documents = wrapper.AdditionalDocuments.ToList();

			CombineAssertions("AdditionalInfo", () =>
			{
				AssertEquals(2, wrapper.AdditionalDocuments.Count());

				AssertEquals("CategoryCode", "1", documents[0].CategoryCode);
				AssertEquals("TypeCode", "08C", documents[0].TypeCode);
				AssertEquals("ID", "An intense dissatisfaction with the world", documents[0].ID);
				AssertEquals("LPCOExemptionCode", "AC", documents[0].LPCOExemptionCode);
				AssertEquals("Name", "And a compulsion to do something", documents[0].Name);
				AssertEquals("Submitter", "about it", documents[0].Submitter);
				AssertEquals("EffectiveDateTime", new ZDateTime(2013, 01, 01), documents[0].EffectiveDateTime);

				AssertEquals("CategoryCode", "1", documents[1].CategoryCode);
				AssertEquals("TypeCode", "09C", documents[1].TypeCode);
				AssertEquals("ID", "My Reference", documents[1].ID);
				AssertEquals("LPCOExemptionCode", "AC", documents[1].LPCOExemptionCode);
				AssertEquals("Name", "My Description", documents[1].Name);
				AssertEquals("Submitter", "Auth", documents[1].Submitter);
				AssertEquals("EffectiveDateTime", new ZDateTime(2013, 01, 02), documents[1].EffectiveDateTime);
			});
		}
	}
}
