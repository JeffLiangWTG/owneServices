
#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// User notification for unit test purposes.
	/// </summary>
	public sealed class UnitTestUserNotification : UserNotificationBase, IUserNotification
	{
		internal UnitTestUserNotification()
		{
		}

		public static UnitTestUserNotification Instance
		{
			get
			{
				if (TestingState.CurrentTestMethod != null && Attribute.IsDefined(TestingState.CurrentTestMethod, typeof(DoNotAllowNotificationDuringTransactionAttribute)))
				{
					using (CargoWise.Data.Db.DisposableActionForDbConnection())
					{
						int validTransactionCount = (TransactionedTestCase.InTransactionedTestCase) ? 1 : 0;

						if (CargoWise.Data.Db.Connection.AppTransactionCount > validTransactionCount && !shownError.Value)
						{
							string description = Res.GetString("16C41CE6-9675-42B6-A4E3-FCB2B9757580", "Opening a form during a transaction [] - Caption: - Form type: ");
							shownError.Value = true;
							ErrorReporter.ReportOnce(description);
						}
					}
				}
				return instance ?? (instance = new UnitTestUserNotification());
			}
		}
		[SuppressThreadStaticFieldMessage]
		static UnitTestUserNotification instance;

		readonly static Overridable<bool> shownError = new Overridable<bool>(false);

		readonly Overridable<bool> throwOnWarningOrError = new Overridable<bool>(false);
		public bool ThrowOnWarningAndError
		{
			get { return throwOnWarningOrError.Value; }
			set { throwOnWarningOrError.Value = value; }
		}

		public bool IsInteractive
		{
			get { return true; }
		}

		public override void ShowDeveloperException(string key, string message, Exception e)
		{
			ExceptionReporter.Instance.ReportException(key, new DeveloperNotificationException(message, e));
		}

		public void ShowDeveloperException(Exception e)
		{
			ExceptionReporter.Instance.ReportException("", e);
		}

		public void Show(INotification notification)
		{
			CargoWise.Common.Argument.NotNull(notification, "Notification");
			PreviousMessages.Add(notification.Message, null, EnumStringConverter.ConvertStringToEnumEntry(notification, MessageType.Information), ZDialogResult.None);
		}

		public void ShowError(string message)
		{
			SetError(message, null);
		}

		public void ShowError(MultilingualString message)
		{
			SetError(message, null);
		}

		public override void ShowError(string message, string caption)
		{
			SetError(message, caption);
		}

		public void ShowError(MultilingualString message, string caption)
		{
			SetError(message, caption);
		}

		public bool IsShowingError { get { return false; } }

		public void ShowWarning(string message)
		{
			SetWarning(message, null);
		}

		public void ShowWarning(MultilingualString message)
		{
			SetWarning(message, null);
		}

		public void ShowWarning(string message, string caption)
		{
			SetWarning(message, caption);
		}

		public void ShowWarning(MultilingualString message, string caption)
		{
			SetWarning(message, caption);
		}

		public void ShowInformation(string message)
		{
			SetInformation(message, null);
		}

		public void ShowInformation(MultilingualString message)
		{
			SetInformation(message, null);
		}

		public void ShowInformation(string message, string caption)
		{
			SetInformation(message, caption);
		}

		public void ShowInformation(MultilingualString message, string caption)
		{
			SetInformation(message, caption);
		}

		public void Show(string message)
		{
			SetInformation(message, null);
		}

		public void Show(MultilingualString message)
		{
			SetInformation(message, null);
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			SetQuestion(message, caption, defaultResult);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			SetQuestion(message, caption, defaultResult);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, string button1Text, string button2Text)
		{
			SetQuestion(message, caption, ZDialogResult.None);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			SetQuestion(message, caption, defaultResult, parent);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			SetQuestion(message, caption, defaultResult, parent);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			SetQuestion(message, caption, ZDialogResult.None);
			return LastMessage.Answer;
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			SetQuestion(message, caption, ZDialogResult.None);
			return LastMessage.Answer;
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			LastConfirmationStringShown = confirmationString;
			return LastMessage.Answer;
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			LastConfirmationStringShown = confirmationString;
			return LastMessage.Answer;
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			var defaultResult = GetResultFor(buttons);
			SetQuestion(message, caption, defaultResult);
			LastConfirmationStringShown = confirmationString;
			return LastMessage.Answer;
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			var defaultResult = GetResultFor(buttons);
			SetQuestion(message, caption, defaultResult);
			LastConfirmationStringShown = confirmationString;
			return LastMessage.Answer;
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationString, icon);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationString, icon);
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return ShowConfirmation(message, caption, confirmationString, icon);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return ShowConfirmation(message, caption, confirmationString, icon);
		}

		public ZDialogResult ShowRepeatableConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			repeatAnswerForEntireSession = RepeatAnswerForEntireSessionUserOption;
			return ShowConfirmation(message, caption, confirmationString, icon, buttons);
		}

		public ZDialogResult ShowRepeatableConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			repeatAnswerForEntireSession = RepeatAnswerForEntireSessionUserOption;
			return ShowConfirmation(message, caption, confirmationString, icon, buttons);
		}

		public string LastConfirmationStringShown { get; private set; }

		#region Defaults

		string lastTestName;
		Queue _nextBindingMemberResponse;
		internal Queue NextDataSourceResponse
		{
			get
			{
				if (_nextBindingMemberResponse == null || TestCase.CurrentTestName != lastTestName)
				{
					lastTestName = TestCase.CurrentTestName;
					_nextBindingMemberResponse = new Queue();
				}

				return _nextBindingMemberResponse;
			}
		}

		public void AddDataSourceResponse(object o)
		{
			NextDataSourceResponse.Enqueue(o);
		}

		public void ClearDataSourceResponses()
		{
			NextDataSourceResponse.Clear();
		}

		public ZDialogResult ShowOrDefault(DialogDefaultContext context, string message)
		{
			Assertion.AssertNotNull(context);
			Assertion.AssertNotNull(message);

			SetDefaultable(context, GetResultFor(context.Buttons), message);

			return LastMessage.Answer;
		}

		internal ZDialogResult GetResultFor(ZMessageBoxButtons? buttons)
		{
			return (buttons == ZMessageBoxButtons.YesNo) ? ZDialogResult.Yes : ZDialogResult.OK;
		}

		#endregion

		#region ShowConfirmationWithNotifications

		public ZDialogResult ShowConfirmationWithNotifications(ConfirmationDialogDescriptor descriptor)
		{
			ZDialogResult result;

			if (NextAnswers.Count > 0)
			{
				var answer = NextAnswers.Dequeue();

				if (IsSuccessResult(answer))
				{
					var errors = descriptor.ConfirmationNotifications.Where(n => n.NotificationType == NotificationTypes.Error);

					if (errors.Any())
					{
						var message = "Success answer is not possible because there are notification errors.\r\n" + string.Join("\r\n", errors.Select(n => n.Message));
						ErrorReporter.ReportOnce(message);
					}
				}

				result = answer;
			}
			else
			{
				result = descriptor.Result;
			}

			PreviousMessages.Add(descriptor, result);

			return result;
		}

		static bool IsSuccessResult(ZDialogResult result)
		{
			return result == ZDialogResult.OK || result == ZDialogResult.Yes;
		}

		#endregion

		public string QueryUserResponse(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZMessageBoxDefaultButton defaultButton)
		{
			SetQuestion(message, caption, ZDialogResult.OK);

			if (LastMessage.Answer == ZDialogResult.OK)
			{
				return GetUserResponse();
			}
			else
			{
				return "";
			}
		}

		public string QueryUserResponse(UserResponseArgument args)
		{
			SetQuestion(args.Message, args.Caption, ZDialogResult.OK);
			if (LastMessage.Answer == ZDialogResult.OK || LastMessage.Answer == ZDialogResult.Yes)
			{
				return GetUserResponse();
			}
			else
			{
				return "";
			}
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			if (LastMessage.Answer == ZDialogResult.OK)
			{
				return GetUserResponse(value);
			}
			else
			{
				return "";
			}
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			if (LastMessage.Answer == ZDialogResult.OK)
			{
				return GetUserResponse(value);
			}
			else
			{
				return "";
			}
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength, bool forceCancelButton)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			if (LastMessage.Answer == ZDialogResult.OK)
			{
				return GetUserResponse(value);
			}
			else
			{
				return "";
			}
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, bool forceCancelButton, string button1Text)
		{
			SetQuestion(message, caption, ZDialogResult.OK);
			if (LastMessage.Answer == ZDialogResult.OK)
			{
				return GetUserResponse(value);
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Forget all messages, including the next answer.
		/// Call this in your unit test before calling code that shows a message, to ensure
		/// that your assertion won't be detecting a left-over message from an earlier unit test.
		/// </summary>
		public void ClearMessagesAndAnswers()
		{
			ClearMessages();
			NextAnswers.Clear();
		}

		public void ClearMessages()
		{
			fPreviousMessages = null;
		}

		public void ClearUserResponses()
		{
			NextUserResponses.Clear();
		}

		public bool RepeatAnswerForEntireSessionUserOption { get; set; }

		/// <summary>
		/// The last message remembered during unit tests.
		/// </summary>
		public PreviousMessage LastMessage
		{
			get { return PreviousMessages[0]; }
		}

		/// <summary>
		/// The messages remembered during unit tests, beginning with the last.
		/// </summary>
		public PreviousMessageList PreviousMessages
		{
			get
			{
				if (fPreviousMessages == null)
				{
					fPreviousMessages = new PreviousMessageList();
				}

				return fPreviousMessages;
			}
		}

		public void AddOKAnswer()
		{
			AddAnswer(ZDialogResult.OK);
		}

		public void AddYesAnswer()
		{
			AddAnswer(ZDialogResult.Yes);
		}

		public void AddUserResponse(string response)
		{
			NextUserResponses.Enqueue(response);
		}

		/// <summary>
		/// Set the next answer to be returned by a subsequent question message.
		/// </summary>
		public void AddAnswer(ZDialogResult answer)
		{
			NextAnswers.Enqueue(answer);
		}

		public enum MessageType { None, Question, Information, Warning, Error }

		#region Implementation

		PreviousMessageList fPreviousMessages;
		internal readonly Queue<ZDialogResult> NextAnswers = new Queue<ZDialogResult>();
		internal readonly Queue<string> NextUserResponses = new Queue<string>();

		string GetUserResponse()
		{
			if (NextUserResponses.Count > 0)
			{
				return NextUserResponses.Dequeue();
			}
			else
			{
				return "Valid user string entered and returned for testing";
			}
		}

		string GetUserResponse(string defaultValue)
		{
			if (NextUserResponses.Count > 0)
			{
				return NextUserResponses.Dequeue();
			}
			else
			{
				return defaultValue;
			}
		}

		void SetError(string text, string caption)
		{
			if (ThrowOnWarningAndError)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error - {0} - {1}", text, caption));
			}

			PreviousMessages.Add(text, caption, MessageType.Error, ZDialogResult.None);
		}

		void SetWarning(string text, string caption)
		{
			if (ThrowOnWarningAndError)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Warning - {0} - {1}", text, caption));
			}

			PreviousMessages.Add(text, caption, MessageType.Warning, ZDialogResult.None);
		}

		void SetInformation(string text, string caption)
		{
			PreviousMessages.Add(text, caption, MessageType.Information, ZDialogResult.None);
		}

		void SetQuestion(string text, string caption, ZDialogResult defaultResult, IComponent parent = null)
		{
			if (NextAnswers.Count > 0)
			{
				defaultResult = NextAnswers.Dequeue();
			}

			PreviousMessages.Add(text, caption, MessageType.Question, defaultResult, parent);
		}

		internal void SetDefaultable(DialogDefaultContext context, ZDialogResult answer, string text = null)
		{
			if (NextAnswers.Count > 0)
			{
				answer = NextAnswers.Dequeue();
			}

			PreviousMessages.Add(context, MessageType.None, answer, text);
		}

		#endregion

		public class PreviousMessageList : IEnumerable<PreviousMessage>
		{
			readonly LinkedList<PreviousMessage> messages = new LinkedList<PreviousMessage>(new[] { new PreviousMessage() });

			public bool ContainsMessageWithThisText(string messageTextToSeek)
			{
				return messages.Any(message => message.Text == messageTextToSeek);
			}

			public bool ContainsMessageContainingThisText(string messageTextToSeek)
			{
				return messages.Any(message => message.Contains(messageTextToSeek));
			}

			public PreviousMessage this[int index]
			{
				get { return messages.Skip(index).First(); }
			}

			public int Length
			{
				get { return messages.Count; }
			}

			public void Add(string text, string caption, MessageType messageType, ZDialogResult answer, IComponent parent = null)
			{
				messages.AddFirst(new PreviousMessage(text, caption, messageType, answer, parent));
			}

			public void Add(DialogDefaultContext context, MessageType messageType, ZDialogResult answer, string text = null)
			{
				messages.AddFirst(new PreviousMessage(context, messageType, answer, text));
			}

			public void Add(ConfirmationDialogDescriptor descriptor, ZDialogResult answer)
			{
				messages.AddFirst(new PreviousMessage(descriptor, answer));
			}

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<PreviousMessage> GetEnumerator()
			{
				return messages.GetEnumerator();
			}

			#endregion
		}

		public class PreviousMessage
		{
			public PreviousMessage()
			{
			}

			public PreviousMessage(string text, string caption, MessageType messageType, ZDialogResult answer, IComponent parent = null)
			{
				MessageType = messageType;
				Text = text;
				Caption = caption;
				Answer = answer;
				ParentComponent = parent;
			}

			public PreviousMessage(DialogDefaultContext context, MessageType messageType, ZDialogResult answer, string text = null)
				: this(text, context.Caption, messageType, answer)
			{
				Context = context;
			}

			public PreviousMessage(ConfirmationDialogDescriptor descriptor, ZDialogResult answer)
				: this(descriptor.Text, descriptor.ActionName, GetMessageType(descriptor), answer)
			{
				ConfirmationDialogDescriptor = descriptor;
			}

			static MessageType GetMessageType(ConfirmationDialogDescriptor descriptor)
			{
				var worstType = descriptor.ConfirmationNotifications.Where(n => n.NotificationType != NotificationTypes.None).MinOrDefault(n => n.NotificationType);

				switch (worstType)
				{
					case NotificationTypes.Warning:
						return MessageType.Warning;

					case NotificationTypes.Error:
						return MessageType.Error;

					case NotificationTypes.MessageError:
						return MessageType.Information;

					default:
						return MessageType.None;
				}
			}

			public override string ToString()
			{
				return MessageType.ToString() + " " + Text;
			}

			public DialogDefaultContext Context { get; private set; }
			public ConfirmationDialogDescriptor ConfirmationDialogDescriptor { get; private set; }
			public IComponent ParentComponent { get; private set; }

			/// <summary>
			/// The last message body text; null if there has been no message since the last call to Clear().
			/// </summary>
			public readonly string Text;

			/// <summary>
			/// The last message caption; null if there has been no message since the last call to Clear().
			/// </summary>
			public readonly string Caption;

			/// <summary>
			/// The return value of a question; None if the message isn't a question.
			/// </summary>
			public readonly ZDialogResult Answer;

			/// <summary>
			/// Does the last message, if any, contain the given string?
			/// </summary>
			public bool Contains(string s)
			{
				return Text != null && Text.Contains(s);
			}

			public bool WasDefaultable
			{
				get { return Context != null; }
			}

			/// <summary>
			/// Was there no last message?
			/// </summary>
			public bool WasNone
			{
				get { return MessageType == MessageType.None; }
			}

			/// <summary>
			/// Was the last message, if any, a question?
			/// </summary>
			public bool WasQuestion
			{
				get { return MessageType == MessageType.Question; }
			}

			/// <summary>
			/// Was the last message, if any, an information message?
			/// </summary>
			public bool WasInformation
			{
				get { return MessageType == MessageType.Information; }
			}

			/// <summary>
			/// Was the last message, if any, a warning?
			/// </summary>
			public bool WasWarning
			{
				get { return MessageType == MessageType.Warning; }
			}

			/// <summary>
			/// Was the last message, if any, an error?
			/// </summary>
			public bool WasError
			{
				get { return MessageType == MessageType.Error; }
			}

			protected MessageType MessageType;
		}
	}
}
#endif
