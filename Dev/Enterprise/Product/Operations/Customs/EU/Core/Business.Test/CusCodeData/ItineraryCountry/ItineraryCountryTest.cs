using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(ItineraryCountry))]
	sealed class ItineraryCountryTest : Customs.Business.Testing.CusCodeDataTest<ItineraryCountry>
	{
		[ExpectNoExceptions]
		public void TestLookups()
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			NUnit.Framework.Assert.That(itineraryCountry.Lookups, NUnit.Framework.Is.TypeOf<ItineraryCountryLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			NUnit.Framework.Assert.That(itineraryCountry.Validation, NUnit.Framework.Is.TypeOf<ItineraryCountryValidation>());
		}

		[ExpectNoExceptions]
		public void TestCY_Order_Attributes() => CombineAssertions(() =>
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			NUnit.Framework.Assert.That(itineraryCountry.CY_OrderInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "ReadOnly");
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(itineraryCountry.CY_OrderInfo).Caption, NUnit.Framework.Is.EqualTo("Sequence"), "Caption");
		});

		[ExpectNoExceptions]
		public void TestCY_Code_Attributes() => CombineAssertions(() =>
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			NUnit.Framework.Assert.That(itineraryCountry.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(2), "MaxLength");
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(itineraryCountry.CY_CodeInfo).Caption, NUnit.Framework.Is.EqualTo("Country"), "Caption");
			NUnit.Framework.Assert.That(typeof(ItineraryCountry), CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(ItineraryCountry.CY_Code), false, attribute => attribute.ListDataSourceMember == nameof(ItineraryCountry.Lookups) + "." + nameof(ItineraryCountryLookups.CountryList)));
		});

		[ExpectNoExceptions]
		public void TestCY_Order()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var itineraryCountry1 = declaration.ItineraryCountries.AddNew();
			var itineraryCountry2 = declaration.ItineraryCountries.AddNew();
			var itineraryCountry3 = declaration.ItineraryCountries.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(itineraryCountry1.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1), "Order 1");
				NUnit.Framework.Assert.That(itineraryCountry2.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2), "Order 2");
				NUnit.Framework.Assert.That(itineraryCountry3.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)3), "Order 3");

				declaration.ItineraryCountries.Remove(itineraryCountry2);
				NUnit.Framework.Assert.That(itineraryCountry1.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)1), "Order 1 stay same");
				NUnit.Framework.Assert.That(itineraryCountry3.CY_Order, NUnit.Framework.Is.EqualTo((ZShort)2), "Order 3 change to 2");
			});
		}

		[ExpectNoExceptions]
		public void TestISequenceNumberLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var itineraryCountry = declaration.ItineraryCountries.AddNew();

			CombineAssertions(() =>
			{
				var sequenceLine = (IShortSequenceNumberLine)itineraryCountry;
				NUnit.Framework.Assert.That(sequenceLine.FKToHeader, NUnit.Framework.Is.EqualTo(declaration.PK), "FKToHeader");
				NUnit.Framework.Assert.That(sequenceLine.SequenceNumber, NUnit.Framework.Is.EqualTo((ZShort)1), "SequenceNumber");
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override IEnumerable<ItineraryCountry> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (ItineraryCountry)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			return declaration.ItineraryCountries.AddNew();
		}

		#endregion
	}
}
