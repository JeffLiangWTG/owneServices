using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class MCommodityCodeType03ProviderTest : DataProviderTestCase<MCommodityCodeType03Provider>
	{
		public void TestIMCommodityCode()
		{
			Assert("Should implement IMCommodityCode", Provider is IMCommodityCode);
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("HarmonizedSystemSubheadingCode", "123456", Provider.HarmonizedSystemSubheadingCode);
			invoiceLine.JI_Tariff = "1234";
			AssertNull("HarmonizedSystemSubheadingCode", Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("CombinedNomenclatureCode", "78", Provider.CombinedNomenclatureCode);
			invoiceLine.JI_Tariff = "1234";
			AssertNull("CombinedNomenclatureCode", Provider.CombinedNomenclatureCode);
			invoiceLine.JI_Tariff = "1234567890";
			entryInstruction.CEI_Style = "I1";
			invoiceLine.JI_LinePrice = 10;
			AssertNull("CombinedNomenclatureCode", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("TaricCode", "90", Provider.TaricCode);
			invoiceLine.JI_Tariff = "1234";
			AssertNull("TaricCode", Provider.TaricCode);
		}

		public void TestTaricAdditionalCode()
		{
			SetUpTestData();
			invoiceLine.JI_SupplementaryCode1 = "123";
			invoiceLine.JI_SupplementaryCode2 = "456";
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "789";

			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber", "1", Provider.TaricAdditionalCode.FirstOrDefault().SequenceNumber);
				AssertEquals("TaricAdditionalCode", "123456789", Provider.TaricAdditionalCode.FirstOrDefault().TaricAdditionalCode);
			});
		}

		public void TestNationalAdditionalCode()
		{
			SetUpTestData();
			invoiceLine.CusLineTariffDetails.AddNew().BZ_Tariff = "AIS";

			AssertEquals("SequenceNumber", "1", Provider.NationalAdditionalCode.FirstOrDefault().SequenceNumber);
			AssertEquals("NationalAdditionalCode", "AIS", Provider.NationalAdditionalCode.FirstOrDefault().NationalAdditionalCode);
		}

		public void TestTypeGoods()
		{
			AssertNull("TypeGoods", Provider.TypeGoods);
		}

		protected override MCommodityCodeType03Provider GetProvider()
		{
			SetUpTestData();
			return new MCommodityCodeType03Provider(entryLineWrapper);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLineWrapper = testBizObjs.entryLineWrapper;
				entryLineWrapper.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				entryInstruction = entryLineWrapper.Instruction;
				invoiceLine = entryLineWrapper.RandomInvoiceLine;
			}
		}

		EntryLineWrapper entryLineWrapper;
		CusEntryInstruction entryInstruction;
		JobComInvoiceLine invoiceLine;
	}
}
