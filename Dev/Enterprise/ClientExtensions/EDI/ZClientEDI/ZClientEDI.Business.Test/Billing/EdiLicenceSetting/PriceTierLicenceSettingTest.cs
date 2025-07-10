using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceTierLicenceSetting))]
	internal class PriceTierLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestProperties()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var setting = Factory.New<PriceTierLicenceSetting>();
			lic.Database.LicenceSettings.Add(setting);
			AssertEquals("PRT", setting.LS9_Type);
			AssertEquals(true, setting.EnableUnits);
			AssertEquals(true, setting.SupportUnitBreak);
			setting.LS9_ValidFrom = new ZDateTime(2023, 1, 1);
			setting.PriceKey = new UsageCodeKey("STL", "CTR");
			AssertEquals("STL", setting.PriceCategory);
			AssertEquals("CTR", setting.PriceCode);

			var line1 = setting.Lines.AddNew();
			line1.UnitBreak = 0;
			line1.Price = 1;
			line1.Units = 100;
			var line2 = setting.Lines.AddNew();
			line2.UnitBreak = 5;
			line2.Price = 50;
			line2.Units = 500;
			var line3 = setting.Lines.AddNew();
			line3.UnitBreak = 10;
			line3.Price = 100;
			line3.Units = 1000;

			var result = setting.GetPriceAndUnits(5);
			AssertEquals(50m, result.Price.Value);
			AssertEquals(500m, result.Units.Value);

			result = setting.GetPriceAndUnits(100);
			AssertEquals(false, result.Price.HasValue);
			AssertEquals(false, result.Units.HasValue);

			AssertExceptionThrown<NotImplementedException>(() => setting.GetPriceAndUnits());

			Factory.Save();

			var settingInNewFactory = new BusinessObjectFactory().Load<PriceTierLicenceSetting>(setting.PK);
			AssertEquals("PRT", settingInNewFactory.LS9_Type);
			AssertEquals(true, settingInNewFactory.EnableUnits);
			AssertEquals(true, settingInNewFactory.SupportUnitBreak);
			AssertEquals("STL", settingInNewFactory.PriceCategory);
			AssertEquals("CTR", settingInNewFactory.PriceCode);
			result = settingInNewFactory.GetPriceAndUnits(10);
			AssertEquals(100m, result.Price.Value);
			AssertEquals(1000m, result.Units.Value);
		}
	}

	[TestedType(typeof(PriceTierLicenceSettingLine))]
	public class PriceTierLicenceSettingLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var collection = new PriceTierLicenceSettingLineCollection();
			var line1 = collection.AddNew();
			line1.UnitBreak = -1;
			AssertHasError(line1.UnitBreakInfo, "Unit Break cannot be negative.");
			line1.UnitBreak = 10;
			AssertNoErrors(line1.UnitBreakInfo);

			var line2 = collection.AddNew();
			line2.UnitBreak = 10;
			AssertHasError(line2.UnitBreakInfo, "Duplicate Unit Break not allowed.");
			line2.UnitBreak = 20;
			AssertNoErrors(line2.UnitBreakInfo);
		}

		public void TestXmlSerialization()
		{
			var line = new PriceTierLicenceSettingLine();
			line.UnitBreak = 10;
			line.Price = 100;
			line.Units = 200;

			var serializer = ZXmlSerializer.New(typeof(PriceTierLicenceSettingLine));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, line);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var line2 = (PriceTierLicenceSettingLine)serializer.Deserialize(reader);
			AssertEquals(10, line2.UnitBreak);
			AssertEquals(100m, line2.Price);
			AssertEquals(200m, line2.Units);
		}
	}

	[TestedType(typeof(PriceTierLicenceSettingLineCollection))]
	internal class PriceTierLicenceSettingLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceTierLicenceSettingLineCollection>
	{
		protected override PriceTierLicenceSettingLineCollection GetCollectionToTest()
		{
			return new PriceTierLicenceSettingLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PriceTierLicenceSettingLine();
		}
	}
}
