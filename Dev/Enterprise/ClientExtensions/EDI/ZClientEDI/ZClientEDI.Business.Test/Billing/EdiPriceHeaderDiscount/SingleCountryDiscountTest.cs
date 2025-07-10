using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SingleCountryDiscount))]
	internal class SingleCountryDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var discount = new SingleCountryDiscount();
			discount.RunPreSaveValidation();
			AssertHasErrors(discount.CountryInfo);

			discount.Country = "--";
			AssertHasErrors(discount.CountryInfo);

			discount.Country = "AU";
			AssertNoErrors(discount.CountryInfo);
		}

		public void TestXmlSerialization()
		{
			var discount = new SingleCountryDiscount();
			discount.Country = "NZ";

			var serializer = ZXmlSerializer.New(typeof(SingleCountryDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (SingleCountryDiscount)serializer.Deserialize(reader);
			AssertEquals("NZ", discount2.Country);
		}
	}
}
