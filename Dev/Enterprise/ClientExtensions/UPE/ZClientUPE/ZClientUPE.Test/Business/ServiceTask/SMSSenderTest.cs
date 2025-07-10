using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	class SMSSenderTest : TestCaseWithFactory
	{
		public void TestSendSMS()
		{
			Recipient.GS_MobilePhone = "0421 944-317";
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.GG_Code);
			UPEDataRegistry.Instance.SMSEmailAddressSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".fwd@smsprovider.com.au");
			NotificationGroup.Factory.Save();
			SMSSender.SendSMS("SMS Message");
			EmailDef sMSEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Sent to correct email address", "0421944317.fwd@smsprovider.com.au", sMSEmail.Recipients[0]);
			AssertEquals("Subject should be empty to avoid unnecessary SMS text", "", sMSEmail.Subject);
			AssertEquals("SMS message content should be in the body", "SMS Message", sMSEmail.Body);
		}

		public void TestCheckEnvironmentValid_RequiresSMSNotificationGroup()
		{
			GlbGroup groupWithNoStaff = Factory.New<GlbGroup>();
			groupWithNoStaff.GG_Code = "_NS";
			GlbGroup groupWithNoMobileNumbers = Factory.New<GlbGroup>();
			groupWithNoMobileNumbers.GG_Code = "_NM";
			GlbStaff staffWithNoMobileNumber = groupWithNoMobileNumbers.Staff.AddNew();
			staffWithNoMobileNumber.GS_LoginName = "login";
			staffWithNoMobileNumber.GS_MobilePhone = "";
			staffWithNoMobileNumber.GS_Code = "_NM";
			NotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.SMSEmailAddressSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".fwd@smsprovider.com.au");
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.GG_Code);
			fSMSSender = null;
			AssertEquals("When the notification group exists and has staff with mobile numbers", true, SMSSender.CheckEnvironmentValid());
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "*X*");
			fSMSSender = null;
			AssertEquals("When the notification group doesnt exist", false, SMSSender.CheckEnvironmentValid());
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupWithNoStaff.GG_Code);
			fSMSSender = null;
			AssertEquals("When the notification group has no staff", false, SMSSender.CheckEnvironmentValid());
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupWithNoMobileNumbers.GG_Code);
			fSMSSender = null;
			AssertEquals("When the notification group has no staff with mobile numbers", false, SMSSender.CheckEnvironmentValid());
		}

		public void TestCheckEnvironmentValid_RequiresSMSEmailAddressSuffix()
		{
			NotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.GG_Code);
			UPEDataRegistry.Instance.SMSEmailAddressSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".fwd@smsprovider.com.au");
			AssertEquals("When the suffix is configured", true, SMSSender.CheckEnvironmentValid());
			UPEDataRegistry.Instance.SMSEmailAddressSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("When the suffix is not configured", false, SMSSender.CheckEnvironmentValid());
		}

		#region Implementation
		SMSSender SMSSender
		{
			get
			{
				if (fSMSSender == null)
				{
					fSMSSender = new SMSSender(Notifications);
				}

				return fSMSSender;
			}
		}

		SMSSender fSMSSender;
		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}

				return fNotifications;
			}
		}

		NotificationBuffer fNotifications;
		GlbGroup NotificationGroup
		{
			get
			{
				if (fNotificationGroup == null)
				{
					CreateNotificationGroupAndRecipient();
				}

				return fNotificationGroup;
			}
		}

		GlbGroup fNotificationGroup;
		GlbStaff Recipient
		{
			get
			{
				if (fRecipient == null)
				{
					CreateNotificationGroupAndRecipient();
				}

				return fRecipient;
			}
		}

		GlbStaff fRecipient;
		void CreateNotificationGroupAndRecipient()
		{
			fNotificationGroup = Factory.New<GlbGroup>();
			fNotificationGroup.GG_Code = "NGP";
			fRecipient = fNotificationGroup.Staff.AddNew();
			fRecipient.GS_MobilePhone = "0421944317";
			fRecipient.GS_Code = "NSF";
		}
		#endregion
	}
}
