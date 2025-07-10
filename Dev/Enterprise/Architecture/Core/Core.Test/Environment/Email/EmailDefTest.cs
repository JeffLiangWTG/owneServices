using System;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EmailDefTest : EmailTest
	{
		public void TestToStringForTesting()
		{
			EmailDef email = new EmailDef();
			AssertMultilineASCIIEquals("Empty Email", @"
To: 
Subject: 
---- Body Text ----
".Trim(), email.ToStringForTesting());

			email.AddRecipient("to_1@sample.com", RecipientDef.RecipientTypes.TO, false);
			email.AddRecipient("to_2@sample.com", RecipientDef.RecipientTypes.TO, false);
			email.AddRecipient("to_3@sample.com", RecipientDef.RecipientTypes.CC, false);
			email.AddRecipient("to_4@sample.com", RecipientDef.RecipientTypes.CC, false);
			email.AddRecipient("to_5@sample.com", RecipientDef.RecipientTypes.BCC, false);
			email.AddRecipient("to_6@sample.com", RecipientDef.RecipientTypes.BCC, false);
			email.Subject = "Let's talk about...";
			email.Body = "Your body.";

			AssertMultilineASCIIEquals("Full Email", @"
To: to_1@sample.com; to_2@sample.com
CC: to_3@sample.com; to_4@sample.com
BCC: to_5@sample.com; to_6@sample.com
Subject: Let's talk about...
---- Body Text ----
Your body.
".Trim(), email.ToStringForTesting());
		}

		public void TestFromEmailAddress()
		{
			Guid staffPK = Guid.NewGuid();
			InsertUser(staffPK, "test1@blah.com");
			EmailDef email = new EmailDef(staffPK);
			AssertEquals("test1@blah.com", email.FromAddress);

			staffPK = Guid.NewGuid();
			InsertUser(staffPK, "");
			EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress = "test2@blah.com";
			email = new EmailDef(staffPK);
			AssertEquals("test2@blah.com", email.FromAddress);

			((IRegistryItemInternals)EnvProxy.Instance.Registry.RawRegistry.SMTPDefaultReturnEmailAddress).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			email = new EmailDef(staffPK);
			AssertEquals(EnvProxy.Instance.Registry.MailboxEmailAddress, email.FromAddress);

			EnvProxy.Instance.Registry.MailboxDisplayName = "Demo Company";

			email = new EmailDef();
			email.FromAddress = "CargoWise <PleaseDoNotReply@cargowise.com>";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);
			AssertEquals("CargoWise", email.FromDisplayName);

			email = new EmailDef();
			email.FromAddress = "WiseTech Global (Australia) Pty Ltd <PleaseDoNotReply@cargowise.com>";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);
			AssertEquals("WiseTech Global (Australia) Pty Ltd", email.FromDisplayName);

			email = new EmailDef();
			email.FromAddress = "<PleaseDoNotReply@cargowise.com>";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);
			AssertEquals("Demo Company", email.FromDisplayName);

			email = new EmailDef();
			email.FromAddress = "PleaseDoNotReply@cargowise.com";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);
			AssertEquals("Demo Company", email.FromDisplayName);

			email = new EmailDef();
			email.FromAddress = "CargoWise < PleaseDoNotReply@cargowise.com >";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);
			AssertEquals("CargoWise", email.FromDisplayName);

			email = new EmailDef();
			email.FromAddress = " PleaseDoNotReply@cargowise.com ";
			AssertEquals("PleaseDoNotReply@cargowise.com", email.FromAddress);

			staffPK = Guid.NewGuid();
			InsertUser(staffPK, "test3@blah.com ");
			email = new EmailDef(staffPK);
			AssertEquals("test3@blah.com", email.FromAddress);
		}

		public void TestCampaignItemID()
		{
			var email = new EmailDefForTest();
			var guid = ZGuid.NewZGuid();
			email.BusinessEntityIDForTesting = guid;
			AssertEquals(guid, email.BusinessEntityID);
		}

		public void TestSenderStaffPK()
		{
			var staffPK = Guid.NewGuid();
			InsertUser(staffPK, "example@example.com");
			var email = new EmailDef(staffPK);
			AssertEquals(staffPK, email.SenderStaffPK);

			email = new EmailDef();
			AssertEquals(EmailDef.CurrentUserPK, email.SenderStaffPK);
		}

		public void TestBusinessEntityTableCode()
		{
			var email = new EmailDefForTest();
			email.BusinessEntityTableCodeForTesting = GlbCompanyCampaignSchema.Constants.Prefix;
			AssertEquals(GlbCompanyCampaignSchema.Constants.Prefix, email.BusinessEntityTableCode);
		}

		public void TestBusinessEntityJobNumber()
		{
			var email = new EmailDefForTest();
			email.BusinessEntityJobNumberForTesting = "S00000001";
			AssertEquals("S00000001", email.BusinessEntityJobNumber);
		}

		public void TestListUnsubscribe()
		{
			var email = new EmailDef();
			email.ListUnsubscribe = "<http://www.cw1.com/unsubscribe?1234>";
			AssertEquals("<http://www.cw1.com/unsubscribe?1234>", email.ListUnsubscribe);
		}

		public void TestBusinessEntityInfo()
		{
			var email = new EmailDefForTest();
			var dummyObj = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			email.SetupBusinessEntityInfo(null);
			AssertEquals("", email.BusinessEntityTableCode);
			AssertEquals(ZGuid.Empty, email.BusinessEntityIDForTesting);

			email.SetupBusinessEntityInfo(dummyObj);
			AssertEquals(dummyObj.TablePrefix, email.BusinessEntityTableCode);
			AssertEquals(dummyObj.PK, email.BusinessEntityIDForTesting);

			email.SetupBusinessEntityInfo(dummyObj.PK, dummyObj.TablePrefix, dummyObj.HumanReadableName);
			AssertEquals(dummyObj.TablePrefix, email.BusinessEntityTableCode);
			AssertEquals(dummyObj.PK, email.BusinessEntityIDForTesting);
			AssertEquals(dummyObj.HumanReadableName, email.BusinessEntityJobNumberForTesting);
		}

		public void TestReplyTo()
		{
			InsertUser(Guid.NewGuid(), "example@example.com");
			using (EnvProxy.Instance.SetTemporaryUserContext("Test1", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("ReplyTo should be blank by default - replies will go to the From address if ReplyTo is blank", null, new EmailDef().ReplyTo);
			}
		}

		public void TestContentType()
		{
			EmailDef email = new EmailDef();
			AssertEquals("default Content Type", EmailContentTypes.PlainText.ContentTypeCode, email.ContentType.ContentTypeCode);

			email.ContentType = EmailContentTypes.HTML;
			AssertEquals("Content Type changed", EmailContentTypes.HTML.ContentTypeCode, email.ContentType.ContentTypeCode);
		}

		public void TestPriority()
		{
			EmailDef email = new EmailDef();
			AssertEquals("Priority", EmailDef.PriorityFlag.Medium, email.Priority);

			email.Priority = EmailDef.PriorityFlag.High;
			AssertEquals("Priority", EmailDef.PriorityFlag.High, email.Priority);
		}

		public void TestQueueWithLowPriority()
		{
			EmailDef email = new EmailDef();
			AssertEquals("QueueWithLowPriority", false, email.QueueWithLowPriority);

			email.QueueWithLowPriority = true;
			AssertEquals("QueueWithLowPriority", true, email.QueueWithLowPriority);
		}

		public void TestFooterTextAppendedToBodyText()
		{
			EmailDef email = new EmailDef();
			AssertEquals(string.Empty, email.FooterText);
			email.FooterText = "HELLO";
			Assert(email.Body.EndsWith("HELLO"));
		}

		public void TestBody()
		{
			EmailDef email = new EmailDef();
			email.Body = "BODY";
			email.FooterText = "FOOTER";
			AssertEquals("BODY\r\n\r\nFOOTER", email.Body);
		}

		public void TestAddGroupOfRecipients()
		{
			Guid notificationGroup = CreateGroupOfOne();
			EmailDef email = new EmailDef();
			AssertEquals(string.Empty, email.FooterText);
			AssertEquals(0, email.Recipients.Count);
			email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(notificationGroup, (NoResString)"Item Location");
			AssertNotNull("FooterText", email.FooterText);
			Assert("FooterTextContainsItemLocation", email.FooterText.IndexOf("Item Location") != -1);
			Assert("Recipients", email.Recipients.Count == 1);
		}

		public void TestAddGroupOfRecipientsWhenNobodyIsInGroup()
		{
			Guid staffPK = Guid.NewGuid();
			InsertUser(staffPK, "address@example.com");
			InsertGroupLink(staffPK, EmailDef.GetGlbGroupAllPK());
			EmailDef email = new EmailDef();
			AssertEquals(string.Empty, email.FooterText);
			AssertEquals(0, email.Recipients.Count);
			email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Guid.Empty, (NoResString)"Item Location");
			AssertNotNull("FooterText", email.FooterText);
			Assert("FooterTextContainsItemLocation", email.FooterText.IndexOf("Item Location") != -1);
			Assert("FooterTextContainsExplainationOfWhyAllUsersWereSentTo", email.FooterText.IndexOf("Since there are no valid email addresses set up in this group this email has been sent to all users.") != -1);
			bool foundTestEmail = false;
			foreach (string recipient in email.Recipients.ToStringCollection())
			{
				if (recipient.IndexOf("address@example.com") != -1)
				{
					foundTestEmail = true;
					break;
				}
			}
			Assert("Recipients include Test Email", foundTestEmail);
		}

		public void TestAddRecipientsAddsCorrectly()
		{
			EmailDef email = new EmailDef();

			email.AddRecipientForUserCommunication("aaa");
			AssertEquals(1, email.RecipientsCore.Count);
			AssertEquals(1, email.Recipients.Count);

			email.AddRecipientForUserCommunication("bbb");
			AssertEquals(2, email.RecipientsCore.Count);
			AssertEquals(2, email.Recipients.Count);
		}

		public void TestAddUserRecipients()
		{
			EmailDef email = new EmailDef();

			email.AddRecipientForUserCommunication("adr1");
			AssertRecipient(email, new string[] { "adr1" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication("adr2", RecipientDef.RecipientTypes.TO);
			AssertRecipient(email, new string[] { "adr2" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication("adr3", RecipientDef.RecipientTypes.CC);
			AssertRecipient(email, new string[] { "adr3" }, RecipientDef.RecipientTypes.CC, false);

			email.AddRecipientForUserCommunication("adr4", RecipientDef.RecipientTypes.BCC);
			AssertRecipient(email, new string[] { "adr4" }, RecipientDef.RecipientTypes.BCC, false);

			email.AddRecipientForUserCommunication(new string[] { "adr5", "adr6" });
			AssertRecipient(email, new string[] { "adr5", "adr6" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication(new string[] { "adr7", "adr8" }, RecipientDef.RecipientTypes.TO);
			AssertRecipient(email, new string[] { "adr7", "adr8" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication(new string[] { "adr9", "adr10" }, RecipientDef.RecipientTypes.CC);
			AssertRecipient(email, new string[] { "adr9", "adr10" }, RecipientDef.RecipientTypes.CC, false);

			email.AddRecipientForUserCommunication(new string[] { "adr11", "adr12" }, RecipientDef.RecipientTypes.BCC);
			AssertRecipient(email, new string[] { "adr11", "adr12" }, RecipientDef.RecipientTypes.BCC, false);

			email.AddRecipientForUserCommunication(GetNewStringCollection(new string[] { "adr13", "adr14" }));
			AssertRecipient(email, new string[] { "adr13", "adr14" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication(GetNewStringCollection(new string[] { "adr15", "adr16" }), RecipientDef.RecipientTypes.TO);
			AssertRecipient(email, new string[] { "adr15", "adr16" }, RecipientDef.RecipientTypes.TO, false);

			email.AddRecipientForUserCommunication(GetNewStringCollection(new string[] { "adr17", "adr18" }), RecipientDef.RecipientTypes.CC);
			AssertRecipient(email, new string[] { "adr17", "adr18" }, RecipientDef.RecipientTypes.CC, false);

			email.AddRecipientForUserCommunication(GetNewStringCollection(new string[] { "adr19", "adr20" }), RecipientDef.RecipientTypes.BCC);
			AssertRecipient(email, new string[] { "adr19", "adr20" }, RecipientDef.RecipientTypes.BCC, false);
		}

		public void TestDoNotAddDuplicateRecipients()
		{
			var email = new EmailDef();

			email.AddRecipientForUserCommunication("adr1");
			AssertEquals(1, email.Recipients.Count);

			email.AddRecipientForUserCommunication("adr2");
			AssertEquals(2, email.Recipients.Count);

			email.AddRecipientForUserCommunication("adr2");
			AssertEquals("Should not add duplicate", 2, email.Recipients.Count);

			email.AddRecipientForUserCommunication("adr3");
			AssertEquals(3, email.Recipients.Count);
		}

		public void TestDoNotAddDuplicateRecipientsInSameGroup()
		{
			EmailDef email = new EmailDef();

			email.AddRecipientForUserCommunication("adr1", RecipientDef.RecipientTypes.TO);
			AssertEquals(1, email.Recipients.Count);

			email.AddRecipientForUserCommunication("adr1", RecipientDef.RecipientTypes.CC);
			AssertEquals("Should not add duplicate", 0, email.CCRecipients.Count);

			email.AddRecipientForUserCommunication("adr1", RecipientDef.RecipientTypes.BCC);
			AssertEquals("Should not add duplicate", 0, email.BCCRecipients.Count);

			email.AddRecipientForUserCommunication("adr2", RecipientDef.RecipientTypes.CC);
			AssertEquals(1, email.CCRecipients.Count);

			email.AddRecipientForUserCommunication("adr2", RecipientDef.RecipientTypes.TO);
			AssertEquals("Should not add duplicate", 1, email.Recipients.Count);

			email.AddRecipientForUserCommunication("adr2", RecipientDef.RecipientTypes.BCC);
			AssertEquals("Should not add duplicate", 0, email.BCCRecipients.Count);
		}

		public void TestAddSystemRecipients()
		{
			EmailDef email = new EmailDef();

			email.AddRecipientForSystemCommunication("adr1");
			AssertRecipient(email, new string[] { "adr1" }, RecipientDef.RecipientTypes.TO, true);

			email.AddRecipientForSystemCommunication("adr2", RecipientDef.RecipientTypes.TO);
			AssertRecipient(email, new string[] { "adr2" }, RecipientDef.RecipientTypes.TO, true);

			email.AddRecipientForSystemCommunication("adr3", RecipientDef.RecipientTypes.CC);
			AssertRecipient(email, new string[] { "adr3" }, RecipientDef.RecipientTypes.CC, true);

			email.AddRecipientForSystemCommunication("adr4", RecipientDef.RecipientTypes.BCC);
			AssertRecipient(email, new string[] { "adr4" }, RecipientDef.RecipientTypes.BCC, true);

			email.AddRecipientForSystemCommunication(new string[] { "adr5", "adr6" });
			AssertRecipient(email, new string[] { "adr5", "adr6" }, RecipientDef.RecipientTypes.TO, true);

			email.AddRecipientForSystemCommunication(new string[] { "adr7", "adr8" }, RecipientDef.RecipientTypes.TO);
			AssertRecipient(email, new string[] { "adr7", "adr8" }, RecipientDef.RecipientTypes.TO, true);

			email.AddRecipientForSystemCommunication(new string[] { "adr9", "adr10" }, RecipientDef.RecipientTypes.CC);
			AssertRecipient(email, new string[] { "adr9", "adr10" }, RecipientDef.RecipientTypes.CC, true);

			email.AddRecipientForSystemCommunication(new string[] { "adr11", "adr12" }, RecipientDef.RecipientTypes.BCC);
			AssertRecipient(email, new string[] { "adr11", "adr12" }, RecipientDef.RecipientTypes.BCC, true);
		}

		public void TestGetGlbGroupAllPK()
		{
			// If the group with the default PK for ALL exists, change its code. This will pick up bugs due to people hard-coding the default PK.
			using (DbCommand command = Db.Connection.Command("UPDATE dbo.GlbGroup SET GG_Code = 'Dummy', GG_SystemLastEditUser = 'E', GG_SystemLastEditTimeUtc = GetDate() WHERE GG_PK = @pk")) // Enterprise.ZArchitecture.Core project hasn't access to Business Objects
			{
				command.AddParameterBasedOnDbColumn("@pk", new Guid("94755E71-A87A-4034-8DFA-785773A49607"), GlbGroupSchema.PK);
				command.ExecuteNonQuery();
			}

			// If there is already an ALL group, rename it.
			using (DbCommand command = Db.Connection.Command("UPDATE dbo.GlbGroup SET GG_Code = 'TEST_' + GG_CODE, GG_SystemLastEditUser = 'E', GG_SystemLastEditTimeUtc = GetDate() WHERE GG_Code = @code")) // Enterprise.ZArchitecture.Core project hasn't access to Business Objects
			{
				command.AddParameterBasedOnDbColumn("@code", Constants.Groups.ALL, GlbGroupSchema.GG_Code);
				command.ExecuteNonQuery();
			}

			Guid testPK = Guid.NewGuid();
			// Insert test ALL group.
			using (DbCommand command = Db.Connection.Command("INSERT INTO dbo.GlbGroup (GG_PK, GG_Code, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('" + testPK.ToString() + "', '" + Constants.Groups.ALL + "', GetUtcDate(), '~BP', GetUtcDate(), '~BP')")) // Enterprise.ZArchitecture.Core project hasn't access to Business Objects
			{
				command.ExecuteNonQuery();
			}

			AssertEquals("GetGlbGroupAllPK()", testPK, EmailDef.GetGlbGroupAllPK());
		}

		public void TestSaveAsEml()
		{
			EmailDef email = new EmailDef();

			email.FromDisplayName = "fromName";
			email.FromAddress = "from@address.com";

			email.Body = "<BODY_A></BODY_A>";
			email.FooterText = "<FOOTER_A></FOOTER_A>";

			string filename;
			string fileContent;
			using (var tempFile = email.SaveAsEml())
			{
				filename = tempFile.Filename;
				fileContent = File.ReadAllText(tempFile.Filename);
			}

			Assert("eml file must end with .eml extension", filename.EndsWith(".eml"));
			AssertContains("From: fromName <from@address.com>", fileContent);
			AssertContains("<BODY_A></BODY_A>\r\n\r\n<FOOTER_A></FOOTER_A>", fileContent);
		}

		StringCollection GetNewStringCollection(string[] addresses)
		{
			StringCollection col = new StringCollection();
			foreach (string adr in addresses)
			{
				col.Add(adr);
			}
			return col;
		}

		void AssertRecipient(EmailDef email, string[] addresses, RecipientDef.RecipientTypes type, bool isSystem)
		{
			RecipientDefReadonlyCollection col;
			switch (type)
			{
				case RecipientDef.RecipientTypes.TO:
					col = email.Recipients;
					break;
				case RecipientDef.RecipientTypes.CC:
					col = email.CCRecipients;
					break;
				case RecipientDef.RecipientTypes.BCC:
					col = email.BCCRecipients;
					break;
				default:
					return;
			}
			foreach (string adr in addresses)
			{
				RecipientDef recipient = GetRecipientByEmail(adr, col);
				AssertNotNull(recipient);
				AssertEquals(isSystem, recipient.IsForSystemCommunication);
			}
		}

		RecipientDef GetRecipientByEmail(string address, RecipientDefReadonlyCollection col)
		{
			foreach (RecipientDef recipient in col)
			{
				if (recipient.Email == address)
				{
					return recipient;
				}
			}
			return null;
		}

		#region Implementation

		Guid CreateGroupOfOne()
		{
			Guid userPK = Guid.NewGuid();
			InsertUser(userPK, "Notification@edi.com.au");

			Guid groupPK = Guid.NewGuid();
			InsertGroup(groupPK);

			InsertGroupLink(userPK, groupPK);
			return groupPK;
		}

		class EmailDefForTest : EmailDef
		{
			public ZGuid BusinessEntityIDForTesting
			{
				get { return businessEntityID; }
				set { businessEntityID = value; }
			}

			public string BusinessEntityTableCodeForTesting
			{
				get { return businessEntityTableCode; }
				set { businessEntityTableCode = value; }
			}

			public string BusinessEntityJobNumberForTesting
			{
				get { return businessEntityJobNumber; }
				set { businessEntityJobNumber = value; }
			}
		}

		#endregion
	}
}
