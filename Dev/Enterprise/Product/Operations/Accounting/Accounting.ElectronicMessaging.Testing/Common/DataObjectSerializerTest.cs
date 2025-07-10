using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class DataObjectSerializerTest : TestCaseWithFactory
	{
		public void TestSerialization()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice();

			using (var stream = new MemoryStream())
			{
				var serializer = new DataObjectSerializer();
				serializer.Serialize(eInvoice, stream);
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var xml = reader.ReadToEnd();
					var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>GEN</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>Test Payload</Payload>
</GlobalElectronicInvoicing>";
					this.AssertXMLEqualsIgnoreChildOrder("XML not match",expectedMessage, xml);
				}
			}
		}

		public void TestDeSerialization()
		{
			var serializedObj = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>TST</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>Test Payload</Payload>
</GlobalElectronicInvoicing>";
			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedObj);
			AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("MessageType", "TST", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);

			AssertEquals("Payload", "Test Payload", deserializedObj.Payload);
			AssertEquals("TransactionBatch.Transactions.Length", 0, deserializedObj.TransactionBatch?.Transactions?.Length ?? 0);
			AssertNullOrEmpty("Transaction", deserializedObj.Transaction);
		}

		public void TestSerialization_WithCompanyAndBranch()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoiceWithComapnyAndBranch();

			using (var stream = new MemoryStream())
			{
				var serializer = new DataObjectSerializer();
				serializer.Serialize(eInvoice, stream);
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var xml = reader.ReadToEnd();
					var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>GEN</MessageType>
      <BatchNumber>125896</BatchNumber>
      <CompanyCode>COM</CompanyCode>
      <BranchCode>BRN</BranchCode>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>Test Payload</Payload>
</GlobalElectronicInvoicing>";
					this.AssertXMLEqualsIgnoreChildOrder("XML not match", expectedMessage, xml);
				}
			}
		}

		public void TestDeSerialization_WithCompanyAndBranch()
		{
			var serializedObj = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>TST</MessageType>
      <BatchNumber>125896</BatchNumber>
      <CompanyCode>COM</CompanyCode>
      <BranchCode>BRN</BranchCode>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>Test Payload</Payload>
</GlobalElectronicInvoicing>";
			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedObj);
			AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("MessageType", "TST", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("Company", "COM", deserializedObj.Header.ElectronicInvoiceBatchRequest.CompanyCode);
			AssertEquals("Branch", "BRN", deserializedObj.Header.ElectronicInvoiceBatchRequest.BranchCode);

			AssertEquals("Payload", "Test Payload", deserializedObj.Payload);
			AssertEquals("TransactionBatch.Transactions.Length", 0, deserializedObj.TransactionBatch?.Transactions?.Length ?? 0);
			AssertNullOrEmpty("Transaction", deserializedObj.Transaction);
		}

		public void TestSerialization_WithBatchedTransactions()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice();
			eInvoice.TransactionBatch = new GlobalElectronicInvoicingTransactionBatch
			{
				Transactions = new string[]
				{
					"a",
					"b",
					"c",
				}
			};
			eInvoice.Payload = null;
			Assert("Precondition: eInvoice.TransactionBatchSpecified = true", eInvoice.TransactionBatchSpecified);

			using (var stream = new MemoryStream())
			{
				var serializer = new DataObjectSerializer();
				serializer.Serialize(eInvoice, stream);
				stream.Position = 0;
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var xml = reader.ReadToEnd();
					var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>GEN</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <TransactionBatch>
    <Transactions>
      <Transaction>a</Transaction>
      <Transaction>b</Transaction>
      <Transaction>c</Transaction>
    </Transactions>
  </TransactionBatch>
</GlobalElectronicInvoicing>";
					this.AssertXMLEqualsIgnoreChildOrder("XML should match", expectedMessage, xml);
				}
			}
		}

		public void TestDeSerialization_WithBatchedTransactions()
		{
			var serializedObj = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>TST</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <TransactionBatch>
    <Transactions>
      <Transaction>a</Transaction>
      <Transaction>b</Transaction>
      <Transaction>c</Transaction>
    </Transactions>
  </TransactionBatch>
</GlobalElectronicInvoicing>";
			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedObj);
			AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("MessageType", "TST", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);

			AssertNullOrEmpty("Payload", deserializedObj.Payload);
			AssertNullOrEmpty("Transaction", deserializedObj.Transaction);

			AssertNotNull("TransactionBatch.Transactions", deserializedObj.TransactionBatch?.Transactions);
			AssertArrayEqualsByElements(new string[] { "a", "b", "c" }, deserializedObj.TransactionBatch.Transactions);
		}

		public void TestSerialization_WithSingleTransaction()
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice();
			eInvoice.Transaction = "a";
			eInvoice.Payload = null;

			using (var stream = new MemoryStream())
			{
				var serializer = new DataObjectSerializer();
				serializer.Serialize(eInvoice, stream);
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var xml = reader.ReadToEnd();
					var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>GEN</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Transaction>a</Transaction>
</GlobalElectronicInvoicing>";
					this.AssertXMLEqualsIgnoreChildOrder("XML should match", expectedMessage, xml);
				}
			}
		}

		public void TestDeSerialization_WithSingleTransaction()
		{
			var serializedObj = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GlobalElectronicInvoicing xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>TST</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Transaction>a</Transaction>
</GlobalElectronicInvoicing>";
			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedObj);
			AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("MessageType", "TST", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);

			AssertNullOrEmpty("TransactionBatch.Payload", deserializedObj.Payload);
			AssertNull("TransactionBatch.Transactions", deserializedObj.TransactionBatch?.Transactions);
			AssertEquals("TransactionBatch.Transactions.Length", 0, deserializedObj.TransactionBatch?.Transactions?.Length ?? 0);

			AssertEquals("Transaction", "a", deserializedObj.Transaction);
		}
	}
}