using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(WebShipmentCustom))]
	sealed class WebShipmentCustomTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				30, typeof(WebShipmentCustom).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			WebShipmentCustom custom = new WebShipmentCustom();
			AssertEquals(false, custom.IsSpecified);

			custom.CustomAttrib1 = "Value";
			AssertEquals(true, custom.IsSpecified);
			custom.CustomAttrib1 = "";

			custom.CustomAttrib2 = "Value";
			AssertEquals(true, custom.IsSpecified);
			custom.CustomAttrib2 = "";

			custom.CustomDate1 = ZDateTime.Now;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomDate1 = ZDateTime.Empty;

			custom.CustomDate2 = ZDateTime.Now;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomDate2 = ZDateTime.Empty;

			custom.CustomDecimal1 = 1m;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomDecimal1 = 0m;

			custom.CustomDecimal2 = 1m;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomDecimal2 = 0m;

			custom.CustomFlag1 = true;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomFlag1 = false;

			custom.CustomFlag2 = true;
			AssertEquals(true, custom.IsSpecified);
			custom.CustomFlag2 = false;
		}
	}
}
