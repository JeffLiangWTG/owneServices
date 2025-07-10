using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryListElement))]
	sealed class CountryListElementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCountryCollection()
		{
			var element = CountryListCollection.AddNew();
			AssertEquals("CountryCollection.GetType()", ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), element.CountryCollection.GetType());
		}

		public void TestValidateCountryPK()
		{
			var element1 = CountryListCollection.AddNew();
			AssertNoErrors("Precondition: countryPK should not have errors.", element1.CountryPKInfo);

			element1.CountryPK = ZGuid.Invalid;
			AssertHasError(element1.CountryPKInfo, "Enter a valid selection.");

			var country = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			element1.CountryPK = country.PK;
			AssertNoErrors(element1.CountryPKInfo);

			var element2 = CountryListCollection.AddNew();
			element2.CountryPK = country.PK;

			AssertHasError(element2.CountryPKInfo, "Remove Duplicate Countries: No Duplicates Allowed.");
		}

		public void TestCountryName()
		{
			var element = CountryListCollection.AddNew();
			var country = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			element.CountryPK = country.PK;

			AssertEquals("Australia", element.CountryName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CountryListCollection collection = new CountryListCollection(Factory);
			return new CountryListElement(collection);
		}

		CountryListCollection CountryListCollection
		{
			get
			{
				if (countryListCollection == null)
				{
					countryListCollection = new CountryListCollection(Factory);
				}
				return countryListCollection;
			}
		}

		CountryListCollection countryListCollection;
	}
}
