using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecipientValidation : AutoStmScheduleTaskRecipientValidation
	{
		public StmScheduleTaskRecipientValidation(AutoStmScheduleTaskRecipient parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateS6_EmailToRecipientsAsString();
			ValidateDeliveryRecipientType();
		}

		protected new StmScheduleTaskRecipient Parent
		{
			get { return (StmScheduleTaskRecipient)base.Parent; }
		}

		void AddErrorIfDeliveryAddressIsEmpty(ZPropertyInfo propertyInfo, string recipientDescription)
		{
			if (!propertyInfo.HasErrors() && Parent.DeliveryAddress.IsEmpty)
			{
				if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					propertyInfo.AddError(Res.GetString("c4e79745-467c-4481-9d86-af1df96db926", "Please select a {0} that has an email address.", recipientDescription));
				}
				else if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
				{
					propertyInfo.AddError(Res.GetString("79e6084d-7240-4221-8514-8ecd4bc21f2d", "Please select a {0} that has a fax number.", recipientDescription));
				}
			}
		}

		protected override void CheckS6_AttachmentType()
		{
			base.CheckS6_AttachmentType();
			if (Parent.ShouldHaveAttachmentType)
			{
				MandatoryValidation.CheckEntered(Parent.S6_AttachmentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.S6_AttachmentTypeInfo, Parent.Lookups.AttachmentTypes);
				if (Parent.S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Email
						&& Parent.S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc
						&& (Parent.S6_AttachmentType == OrgConstants.AttachmentType.HTML || Parent.S6_AttachmentType == OrgConstants.AttachmentType.HTMF))
				{
					Parent.S6_AttachmentTypeInfo.AddError(Res.GetString("f5fa5050-18cd-4a8a-a5c0-b430efe439ff", "Attachment types HTML and HTMF are only supported for Email or Deliver to EDoc."));
				}
			}
		}

		protected override void CheckS6_DeliveryMethod()
		{
			base.CheckS6_DeliveryMethod();

			MandatoryValidation.CheckEntered(Parent.S6_DeliveryMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S6_DeliveryMethodInfo, Parent.Lookups.NotifyModes);

			ValidateS6_EmptyReportDeliveryOptions();

			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(Parent.S6_DeliveryMethodInfo);
			}
		}

		protected override void CheckS6_GG()
		{
			base.CheckS6_GG();
			if (Parent.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
			{
				MandatoryValidation.CheckEntered(Parent.S6_GGInfo);
				ListValidation.ErrorIfInvalidPK(Parent.S6_GGInfo, Parent.Lookups.Groups);
				AddErrorIfDeliveryAddressIsEmpty(Parent.S6_GGInfo, Res.GetString("a1b96b25-535d-444f-9a3d-15bc3a64bce9", "group with at least one staff"));
			}
		}

		protected override void CheckS6_GS_NKRecipient()
		{
			base.CheckS6_GS_NKRecipient();
			if (Parent.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
			{
				MandatoryValidation.CheckEntered(Parent.S6_GS_NKRecipientInfo);
				ListValidation.ErrorIfInvalidCode(Parent.S6_GS_NKRecipientInfo, Parent.Lookups.Recipients);
				AddErrorIfDeliveryAddressIsEmpty(Parent.S6_GS_NKRecipientInfo, Res.GetString("b3811394-576a-46b6-93d8-651edf3b4a3c", "staff"));
			}
		}

		protected override void CheckS6_FaxOverride()
		{
			base.CheckS6_FaxOverride();

			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
			{
				if (Parent.DeliveryAddress.IsEmpty && Parent.S6_FaxOverride.IsEmpty)
				{
					Parent.S6_FaxOverrideInfo.AddError(ErrorMessageEnterFaxNumber);
				}
				RefUNLOCO homePort = Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
				PhoneNumberFormatAndValidation phoneNumberValidator = new PhoneNumberFormatAndValidation();
				ZString[] faxNumbers = Parent.S6_FaxOverride.Split(',');
				ZString trueEmailFaxOverride = Parent.S6_FaxOverride;

				using (Parent.SuspendSettingHasChanges())
				{
					foreach (ZString faxNumber in faxNumbers)
					{
						Parent.S6_FaxOverride = faxNumber;
						phoneNumberValidator.PerformNumberValidation(Parent.S6_FaxOverrideInfo, homePort, true);
					}

					Parent.S6_FaxOverride = trueEmailFaxOverride;
				}
			}
		}

		public void ValidateDeliveryRecipientType()
		{
			ValidateCalculatedProperty(Parent.DeliveryRecipientTypeInfo);
		}

		protected void CheckDeliveryRecipientType()
		{
			MandatoryValidation.CheckEntered(Parent.DeliveryRecipientTypeInfo);
			if (string.IsNullOrEmpty(Parent.Lookups.DeliveryRecipientTypes.GetCodeFromDescription(Parent.DeliveryRecipientTypeInfo.Value.ToString())))
			{
				Parent.DeliveryRecipientTypeInfo.AddError(Res.GetString("cc03bd4F-fe70-4c31-8286-84abfc22fa81", "Enter a valid selection.", Parent.DeliveryRecipientTypeInfo.HumanReadableName));
			}
		}

		protected override void CheckS6_EmptyReportDeliveryOptions()
		{
			base.CheckS6_EmptyReportDeliveryOptions();
			ListValidation.ErrorIfInvalidCode(Parent.S6_EmptyReportDeliveryOptionsInfo, Parent.Lookups.BlankReportActivities);

			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax && Parent.S6_EmptyReportDeliveryOptions == EmptyReportContingencyList.Codes.SendEmailNotification)
			{
				Parent.S6_EmptyReportDeliveryOptionsInfo.AddError(ErrorMessageCannotUseEmailNotificationOnFaxDelivery);
			}

			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
			{
				ValidateS6_FaxOverride();
			}
			else if (Parent.CanSetEmail)
			{
				ValidateS6_EmailToRecipientsAsString();
			}
		}

		#region CopyRecipients

		#region S6_EmailToRecipientsAsString

		public virtual void ValidateS6_EmailToRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.S6_EmailToRecipientsAsStringInfo);
		}

		protected virtual void CheckS6_EmailToRecipientsAsString()
		{
			if (!Parent.S6_EmailToRecipientsAsStringInfo.HasErrors() && string.IsNullOrEmpty(Parent.EmailToRecipients.Value) && string.IsNullOrEmpty(Parent.DeliveryAddress))
			{
				if (Parent.S6_EmptyReportDeliveryOptions == EmptyReportContingencyList.Codes.SendEmailNotification)
				{
					Parent.S6_EmailToRecipientsAsStringInfo.AddError(ErrorMessageMustHaveEmailAddressForNotification);
				}
				else if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					Parent.S6_EmailToRecipientsAsStringInfo.AddError(StmScheduleTaskRecipientValidation.ErrorMessageEnterEmailAddress);
				}
			}
		}

		#endregion

		#region S6_CarbonCopyRecipientsAsString

		public void ValidateS6_CarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.S6_CarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckS6_CarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.S6_CarbonCopyRecipientsAsStringInfo, StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress);
		}

		#endregion

		#region S6_BlindCarbonCopyRecipientsAsString

		public void ValidateS6_BlindCarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.S6_BlindCarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckS6_BlindCarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.S6_BlindCarbonCopyRecipientsAsStringInfo, StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress);
		}

		#endregion

		#endregion

		internal static string ErrorMessageCannotUseEmailNotificationOnFaxDelivery
		{
			get { return Res.GetString("922ac561-6672-49bd-8669-d5e514ccbb71", "Cannot use Email notification when Fax Delivery method selected as there is no Email Address available to deliver to."); }
		}

		internal static string ErrorMessageMustHaveEmailAddressForNotification
		{
			get { return Res.GetString("08f71b9c-bade-4274-81b5-53868a44840d", "You must have an email address if you have chosen to Email a notification when the Report contents are empty."); }
		}

		internal static string ErrorMessageEnterEmailAddress
		{
			get { return Res.GetString("03d6aa3a-b390-4093-a5a5-62c053141b56", "Please enter an Email Address."); }
		}

		internal static string ErrorMessageEnterFaxNumber
		{
			get { return Res.GetString("7fcb8707-5092-4969-bb95-eb25c3b3d1d3", "Please enter a Fax Number."); }
		}

		internal static string ErrorMessageEnterDestinationAddress
		{
			get { return Res.GetString("a0f7cd99-85d0-4180-ad31-48ee2e924d23", "Please enter an override destination address."); }
		}
	}
}
