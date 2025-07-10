using System;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class XMLTask
	{
		public XMLTask(INotifications notify)
		{
			Notify = notify;
		}

		public void Run()
		{
			if (IsEnvironmentDataValid())
			{
				RunTask();
			}
		}

		protected bool SendEmailToNotificationGroup(Guid notificationGroup, IRegistryItem notificationGroupItem, FileInfo dataFile, ZString message)
		{
			bool sent;
			if (sent = (notificationGroup != Guid.Empty))
			{
				EmailDef email = null;
				try
				{
					email = CreateEmailDef(message, dataFile);
					Env.OutgoingMailManager.CreateAndSave(email, notificationGroup, GroupSourceLocator.GetFromRegistryItem(notificationGroupItem));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (Marshal.GetHRForException(ex) == TempFile.FileIsInUseByAnotherProcess)
					{
						Notify.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("b1d451b0-c2c1-43ca-b1bc-7caf063c5b17", "The file is locked.")));
						sent = false;
					}
					else
					{
						if (email != null)
						{
							Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
							Notify.Notify(new WarningNotification(Res.GetString("7eaf47d6-3bf4-4c92-9d33-ed0dd837e85a", "The email notification group is empty. Notification email was sent to Postmaster")));
						}
						else
						{
							ExceptionDetails exceptionDetails = new ExceptionDetails(ex);
							Notify.Notify(new ErrorNotification(ErrorType.Error, exceptionDetails.GetFullReport()));
							sent = false;
						}
					}
				}
			}
			return sent;
		}

		protected virtual EmailDef CreateEmailDef(ZString message, FileInfo dataFile)
		{
			EmailDef email = new EmailDef();
			email.Subject = NotificationEmailSubject;
			email.Body = message;
			if (dataFile != null && dataFile.Exists)
			{
				email.Attachments.Add(new AttachmentDef(dataFile.FullName));
				email.Subject += " - " + dataFile.Name;
			}
			return email;
		}

		protected virtual bool IsEnvironmentDataValid()
		{
			return true;
		}

		/// <summary>
		/// UniqueIdentifier should uniquely describe what files the Task is concerned with. This is used to ensure two Tasks are not run on different service task instances simultaneously.
		/// </summary>
		public abstract ZString UniqueIdentifier { get; }

		protected abstract void RunTask();
		protected abstract string NotificationEmailSubject { get; }
		protected readonly INotifications Notify;
	}
}
