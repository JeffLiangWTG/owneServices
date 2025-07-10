using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class EUH7AsycudaForCustomsDeclarationDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestWriterDataMapping()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			bill.ABL_BillNumber = "BN123";
			bill.ABL_GoodsDescription = "goods description";
			bill.ABL_GrossWeight = 0.9;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 1.1;
			bill.ABL_VolumeUQ = "L";
			bill.ABL_ManifestQty = 13;
			bill.ABL_GoodsValue = 110;
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_SellerRegNo = "1234";
			bill.ABL_Incoterm = IncoTerms.Other;

			pack1.APA_PackUQ = "BX";
			pack2.APA_PackUQ = "FR";
			item1.API_FormattedTariff = "11081990009";
			item1.API_GoodsDescription = "item1 goods description";
			item1.API_RN_NKGoodsOrigin = "AU";
			item1.API_RX_NKGoodsValueCurrency = "AUD";
			item1.API_CustomsQty = 1;
			item1.API_GrossWeight = 10;
			item1.API_GrossWeightUQ = Weight.Kilograms;
			item1.API_NetWeight = 5;
			item1.API_NetWeightUQ = Weight.Kilograms;
			item1.API_CustomsValue = 100;
			item2.API_FormattedTariff = "11081990010";
			item2.API_GoodsDescription = "item2 goods description";
			item2.API_RN_NKGoodsOrigin = "US";
			item2.API_RX_NKGoodsValueCurrency = "USD";
			item2.API_CustomsQty = 2;
			item2.API_GrossWeight = 20;
			item2.API_GrossWeightUQ = Weight.Kilograms;
			item2.API_NetWeight = 10;
			item2.API_NetWeightUQ = Weight.Kilograms;
			item2.API_CustomsValue = 200;

			var writeManager = new DataWritingManager(new ActionInfo(null, bill));
			var shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);

			CombineAssertions("Shipment properties", () =>
			{
				AssertEquals("BN123", shipment.WayBillNumber);
				AssertEquals("goods description", shipment.GoodsDescription);
				AssertEquals(0.9m, (decimal)shipment.TotalWeight);
				AssertEquals("KG", shipment.TotalWeightUnit.Code);
				AssertEquals(1.1m, (decimal)shipment.TotalVolume);
				AssertEquals("L", shipment.TotalVolumeUnit.Code);
				AssertEquals(13, (int)shipment.OuterPacks);
				AssertEquals("BX", shipment.OuterPacksPackageType.Code);
				AssertEquals(IncoTerms.Other, shipment.ShipmentIncoTerm.Code);
				AssertEquals("IMP", shipment.MessageType.Code);
			});

			var invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Invoice Header properties", () =>
			{
				AssertEquals(110m, (decimal)invoiceHeader.InvoiceAmount);
				AssertEquals("AUD", invoiceHeader.InvoiceCurrency.Code);
				AssertEquals(30m, invoiceHeader.Weight);
				AssertEquals(Weight.Kilograms, invoiceHeader.WeightUnit.Code);
				AssertEquals(15m, invoiceHeader.NetWeight);
				AssertEquals(Weight.Kilograms, invoiceHeader.WeightUnit.Code);
			});

			var invoiceLines = invoiceHeader.CommercialInvoiceLineCollection;
			CombineAssertions("Invoice Line properties", () =>
			{
				AssertEquals(2, invoiceLines.Count);
				AssertEquals("1108.19.90 009", invoiceLines[0].HarmonisedCode);
				AssertEquals("item1 goods description", invoiceLines[0].Description);
				AssertEquals("AU", invoiceLines[0].CountryOfOrigin.Code);
			});
		}

		public void TestInvoiceHeaderGrossWeightConversion()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			item1.API_GrossWeight = 10;
			item1.API_GrossWeightUQ = Weight.Kilograms;
			item2.API_GrossWeight = 500;
			item2.API_GrossWeightUQ = Weight.Grams;

			var writeManager = new DataWritingManager(new ActionInfo(null, bill));
			var shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			var invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use gross weight unit when available", () =>
			{
				AssertEquals(10.5m, invoiceHeader.Weight);
				AssertEquals(Weight.Kilograms, invoiceHeader.WeightUnit.Code);
			});

			item1.API_GrossWeight = 10;
			item1.API_GrossWeightUQ = ZString.Empty;
			item1.API_NetWeightUQ = Weight.Grams;
			item2.API_GrossWeight = 500;
			item2.API_GrossWeightUQ = ZString.Empty;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use net weight unit when missing gross weight unit", () =>
			{
				AssertEquals(510m, invoiceHeader.Weight);
				AssertEquals(Weight.Grams, invoiceHeader.WeightUnit.Code);
			});

			bill.ABL_GrossWeightUQ = Weight.Milligrams;
			item1.API_GrossWeight = 10;
			item1.API_GrossWeightUQ = ZString.Empty;
			item1.API_NetWeightUQ = ZString.Empty;
			item2.API_GrossWeight = 500;
			item2.API_GrossWeightUQ = ZString.Empty;
			item2.API_NetWeightUQ = ZString.Empty;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use bill gross weight unit when missing units on pack item", () =>
			{
				AssertEquals(510m, invoiceHeader.Weight);
				AssertEquals(Weight.Milligrams, invoiceHeader.WeightUnit.Code);
			});
		}
		public void TestInvoiceHeaderNetWeightConversion()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			item1.API_NetWeight = 10;
			item1.API_NetWeightUQ = Weight.Kilograms;
			item2.API_NetWeight = 500;
			item2.API_NetWeightUQ = Weight.Grams;

			var writeManager = new DataWritingManager(new ActionInfo(null, bill));
			var shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			var invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use net weight unit when available", () =>
			{
				AssertEquals(10.5m, invoiceHeader.NetWeight);
				AssertEquals(Weight.Kilograms, invoiceHeader.NetWeightUQ.Code);
			});

			item1.API_NetWeight = 10;
			item1.API_NetWeightUQ = ZString.Empty;
			item1.API_GrossWeightUQ = Weight.Grams;
			item2.API_NetWeight = 500;
			item2.API_NetWeightUQ = ZString.Empty;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use gross weight unit when missing net weight unit", () =>
			{
				AssertEquals(510m, invoiceHeader.NetWeight);
				AssertEquals(Weight.Grams, invoiceHeader.NetWeightUQ.Code);
			});

			bill.ABL_GrossWeightUQ = Weight.Milligrams;
			item1.API_NetWeight = 10;
			item1.API_NetWeightUQ = ZString.Empty;
			item1.API_GrossWeightUQ = ZString.Empty;
			item2.API_NetWeight = 500;
			item2.API_NetWeightUQ = ZString.Empty;
			item2.API_GrossWeightUQ = ZString.Empty;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			CombineAssertions("Should use bill net weight unit when missing unit on pack item", () =>
			{
				AssertEquals(510m, invoiceHeader.NetWeight);
				AssertEquals(Weight.Milligrams, invoiceHeader.NetWeightUQ.Code);
			});
		}

		public void TestInvoiceLinePriceCurrencyConversion()
		{
			SetupExchangeRate(CurrencyCodes.UnitedStates, 1.5, ZDateTime.Today);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			bill.ABL_RX_NKGoodsValueCurrency = CurrencyCodes.UnitedStates;

			item1.API_CustomsValue = 100;
			item1.API_RX_NKGoodsValueCurrency = CurrencyCodes.EuropeanUnion;
			item1.API_CustomsQty = 1;
			item2.API_CustomsValue = 200;
			item2.API_RX_NKGoodsValueCurrency = CurrencyCodes.UnitedStates;
			item2.API_CustomsQty = 1;

			var writeManager = new DataWritingManager(new ActionInfo(null, bill));
			var shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			var invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			var invoiceLines = invoiceHeader.CommercialInvoiceLineCollection;
			CombineAssertions("Should convert packed item currency to bill goods value currency", () =>
			{
				AssertEquals(2, invoiceLines.Count);
				AssertEquals(150m, invoiceLines[0].LinePrice);
				AssertEquals(200m, invoiceLines[1].LinePrice);
			});

			item1.API_RX_NKGoodsValueCurrency = CurrencyCodes.UnitedStates;
			item2.API_RX_NKGoodsValueCurrency = CurrencyCodes.UnitedStates;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			invoiceLines = invoiceHeader.CommercialInvoiceLineCollection;
			CombineAssertions("Should not do currency conversion on packeditem when same as bill goods value currency", () =>
			{
				AssertEquals(2, invoiceLines.Count);
				AssertEquals(100m, invoiceLines[0].LinePrice);
				AssertEquals(200m, invoiceLines[1].LinePrice);
			});

			item1.API_RX_NKGoodsValueCurrency = ZString.Empty;
			item2.API_RX_NKGoodsValueCurrency = ZString.Empty;

			writeManager = new DataWritingManager(new ActionInfo(null, bill));
			shipment = new EUH7AsycudaForCustomsDeclarationDataObjectWriter(writeManager, new AsycudaManifestHeaderDataObjectWriterHelper(header)).GetDataObject(bill);
			invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			invoiceLines = invoiceHeader.CommercialInvoiceLineCollection;
			CombineAssertions("Should not do currency conversion on packed item when no currency specified", () =>
			{
				AssertEquals(2, invoiceLines.Count);
				AssertEquals(100m, invoiceLines[0].LinePrice);
				AssertEquals(200m, invoiceLines[1].LinePrice);
			});
		}

		void SetupExchangeRate(string currency, ZDecimal rate, ZDateTime effectiveDate)
		{
			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, refCurrency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, "CUS");

			var exchangeRate = Factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = refCurrency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = "CUS";
			}
			exchangeRate.RE_SellRate = rate;
		}
	}
}
