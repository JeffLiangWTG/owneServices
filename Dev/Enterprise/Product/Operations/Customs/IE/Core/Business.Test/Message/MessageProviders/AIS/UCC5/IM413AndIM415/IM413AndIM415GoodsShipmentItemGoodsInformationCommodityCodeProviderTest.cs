using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider>
	{
		public void TestCombinedNomenclatureCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("CombinedNomenclatureCode", "12345678", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("TaricCode", "90", Provider.TaricCode);
		}

		public void TestTaricAdditionalCodes()
		{
			SetUpTestData();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";
			var addcode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			addcode1.CY_Order = 3;
			addcode1.CY_Code = "5";
			var addcode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			addcode2.CY_Order = 1;
			addcode2.CY_Code = "3";
			var addcode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			addcode3.CY_Order = 2;
			addcode3.CY_Code = "";
			AssertEquals("TaricAdditionalCodes", "1,2,3,5", ZString.Join(",", Provider.TaricAdditionalCodes.Select(p => new ZString(p)).ToArray()));
		}

		public void TestNationalAdditionalCodes()
		{
			SetUpTestData();
			invoiceLine.JI_ZZF_NKTaxType = "t1";
			var tariffDetail1 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail1.BZ_Tariff = "t2";
			var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Tariff = "";
			var tariffDetail3 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail3.BZ_Tariff = "t3";

			CombineAssertions(() =>
			{
				var codes = Provider.NationalAdditionalCodes.ToList();
				AssertEquals("Count", 3, codes.Count);
				AssertEquals("NationalAdditionalCode 1", "t1", codes[0]);
				AssertEquals("NationalAdditionalCode 2", "t2", codes[1]);
				AssertEquals("NationalAdditionalCode 3", "t3", codes[2]);
			});
		}

		protected override IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider(invoiceLine);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
