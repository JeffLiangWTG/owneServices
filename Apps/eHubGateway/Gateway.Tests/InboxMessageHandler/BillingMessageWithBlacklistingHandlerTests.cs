using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class BillingMessageWithBlacklistingHandlerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			new BillingMessageWithBlacklistingHandlerMock().BlacklistedBillingClients.Clear();
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_NotSendBillingInfoWhenCachedInInvalidClientDictionary()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var billingMessageWithBlacklistingHandlerMock = new BillingMessageWithBlacklistingHandlerThrowValidationExceptionMock();

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
				try
				{
					billingMessageWithBlacklistingHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
						MessageStream = messageStream.CompressAndEncode(),
					});
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(BillingTransactionValidationException.MessagePrefix + @"
Validation Error 1
Validation Error 2", e.Message);
					Assert.AreEqual(1, billingMessageWithBlacklistingHandlerMock.BlacklistedBillingClients.Count);
					Assert.AreEqual(true, billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo);
					Assert.AreEqual(true, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
				}

				billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo = false;
				billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo = false;

				try
				{
					billingMessageWithBlacklistingHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
						MessageStream = messageStream.CompressAndEncode(),
					});
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(BillingTransactionValidationException.MessagePrefix + @"
Validation Error 1
Validation Error 2", e.Message);
					Assert.AreEqual(1, billingMessageWithBlacklistingHandlerMock.BlacklistedBillingClients.Count);
					Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo);
					Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
				}
			}
		}

		[TestMethod]
		public void TestBillingTransactionMessageV1_NotExtractBillingInfoWhenCachedInInvalidClientDictionary()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
			var billingMessageWithBlacklistingHandlerMock = new BillingMessageWithBlacklistingHandlerThrowInternalValidationExceptionMock();

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
				try
				{
					billingMessageWithBlacklistingHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
						MessageStream = messageStream.CompressAndEncode(),
					});
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(BillingTransactionValidationException.MessagePrefix + @"
Validation Error 1
Validation Error 2", e.Message);
					Assert.AreEqual(1, billingMessageWithBlacklistingHandlerMock.BlacklistedBillingClients.Count);
					Assert.AreEqual(true, billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo);
					Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
				}

				billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo = false;
				billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo = false;

				try
				{
					billingMessageWithBlacklistingHandlerMock.Handle("ABC123DEF", Guid.NewGuid(), new Common.eHubGatewayMessage()
					{
						SchemaType = Common.MessageSchemaType.Xml,
						SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
						MessageStream = messageStream.CompressAndEncode(),
					});
				}
				catch (BillingTransactionValidationException e)
				{
					Assert.AreEqual(BillingTransactionValidationException.MessagePrefix + @"
Validation Error 1
Validation Error 2", e.Message);
					Assert.AreEqual(1, billingMessageWithBlacklistingHandlerMock.BlacklistedBillingClients.Count);
					Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
					Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo);
				}
			}
		}

        [TestMethod]
        public void TestBillingTransactionMessageV1_NotSendBillingInfoWhenCachedInInvalidClientDictionaryForNonCW1Sender()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";
            var billingMessageWithBlacklistingHandlerMock = new BillingMessageWithBlacklistingHandlerThrowValidationExceptionMock();

            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                Assert.AreEqual(false, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
                try
                {
                    billingMessageWithBlacklistingHandlerMock.Handle("ABC123DEF123", Guid.NewGuid(), new Common.eHubGatewayMessage()
                    {
                        SchemaType = Common.MessageSchemaType.Xml,
                        SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.0",
                        MessageStream = messageStream.CompressAndEncode(),
                    });
                    Assert.Fail("The message handler should've thrown an exception.");
                }
                catch (BillingTransactionValidationException e)
                {
                    Assert.AreEqual(BillingTransactionValidationException.MessagePrefix + @"
Validation Error 1
Validation Error 2", e.Message);
                    Assert.AreEqual(1, billingMessageWithBlacklistingHandlerMock.BlacklistedBillingClients.Count);
                    Assert.AreEqual(true, billingMessageWithBlacklistingHandlerMock.IsCalledExtractBillingInfo);
                    Assert.AreEqual(true, billingMessageWithBlacklistingHandlerMock.IsCalledSendBillingInfo);
                }
            }
        }

		internal class BillingMessageWithBlacklistingHandlerMock : BillingMessageWithBlacklistingHandler
		{
			public ConcurrentDictionary<string, Exception> BlacklistedBillingClients { get { return blacklistedBillingClients; } }
		}

		class BillingMessageWithBlacklistingHandlerThrowValidationExceptionMock : BillingMessageWithBlacklistingHandler
		{
			public bool IsCalledSendBillingInfo { get; set; }
			public bool IsCalledExtractBillingInfo { get; set; }

			public ConcurrentDictionary<string, Exception> BlacklistedBillingClients { get { return blacklistedBillingClients; } }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				IsCalledSendBillingInfo = true;
				throw new BillingTransactionValidationException(new[] { "Validation Error 1", "Validation Error 2" });
			}

			protected override BillingTransaction DeserializeMessageContent(eHubGatewayMessage message)
			{
				IsCalledExtractBillingInfo = true;
				return base.DeserializeMessageContent(message);
			}

            internal override bool IsCW1Sender(string senderID, eHubGatewayMessage message)
            {
                var result = true;
                if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
                {
                    result = false;
                }
                return result;
            }
		}

		class BillingMessageWithBlacklistingHandlerThrowInternalValidationExceptionMock : BillingMessageWithBlacklistingHandler
		{
			public bool IsCalledSendBillingInfo { get; set; }
			public bool IsCalledExtractBillingInfo { get; set; }

			public ConcurrentDictionary<string, Exception> BlacklistedBillingClients { get { return blacklistedBillingClients; } }

			internal override void SendBillingInfo(string trackingId, BillingTransaction transaction)
			{
				IsCalledSendBillingInfo = true;
			}

			protected override BillingTransaction DeserializeMessageContent(eHubGatewayMessage message)
			{
				IsCalledExtractBillingInfo = true;
				throw new BillingTransactionValidationException(new[] { "Validation Error 1", "Validation Error 2" });
			}
		}
	}
}
