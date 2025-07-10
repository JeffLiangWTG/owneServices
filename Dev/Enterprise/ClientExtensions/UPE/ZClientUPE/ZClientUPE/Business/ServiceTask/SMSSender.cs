using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.ServiceTask
{
	public class SMSSender
	{
		public SMSSender(INotifications notifications)
		{
			this.Notifications = notifications;
		}

		public void SendSMS(string message)
		{
			EmailDef email = new EmailDef();
			email.Body = message;
			foreach (string mobile in SMSRecipientMobileNumbers)
			{
				email.AddRecipientForUserCommunication(mobile + UPEDataRegistry.Instance.SMSEmailAddressSuffix.Value);
			}
			if (email.Recipients.Count > 0)
			{
				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		public bool CheckEnvironmentValid()
		{
			bool result = true;
			if (string.IsNullOrEmpty(UPEDataRegistry.Instance.SMSEmailAddressSuffix.Value))
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "You must set the '" + UPEDataRegistry.Instance.SMSEmailAddressSuffix.Caption + "' in the registry."));
				result = false;
			}
			if (SMSRecipientMobileNumbers.Length == 0)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "You must set the '" + UPEDataRegistry.Instance.SMSNotificationGroup.Caption + "' in the registry with a staff group that has at least 1 valid mobile phone number."));
				result = false;
			}
			return result;
		}

		#region SMSRecipientMobileNumbers

		string[] SMSRecipientMobileNumbers
		{
			get
			{
				if (fLastDateRetrieved.IsEmpty || (ZDateTime.Now - fLastDateRetrieved).TotalMinutes > 20)
				{
					List<string> list = new List<string>();
					GlbGroup notificationGroup = LoadNotificationGroupInNewFactory();
					if (notificationGroup != null)
					{
						foreach (GlbStaff staff in notificationGroup.Staff)
						{
							ZString mobile = ParseMobilePhoneNo(staff.GS_MobilePhone);
							if (!mobile.IsEmpty && !mobile.Contains("+", StringComparison.OrdinalIgnoreCase))
							{
								list.Add(mobile);
							}
						}
					}
					fSMSRecipientMobileNumbers = list.ToArray();
					fLastDateRetrieved = ZDateTime.Now;
				}
				return fSMSRecipientMobileNumbers;
			}
		}
		string[] fSMSRecipientMobileNumbers;
		ZDateTime fLastDateRetrieved = ZDateTime.Empty;

		string ParseMobilePhoneNo(string mobilePhoneNo)
		{
			StringBuilder result = new StringBuilder(mobilePhoneNo);
			for (int i = mobilePhoneNo.Length - 1; i >= 0; i--)
			{
				if (char.IsWhiteSpace(mobilePhoneNo[i]) || mobilePhoneNo[i] == '-')
				{
					result.Remove(i, 1);
				}
			}
			return result.ToString();
		}

		GlbGroup LoadNotificationGroupInNewFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup result = null;
			if (!String.IsNullOrEmpty(UPEDataRegistry.Instance.SMSNotificationGroup.Value))
			{
				result = factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, UPEDataRegistry.Instance.SMSNotificationGroup.Value);
			}
			return result;
		}

		#endregion

		readonly INotifications Notifications;
	}
}
