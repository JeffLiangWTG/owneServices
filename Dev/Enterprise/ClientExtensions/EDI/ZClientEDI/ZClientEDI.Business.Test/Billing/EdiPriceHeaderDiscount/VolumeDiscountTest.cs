using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(VolumeDiscount))]
	internal class VolumeDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var discount = new VolumeDiscount();

			var line1 = discount.Lines.AddNew();
			line1.UnitCount = 0;
			line1.Percent = 10m;

			var line2 = discount.Lines.AddNew();
			line2.UnitCount = 200;
			line2.Percent = 20m;

			var line3 = discount.Lines.AddNew();
			line3.UnitCount = -1;
			line3.Percent = 45m;
			discount.RunPreSaveValidation();

			AssertHasErrors(line1.UnitCountInfo);
			AssertNoErrors(line2.UnitCountInfo);
			AssertHasErrors(line3.UnitCountInfo);
		}

		public void TestXmlSerialization()
		{
			var discount = new VolumeDiscount();
			var line1 = discount.Lines.AddNew();
			line1.UnitCount = 100;
			line1.Percent = 40.38m;

			var line2 = discount.Lines.AddNew();
			line2.UnitCount = 150;
			line2.Percent = 45.19m;

			var line3 = discount.Lines.AddNew();
			line3.UnitCount = 9000000000m;
			line3.Percent = 60m;

			var serializer = ZXmlSerializer.New(typeof(VolumeDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (VolumeDiscount)serializer.Deserialize(reader);

			AssertEquals(3, discount2.Lines.Count);
			AssertEquals(100m, discount2.Lines[0].UnitCount);
			AssertEquals(40.38m, discount2.Lines[0].Percent);
			AssertEquals(150m, discount2.Lines[1].UnitCount);
			AssertEquals(45.19m, discount2.Lines[1].Percent);
			AssertEquals(9000000000m, discount2.Lines[2].UnitCount);
			AssertEquals(60m, discount2.Lines[2].Percent);
		}
	}

	[TestedType(typeof(VolumeDiscountLineCollection))]
	internal class VolumeDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VolumeDiscountLineCollection>
	{
		protected override VolumeDiscountLineCollection GetCollectionToTest()
		{
			var parent = new VolumeDiscount();
			return new VolumeDiscountLineCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VolumeDiscountLine();
		}
	}

	[TestedType(typeof(VolumeDiscountLine))]
	internal class VolumeDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnitCount()
		{
			var parent = new VolumeDiscount();
			var line = parent.Lines.AddNew();
			line.UnitCount = 0;
			AssertHasErrors(line.UnitCountInfo);

			line.UnitCount = 1;
			AssertNoErrors(line.UnitCountInfo);

			line.UnitCount = -1;
			AssertHasErrors(line.UnitCountInfo);
		}

		public void TestPercent()
		{
			var parent = new VolumeDiscount();
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
