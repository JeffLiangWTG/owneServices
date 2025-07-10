using System;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Interop.OutlookIntegration
{
	public class OutlookMailItem : IOutlookMailItem
	{
		public OutlookMailItem(Outlook._DMailItem inner, object customData)
		{
			this.Inner = inner;
			this.fCustomData = customData;

			((Outlook._DItemEvents_Event)inner).Send += new Outlook._DItemEvents_SendEventHandler(OnMailItem_Send); // this doesn't get called for all versions of Outlook
			((Outlook._DItemEvents_Event)inner).PropertyChange += new Outlook._DItemEvents_PropertyChangeEventHandler(OnMailItem_PropertyChange);
		}

		public object CustomData
		{
			get { return fCustomData; }
		}
		readonly object fCustomData;

		public string FileNameToSaveOnSend
		{
			get { return fFileNameToSaveOnSend; }
			set { fFileNameToSaveOnSend = value; }
		}
		string fFileNameToSaveOnSend;

		public virtual string Subject
		{
			get
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					// required as you can't access the subject after the mail has been sent
					return fSubject ?? Inner.Subject;
				}
			}
			set
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.Subject = value;
				}
			}
		}
		string fSubject;

		public virtual string Body
		{
			get
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					return Inner.Body;
				}
			}
			set
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.Body = value;
				}
			}
		}

		public virtual void AddRecipient(string recipient)
		{
			try
			{
				if (string.IsNullOrEmpty(recipient.Trim()))
				{
					throw new OutlookException("Email address cannot be empty.");
				}
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.Recipients.Add(recipient);
				}
			}
			catch (COMException ex)
			{
				OutlookException.HandleCOMException(ex, (NoResString)"Could not add recipient '" + recipient + (NoResString)"'");
			}
		}

		public void AddAttachment(string fileName)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName.Trim()))
				{
					throw new OutlookException("FileName cannot be empty.");
				}
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					object missing = System.Reflection.Missing.Value;
					Inner.Attachments.Add(fileName, missing, missing, fileName);
				}
			}
			catch (COMException ex)
			{
				OutlookException.HandleCOMException(ex, (NoResString)"Could not add attachment '" + fileName + (NoResString)"'");
			}
		}

		public virtual void AddReplyRecipient(string replyRecipient)
		{
			try
			{
				if (string.IsNullOrEmpty(replyRecipient.Trim()))
				{
					throw new OutlookException("Email address cannot be empty.");
				}
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.ReplyRecipients.Add(replyRecipient);
				}
			}
			catch (COMException ex)
			{
				OutlookException.HandleCOMException(ex, (NoResString)"Could not add reply recipient '" + replyRecipient + (NoResString)"'");
			}
		}

		public virtual void Display(bool modal)
		{
			try
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.Display(modal);
				}
			}
			catch (COMException ex)
			{
				OutlookException.HandleCOMException(ex, ex.Message);
			}
		}

		public event OutlookMailItemSendEventHandler MailItemSend;

		#region Implementation

		protected readonly Outlook._DMailItem Inner;

		bool OnMailItem_Send()
		{
			fSubject = Subject;
			if (FileNameToSaveOnSend != null)
			{
				using (ExternalAppDialogButtonClicker yeser = new ExternalAppDialogButtonClicker((NoResString)"Yes"))
				{
					yeser.PressButtonOnNextDialogs();
					Inner.Save();
					Inner.SaveAs(FileNameToSaveOnSend, Outlook.OlSaveAsType.olMSG);
				}
			}

			try
			{
				if (MailItemSend != null)
				{
					MailItemSend(this.CustomData);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowWarning(ex.Message);
			}
			return true;
		}

		void OnMailItem_PropertyChange(string propertyName)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (propertyName == "ReceivedTime")
				{
					if (Inner.SentOn > DateTime.Now.Subtract(new TimeSpan(365, 0, 0)))
					{
						OnMailItem_Send();
					}
				}
			}
		}

		#endregion
	}
}
