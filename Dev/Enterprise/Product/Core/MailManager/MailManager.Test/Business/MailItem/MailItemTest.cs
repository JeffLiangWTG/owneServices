using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailItemTest : TestCaseWithFactory
	{
		public void TestTruncateSenderFromInfo_WhenHeaderFromIsLessThanMaxLength()
		{
			MyItem = Factory.New<MailItem>();
			var fromHeaderItem = @"Nick Martin <nick.martin@edi.com.au>";
			var maxLength = fromHeaderItem.Length + 1;

			AssertEquals(@"Nick Martin <nick.martin@edi.com.au>", MyItem.TruncateSenderFromInfo(fromHeaderItem, maxLength));
		}

		public void TestTruncateSenderFromInfo_WhenHeaderFromIsGreaterThanMaxLength()
		{
			MyItem = Factory.New<MailItem>();
			var sender = "Nick Martin";
			var fromHeaderItem = $@"{sender} <nick@edi.com.au>";
			var maxLength = fromHeaderItem.Length - (sender.Length / 2);

			AssertEquals($"Sender='{sender}' should have been truncated.", @"Nick Ma<nick@edi.com.au>", MyItem.TruncateSenderFromInfo(fromHeaderItem, maxLength));
		}

		public void TestTruncateSenderFromInfo_WhenEmailAddressLengthIsLongerThanMaxLength()
		{
			MyItem = Factory.New<MailItem>();
			var emailAddress = @"<nick.martin.reallylongEmailaddress@edi.com.au>";
			var fromHeaderItem = $@"Nick Martin {emailAddress}";
			var maxLength = emailAddress.Length / 2;

			AssertExceptionThrown(typeof(InvalidOperationException), $"Email address is longer than max length. headerFrom:{fromHeaderItem}, maxLength:{maxLength}", () => MyItem.TruncateSenderFromInfo(fromHeaderItem, maxLength));
		}

		public void TestTruncateSenderFromInfo_WhenSenderLengthIsLongerThanMaxLength()
		{
			MyItem = Factory.New<MailItem>();
			var sender = "Nicholas Reallylongsendername Martin";
			var fromHeaderItem = $@"{sender} <nick@edi.com.au>";
			var maxLength = sender.Length / 2;

			AssertEquals($"Sender='{sender}' should have been truncated.", @"N<nick@edi.com.au>", MyItem.TruncateSenderFromInfo(fromHeaderItem, maxLength));
		}

		public void TestTruncateSenderFromInfo_WhenSenderHasLotsOfTrailingWhiteSpace()
		{
			MyItem = Factory.New<MailItem>();
			var sender = "Nick Martin";
			var whiteSpace = "     ";
			var fromHeaderItem = $@"{sender}{whiteSpace} <nick@edi.com>";
			var maxLength = fromHeaderItem.Length - 1;

			AssertEquals($"Sender='{sender}' should have been truncated.", $@"Nick Martin{whiteSpace}<nick@edi.com>", MyItem.TruncateSenderFromInfo(fromHeaderItem, maxLength));
		}

		public void TestMI_FromAndMI_ReplyToMaxLength()
		{
			var email = Factory.New<MailItem>();
			var emailPrefix = new string('A', 150);
			var emailHeader = @$"From: Australia Demo Company <{emailPrefix}@wtg.com>
ReplyTo: Australia Demo Company <{emailPrefix}@gmail.com>";

			AssertNoExceptionThrown(() => email.MI_Header = emailHeader);

			var emailSuffix = "@wtg.com";
			emailPrefix = new string('A', MailItem.Schema.MI_FromMaxLength - emailSuffix.Length);
			emailHeader = @$"From: Australia Demo Company <{emailPrefix}{emailSuffix}>
ReplyTo: Australia Demo Company <{emailPrefix}{emailSuffix}>";

			var email2 = Factory.New<MailItem>();
			AssertNoExceptionThrown(() => email2.MI_Header = emailHeader);
			AssertEquals("If emailAddr is too long, the name will be omitted, so angle brackets are not needed.", emailPrefix + emailSuffix, email2.MI_From);
			AssertEquals("If emailAddr is too long, the name will be omitted, so angle brackets are not needed.", emailPrefix + emailSuffix, email2.MI_ReplyTo);
		}

		public void TestNonASCIICharactersAreRemovedWhenSettingHeader()
		{
			var contentWithNonAsciiChars = "拼箱 C00022170 (提单=《“TXSV91413500”》)";
			var contentWithoutInvalidPunctuations = "拼箱 C00022170 (提单=TXSV91413500)";
			var contentWithoutNonAsciiChars = " C00022170 (=TXSV91413500)";

			DataRegistry.Instance.RemoveNonAsciiCharactersInEmailMessageHeader = true;
			MyItem.MI_Header = contentWithNonAsciiChars;
			AssertEquals(contentWithoutNonAsciiChars, MyItem.MI_Header);

			DataRegistry.Instance.RemoveNonAsciiCharactersInEmailMessageHeader = false;
			MyItem.MI_Header = contentWithNonAsciiChars;
			AssertEquals(contentWithoutInvalidPunctuations, MyItem.MI_Header);
		}

		public void TestEmailDestinationOverride()
		{
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";

			MyItem.RemoveAndDeleteAllMailRecipients();

			AssertEquals("Recipient count", 0, MyItem.MailRecipients.Count);
			AssertEquals("Overriden recipient count", 0, MyItem.overriddenRecipients.Count);

			MyItem.AddRecipientForUserCommunication("peter.griffin@cargowise.com");
			MyItem.AddRecipientForUserCommunication("homer.simpson@cargowise.com");

			AssertEquals("Recipient count", 1, MyItem.MailRecipients.Count);
			AssertEquals("override@cargowise.com", MyItem.MailRecipients[0].EmailAddress);
			AssertEquals("Overriden recipient count", 2, MyItem.overriddenRecipients.Count);
			Assert(MyItem.overriddenRecipients.Contains("peter.griffin@cargowise.com"));
			Assert(MyItem.overriddenRecipients.Contains("homer.simpson@cargowise.com"));
		}

		public void TestSystemEmailDestinationOverride()
		{
			using (EnvProxy.Instance.Registry.RawRegistry.EmailDestinationOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "override@cargowise.com"))
			{
				MyItem.RemoveAndDeleteAllMailRecipients();

				AssertEquals("Recipient count", 0, MyItem.MailRecipients.Count);
				AssertEquals("Overriden recipient count", 0, MyItem.overriddenRecipients.Count);

				MyItem.AddRecipientForSystemCommunication("peter.griffin@cargowise.com");
				MyItem.AddRecipientForSystemCommunication("homer.simpson@cargowise.com");

				AssertEquals("Recipient count", 1, MyItem.MailRecipients.Count);
				AssertEquals("override@cargowise.com", MyItem.MailRecipients[0].EmailAddress);
				AssertEquals("Overriden recipient count", 2, MyItem.overriddenRecipients.Count);
				Assert(MyItem.overriddenRecipients.Contains("peter.griffin@cargowise.com"));
				Assert(MyItem.overriddenRecipients.Contains("homer.simpson@cargowise.com"));
			}
		}

		public void TestSystemEmailDestinationOverrideIsFalse()
		{
			using (EnvProxy.Instance.Registry.RawRegistry.EmailDestinationOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "override@cargowise.com"))
			using (EnvProxy.Instance.Registry.RawRegistry.SystemEmailDestinationOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				MyItem.RemoveAndDeleteAllMailRecipients();

				AssertEquals("Recipient count", 0, MyItem.MailRecipients.Count);
				AssertEquals("Overriden recipient count", 0, MyItem.overriddenRecipients.Count);

				MyItem.AddRecipientForSystemCommunication("peter.griffin@cargowise.com");
				MyItem.AddRecipientForSystemCommunication("homer.simpson@cargowise.com");

				AssertEquals("Recipient count", 2, MyItem.MailRecipients.Count);
				AssertEquals("peter.griffin@cargowise.com", MyItem.MailRecipients[0].EmailAddress);
				AssertEquals("homer.simpson@cargowise.com", MyItem.MailRecipients[1].EmailAddress);
				AssertEquals("Overriden recipient count", 0, MyItem.overriddenRecipients.Count);
			}
		}

		public void TestMarkMailItemAsFailed()
		{
			MyItem.MI_ReceivedDateTime = ZDateTime.Now;
			MyItem.MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();
			Assert("Precondition", MailStatus.Failed != MyItem.MI_Status);
			MailItem.MarkMailItemAsFailed(MyItem.PK);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			MailItem myItemInAnotherFactory = newFactory.Load<MailItem>(MyItem.PK);
			AssertEquals("Now failed", MailStatus.Failed, myItemInAnotherFactory.MI_Status);
		}

		public void TestHeaderParsedCorrectly()
		{
			MyItem.RemoveAndDeleteAllMailRecipients();
			MyItem.MI_Header = "To: \"Test, DJ\" <djtest@edi.com.au>,\r\n\t\"Test 2, \\\"DJ\\\"! \\\\r\\\\n\" <djtest2@edi.com.au>,\r\n bob@example.com, fred@example.com\r\nCc: mary@example.com, dave@example.com,\r\n\tmatt@example.com\r\n";
			AssertEquals("Recipient count", 7, MyItem.MailRecipients.Count);

			MyItem.RemoveAndDeleteAllMailRecipients();
			MyItem.MI_Header = @"
To: undisclosed-recipients:;
BCC: <support@cargowise.com>
";
			AssertEquals("Recipient count", 0, MyItem.MailRecipients.Count);
		}

		public void TestBodyTextDecoded()
		{
			MyItem.MI_Header = @"Return-path: <ben@edi.com.au>  
X-MimeOLE: Produced By Microsoft Exchange V6.5.7226.0  
Content-class: urn:content-classes:message  
MIME-Version: 1.0  
Content-Type: text/plain; charset=""us-ascii"" 
Content-Transfer-Encoding: quoted-printable  
Subject: AMIMFT  
Date: Thu, 28 Oct 2004 11:33:37 +1000  
Message-ID: <1B2B33FE8EF8C2439A2C345FA2FD5EB401F5498F@XC.syd.edi>  
Thread-Topic: AMIMFT  
Thread-Index: AcS8jiVsVcGFd8fvTuCS7j3XqQ5k1A==  
From: ""Ben Govett"" <ben@edi.com.au>  
To: <dvedi_bg@acsedi.edi.net.au>  
";
			MyItem.MI_Body = @"FNO  1949020  =20
BKG  P3897043
HWB  15392                   =20
HDES SYD  =20
TYP  PPD
ACC  #AI528   =20
PCS  1  =20
WGT  116     =20
DE1  PRESS TOOL";
			string expectedValue = @"FNO  1949020   
BKG  P3897043
HWB  15392                    
HDES SYD   
TYP  PPD
ACC  #AI528    
PCS  1   
WGT  116      
DE1  PRESS TOOL";
			string actualValue = MyItem.BodyTextDecoded;
			AssertEquals(expectedValue, actualValue);

			MyItem.MI_Header = @"Return-path: <ben@edi.com.au>  
X-MimeOLE: Produced By Microsoft Exchange V6.5.7226.0  
Content-class: urn:content-classes:message  
MIME-Version: 1.0  
Content-Type: text/plain; charset=""us-ascii"" 
Content-Transfer-Encoding: NOTquoted-NOTprintable  
Subject: AMIMFT  
Date: Thu, 28 Oct 2004 11:33:37 +1000  
Message-ID: <1B2B33FE8EF8C2439A2C345FA2FD5EB401F5498F@XC.syd.edi>  
Thread-Topic: AMIMFT  
Thread-Index: AcS8jiVsVcGFd8fvTuCS7j3XqQ5k1A==  
From: ""Ben Govett"" <ben@edi.com.au>  
To: <dvedi_bg@acsedi.edi.net.au>  
";
			MyItem.MI_Body = @"BODY";

			AssertEquals("No Decoding", MyItem.MI_Body, MyItem.BodyTextDecoded);
		}

		public void TestMailItem()
		{
			AssertNotNull("MyItem", MyItem);
		}

		public void TestMailRecipients()
		{
			MailRecipientReadonlyCollection recipients = MyItem.MailRecipients;
			AssertEquals("NumberOfRecipients", 9, recipients.Count);
			Assert("Recipient1ARecipient", recipients.Contains(Recipient1.PK));
			Assert("Recipient2ARecipient", recipients.Contains(Recipient2.PK));
			Assert("Recipient3ARecipient", recipients.Contains(Recipient3.PK));
		}

		public void TestMailAttachments()
		{
			MailAttachmentCollection attachments = MyItem.MailAttachments;
			AssertEquals("NumberOfAttachments", 3, attachments.Count);
			Assert("Attachment1AnAttachment", attachments.Contains(Attachment1.PK));
			Assert("Attachment2AnAttachment", attachments.Contains(Attachment2.PK));
			Assert("Attachment3AnAttachment", attachments.Contains(Attachment3.PK));
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestCalculatedLocalTimes()
		{
			AssertEquals("[PRE-CONDITION] CalcReceivedDateTimeLocal valid initially?", false, MyItem.CalcReceivedDateTimeLocal.IsValid);
			AssertEquals("[PRE-CONDITION] CalcSendDateTimeLocal valid initially?", false, MyItem.CalcSendDateTimeLocal.IsValid);

			MyItem.MI_ReceivedDateTime = new ZDateTime(2010, 6, 1, 10, 36, 46);
			MyItem.MI_SendDateTime = new ZDateTime(2010, 6, 1, 11, 0, 0);

			AssertEquals("[Local=UTC+2] CalcReceivedDateTimeLocal", new ZDateTime(2010, 6, 1, 12, 36, 46), MyItem.CalcReceivedDateTimeLocal);
			AssertEquals("[Local=UTC+2] CalcSendDateTimeLocal", new ZDateTime(2010, 6, 1, 13, 0, 0), MyItem.CalcSendDateTimeLocal);

			TestUtcOffsetAttribute.Time = TimeSpan.FromHours(-4);
			AssertEquals("[Local=UTC-4] CalcReceivedDateTimeLocal", new ZDateTime(2010, 6, 1, 6, 36, 46), MyItem.CalcReceivedDateTimeLocal);
			AssertEquals("[Local=UTC-4] CalcSendDateTimeLocal", new ZDateTime(2010, 6, 1, 7, 0, 0), MyItem.CalcSendDateTimeLocal);

			TestUtcOffsetAttribute.Time = TimeSpan.FromHours(0);
			AssertEquals("[Local=UTC] CalcReceivedDateTimeLocal", new ZDateTime(2010, 6, 1, 10, 36, 46), MyItem.CalcReceivedDateTimeLocal);
			AssertEquals("[Local=UTC] CalcSendDateTimeLocal", new ZDateTime(2010, 6, 1, 11, 0, 0), MyItem.CalcSendDateTimeLocal);
		}

		public void TestHeaderReceivedTimeUtc()
		{
			MyItem.MI_Header = @"Received: from [127.0.0.1] by NM293  (ArGoSoft Mail Server Plus for WinNT/2000, Version 1.8 (1.8.3.1)); Fri, 14 Nov 2008 10:48:59 +1100
Message-ID: <236837462.1047421985219.JavaMail.nick@NM293>
From: sedi@example.com
To: sedi@example.com
Subject: 1111111_1_1234567
Mime-Version: 1.0
Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=smime.p7m
Content-Transfer-Encoding: base64
Date: Wed, 12 Mar 2003 09:33:05 +1100
";
			AssertEquals("UTC Time", new ZDateTime(2008, 11, 13, 23, 48, 59), MyItem.HeaderReceivedTimeUtc);
		}

		public void TestHeaderReceivedTimeUtcFallbackToDate()
		{
			MyItem.MI_Header = @"Received: from [127.0.0.1] by NM293  (ArGoSoft Mail Server Plus for WinNT/2000, Version 1.8 (1.8.3.1)); 
Message-ID: <236837462.1047421985219.JavaMail.nick@NM293>
From: sedi@example.com
To: sedi@example.com
Subject: 1111111_1_1234567
Mime-Version: 1.0
Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=smime.p7m
Content-Transfer-Encoding: base64
Date: Fri, 14 Nov 2008 10:48:58 +1100
";
			AssertEquals("UTC Time", new ZDateTime(2008, 11, 13, 23, 48, 58), MyItem.HeaderReceivedTimeUtc);
		}

		[TestTimeZone]
		public void TestHeaderReceivedTimeUtcFallbackToReceivedDate()
		{
			MyItem.MI_ReceivedDateTime = new ZDateTime(2008, 11, 14, 10, 48, 57);
			MyItem.MI_Header = @"Received: from [127.0.0.1] by NM293  (ArGoSoft Mail Server Plus for WinNT/2000, Version 1.8 (1.8.3.1)); 
Message-ID: <236837462.1047421985219.JavaMail.nick@NM293>
From: sedi@example.com
To: sedi@example.com
Subject: 1111111_1_1234567
Mime-Version: 1.0
Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=smime.p7m
Content-Transfer-Encoding: base64
";
			AssertEquals("UTC Time", new ZDateTime(2008, 11, 14, 10, 48, 57), MyItem.HeaderReceivedTimeUtc);
		}

		public void TestGetHeaderItem()
		{
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: \"Test, DJ\" <djtest@edi.com.au>,\r\n\t\"Test 2, \\\"DJ\\\"! \\\\r\\\\n\" <djtest2@edi.com.au>,\r\n bob@example.com, fred@example.com\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("From", "<sedi@example.com>", MyItem.GetHeaderItem("From"));
			AssertEquals("To", "\"Test, DJ\" <djtest@edi.com.au>,\t\"Test 2, \\\"DJ\\\"! \\\\r\\\\n\" <djtest2@edi.com.au>, bob@example.com, fred@example.com", MyItem.GetHeaderItem("To"));
			AssertEquals("Subject", "1111111_1_1234567", MyItem.GetHeaderItem("Subject"));
			AssertEquals("Date", "Wed, 12 Mar 2003 09:33:05 +1100", MyItem.GetHeaderItem("Date"));
			AssertEquals("Mime-Version", "1.0", MyItem.GetHeaderItem("Mime-Version"));
			AssertEquals("Content-Transfer-Encoding", "base64", MyItem.GetHeaderItem("Content-Transfer-Encoding"));
			AssertEquals("Content-Type", "application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"", MyItem.GetHeaderItem("Content-Type"));

			AssertEquals("BLAHBLA", ZString.Empty, MyItem.GetHeaderItem("BLABLA"));
		}

		public void TestGetAllHeaderKeys()
		{
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			string[] result = MyItem.GetAllHeaderKeys();
			AssertEquals(7, result.Length);
			AssertEquals("From", result[0]);
			AssertEquals("To", result[1]);
			AssertEquals("Subject", result[2]);
			AssertEquals("Date", result[3]);
			AssertEquals("Mime-Version", result[4]);
			AssertEquals("Content-Transfer-Encoding", result[5]);
			AssertEquals("Content-Type", result[6]);
		}

		public void TestSetHeaderItem()
		{
			MyItem.SetHeaderItem("From", "<sedi@example.com>");
			MyItem.SetHeaderItem("To", "<sedi@example.com>");
			MyItem.SetHeaderItem("Subject", "1111111_1_1234567");
			MyItem.SetHeaderItem("Date", "Wed, 12 Mar 2003 09:33:05 +1100");
			MyItem.SetHeaderItem("Mime-Version", "1.0");
			MyItem.SetHeaderItem("Content-Transfer-Encoding", "base64");
			MyItem.SetHeaderItem("Content-Type", "application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"");

			AssertEquals("MyItem.MI_Header", (ZString)(
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n"), MyItem.MI_Header);

			MyItem.SetHeaderItem("DATE", "new date");
			AssertEquals("MyItem.MI_Header", (ZString)(
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"DATE: new date\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n"), MyItem.MI_Header);

			MyItem.SetHeaderItem("DATE", null);
			AssertEquals("MyItem.MI_Header", (ZString)(
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n"), MyItem.MI_Header);
		}

		public void TestSetHeaderUnderstandsFolding()
		{
			MyItem.MI_Header = "Bollocks: garbage\r\n junk\r\nSubject: The quick\r\n brown fox\r\n jumps over\r\nStuff: foo\r\n";
			MyItem.SetHeaderItem("Subject", "The slow");
			AssertEquals("Bollocks: garbage\r\n junk\r\nSubject: The slow\r\nStuff: foo\r\n", MyItem.MI_Header);
			AssertEquals("The slow", MyItem.GetHeaderItem("Subject"));
		}

		public void TestIsMIMEEmail()
		{
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MIMEEmail", true, MyItem.IsMIMEEmail);
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n";
			AssertEquals("MIMEEmail", false, MyItem.IsMIMEEmail);
		}

		public void TestGetMimeMessageBody()
		{
			MyItem.MI_Header = ComplexHeaderTxt;
			MyItem.MI_Body = ComplexMIMETxt;
			var expectedMimeBody = "Some Text\r\n\r\n \r\n\r\nText\r\n\r\n \r\n\r\nText\r\n\r\n \r\n\r\n";
			AssertEquals("MIME Message Body", expectedMimeBody, MyItem.GetMimeMessageBody());
		}

		public void TestIsSingleAttachmentMIMEEmail()
		{
			MyItem = Factory.New<MailItem>();
			Attachment1 = Factory.New<MailAttachment>();
			Attachment2 = Factory.New<MailAttachment>();

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("IsSingleAttachmentMIMEEmail", false, MyItem.IsSingleAttachmentMIMEEmail);

			MyItem.MailAttachments.Add(Attachment1);
			AssertEquals("IsSingleAttachmentMIMEEmail", true, MyItem.IsSingleAttachmentMIMEEmail);

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n";
			AssertEquals("IsSingleAttachmentMIMEEmail", false, MyItem.IsSingleAttachmentMIMEEmail);

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			MyItem.MailAttachments.Add(Attachment2);
			AssertEquals("IsSingleAttachmentMIMEEmail", false, MyItem.IsSingleAttachmentMIMEEmail);
		}

		public void TestAllRecipients()
		{
			string expected = "email1@email.com;email2@email.com;email3@email.com;" +
								"ccemail1@email.com;ccemail2@email.com;ccemail3@email.com;" +
								"bccemail1@email.com;bccemail2@email.com;bccemail3@email.com";

			AssertEquals("NumberOfRecipients", 9, MyItem.MailRecipients.Count);
			AssertEquals("All Recipient", expected, MyItem.AllRecipients);
		}

		public void TestSetMI_Subject()
		{
			MyItem.MI_Subject = "My Subject";
			AssertEquals("SubjectInHeader", "My Subject", MyItem.GetHeaderItem("Subject"));
		}

		public void TestSetMI_BusinessEntityID()
		{
			MyItem.MI_BusinessEntityID = "Mail Campaign_PRINT_PREAP";
			AssertEquals("CampaignIDInHeader", "Mail Campaign_PRINT_PREAP", MyItem.GetHeaderItem("X-BusinessEntityID"));
		}

		public void TestSetMI_BusinessEntityTableCode()
		{
			MyItem.MI_BusinessEntityTableCode = ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix;
			AssertEquals("TableCode", ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix, MyItem.GetHeaderItem("X-BusinessEntityTableCode"));
		}

		public void TestSetMI_BusinessEntityjobNumber()
		{
			MyItem.MI_BusinessEntityjobNumber = "S00000001";
			AssertEquals("jobNumber", "S00000001", MyItem.GetHeaderItem("X-BusinessEntityJobNumber"));
		}

		public void TestSetMI_ListUnsubscribe()
		{
			MyItem.MI_ListUnsubscribe = "<http://www.cw1.com/u?123>";
			AssertEquals("<http://www.cw1.com/u?123>", MyItem.GetHeaderItem("List-Unsubscribe"));
		}

		public void TestMI_DocumentName()
		{
			string name = "Green Alien";
			MyItem.MI_DocumentName = name;
			AssertEquals(name, MyItem.MI_DocumentName);
			// header field body must be US-ASCII - encoding is used to ensure this
			AssertEquals(BounceEmailParser.EncodeDocumentName(name), MyItem.GetHeaderItem("X-DocumentName"));

			name = "é";
			MyItem.MI_DocumentName = name;
			AssertNotContains("unencoded name is not in the header", name, MyItem.MI_Header);
			AssertEquals(name, MyItem.MI_DocumentName);
			AssertEquals(BounceEmailParser.EncodeDocumentName(name), MyItem.GetHeaderItem("X-DocumentName"));

			MyItem.SetHeaderItem(BounceEmailParser.BounceEmailConstants.DocumentNameKey, "not base64 encoded");
			AssertEquals("unencoded value is returned directly", "not base64 encoded", MyItem.MI_DocumentName);
		}

		public void TestMI_SenderStaffID()
		{
			var staffPK = "2FEA7580-9F33-4BEB-80F2-155FB1FCCC6B";
			MyItem.MI_SenderStaffID = staffPK;
			AssertEquals("2FEA7580-9F33-4BEB-80F2-155FB1FCCC6B", MyItem.MI_SenderStaffID);
			AssertEquals("2FEA7580-9F33-4BEB-80F2-155FB1FCCC6B", MyItem.GetHeaderItem("X-SenderStaffID"));

			MyItem.SetHeaderItem(BounceEmailParser.BounceEmailConstants.SenderStaffIDKey, staffPK);
			AssertEquals("2FEA7580-9F33-4BEB-80F2-155FB1FCCC6B", MyItem.MI_SenderStaffID);
		}

		public void TestSetMI_From()
		{
			MyItem.MI_From = "nick@edi.com.au";
			AssertEquals("FromInHeader", "nick@edi.com.au", MyItem.GetHeaderItem("From"));
			MyItem.MI_From = "<nick@edi.com.au>";
			AssertEquals("FromInHeader", "nick@edi.com.au", MyItem.GetHeaderItem("From"));
			MyItem.MI_From = "\"Nick\" <nick@edi.com.au>";
			AssertEquals("FromInHeader", "\"Nick\" <nick@edi.com.au>", MyItem.GetHeaderItem("From"));

			string unescapedAddress = "Foo\"Bar <x@y>";
			string escapedAddress = "\"Foo\\\"Bar\" <x@y>";
			MyItem.MI_From = unescapedAddress;
			AssertEquals("FromInHeader", escapedAddress, MyItem.GetHeaderItem("From"));
			MyItem.MI_From = escapedAddress;
			AssertEquals("FromInHeader after re-assigning", escapedAddress, MyItem.GetHeaderItem("From"));

			string unescapedComplex = "Test extra.\r \"Symbols\\\"(),)( <alex@edi.com.au>";
			string escapedComplex = "\"Test extra\\.\\\r \\\"Symbols\\\"\\(\\)\\,\\)\\(\" <alex@edi.com.au>";
			MyItem.MI_From = unescapedComplex;
			AssertEquals("FromInHeader", escapedComplex, MyItem.GetHeaderItem("From"));
			MyItem.MI_From = escapedComplex;
			AssertEquals("FromInHeader after re-assigning", escapedComplex, MyItem.GetHeaderItem("From"));
		}

		public void TestSetMI_FromWithSuperLongSenderInformation()
		{
			var myItem = Factory.New<MailItem>();
			var fromName = new string('A', 350);
			var fromAddr = "<sedi@example.com>";

			myItem.MI_Header =
				$"From: {fromName}{fromAddr}\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"X-BusinessEntityID: Mail Campaign_MEDIA_TV\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";

			var expectedFromName = fromName.Substring(0, MailItem.Schema.MI_FromMaxLength - fromAddr.Length);
			AssertEquals("MI_From", $"{expectedFromName}{fromAddr}", myItem.MI_From);
			AssertEquals("sedi@example.com", myItem.GetFromEmailAddress());
		}

		public void TestSetMI_FromDecoded()
		{
			MyItem = Factory.New<MailItem>();
			MyItem.MI_Header =
				"From: =?windows-874?B?v8uhtL/LobS/y6G0v8uhtL/LobS/y6G0v8uhtL/LobS/y6G0v8uhtL/L?=<test@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"X-BusinessEntityID: Mail Campaign_MEDIA_TV\r\n" +
				"Content-Transfer-Encoding: quoted-printable\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_From", "ฟหกดฟหกดฟหกดฟหกดฟหกดฟหกดฟหกดฟหกดฟหกดฟหกดฟห<test@example.com>", MyItem.MI_From);
		}

		public void TestSetMI_ReplyTo()
		{
			MyItem.MI_ReplyTo = "nick@edi.com.au";
			AssertEquals("ReplyToInHeader", "nick@edi.com.au", MyItem.GetHeaderItem("ReplyTo"));
			MyItem.MI_ReplyTo = "<nick@edi.com.au>";
			AssertEquals("ReplyToInHeader", "nick@edi.com.au", MyItem.GetHeaderItem("ReplyTo"));
			MyItem.MI_ReplyTo = "\"Nick\" <nick@edi.com.au>";
			AssertEquals("ReplyToInHeader", "\"Nick\" <nick@edi.com.au>", MyItem.GetHeaderItem("ReplyTo"));

			string unescapedAddress = "Foo\"Bar <x@y>";
			string escapedAddress = "\"Foo\\\"Bar\" <x@y>";
			MyItem.MI_ReplyTo = unescapedAddress;
			AssertEquals("ReplyToInHeader", escapedAddress, MyItem.GetHeaderItem("ReplyTo"));
			MyItem.MI_ReplyTo = escapedAddress;
			AssertEquals("ReplyToInHeader after re-assigning", escapedAddress, MyItem.GetHeaderItem("ReplyTo"));

			string unescapedComplex = "Test extra.\r \"Symbols\\\"(),)( <alex@edi.com.au>";
			string escapedComplex = "\"Test extra\\.\\\r \\\"Symbols\\\"\\(\\)\\,\\)\\(\" <alex@edi.com.au>";
			MyItem.MI_ReplyTo = unescapedComplex;
			AssertEquals("ReplyToInHeader", escapedComplex, MyItem.GetHeaderItem("ReplyTo"));
			MyItem.MI_ReplyTo = escapedComplex;
			AssertEquals("ReplyToInHeader after re-assigning", escapedComplex, MyItem.GetHeaderItem("ReplyTo"));
		}

		public void TestSetMI_ReplyToDecoded()
		{
			MyItem = Factory.New<MailItem>();
			MyItem.MI_Header =
				"From: sender<test@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"ReplyTo: =?shift-jis?B?k/qWe5LKiV6DV4ODg3CDkyCBpzEyMy00NTY3?=<test@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"X-BusinessEntityID: Mail Campaign_MEDIA_TV\r\n" +
				"Content-Transfer-Encoding: quoted-printable\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_ReplyTo", "日本通運ジャパン 〒123-4567<test@example.com>", MyItem.MI_ReplyTo);
		}

		public void TestSetMI_Header()
		{
			MyItem = Factory.New<MailItem>();
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"X-BusinessEntityID: Mail Campaign_MEDIA_TV\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_From", "<sedi@example.com>", MyItem.MI_From);
			AssertEquals("MI_Subject", "1111111_1_1234567", MyItem.MI_Subject);
			AssertEquals("MI_CampaignItemID", "Mail Campaign_MEDIA_TV", MyItem.MI_BusinessEntityID);
			AssertEquals("MI_SentDateTime", new ZDateTime(Convert.ToDateTime("Wed, 12 Mar 2003 09:33:05 +1100").ToUniversalTime()), MyItem.MI_SendDateTime);

			AssertEquals("ToNumber", 1, MyItem.MailRecipients.Count);
			AssertEquals("To", "<sedi@example.com>", MyItem.MailRecipients[0].EmailAddress);

			MyItem.MI_Header =
				"From: <234234234om>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 111234234111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_From", "<sedi@example.com>", MyItem.MI_From);
			AssertEquals("MI_Subject", "1111111_1_1234567", MyItem.MI_Subject);

			AssertEquals("ToNumber", 1, MyItem.MailRecipients.Count);
			AssertEquals("To", "<sedi@example.com>", MyItem.MailRecipients[0].EmailAddress);

			MyItem.MI_Subject = string.Empty;
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: =?shift-jis?B?k/qWe5LKiV6DV4ODg3CDkyCBpzEyMy00NTY3?=\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_Subject", "日本通運ジャパン 〒123-4567", MyItem.MI_Subject);

			MyItem.MI_Subject = string.Empty;
			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: =?iso-8859-1?Q?=A1Hola,_se=F1or!?=\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("MI_Subject", "¡Hola, señor!", MyItem.MI_Subject);
		}

		public void TestRawEmailString()
		{
			MyItem = Factory.New<MailItem>();
			MailAttachment attachment = Factory.New<MailAttachment>();
			attachment.MA_Encoding = "B64";
			attachment.MA_Data = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			MyItem.MI_Body = "asd";

			string expected = "From: <sedi@example.com>\r\n" +
								"To: <sedi@example.com>\r\n" +
								"Subject: 1111111_1_1234567\r\n" +
								"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
								"Mime-Version: 1.0\r\n" +
								"Content-Transfer-Encoding: base64\r\n" +
								"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n\r\n" +
								"asd";
			AssertEquals("RawMIMEText", expected, MyItem.RawEmailString);

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"";
			MyItem.MI_Body = "asd";

			AssertEquals("RawMIMEText2", expected, MyItem.RawEmailString);
		}

		public void TestRawMIMEString()
		{
			MyItem = Factory.New<MailItem>();
			MailAttachment attachment = Factory.New<MailAttachment>();
			attachment.MA_Encoding = "B64";
			attachment.MA_Data = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 2003 09:33:05 +1100\r\n";
			MyItem.MI_Body = "asd";
			string expected = null;
			AssertEquals("RawMIMEText", expected, MyItem.RawMIMEString);
		}

		[TestDate(2005, 01, 02, 03, 04, 05)]
		public void TestSetRawMIMEString()
		{
			MyItem = Factory.New<MailItem>();
			MyItem.RawMIMEString = @"Received: from [127.0.0.1] by NM293  (ArGoSoft Mail Server Plus for WinNT/2000, Version 1.8 (1.8.3.1)); Wed, 12 Mar 2003 09:33:05 +1100
Message-ID: <236837462.1047421985219.JavaMail.nick@NM293>
From: sedi@example.com
To: sedi@example.com
Subject: 1111111_1_1234567
Mime-Version: 1.0
Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=smime.p7m
Content-Transfer-Encoding: base64
Date: Wed, 12 Mar 2003 09:33:05 +1100

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";

			AssertEquals("NumberOfRecipients", 1, MyItem.MailRecipients.Count);
			AssertEquals("Recipient", "<sedi@example.com>", MyItem.MailRecipients[0].EmailAddress);
			AssertEquals("NumberOfAttachments", 0, MyItem.MailAttachments.Count);
			AssertEquals("SentDateTime", new ZDateTime(Convert.ToDateTime("Wed, 12 Mar 2003 09:33:05 +1100").ToUniversalTime()), MyItem.MI_SendDateTime);
			AssertEquals("ReceivedDateTime", new ZDateTime(2005, 01, 02, 03, 04, 05), MyItem.MI_ReceivedDateTime);
		}

		[ExpectException(typeof(Exception))]
		public void TestSetRawMIMEStringWithException()
		{
			MyItem = Factory.New<MailItem>();
			MyItem.MI_Header = "Header";
			try
			{
				MyItem.RawMIMEString = @"Received: from [127.0.0.1] by NM293  (ArGoSoft Mail Server Plus for WinNT/2000, Version 1.8 (1.8.3.1)); Wed, 12 Mar 2003 09:33:05 +1100
Message-ID: <236837462.1047421985219.JavaMail.nick@NM293>
From: sedi@example.com
To: sedi@example.com
Subject: 1111111_1_1234567
Mime-Version: 1.0
Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name=smime.p7m
Content-Transfer-Encoding: base64
Date: Wed, 12 Mar 2003 09:33:05 +1100
";
			}
			finally
			{
				AssertEquals("No Recipients", 0, MyItem.MailRecipients.Count);
			}
		}

		public void TestDelete()
		{
			foreach (IMailRecipient recipient in MyItem.MailRecipients)
			{
				AssertEquals("Recipient.IsDeleted", false, recipient.IsDeleted);
			}
			foreach (MailAttachment attachment in MyItem.MailAttachments)
			{
				AssertEquals("Attachment.IsDeleted", false, attachment.IsDeleted);
			}
			AssertEquals("MyItem.IsDeleted", false, MyItem.IsDeleted);

			MyItem.Delete();

			foreach (IMailRecipient recipient in MyItem.MailRecipients)
			{
				AssertEquals("Recipient.IsDeleted", true, recipient.IsDeleted);
			}
			foreach (MailAttachment attachment in MyItem.MailAttachments)
			{
				AssertEquals("Attachment.IsDeleted", true, attachment.IsDeleted);
			}
			AssertEquals("MyItem.IsDeleted", true, MyItem.IsDeleted);
		}

		public void TestCopyValuesFrom()
		{
			MyItem.MI_Subject = "My Subject";
			MyItem.MI_From = "nick@edi.com.au";
			MyItem.MI_ReplyTo = "nick@edi.com.au";
			MyItem.MI_Direction = MailDirection.Transmit;
			MyItem.MI_Status = MailStatus.Sent;
			MyItem.MI_Body = "Message Body";
			MyItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			MyItem.MI_SendDateTime = ZDateTime.UtcNow;
			MyItem.MI_SystemCreateTimeUtc = ZDateTime.UtcNow;

			MailItem newItem = Factory.New<MailItem>();
			newItem.CopyValuesFrom(MyItem);

			AssertEquals("Subject", "My Subject", newItem.MI_Subject);
			AssertEquals("From", "nick@edi.com.au", newItem.MI_From);
			AssertEquals("Reply To", "nick@edi.com.au", newItem.MI_ReplyTo);
			AssertEquals("Direction", MailDirection.Transmit, newItem.MI_Direction);
			AssertEquals("Status", MailStatus.Sent, newItem.MI_Status);
			AssertEquals("Body", "Message Body", newItem.MI_Body);
			AssertEquals("Received", MyItem.MI_ReceivedDateTime, newItem.MI_ReceivedDateTime);
			AssertEquals("Send", MyItem.MI_SendDateTime, newItem.MI_SendDateTime);
			AssertEquals("create", MyItem.MI_SystemCreateTimeUtc, MyItem.MI_SystemCreateTimeUtc);

			AssertEquals("Number of recipients", MyItem.MailRecipients.Count, newItem.MailRecipients.Count);

			for (int i = 0; i < newItem.MailRecipients.Count; i++)
			{
				IMailRecipient old = MyItem.MailRecipients[i];
				IMailRecipient @new = newItem.MailRecipients[i];
				AssertEquals("MR_MI assigned to new", newItem.PK, @new.MR_MI);
				AssertEquals("Recipent Address", old.EmailAddress, @new.EmailAddress);
				AssertEquals("Recipent Type", old.MR_RecipientType, @new.MR_RecipientType);
			}

			AssertEquals("Number of attachments", MyItem.MailAttachments.Count, newItem.MailAttachments.Count);

			for (int i = 0; i < newItem.MailAttachments.Count; i++)
			{
				MailAttachment old = MyItem.MailAttachments[i];
				MailAttachment @new = newItem.MailAttachments[i];
				AssertEquals("MA_MI assigned to new", newItem.PK, @new.MA_MI);
				AssertEquals("File name", old.MA_FileName, @new.MA_FileName);
				AssertEquals("Encoding", old.MA_Encoding, @new.MA_Encoding);
			}
		}

		public void TestSetSendAttempts()
		{
			MyItem.SendAttempts = 10;
			AssertEquals("AcksAtt1", (byte)10, Recipient1.MR_AckAttempt);
			AssertEquals("AcksAtt2", (byte)10, Recipient1.MR_AckAttempt);
			AssertEquals("AcksAtt3", (byte)10, Recipient1.MR_AckAttempt);
			MyItem.SendAttempts = 0;
			AssertEquals("AcksAtt1", (byte)0, Recipient1.MR_AckAttempt);
			AssertEquals("AcksAtt2", (byte)0, Recipient1.MR_AckAttempt);
			AssertEquals("AcksAtt3", (byte)0, Recipient1.MR_AckAttempt);
		}

		public void TestExtractAttachments()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MyItem = factory.New<MailItem>();
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			MyItem.MI_Header = ComplexHeaderTxt;
			MyItem.MI_Body = ComplexMIMETxt;
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			MyItem.ExtractAttachments();
			AssertEquals("AttachmentCount", 2, MyItem.MailAttachments.Count);

			string filename1 = MyItem.MailAttachments[0].MA_FileName;
			string filename2 = MyItem.MailAttachments[1].MA_FileName;

			byte[] bytes1 = MyItem.MailAttachments[0].MA_Data;
			byte[] bytes2 = MyItem.MailAttachments[1].MA_Data;

			AssertEquals("Attachment1Name", "Text File.txt", filename1);
			AssertEquals("Attachment2Name", "Blue Lace 16.bmp", filename2);

			AssertEquals("Attachment1Text", "I am some text", Encoding.ASCII.GetString(bytes1));
			AssertEquals("Attachment2Name", PicBmp, bytes2);
		}

		public void TestExtractAttachmentsForAttachmentlessEmail()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MyItem = factory.New<MailItem>();
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			MyItem.MI_Header = ComplexHeaderTxt;
			MyItem.MI_Body = "foo";
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			string oldBody = MyItem.MI_Body;
			MyItem.ExtractAttachments();
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);

			string newBody = MyItem.MI_Body;
			AssertEquals("MI_Body", oldBody, newBody);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractAttachmentsForSMIMEEmail()
		{
			var factory = new BusinessObjectFactory();
			MyItem = factory.New<MailItem>();
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			MyItem.MI_Header = ComplexHeaderTxt;
			MyItem.MI_Body = ReadFile(BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\BatchProcessor\TestFiles\NewEmail.txt");
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);
			string oldBody = MyItem.MI_Body;
			MyItem.ExtractAttachments();
			AssertEquals("AttachmentCount", 0, MyItem.MailAttachments.Count);

			string newBody = MyItem.MI_Body;
			AssertEquals("MI_Body", oldBody, newBody);
		}

		public void TestDontStoreAngledBracketsInFromAddress()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MyItem = factory.New<MailItem>();
			MyItem.MI_From = "test@test.com";
			AssertEquals("EmailAddress", "test@test.com", MyItem.MI_From);
			MyItem.MI_From = "<cargo@ccf.customs.gov.au>";
			AssertEquals("EmailAddress", "cargo@ccf.customs.gov.au", MyItem.MI_From);
		}

		public void TestTemp()
		{
			string base64String = @"77u/PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48Q29uc29scyB4bWxuczp4
c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4c2k6bm9OYW1l
c3BhY2VTY2hlbWFMb2NhdGlvbj0iQzpcRG9jdW1lbnRzIGFuZCBTZXR0aW5nc1xqeHZcRGVza3Rv
cFxFYWdsZSBTY2hlbWFzXENvbnNvbC54c2QiPjxDb25zb2w+PENvbnNvbEhlYWRlcj48Q29uc29s
SWRlbnRpZmllciBDb25zb2xJZGVudGlmaWVyVHlwZT0iTWFzdGVyV2F5YmlsbCI+MDg2Nzg4ODI1
NjU8L0NvbnNvbElkZW50aWZpZXI+PFNoaXBtZW50cz48U2hpcG1lbnQ+PFNoaXBtZW50SWRlbnRp
ZmllciBTaGlwbWVudElkZW50aWZpZXJUeXBlPSJIb3VzZWJpbGwiPjM5MTMzMTM4PC9TaGlwbWVu
dElkZW50aWZpZXI+PFNoaXBtZW50RGV0YWlscz48VHJhbnNwb3J0TW9kZT5BSVI8L1RyYW5zcG9y
dE1vZGU+PFBvcnRPZk9yaWdpbj48UG9ydD5aWlpaWjwvUG9ydD48RXN0aW1hdGVkRGF0ZVRpbWU+
MjAwNC0xMS0yOFQwMDowMDowMDwvRXN0aW1hdGVkRGF0ZVRpbWU+PC9Qb3J0T2ZPcmlnaW4+PFBv
cnRvZkRlc3RpbmF0aW9uPjxQb3J0Pk5aQUtMPC9Qb3J0PjxFc3RpbWF0ZWREYXRlVGltZT4yMDA0
LTEyLTA2VDAwOjAwOjAwPC9Fc3RpbWF0ZWREYXRlVGltZT48L1BvcnRvZkRlc3RpbmF0aW9uPjxD
b25zaWduZWUgRURJQ29kZT0iTklLRU5FV0FLTCIgLz48VG90YWxPdXRlclBhY2tzUXR5IERpbWVu
c2lvblR5cGU9IkNUTiI+MDwvVG90YWxPdXRlclBhY2tzUXR5PjxXZWlnaHQgRGltZW5zaW9uVHlw
ZT0iS0ciPjA8L1dlaWdodD48Vm9sdW1lIERpbWVuc2lvblR5cGU9Ik0zIj4wPC9Wb2x1bWU+PEdv
b2RzRGVzY3JpcHRpb24+PC9Hb29kc0Rlc2NyaXB0aW9uPjwvU2hpcG1lbnREZXRhaWxzPjxJbnZv
aWNlcz48SW52b2ljZUhlYWRlcj48SW52b2ljZU51bWJlcj4zMzQxPC9JbnZvaWNlTnVtYmVyPjxJ
bnZvaWNlQW1vdW50IEN1cnJlbmN5Q29kZT0iVVNEIj4wPC9JbnZvaWNlQW1vdW50PjxJbnZvaWNl
RGF0ZT4yMDA0LTExLTIzPC9JbnZvaWNlRGF0ZT48Q29uc2lnbm9yIEVESUNvZGU9Ik1EIiAvPjxJ
bmNvdGVybT5GT0I8L0luY290ZXJtPjxJbnZvaWNlTGluZXM+PEludm9pY2VMaW5lPjxJbnZvaWNl
UXR5IERpbWVuc2lvblR5cGU9IlBDRSI+MTI3PC9JbnZvaWNlUXR5PjxMaW5lUHJpY2UgQ3VycmVu
Y3lDb2RlPSJVU0QiPjA8L0xpbmVQcmljZT48UHJvZHVjdE51bWJlcj4zMDgyMjk8L1Byb2R1Y3RO
dW1iZXI+PEN1c3RvbVRleHQxPjEwMjwvQ3VzdG9tVGV4dDE+PEN1c3RvbVRleHQyPkVHTDwvQ3Vz
dG9tVGV4dDI+PC9JbnZvaWNlTGluZT48L0ludm9pY2VMaW5lcz48L0ludm9pY2VIZWFkZXI+PC9J
bnZvaWNlcz48L1NoaXBtZW50PjwvU2hpcG1lbnRzPjwvQ29uc29sSGVhZGVyPjwvQ29uc29sPjxD
b25zb2w+PENvbnNvbEhlYWRlcj48Q29uc29sSWRlbnRpZmllciBDb25zb2xJZGVudGlmaWVyVHlw
ZT0iTWFzdGVyV2F5YmlsbCI+MTI1NzA4MzU5OTU8L0NvbnNvbElkZW50aWZpZXI+PFNoaXBtZW50
cz48U2hpcG1lbnQ+PFNoaXBtZW50SWRlbnRpZmllciBTaGlwbWVudElkZW50aWZpZXJUeXBlPSJI
b3VzZWJpbGwiPjg1NDc4MDA8L1NoaXBtZW50SWRlbnRpZmllcj48U2hpcG1lbnREZXRhaWxzPjxU
cmFuc3BvcnRNb2RlPkFJUjwvVHJhbnNwb3J0TW9kZT48UG9ydE9mT3JpZ2luPjxQb3J0PlpaWlpa
PC9Qb3J0PjxFc3RpbWF0ZWREYXRlVGltZT4yMDA0LTExLTA5VDAwOjAwOjAwPC9Fc3RpbWF0ZWRE
YXRlVGltZT48L1BvcnRPZk9yaWdpbj48UG9ydG9mRGVzdGluYXRpb24+PFBvcnQ+WlpaWlo8L1Bv
cnQ+PEVzdGltYXRlZERhdGVUaW1lPjIwMDQtMTEtMTVUMDA6MDA6MDA8L0VzdGltYXRlZERhdGVU
aW1lPjwvUG9ydG9mRGVzdGluYXRpb24+PENvbnNpZ25lZSBFRElDb2RlPSIiIC8+PFRvdGFsT3V0
ZXJQYWNrc1F0eSBEaW1lbnNpb25UeXBlPSJDVE4iPjA8L1RvdGFsT3V0ZXJQYWNrc1F0eT48V2Vp
Z2h0IERpbWVuc2lvblR5cGU9IktHIj4wPC9XZWlnaHQ+PFZvbHVtZSBEaW1lbnNpb25UeXBlPSJN
MyI+MDwvVm9sdW1lPjxHb29kc0Rlc2NyaXB0aW9uPjwvR29vZHNEZXNjcmlwdGlvbj48L1NoaXBt
ZW50RGV0YWlscz48SW52b2ljZXM+PEludm9pY2VIZWFkZXI+PEludm9pY2VOdW1iZXI+MDwvSW52
b2ljZU51bWJlcj48SW52b2ljZUFtb3VudCBDdXJyZW5jeUNvZGU9IlVTRCI+MDwvSW52b2ljZUFt
b3VudD48SW52b2ljZURhdGU+MTkwMC0wMS0wMTwvSW52b2ljZURhdGU+PENvbnNpZ25vciBFRElD
b2RlPSJQQSIgLz48SW5jb3Rlcm0+Rk9CPC9JbmNvdGVybT48SW52b2ljZUxpbmVzPjxJbnZvaWNl
TGluZT48SW52b2ljZVF0eSBEaW1lbnNpb25UeXBlPSJQQ0UiPjY8L0ludm9pY2VRdHk+PExpbmVQ
cmljZSBDdXJyZW5jeUNvZGU9IlVTRCI+MDwvTGluZVByaWNlPjxQcm9kdWN0TnVtYmVyPjMxMTIx
MDwvUHJvZHVjdE51bWJlcj48Q3VzdG9tVGV4dDE+NDQxPC9DdXN0b21UZXh0MT48Q3VzdG9tVGV4
dDI+RERBTzwvQ3VzdG9tVGV4dDI+PC9JbnZvaWNlTGluZT48L0ludm9pY2VMaW5lcz48L0ludm9p
Y2VIZWFkZXI+PEludm9pY2VIZWFkZXI+PEludm9pY2VOdW1iZXI+UEE1U1NGQTA0MTc0PC9JbnZv
aWNlTnVtYmVyPjxJbnZvaWNlQW1vdW50IEN1cnJlbmN5Q29kZT0iVVNEIj4wPC9JbnZvaWNlQW1v
dW50PjxJbnZvaWNlRGF0ZT4yMDA0LTEwLTE4PC9JbnZvaWNlRGF0ZT48Q29uc2lnbm9yIEVESUNv
ZGU9IlBBIiAvPjxJbmNvdGVybT5GT0I8L0luY290ZXJtPjxJbnZvaWNlTGluZXM+PEludm9pY2VM
aW5lPjxJbnZvaWNlUXR5IERpbWVuc2lvblR5cGU9IlBDRSI+NjwvSW52b2ljZVF0eT48TGluZVBy
aWNlIEN1cnJlbmN5Q29kZT0iVVNEIj4wPC9MaW5lUHJpY2U+PFByb2R1Y3ROdW1iZXI+MzExMjEw
PC9Qcm9kdWN0TnVtYmVyPjxDdXN0b21UZXh0MT40NDE8L0N1c3RvbVRleHQxPjxDdXN0b21UZXh0
Mj5EREFPPC9DdXN0b21UZXh0Mj48L0ludm9pY2VMaW5lPjwvSW52b2ljZUxpbmVzPjwvSW52b2lj
ZUhlYWRlcj48L0ludm9pY2VzPjwvU2hpcG1lbnQ+PC9TaGlwbWVudHM+PC9Db25zb2xIZWFkZXI+
PC9Db25zb2w+PENvbnNvbD48Q29uc29sSGVhZGVyPjxDb25zb2xJZGVudGlmaWVyIENvbnNvbElk
ZW50aWZpZXJUeXBlPSJNYXN0ZXJXYXliaWxsIj5BUExVMDc0MDg4NTUwPC9Db25zb2xJZGVudGlm
aWVyPjxTaGlwbWVudHM+PFNoaXBtZW50PjxTaGlwbWVudElkZW50aWZpZXIgU2hpcG1lbnRJZGVu
dGlmaWVyVHlwZT0iSG91c2ViaWxsIj48L1NoaXBtZW50SWRlbnRpZmllcj48U2hpcG1lbnREZXRh
aWxzPjxUcmFuc3BvcnRNb2RlPlNFQTwvVHJhbnNwb3J0TW9kZT48UG9ydE9mT3JpZ2luPjxQb3J0
PlpaWlpaPC9Qb3J0PjxFc3RpbWF0ZWREYXRlVGltZT4yMDA0LTExLTE0VDAwOjAwOjAwPC9Fc3Rp
bWF0ZWREYXRlVGltZT48L1BvcnRPZk9yaWdpbj48UG9ydG9mRGVzdGluYXRpb24+PFBvcnQ+Wlpa
Wlo8L1BvcnQ+PEVzdGltYXRlZERhdGVUaW1lPjIwMDQtMTItMTBUMDA6MDA6MDA8L0VzdGltYXRl
ZERhdGVUaW1lPjwvUG9ydG9mRGVzdGluYXRpb24+PENvbnNpZ25lZSBFRElDb2RlPSIiIC8+PFRv
dGFsT3V0ZXJQYWNrc1F0eSBEaW1lbnNpb25UeXBlPSJDVE4iPjA8L1RvdGFsT3V0ZXJQYWNrc1F0
eT48V2VpZ2h0IERpbWVuc2lvblR5cGU9IktHIj4wPC9XZWlnaHQ+PFZvbHVtZSBEaW1lbnNpb25U
eXBlPSJNMyI+MDwvVm9sdW1lPjxHb29kc0Rlc2NyaXB0aW9uPjwvR29vZHNEZXNjcmlwdGlvbj48
L1NoaXBtZW50RGV0YWlscz48SW52b2ljZXM+PEludm9pY2VIZWFkZXI+PEludm9pY2VOdW1iZXI+
PC9JbnZvaWNlTnVtYmVyPjxJbnZvaWNlQW1vdW50IEN1cnJlbmN5Q29kZT0iVVNEIj4wPC9JbnZv
aWNlQW1vdW50PjxJbnZvaWNlRGF0ZT4xOTAwLTAxLTAxPC9JbnZvaWNlRGF0ZT48Q29uc2lnbm9y
IEVESUNvZGU9IlZQIiAvPjxJbmNvdGVybT5GT0I8L0luY290ZXJtPjxJbnZvaWNlTGluZXM+PElu
dm9pY2VMaW5lPjxJbnZvaWNlUXR5IERpbWVuc2lvblR5cGU9IlBDRSI+NDUwNjwvSW52b2ljZVF0
eT48TGluZVByaWNlIEN1cnJlbmN5Q29kZT0iVVNEIj4wPC9MaW5lUHJpY2U+PFByb2R1Y3ROdW1i
ZXI+MzA4MjMxPC9Qcm9kdWN0TnVtYmVyPjxDdXN0b21UZXh0MT4wMDI8L0N1c3RvbVRleHQxPjxD
dXN0b21UZXh0Mj5BUExVPC9DdXN0b21UZXh0Mj48L0ludm9pY2VMaW5lPjxJbnZvaWNlTGluZT48
SW52b2ljZVF0eSBEaW1lbnNpb25UeXBlPSJQQ0UiPjU0OTY8L0ludm9pY2VRdHk+PExpbmVQcmlj
ZSBDdXJyZW5jeUNvZGU9IlVTRCI+MDwvTGluZVByaWNlPjxQcm9kdWN0TnVtYmVyPjMwODIzMTwv
UHJvZHVjdE51bWJlcj48Q3VzdG9tVGV4dDE+MDAyPC9DdXN0b21UZXh0MT48Q3VzdG9tVGV4dDI+
QVBMVTwvQ3VzdG9tVGV4dDI+PC9JbnZvaWNlTGluZT48SW52b2ljZUxpbmU+PEludm9pY2VRdHkg
RGltZW5zaW9uVHlwZT0iUENFIj4xMDAyPC9JbnZvaWNlUXR5PjxMaW5lUHJpY2UgQ3VycmVuY3lD
b2RlPSJVU0QiPjA8L0xpbmVQcmljZT48UHJvZHVjdE51bWJlcj4zMTA2MzU8L1Byb2R1Y3ROdW1i
ZXI+PEN1c3RvbVRleHQxPjE2MTwvQ3VzdG9tVGV4dDE+PEN1c3RvbVRleHQyPkFQTFU8L0N1c3Rv
bVRleHQyPjwvSW52b2ljZUxpbmU+PC9JbnZvaWNlTGluZXM+PC9JbnZvaWNlSGVhZGVyPjxJbnZv
aWNlSGVhZGVyPjxJbnZvaWNlTnVtYmVyPlBDVkkwNDQ2MzU8L0ludm9pY2VOdW1iZXI+PEludm9p
Y2VBbW91bnQgQ3VycmVuY3lDb2RlPSJVU0QiPjA8L0ludm9pY2VBbW91bnQ+PEludm9pY2VEYXRl
PjIwMDQtMTEtMDU8L0ludm9pY2VEYXRlPjxDb25zaWdub3IgRURJQ29kZT0iVlAiIC8+PEluY290
ZXJtPkZPQjwvSW5jb3Rlcm0+PEludm9pY2VMaW5lcz48SW52b2ljZUxpbmU+PEludm9pY2VRdHkg
RGltZW5zaW9uVHlwZT0iUENFIj4xMDAyPC9JbnZvaWNlUXR5PjxMaW5lUHJpY2UgQ3VycmVuY3lD
b2RlPSJVU0QiPjA8L0xpbmVQcmljZT48UHJvZHVjdE51bWJlcj4zMTA2MzU8L1Byb2R1Y3ROdW1i
ZXI+PEN1c3RvbVRleHQxPjE2MTwvQ3VzdG9tVGV4dDE+PEN1c3RvbVRleHQyPkFQTFU8L0N1c3Rv
bVRleHQyPjwvSW52b2ljZUxpbmU+PC9JbnZvaWNlTGluZXM+PC9JbnZvaWNlSGVhZGVyPjxJbnZv
aWNlSGVhZGVyPjxJbnZvaWNlTnVtYmVyPlBDVkkwNDQ2NjM8L0ludm9pY2VOdW1iZXI+PEludm9p
Y2VBbW91bnQgQ3VycmVuY3lDb2RlPSJVU0QiPjA8L0ludm9pY2VBbW91bnQ+PEludm9pY2VEYXRl
PjIwMDQtMTEtMDY8L0ludm9pY2VEYXRlPjxDb25zaWdub3IgRURJQ29kZT0iVlAiIC8+PEluY290
ZXJtPkZPQjwvSW5jb3Rlcm0+PEludm9pY2VMaW5lcz48SW52b2ljZUxpbmU+PEludm9pY2VRdHkg
RGltZW5zaW9uVHlwZT0iUENFIj40NTA2PC9JbnZvaWNlUXR5PjxMaW5lUHJpY2UgQ3VycmVuY3lD
b2RlPSJVU0QiPjA8L0xpbmVQcmljZT48UHJvZHVjdE51bWJlcj4zMDgyMzE8L1Byb2R1Y3ROdW1i
ZXI+PEN1c3RvbVRleHQxPjAwMjwvQ3VzdG9tVGV4dDE+PEN1c3RvbVRleHQyPkFQTFU8L0N1c3Rv
bVRleHQyPjwvSW52b2ljZUxpbmU+PEludm9pY2VMaW5lPjxJbnZvaWNlUXR5IERpbWVuc2lvblR5
cGU9IlBDRSI+NTQ5NjwvSW52b2ljZVF0eT48TGluZVByaWNlIEN1cnJlbmN5Q29kZT0iVVNEIj4w
PC9MaW5lUHJpY2U+PFByb2R1Y3ROdW1iZXI+MzA4MjMxPC9Qcm9kdWN0TnVtYmVyPjxDdXN0b21U
ZXh0MT4wMDI8L0N1c3RvbVRleHQxPjxDdXN0b21UZXh0Mj5BUExVPC9DdXN0b21UZXh0Mj48L0lu
dm9pY2VMaW5lPjwvSW52b2ljZUxpbmVzPjwvSW52b2ljZUhlYWRlcj48L0ludm9pY2VzPjwvU2hp
cG1lbnQ+PC9TaGlwbWVudHM+PC9Db25zb2xIZWFkZXI+PC9Db25zb2w+PC9Db25zb2xzPg==";
			byte[] result = System.Convert.FromBase64String(base64String);
			AssertNotNull(result);
			//-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader -MAI -Branch:SYD
		}

		public void TestSizeOfBodyAndAttachments()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MyItem = factory.New<MailItem>();

			MyItem.MI_Header = ComplexHeaderTxt;
			MyItem.MI_Body = ComplexMIMETxt;
			AssertEquals("BodySize", MyItem.MI_Body.Length, MyItem.SizeOfBodyAndAttachments);

			MyItem.ExtractAttachments();
			AssertEquals("AttachmentCount", 2, MyItem.MailAttachments.Count);
			int expectedSize = MyItem.MI_Body.Length +
								MyItem.MailAttachments[0].MA_Data.Length +
								MyItem.MailAttachments[1].MA_Data.Length;

			AssertEquals("BodyAndAttachmentsSize", expectedSize, MyItem.SizeOfBodyAndAttachments);
		}

		public void TestBodyFirst2K()
		{
			MyItem.MI_SendDateTime = ZDateTime.UtcNow;
			MyItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			MyItem.MI_Body = ComplexMIMETxt;
			MyItem.MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();

			AssertEquals("Body First 2 K", MyItem.MI_Body.Substring(0, 2048), MyItem.BodyFirst2K);

			MyItem = Factory.New<MailItem>();

			AssertEquals("Empty Body First 2 K for unsaved MailItem", "", MyItem.BodyFirst2K);
		}

		public void TestSaveAttachmentsTo()
		{
			MyItem.MailAttachments.RemoveAll();
			var attachment1 = MyItem.MailAttachments.AddNew();
			attachment1.MA_FileName = "pic.bmp";
			var fileData1 = PicBmp;
			attachment1.MA_Data = new ZBlob(fileData1);

			var attachment2 = MyItem.MailAttachments.AddNew();
			attachment2.MA_FileName = "ComplexHeader.txt";
			var fileData2 = resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.Business.MailItem.TestFiles.ComplexHeader.txt");
			attachment2.MA_Data = new ZBlob(fileData2);

			var testDestinationDir = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			Directory.CreateDirectory(testDestinationDir);
			var fileName1 = Path.Combine(testDestinationDir, attachment1.MA_FileName);
			var fileName2 = Path.Combine(testDestinationDir, attachment2.MA_FileName);

			AssertEquals("Attachment1 file does not exist", false, File.Exists(fileName1));
			AssertEquals("Attachment2 file does not exist", false, File.Exists(fileName2));

			try
			{
				MyItem.SaveAllAttachmentsTo(testDestinationDir);
				var savedFileData1 = ReadBytes(fileName1);
				var savedFileData2 = ReadBytes(fileName2);

				AssertEquals("Attachment1 file exists", true, File.Exists(fileName1));
				AssertEquals("File1 has Identical content", fileData1, savedFileData1);
				AssertEquals("Attachment2 file exists", true, File.Exists(fileName2));
				AssertEquals("File2 has Identical content", fileData2, savedFileData2);
			}
			finally
			{
				File.Delete(fileName1);
				File.Delete(fileName2);
				Directory.Delete(testDestinationDir);
				AssertEquals("Attachment1 file does not exist", false, File.Exists(fileName1));
				AssertEquals("Attachment2 file does not exist", false, File.Exists(fileName2));
				AssertEquals("Test directory does not exist", false, Directory.Exists(testDestinationDir));
			}
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestSaveEntireEmailToDirectory()
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Header = ComplexHeaderTxt;
			item.MI_Body = ComplexMIMETxt;

			using (var tempFile = TempFile.NewInDirectory(Env.TempPath))
			using (var tempFileStream = File.OpenWrite(tempFile.Filename))
			{
				item.SaveEntireEmailAsEml(tempFileStream);
				var writtenContent = File.ReadAllText(tempFile.Filename);
				AssertContains(item.MI_Header.ToLower(), writtenContent.ToLower());
				AssertContains("Some Text", writtenContent);
				AssertContains("Blue Lace 16.bmp", writtenContent);

				bool fileHasBOM = DetectByteOrderMarks(tempFile.Filename);
				AssertEquals("Should not contain Byte Order Marks since it will not render correctly in Outlook", false, fileHasBOM);
			}

			item = Factory.New<MailItem>();
			item.MI_Header = "Subject: Hello everyone";
			item.MI_Body = "Daniel";
			MailAttachment attachment = Factory.New<MailAttachment>();
			attachment.MA_Encoding = "B64";
			attachment.MA_Data = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			attachment.MA_FileName = "test.txt";
			item.MailAttachments.Add(attachment);

			using (var tempFile = TempFile.NewInDirectory(Env.TempPath))
			using (var tempFileStream = File.OpenWrite(tempFile.Filename))
			{
				item.SaveEntireEmailAsEml(tempFileStream);
				var writtenContent = File.ReadAllText(tempFile.Filename);
				AssertContains("AQIDBAUGBwgJCg==", writtenContent);
				AssertContains("Daniel", writtenContent);
				AssertContains("Subject: Hello everyone", writtenContent);

				bool fileHasBOM = DetectByteOrderMarks(tempFile.Filename);
				AssertEquals("Should not contain Byte Order Marks since it will not render correctly in Outlook", false, fileHasBOM);
			}

			item.MI_Body = "Multibyte characters: “ῦ–ῤ”";
			using (var tempFile = TempFile.NewInDirectory(Env.TempPath))
			using (var tempFileStream = File.OpenWrite(tempFile.Filename))
			{
				item.SaveEntireEmailAsEml(tempFileStream);
				var writtenContent = File.ReadAllText(tempFile.Filename);
				AssertContains("Multibyte characters: “ῦ–ῤ”", writtenContent);
			}
		}

		public void TestSaveEntireEmailAsEmWhenRecipientMailAddressIsEmpty()
		{
			var mailItem = Factory.New<MailItem>();
			var mailRecipient = Factory.New<MailRecipient>();
			mailRecipient.MR_RecipientType = nameof(MailRecipient.RecipientTypes.TO);
			mailRecipient.MR_MI = mailItem.PK;

			mailItem.MI_Body = "Test Recipient Mail Address Is Empty";
			using (var tempFile = TempFile.NewInDirectory(Env.TempPath))
			using (var tempFileStream = File.OpenWrite(tempFile.Filename))
			{
				AssertNoExceptionThrown(() => mailItem.SaveEntireEmailAsEml(tempFileStream));
				var writtenContent = File.ReadAllText(tempFile.Filename);
				AssertContains("Test Recipient Mail Address Is Empty", writtenContent);
			}
		}

		public void TestMI_SendDateTime()
		{
			MyItem = Factory.New<MailItem>();
			MailAttachment attachment = Factory.New<MailAttachment>();
			attachment.MA_Encoding = "B64";
			attachment.MA_Data = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			MyItem.MI_Header =
				"From: <sedi@example.com>\r\n" +
				"To: <sedi@example.com>\r\n" +
				"Subject: 1111111_1_1234567\r\n" +
				"Date: Wed, 12 Mar 1601 09:33:05 +1100\r\n" +
				"Mime-Version: 1.0\r\n" +
				"Content-Transfer-Encoding: base64\r\n" +
				"Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"\r\n";
			AssertEquals("Invalid Small Date Time should not be set (1601)", DateTime.MinValue, MyItem.MI_SendDateTime);
		}

		public void TestHasContentDefault()
		{
			AssertEquals(false, MyItem.HasContent);
		}

		public void TestHasContentBody()
		{
			MyItem.MI_Body = "foo";
			AssertEquals(true, MyItem.HasContent);
		}

		public void TestHasContentAttachment()
		{
			MailAttachment attachment = MyItem.MailAttachments.AddNew();
			AssertEquals("While attachment body is still empty, has no content", false, MyItem.HasContent);
			attachment.MA_Data = ZBlob.FromAscii("monkeys in the trees");
			AssertEquals(true, MyItem.HasContent);
		}

		public void TestHasContentMultipleAttachments()
		{
			MailAttachment attachment = MyItem.MailAttachments.AddNew();
			MailAttachment attachment2 = MyItem.MailAttachments.AddNew();
			attachment2.MA_Data = ZBlob.FromAscii("naked bananas");
			AssertEquals(true, MyItem.HasContent);
		}

		#region TestAddRecipientForUserCommunication

		#region DEBUG build

		public void TestAddRecipientForUserCommunication_DebugBuild()
		{
			AssertTestAddRecipientForUserCommunication_DebugBuild(
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: true,
				recipientShouldBeNull: true,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: string.Empty
			);

			AssertTestAddRecipientForUserCommunication_DebugBuild(
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "user@test.com"
			);
		}

		public void TestAddRecipientForUserCommunication_DebugBuild_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_DebugBuild_Hosted(
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_DebugBuild_Hosted(
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_DebugBuild_EmailDestinationOverride()
		{
			AssertTestAddRecipientForUserCommunication_DebugBuild(
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "override@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_DebugBuild(
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "override@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_DebugBuild_EmailDestinationOverride_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_DebugBuild_Hosted(
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_DebugBuild_Hosted(
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: true,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		#endregion

		#region RELEASE build, PRODUCTION DB

		public void TestAddRecipientForUserCommunication_ReleaseBuild_ProductionDB()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "email1@email.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "email1@email.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_ProductionDB_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_ProductionDB_EmailDestinationOverride()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "override@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "override@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_ProductionDB_EmailDestinationOverride_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunicationShouldReportErrorWhenRecipientMailAddressIsEmpty_ProductionDB()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Production,
				emailDestination: "",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: ""
			);
			AssertContains("Should report error when recipient mail address is empty", "Recipient Email Address is empty when added to MailItem", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region RELEASE build, NON-PRODUCTION DB

		public void TestAddRecipientForUserCommunication_ReleaseBuild_NonProductionDB()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: true,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: string.Empty
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "email1@email.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "user@test.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_NonProductionDB_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: string.Empty,
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_NonProductionDB_EmailDestinationOverride()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "override@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "email1@email.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "override@cargowise.com"
			);
		}

		public void TestAddRecipientForUserCommunication_ReleaseBuild_NonProductionDB_EmailDestinationOverride_InHostingSupportGroup()
		{
			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "",
				expectedRecipientAddress: "hosting@cargowise.com"
			);

			AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(
				dbType: DatabaseTypes.Codes.Training,
				emailDestination: "hosting@cargowise.com",
				emailDestinationOverride: "override@cargowise.com",
				isTestingDebug: false,
				recipientShouldBeNull: false,
				loggedInUserEmailAddress: "user@test.com",
				expectedRecipientAddress: "hosting@cargowise.com"
			);
		}

		#endregion

		void AssertTestAddRecipientForUserCommunication_DebugBuild(string emailDestination, string emailDestinationOverride, bool isTestingDebug, bool recipientShouldBeNull, string loggedInUserEmailAddress, string expectedRecipientAddress)
		{
			staff.GS_EmailAddress = loggedInUserEmailAddress;
			using (EnvProxy.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, null, true, Factory)))
			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				AssertTestAddRecipientForUserCommunication(emailDestination, emailDestinationOverride, isTestingDebug, recipientShouldBeNull, expectedRecipientAddress);
			}
		}

		void AssertTestAddRecipientForUserCommunication_DebugBuild_Hosted(string emailDestination, string emailDestinationOverride, bool isTestingDebug, bool recipientShouldBeNull, string loggedInUserEmailAddress, string expectedRecipientAddress)
		{
			string originalHostedLocation = EnvProxy.HostedLocation;
			try
			{
				EnvProxy.SetHostedLocationForTest("CW1");
				staff.GS_EmailAddress = loggedInUserEmailAddress;
				using (EnvProxy.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, null, true, Factory)))
				{
					AssertTestAddRecipientForUserCommunication(emailDestination, emailDestinationOverride, isTestingDebug, recipientShouldBeNull, expectedRecipientAddress);
				}
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedLocation);
			}
		}

		void AssertTestAddRecipientForUserCommunication_ReleaseBuild(string dbType, string emailDestination, string emailDestinationOverride, bool isTestingDebug, bool recipientShouldBeNull, string loggedInUserEmailAddress, string expectedRecipientAddress)
		{
			LicenceTypeChanger.SetSystemLicence(dbType);
			staff.GS_EmailAddress = loggedInUserEmailAddress;
			using (EnvProxy.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, null, true, Factory)))
			{
				AssertTestAddRecipientForUserCommunication(emailDestination, emailDestinationOverride, isTestingDebug, recipientShouldBeNull, expectedRecipientAddress);
			}
		}

		void AssertTestAddRecipientForUserCommunication_ReleaseBuild_Hosted(string dbType, string emailDestination, string emailDestinationOverride, bool isTestingDebug, bool recipientShouldBeNull, string loggedInUserEmailAddress, string expectedRecipientAddress)
		{
			string originalHostedLocation = EnvProxy.HostedLocation;
			try
			{
				EnvProxy.SetHostedLocationForTest("CW1");
				LicenceTypeChanger.SetSystemLicence(dbType);
				staff.GS_EmailAddress = loggedInUserEmailAddress;
				using (EnvProxy.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, null, true, Factory)))
				{
					AssertTestAddRecipientForUserCommunication(emailDestination, emailDestinationOverride, isTestingDebug, recipientShouldBeNull, expectedRecipientAddress);
				}
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedLocation);
			}
		}

		void AssertTestAddRecipientForUserCommunication(string emailDestination, string emailDestinationOverride, bool isTestingDebug, bool recipientShouldBeNull, string expectedRecipientAddress)
		{
			using (MailItem.SkipDebugCheckForShouldAddRecipientTesting(!isTestingDebug))
			using (EnvProxy.Instance.Registry.RawRegistry.HostedNotificationsEmailOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "hosting@cargowise.com"))
			{
				Env.Registry.EmailDestinationOverride = emailDestinationOverride;

				var item = Factory.New<MailItem>();
				AssertEquals(0, item.MailRecipients.Count);

				IMailRecipient reciptient = item.AddRecipientForUserCommunication(emailDestination, MailRecipient.RecipientTypes.TO);
				AssertEquals(recipientShouldBeNull, reciptient == null);

				if (!recipientShouldBeNull)
				{
					AssertEquals(expectedRecipientAddress, item.MailRecipients[0].EmailAddress);
				}
				else
				{
					AssertEquals(emailDestination, item.overriddenRecipients.First());
				}
			}
		}

		#endregion

		public void TestMI_Header_RecipientsAreAddedAsUserCommunication()
		{
			Env.Registry.EmailDestinationOverride = "fake@email.com";
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Header = "To: <Mykola@w1seTek.com>, <egi@blah.com>, <alex@example.com>";
			var sortedRecipients = mailItem.MailRecipients.Cast<MailRecipient>().OrderBy(r => r.EmailAddress).ToList();
			AssertEquals(1, sortedRecipients.Count);
			AssertEquals("fake@email.com", sortedRecipients[0].EmailAddress);
		}

		public void TestAddRecipients()
		{
			AssertAddRecipients(null);
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";
			Env.Registry.SystemEmailDestinationOverride = false;
			AssertAddRecipients("override@cargowise.com");
		}

		void AssertAddRecipients(string overrideEmail)
		{
			MailItem item = Factory.New<MailItem>();

			AssertEquals(0, item.MailRecipients.Count);
			int counter = 0;

			IMailRecipient obj = item.AddRecipientForUserCommunication("email1@email.com", MailRecipient.RecipientTypes.TO);
			AssertEquals(++counter, item.MailRecipients.Count);
			AssertEquals(string.IsNullOrEmpty(overrideEmail) ? "email1@email.com" : overrideEmail, item.MailRecipients[0].EmailAddress);
			AssertEquals(nameof(MailRecipient.RecipientTypes.TO), item.MailRecipients[0].MR_RecipientType);

			obj = item.AddRecipientForUserCommunication("email2@email.com", MailRecipient.RecipientTypes.CC);
			if (string.IsNullOrEmpty(overrideEmail))
			{
				AssertNotNull(obj);
				AssertEquals(++counter, item.MailRecipients.Count);
				AssertEquals("email2@email.com", item.MailRecipients[counter - 1].EmailAddress);
				AssertEquals(nameof(MailRecipient.RecipientTypes.CC), item.MailRecipients[counter - 1].MR_RecipientType);
			}
			else
			{
				AssertNull(obj);
			}

			obj = item.AddRecipientForUserCommunication("email3@email.com", MailRecipient.RecipientTypes.BCC);
			if (string.IsNullOrEmpty(overrideEmail))
			{
				AssertNotNull(obj);
				AssertEquals(++counter, item.MailRecipients.Count);
				AssertEquals("email3@email.com", item.MailRecipients[counter - 1].EmailAddress);
				AssertEquals(nameof(MailRecipient.RecipientTypes.BCC), item.MailRecipients[counter - 1].MR_RecipientType);
			}
			else
			{
				AssertNull(obj);
			}
			RecipientDef def = new RecipientDef("qwerty", false);
			obj = item.AddRecipientFromRecipientDef(def, MailRecipient.RecipientTypes.TO);
			if (string.IsNullOrEmpty(overrideEmail))
			{
				AssertNotNull(obj);
				AssertEquals(++counter, item.MailRecipients.Count);
				AssertEquals("qwerty", item.MailRecipients[counter - 1].EmailAddress);
				AssertEquals(nameof(MailRecipient.RecipientTypes.TO), item.MailRecipients[counter - 1].MR_RecipientType);
			}
			else
			{
				AssertNull(obj);
			}

			obj = item.AddRecipientForSystemCommunication("email1x@email.com", MailRecipient.RecipientTypes.TO);
			AssertEquals(++counter, item.MailRecipients.Count);
			AssertEquals("email1x@email.com", item.MailRecipients[counter - 1].EmailAddress);
			AssertEquals(nameof(MailRecipient.RecipientTypes.TO), item.MailRecipients[counter - 1].MR_RecipientType);

			obj = item.AddRecipientForSystemCommunication("email2x@email.com", MailRecipient.RecipientTypes.CC);
			AssertEquals(++counter, item.MailRecipients.Count);
			AssertEquals("email2x@email.com", item.MailRecipients[counter - 1].EmailAddress);
			AssertEquals(nameof(MailRecipient.RecipientTypes.CC), item.MailRecipients[counter - 1].MR_RecipientType);

			obj = item.AddRecipientForSystemCommunication("email3x@email.com", MailRecipient.RecipientTypes.BCC);
			AssertEquals(++counter, item.MailRecipients.Count);
			AssertEquals("email3x@email.com", item.MailRecipients[counter - 1].EmailAddress);
			AssertEquals(nameof(MailRecipient.RecipientTypes.BCC), item.MailRecipients[counter - 1].MR_RecipientType);

			def = new RecipientDef("qwerty2", true);
			obj = item.AddRecipientFromRecipientDef(def, MailRecipient.RecipientTypes.TO);
			AssertEquals(++counter, item.MailRecipients.Count);
			AssertEquals("qwerty2", item.MailRecipients[counter - 1].EmailAddress);
			AssertEquals(nameof(MailRecipient.RecipientTypes.TO), item.MailRecipients[counter - 1].MR_RecipientType);
		}

		public void TestHumanReadableShortcutName()
		{
			var factory = new BusinessObjectFactory();
			var item = Factory.New<MailItem>();

			AssertEquals("Empty", ZString.Empty, item.HumanReadableShortcutName);

			item.MI_From = "Bob <bob@cargowise.com>";
			AssertEquals("From", "Bob <bob@cargowise.com>", item.HumanReadableShortcutName);

			item.MI_Subject = "Hello from Bob";
			AssertEquals("Subject", "Hello from Bob", item.HumanReadableShortcutName);
		}

		public void TestGetFromEmailAddress()
		{
			var email = Factory.New<MailItem>();
			email.MI_Body = "Test email with invalid MI_From";
			email.MI_Subject = "Test email with invalid MI_From";
			email.MI_From = "; ";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);

			AssertEquals(string.Empty, email.GetFromEmailAddress());
		}

		protected override void SetUp()
		{
			base.SetUp();

			MyItem = Factory.New<MailItem>();
			Attachment1 = Factory.New<MailAttachment>();
			Attachment2 = Factory.New<MailAttachment>();
			Attachment3 = Factory.New<MailAttachment>();

			Recipient1 = MyItem.AddRecipientForUserCommunication("email1@email.com", MailRecipient.RecipientTypes.TO);
			Recipient2 = MyItem.AddRecipientForUserCommunication("email2@email.com", MailRecipient.RecipientTypes.TO);
			Recipient3 = MyItem.AddRecipientForUserCommunication("email3@email.com", MailRecipient.RecipientTypes.TO);

			MyItem.AddRecipientForUserCommunication("ccemail1@email.com", MailRecipient.RecipientTypes.CC);
			MyItem.AddRecipientForUserCommunication("ccemail2@email.com", MailRecipient.RecipientTypes.CC);
			MyItem.AddRecipientForUserCommunication("ccemail3@email.com", MailRecipient.RecipientTypes.CC);

			MyItem.AddRecipientForUserCommunication("bccemail1@email.com", MailRecipient.RecipientTypes.BCC);
			MyItem.AddRecipientForUserCommunication("bccemail2@email.com", MailRecipient.RecipientTypes.BCC);
			MyItem.AddRecipientForUserCommunication("bccemail3@email.com", MailRecipient.RecipientTypes.BCC);

			Attachment1.MA_MI = MyItem.PK;
			Attachment2.MA_MI = MyItem.PK;
			Attachment3.MA_MI = MyItem.PK;

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "kelvin.master";
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
		MailItem MyItem;
		MailAttachment Attachment1;
		MailAttachment Attachment2;
		MailAttachment Attachment3;
		IMailRecipient Recipient1;
		IMailRecipient Recipient2;
		IMailRecipient Recipient3;
		GlbStaff staff;

		string ComplexHeaderTxt => resourceRetriever.Value.GetString("Enterprise.MailManager.Test.Business.MailItem.TestFiles.ComplexHeader.txt");

		string ComplexMIMETxt => resourceRetriever.Value.GetString("Enterprise.MailManager.Test.Business.MailItem.TestFiles.ComplexMIME.txt");

		byte[] PicBmp => resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.Business.MailItem.TestFiles.pic.bmp");

		string ReadFile(string filename)
		{
			using (FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				byte[] inputBytes = new byte[inputStream.Length];
				inputStream.Read(inputBytes, 0, inputBytes.Length);
				return System.Text.Encoding.ASCII.GetString(inputBytes);
			}
		}

		byte[] ReadBytes(string filename)
		{
			using (FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				byte[] inputBytes = new byte[inputStream.Length];
				inputStream.Read(inputBytes, 0, inputBytes.Length);
				return inputBytes;
			}
		}

		bool DetectByteOrderMarks(string fileName)
		{
			var fileBytes = ReadBytes(fileName);
			if (fileBytes.Length >= 3)
			{
				if (fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
				{
					return true;
				}
			}

			return false;
		}
	}
}
