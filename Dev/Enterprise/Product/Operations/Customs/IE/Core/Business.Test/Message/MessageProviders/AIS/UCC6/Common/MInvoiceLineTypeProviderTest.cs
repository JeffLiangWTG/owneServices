using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class MInvoiceLineTypeProviderTest : DataProviderTestCase<MInvoiceLineTypeProvider>
	{
		public void TestIMoney()
		{
			Assert("Should implement IMoney", Provider is IMoney);
		}

		public void TestAmount()
		{
			AssertEquals("Amount", 12m, Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "EUR", Provider.Currency);
		}

		protected override MInvoiceLineTypeProvider GetProvider()
		{
			SetUpTestData();
			return new MInvoiceLineTypeProvider(entryLineWrapper);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLineWrapper = testBizObjs.entryLineWrapper;
				entryLineWrapper.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var invoiceLine = entryLineWrapper.RandomInvoiceLine;
				invoiceLine.JI_LinePrice = 12;
				entryLineWrapper.RandomInvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			}
		}

		EntryLineWrapper entryLineWrapper;
	}
}
