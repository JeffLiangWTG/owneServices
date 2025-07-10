using CargoWise.Types;
using Enterprise.DataTransfer.DataFileDefinitions.Xml.XsdValueObjects;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(PackageCustom))]
	sealed class PackageCustomTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				38, typeof(PackageCustom).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			PackageCustom custom = new PackageCustom();
			AssertEquals(false, custom.IsSpecified);

			custom.Date1 = ZDateTime.Now;
			AssertEquals(true, custom.IsSpecified);
			custom.Date1 = ZDateTime.Empty;

			custom.Date2 = ZDateTime.Now;
			AssertEquals(true, custom.IsSpecified);
			custom.Date2 = ZDateTime.Empty;

			custom.Text1 = "blah";
			AssertEquals(true, custom.IsSpecified);
			custom.Text1 = ZString.Empty;

			custom.Text2 = "blah";
			AssertEquals(true, custom.IsSpecified);
			custom.Text2 = ZString.Empty;

			custom.Text3 = "blah";
			AssertEquals(true, custom.IsSpecified);
			custom.Text3 = ZString.Empty;

			custom.Text4 = "blah";
			AssertEquals(true, custom.IsSpecified);
			custom.Text4 = ZString.Empty;

			custom.Decimal1 = new ZDecimal(123m);
			AssertEquals(true, custom.IsSpecified);
			custom.Decimal1 = ZDecimal.Zero;
			custom.Decimal1Specified = false;

			custom.Decimal2 = new ZDecimal(123m);
			AssertEquals(true, custom.IsSpecified);
			custom.Decimal2 = ZDecimal.Zero;
			custom.Decimal2Specified = false;

			custom.Flag1 = TrueFalse.@true;
			AssertEquals(true, custom.IsSpecified);
			custom.Flag1Specified = false;

			custom.Flag2 = TrueFalse.@false;
			AssertEquals(true, custom.IsSpecified);
			custom.Flag2Specified = false;
		}
	}
}
