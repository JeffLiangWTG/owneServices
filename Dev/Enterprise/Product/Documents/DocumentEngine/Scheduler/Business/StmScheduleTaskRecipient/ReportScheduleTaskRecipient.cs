using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	[DependentBusinessObject(typeof(ReportScheduleTask), "Recipients")]
	public class ReportScheduleTaskRecipient : StmScheduleTaskRecipient, IServiceDtoMapper
	{
		public ReportScheduleTaskRecipient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties overrides

		public override ZString S6_DeliveryToType
		{
			get => base.S6_DeliveryToType;
			set
			{
				base.S6_DeliveryToType = value;

				if (IsEDocDeliverMode)
				{
					S6_DeliveryMethod = ZString.Empty;
					S6_EmptyReportDeliveryOptions = ZString.Empty;
				}

				if (S6_AttachmentType_ReadOnly)
				{
					S6_AttachmentType = ZString.Empty;
				}
			}
		}

		public override ZString S6_DeliveryMethod
		{
			get => base.S6_DeliveryMethod;
			set
			{
				base.S6_DeliveryMethod = value;

				if (!IsEmailDeliverMode)
				{
					Validation.ValidateS6_EmailFromAddress();
					if (S6_EmailFromAddressInfo.HasErrors())
					{
						base.S6_EmailFromAddress = ZString.Empty;
					}
				}

				if (!IsFtpDeliverMode)
				{
					S6_FtpAddress = ZString.Empty;
				}

				if (reportScheduleTask != null)
				{
					reportScheduleTask.Validation.ValidateUserFK();
				}
			}
		}

		public override ZGuid S6_OH
		{
			get => base.S6_OH;
			set
			{
				base.S6_OH = value;
				if (value.IsEmpty)
				{
					ContactName = ZString.Empty;
				}
			}
		}

		public override ZString S6_GS_NKRecipient
		{
			get => base.S6_GS_NKRecipient;
			set
			{
				base.S6_GS_NKRecipient = value;
				if (!value.IsEmpty)
				{
					ContactName = ZString.Empty;
				}
			}
		}

		protected bool S6_DeliveryMethod_ReadOnly => IsEDocDeliverMode;

		protected bool S6_EmptyReportDeliveryOptions_ReadOnly => IsEDocDeliverMode;

		bool IsEDocDeliverMode => S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;

		protected override bool S6_GS_NKRecipient_ReadOnly => base.S6_GS_NKRecipient_ReadOnly && !(IsEDocDeliverMode && S6_OH.IsEmpty);

		protected override bool S6_OH_ReadOnly => base.S6_OH_ReadOnly && !(IsEDocDeliverMode && S6_GS_NKRecipient.IsEmpty);

		protected override bool ContactName_ReadOnly => base.ContactName_ReadOnly && !(IsEDocDeliverMode && S6_GS_NKRecipient.IsEmpty);

		[Password]
		public override ZString S6_Password
		{
			get => base.S6_Password;
			set => base.S6_Password = value;
		}

		[MaxLength(nameof(ToFaxOrEmail_MaxLength))]
		public ZString ToFaxOrEmail
		{
			get { return CanSetEmail ? S6_EmailToRecipientsAsString : S6_FaxOverride; }
			set
			{
				if (CanSetEmail)
				{
					S6_EmailToRecipientsAsString = value;
				}
				else
				{
					S6_FaxOverride = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateToFaxOrEmail();
				}

				ToFaxOrEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ToFaxOrEmailInfo
		{
			get { return GetZPropertyInfo(nameof(ToFaxOrEmail)); }
		}

		protected bool ToFaxOrEmail_ReadOnly
			=> !CanEditAddressOverride;

		int ToFaxOrEmail_MaxLength
			=> S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email ? -1 : StmScheduleTaskRecipient.Schema.S6_FaxOverrideMaxLength;

		bool CanEditAddressOverride
			=> S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax || CanSetEmail;

		public override bool CanSetEmail
			=> base.CanSetEmail ||
			(S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print && S6_EmptyReportDeliveryOptions == EmptyReportContingencyList.Codes.SendEmailNotification);

		public FieldType DeliveryMethodFieldType
			=> CanSetEmail ? FieldType.TextCodeFindBox : FieldType.Text;

		protected override bool ShouldHaveAttachmentType => IsFtpDeliverMode || base.ShouldHaveAttachmentType || IsEDocDeliverMode;

		#region Test stuff
#if DEBUG

		internal bool ShouldHaveAttachmentTypeExposedForTests => ShouldHaveAttachmentType;

#endif
		#endregion

		#region Ftp details

		public bool IsFtpDeliverMode => S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Ftp;

		protected bool S6_FtpAddress_ReadOnly => !IsFtpDeliverMode;

		protected bool S6_UserName_ReadOnly => !IsFtpDeliverMode;

		protected bool S6_Password_ReadOnly => !IsFtpDeliverMode;

		#endregion

		#region Email details

		public bool IsEmailDeliverMode => S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email;

		#endregion

		#region EmailFromAddress

		[BusinessObjectTestExclude]
		[List("EmailFromAddressList")]
		public ZString EmailFromAddress
		{
			get
			{
				if (S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					if (!S6_EmailFromAddress.IsEmpty)
					{
						var result = EmailFromAddressList.GetCodeFromDescription(S6_EmailFromAddress);

						return result ?? S6_EmailFromAddress;
					}
				}

				return ZString.Empty;
			}

			set
			{
				S6_EmailFromAddress = EmailFromAddressList.GetDescriptionFromCode(value) ?? value;

				EmailFromAddressInfo.RefreshBinding();
			}
		}

		internal bool EmailFromAddress_ReadOnly => S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Email;

		public ZPropertyInfo EmailFromAddressInfo => GetWrappedZPropertyInfo(nameof(EmailFromAddress), x => S6_EmailFromAddressInfo);

		public CodeDescriptionPairList EmailFromAddressList
		{
			get
			{
				emailFromAddressList = new CodeDescriptionPairList();

				if (PrintUser != null)
				{
					var emailAddresses = PrintUser?.EmailAddresses.ToArray();

					if (emailAddresses != null)
					{
						var mainEmailAddressString = ZString.Empty;

						foreach (var emailAddress in emailAddresses)
						{
							if (!emailAddress.GSE_EmailAddress.IsEmpty)
							{
								if (emailAddress.GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main)
								{
									mainEmailAddressString = emailAddress.GSE_EmailAddress;
								}
								else
								{
									var emailAddressWithType = JoinEmailTypeWithEmailAddress(emailAddress.EmailType, emailAddress.GSE_EmailAddress);
									emailFromAddressList.AddPair(emailAddressWithType, emailAddress.GSE_EmailAddress);
								}
							}
						}

						emailFromAddressList.Sort();

						if (!mainEmailAddressString.IsEmpty)
						{
							emailFromAddressList.Insert(0, new CodeDescriptionPair(JoinEmailTypeWithEmailAddress(Core.Constants.EmailFromAddressTypes.Descriptions.Main, mainEmailAddressString), mainEmailAddressString));
						}
					}
				}

				return emailFromAddressList;
			}
		}

		CodeDescriptionPairList emailFromAddressList;

		string JoinEmailTypeWithEmailAddress(string emailType, string emailAddress)
		{
			return FormattableString.Invariant($"{emailType} - {emailAddress}");
		}

		public GlbStaff PrintUser
		{
			get
			{
				if (reportScheduleTask == null)
				{
					reportScheduleTask = Factory.Load<ReportScheduleTask>(S6_S5);
				}

				return reportScheduleTask?.PrintUser;
			}
		}

		ReportScheduleTask reportScheduleTask;

		#endregion

		#endregion

		#region Populate from Contact

		public void PopulateFromContact(DeliveryInstructions instructions, DocDeliveryContact docContact)
		{
			S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			S6_OH = docContact.OrgHeaderPK;
			S6_DeliveryMethod = docContact.DeliveryMethod;

			if (DeliveryMethodHelper.IsEmailOrEPrint(S6_DeliveryMethod))
			{
				ZString translatedEmailSubject = new TextMacroProcessor().Replace(docContact.EmailSubjectMacro, instructions.GetRelatedBusinessObjectsForEmailSubject(docContact));
				if (!translatedEmailSubject.IsEmpty)
				{
					S6_EmailSubjectLineOverride = translatedEmailSubject;
				}

				S6_AttachmentType = docContact.AttachmentType;
			}
			else if (S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
			{
				S6_SQ = instructions.PrinterDelivery.PrintQueuePK;
			}

			if (docContact.Contact != null)
			{
				S6_OC = docContact.Contact.PK;
			}
			else if (docContact.Staff != null)
			{
				S6_GS_NKRecipient = docContact.Staff.GS_Code;
				S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			}

			if (!docContact.DeliveryAddress.IsEmpty)
			{
				if (S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					S6_EmailToRecipientsAsString = docContact.DeliveryAddress;
				}
				else
				{
					S6_FaxOverride = docContact.DeliveryAddress.SubstringSafe(0, StmScheduleTaskRecipientSchema.S6_FaxOverride.MaxLength);
				}
			}

			if (docContact.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				if (!docContact.Email.IsEmpty)
				{
					S6_EmailToRecipientsAsString = docContact.Email;
				}
				if (!docContact.EmailCarbonCopyRecipientsAsString.IsEmpty)
				{
					S6_CarbonCopyRecipientsAsString = docContact.EmailCarbonCopyRecipientsAsString;
				}

				if (!docContact.EmailBlindCarbonCopyRecipientsAsString.IsEmpty)
				{
					S6_BlindCarbonCopyRecipientsAsString = docContact.EmailBlindCarbonCopyRecipientsAsString;
				}

				if (!docContact.EmailFromAddress.IsEmpty)
				{
					S6_EmailFromAddress = docContact.EmailFromAddress;
				}
			}
		}

		#endregion

		#region Validation / Lookups

		public new ReportScheduleTaskRecipientLookups Lookups => (ReportScheduleTaskRecipientLookups)base.Lookups;

		protected override StmScheduleTaskRecipientLookups GetNewLookups()
		{
			return new ReportScheduleTaskRecipientLookups(this);
		}

		public new ReportScheduleTaskRecipientValidation Validation => (ReportScheduleTaskRecipientValidation)base.Validation;

		protected override StmScheduleTaskRecipientValidation GetNewValidation()
		{
			return new ReportScheduleTaskRecipientValidation(this);
		}

		#endregion

		#region IServiceDtoMapper.Identifier

		string IServiceDtoMapper.Identifier { get; set; }

		#endregion

	}
}
