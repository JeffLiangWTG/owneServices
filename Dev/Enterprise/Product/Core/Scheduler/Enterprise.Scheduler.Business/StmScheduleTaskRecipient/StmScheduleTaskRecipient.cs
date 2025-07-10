using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	[DependentBusinessObject(typeof(StmScheduleTask), "Recipients")]
	public class StmScheduleTaskRecipient : AutoStmScheduleTaskRecipient, IStmScheduleTaskRecipient
	{
		public StmScheduleTaskRecipient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString S6_DeliveryMethod
		{
			get { return base.S6_DeliveryMethod; }
			set
			{
				base.S6_DeliveryMethod = value;

				if (!ShouldHaveAttachmentType)
				{
					S6_AttachmentType = ZString.Empty;
				}

				if (S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Fax)
				{
					S6_FaxOverride = ZString.Empty;
				}

				if (S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Print)
				{
					S6_SQ = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateS6_GG();
					Validation.ValidateS6_GS_NKRecipient();
				}
			}
		}

		public GlbStaff Staff
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, S6_GS_NKRecipient);
			}
		}

		public override ZString S6_DeliveryToType
		{
			get { return base.S6_DeliveryToType; }
			set
			{
				base.S6_DeliveryToType = value;

				if (value != ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
				{
					S6_OH = ZGuid.Empty;
					ContactName = "";
				}

				if (value != ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
				{
					S6_GS_NKRecipient = ZString.Empty;
				}

				if (value != ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
				{
					S6_GG = ZGuid.Empty;
				}
			}
		}

		public override ZGuid S6_OH
		{
			get { return base.S6_OH; }
			set
			{
				base.S6_OH = value;
				contacts = null;
			}
		}

		protected bool S6_AttachmentType_ReadOnly
		{
			get { return !ShouldHaveAttachmentType; }
		}

		protected internal virtual bool ShouldHaveAttachmentType
		{
			get { return DeliveryMethodHelper.IsEmailOrEPrint(S6_DeliveryMethod); }
		}

		protected virtual bool S6_GS_NKRecipient_ReadOnly
		{
			get { return (S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.Staff); }
		}

		protected virtual bool S6_OH_ReadOnly
		{
			get { return (S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.Contact); }
		}

		protected bool S6_GG_ReadOnly
		{
			get { return (S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.Group); }
		}

		protected bool S6_SQ_ReadOnly
		{
			get { return (S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Print); }
		}

		protected bool S6_FaxOverride_ReadOnly
		{
			get { return S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Fax; }
		}

		#region Contact Name

		[MaxLength(OrgContact.Schema.OC_ContactNameMaxLength)]
		public ZString ContactName
		{
			get
			{
				ZString result;
				if (contactName.HasValue)
				{
					result = contactName.Value;
				}
				else
				{
					result = (Contact != null) ? Contact.OC_ContactName : ZString.Empty;
				}
				return result;
			}
			set
			{
				CheckMaximumLength(ContactNameInfo, value);
				contactName = value;

				if (!ContactNameInfo.HasErrors())
				{
					ZQuery filter = new ZQuery(OrgContactSchema.OC_ContactName, value);
					BusinessObject[] results = Contacts.Find(filter);
					if (results.Length > 0)
					{
						S6_OC = results[0].PK;
					}
					else
					{
						S6_OC = ZGuid.Empty;
					}
				}
				else
				{
					S6_OC = ZGuid.Empty;
				}

				ContactNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(ContactName)); }
		}

		protected virtual bool ContactName_ReadOnly
		{
			get { return (S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.Contact); }
		}

		ZString? contactName;

		#endregion

		#region Organisation Name

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString OrganisationName
		{
			get { return (Header != null) ? Header.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationNameInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationName)); }
		}

		#endregion

		#region Delivery Recipient Type

		public ZString DeliveryRecipientType
		{
			get { return deliveryRecipientType ?? Lookups.DeliveryRecipientTypes.GetDescriptionFromCode(S6_DeliveryToType); }
			set
			{
				CheckMaximumLength(DeliveryRecipientTypeInfo, value);
				deliveryRecipientType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryRecipientType();
				}
				if (!DeliveryRecipientTypeInfo.HasErrors())
				{
					S6_DeliveryToType = Lookups.DeliveryRecipientTypes.GetCodeFromDescription(value);
				}
				DeliveryRecipientTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryRecipientTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryRecipientType)); }
		}

		ZString? deliveryRecipientType;

		#endregion

		#region Delivery Address

		public ZString DeliveryAddress
		{
			get
			{
				switch (S6_DeliveryMethod)
				{
					case Core.Constants.ContactNotifyModes.Email:
						return GetDeliveryAddress(OrgContactSchema.Constants.OC_Email, GlbStaffSchema.Constants.GS_EmailAddress);

					case Core.Constants.ContactNotifyModes.EPrint:
						return DocumentsDataRegistry.Instance.EPrintEmailAddress.Value;

					case Core.Constants.ContactNotifyModes.Fax:
						return GetDeliveryAddress(OrgContactSchema.Constants.OC_Fax, GlbStaffSchema.Constants.GS_FaxNum);

					case Core.Constants.ContactNotifyModes.Ftp:
					case Core.Constants.ContactNotifyModes.Print:
						if (S6_EmptyReportDeliveryOptions == EmptyReportContingencyList.Codes.SendEmailNotification)
						{
							return GetDeliveryAddress(OrgContactSchema.Constants.OC_Email, GlbStaffSchema.Constants.GS_EmailAddress);
						}
						return ZString.Empty;

					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo DeliveryAddressInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryAddress)); }
		}

		ZString GetDeliveryAddress(string orgContactProperty, string glbStaffProperty)
		{
			switch (S6_DeliveryToType)
			{
				case ScheduledReportDeliveryRecipientConstants.RecipientType.Contact:
					return new ZString(Contact?[orgContactProperty]);

				case ScheduledReportDeliveryRecipientConstants.RecipientType.Staff:
					return new ZString(Recipient?[glbStaffProperty]);

				case ScheduledReportDeliveryRecipientConstants.RecipientType.Group:
					var staffEmails = Group?.Staff.Cast<GlbStaff>().Select(s => (ZString)s[glbStaffProperty]) ?? Enumerable.Empty<ZString>();
					return string.Join(", ", staffEmails.Where(email => !email.IsEmpty));

				default:
					return ZString.Empty;
			}
		}

		#endregion

		#region CopyRecipients

		#region EmailToRecipients

		[ChildEditable(true)]
		public StmScheduleTaskCopyRecipientCollection EmailToRecipients
		{
			get
			{
				if (emailToRecipients == null)
				{
					emailToRecipients = new StmScheduleTaskCopyRecipientCollection(this, Constants.CopyRecipientType.EmailToRecipient);
					RegisterEditableChildObject(emailToRecipients);
					emailToRecipients.Updated += EmailToRecipients_Updated;
				}
				return emailToRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString S6_EmailToRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (CanSetEmail)
				{
					if (emailToRecipientsAsString == null)
					{
						emailToRecipientsAsString = EmailToRecipients.Value;
					}
					result = emailToRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (emailToRecipientsAsString != value)
				{
					isUpdatingEmailToRecipientsAsString = true;
					try
					{
						CheckMaximumLength(S6_EmailToRecipientsAsStringInfo, value);
						emailToRecipientsAsString = value;
						EmailToRecipients.Value = value;
						Validation.ValidateS6_EmailToRecipientsAsString();
						if (!S6_EmailToRecipientsAsStringInfo.HasErrors())
						{
							emailToRecipientsAsString = EmailToRecipients.Value;
						}
						S6_EmailToRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingEmailToRecipientsAsString = false;
					}
				}
			}
		}

		public virtual bool CanSetEmail => S6_DeliveryMethod == Constants.ContactNotifyModes.Email;

		public ZPropertyInfo S6_EmailToRecipientsAsStringInfo => GetZPropertyInfo(nameof(S6_EmailToRecipientsAsString));

		protected bool S6_EmailToRecipientsAsString_ReadOnly
		{
			get
			{
				var sendEmailOnEmpty = (S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print && S6_EmptyReportDeliveryOptions == EmptyReportContingencyList.Codes.SendEmailNotification);

				return !sendEmailOnEmpty && (S6_DeliveryMethod != Constants.ContactNotifyModes.Email);
			}
		}

		void EmailToRecipients_Updated(object sender, EventArgs e)
		{
			if (!isUpdatingEmailToRecipientsAsString)
			{
				emailToRecipientsAsString = null;
			}

			S6_EmailToRecipientsAsStringInfo.RefreshBinding();
		}

		StmScheduleTaskCopyRecipientCollection emailToRecipients;
		string emailToRecipientsAsString;
		bool isUpdatingEmailToRecipientsAsString;

		#endregion

		#region CarbonCopyRecipients

		[ChildEditable(true)]
		public StmScheduleTaskCopyRecipientCollection CarbonCopyRecipients
		{
			get
			{
				if (carbonCopyRecipients == null)
				{
					carbonCopyRecipients = new StmScheduleTaskCopyRecipientCollection(this, Constants.CopyRecipientType.CarbonCopyRecipient);
					RegisterEditableChildObject(carbonCopyRecipients);
					carbonCopyRecipients.Updated += CarbonCopyRecipients_Updated;
				}
				return carbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString S6_CarbonCopyRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (S6_DeliveryMethod == Constants.ContactNotifyModes.Email)
				{
					if (carbonCopyRecipientsAsString == null)
					{
						carbonCopyRecipientsAsString = CarbonCopyRecipients.Value;
					}
					result = carbonCopyRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (carbonCopyRecipientsAsString != value)
				{
					isUpdatingCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(S6_CarbonCopyRecipientsAsStringInfo, value);
						carbonCopyRecipientsAsString = value;
						CarbonCopyRecipients.Value = value;
						Validation.ValidateS6_CarbonCopyRecipientsAsString();
						if (!S6_CarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							carbonCopyRecipientsAsString = CarbonCopyRecipients.Value;
						}
						S6_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo S6_CarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(S6_CarbonCopyRecipientsAsString)); }
		}

		protected bool S6_CarbonCopyRecipientsAsString_ReadOnly
		{
			get { return S6_DeliveryMethod != Constants.ContactNotifyModes.Email; }
		}

		void CarbonCopyRecipients_Updated(object sender, EventArgs e)
		{
			if (!isUpdatingCarbonCopyRecipientsAsString)
			{
				carbonCopyRecipientsAsString = null;
			}
			S6_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		StmScheduleTaskCopyRecipientCollection carbonCopyRecipients;
		string carbonCopyRecipientsAsString;
		bool isUpdatingCarbonCopyRecipientsAsString;

		#endregion

		#region BlindCarbonCopyRecipients

		[ChildEditable(true)]
		public StmScheduleTaskCopyRecipientCollection BlindCarbonCopyRecipients
		{
			get
			{
				if (blindCarbonCopyRecipients == null)
				{
					blindCarbonCopyRecipients = new StmScheduleTaskCopyRecipientCollection(this, "BCC");
					RegisterEditableChildObject(blindCarbonCopyRecipients);
					blindCarbonCopyRecipients.Updated += BlindCarbonCopyRecipientsOnUpdated;
				}
				return blindCarbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString S6_BlindCarbonCopyRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (S6_DeliveryMethod == Constants.ContactNotifyModes.Email)
				{
					if (blindCarbonCopyRecipientsAsString == null)
					{
						blindCarbonCopyRecipientsAsString = BlindCarbonCopyRecipients.Value;
					}
					result = blindCarbonCopyRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (blindCarbonCopyRecipientsAsString != value)
				{
					isUpdatingBlindCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(S6_BlindCarbonCopyRecipientsAsStringInfo, value);
						blindCarbonCopyRecipientsAsString = value;
						BlindCarbonCopyRecipients.Value = value;
						Validation.ValidateS6_BlindCarbonCopyRecipientsAsString();
						if (!S6_BlindCarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							blindCarbonCopyRecipientsAsString = BlindCarbonCopyRecipients.Value;
						}
						S6_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingBlindCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo S6_BlindCarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(S6_BlindCarbonCopyRecipientsAsString)); }
		}

		protected bool S6_BlindCarbonCopyRecipientsAsString_ReadOnly
		{
			get { return S6_DeliveryMethod != Constants.ContactNotifyModes.Email; }
		}

		void BlindCarbonCopyRecipientsOnUpdated(object sender, EventArgs eventArgs)
		{
			if (!isUpdatingBlindCarbonCopyRecipientsAsString)
			{
				blindCarbonCopyRecipientsAsString = null;
			}
			S6_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		StmScheduleTaskCopyRecipientCollection blindCarbonCopyRecipients;
		string blindCarbonCopyRecipientsAsString;
		bool isUpdatingBlindCarbonCopyRecipientsAsString;

		#endregion

		public List<string> CopyRecipientList
		{
			get { return new List<string>(); }
		}

		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var recipient = (StmScheduleTaskRecipient)base.CloneInternal(args);
			recipient.S6_EmailToRecipientsAsString = S6_EmailToRecipientsAsString;
			recipient.S6_CarbonCopyRecipientsAsString = S6_CarbonCopyRecipientsAsString;
			recipient.S6_BlindCarbonCopyRecipientsAsString = S6_BlindCarbonCopyRecipientsAsString;
			return recipient;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			CarbonCopyRecipients.DeleteAll();
			BlindCarbonCopyRecipients.DeleteAll();
			EmailToRecipients.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			if (S6_EmailToRecipientsAsString_ReadOnly)
			{
				EmailToRecipients.DeleteAll();
			}
			if (S6_CarbonCopyRecipientsAsString_ReadOnly)
			{
				CarbonCopyRecipients.DeleteAll();
			}
			if (S6_BlindCarbonCopyRecipientsAsString_ReadOnly)
			{
				BlindCarbonCopyRecipients.DeleteAll();
			}
			base.OnSaving();
		}

		#endregion

		#region OrgContactCollection Contacts

		public OrgContactCollection Contacts
		{
			get
			{
				if (contacts == null)
				{
					if (Header == null)
					{
						contacts = new OrgContactCollection(Factory);
					}
					else
					{
						ZQuery filter = new ZQuery(OrgContactSchema.OC_OH, S6_OH);
						contacts = new OrgContactCollection(Factory, filter);
						contacts.Load();
					}
				}
				return contacts;
			}
		}
		OrgContactCollection contacts;

		#endregion
	}
}
