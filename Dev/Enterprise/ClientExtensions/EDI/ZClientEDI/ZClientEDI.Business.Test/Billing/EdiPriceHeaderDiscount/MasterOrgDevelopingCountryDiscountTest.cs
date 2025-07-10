using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(MasterOrgDevelopingCountryDiscount))]
	public class MasterOrgDevelopingCountryDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestXmlSerialization()
		{
			var discount = new MasterOrgDevelopingCountryDiscount();
			var line1 = discount.Lines.AddNew();
			line1.Country = "AU";
			line1.Percent = 10;
			var line2 = discount.Lines.AddNew();
			line2.Country = "US";
			line2.Percent = 20;

			var serializer = ZXmlSerializer.New(typeof(MasterOrgDevelopingCountryDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (MasterOrgDevelopingCountryDiscount)serializer.Deserialize(reader);
			AssertEquals(2, discount2.Lines.Count);
			AssertNotNull(discount2.Lines.OfType<MasterOrgDevelopingCountryDiscountLine>().Single(x => x.Country == "AU" && x.Percent == 10));
			AssertNotNull(discount2.Lines.OfType<MasterOrgDevelopingCountryDiscountLine>().Single(x => x.Country == "US" && x.Percent == 20));
		}

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(AutoMasterOrgDevelopingCountryDiscount.Schema));
		}
	}

	[TestedType(typeof(MasterOrgDevelopingCountryDiscountLine))]
	public class MasterOrgDevelopingCountryDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var line = new MasterOrgDevelopingCountryDiscountLine();
			line.RunPreSaveValidation();
			AssertHasErrors(line.CountryInfo);

			line.Country = "--";
			AssertHasErrors(line.CountryInfo);

			line.Country = "AU";
			AssertNoErrors(line.CountryInfo);

			var discount = new MasterOrgDevelopingCountryDiscount();
			var line1 = discount.Lines.AddNew();
			line1.Country = "AU";
			AssertNoErrors(line1.CountryInfo);
			var line2 = discount.Lines.AddNew();
			line2.Country = "AU";
			AssertHasError(line2.CountryInfo, "The Country has been duplicated and must be unique.");
			line2.Country = "US";
			AssertNoErrors(line2.CountryInfo);
		}

		public void TestXmlSerialization()
		{
			var line = new MasterOrgDevelopingCountryDiscountLine();
			line.Country = "AU";
			line.Percent = 20;

			var serializer = ZXmlSerializer.New(typeof(MasterOrgDevelopingCountryDiscountLine));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, line);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var line2 = (MasterOrgDevelopingCountryDiscountLine)serializer.Deserialize(reader);
			AssertEquals("AU", line2.Country);
			AssertEquals(20m, line2.Percent);
		}
	}

	[TestedType(typeof(MasterOrgDevelopingCountryDiscountLineCollection))]
	internal class MasterOrgDevelopingCountryDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MasterOrgDevelopingCountryDiscountLineCollection>
	{
		protected override MasterOrgDevelopingCountryDiscountLineCollection GetCollectionToTest()
		{
			return new MasterOrgDevelopingCountryDiscountLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MasterOrgDevelopingCountryDiscountLine();
		}
	}
}
