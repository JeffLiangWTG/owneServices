using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(CountryDiscount))]
	internal class CountryDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var discount = new CountryDiscount();

			var line1 = discount.Lines.AddNew();
			line1.Country = "AU";
			line1.Percent = 40m;

			var line2 = discount.Lines.AddNew();
			line2.Country = "US";
			line2.Percent = 12.15;

			var line3 = discount.Lines.AddNew();
			line3.Country = "AU";
			line3.Percent = 12.15;
			discount.RunPreSaveValidation();

			AssertHasErrors(line1.CountryInfo);
			AssertNoErrors(line2.CountryInfo);
			AssertHasErrors(line3.CountryInfo);

			//TestNoDeadCode
			var type = typeof(AutoCountryDiscount.Schema);
			AssertNotNull(type);
		}

		public void TestXmlSerialization()
		{
			var discount = new CountryDiscount();
			var line1 = discount.Lines.AddNew();
			line1.Country = "AU";
			line1.Percent = 40m;

			var line2 = discount.Lines.AddNew();
			line2.Country = "US";
			line2.Percent = 12.15;
			line2.RequiresDomesticDiscount = false;

			var serializer = ZXmlSerializer.New(typeof(CountryDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (CountryDiscount)serializer.Deserialize(reader);

			AssertEquals(2, discount2.Lines.Count);
			AssertEquals("AU", discount2.Lines[0].Country);
			AssertEquals(40m, discount2.Lines[0].Percent);
			AssertEquals(true, discount2.Lines[0].RequiresDomesticDiscount);
			AssertEquals("US", discount2.Lines[1].Country);
			AssertEquals(12.15m, discount2.Lines[1].Percent);
			AssertEquals(false, discount2.Lines[1].RequiresDomesticDiscount);
		}
	}

	[TestedType(typeof(CountryDiscountLineCollection))]
	internal class CountryDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountryDiscountLineCollection>
	{
		protected override CountryDiscountLineCollection GetCollectionToTest()
		{
			var parent = new CountryDiscount();
			return new CountryDiscountLineCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryDiscountLine();
		}
	}

	[TestedType(typeof(CountryDiscountLine))]
	internal class CountryDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCountryCode()
		{
			var parent = new CountryDiscount();
			var line = parent.Lines.AddNew();
			line.Country = "$$";
			AssertHasErrors(line.CountryInfo);

			line.Country = "AU";
			AssertNoErrors(line.CountryInfo);

			line.Country = "";
			AssertHasErrors(line.CountryInfo);
		}

		public void TestPercent()
		{
			var parent = new CountryDiscount();
			var line = parent.Lines.AddNew();
			line.Percent = 0;
			AssertHasErrors(line.PercentInfo);

			line.Percent = 1;
			AssertNoErrors(line.PercentInfo);

			line.Percent = 100;
			AssertNoErrors(line.PercentInfo);

			line.Percent = 101m;
			AssertHasErrors(line.PercentInfo);
		}
	}
}
