using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class AddressFactTest : TestCase
	{
		public void TestNullAddress_ThrowsException()
		{
			var countryFactMock = new Mock<ICountryFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new AddressFact(null, countryFactMock.Object));
		}

		public void TestNull_Address_Country_ThrowsException()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_RN_NKCountryCode = string.Empty;
			Assert("Precondition", address.Country == null);
			var countryFactMock = new Mock<ICountryFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new AddressFact(address, countryFactMock.Object));
		}

		public void TestNullCountryFact_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AddressFact(validAddress, null));
		}

		public void TestPK()
		{
			var countryFact = new CountryFact(validAddress.Country);
			var addressFact = new AddressFact(validAddress, countryFact);

			AssertEquals(validAddress.PK, addressFact.PK);
		}

		public void TestCountry_NotNull()
		{
			var countryFact = new CountryFact(validAddress.Country);
			var addressFact = new AddressFact(validAddress, countryFact);

			AssertNotNull(addressFact.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var pk = Guid.NewGuid();
			var country = Factory.New<RefCountry>();
			country.Code = "AU";

			validAddress = Factory.NewWithPrimaryKey<OrgAddress>(pk);
			validAddress.OA_RN_NKCountryCode = country.Code;
		}

		OrgAddress validAddress;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
