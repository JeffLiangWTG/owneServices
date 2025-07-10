using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MarketingManager.Testing
{
	[TestedType(typeof(DocBounceBackEmailProcessResult))]
	sealed class DocBounceBackEmailProcessResultTest : DocumentWrapperTestCase
	{
		public void TestJobNumber()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.JobNumber);

			processResult.JobNumber = "A00001000";
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("A00001000", wrapper.JobNumber);
		}

		public void TestDocumentName()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.DocumentName);

			processResult.DocumentName = "Document To Deliver";
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("Document To Deliver", wrapper.DocumentName);
		}

		public void TestRecipient()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.Recipient);

			processResult.BouncedRecipients = "abc@123.com;user@abc.net";
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("abc@123.com;user@abc.net", wrapper.Recipient);
		}

		public void TestBouncedReasonCode()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.BouncedReasonCode);

			processResult.BounceReasonCode = "550";
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("550", wrapper.BouncedReasonCode);
		}

		public void TestOriginEmailSentTime()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.OriginEmailSentTime);

			processResult.SentTimeText = "Tue, 15 Aug 2017 14:58:51 +1000";
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("Tue, 15 Aug 2017 14:58:51 +1000", wrapper.OriginEmailSentTime);
		}

		public void TestNDREmail()
		{
			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("00000000-0000-0000-0000-000000000000", wrapper.NonDeliveryReceiptEmailPK);
			AssertEquals("", wrapper.NonDeliveryReceiptEmailReceivedTime);

			processResult.MailItemPK = Guid.Parse("b47664f2-6cdb-4e16-a499-36a4d2e38c3b");
			processResult.MailItemReceivedTimeUtc = new ZDateTime(2017, 8, 15, 10, 10, 10);
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("b47664f2-6cdb-4e16-a499-36a4d2e38c3b", wrapper.NonDeliveryReceiptEmailPK);
			AssertEquals(new ZDateTime(2017, 8, 15, 10, 10, 10).ToLongTimeString(), wrapper.NonDeliveryReceiptEmailReceivedTime);
		}

		public void TestEmailSender()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "User A";
			staff.GS_EmailAddress = "aaa@123.com";

			var processResult = new BounceBackEmailProcessResult();
			var wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals(string.Empty, wrapper.SenderEmailAddress);
			AssertEquals(string.Empty, wrapper.SenderStaffName);
			AssertEquals(string.Empty, wrapper.SenderStaffCode);

			processResult.SenderStaff = staff;
			wrapper = DocBounceBackEmailProcessResult.New(processResult, Factory);
			AssertEquals("aaa@123.com", wrapper.SenderEmailAddress);
			AssertEquals("User A", wrapper.SenderStaffName);
			AssertEquals("AAA", wrapper.SenderStaffCode);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var processResult = new BounceBackEmailProcessResult();
			return new DocumentWrapper[] { DocBounceBackEmailProcessResult.New(processResult, Factory) };
		}
	}
}
