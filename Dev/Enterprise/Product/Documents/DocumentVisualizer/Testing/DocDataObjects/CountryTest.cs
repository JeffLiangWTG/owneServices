using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(Country))]
	sealed class CountryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreate_UniversalCountry()
		{
			var universalCountry = new UniversalCountry
			{
				Code = "AU",
				Name = "Australia"
			};

			var country = Country.Create(Context, universalCountry);

			AssertEquals(nameof(country.Code), "AU", country.Code);
			AssertEquals(nameof(country.Name), "Australia", country.Name);
		}

		public void TestCreate_UniversalCountry_Null()
		{
			var country = Country.Create(Context, (UniversalCountry)null);

			AssertEquals(nameof(country.Code), ZString.Empty, country.Code);
			AssertEquals(nameof(country.Name), ZString.Empty, country.Name);
		}

		public void TestSuspendOnValueChanged()
		{
			var universalCountry = new UniversalCountry
			{
				Code = "AU",
				Name = "Australia"
			};

			var country = Country.Create(Context, universalCountry);
			var message = string.Empty;

			country.CodeInfo.ValueChanged += (object sender, System.EventArgs e) =>
			{
				message += "CodeInfo ValueChanged" + System.Environment.NewLine;
			};

			country.NameInfo.ValueChanged += (object sender, System.EventArgs e) =>
			{
				message += "NameInfo ValueChanged" + System.Environment.NewLine;
			};

			using (country.CodeInfo.SuspendOnValueChanged())
			{
				country.Code = "CN";
			}
			AssertNullOrEmpty(message);
			AssertEquals("Australia", country.Name);

			country.Code = "DE";
			AssertEquals("CodeInfo ValueChanged" + System.Environment.NewLine + "NameInfo ValueChanged" + System.Environment.NewLine, message);
			AssertEquals("Germany", country.Name);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Country(Factory, new RefCountryCollection(Factory))
			{
				Code = "ZA"
			};
		}

		IContext Context => context ?? (context = ObjectFactory.Get<IContext>("IContext", Factory));
		IContext context;
	}
}
