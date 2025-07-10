using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom))]
	sealed class OrderOrderLineOrderLineDeliveryDeliveryDetailsCustomTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				72, typeof(OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom custom = new OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom();
			AssertEquals(false, custom.IsSpecified);

			//decimal fields
			custom.Decimal1Specified = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Decimal1Specified = false;
			custom.Decimal2Specified = true;

			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Decimal2Specified = false;
			custom.Decimal3Specified = true;

			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Decimal3Specified = false;
			custom.Decimal4Specified = true;

			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Decimal4Specified = false;
			custom.Decimal5Specified = true;

			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			//text field
			custom.Decimal5Specified = false;
			custom.Text1 = "text1";
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Text1 = "";
			custom.Text2 = "text2";
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Text2 = "";
			custom.Text3 = "text3";
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Text3 = "";
			custom.Text4 = "text4";
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Text4 = "";
			custom.Text5 = "text5";
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Text5 = "";
			//boolean field
			AssertEquals("Is Specified is false", false, custom.IsSpecified);

			custom.Flag1Specified = true;
			custom.Flag1 = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Flag1Specified = false;
			custom.Flag2 = false;
			custom.Flag2Specified = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Flag2Specified = false;
			custom.Flag3 = false;
			custom.Flag3Specified = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Flag3Specified = false;
			custom.Flag4 = false;
			custom.Flag4Specified = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Flag4Specified = false;
			custom.Flag5 = false;
			custom.Flag5Specified = true;
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			//date fields
			custom.Flag5Specified = false;
			custom.Date1 = new ZDateTime(2009, 12, 12);
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Date1 = ZDateTime.Empty;
			custom.Date2 = new ZDateTime(2009, 12, 12);
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Date2 = ZDateTime.Empty;
			custom.Date3 = new ZDateTime(2009, 12, 12);
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Date3 = ZDateTime.Empty;
			custom.Date4 = new ZDateTime(2009, 12, 12);
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Date4 = ZDateTime.Empty;
			custom.Date5 = new ZDateTime(2009, 12, 12);
			AssertEquals("Is Specified is true", true, custom.IsSpecified);

			custom.Date5 = ZDateTime.Empty;
			AssertEquals("Is Specified is false", false, custom.IsSpecified);
		}
	}
}
