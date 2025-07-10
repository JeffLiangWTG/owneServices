using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC228CGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("GuarantorType missing", () => new CC228CGuarantorProvider(null));
			});
		}

		public void TestId()
		{
			AssertEquals("Id", "ID1", provider.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name", "BOB THE BUILDER", provider.Name);
		}

		public void TestAddress()
		{
			var address = provider.Address;
			AssertEquals("2020", address.Postcode);
			AssertEquals("CITY", address.City);
			AssertEquals("IE", address.Country);
		}

		public static GuarantorType03 CreateStandardProvider() => new GuarantorType03
		{
			IdentificationNumber = "ID1",
			Name = "BOB THE BUILDER",
			Address = new AddressType06
			{
				Postcode = "2020",
				City = "CITY",
				Country = CountryCodesCustomsOfficeLists.Ie,
			},
		};

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC228CGuarantorProvider(CreateStandardProvider());
		}
		CC228CGuarantorProvider provider;
	}
}
