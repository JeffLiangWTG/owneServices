using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class TaricAdditionalCodeProviderTest : DataProviderTestCase<TaricAdditionalCodeProvider>
	{
		public void TestITaricAdditionalCode()
		{
			Assert("Should implement ITaricAdditionalCode", Provider is ITaricAdditionalCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", "1", Provider.SequenceNumber);
		}

		public void TestTaricAdditionalCode()
		{
			SetUpTestData();
			AssertEquals("TaricAdditionalCode", "123456789", Provider.TaricAdditionalCode);
		}

		protected override TaricAdditionalCodeProvider GetProvider()
		{
			SetUpTestData();
			return new TaricAdditionalCodeProvider(1, invoiceLine);
		}

		void SetUpTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "123";
			invoiceLine.JI_SupplementaryCode2 = "456";
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "789";
		}

		JobComInvoiceLine invoiceLine;
	}
}
