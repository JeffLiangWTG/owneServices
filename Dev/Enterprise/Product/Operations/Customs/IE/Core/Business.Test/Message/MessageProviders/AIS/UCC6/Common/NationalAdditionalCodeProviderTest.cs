using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class NationalAdditionalCodeProviderTest : DataProviderTestCase<NationalAdditionalCodeProvider>
	{
		public void TestICcQualifierNationalAdditionalCode()
		{
			Assert("Should implement ICcQualifierNationalAdditionalCode", Provider is ICcQualifierNationalAdditionalCode);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", GetProvider().CcQualifier);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", "1", GetProvider().SequenceNumber);
		}

		public void TestNationalAdditionalCode()
		{
			SetUpTestData();
			AssertEquals("NationalAdditionalCode", "EN", Provider.NationalAdditionalCode);
		}

		protected override NationalAdditionalCodeProvider GetProvider()
		{
			SetUpTestData();
			return new NationalAdditionalCodeProvider(1, cusLineTariffDetail);
		}

		void SetUpTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			cusLineTariffDetail.BZ_Tariff = "EN";
		}

		CusLineTariffDetail cusLineTariffDetail;
	}
}
