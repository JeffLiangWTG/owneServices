using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	public class EDIDatabaseEmailManagementTest : DatabaseEmailManagementTest
	{
		public void TestTruncateBodyOutgoingUpgradeEmailOlderThan()
		{
			int numberApplied = DatabaseEmailManagement.Create().TruncateBodyOutgoingUpgradeEmailOlderThan(3);
			AssertEquals("NumberApplied", 1, numberApplied);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			MailItem newItem = secondFactory.Load<MailItem>(NewQueuedWithAckItem.PK);
			MailItem oldItem = secondFactory.Load<MailItem>(OldQueuedWithAckItem.PK);
			AssertNotNull(oldItem);
			AssertNotNull(newItem);
			AssertEquals("Body Truncated", FullUpgradeBody.Substring(0, 69), oldItem.MI_Body);
			AssertEquals("Body Unchanged", FullUpgradeBody, newItem.MI_Body);
		}

		public void TestStopUnsuccessfulUpgradeSending()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "~TSTAAA1";
			org1.OH_FullName = "Test Org #1";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "~TSTAAA2";
			org2.OH_FullName = "Test Org #2";
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "~test1";
			staff.GS_Code = "~t1";
			staff.GS_EmailAddress = "test1@abcde.aaa";
			Factory.Save();
			ZGuid upgrade1 = GetNewUpgradesToClient(staff, org1);
			ZGuid upgrade2 = GetNewUpgradesToClient(staff, org2);
			ZGuid upgrade3 = GetNewUpgradesToClient(staff, org1);
			MailItem failed = GetNewUpgradeMailItem("Package20041231_135600_1_2_3_4.edp", new string[] { "Deliverance Update <dummy@recipient.com>", "other@edi.com.au" }, new ZGuid[] { upgrade1, upgrade2 });
			AssertEquals("Two Recipients", 2, failed.MailRecipients.Count);
			failed.MailRecipients[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			failed.MailRecipients[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			MailItem item1 = GetNewUpgradeMailItem("Package20041231_135600_1_2_3_4.edp", new string[] { "Deliverance Update <dummy@recipient.com>", "other@edi.com.au" }, new ZGuid[] { upgrade1, upgrade2 });
			AssertEquals("Two Recipients", 2, item1.MailRecipients.Count);
			AssertEquals("Recipient Ok", 0, (int)item1.MailRecipients[0].MR_AckAttempt);
			MailItem item2 = GetNewUpgradeMailItem("Package20041231_135600_1_2_3_4.edp", new string[] { "Deliverance Update <dummy@recipient.com>", "other@edi.com.au" }, new ZGuid[] { upgrade1, upgrade2 });
			AssertEquals("Two Recipients", 2, item1.MailRecipients.Count);
			AssertEquals("Recipient Ok", 0, (int)item2.MailRecipients[0].MR_AckAttempt);
			MailItem item3 = GetNewUpgradeMailItem("Package20041231_135600_1_2_3_4.edp", new string[] { "other@edi.com.au" }, new ZGuid[] { upgrade2 });
			MailItem stopped1 = GetNewUpgradeMailItem("Package20051231_135600_1_2_3_4.edp", new string[] { "Deliverance Update <dummy@recipient.com>" }, new ZGuid[] { upgrade3 });
			stopped1.MailRecipients[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			stopped1.MailRecipients[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 10);
			MailItem stopped2 = GetNewUpgradeMailItem("Package20051231_135600_1_2_3_4.edp", new string[] { "Deliverance Update <dummy@recipient.com>" }, new ZGuid[] { upgrade3 });
			stopped2.MailRecipients[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts + 1;
			Factory.Save();
			DatabaseEmailManagement.Create().StopUnsuccessfulUpgradeSending();
			failed.MailRecipients[0].Reload();
			item1.MailRecipients[0].Reload();
			item2.MailRecipients[0].Reload();
			AssertEquals("First Recipient Blocked", MailAcknowledgement.MaxAttempts + 1, (int)failed.MailRecipients[0].MR_AckAttempt);
			AssertEquals("First Recipient Blocked", MailAcknowledgement.MaxAttempts + 1, (int)item1.MailRecipients[0].MR_AckAttempt);
			AssertEquals("First Recipient Blocked", MailAcknowledgement.MaxAttempts + 1, (int)item2.MailRecipients[0].MR_AckAttempt);
			AssertEquals("Second Recipient Not Blocked", 0, (int)item1.MailRecipients[1].MR_AckAttempt);
			AssertEquals("Second Recipient Not Blocked", 0, (int)item2.MailRecipients[1].MR_AckAttempt);
			AssertEquals("Recipient Not Blocked", 0, (int)item3.MailRecipients[0].MR_AckAttempt);
			MailItem[] items = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "Upgrade Delivery Failure"));
			AssertEquals(1, items.Length);
			AssertEquals("FromAddress", EnvProxy.Instance.CurrentCompany.Name + " <Default@edi.com.au>", items[0].MI_From);
			AssertEquals("Subject", "Upgrade Delivery Failure", items[0].MI_Subject);
			AssertEquals("One Recipient", 1, items[0].MailRecipients.Count);
			AssertEquals("Recipient", "test1@abcde.aaa", items[0].MailRecipients[0].EmailAddress);
			AssertEquals("Body", @"Dear User.

The upgrade package Package20041231_135600_1_2_3_4.edp which you sent to the Client ~TSTAAA1 Test Org #1 to the following email address <dummy@recipient.com> could not be delivered.
Please try to identify and fix the reason of the failure before sending any upgrades to this address", items[0].MI_Body.ToString());
		}

		#region Implementation
		ZGuid GetNewUpgradesToClient(GlbStaff staff, OrgHeader org)
		{
			ZGuid upgradesToClientPK = ZGuid.NewZGuid();
			string sqlText = String.Format("insert into {0} ({1}, {3}, {5}) values ('{2}', '{4}', '{6}')", UpgradesToClientSchema.Constants.TableName, UpgradesToClientSchema.PK.Name, upgradesToClientPK, UpgradesToClientSchema.L1_GS_NKStaffCode.Name, staff.GS_Code, UpgradesToClientSchema.L1_OH.Name, org.PK);
			Db.Connection.ExecuteNonQuery(sqlText);
			return upgradesToClientPK;
		}

		MailItem GetNewUpgradeMailItem(string subject, string[] recipients, ZGuid[] upgradesToClients)
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.QueuedWithAck;
			result.MI_Direction = MailDirection.Transmit;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.MI_Subject = subject;
			for (int i = 0; i < recipients.Length; i++)
			{
				var recipient = (MailRecipient)result.AddRecipientForUserCommunication(recipients[i], MailRecipient.RecipientTypes.TO);
				var upgradeLink = Factory.New<ClientMailRecipient>();
				upgradeLink.MRX_MR = recipient.PK;
				upgradeLink.MRX_L1 = upgradesToClients[i];
			}

			return result;
		}
		#endregion
	}
}
