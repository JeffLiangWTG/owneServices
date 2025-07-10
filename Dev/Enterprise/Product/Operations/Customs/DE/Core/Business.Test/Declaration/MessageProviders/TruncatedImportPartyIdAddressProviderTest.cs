using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class TruncatedImportPartyIdAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<TruncatedImportPartyIdAddressProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", TruncatedImportPartyIdAddressProvider.NewOrNull(null));
				AssertNotNull("Not Null", TruncatedImportPartyIdAddressProvider.NewOrNull(inner.Object));
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.Name);

				inner.Setup(e => e.Name).Returns("SampleFreight");
				AssertEquals("FullName", "SampleFreight", Provider.Name);

				inner.Setup(e => e.Name).Returns(new string('p', 121));
				AssertEquals("Truncated", 120, Provider.Name.Length);
			});
		}

		public void TestDistrict()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.District);

				inner.Setup(e => e.District).Returns("Finthen");
				AssertEquals("Not Empty", "Finthen", Provider.District);

				inner.Setup(e => e.District).Returns(new string('p', 36));
				AssertEquals("Truncated", 35, Provider.District.Length);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.Address);

				inner.Setup(e => e.Address).Returns("Poststraße 1");
				AssertEquals("Not Empty", "Poststraße 1", Provider.Address);

				inner.Setup(e => e.Address).Returns(new string('p', 36));
				AssertEquals("Truncated", 35, Provider.Address.Length);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.City);

				inner.Setup(e => e.City).Returns("Mainz");
				AssertEquals("Not Empty", "Mainz", Provider.City);

				inner.Setup(e => e.City).Returns(new string('p', 36));
				AssertEquals("Truncated", 35, Provider.City.Length);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.Postcode);

				inner.Setup(e => e.Postcode).Returns("55126");
				AssertEquals("Not Empty", "55126", Provider.Postcode);

				inner.Setup(e => e.Postcode).Returns("1234567890");
				AssertEquals("Truncated", 9, Provider.Postcode.Length);
			});
		}

		public void TestCountry()
		{
			AssertNull("Empty", Provider.Country);

			inner.Setup(e => e.Country).Returns("FR");
			AssertEquals("FR", Provider.Country);
		}

		protected override TruncatedImportPartyIdAddressProvider GetProvider() => (TruncatedImportPartyIdAddressProvider)TruncatedImportPartyIdAddressProvider.NewOrNull(inner.Object);

		protected override void SetUp()
		{
			base.SetUp();
			inner = new Mock<IImportPartyIdAddress>();
		}
		Mock<IImportPartyIdAddress> inner;
	}
}
