using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture
{
	public class NotificationBuffer : MarshalByRefObject, INotifications, INotificationSubscriberQueryUser
	{
		/// <summary>
		/// Make sure you pass the parent level INotifications in if there is one or it will not get any events posted after this buffer is instantiated.
		/// </summary>
		public NotificationBuffer(INotifications inner)
		{
			this.Inner = inner;
		}

		/// <summary>
		/// DO NOT USE THIS unless there is no parent level INotifications already instantiated. Use the other constructor instead.
		/// </summary>
		public NotificationBuffer()
			: this(null)
		{
		}

		public readonly INotifications Inner;

		public INotification[] Events
		{
			get { return (INotification[])fEvents.ToArray(typeof(INotification)); }
		}

		public INotification[] GetEventsByType(params INotificationType[] types)
		{
			ArrayList eventList = new ArrayList();
			foreach (INotification notification in Events)
			{
				if (((IList)types).Contains(notification.Type))
				{
					eventList.Add(notification);
				}
			}
			return (INotification[])eventList.ToArray(typeof(INotification));
		}

		public bool ContainsNotificationType(INotificationType type)
		{
			return fNotificationTypes.Contains(type);
		}

		public bool HasErrors
		{
			get
			{
				foreach (INotificationType type in fNotificationTypes)
				{
					if (type is ErrorType || type == CargoWise.ComponentModel.NotificationType.Error)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasWarnings
		{
			get
			{
				foreach (INotificationType type in fNotificationTypes)
				{
					if (type is WarningType || type == CargoWise.ComponentModel.NotificationType.Warning)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasErrorsNotIncluding(ErrorType type)
		{
			bool result = false;
			foreach (INotificationType next in fNotificationTypes)
			{
				ErrorType nextErrorType = next as ErrorType;
				if (nextErrorType != null && nextErrorType != type)
				{
					result = true;
				}
			}
			return result;
		}

		#region Sending By Email

		public void SendEmail(Guid notificationGroupPK, IRegistryItem groupLocation, string subject, ZString bodyHeader, ZString bodyFooter, INotifications emailErrorLog)
		{
			SendEmail(notificationGroupPK, groupLocation, subject, bodyHeader, bodyFooter, emailErrorLog, EnvProxy.Instance.OutgoingMailManager);
		}

		public void SendEmail(Guid notificationGroupPK, IRegistryItem groupLocation, string subject, ZString bodyHeader, ZString bodyFooter, INotifications emailErrorLog, IOutgoingMailManager outgoingMailManager)
		{
			SendEmail(notificationGroupPK, groupLocation, subject, bodyHeader, bodyFooter, emailErrorLog, outgoingMailManager, new BusinessObjectFactory());
		}

		public void SendEmail(Guid notificationGroupPK, IRegistryItem groupLocation, string subject, ZString bodyHeader, ZString bodyFooter, INotifications emailErrorLog, IOutgoingMailManager outgoingMailManager, BusinessObjectFactory factory)
		{
			SendEmail(notificationGroupPK, groupLocation, subject, bodyHeader, bodyFooter, emailErrorLog, outgoingMailManager, factory, null);
		}

		public void SendEmail(Guid notificationGroupPK, IRegistryItem groupLocation, string subject, ZString bodyHeader, ZString bodyFooter, INotifications emailErrorLog, IOutgoingMailManager outgoingMailManager, BusinessObjectFactory factory, AttachmentDefCollection attachments)
		{
			EmailDef email = new EmailDef();
			email.Subject = subject;
			if (bodyHeader.IsEmpty && bodyFooter.IsEmpty)
			{
				email.Body = EmailBody;
			}
			else
			{
				email.Body = bodyHeader + AsString + bodyFooter;
			}

			AppendAttachments(email, attachments);
			AddRecipientsAndSend(email, outgoingMailManager, emailErrorLog, notificationGroupPK, groupLocation, factory);
		}

		void AppendAttachments(EmailDef email, AttachmentDefCollection attachments)
		{
			if (attachments != null && attachments.Count > 0)
			{
				foreach (AttachmentDef attachment in attachments)
				{
					email.Attachments.Add(attachment);
				}
			}
		}

		void AddRecipientsAndSend(EmailDef email, IOutgoingMailManager outgoingMailManager, INotifications emailErrorLog, Guid notificationGroupPK, IRegistryItem groupLocation, BusinessObjectFactory factory)
		{
			try
			{
				if (groupLocation == null)
				{
					IGlbGroup group = factory.Load<IGlbGroup>(notificationGroupPK);
					outgoingMailManager.Create(factory, email, notificationGroupPK, GroupSourceLocator.GetFromGroup(group));
				}
				else
				{
					email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(notificationGroupPK, groupLocation);
					outgoingMailManager.Create(factory, email);
				}

				factory.Save();
				emailErrorLog.Notify(new InfoNotification(Res.GetString("d8f6912f-5c19-4b68-89eb-ac9fc812256b", "Email Containing Results Sent.")));
			}
			catch (EmailSendFailedException ex)
			{
				emailErrorLog.Notify(new ErrorNotification(ErrorType.ErrorSendingEmail, ex.Message));
			}
			catch (SqlException ex)
			{
				emailErrorLog.Notify(new ErrorNotification(ErrorType.ErrorSendingEmail, ex.Message));
			}
			catch (ZSaveException ex)
			{
				emailErrorLog.Notify(new ErrorNotification(ErrorType.ErrorSendingEmail, ex.Message));
			}
		}

		public void SendEmail(string subject, string notificationGroupCode)
		{
			SendEmail(EnvProxy.Instance.OutgoingMailManager, subject, notificationGroupCode);
		}

		public void SendEmail(IOutgoingMailManager outgoingMailManager, string subject, string notificationGroupCode)
		{
			if (this.Inner == null)
			{
				ErrorReporter.ReportOnce(new StackTrace().ToString(), "You must have an inner INotifications object to send the errors");
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			IGlbGroup notificationGroup = factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, notificationGroupCode));
			if (notificationGroup == null)
			{
				Notify(new ErrorNotification(ErrorType.EmailNotifyGroupNotExist, Res.GetString("5599c28a-4b2e-4ee3-acc1-8cde2c70824f", "Use group code '{0}'", notificationGroupCode)));
			}
			else
			{
				SendEmail(notificationGroup.PK.ToGuid(), null, subject, "", "", Inner, outgoingMailManager, factory);
			}
		}

		protected string EmailBody
		{
			get
			{
				StringBuilder result = new StringBuilder();
				result.Append(EmailBodyHeader);
				result.Append("\r\n\r\n");
				result.Append(AsString);
				result.Append("\r\n");
				result.Append(EmailBodyFooter);
				return result.ToString();
			}
		}

		protected virtual string EmailBodyHeader
		{
			get { return Res.GetString("372ed00f-f555-4b39-a8d0-16f433956872", "Data Notifications; Generated {0}", ZDateTime.Now.ToString()); }
		}

		protected virtual string EmailBodyFooter
		{
			get { return Res.GetString("3a113d44-0938-44cf-9b34-3fdaa561f66e", "{0} Data Notifications Process", BrandingFactory.Instance.ProductName); }
		}

		#endregion

		public string AsString
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (INotification @event in Events)
				{
					if (!string.IsNullOrEmpty(@event.Message) || @event is NewlineNotification)
					{
						result.Append(@event.Message.Replace("\r\n", "\n").Replace("\n", "\r\n") + "\r\n");
					}
				}
				return result.ToString();
			}
		}

		public void Clear()
		{
			fEvents.Clear();
			fNotificationTypes.Clear();
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		public virtual void Notify(INotification @event)
		{
			if (Inner != null)
			{
				Inner.Notify(@event);
			}

			fNotificationTypes.Add(@event.Type);
			fEvents.Add(@event);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			if (Inner != null)
			{
				Inner.QueryUser(e);
			}
			QueryUser(e);
		}

		protected virtual void QueryUser(IQueryUserEventArgs e)
		{
		}

		#endregion

		#region Implementation

		readonly ArrayList fNotificationTypes = new ArrayList();
		readonly ArrayList fEvents = new ArrayList();

#if NET
		[Obsolete]
#endif
		public override object InitializeLifetimeService()
		{
			return null;
		}

		#endregion
	}
}
