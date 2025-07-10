using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.Testing
{
	abstract class MailManagerEmailSenderTest<T> : TestCaseWithFactory where T : class
	{
		public void TestAnalyzeTableHeaders()
		{
			SetupPostMaster();
			AssertEmailException(new HtmlEmailDef(), true);
			AssertEmailException(new EmailDef() { ContentType = EmailContentTypes.HTML }, true);
			AssertEmailException(new EmailDef() { ContentType = EmailContentTypes.PlainText }, false);
			AssertEmailException(new EmailDef() { ContentType = EmailContentTypes.Calendar }, false);
		}

		void AssertEmailException(EmailDef email, bool shouldThrow)
		{
			email.Subject = "Email Subject";
			AssertNoExceptionThrown(() => SetBodyAndSend(email, ""));
			AssertNoExceptionThrown(() => SetBodyAndSend(email, "<tr class='hello'><td></td></tr>"));
			AssertNoExceptionThrown(() => SetBodyAndSend(email, "<tr class='hello'><th></th></tr>"));
			AssertNoExceptionThrown(() => SetBodyAndSend(email, "<tr class='tableheadings'><th></th></tr>"));
			AssertNoExceptionThrown(() => SetBodyAndSend(email, "<tr class='tableheadings'><th></th>"));
			string body1 = "<tr class='tableheadings'><td></td></tr>";
			string body2 = "<tr class='tableheadings'><td></td><th></th></tr>";
			string body3 = "<tr class='tableheadings'><th></th><td></td></tr>";
			string body4 = "<tr class='tableheadings'><td></td>";
			if (shouldThrow)
			{
				AssertExceptionThrown(typeof(ArgumentException), "Wrong table header tag found in email. You should use <th> tags inside row with \"tableheadings\" class.\r\nMessage Body: <tr class='tableheadings'><td></td></tr>",
					() => SetBodyAndSend(email, body1));
				AssertExceptionThrown(typeof(ArgumentException), "Wrong table header tag found in email. You should use <th> tags inside row with \"tableheadings\" class.\r\nMessage Body: <tr class='tableheadings'><td></td><th></th></tr>",
					() => SetBodyAndSend(email, body2));
				AssertExceptionThrown(typeof(ArgumentException), "Wrong table header tag found in email. You should use <th> tags inside row with \"tableheadings\" class.\r\nMessage Body: <tr class='tableheadings'><th></th><td></td></tr>",
					() => SetBodyAndSend(email, body3));
				AssertExceptionThrown(typeof(ArgumentException), "Wrong table header tag found in email. You should use <th> tags inside row with \"tableheadings\" class.\r\nMessage Body: <tr class='tableheadings'><td></td>",
					() => SetBodyAndSend(email, body4));
			}
			else
			{
				AssertNoExceptionThrown(() => SetBodyAndSend(email, body1));
				AssertNoExceptionThrown(() => SetBodyAndSend(email, body2));
				AssertNoExceptionThrown(() => SetBodyAndSend(email, body3));
				AssertNoExceptionThrown(() => SetBodyAndSend(email, body4));
			}
		}

		void SetBodyAndSend(EmailDef email, string body)
		{
			email.Body = body;
			((IOutgoingMailManager)MailCreator).CreateAndSaveToPostmasterGroup(email);
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestSendToPostmastersGroupIfGroupIsAbsent()
		{
			GlbGroup pmg = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			MethodInfo delete = typeof(GlbGroup).GetMethod("Delete", BindingFlags.Instance | BindingFlags.NonPublic);
			delete.Invoke(pmg, new object[] { false });
			Factory.Save();

			defForPostmasterTests = new EmailDef();
			defForPostmasterTests.Body = "wazuuuuuuuuuuuuuuuup";
			defForPostmasterTests.Subject = "test subject";
			CreateOutboundEmailForTestingSendingToPostmasters();
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestSendToGroupIfPMGGroupIsAbsent()
		{
			GlbGroup pmg = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			MethodInfo delete = typeof(GlbGroup).GetMethod("Delete", BindingFlags.Instance | BindingFlags.NonPublic);
			delete.Invoke(pmg, new object[] { false });
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "111";
			group.GG_Desc = "zzz";
			Factory.Save();

			defForPostmasterTests = new EmailDef();
			defForPostmasterTests.Body = "wazuuuuuuuuuuuuuuuup";
			defForPostmasterTests.Subject = "test subject";
			((IOutgoingMailManager)MailCreator).CreateAndSave(defForPostmasterTests, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));
		}

		public void TestSendToGroupIfPMGGroupIsAbsentButEmailHasOwnReceipient()
		{
			GlbGroup pmg = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			MethodInfo delete = typeof(GlbGroup).GetMethod("Delete", BindingFlags.Instance | BindingFlags.NonPublic);
			delete.Invoke(pmg, new object[] { false });
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "111";
			group.GG_Desc = "zzz";
			Factory.Save();

			defForPostmasterTests = new EmailDef();
			defForPostmasterTests.Body = "wazuuuuuuuuuuuuuuuup";
			defForPostmasterTests.Subject = "test subject";
			defForPostmasterTests.AddRecipientForUserCommunication("foo@bar.com", RecipientDef.RecipientTypes.TO);
			((IOutgoingMailManager)MailCreator).CreateAndSave(defForPostmasterTests, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));
			AssertEquals("Should have no error sending to an empty group if email has own receipient", 1, ((IOutgoingMailManager)MailCreator).EmailsCreated.Count);
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestSendToPostmastersGroupIfGroupIsAbsentAndAnotherPMGCreated()
		{
			GlbGroup pmg = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			MethodInfo delete = typeof(GlbGroup).GetMethod("Delete", BindingFlags.Instance | BindingFlags.NonPublic);
			delete.Invoke(pmg, new object[] { false });
			Factory.Save();

			GlbGroup newpmg = Factory.New<GlbGroup>();
			newpmg.GG_Code = "PMG";
			newpmg.GG_Desc = "zzz";
			Factory.Save();

			defForPostmasterTests = new EmailDef();
			defForPostmasterTests.Body = "wazuuuuuuuuuuuuuuuup";
			defForPostmasterTests.Subject = "test subject";
			try
			{
				CreateOutboundEmailForTestingSendingToPostmasters();
				Fail("Should throw EmailHasNoRecipientsException.");
			}
			catch (EmailHasNoRecipientsException)
			{
				newpmg = new BusinessObjectFactory().Load<GlbGroup>(newpmg.PK);
				AssertEquals("PMG1", newpmg.GG_Code);
				AssertEquals("zzz", newpmg.GG_Desc);
				throw;
			}
		}

		EmailDef defForPostmasterTests;
		public void TestSendToPostmastersGroup()
		{
			ResetStaff();
			// Put Pluto in the PMG and send again
			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"Pluto.Something";
			staffPluto.GS_Code = "PS";
			staffPluto.GS_EmailAddress = "Pluto.Something@cargowise.com";
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staffPluto.Groups.Add(postMastersGroup);
			Factory.Save();
			EmailDef emailDefForPluto = new EmailDef();
			emailDefForPluto.Body = "Hello again";
			emailDefForPluto.Subject = "Still a test subject";
			// Expect to see a mail to Pluto
			defForPostmasterTests = emailDefForPluto;
			CreateOutboundEmailForTestingSendingToPostmasters();
			MailItem sentItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
			AssertEquals(sentItem.MI_Body, emailDefForPluto.Body);
			AssertEquals(sentItem.MI_Subject, emailDefForPluto.Subject);
			AssertEquals(sentItem.MailRecipients[0].EmailAddress, staffPluto.GS_EmailAddress);

			// Now put Minnie in the PMG too, expect to see an emai to both miine and Pluto
			GlbStaff staffMinnie = Factory.New<GlbStaff>();
			staffMinnie.GS_LoginName = @"minnie.mouse";
			staffMinnie.GS_Code = "MIN";
			staffMinnie.GS_EmailAddress = "minnie.mouse@cargowise.com";
			staffMinnie.Groups.Add(postMastersGroup);
			Factory.Save();
			EmailDef emailDefForPlutoAndMinnie = new EmailDef();
			emailDefForPlutoAndMinnie.Body = "Mickey is a cuckold";
			emailDefForPlutoAndMinnie.Subject = "Yet one more test subject";
			defForPostmasterTests = emailDefForPlutoAndMinnie;
			CreateOutboundEmailForTestingSendingToPostmasters();
			// Now send and expect to see a mail with TWO recipients
			sentItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
			AssertEquals(sentItem.MI_Body, emailDefForPlutoAndMinnie.Body);
			AssertEquals(sentItem.MI_Subject, emailDefForPlutoAndMinnie.Subject);
			// No guarantee of the order that the recipients are added, sooo....
			AssertEquals(2, sentItem.MailRecipients.Count);
			Assert("Recipients must be Minnie and Pluto, in any order",
							(sentItem.MailRecipients[0].EmailAddress == staffMinnie.GS_EmailAddress && sentItem.MailRecipients[1].EmailAddress == staffPluto.GS_EmailAddress)
							||
							(sentItem.MailRecipients[1].EmailAddress == staffMinnie.GS_EmailAddress && sentItem.MailRecipients[0].EmailAddress == staffPluto.GS_EmailAddress)
					);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		void CreateOutboundEmailForTestingSendingToPostmasters()
		{
			((IOutgoingMailManager)MailCreator).CreateAndSaveToPostmasterGroup(this.defForPostmasterTests);
		}

		public void TestMailInterface()
		{
			AssertNotNull("MailSender", MailCreator);
		}

		public void TestGetEmailsFromOutlookFormatEmailAddresses()
		{
			string[] result = MailCreator.GetEmailsFromOutlookFormatEmailAddresses("123;345");
			AssertEquals("Result.Length", 2, result.Length);
			AssertEquals("Result[0]", "123", result[0]);
			AssertEquals("Result[1]", "345", result[1]);
		}

		public void TestGetEmailsCreated()
		{
			AssertEquals("EmailsNotSentBecauseInTestMode", 0, MailCreator.EmailsCreated.Count);
			TestCreateAndSaveSimple();
			AssertEquals("EmailsNotSentBecauseInTestMode", 1, MailCreator.EmailsCreated.Count);
		}

		public void TestClearEmailsCreated()
		{
			AssertEquals("EmailsNotSentBecauseInTestMode", 0, MailCreator.EmailsCreated.Count);
			TestCreateAndSaveSimple();
			AssertEquals("EmailsNotSentBecauseInTestMode", 1, MailCreator.EmailsCreated.Count);
			MailCreator.EmailsCreated.Clear();
			AssertEquals("EmailsNotSentBecauseInTestMode", 0, MailCreator.EmailsCreated.Count);
		}

		public void TestNewMailItem()
		{
			EmailDef myEmail = new EmailDef();
			myEmail.Subject = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
			MailItem item = MailCreator.NewMailItem(Factory, myEmail);
			AssertEquals("Mail item content type", EmailContentTypes.PlainText.ContentTypeCode, item.MI_ContentType);

			EmailDef email2 = new EmailDef();
			email2.ContentType = EmailContentTypes.HTML;
			MailItem item2 = MailCreator.NewMailItem(Factory, email2);
			AssertEquals("Mail item content type", EmailContentTypes.HTML.ContentTypeCode, item2.MI_ContentType);
		}

		public void TestNewMailItemForBCCOnlyEmail()
		{
			EmailDef myEmail = new EmailDef();
			myEmail.AddRecipientForUserCommunication("test@test.com");
			MailItem item = MailCreator.NewMailItem(Factory, myEmail);
			AssertEquals("Recipients Count", 1, item.MailRecipients.Count);
			AssertEquals("Recipient Type", "TO", item.MailRecipients[0].MR_RecipientType);
		}

		public void TestNewMailItemForBusinessEntityID()
		{
			EmailDef email = new EmailDef();
			MailItem item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("Business Entity ID", "", item.MI_BusinessEntityID);

			ZGuid guid = ZGuid.NewZGuid();
			email.SetupBusinessEntityInfo(guid, string.Empty, string.Empty);
			item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("BusinessEntityID", guid.ToString(), item.MI_BusinessEntityID);
		}

		public void TestNewMailItemForBusinessEntityTableCode()
		{
			EmailDef email = new EmailDef();
			MailItem item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("Business Entity TableCode", "", item.MI_BusinessEntityTableCode);

			email.SetupBusinessEntityInfo(ZGuid.Empty, ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix, string.Empty);
			item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("BusinessEntityTableCode", ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix, item.MI_BusinessEntityTableCode);
		}

		public void TestNewMailItemForBusinessEntityJobNumber()
		{
			var email = new EmailDef();
			var item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("BusinessEntityJobNumber", "", item.MI_BusinessEntityjobNumber);

			email.SetupBusinessEntityInfo(ZGuid.Empty, string.Empty, "S00000001");
			item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("BusinessEntityJobNumber", "S00000001", item.MI_BusinessEntityjobNumber);
		}

		public void TestNewMailItemForListUnsubscribe()
		{
			var email = new EmailDef();
			var item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("", item.MI_ListUnsubscribe);

			email.ListUnsubscribe = "<http://www.cw1.com/u?123>";
			item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("<http://www.cw1.com/u?123>", item.MI_ListUnsubscribe);
		}

		public void TestNewMailItemForDocumentName()
		{
			EmailDef email = new EmailDef();
			MailItem item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("DocumentName", "", item.MI_DocumentName);

			email.DocumentName = "Pre-Alert";
			item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("DocumentName", "Pre-Alert", item.MI_DocumentName);
		}

		public void TestNewMailItemForSenderStaffID()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var email = new EmailDef(staff.PK.ToGuid());
			var item = MailCreator.NewMailItem(Factory, email);
			AssertEquals(staff.PK.ToString(), item.MI_SenderStaffID);
		}

		public void TestNewMailItemForQueueWithLowPriority()
		{
			EmailDef email = new EmailDef();
			MailItem item = MailCreator.NewMailItem(Factory, email);
			AssertEquals("QueueWithLowPriority", false, item.MI_QueueWithLowPriority);

			EmailDef email2 = new EmailDef();
			email2.QueueWithLowPriority = true;
			MailItem item2 = MailCreator.NewMailItem(Factory, email2);
			AssertEquals("QueueWithLowPriority", true, item2.MI_QueueWithLowPriority);
		}

		public void TestNewMailItemForHeader()
		{
			EmailDef email = new EmailDef();
			MailItem item = MailCreator.NewMailItem(Factory, email);
			var baseHeader = $"From: \"{email.FromDisplayName}\" <{email.FromAddress}>\r\nX-SenderStaffID: {email.SenderStaffPK}\r\n";
			AssertEquals("Header", baseHeader, item.MI_Header);

			EmailDef email2 = new EmailDef();
			email2.Headers["X-Auto-Response-Suppress-Header"] = "All";
			MailItem item2 = MailCreator.NewMailItem(Factory, email2);
			AssertEquals("Header", baseHeader + "X-Auto-Response-Suppress-Header: All\r\n", item2.MI_Header);

			EmailDef email3 = new EmailDef();
			email3.Headers["X-Auto-Response-Suppress-Header"] = "All";
			email3.Headers["Auto-Submitted"] = "auto-replied";
			email3.Headers["testHeader"] = "wrong";
			email3.Headers["testHeader"] = "right";
			MailItem item3 = MailCreator.NewMailItem(Factory, email3);
			AssertEquals("Header", baseHeader + "X-Auto-Response-Suppress-Header: All\r\nAuto-Submitted: auto-replied\r\ntestHeader: right\r\n", item3.MI_Header);
		}

		public void TestValidateEmail_CreatesLogEntryInsteadOfSendingEmail()
		{
			EmailDef email = new EmailDef();
			email.Subject = "Set an email address you fool.";
			email.FromAddress = "";

			MailItem item = MailCreator.NewMailItem(Factory, email);

			AssertExceptionThrown(typeof(EmailHasNoFromAddressException), () => MailCreator.ValidateEmail(email, ""));
		}

		#region MIME

		public void TestCreateAndSaveMIMERawWithTimeDelay()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var rawEmail = resourceRetriever.GetString("Enterprise.MailManager.Test.UTF8Header.txt");
				MailCreator.CreateAndSaveMIMERaw(rawEmail, new TimeSpan(1, 0, 0));
				var savedItem = Factory.LoadTop1<MailItem>(new ZQuery());
				AssertNotNull(savedItem);
				Assert("SentTime is in future", savedItem.MI_SendDateTime > ZDateTime.UtcNow);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendSMIME1()
		{
			string sender = "nick@edi.com.au";
			string recipient = "nick@edi.com.au";
			string subject = "Subject1";

			string p12Filename = BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\BatchProcessor\TestFiles\SEDI_Test_SigningKeyPair.p12";
			string p12Password = @"4Customs&SEDI";

			using (Store signingStore = new Store(p12Filename, p12Password, true))
			{
				string crtFilename = BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\BatchProcessor\TestFiles\SigningCert.crt";

				using (Certificate encryptionCertificate = new Certificate(crtFilename))
				{
					((IOutgoingMIMEManager)MailCreator).CreateAndSaveSignedMIME("inner.txt", new byte[3] { 43, 54, 47 }, "application/edifact; name=inner.txt", "attachment; filename=inner.txt", sender, recipient, subject, signingStore, encryptionCertificate);

					MailItem testItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
					AssertEquals("MyItem.MI_Direction", "TRX", testItem.MI_Direction);
					AssertEquals("MyItem.MI_From", "nick@edi.com.au", testItem.MI_From);
					AssertEquals("MyItem.MI_Subject", "Subject1", testItem.MI_Subject);
					Assert("Importance is High in Header", testItem.MI_Header.Contains("Importance: HIGH\r\n"));
					Assert("Priority is 1 in Header", testItem.MI_Header.Contains("X-Priority: 1\r\n"));
					AssertEquals("MyItem.Recipient", "<nick@edi.com.au>", testItem.MailRecipients[0].EmailAddress);
					AssertEquals("Attachments", 0, testItem.MailAttachments.Count);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendSMIME2()
		{
			string sender = "nick@edi.com.au";
			string recipient = "nick@edi.com.au";
			string subject = "Subject2";

			string p12Filename = BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\BatchProcessor\TestFiles\SEDI_Test_SigningKeyPair.p12";
			string p12Password = @"4Customs&SEDI";

			using (Store signingStore = new Store(p12Filename, p12Password, true))
			{
				Certificate encryptionCertificate = null;

				((IOutgoingMIMEManager)MailCreator).CreateAndSaveSignedMIME("inner.txt", new byte[3] { 43, 54, 47 }, "application/edifact; name=inner.txt", "attachment; filename=inner.txt", sender, recipient, subject, signingStore, encryptionCertificate);

				MailItem testItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
				AssertEquals("MyItem.MI_Direction", "TRX", testItem.MI_Direction);
				AssertEquals("MyItem.MI_From", "nick@edi.com.au", testItem.MI_From);
				AssertEquals("MyItem.MI_Subject", "Subject2", testItem.MI_Subject);
				Assert("Importance is High in Header", testItem.MI_Header.Contains("Importance: HIGH\r\n"));
				Assert("Priority is 1 in Header", testItem.MI_Header.Contains("X-Priority: 1\r\n"));
				AssertEquals("MyItem.Recipient", "<nick@edi.com.au>", testItem.MailRecipients[0].EmailAddress);
				AssertEquals("Attachments", 0, testItem.MailAttachments.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendSMIME3()
		{
			string sender = "nick@edi.com.au";
			string recipient = "nick@edi.com.au";
			string subject = "Subject3";

			Store signingStore = null;
			string crtFilename = BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\BatchProcessor\TestFiles\SigningCert.crt";

			using (Certificate encryptionCertificate = new Certificate(crtFilename))
			{
				((IOutgoingMIMEManager)MailCreator).CreateAndSaveSignedMIME("inner.txt", new byte[3] { 43, 54, 47 }, "application/edifact; name=inner.txt", "attachment; filename=inner.txt", sender, recipient, subject, signingStore, encryptionCertificate);

				MailItem testItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
				AssertEquals("MyItem.MI_Direction", "TRX", testItem.MI_Direction);
				AssertEquals("MyItem.MI_From", "nick@edi.com.au", testItem.MI_From);
				AssertEquals("MyItem.MI_Subject", "Subject3", testItem.MI_Subject);
				Assert("Importance is High in Header", testItem.MI_Header.Contains("Importance: HIGH\r\n"));
				Assert("Priority is 1 in Header", testItem.MI_Header.Contains("X-Priority: 1\r\n"));
				AssertEquals("MyItem.Recipient", "<nick@edi.com.au>", testItem.MailRecipients[0].EmailAddress);
				AssertEquals("Attachments", 0, testItem.MailAttachments.Count);
			}
		}

		public void TestSendSMIME4()
		{
			string sender = "nick@edi.com.au";
			string recipient = "nick@edi.com.au";
			string subject = "Subject4";

			Store signingStore = null;
			Certificate encryptionCertificate = null;

			((IOutgoingMIMEManager)MailCreator).CreateAndSaveSignedMIME("inner.txt", new byte[3] { 43, 54, 47 }, "application/edifact; name=inner.txt", "attachment; filename=inner.txt", sender, recipient, subject, signingStore, encryptionCertificate);

			MailItem testItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
			AssertEquals("MyItem.MI_Direction", "TRX", testItem.MI_Direction);
			AssertEquals("MyItem.MI_From", "nick@edi.com.au", testItem.MI_From);
			AssertEquals("MyItem.MI_Subject", "Subject4", testItem.MI_Subject);
			Assert("Importance is High in Header", testItem.MI_Header.Contains("Importance: HIGH\r\n"));
			Assert("Priority is 1 in Header", testItem.MI_Header.Contains("X-Priority: 1\r\n"));
			AssertEquals("MyItem.Recipient", "<nick@edi.com.au>", testItem.MailRecipients[0].EmailAddress);
			AssertEquals("Attachments", 0, testItem.MailAttachments.Count);
		}

		#endregion

		#region Mail Creating

		public void TestCreateAndSaveSimple()
		{
			((IOutgoingMailManager)MailCreator).CreateAndSaveSimple("Subject", "Hello there", "nick@edi.com.au");
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MailItem myItem = factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);

			AssertEquals("MyItem.MI_Body", "Hello there", myItem.MI_Body);
			AssertEquals("MyItem.MI_Direction", "TRX", myItem.MI_Direction);
			AssertEquals("MyItem.MI_To", "nick@edi.com.au", myItem.MailRecipients[0].EmailAddress);
			AssertEquals("MyItem.MI_Subject", "Subject", myItem.MI_Subject);
			AssertEquals("MyItem.MI_Status", "QUE", myItem.MI_Status);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestCreateAndSaveSimpleWithBlankRecipientThrowsException()
		{
			((IOutgoingMailManager)MailCreator).CreateAndSaveSimple("The Subject", "The Body", "");
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestCreateWithNoRecipientsThrowsException()
		{
			EmailDef email = new EmailDef();
			email.Subject = "The Subject";
			email.Body = "The Body";
			((IOutgoingMailManager)MailCreator).CreateAndSave(email);
		}

		public void TestCreateAndSave()
		{
			EmailDef email = GetTestEmailDef1();
			((IOutgoingMailManager)MailCreator).CreateAndSave(email);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MailItem myItem = factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);

			AssertEquals("MyItem.MI_Body", email.Body, myItem.MI_Body);
			AssertEquals("MyItem.MI_Direction", "TRX", myItem.MI_Direction);
			AssertEquals("MyItem.MI_Subject", email.Subject, myItem.MI_Subject);
			AssertEquals("MyItem.MI_Status", "QUE", myItem.MI_Status);

			AssertEquals("MyItem.Recipient1", "nick@edi.com.au", myItem.MailRecipients[0].EmailAddress);
			AssertEquals("MyItem.Recipient2", "scally@edi.com.au", myItem.MailRecipients[1].EmailAddress);

			AssertEquals("MyItem.AttachmentFilename", "name", myItem.MailAttachments[0].MA_FileName);
		}

		public void TestNewItemWithToCCAndBCC()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			EmailDef email = GetTestEmailDef2();
			MailItem item = MailCreator.NewMailItem(factory, email);
			AssertEquals("RecipientCount", 3, item.MailRecipients.Count);
			foreach (IMailRecipient recipient in item.MailRecipients)
			{
				if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.TO))
				{
					AssertEquals("TORecipient", recipient.EmailAddress, "to@edi.com.au");
				}
				else if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.CC))
				{
					AssertEquals("CCRecipient", recipient.EmailAddress, "cc@edi.com.au");
				}
				else if (recipient.MR_RecipientType == nameof(MailRecipient.RecipientTypes.BCC))
				{
					AssertEquals("BCCRecipient", recipient.EmailAddress, "bcc@edi.com.au");
				}
				else
				{
					Assert(false);
				}
			}
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		public void TestCreateAndSaveWithRecipientGroup()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "~test1";
			staff1.GS_Code = "~t1";
			staff1.GS_EmailAddress = "test1@hotmail.com";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "~test2";
			staff2.GS_Code = "~t2";
			staff2.GS_EmailAddress = "test2@hotmail.com";

			GlbStaff inactive = Factory.New<GlbStaff>();
			inactive.GS_LoginName = "~test3";
			inactive.GS_Code = "~t3";
			inactive.GS_EmailAddress = "inactive@hotmail.com";
			inactive.GS_IsActive = false;

			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			Factory.Save();

			EmailDef email = new EmailDef();
			email.AddRecipientForUserCommunication("henry@edi.com.au");
			((IOutgoingMailManager)MailCreator).CreateAndSave(email, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));

			MailItem myItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);

			AssertEquals("Count", 3, myItem.MailRecipients.Count);

			bool explicitRecipientFound = false;
			bool groupRecipient1Found = false;
			bool groupRecipient2Found = false;
			bool inactiveGroupRecipientFound = false;

			foreach (IMailRecipient recipient in myItem.MailRecipients)
			{
				switch (recipient.EmailAddress)
				{
					case "henry@edi.com.au":
						explicitRecipientFound = true;
						break;
					case "test1@hotmail.com":
						groupRecipient1Found = true;
						break;
					case "test2@hotmail.com":
						groupRecipient2Found = true;
						break;
					case "inactive@hotmail.com":
						inactiveGroupRecipientFound = true;
						break;
				}
			}

			AssertEquals("Explicit Recipient is in the list", true, explicitRecipientFound);
			AssertEquals("Group Recipient 1 is in the list", true, groupRecipient1Found);
			AssertEquals("Group Recipient 2 is in the list", true, groupRecipient2Found);
			AssertEquals("Inactive Group Recipient not in the list", false, inactiveGroupRecipientFound);
		}

		public void TestCreateAndSaveWithRecipientGroupHasNoDuplicatedEmails()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "~test1";
			staff1.GS_Code = "~t1";
			staff1.GS_EmailAddress = "test1@hotmail.com";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "~test2";
			staff2.GS_Code = "~t2";
			staff2.GS_EmailAddress = "test2@hotmail.com";

			GlbStaff inactive = Factory.New<GlbStaff>();
			inactive.GS_LoginName = "~test3";
			inactive.GS_Code = "~t3";
			inactive.GS_EmailAddress = "inactive@hotmail.com";
			inactive.GS_IsActive = false;

			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			Factory.Save();

			EmailDef email = new EmailDef();
			email.AddRecipientForUserCommunication(staff1.GS_EmailAddress);
			((IOutgoingMailManager)MailCreator).CreateAndSave(email, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));

			MailItem myItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);

			AssertEquals("Count", 2, myItem.MailRecipients.Count);

			bool groupRecipient1Found = false;
			bool groupRecipient2Found = false;
			bool inactiveGroupRecipientFound = false;

			foreach (IMailRecipient recipient in myItem.MailRecipients)
			{
				switch (recipient.EmailAddress)
				{
					case "test1@hotmail.com":
						groupRecipient1Found = true;
						break;
					case "test2@hotmail.com":
						groupRecipient2Found = true;
						break;
					case "inactive@hotmail.com":
						inactiveGroupRecipientFound = true;
						break;
				}
			}

			AssertEquals("Group Recipient 1 is in the list", true, groupRecipient1Found);
			AssertEquals("Group Recipient 2 is in the list", true, groupRecipient2Found);
			AssertEquals("Inactive Group Recipient not in the list", false, inactiveGroupRecipientFound);
		}

		public void TestCreateAndAttachToEDocs()
		{
			var emailDef = GetTestEmailDef1();

			var staffToAttachEDocTo = Factory.New<GlbStaff>();
			staffToAttachEDocTo.GS_LoginName = "Test Staff";
			staffToAttachEDocTo.GS_Code = "TST";

			Factory.Save();

			var fileName = "Test File Name";
			var docType = Factory.Load<RefDocType>(OrganisationsDataRegistry.Instance.PermanentlySavedCampaignEmailDocType.Value);

			AssertEquals("Pre-Condition", 0, staffToAttachEDocTo.DocManagerInfo.AllEDocs.Count);

			var creator = (IOutgoingMailManager)OutgoingMailCreator.Instance;
			creator.CreateAndAttachToEDocs(Factory, emailDef, staffToAttachEDocTo, fileName, docType.RT_DocType, docType.RT_Desc);

			AssertEquals("eDoc should have been added to the bizO", 1, staffToAttachEDocTo.DocManagerInfo.AllEDocs.Count);
		}

		public void TestEmailDestinationOverride()
		{
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";

			Factory.Save();

			var email = new EmailDef();

			AssertEquals("Recipient count", 0, email.Recipients.Count);

			email.AddRecipientForUserCommunication("peter.griffin@cargowise.com");
			email.AddRecipientForUserCommunication("homer.simpson@cargowise.com");

			AssertEquals("Recipient count", 2, email.Recipients.Count);

			((IOutgoingMailManager)MailCreator).CreateAndSave(email);

			var myItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);

			//the order changes randomly, so we accept either ordering
			Assert("Override message contained",
				myItem.MI_Body.Contains(System.Environment.NewLine + "(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to peter.griffin@cargowise.com, homer.simpson@cargowise.com.)")
				||
				myItem.MI_Body.Contains(System.Environment.NewLine + "(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to homer.simpson@cargowise.com, peter.griffin@cargowise.com.)")
				);
		}

		public void TestCreateReminder_OverridenRecipient()
		{
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";

			Factory.Save();

			var reminder = new ReminderForTesting("GoGoGo",
															DateTimeKind.Local,
															new DateTime(2005, 11, 26, 11, 13, 0),
															new DateTime(2005, 11, 26, 13, 13, 0),
															"Test Calendar Event",
															"Some Body\r\nAnother Line of the body\r\n",
															"<HTML>" +
																"<HEAD>" +
																	"<TITILE>" +
																	"</TITLE>" +
																"</HEAD>" +
																"<BODY>" +
																	"Some Body<BR />" +
																	"Another Line of the body<BR />" +
																"</BODY>" +
															"</HTML>");

			reminder.Location = "A13 Some Ave";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Samuel", "Samuel@yoyo.com.au");
			reminder.Recipients.Add(Env.CurrentUser.FullName, new EmailDef(Env.CurrentUser.PK).FromAddress);

			reminder.CreateAppointment();

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			var sentItem = MailCreator.NewMailItem(Factory, sentEmail);

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
														"METHOD:REQUEST\r\n" +
														$"PRODID:{Core.Constants.ProductName}\r\n" +
														"VERSION:2.0\r\n" +
														"BEGIN:VEVENT\r\n" +
														"DTSTAMP:{0}\r\n" +
														"DTSTART:{1}\r\n" +
														"SUMMARY:Test Calendar Event\r\n" +
														"UID:GoGoGo\r\n" +
														"SEQUENCE:123\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"Samuel@yo\r\n yo.com.au\":MAILTO:Samuel@yoyo.com.au\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=\"Default@\r\n edi.com.au\":MAILTO:Default@edi.com.au\r\n" +
														$"ORGANIZER;CN=\"{Core.Constants.ProductSupportName}\":MAILTO:Default@edi.com.au\r\n" +
														"LOCATION:A13 Some Ave\r\n" +
														"DTEND:{2}\r\n" +
														"DESCRIPTION:Some Body\\NAnother Line of the body\\N" +
														"(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to Samuel@yoyo.com.au, Default@edi.com.au.)\\N\r\n" +
														"X-ALT-DESC;FMTTYPE=text/html:<HTML>" +
																						"<HEAD>" +
																							"<TITILE>" +
																							"</TITLE>" +
																						"</HEAD>" +
																						"<BODY>" +
																							"Some \r\n Body<BR />" +
																							"Another Line of the body<BR />" +
																							"(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to Samuel@yoyo.com.au, Default@edi.com.au.)<BR />" +
																						"</BODY>" +
																					"</HTML>\r\n" +
														"BEGIN:VALARM\r\n" +
														"ACTION:DISPLAY\r\n" +
														"DESCRIPTION:REMINDER\r\n" +
														"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
														"END:VALARM\r\n" +
														"TRANSP:OPAQUE\r\n" +
														"END:VEVENT\r\n" +
														"END:VCALENDAR",
														GetAndCheckDateStamp(sentEmail.Body),
														reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
														reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentItem.MI_Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		public void TestCreateReminder_OverridenRecipient2()
		{
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";

			Factory.Save();

			var reminder = new ReminderForTesting("GoGoGo",
															DateTimeKind.Local,
															new DateTime(2005, 11, 26, 11, 13, 0),
															new DateTime(2005, 11, 26, 13, 13, 0),
															"Test Calendar Event",
															"Some Body\r\nAnother Line of the body\r\n",
															"");

			reminder.Location = "A13 Some Ave";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Samuel", "Samuel@yoyo.com.au");
			reminder.Recipients.Add(Env.CurrentUser.FullName, new EmailDef(Env.CurrentUser.PK).FromAddress);

			reminder.CreateAppointment();

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			var sentItem = MailCreator.NewMailItem(Factory, sentEmail);

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
														"METHOD:REQUEST\r\n" +
														$"PRODID:{Core.Constants.ProductName}\r\n" +
														"VERSION:2.0\r\n" +
														"BEGIN:VEVENT\r\n" +
														"DTSTAMP:{0}\r\n" +
														"DTSTART:{1}\r\n" +
														"SUMMARY:Test Calendar Event\r\n" +
														"UID:GoGoGo\r\n" +
														"SEQUENCE:123\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"Samuel@yo\r\n yo.com.au\":MAILTO:Samuel@yoyo.com.au\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=\"Default@\r\n edi.com.au\":MAILTO:Default@edi.com.au\r\n" +
														$"ORGANIZER;CN=\"{Core.Constants.ProductSupportName}\":MAILTO:Default@edi.com.au\r\n" +
														"LOCATION:A13 Some Ave\r\n" +
														"DTEND:{2}\r\n" +
														"DESCRIPTION:Some Body\\NAnother Line of the body\\N" +
														"(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to Samuel@yoyo.com.au, Default@edi.com.au.)\r\n" +
														"BEGIN:VALARM\r\n" +
														"ACTION:DISPLAY\r\n" +
														"DESCRIPTION:REMINDER\r\n" +
														"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
														"END:VALARM\r\n" +
														"TRANSP:OPAQUE\r\n" +
														"END:VEVENT\r\n" +
														"END:VCALENDAR",
														GetAndCheckDateStamp(sentEmail.Body),
														reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
														reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentItem.MI_Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		string GetAndCheckDateStamp(string vCalMessage)
		{
			string result = "";

			int dateStampStartIndex = vCalMessage.IndexOf("DTSTAMP:");
			if (dateStampStartIndex != -1)
			{
				dateStampStartIndex += 8;
				int dateStampEndIndex = vCalMessage.IndexOf("\r\n", dateStampStartIndex);

				if (dateStampEndIndex > dateStampStartIndex)
				{
					result = vCalMessage.Substring(dateStampStartIndex, dateStampEndIndex - dateStampStartIndex);
				}
			}

			ZDateTime resultTime;
			Assert("Date Time stamp should be parseable", ZDateTime.TryParseExact(result, out resultTime, "yyyyMMdd\\THHmmss\\Z"));
			Assert("Date Time stamp should not be empty", !resultTime.IsEmpty);
			return result;
		}

		public void TestCreateReminder_OverridenRecipient3()
		{
			Env.Registry.EmailDestinationOverride = "override@cargowise.com";

			Factory.Save();

			var reminder = new ReminderForTesting("GoGoGo",
															DateTimeKind.Local,
															new DateTime(2005, 11, 26, 11, 13, 0),
															new DateTime(2005, 11, 26, 13, 13, 0),
															"Test Calendar Event",
															"Some Body\r\nAnother Line of the body\r\nNotes:\r\nFollow Up Notes:\r\n",
															"<HTML>" +
																"<HEAD>" +
																	"<TITILE>" +
																	"</TITLE>" +
																"</HEAD>" +
																"<BODY>" +
																	"Some Body<BR />" +
																	"Another Line of the body<BR />" +
																	"Notes:<BR />" +
																	"Follow Up Notes:<BR />" +
																"</BODY>" +
															"</HTML>");

			reminder.Location = "A13 Some Ave";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Samuel", "Samuel@yoyo.com.au");
			reminder.Recipients.Add(Env.CurrentUser.FullName, new EmailDef(Env.CurrentUser.PK).FromAddress);

			reminder.CreateAppointment();

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			var sentItem = MailCreator.NewMailItem(Factory, sentEmail);

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
														"METHOD:REQUEST\r\n" +
														$"PRODID:{Core.Constants.ProductName}\r\n" +
														"VERSION:2.0\r\n" +
														"BEGIN:VEVENT\r\n" +
														"DTSTAMP:{0}\r\n" +
														"DTSTART:{1}\r\n" +
														"SUMMARY:Test Calendar Event\r\n" +
														"UID:GoGoGo\r\n" +
														"SEQUENCE:123\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"Samuel@yo\r\n yo.com.au\":MAILTO:Samuel@yoyo.com.au\r\n" +
														"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=\"Default@\r\n edi.com.au\":MAILTO:Default@edi.com.au\r\n" +
														$"ORGANIZER;CN=\"{Core.Constants.ProductSupportName}\":MAILTO:Default@edi.com.au\r\n" +
														"LOCATION:A13 Some Ave\r\n" +
														"DTEND:{2}\r\n" +
														"DESCRIPTION:Some Body\\NAnother Line of the body\\N" +
														"Notes:\\N" +
														"Follow Up Notes:\\N" +
														"(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to Samuel@yoyo.com.au, Default@edi.com.au.)\\N\r\n" +
														"X-ALT-DESC;FMTTYPE=text/html:<HTML>" +
																						"<HEAD>" +
																							"<TITILE>" +
																							"</TITLE>" +
																						"</HEAD>" +
																						"<BODY>" +
																							"Some \r\n Body<BR />" +
																							"Another Line of the body<BR />" +
																							"Notes:<BR />" +
																							"Follow Up Notes:<BR />\r\n " +
																							"(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to Samuel@yoyo.com.au, Default@edi.com.au.)<BR />" +
																						"</BODY>" +
																					"</HTML>\r\n" +
														"BEGIN:VALARM\r\n" +
														"ACTION:DISPLAY\r\n" +
														"DESCRIPTION:REMINDER\r\n" +
														"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
														"END:VALARM\r\n" +
														"TRANSP:OPAQUE\r\n" +
														"END:VEVENT\r\n" +
														"END:VCALENDAR",
														GetAndCheckDateStamp(sentEmail.Body),
														reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
														reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentItem.MI_Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		public void TestSendEmailToGroupAll()
		{
			var hostingNotificationEmail = EnvProxy.Instance.Registry.HostedNotificationsEmailOverride;
			var hostEmails = new string[] { hostingNotificationEmail, "Hosting.Notifications@cargowise.com", "Hosting.Notifications@corporate.cargowise.com" };
			var allGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);

			var emailRecipient = allGroup.Staff.AddNew();
			emailRecipient.GS_EmailAddress = "clinton@edi.com.au";
			emailRecipient.GS_LoginName = "TMP";
			emailRecipient.GS_Code = "ZAC";

			for (int i = 0; i < hostEmails.Length; i++)
			{
				emailRecipient = allGroup.Staff.AddNew();
				emailRecipient.GS_EmailAddress = hostEmails[i];
				emailRecipient.GS_LoginName = "TMP" + i;
				emailRecipient.GS_Code = "XY" + i;
			}

			Factory.Save();

			var email = new EmailDef();
			email.AddRecipientForUserCommunication("henry@edi.com.au");
			((IOutgoingMailManager)MailCreator).CreateAndSave(email, Core.Constants.Groups.AllPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));

			var myItem = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
			var emailsSent = new List<string>();

			foreach (IMailRecipient recipient in myItem.MailRecipients)
			{
				emailsSent.Add(recipient.EmailAddress);
			}

			Assert(emailsSent.Any(x => x.Equals("clinton@edi.com.au", StringComparison.OrdinalIgnoreCase)));
			Assert(emailsSent.Any(x => x.Equals("henry@edi.com.au", StringComparison.OrdinalIgnoreCase)));
			Assert(emailsSent.All(x => hostEmails.All(y => !x.Equals(y, StringComparison.OrdinalIgnoreCase))));
		}

		public void TestCreate_WhenEmailWithNoRecipients()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.com";
			Factory.Save();

			var email = new EmailDef()
			{
				ContentType = EmailContentTypes.PlainText,
				FromAddress = Env.Registry.SMTPDefaultDoNotReplyEmailAddress,
				Subject = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
				Body = "Test Body"
			};
			AssertEquals(0, email.Attachments.Count);

			((IOutgoingMailManager)MailCreator).Create(Factory, email,
						SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.Value,
						GroupSourceLocator.GetFromRegistryItem(SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup));

			AssertEquals(1, email.Attachments.Count);

			var newMail = Factory.Load<MailItem>(MailCreator.LastCreatedMailGuid);
			AssertEquals(1, newMail.MailAttachments.Count);
			var attachmentName = newMail.MailAttachments[0].MA_FileName;
			AssertEquals("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012.txt", attachmentName);
		}

		public void TestNewMailItemForAttachment()
		{
			var email = new EmailDef
			{
				ContentType = EmailContentTypes.PlainText,
				FromAddress = Env.Registry.SMTPDefaultDoNotReplyEmailAddress,
				Subject = "Test Subject",
				Body = "Test Body"
			};

			var filename = new ZString('a', MailDBAttachmentsSchema.MA_FileName.MaxLength + 5);
			email.Attachments.Add(new AttachmentDef(filename, new byte[] { 0, 2, 3, 4, 5 }));

			var newMail = MailCreator.NewMailItem(Factory, email);
			var attachment = newMail.MailAttachments[0];
			AssertEquals(attachment.MA_FileName, filename.Left(attachment.MA_FileNameInfo.MaxLength));
		}

		public void TestHandleEmailCreationForNoRecipients()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GB1";
			group.GG_Desc = "BOB'S GROUP";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff.RemoveAll();
			postMasterGroup.Staff.Add(staff);
			Factory.Save();

			var email = new EmailDef();
			email.Subject = "TestSendEmailToNotification Subject";
			email.Body = "TestSendEmailToNotification Body";
			IOutgoingMailManager mailManager = MailCreator;
			var groupSourceLocator = GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup);
			MailCreator.EmailsCreated.Clear();
			var ex = AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.Create(Factory, email, Core.Constants.Groups.PostMastersGroupPK, groupSourceLocator));
			AssertStartsWith("", "Email (Subject: 'TestSendEmailToNotification Subject') This email could not be delivered to the requested group Notification -> Company Notification Group. Please add a member with an email address to this group or specify an email address on an existing member.", ex.Message);
			AssertEquals(0, MailCreator.EmailsCreated.Count);

			AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.CreateAndSaveToPostmasterGroup(email));
			AssertEquals(0, MailCreator.EmailsCreated.Count);

			AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, groupSourceLocator));
			AssertEquals(0, MailCreator.EmailsCreated.Count);

			AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.CreateAndSaveToCompanyNotificationGroup(email.Subject, email.Body));
			AssertEquals(0, MailCreator.EmailsCreated.Count);

			AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.CreateAndSaveWithCopyToCompanyNotificationGroup(email.Subject, email.Body, null, true));
			AssertEquals(0, MailCreator.EmailsCreated.Count);

			AssertExceptionThrown<EmailHasNoRecipientsException>("HELLO", () => mailManager.CreateAndSaveSimple(email.Subject, email.Body, ""));
			AssertEquals(0, MailCreator.EmailsCreated.Count);
		}

		public void TestCreateAndSaveToCompanyNotificationGroup()
		{
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			MailCreator.EmailsCreated.Clear();

			((IOutgoingMailManager)MailCreator).CreateAndSaveToCompanyNotificationGroup("TestSendEmailToNotification Subject", "TestSendEmailToNotification Body");

			AssertEquals("1 Email Sent", 1, MailCreator.EmailsCreated.Count);
			AssertEquals("1 Email Sent", "TestSendEmailToNotification Subject", MailCreator.EmailsCreated[0].Subject);
			AssertEquals("1 Email Sent", "TestSendEmailToNotification Body", MailCreator.EmailsCreated[0].Body);
			AssertEquals("Content type", EmailContentTypes.PlainText, MailCreator.EmailsCreated[0].ContentType);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		public void TestCreateAndSaveToCompanyNotificationGroupAsCCAndExtraRecipientAsTo()
		{
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			ArrayList list = new ArrayList();
			list.Add("test1@hotmail.com");
			list.Add("test2@hotmail.com");
			list.Add("test3@hotmail.com");

			string[] emailAddresses = (string[])list.ToArray(typeof(string));
			MailCreator.EmailsCreated.Clear();

			((IOutgoingMailManager)MailCreator).CreateAndSaveWithCopyToCompanyNotificationGroup("TestSendEmailToNotification Subject", "TestSendEmailToNotification Body", emailAddresses);

			AssertEquals("1 Email Sent", 1, MailCreator.EmailsCreated.Count);
			AssertEquals("1 Email Sent", "TestSendEmailToNotification Subject", MailCreator.EmailsCreated[0].Subject);
			AssertEquals("1 Email Sent", "TestSendEmailToNotification Body", MailCreator.EmailsCreated[0].Body);
			AssertEquals("3 ToRecipients", 3, MailCreator.EmailsCreated[0].Recipients.Count);
			AssertEquals("1 CCRecipients", 1, MailCreator.EmailsCreated[0].CCRecipients.Count);
			AssertEquals("test1@hotmail.com", MailCreator.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("test2@hotmail.com", MailCreator.EmailsCreated[0].Recipients[1].Email);
			AssertEquals("test3@hotmail.com", MailCreator.EmailsCreated[0].Recipients[2].Email);
			AssertEquals("a@b.c", MailCreator.EmailsCreated[0].CCRecipients[0].Email);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		public void TestArgumentNullException()
		{
			int numberOfConstructors = 5;
			int i = 0;
			try
			{
				do
				{
					MailCreator.EmailsCreated.Clear();
					switch (i)
					{
						case 0:
							((IOutgoingMailManager)MailCreator).Create(Factory, null);
							break;
						case 1:
							((IOutgoingMailManager)MailCreator).Create(Factory, null, Guid.NewGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
							break;
						case 2:
							((IOutgoingMailManager)MailCreator).CreateAndSave(null);
							break;
						case 3:
							((IOutgoingMailManager)MailCreator).CreateAndSave(null, Guid.NewGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
							break;
						case 4:
							((IOutgoingMailManager)MailCreator).CreateAndSaveToPostmasterGroup(null);
							break;
					}
					Fail("Should throw exception");
				}
				while (i++ < numberOfConstructors);
			}
			catch (ArgumentNullException ex)
			{
				if (ex.Message.Contains("emailToSend") || ex.Message.Contains("emailToAddRecipients"))
				{
					Assert(true);
				}
				else
				{
					Fail("should throw proper exception");
				}
			}
			catch
			{
				Fail("should throw proper exception");
			}
		}

		public void TestCreateAndSave_FromDisplayName()
		{
			var email = new EmailDef() { Subject = "hi", ReplyTo = "a@a.com" };
			email.AddRecipientForUserCommunication("test@cargowise.com");
			email.FromAddress = "enterpriseproduction@cargowise.com";
			email.FromDisplayName = "eNovator Ltd (WBP, WSP, WTP)";

			var email2 = new EmailDef() { Subject = "a", ReplyTo = "a@a.com" };
			email2.AddRecipientForUserCommunication("test@cargowise.com");
			email2.FromAddress = "enterpriseproduction@cargowise.com";
			string allValidDisplayName = "09AZaz " + OutgoingMailCreator.ValidDisplayNameSpecials;
			email2.FromDisplayName = allValidDisplayName;

			var creator = (IOutgoingMailManager)(OutgoingMailCreator.Instance);
			creator.CreateAndSave(email, Factory);
			var mailItem = Factory.Load<MailItem>(OutgoingMailCreator.Instance.LastCreatedMailGuid);
			AssertEquals("\"eNovator Ltd (WBP, WSP, WTP)\" <enterpriseproduction@cargowise.com>", mailItem.MI_From);

			var msg = Enterprise.MailManager.ExternalMailInterface.MailKitMailBuilder.BuildMimeMessage(mailItem);
			AssertEquals(1, msg.From.Count);
			AssertEquals("enterpriseproduction@cargowise.com", msg.From.Mailboxes.First().Address);
			AssertEquals("eNovator Ltd (WBP, WSP, WTP)", msg.From.Mailboxes.First().Name);

			creator.CreateAndSave(email2, Factory);
			var mailItem2 = Factory.Load<MailItem>(OutgoingMailCreator.Instance.LastCreatedMailGuid);
			AssertEquals("no quotes needed", allValidDisplayName + " <enterpriseproduction@cargowise.com>", mailItem2.MI_From);
			var msg2 = Enterprise.MailManager.ExternalMailInterface.MailKitMailBuilder.BuildMimeMessage(mailItem2);
			AssertEquals(allValidDisplayName, msg2.From.Mailboxes.First().Name);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		#region Escalating to wider group

		GlbGroup GetEmptyGroup(string code)
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "I am empty";
			group.GG_Code = code;
			Factory.Save();
			return group;
		}

		EmailDef GetTestEmail(EmailContentTypes type)
		{
			EmailDef email = new EmailDef();
			email.ContentType = type;
			email.Body = "Hello world";
			email.Subject = "Test subject";
			return email;
		}

		void SetupPostMaster()
		{
			GlbStaff pm = Factory.NewWithValidTestData<GlbStaff>();
			pm.GS_EmailAddress = "pm@pm";
			GlbGroup pmg = Factory.Load<GlbGroup>(EnvProxy.Instance.Registry.PostMasterGroup);
			pm.Groups.Add(pmg);
			Factory.Save();
		}

		void MakeStaff(string email, bool isAdmin)
		{
			GlbStaff admin = Factory.NewWithValidTestData<GlbStaff>();
			admin.GS_EmailAddress = email;
			if (isAdmin)
			{
				admin.GS_IsController = true;
			}
			Factory.Save();
		}

		void ResetStaff()
		{
			GlbStaff[] admins = Factory.Load<GlbStaff>(new ZQuery());
			foreach (GlbStaff staff in admins)
			{
				staff.GS_EmailAddress = "";
			}
			Factory.Save();
		}

		string SendAndAssertEmailForEscalating(EmailContentTypes type, bool fromRegistry, string extension)
		{
			((IOutgoingMailManager)MailCreator).EmailsCreated.Clear();
			string code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
			GlbGroup group = GetEmptyGroup(code);
			IGroupSourceLocator locator = null;
			string location = string.Empty;
			if (fromRegistry)
			{
				locator = GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup);
				location = "Notification -> Company Notification Group";
			}
			else
			{
				locator = GroupSourceLocator.GetFromGroup(group);
				location = string.Format("'I am empty' (code: '{0}')", code);
			}
			((IOutgoingMailManager)MailCreator).CreateAndSave(GetTestEmail(type), group.PK.ToGuid(), locator);
			AssertEquals(1, ((IOutgoingMailManager)MailCreator).EmailsCreated.Count);
			AssertEquals(1, ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Attachments.Count);
			AssertEquals(extension, Path.GetExtension(((IOutgoingMailManager)MailCreator).EmailsCreated[0].Attachments[0].DisplayName));
			AssertEquals("Test subject", Path.GetFileNameWithoutExtension(((IOutgoingMailManager)MailCreator).EmailsCreated[0].Attachments[0].DisplayName));
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
			return location;
		}

		public void TestEscalateToPostMasters()
		{
			AssertEscalateToPostMasters(EmailContentTypes.PlainText, ".txt", true);
			AssertEscalateToPostMasters(EmailContentTypes.HTML, ".html", true);
			AssertEscalateToPostMasters(EmailContentTypes.Calendar, ".txt", true);
			AssertEscalateToPostMasters(EmailContentTypes.PlainText, ".txt", false);
			AssertEscalateToPostMasters(EmailContentTypes.HTML, ".html", false);
			AssertEscalateToPostMasters(EmailContentTypes.Calendar, ".txt", false);
		}

		void AssertEscalateToPostMasters(EmailContentTypes type, string extension, bool fromRegistry)
		{
			ResetStaff();
			SetupPostMaster();
			string location = SendAndAssertEmailForEscalating(type, fromRegistry, extension);
			AssertEquals("Post Master is setup, email should be escalated to him", "pm@pm", ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Recipients[0].Email);
			AssertMultilineASCIIEquals("Should contain explanation", string.Format(@"This email could not be delivered to the requested group {0}. Please add a member with an email address to this group or specify an email address on an existing member.

Original message attached.", location), ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Body);
		}

		public void TestEscalateToAdmin()
		{
			AssertEscalateToAdmin(EmailContentTypes.PlainText, ".txt", true);
			AssertEscalateToAdmin(EmailContentTypes.HTML, ".html", true);
			AssertEscalateToAdmin(EmailContentTypes.Calendar, ".txt", true);
			AssertEscalateToAdmin(EmailContentTypes.PlainText, ".txt", false);
			AssertEscalateToAdmin(EmailContentTypes.HTML, ".html", false);
			AssertEscalateToAdmin(EmailContentTypes.Calendar, ".txt", false);
		}

		void AssertEscalateToAdmin(EmailContentTypes type, string extenstion, bool fromRegistry)
		{
			ResetStaff();
			MakeStaff("adm@adm", true);
			string location = SendAndAssertEmailForEscalating(type, fromRegistry, extenstion);
			AssertEquals("PostMaster is not setup, next step - admin", "adm@adm", ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Recipients[0].Email);
			AssertMultilineASCIIEquals("Should contain explanation", string.Format(@"This email could not be delivered to the requested group {0}. Please add a member with an email address to this group or specify an email address on an existing member.
This email could not be delivered to the Post Masters Group as it is either empty or no members have an email address specified.

Original message attached.", location), ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Body);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		public void TestEscalateToAll()
		{
			AssertEscalateToAll(EmailContentTypes.PlainText, ".txt", true);
			AssertEscalateToAll(EmailContentTypes.HTML, ".html", true);
			AssertEscalateToAll(EmailContentTypes.Calendar, ".txt", true);
			AssertEscalateToAll(EmailContentTypes.PlainText, ".txt", false);
			AssertEscalateToAll(EmailContentTypes.HTML, ".html", false);
			AssertEscalateToAll(EmailContentTypes.Calendar, ".txt", false);
		}

		void AssertEscalateToAll(EmailContentTypes type, string extenstion, bool fromRegistry)
		{
			ResetStaff();
			MakeStaff("common@common", false);
			string location = SendAndAssertEmailForEscalating(type, fromRegistry, extenstion);
			AssertEquals("PostMaster and admin are not setup, next step - all staff", "common@common", ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Recipients[0].Email);
			AssertMultilineASCIIEquals("Should contain explanation", string.Format(@"This email could not be delivered to the requested group {0}. Please add a member with an email address to this group or specify an email address on an existing member.
This email could not be delivered to the Post Masters Group as it is either empty or no members have an email address specified.
This email could not be delivered to Admin as there is either no Admin (Is Controller flag on Staff form should be ticked) or none have an email address specified.

Original message attached.", location), ((IOutgoingMailManager)MailCreator).EmailsCreated[0].Body);
			AssertRecipientsHaveCorrectCommunicationType(((IOutgoingMailManager)MailCreator).EmailsCreated);
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestCantEscalate()
		{
			ResetStaff();
			((IOutgoingMailManager)MailCreator).EmailsCreated.Clear();
			((IOutgoingMailManager)MailCreator).CreateAndSave(GetTestEmail(EmailContentTypes.PlainText), GetEmptyGroup("MUHAHA").PK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			AssertEquals(0, ((IOutgoingMailManager)MailCreator).EmailsCreated.Count);
		}

		public void TestGroupSourceLocatorCantBeNull()
		{
			ResetStaff();
			SetupPostMaster();
			string code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
			GlbGroup group = GetEmptyGroup(code);
			AssertExceptionThrown(typeof(ArgumentNullException), () => ((IOutgoingMailManager)MailCreator).CreateAndSave(GetTestEmail(EmailContentTypes.PlainText), group.PK.ToGuid(), null));
		}

		void AssertRecipientsHaveCorrectCommunicationType(List<EmailDef> emails)
		{
			foreach (var email in emails)
			{
				AssertRecipientsHaveCorrectCommunicationType(email);
			}
		}

		void AssertRecipientsHaveCorrectCommunicationType(EmailDef email)
		{
			foreach (RecipientDef recipient in email.Recipients)
			{
				AssertEquals(ShouldRecipientBeForSystemCommunication, recipient.IsForSystemCommunication);
			}
		}

		#endregion

		#endregion

		#region Implementation

		class ReminderForTesting : ReminderBase
		{
			internal ReminderForTesting(string identifier, DateTimeKind dateTimeKind, ZDateTime from, ZDateTime to, string subject = "", string body = "", string htmlBody = "", ITimeZone timeZoneOverride = null)
				: base(identifier, dateTimeKind, from, to, subject, body, htmlBody, timeZoneOverride)
			{
			}

			internal ReminderForTesting(string identifier, DateTimeKind dateTimeKind, DateTime from, TimeSpan duration, string subject = "", string body = "", string htmlBody = "")
				: base(identifier, dateTimeKind, from, duration, subject, body, htmlBody)
			{
			}

			internal ReminderForTesting(string identifier)
				: base(identifier, DateTimeKind.Local, DateTime.Now, DateTime.Now, "", "", "")
			{
			}

			public override void CreateAppointment()
			{
				var creator = new VCalendarReminderCreator(this);
				creator.CreateAppointment();
			}

			protected override uint GenerateSequenceNumber()
			{
				return 123u;
			}
		}

		protected EmailDef GetTestEmailDef1()
		{
			const string TestSubject = "Subject";
			const string TestBody = "Body";
			const string TestTo1 = "nick@edi.com.au";
			const string TestTo2 = "scally@edi.com.au";

			EmailDef email = new EmailDef();
			email.Subject = TestSubject;
			email.Body = TestBody;

			email.Attachments.Add(new AttachmentDef("name", new byte[5] { 0, 2, 3, 4, 5 }));
			email.AddRecipientForUserCommunication(TestTo1);
			email.AddRecipientForUserCommunication(TestTo2);

			return email;
		}

		protected EmailDef GetTestEmailDef2()
		{
			const string TestSubject = "Subject";
			const string TestBody = "Body";
			const string TestTO = "to@edi.com.au";
			const string TestCC = "cc@edi.com.au";
			const string TestBCC = "bcc@edi.com.au";

			EmailDef email = new EmailDef();
			email.Subject = TestSubject;
			email.Body = TestBody;

			email.AddRecipientForUserCommunication(TestTO, RecipientDef.RecipientTypes.TO);
			email.AddRecipientForUserCommunication(TestCC, RecipientDef.RecipientTypes.CC);
			email.AddRecipientForUserCommunication(TestBCC, RecipientDef.RecipientTypes.BCC);
			return email;
		}

		protected abstract OutgoingBaseMailCreator<T> MailCreator { get; }

		protected abstract bool ShouldRecipientBeForSystemCommunication { get; }

		#endregion
	}
}
