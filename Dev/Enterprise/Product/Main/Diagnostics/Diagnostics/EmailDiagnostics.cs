using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Diagnostics
{
	public class EmailDiagnostics : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EmailDiagnostics() : base(new BusinessObjectFactory())
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			To = "EmailTest@cargowise.com";
		}

		#region To
		public ZPropertyInfo ToInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(To));
			}
		}

		ZString fTo;
		[MaxLength(MailManager.Business.MailRecipient.Schema.MR_RecipientMailAddressMaxLength)]
		public ZString To
		{
			get
			{
				return fTo;
			}
			set
			{
				CheckMaximumLength(ToInfo, value);
				fTo = value;
				ToInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateTo();
				}
			}
		}

		public void ValidateTo()
		{
			ToInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ToInfo);
		}
		#endregion

		#region ReceivedSubject
		public ZPropertyInfo ReceivedSubjectInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ReceivedSubject));
			}
		}

		ZString fReceivedSubject;
		public ZString ReceivedSubject
		{
			get => fReceivedSubject;
#if DEBUG
			internal set => fReceivedSubject = value;
#endif
		}
		#endregion

		#region ReceivedDateTime
		public ZPropertyInfo ReceivedDateTimeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ReceivedDateTime));
			}
		}

		ZDateTime fReceivedDateTime;
		public ZDateTime ReceivedDateTime
		{
			get => fReceivedDateTime;
#if DEBUG
			internal set => fReceivedDateTime = value;
#endif
		}
		#endregion

		#region Send

		public void Send()
		{
			ValidateTo();
			if (!HasErrors)
			{
				EmailDef diagnosticMail = new EmailDef(Guid.Empty); // always send from system address, not user address, so replies go to system POP3 account
				diagnosticMail.AddRecipientForSystemCommunication(To);
				diagnosticMail.ReplyTo = EnvProxy.Instance.Registry.MailboxEmailAddress;
				diagnosticMail.Subject = SubjectIdentifier + (NoResString)" from " + MasterFiles.Business.GlbCompany.CurrentCompany.GC_Name + (NoResString)" at " + ZDateTime.Now.ToString(); // Diagnostic message
				diagnosticMail.Body = FormattableString.Invariant($@"{diagnosticMail.Subject}

To send a test message back to the client's {Core.Constants.ProductName} system, reply to this email and make sure
""{SubjectIdentifier}"" appears somewhere in the subject line."); // Diagnostic message

				EnvProxy.Instance.OutgoingMailManager.Create(Factory, diagnosticMail);

				Factory.Save();
			}
		}

		#endregion

		#region Receive

		public void CheckMail()
		{
			ZQuery filter = new ZDBOnlyQuery(typeof(MailItem));
			filter.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Direction, SQLComparisonOperator.Equal, MailDirection.Receive);
			filter.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, SubjectIdentifier);

			filter.OrderBy = MailItem.Schema.MI_ReceivedDateTime + " DESC";

			MailItem testMessage = Factory.LoadTop1(typeof(MailItem), filter) as MailItem;

			if (testMessage == null)
			{
				fReceivedSubject = NoMessages;
				fReceivedDateTime = ZDateTime.Empty;
			}
			else
			{
				fReceivedSubject = testMessage.MI_Subject;
				fReceivedDateTime = testMessage.MI_ReceivedDateTime;
			}

			ReceivedSubjectInfo.RefreshBinding();
			ReceivedDateTimeInfo.RefreshBinding();
		}

		public string SubjectIdentifier => string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} Diagnostic Test Message", Core.Constants.ProductName); // Diagnostic message

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic message")]
		internal const string NoMessages = "no test messages received";

		#endregion
	}
}
