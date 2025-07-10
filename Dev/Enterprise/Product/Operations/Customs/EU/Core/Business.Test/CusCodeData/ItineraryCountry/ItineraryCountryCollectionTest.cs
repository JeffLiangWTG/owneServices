using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(ItineraryCountryCollection))]
	sealed class ItineraryCountryCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ItineraryCountry>
	{
		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				for (int i = 0; i < 98; i++)
				{
					declaration.ItineraryCountries.AddNew();
				}
				NUnit.Framework.Assert.That(declaration.ItineraryCountries.AllowNew, NUnit.Framework.Is.EqualTo(true), "Allow new when count is 98");

				declaration.ItineraryCountries.AddNew();
				NUnit.Framework.Assert.That(declaration.ItineraryCountries.AllowNew, NUnit.Framework.Is.EqualTo(false), "Maximum count is 99");
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateItineraryCountryCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.UniqueVoyageIdentifier = "FRDENL";

			var itineraryCountryCollection = new ItineraryCountryCollection(declaration);
			itineraryCountryCollection.PopulateItineraryCountryCollection();

			NUnit.Framework.Assert.That(itineraryCountryCollection.Count, NUnit.Framework.Is.EqualTo(3));

			NUnit.Framework.Assert.That(itineraryCountryCollection[0].CY_Code, NUnit.Framework.Is.EqualTo("FR").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountryCollection[0].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1));

			NUnit.Framework.Assert.That(itineraryCountryCollection[1].CY_Code, NUnit.Framework.Is.EqualTo("DE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountryCollection[1].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2));

			NUnit.Framework.Assert.That(itineraryCountryCollection[2].CY_Code, NUnit.Framework.Is.EqualTo("NL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountryCollection[2].CY_Order, NUnit.Framework.Is.EqualTo((ZShort)3));
		}

		[ExpectNoExceptions]
		public void TestCollectionOrder()
		{
			var declaration = Factory.New<JobDeclaration>();

			var itineraryCountry1 = declaration.ItineraryCountries.AddNew();
			var itineraryCountry2 = declaration.ItineraryCountries.AddNew();
			var itineraryCountry3 = declaration.ItineraryCountries.AddNew();

			NUnit.Framework.Assert.That(itineraryCountry1.CY_Order, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountry2.CY_Order, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountry3.CY_Order, NUnit.Framework.Is.EqualTo((short)3).Using(CustomComparers.TypeComparison));

			declaration.ItineraryCountries.RemoveAndDelete(itineraryCountry2);

			NUnit.Framework.Assert.That(itineraryCountry1.CY_Order, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(itineraryCountry3.CY_Order, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
		}

		protected override CusCodeDataCollection<ItineraryCountry> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new ItineraryCountryCollection(declaration);
		}
	}
}
