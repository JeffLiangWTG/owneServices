using System;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	sealed class GlobalElectronicInvoiceWithCDataPayloadSerializerTest : TestCase
	{
		public void TestSerialize()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice(MessageEncoding.UTF8WithoutBOM.GetBytes(JSONPayload));
			var serializer = new GlobalElectronicInvoiceWithCDataPayloadSerializer();
			var gei = serializer.Serialize(eInvoice);
			this.AssertXMLEqualsIgnoreChildOrder("XML not match", SerializedEInvoice, gei);
		}

		public void TestDeserialize()
		{
			var serializer = new GlobalElectronicInvoiceWithCDataPayloadSerializer();
			var gei = serializer.Deserialize(SerializedEInvoice);
			AssertEquals("Test Electronic Invoicing System", gei.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("GEN", gei.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("125896", gei.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertMultilineASCIIEquals(JSONPayload, gei.Payload);
		}

		public void TestSerializeAndDeserialize_LoadFromFile()
		{
			var serializedInvoice = EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String("SampleCDataEInvoice.xml", EInvoicingTestHelper.CommonXmlFilesEmbeddedLocation);
			var serializer = new GlobalElectronicInvoiceWithCDataPayloadSerializer();
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
  <Payload><![CDATA[{JSONPayload}]]></Payload>
</GlobalElectronicInvoicing>");

		string JSONPayload =>
			@"{ 
   ""glossary"":{ 
      ""title"":""example glossary"",
      ""GlossDiv"":{ 
         ""title"":""S"",
         ""GlossList"":{ 
            ""GlossEntry"":{ 
               ""ID"":""SGML"",
               ""SortAs"":""SGML"",
               ""GlossTerm"":""Standard Generalized Markup Language"",
               ""Acronym"":""SGML"",
               ""Abbrev"":""ISO 8879:1986"",
               ""GlossDef"":{ 
                  ""para"":""A meta-markup language, used to create markup languages such as DocBook."",
                  ""GlossSeeAlso"":[ 
                     ""GML"",
                     ""XML""
                  ]
               },
               ""GlossSee"":""markup""
            }
         }
      }
   }
}";
	}
}
