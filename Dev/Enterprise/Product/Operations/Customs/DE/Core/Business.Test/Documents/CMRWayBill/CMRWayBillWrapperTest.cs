using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Integration;

namespace Enterprise.Customs.DE.Business.Documents.CMR.Testing
{
	sealed class CMRWayBillWrapperTest : TestCaseWithFactory
	{
		public void TestBoxNumbers()
		{
			var wrapper = GetNewWrapper(declaration);

			AssertEquals("Box 14", "15", wrapper.Box14PaymentCarriage);
			AssertEquals("Box 19", "20", wrapper.Box19SpecialAgreements);
			AssertEquals("Box 20", "19", wrapper.Box20ToBePaidBy);
			AssertEquals("Box 15", "14", wrapper.Box15CashOnDelivery);
			AssertEquals("Box 23", "23", wrapper.Box23TransportAndTrailerID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}
		JobDeclaration declaration;

		ICMRConsignmentNote GetNewWrapper(JobDeclaration declaration) => new CMRWayBillWrapper(declaration);
	}
}
