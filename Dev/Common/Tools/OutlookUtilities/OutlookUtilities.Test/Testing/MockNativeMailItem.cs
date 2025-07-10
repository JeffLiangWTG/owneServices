using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.IO;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	class MockNativeMailItem : Outlook._DMailItem, Outlook._DItemEvents_Event  // earliest supported version of the interfaces
	{
		public void FireSendEvent()
		{
			if (fSendEvent != null)
			{
				fSendEvent();
			}
		}

		public void FirePropertyChangeEvent(string propertyName)
		{
			if (fPropertyChangeEvent != null)
			{
				fPropertyChangeEvent(propertyName);
			}
		}

		public void SimulateMailSend()
		{
			//			FireSendEvent(); // outlook doesn't raise this event in all versions!!
			fSentOn = DateTime.Now;
			FirePropertyChangeEvent(nameof(ReceivedTime));

			ThrowInSubjectGetter = true;
		}

		DateTime fSentOn = DateTime.Now;
		public DateTime SentOn
		{
			get { return fSentOn; }
		}

		public virtual void SaveAs(string path, object type)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var tempFilePath = resourceRetriever.SaveResourceToFile(@"Enterprise.OutlookUtilities.Testing.TestOutlookMsg.msg");
				File.Copy(tempFilePath, path, true);
				File.SetAttributes(path, FileAttributes.Normal);
			}
		}

		public bool ThrowDialogBoxOpenExceptionOnDisplay;
		public virtual void Display(object modal)
		{
			if (ThrowDialogBoxOpenExceptionOnDisplay)
			{
				throw new COMException("blah blah A dialog box is open blah blah");
			}
			ShowMockSecurityDialog();
			SimulateMailSend();
		}

		event Outlook._DItemEvents_SendEventHandler Outlook._DItemEvents_Event.Send
		{
			add { this.fSendEvent += value; }
			remove { this.fSendEvent -= value; }
		}
		Outlook._DItemEvents_SendEventHandler fSendEvent;

		event Outlook._DItemEvents_PropertyChangeEventHandler Outlook._DItemEvents_Event.PropertyChange
		{
			add { this.fPropertyChangeEvent += value; }
			remove { this.fPropertyChangeEvent -= value; }
		}
		Outlook._DItemEvents_PropertyChangeEventHandler fPropertyChangeEvent;

		public bool IsSecurityDialogEnabled = true;
		public bool ThrowInSubjectGetter;

		#region _DMailItem Members

		public string Subject
		{
			get
			{
				ShowMockSecurityDialog();
				if (ThrowInSubjectGetter)
				{
					throw new COMException("Error because the mail is already sent - you must cache the subject somewhere instead");
				}
				return fSubject;
			}
			set
			{
				ShowMockSecurityDialog();
				fSubject = value;
			}
		}
		string fSubject = "";

		public string Body
		{
			get
			{
				ShowMockSecurityDialog();
				return fBody;
			}
			set
			{
				ShowMockSecurityDialog();
				fBody = value;
			}
		}
		string fBody;

		public MockNativeRecipients Recipients
		{
			get
			{
				ShowMockSecurityDialog();
				if (fRecipients == null)
				{
					fRecipients = new MockNativeRecipients();
				}
				return fRecipients;
			}
		}
		MockNativeRecipients fRecipients;

		Outlook.Recipients Outlook._DMailItem.Recipients
		{
			get { return Recipients; }
		}

		public MockNativeRecipients ReplyRecipients
		{
			get
			{
				ShowMockSecurityDialog();
				if (fReplyRecipients == null)
				{
					fReplyRecipients = new MockNativeRecipients();
				}
				return fReplyRecipients;
			}
		}
		MockNativeRecipients fReplyRecipients;

		Outlook.Recipients Outlook._DMailItem.ReplyRecipients
		{
			get { return ReplyRecipients; }
		}

		public MockNativeAttachments Attachments
		{
			get
			{
				ShowMockSecurityDialog();
				if (fAttachments == null)
				{
					fAttachments = new MockNativeAttachments();
				}
				return fAttachments;
			}
		}
		MockNativeAttachments fAttachments;

		Outlook.Attachments Outlook._DMailItem.Attachments
		{
			get { return Attachments; }
		}

		public bool ReminderPlaySound
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReminderPlaySound getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReminderPlaySound setter implementation
			}
		}

		public bool ReminderOverrideDefault
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReminderOverrideDefault getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReminderOverrideDefault setter implementation
			}
		}

		public string BCC
		{
			get
			{
				// TODO:  Add MockNativeMailItem.BCC getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.BCC setter implementation
			}
		}

		public string ReceivedByEntryID
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReceivedByEntryID getter implementation
				return null;
			}
		}

		public bool NoAging
		{
			get
			{
				// TODO:  Add MockNativeMailItem.NoAging getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.NoAging setter implementation
			}
		}

		public Outlook.OlSensitivity Sensitivity
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Sensitivity getter implementation
				return new Outlook.OlSensitivity();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.Sensitivity setter implementation
			}
		}

		public DateTime ExpiryTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ExpiryTime getter implementation
				return new DateTime();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ExpiryTime setter implementation
			}
		}

		public string Categories
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Categories getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.Categories setter implementation
			}
		}

		public Outlook.MailItem ReplyAll()
		{
			// TODO:  Add MockNativeMailItem.ReplyAll implementation
			return null;
		}

		public object Copy()
		{
			// TODO:  Add MockNativeMailItem.Copy implementation
			return null;
		}

		public string SentOnBehalfOfName
		{
			get
			{
				// TODO:  Add MockNativeMailItem.SentOnBehalfOfName getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.SentOnBehalfOfName setter implementation
			}
		}

		public bool AlternateRecipientAllowed
		{
			get
			{
				// TODO:  Add MockNativeMailItem.AlternateRecipientAllowed getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.AlternateRecipientAllowed setter implementation
			}
		}

		public Outlook.OlRemoteStatus RemoteStatus
		{
			get
			{
				// TODO:  Add MockNativeMailItem.RemoteStatus getter implementation
				return new Outlook.OlRemoteStatus();
			}
		}

		public string ReceivedOnBehalfOfEntryID
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReceivedOnBehalfOfEntryID getter implementation
				return null;
			}
		}

		public Outlook.MAPIFolder SaveSentMessageFolder
		{
			get
			{
				// TODO:  Add MockNativeMailItem.SaveSentMessageFolder getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.SaveSentMessageFolder setter implementation
			}
		}

		public string ReceivedOnBehalfOfName
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReceivedOnBehalfOfName getter implementation
				return null;
			}
		}

		public bool ReminderSet
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReminderSet getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReminderSet setter implementation
			}
		}

		public DateTime ReminderTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReminderTime getter implementation
				return new DateTime();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReminderTime setter implementation
			}
		}

		public Outlook.OlFlagStatus FlagStatus
		{
			get
			{
				// TODO:  Add MockNativeMailItem.FlagStatus getter implementation
				return new Outlook.OlFlagStatus();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.FlagStatus setter implementation
			}
		}

		public bool DeleteAfterSubmit
		{
			get
			{
				// TODO:  Add MockNativeMailItem.DeleteAfterSubmit getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.DeleteAfterSubmit setter implementation
			}
		}

		public string ReplyRecipientNames
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReplyRecipientNames getter implementation
				return null;
			}
		}

		public Outlook.Actions Actions
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Actions getter implementation
				return null;
			}
		}

		public string EntryID
		{
			get
			{
				// TODO:  Add MockNativeMailItem.EntryID getter implementation
				return null;
			}
		}

		public bool RecipientReassignmentProhibited
		{
			get
			{
				// TODO:  Add MockNativeMailItem.RecipientReassignmentProhibited getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.RecipientReassignmentProhibited setter implementation
			}
		}

		public void PrintOut()
		{
			// TODO:  Add MockNativeMailItem.PrintOut implementation
		}

		public string ReminderSoundFile
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReminderSoundFile getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReminderSoundFile setter implementation
			}
		}

		public string BillingInformation
		{
			get
			{
				// TODO:  Add MockNativeMailItem.BillingInformation getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.BillingInformation setter implementation
			}
		}

		public Outlook.MailItem Forward()
		{
			// TODO:  Add MockNativeMailItem.Forward implementation
			return null;
		}

		public string ConversationIndex
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ConversationIndex getter implementation
				return null;
			}
		}

		public bool OriginatorDeliveryReportRequested
		{
			get
			{
				// TODO:  Add MockNativeMailItem.OriginatorDeliveryReportRequested getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.OriginatorDeliveryReportRequested setter implementation
			}
		}

		public bool AutoForwarded
		{
			get
			{
				// TODO:  Add MockNativeMailItem.AutoForwarded getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.AutoForwarded setter implementation
			}
		}

		public Outlook.UserProperties UserProperties
		{
			get
			{
				// TODO:  Add MockNativeMailItem.UserProperties getter implementation
				return null;
			}
		}

		public string To
		{
			get
			{
				// TODO:  Add MockNativeMailItem.To getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.To setter implementation
			}
		}

		public string ConversationTopic
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ConversationTopic getter implementation
				return null;
			}
		}

		public object Parent
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Parent getter implementation
				return null;
			}
		}

		public bool ReadReceiptRequested
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReadReceiptRequested getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.ReadReceiptRequested setter implementation
			}
		}

		public bool UnRead
		{
			get
			{
				// TODO:  Add MockNativeMailItem.UnRead getter implementation
				return false;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.UnRead setter implementation
			}
		}

		public DateTime CreationTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.CreationTime getter implementation
				return new DateTime();
			}
		}

		public DateTime ReceivedTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReceivedTime getter implementation
				return new DateTime();
			}
		}

		public bool Saved
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Saved getter implementation
				return false;
			}
		}

		public string ReceivedByName
		{
			get
			{
				// TODO:  Add MockNativeMailItem.ReceivedByName getter implementation
				return null;
			}
		}

		public DateTime DeferredDeliveryTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.DeferredDeliveryTime getter implementation
				return new DateTime();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.DeferredDeliveryTime setter implementation
			}
		}

		public string FlagRequest
		{
			get
			{
				// TODO:  Add MockNativeMailItem.FlagRequest getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.FlagRequest setter implementation
			}
		}

		public Outlook.Application Application
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Application getter implementation
				return null;
			}
		}

		public string Companies
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Companies getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.Companies setter implementation
			}
		}

		public string SenderName
		{
			get
			{
				// TODO:  Add MockNativeMailItem.SenderName getter implementation
				return null;
			}
		}

		public string VotingResponse
		{
			get
			{
				// TODO:  Add MockNativeMailItem.VotingResponse getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.VotingResponse setter implementation
			}
		}

		public DateTime FlagDueBy
		{
			get
			{
				// TODO:  Add MockNativeMailItem.FlagDueBy getter implementation
				return new DateTime();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.FlagDueBy setter implementation
			}
		}

		public Outlook.Inspector GetInspector
		{
			get
			{
				// TODO:  Add MockNativeMailItem.GetInspector getter implementation
				return null;
			}
		}

		public void Send()
		{
			// TODO:  Add MockNativeMailItem.Send implementation
		}

		public void Close(Outlook.OlInspectorClose saveMode)
		{
			// TODO:  Add MockNativeMailItem.Close implementation
		}

		public Outlook.OlImportance Importance
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Importance getter implementation
				return new Outlook.OlImportance();
			}
			set
			{
				// TODO:  Add MockNativeMailItem.Importance setter implementation
			}
		}

		public int Size
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Size getter implementation
				return 0;
			}
		}

		public string MessageClass
		{
			get
			{
				// TODO:  Add MockNativeMailItem.MessageClass getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.MessageClass setter implementation
			}
		}

		public void Delete()
		{
			// TODO:  Add MockNativeMailItem.Delete implementation
		}

		public Outlook.MailItem Reply()
		{
			// TODO:  Add MockNativeMailItem.Reply implementation
			return null;
		}

		public string OutlookVersion
		{
			get
			{
				// TODO:  Add MockNativeMailItem.OutlookVersion getter implementation
				return null;
			}
		}

		public Outlook.FormDescription FormDescription
		{
			get
			{
				// TODO:  Add MockNativeMailItem.FormDescription getter implementation
				return null;
			}
		}

		public string OutlookInternalVersion
		{
			get
			{
				// TODO:  Add MockNativeMailItem.OutlookInternalVersion getter implementation
				return null;
			}
		}

		public DateTime LastModificationTime
		{
			get
			{
				// TODO:  Add MockNativeMailItem.LastModificationTime getter implementation
				return new DateTime();
			}
		}

		public string VotingOptions
		{
			get
			{
				// TODO:  Add MockNativeMailItem.VotingOptions getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.VotingOptions setter implementation
			}
		}

		public string CC
		{
			get
			{
				// TODO:  Add MockNativeMailItem.CC getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.CC setter implementation
			}
		}

		public void ClearConversationIndex()
		{
			// TODO:  Add MockNativeMailItem.ClearConversationIndex implementation
		}

		public void Save()
		{
			// TODO:  Add MockNativeMailItem.Save implementation
		}

		public string Mileage
		{
			get
			{
				// TODO:  Add MockNativeMailItem.Mileage getter implementation
				return null;
			}
			set
			{
				// TODO:  Add MockNativeMailItem.Mileage setter implementation
			}
		}

		public object Move(Outlook.MAPIFolder destFldr)
		{
			// TODO:  Add MockNativeMailItem.Move implementation
			return null;
		}

		#endregion

		#region _DItemEvents_Event Members (all unimplemented)

		event Outlook._DItemEvents_ReplyAllEventHandler Outlook._DItemEvents_Event.ReplyAll
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_CustomActionEventHandler Outlook._DItemEvents_Event.CustomAction
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_WriteEventHandler Outlook._DItemEvents_Event.Write
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_CloseEventHandler Outlook._DItemEvents_Event.Close
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_OpenEventHandler Outlook._DItemEvents_Event.Open
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_CustomPropertyChangeEventHandler Outlook._DItemEvents_Event.CustomPropertyChange
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_ForwardEventHandler Outlook._DItemEvents_Event.Forward
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_ReplyEventHandler Outlook._DItemEvents_Event.Reply
		{
			add { }
			remove { }
		}

		event Outlook._DItemEvents_ReadEventHandler Outlook._DItemEvents_Event.Read
		{
			add { }
			remove { }
		}

		#endregion

		#region Implementation

		void ShowMockSecurityDialog()
		{
			if (IsSecurityDialogEnabled)
			{
				MockSecurityDialog form = new MockSecurityDialog();
				form.ShowDialog();
			}
		}

		class MockSecurityDialog : Form
		{
			[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Mock form")]
			[SuppressMessage("CargoWiseOne", "CW1042", Justification = "Mock form")]
			public MockSecurityDialog()
			{
				Button yesButton = new Button();
				yesButton.Left = 5;
				yesButton.Text = "Yes";
				Button noButton = new Button();
				noButton.Left = 75;
				noButton.Text = "No";

				Controls.Add(yesButton);
				Controls.Add(noButton);

				yesButton.Click += OnYesButton_Click;

				timer = new Timer();
				timer.Enabled = false;
				timer.Interval = 500;
				timer.Tick += Timer_Tick;
			}

			Timer timer;

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				timer.Enabled = true;
			}

			void Timer_Tick(object sender, EventArgs e)
			{
				timer.Enabled = false;
				timer.Tick -= Timer_Tick;
				timer = null;
				if (!IsDisposed)
				{
					Dispose();
				}
			}

			void OnYesButton_Click(object sender, EventArgs e)
			{
				Dispose();
			}
		}

		#endregion
	}
}
