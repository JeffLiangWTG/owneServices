using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(Address))]
	sealed class AddressTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddAddressFormattedValidationDependencies()
		{
			var address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.CompanyNameInfo);

			address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.AddressLine1Info);

			address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.AddressLine2Info);

			address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.CityInfo);

			address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.StateInfo);

			address = new Address(Context.Factory);
			AssertAddressFormattedInfo(address, address.PostcodeInfo);

			address = new Address(Context.Factory);
			address.Country = new Country(Factory, new RefCountryCollection(Factory))
			{
				Code = ZString.Empty
			};
			AssertAddressFormattedInfo(address, address.Country.NameInfo);
		}

		void AssertAddressFormattedInfo(Address address, ZPropertyInfo info)
		{
			var errorMessage = "It is Empty";

			address.AddressFormattedInfo.AddMessageError(() => info.Value.IsEmpty, errorMessage);

			address.ValidateAll();
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			info.Value = new ZString("VALUE");
			AssertEquals("VALUE", address.AddressFormatted);
			AssertNoMessageError(address.AddressFormattedInfo, errorMessage);

			info.Value = ZString.Empty;
			AssertNullOrEmpty(address.AddressFormatted);
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			using (address.AddressFormattedInfo.SuspendOnValueChanged())
			{
				address.AddressFormatted = "VALUE";
			}
			AssertNullOrEmpty((ZString)info.Value);
			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("VALUE");
			}
			AssertEquals("VALUE", address.AddressFormatted);
			AssertHasMessageError("Simulate the `Reset` operation in the form.", address.AddressFormattedInfo, errorMessage);

			address.AddAddressFormattedValidationDependencies();

			info.Value = new ZString("VALUE_NEW");
			AssertEquals("VALUE_NEW", address.AddressFormatted);
			AssertNoMessageError(address.AddressFormattedInfo, errorMessage);

			info.Value = ZString.Empty;
			AssertNullOrEmpty(address.AddressFormatted);
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			using (address.AddressFormattedInfo.SuspendOnValueChanged())
			{
				address.AddressFormatted = "VALUE_NEW";
			}
			AssertNullOrEmpty((ZString)info.Value);
			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("VALUE_NEW");
			}
			AssertEquals("VALUE_NEW", address.AddressFormatted);
			AssertNoMessageError("Simulate the `Reset` operation in the form.", address.AddressFormattedInfo, errorMessage);
		}

		public void TestAddAddressFormattedValidationDependencies_CountryCode()
		{
			var errorMessage = "Country Code is Empty";

			var address = new Address(Context.Factory);
			address.Country = new Country(Factory, new RefCountryCollection(Factory))
			{
				Code = ZString.Empty
			};

			address.AddressFormattedInfo.AddMessageError(() => address.Country.Code.IsEmpty, errorMessage);

			address.ValidateAll();
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			address.Country.Code = "AU";
			AssertEquals("AUSTRALIA", address.AddressFormatted);
			AssertNoMessageError(address.AddressFormattedInfo, errorMessage);

			address.Country.Code = ZString.Empty;
			AssertNullOrEmpty(address.AddressFormatted);
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			using (address.AddressFormattedInfo.SuspendOnValueChanged())
			{
				address.AddressFormatted = "AUSTRALIA";
			}
			AssertNullOrEmpty(address.Country.Code);
			using (address.Country.CodeInfo.SuspendOnValueChanged())
			{
				address.Country.Code = "AU";
			}
			using (address.Country.NameInfo.SuspendOnValueChanged())
			{
				address.Country.Name = "AUSTRALIA";
			}
			AssertEquals("AUSTRALIA", address.AddressFormatted);
			AssertHasMessageError("Simulate the `Reset` operation in the form.", address.AddressFormattedInfo, errorMessage);

			address.AddAddressFormattedValidationDependencies();

			address.Country.Code = "DE";
			AssertEquals("GERMANY", address.AddressFormatted);
			AssertNoMessageError(address.AddressFormattedInfo, errorMessage);

			address.Country.Code = ZString.Empty;
			AssertNullOrEmpty(address.AddressFormatted);
			AssertHasMessageError(address.AddressFormattedInfo, errorMessage);

			using (address.AddressFormattedInfo.SuspendOnValueChanged())
			{
				address.AddressFormatted = "GERMANY";
			}
			AssertNullOrEmpty(address.Country.Code);
			using (address.Country.CodeInfo.SuspendOnValueChanged())
			{
				address.Country.Code = "DE";
			}
			using (address.Country.NameInfo.SuspendOnValueChanged())
			{
				address.Country.Name = "GERMANY";
			}
			AssertEquals("GERMANY", address.AddressFormatted);
			AssertNoMessageError("Simulate the `Reset` operation in the form.", address.AddressFormattedInfo, errorMessage);
		}

		public void TestAddAddressFormattedValidationDependencies_AddOnce()
		{
			var address = new Address(Context.Factory);

			var count = 0;
			address.AddressFormattedInfo.AddMessageError(() =>
			{
				count++;
				return false;
			}, "It is for testing");

			address.ValidateAll();
			AssertEquals(1, count);

			address.AddAddressFormattedValidationDependencies();
			using (address.CompanyNameInfo.SuspendOnValueChanged())
			{
				address.CompanyName = "Company";
			}
			AssertEquals(2, count);

			address.CompanyName = "Company_2";
			AssertEquals("(1) CompanyName -> UpdateAddressFormatted -> ValidateAddressFormattedInfo (2) AddAddressFormattedValidationDependencies CompanyNameInfo OnValueChanged -> ValidateAddressFormattedInfo", 4, count);

			address.AddAddressFormattedValidationDependencies();
			using (address.CompanyNameInfo.SuspendOnValueChanged())
			{
				address.CompanyName = "Company";
			}
			AssertEquals("Call AddAddressFormattedValidationDependencies twice", 5, count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Address(Context.Factory);
		}

		IContext Context => context ?? (context = ObjectFactory.Get<IContext>("IContext", Factory));
		IContext context;
	}
}
