using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EmailRecipientSelection : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EmailRecipientSelection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Recipients

		public CodeDescriptionPairList Recipients
		{
			get
			{
				if (fRecipients == null)
				{
					fRecipients = AUCustomsDataRegistry.Instance.CMRContingencyDataEmailAddresses;
				}
				return fRecipients;
			}
		}

		ContingencyDataEmailAddress EmailAddressData
		{
			get { return fEmailAddressData ?? (fEmailAddressData = new ContingencyDataEmailAddress()); }
		}
		ContingencyDataEmailAddress fEmailAddressData;

		CodeDescriptionPairList fRecipients;

		public void AddNewRecipientIfRequired()
		{
			if (Recipients.GetCodeFromDescription(RecipientEmail) == null)
			{
				Recipients.AddPair(RecipientName, RecipientEmail);
				AUCustomsDataRegistry.Instance.CMRContingencyDataEmailAddresses = Recipients;
			}
		}

		#endregion

		#region Properties

		#region Selected Recipient

		public ZString SelectedRecipientName
		{
			get { return fSelectedRecipientName; }
			set
			{
				CheckMaximumLength(SelectedRecipientNameInfo, value);
				SetNonPersistentPropertyValue(SelectedRecipientNameInfo, ref fSelectedRecipientName, value);
				RecipientName = fSelectedRecipientName;
				RecipientEmail = Recipients.GetDescriptionFromCode(RecipientName);
				RecipientNameInfo.RefreshBinding();
				RecipientEmailInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateSelectedRecipient();
				}
			}
		}
		ZString fSelectedRecipientName;

		public ZPropertyInfo SelectedRecipientNameInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedRecipientName)); }
		}

		public int SelectedRecipientName_MaxLength
		{
			get { return RecipientNameInfo.MaxLength; }
		}

		#endregion

		#region Recipient Name

		public ZString RecipientName
		{
			get { return EmailAddressData.Code; }
			set { EmailAddressData.Code = value; }
		}

		public ZPropertyInfo RecipientNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RecipientName), x => EmailAddressData.CodeInfo); }
		}

		#endregion

		#region Recipient Email

		public ZString RecipientEmail
		{
			get { return EmailAddressData.Description; }
			set { EmailAddressData.Description = (NoResString)value; }
		}

		public ZPropertyInfo RecipientEmailInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RecipientEmail), x => EmailAddressData.DescriptionInfo); }
		}

		public string[] RecipientEmails
		{
			get { return Array.ConvertAll(RecipientEmail.Split(';'), x => x.ToString()); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			EmailAddressData.RunPreSaveValidation();
			ValidateSelectedRecipient();
		}

		#region Selected Recipient Name

		public void ValidateSelectedRecipient()
		{
			SelectedRecipientNameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(SelectedRecipientNameInfo, Recipients);
		}

		#endregion

		#endregion
	}
}
