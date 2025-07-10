using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrderOrderDetailReferenceNumber))]
	sealed class OrderOrderDetailReferenceNumberTest : ValueObjectTestCase
	{
		public void TestReferenceNumber()
		{
			Xsd.OrderOrderDetailReferenceNumber referenceNumber = Xsd.OrderOrderDetailReferenceNumber.FromReferenceNumberType(Xsd.OrderOrderDetailReferenceNumberType.Shipment, "S0000001");
			Assert("Should be Specified", referenceNumber.IsSpecified);
			AssertEquals("Type should be Shipment", Xsd.OrderOrderDetailReferenceNumberType.Shipment, referenceNumber.Type);
			AssertEquals("Value should be S0000001", "S0000001", referenceNumber.Value);

			referenceNumber = Xsd.OrderOrderDetailReferenceNumber.FromReferenceNumberType(Xsd.OrderOrderDetailReferenceNumberType.Declaration, "B0000001");
			Assert("Should be Specified", referenceNumber.IsSpecified);
			AssertEquals("Type should be Declaration", Xsd.OrderOrderDetailReferenceNumberType.Declaration, referenceNumber.Type);
			AssertEquals("Value should be B0000001", "B0000001", referenceNumber.Value);

			referenceNumber = Xsd.OrderOrderDetailReferenceNumber.FromReferenceNumberType(Xsd.OrderOrderDetailReferenceNumberType.Declaration, "");
			Assert("Should be NOT Specified", !referenceNumber.IsSpecified);
			referenceNumber = Xsd.OrderOrderDetailReferenceNumber.FromReferenceNumberType(Xsd.OrderOrderDetailReferenceNumberType.Shipment, "");
			Assert("Should be NOT Specified", !referenceNumber.IsSpecified);
		}
	}
}
