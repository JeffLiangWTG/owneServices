using System;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.DocumentImaging
{
	public class UPEDocumentImagingNotifications : INotifications
	{
		public UPEDocumentImagingNotifications()
			: this(null)
		{
		}

		public UPEDocumentImagingNotifications(INotifications inner)
		{
			this.Inner = inner;
		}

		public void Flush()
		{
			ZString newNotificationsString = UPEDataRegistry.Instance.DocumentImagingNotificationsCache.Value + InMemoryNotifications.ToString();
			UPEDataRegistry.Instance.DocumentImagingNotificationsCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newNotificationsString.ToString());
			InMemoryNotifications = new StringBuilder();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public void SendEmailIfRequired()
		{
			Flush();
			if (!string.IsNullOrEmpty(UPEDataRegistry.Instance.DocumentImagingNotificationsCache.Value))
			{
				EmailDef email = new EmailDef();
				email.Subject = "CargoWise One Document Imaging Notifications";

				string bodyHeading = "CargoWise One Document Imaging Notifications; Generated " + ZDateTime.Now.ToString();
				string bodyFooter = "CargoWise One Document Imaging";
				email.Body = bodyHeading + "\r\n\r\n" + UPEDataRegistry.Instance.DocumentImagingNotificationsCache.Value + "\r\n" + bodyFooter + "\r\n";
				Env.OutgoingMailManager.CreateAndSave(email, UPEDataRegistry.Instance.DocumentImagingNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(UPEDataRegistry.Instance.DocumentImagingNotificationGroup));

				UPEDataRegistry.Instance.DocumentImagingNotificationsCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
		}

		#region INotifications Members

		readonly INotifications Inner;

		void INotifications.Add(INotification notification)
		{
			if (Inner != null)
			{
				Inner.Notify(notification);
			}
			INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
			InMemoryNotifications.Append((subscriberNotification != null ? subscriberNotification.MultiLineDisplayMessage : notification.Message) + "\r\n");
		}

		StringBuilder InMemoryNotifications = new StringBuilder();

		#endregion
	}
}
