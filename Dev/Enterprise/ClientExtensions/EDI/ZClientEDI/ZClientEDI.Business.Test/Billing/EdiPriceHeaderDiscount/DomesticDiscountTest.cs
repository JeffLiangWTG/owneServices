using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DomesticDiscount))]
	internal class DomesticDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var discount = new DomesticDiscount();
			var line1 = discount.Lines.AddNew();
			line1.UserCount = 10;
			line1.Percent = 20m;

			var line2 = discount.Lines.AddNew();
			line2.UserCount = 10;
			line2.Percent = 30m;

			var line3 = discount.Lines.AddNew();
			line3.UserCount = 20;
			line3.Percent = 30m;

			discount.RunPreSaveValidation();
			AssertHasErrors(line1.UserCountInfo);
			AssertNoErrors(line1.PercentInfo);

			AssertHasErrors(line2.UserCountInfo);
			AssertHasErrors(line2.PercentInfo);

			AssertNoErrors(line3.UserCountInfo);
			AssertHasErrors(line3.PercentInfo);
		}

		public void TestXmlSerialization()
		{
			var discount = new DomesticDiscount();
			discount.MaxForeignUserCount = 50;
			discount.MaxForeignUserPercent = 20m;
			discount.MaxForeignCompanyCount = 5;
			discount.RequiresDevelopingCountry = true;
			discount.MultiEntityPercent = 10m;
			discount.ExpiryMonthCount = 3;

			var line1 = discount.Lines.AddNew();
			line1.UserCount = 0;
			line1.Percent = 30m;

			var line2 = discount.Lines.AddNew();
			line2.UserCount = 20;
			line2.Percent = 20m;

			var serializer = ZXmlSerializer.New(typeof(DomesticDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (DomesticDiscount)serializer.Deserialize(reader);

			AssertEquals(true, discount2.RequiresDevelopingCountry);
			AssertEquals(50, discount2.MaxForeignUserCount);
			AssertEquals(20m, discount2.MaxForeignUserPercent);
			AssertEquals(5, discount2.MaxForeignCompanyCount);
			AssertEquals(10m, discount2.MultiEntityPercent);
			AssertEquals(3, discount2.ExpiryMonthCount);
			AssertEquals(2, discount2.Lines.Count);
			AssertEquals(0, discount2.Lines[0].UserCount);
			AssertEquals(30m, discount2.Lines[0].Percent);
			AssertEquals(20, discount2.Lines[1].UserCount);
			AssertEquals(20m, discount2.Lines[1].Percent);
		}

		public void TestFindMatchingBreak()
		{
			var discount = new DomesticDiscount();

			var line1 = discount.Lines.AddNew();
			line1.UserCount = 0;
			line1.Percent = 30m;

			var line2 = discount.Lines.AddNew();
			line2.UserCount = 20;
			line2.Percent = 20m;

			var line3 = discount.Lines.AddNew();
			line3.UserCount = 50;
			line3.Percent = 10m;

			AssertEquals(30m, discount.FindMatchingBreak(0).Percent);
			AssertEquals(30m, discount.FindMatchingBreak(1).Percent);
			AssertEquals(30m, discount.FindMatchingBreak(19).Percent);

			AssertEquals(20m, discount.FindMatchingBreak(20).Percent);
			AssertEquals(20m, discount.FindMatchingBreak(21).Percent);
			AssertEquals(20m, discount.FindMatchingBreak(49).Percent);

			AssertEquals(10m, discount.FindMatchingBreak(50).Percent);
			AssertEquals(10m, discount.FindMatchingBreak(51).Percent);
		}

		public void TestCalculatePercent()
		{
			var discount1 = new DomesticDiscount();
			discount1.MaxForeignUserCount = 0;
			discount1.MaxForeignUserPercent = 0m;
			discount1.MaxForeignCompanyCount = 0;
			discount1.RequiresDevelopingCountry = false;
			discount1.MultiEntityPercent = 10m;
			discount1.ExpiryMonthCount = 0;

			var discount2 = new DomesticDiscount();
			discount2.MaxForeignUserCount = 50;
			discount2.MaxForeignUserPercent = 20m;
			discount2.MaxForeignCompanyCount = 5;
			discount2.RequiresDevelopingCountry = true;
			discount2.MultiEntityPercent = 10m;
			discount2.ExpiryMonthCount = 3;

			var line1 = discount2.Lines.AddNew();
			line1.UserCount = 0;
			line1.Percent = 30m;

			var line2 = discount2.Lines.AddNew();
			line2.UserCount = 20;
			line2.Percent = 20m;

			var companyUsers = new List<CompanyCountryUsers>();
			companyUsers.Add(new CompanyCountryUsers(1000, "CN", "CN"));
			var countryUsers = new DatabaseCountryUsers(companyUsers, companyUsers);
			AssertEquals(20m, discount2.CalculatePercent(countryUsers));

			companyUsers.Add(new CompanyCountryUsers(100, "CN", "CN"));
			countryUsers = new DatabaseCountryUsers(companyUsers, companyUsers);
			AssertEquals(10m, discount1.CalculatePercent(countryUsers));
			AssertEquals(10m, discount2.CalculatePercent(countryUsers));

			companyUsers.Add(new CompanyCountryUsers(50, "AU", "AU"));
			countryUsers = new DatabaseCountryUsers(companyUsers, companyUsers);
			AssertEquals(0m, discount1.CalculatePercent(countryUsers));
			AssertEquals(10m, discount2.CalculatePercent(countryUsers));

			companyUsers.Add(new CompanyCountryUsers(1, "NZ", "NZ"));
			countryUsers = new DatabaseCountryUsers(companyUsers, companyUsers);
			AssertEquals(0m, discount2.CalculatePercent(countryUsers));
		}
	}

	[TestedType(typeof(DomesticDiscountLineCollection))]
	internal class DomesticDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DomesticDiscountLineCollection>
	{
		protected override DomesticDiscountLineCollection GetCollectionToTest()
		{
			return new DomesticDiscountLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DomesticDiscountLine();
		}
	}

	[TestedType(typeof(DomesticDiscountLine))]
	internal class DomesticDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPercent()
		{
			var line = new DomesticDiscountLine();
			line.Percent = 0;
			AssertNoErrors(line.PercentInfo);

			line.Percent = 1;
			AssertNoErrors(line.PercentInfo);

			line.Percent = 100;
			AssertNoErrors(line.PercentInfo);

			line.Percent = 101m;
			AssertHasErrors(line.PercentInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DomesticDiscountLine();
		}
	}
}
