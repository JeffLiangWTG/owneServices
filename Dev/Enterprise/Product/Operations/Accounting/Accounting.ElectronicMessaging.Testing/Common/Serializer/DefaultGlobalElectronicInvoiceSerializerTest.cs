using System;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	sealed class DefaultGlobalElectronicInvoiceSerializerTest : TestCase
	{
		public void TestSerialize()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice(MessageEncoding.UTF8WithoutBOM.GetBytes(Base64Payload));
			var serializer = new DefaultGlobalElectronicInvoiceSerializer();
			var gei = serializer.Serialize(eInvoice);
			this.AssertXMLEqualsIgnoreChildOrder("XML not match", SerializedEInvoice, gei);
		}

		public void TestDeserialize()
		{
			var serializer = new DefaultGlobalElectronicInvoiceSerializer();
			var gei = serializer.Deserialize(SerializedEInvoice);
			AssertEquals("Test Electronic Invoicing System", gei.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("GEN", gei.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("125896", gei.Header.ElectronicInvoiceBatchRequest.BatchNumber);

			var base64Payload = @"eyJEYXRlQW5kVGltZU9mSXNzdWUiOiIyMDE5LTA3LTE2VDE1OjI2OjAwWiIsIkJEIjoiMTIzNDU2Nzg3OTgiLCJJVCI6MCwiVFQiOjAsIlBheW1lbnRUeXBlIjowLCJJbnZvaWNlTnVtYmVyIjoiMDAwMDEwMDAiLCJPcHRpb25zIjp7Ik9taXRRUkNvZGVHZW4iOiIxIiwiT21pdFRleHR1YWxSZXByZXNlbnRhdGlvbiI6IjEifSwiSGFzaCI6IkZVYURWRUJyUmx5TUNKeVBhMGEyYlE9PSIsIkl0ZW1zIjpbeyJOYW1lIjoiQ2hhcmdlIENvZGUgMSIsIlF1YW50aXR5IjoxLCJMYWJlbHMiOlsiQSJdLCJUb3RhbEFtb3VudCI6MTAwLjAwMDB9XX0=";
			AssertMultilineASCIIEquals(base64Payload, gei.Payload);
		}

		public void TestSerializeAndDeserialize_LoadFromFile()
		{
			var serializedInvoice = EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String("SampleBase64EInvoice.xml", EInvoicingTestHelper.CommonXmlFilesEmbeddedLocation);
			var serializer = new DefaultGlobalElectronicInvoiceSerializer();
			var gei = serializer.Deserialize(serializedInvoice);
			gei.TransactionBatchSpecified = false;
			var reSerializedInvoice = serializer.Serialize(gei);
			this.AssertXMLEqualsIgnoreChildOrder("XML not match", serializedInvoice, reSerializedInvoice);
		}

		string SerializedEInvoice => FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>GEN</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>{Base64Payload}</Payload>
</GlobalElectronicInvoicing>");

		string Base64Payload => Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(JSONPayload));

		string JSONPayload => @"{""DateAndTimeOfIssue"":""2019-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":0,""PaymentType"":0,""InvoiceNumber"":""00001000"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""FUaDVEBrRlyMCJyPa0a2bQ=="",""Items"":[{""Name"":""Charge Code 1"",""Quantity"":1,""Labels"":[""A""],""TotalAmount"":100.0000}]}";
	}
}
