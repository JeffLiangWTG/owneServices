using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class DeclarationTypeCustomsOfficesProviderTest : DataProviderTestCase<DeclarationTypeCustomsOfficesProvider>
	{
		public void TestPresentationCustomsOffice()
		{
			Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IEDUB100");
			AssertEquals("IEDUB100", Provider.PresentationCustomsOffice);
		}

		public void TestSupervisingCustomsOffice()
		{
			Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IEDUB101");
			AssertEquals("IEDUB101", Provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			Declaration.JE_CustomsOffice = "IEDUB102";
			AssertEquals("IEDUB102", Provider.CustomsOfficeLodgement);
		}

		protected override DeclarationTypeCustomsOfficesProvider GetProvider() => new DeclarationTypeCustomsOfficesProvider(Declaration);

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;
	}
}
