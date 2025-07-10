using System;
using System.Collections.Specialized;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public delegate void WarningEventHandler(object sender, WarningEventArgs e);

	public delegate void ContinueEventHandler(object sender, ContinueEventArgs e);

	public abstract class BaseMessageAction : Customs.Business.ISendsMessagesToCustoms
	{
		protected BaseMessageAction()
		{
		}

		#region Properties

		public bool SendWithMessageErrors
		{
			get { return fSendWithMessageErrors; }
			set { fSendWithMessageErrors = value; }
		}
		bool fSendWithMessageErrors;

		public bool HadMessageErrors
		{
			get { return fHadMessageErrors; }
			set { fHadMessageErrors = value; }
		}
		bool fHadMessageErrors;

		public ZString LastMessage
		{
			get { return fLastMessage; }
			set { fLastMessage = value; }
		}
		ZString fLastMessage;

		public StringCollection MessageSendErrors
		{
			get { return fMessageSendErrors; }
			set { fMessageSendErrors = value; }
		}
		StringCollection fMessageSendErrors;

		public ZString WarningMessage
		{
			get { return fWarningMessage; }
			set { fWarningMessage = value; }
		}
		ZString fWarningMessage;

		public ZString WarningCaption
		{
			get { return fWarningCaption; }
			set { fWarningCaption = value; }
		}
		ZString fWarningCaption;

		public ZString InvalidOperationText
		{
			get { return fInvalidOperationText; }
			set { fInvalidOperationText = value; }
		}
		ZString fInvalidOperationText;

		public event WarningEventHandler ShowWarning;
		protected virtual void OnShowWarning(WarningEventArgs e)
		{
			if (ShowWarning != null)
			{
				ShowWarning(this, e);
			}
		}

		public event ContinueEventHandler ShowQuestion;
		protected virtual void OnShowQuestion(ContinueEventArgs e)
		{
			if (ShowQuestion != null)
			{
				ShowQuestion(this, e);
			}
		}

		#endregion

		#region ISendsMessagesToCustoms Members

		public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject)
		{
			return ContinueWithAction(message, caption);
		}

		public bool ContinueWithAction(string message, string caption)
		{
			return OnContinueWithAction(message, caption);
		}

		public bool YesNoQuery(string message, string caption, Customs.Business.MessageStyle messageStyle = Customs.Business.MessageStyle.Question)
		{
			return false;
		}
		public Customs.Business.YesNoCancel YesNoCancelQuery(string message, string caption, Customs.Business.MessageStyle messageStyle = Customs.Business.MessageStyle.Question)
		{
			return Customs.Business.YesNoCancel.Cancel;
		}

		public bool ContinueWithSend(StringCollection warnings)
		{
			return OnContinueWithSend(warnings);
		}

		public bool ShowUserConfirmation(string message, string captionButton, string confirmationPrompt, string confirmationString)
		{
			return false;
		}

		public Customs.Business.SingleMessageManager[] WhichMessagesShouldWeSend(Customs.Business.SingleMessageManager[] allManagers)
		{
			return OnWhichMessagesShouldWeSend(allManagers);
		}

		public Customs.Business.SingleMessageManager[] WhichMessagesShouldWeReset(Customs.Business.SingleMessageManager[] allManagers)
		{
			return OnWhichMessagesShouldWeReset(allManagers);
		}

		public Customs.Business.SingleMessageManager[] WhichMessagesShouldWeWithdraw(Customs.Business.SingleMessageManager[] allManagers)
		{
			return OnWhichMessagesShouldWeWithdraw(allManagers);
		}

		public void NotifyUserOfASuccessfulSend(string text)
		{
			OnNotifyUserOfASuccessfulSend(text);
		}

		public void MessageSendErrorAlert(StringCollection errors)
		{
			OnMessageSendErrorAlert(errors);
		}

		public void WarnUserAboutSomething(string message, string caption)
		{
			OnWarnUserAboutSomething(message, caption);
		}

		public void NotifyUserOfAnInvalidOperation(string text)
		{
			OnNotifyUserOfAnInvalidOperation(text);
		}

		#endregion

		#region Implementation

		protected virtual bool OnContinueWithAction(string message, string caption)
		{
			if (SendWithMessageErrors)
			{
				ContinueEventArgs e = new ContinueEventArgs(message, caption);
				e.Cancel = true;
				OnShowQuestion(e);
				LastMessage = caption + ":\r\n The following messages errors must be corrected before submitting a Pre-Alert:\r\n" + message;
				return !e.Cancel;
			}
			return false;
		}

		protected virtual bool OnContinueWithSend(StringCollection warnings)
		{
			HadMessageErrors = warnings.Count != 0;

			StringBuilder sb = new StringBuilder();
			foreach (ZString warningMessage in warnings)
			{
				sb.Append(warningMessage + "\r\n");
			}
			ZString warningString = sb.ToString();
			LastMessage = "The following messages errors exist:\r\n" + warningString;

			if (HadMessageErrors)
			{
				if (SendWithMessageErrors)
				{
					ContinueEventArgs e = new ContinueEventArgs(warningString, "Continue with send?");
					if (warnings.Count > 0)
					{
						e.Cancel = true;
						OnShowQuestion(e);
					}
					return e.Cancel;
				}
				else
				{
					WarningEventArgs e = new WarningEventArgs("Message Errors Exist", LastMessage);
					OnShowWarning(e);
					return false;
				}
			}
			return !HadMessageErrors;
		}

		protected virtual Customs.Business.SingleMessageManager[] OnWhichMessagesShouldWeWithdraw(Customs.Business.SingleMessageManager[] allManagers)
		{
			return Array.Empty<Customs.Business.SingleMessageManager>();
		}

		protected virtual Customs.Business.SingleMessageManager[] OnWhichMessagesShouldWeSend(Customs.Business.SingleMessageManager[] allManagers)
		{
			return Array.Empty<Customs.Business.SingleMessageManager>();
		}

		protected virtual Customs.Business.SingleMessageManager[] OnWhichMessagesShouldWeReset(Customs.Business.SingleMessageManager[] allManagers)
		{
			return Array.Empty<Customs.Business.SingleMessageManager>();
		}

		protected virtual void OnMessageSendErrorAlert(StringCollection errors)
		{
			MessageSendErrors = errors;
		}

		protected virtual void OnWarnUserAboutSomething(string message, string caption)
		{
			WarningMessage = message;
			WarningCaption = caption;
			OnShowWarning(new WarningEventArgs(caption, message));
		}

		protected virtual void OnNotifyUserOfAnInvalidOperation(string text)
		{
			InvalidOperationText = text;
		}

		protected virtual void OnNotifyUserOfASuccessfulSend(string text)
		{
			LastMessage = text;
		}

		#endregion
	}
}
