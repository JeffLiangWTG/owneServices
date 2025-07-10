using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public class StandAloneDeclarationConverterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestTryConvert_NoDuplicatedDeclarationErrorMessageGivenBillWithCusEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var bill = BuildBill();
				var cusEntryNumber = bill.CustomsEntryNumbers.AddNew();
				cusEntryNumber.CE_EntryNum = "LRN5678";
				var existingDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
				existingDeclaration1.JE_GB = GlbCompany.CurrentCompany.Branches.GetPKs().FirstOrDefault();
				var cusEntryHeader1 = existingDeclaration1.CustomsEntryHeaders.AddNew();
				cusEntryHeader1.CH_MessageType = "IE";
				cusEntryHeader1.CH_BGMReference = "";
				var existingDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
				existingDeclaration2.JE_GB = GlbCompany.CurrentCompany.Branches.GetPKs().FirstOrDefault();
				var cusEntryHeader2 = existingDeclaration2.CustomsEntryHeaders.AddNew();
				cusEntryHeader2.CH_MessageType = "IE";
				cusEntryHeader2.CH_BGMReference = "";
				Factory.SaveForTesting();

				var errorMessage = "";
				var converter = new StandAloneDeclarationConverter();
				converter.OnError += (sender, e) => errorMessage = (string)sender;
				converter.TryConvert(new List<AsycudaBill> { bill, BuildBill() });

				var declaration = new BusinessObjectFactory().LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, bill.EntrySummaryReferenceNumber));
				AssertNotNull(declaration);
				AssertNullOrEmpty(errorMessage);
			}
		}

		public void TestTryConvert_DataMapping()
		{
			var bill = BuildBill();
			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill, BuildBill() });

			var newFactory = new BusinessObjectFactory();
			var reloadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			var declaration = newFactory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reloadedBill.EntrySummaryReferenceNumber));
			CombineAssertions(() =>
			{
				AssertEquals("BN123", declaration.JE_HouseBill);
				AssertEquals("goods description", declaration.JE_GoodsDescription);
				AssertEquals(0.9m, (decimal)declaration.JE_TotalWeight);
				AssertEquals("KG", declaration.JE_TotalWeightUnit);
				AssertEquals(1.1m, (decimal)declaration.JE_TotalVolume);
				AssertEquals("L", declaration.JE_TotalVolumeUnit);
				AssertEquals(13, (int)declaration.JE_TotalNoOfPacks);
				AssertEquals("BX", declaration.JE_TotalNoOfPacksPackType);
			});

			var invoiceHeader = declaration.AllGroupHeaders[0].AllJobComInvoiceHeaders.Single() as JobComInvoiceHeader;
			CombineAssertions(() =>
			{
				AssertEquals(110m, (decimal)invoiceHeader.JZ_InvoiceAmount);
				AssertEquals("AUD", invoiceHeader.JZ_RX_NKInvoice_Currency);
			});

			var invoiceLines =  invoiceHeader.InvoiceLines;
			CombineAssertions(() =>
			{
				AssertEquals(2, invoiceLines.Count);
				AssertEquals("11081990009", invoiceLines[0].JI_Tariff);
				AssertEquals("item1 goods description", invoiceLines[0].JI_Description);
				AssertEquals("AU", invoiceLines[0].JI_CountryOfOrigin);
				AssertEquals("AUD", invoiceLines[0].JI_RX_NKLinePriceCurr);
				AssertEquals("11081990010", invoiceLines[1].JI_Tariff);
				AssertEquals("item2 goods description", invoiceLines[1].JI_Description);
				AssertEquals("US", invoiceLines[1].JI_CountryOfOrigin);
				AssertEquals("AUD", invoiceLines[1].JI_RX_NKLinePriceCurr);
			});
		}

		public void TestTryConvert_ConvertMutipleBills_HasBeenConvertedToStandaloneDeclarationIsTrue()
		{
			var bill1 = BuildBill();
			var bill2 = BuildBill();
			AssertEquals(false, bill1.HasBeenConvertedToStandaloneDeclaration);
			AssertEquals(false, bill2.HasBeenConvertedToStandaloneDeclaration);
			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill1, bill2 });

			var reloadedBill1 = new BusinessObjectFactory().Load<AsycudaBill>(bill1.PK);
			var reloadedBill2 = new BusinessObjectFactory().Load<AsycudaBill>(bill2.PK);
			CombineAssertions(() =>
			{
				Assert(!reloadedBill1.ABL_IsActive);
				Assert(reloadedBill1.HasBeenConvertedToStandaloneDeclaration);
				Assert(reloadedBill1.ReadOnly);
				Assert(!reloadedBill2.ABL_IsActive);
				Assert(reloadedBill2.HasBeenConvertedToStandaloneDeclaration);
				Assert(reloadedBill2.ReadOnly);
			});
		}

		public void TestTryConvert_ConvertSingleBill_ChangeBillOnlyAfterSavingData()
		{
			var bill = BuildBill();

			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill });
			CombineAssertions("No change on bill if convert a single bill and don't save data", () =>
			{
				Assert(bill.ABL_IsActive);
				Assert(!bill.HasBeenConvertedToStandaloneDeclaration);
				Assert(!bill.ReadOnly);
			});

			converter.OnCompleted += Converter_OnCompleted;
			void Converter_OnCompleted(object sender, EventArgs e)
			{
				(sender as JobDeclaration).Factory.Save();
			}

			converter.TryConvert(new List<AsycudaBill> { bill });
			CombineAssertions("Change bill if convert a single bill and save data", () =>
			{
				Assert(!bill.ABL_IsActive);
				Assert(bill.HasBeenConvertedToStandaloneDeclaration);
				Assert(bill.ReadOnly);
			});
		}

		AsycudaBill BuildBill()
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

			pack1.APA_PackUQ = "BX";
			pack2.APA_PackUQ = "FR";
			item1.API_FormattedTariff = "11081990009";
			item1.API_GoodsDescription = "item1 goods description";
			item1.API_RN_NKGoodsOrigin = "AU";
			item1.API_RX_NKGoodsValueCurrency = "AUD";
			item2.API_FormattedTariff = "11081990010";
			item2.API_GoodsDescription = "item2 goods description";
			item2.API_RN_NKGoodsOrigin = "US";
			item2.API_RX_NKGoodsValueCurrency = "USD";
			Factory.SaveForTesting();
			return bill;
		}
	}
}
