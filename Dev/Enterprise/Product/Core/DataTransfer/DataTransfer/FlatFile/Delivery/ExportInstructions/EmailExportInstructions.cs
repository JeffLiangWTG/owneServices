using System.Collections.Specialized;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;

namespace Enterprise.DataTransfer.Business
{
	public class EmailExportInstructions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EmailExportInstructions() : base()
		{
		}

		public EmailExportInstructions(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Bound Proprerties

		#region UserEnteredRecipients

		[MaxLength(200)]
		public ZString UserEnteredRecipients
		{
			get { return fUserEnteredRecipients; }
			set
			{
				if (fUserEnteredRecipients != value)
				{
					SetNonPersistentPropertyValue<ZString>(UserEnteredRecipientsInfo, ref fUserEnteredRecipients, value);
					if (!IsValidationSuspended)
					{
						ValidateUserEnteredRecipients();
					}
				}
			}
		}

		protected virtual void ValidateUserEnteredRecipients()
		{
			UserEnteredRecipientsInfo.ClearAllNotifications();

			ZString[] addresses = UserEnteredRecipients.Trim().Split(';');

			if (addresses.Length == 1 && !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(addresses[0].Trim()))
			{
				UserEnteredRecipientsInfo.AddError(Res.GetString("dd5de4fe-95a2-40f6-8087-5b7de28716dd", "Please enter a valid email address."));
			}
			else
			{
				foreach (string emailAddress in addresses)
				{
					if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(emailAddress.Trim()))
					{
						UserEnteredRecipientsInfo.AddError(Res.GetString("4b0ea3e6-b8a9-48e1-a39f-1882de5b1069", "'{0}' is not a valid email address. Please try again.", emailAddress));
						break;
					}
				}
			}
		}

		ZString fUserEnteredRecipients;

		public ZPropertyInfo UserEnteredRecipientsInfo
		{
			get { return GetZPropertyInfo(nameof(UserEnteredRecipients)); }
		}

		#endregion

		#region Subject

		[MaxLength(MailItem.Schema.MI_SubjectMaxLength)]
		public ZString Subject
		{
			get { return subject; }
			set
			{
				if (subject != value)
				{
					CheckMaximumLength(SubjectInfo, value);
					SetNonPersistentPropertyValue<ZString>(SubjectInfo, ref subject, value);
				}
			}
		}

		ZString subject;

		public ZPropertyInfo SubjectInfo
		{
			get { return GetZPropertyInfo(nameof(Subject)); }
		}

		#endregion

		#region Body

		[MaxLength(MailItem.Schema.MI_BodyMaxLength)]
		public ZString Body
		{
			get { return body; }
			set
			{
				if (body != value)
				{
					CheckMaximumLength(BodyInfo, value);
					SetNonPersistentPropertyValue<ZString>(BodyInfo, ref body, value);
				}
			}
		}
		ZString body;

		public ZPropertyInfo BodyInfo
		{
			get { return GetZPropertyInfo(nameof(Body)); }
		}

		#endregion

		#endregion

		#region Implementation

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateUserEnteredRecipients();
		}

		#endregion

		#region Recipients

		public StringCollection Recipients
		{
			get
			{
				if (fRecipients == null)
				{
					fRecipients = new StringCollection();
				}
				return fRecipients;
			}
		}

		StringCollection fRecipients;

		#endregion

		public void AddRecipient(ZString emailAddress)
		{
			if (!Recipients.Contains(emailAddress))
			{
				Recipients.Add(emailAddress);
			}
		}

		public void SplitUserEnteredRecipientsIntoSeparateAddresses()
		{
			ZString[] addresses = UserEnteredRecipients.Split(';');

			foreach (ZString address in addresses)
			{
				ZString emailAddress = address.Trim();
				if (!emailAddress.IsEmpty)
				{
					Recipients.Add(emailAddress);
				}
			}
		}

		#endregion
	}
}
