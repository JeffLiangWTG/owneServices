using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskRecipientValidation : StmScheduleTaskRecipientValidation
	{
		public ReportScheduleTaskRecipientValidation(ReportScheduleTaskRecipient parent)
			: base(parent)
		{
			ZValidationInternals = this;
		}

		protected readonly IValidationInternals ZValidationInternals;

		protected new ReportScheduleTaskRecipient Parent => (ReportScheduleTaskRecipient)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateToFaxOrEmail();
		}

		public void ValidateToFaxOrEmail()
		{
			ZValidationInternals.Validate(Parent.ToFaxOrEmailInfo, CheckToFaxOrEmail);
		}

		public void CheckToFaxOrEmail()
		{
			if (Parent.CanSetEmail)
			{
				EmailAddressValidation.ValidateEmailAddressesAsString(Parent.ToFaxOrEmailInfo, StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress);
			}
			else
			{
				ZString countryCode = ZString.Empty;
				if (Parent.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
				{
					countryCode = Parent.Staff?.GS_RN_NKCountryCode ?? ZString.Empty;
				}
				else if (Parent.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
				{
					countryCode = Parent.Header?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
				}
				PhoneNumberFormatterAndValidator.Validate(Parent.ToFaxOrEmailInfo, null, null, countryCode);
			}
		}

		protected override void CheckS6_SQ()
		{
			base.CheckS6_SQ();
			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
			{
				MandatoryValidation.CheckEntered(Parent.S6_SQInfo);
				ListValidation.ErrorIfInvalidPK(Parent.S6_SQInfo, Parent.Lookups.Printers);
			}
		}

		protected override void CheckS6_FtpAddress()
		{
			base.CheckS6_FtpAddress();

			if (Parent.IsFtpDeliverMode)
			{
				MandatoryValidation.CheckEntered(Parent.S6_FtpAddressInfo, Res.GetString("CAF80B10-A683-4AE3-A6CA-958F43EAB744", "FTP Address e.g.: {0}", "ftp://10.10.10.10"));
			}
		}

		protected override void CheckS6_DeliveryMethod()
		{
			if (Parent.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc)
			{
				return;
			}

			base.CheckS6_DeliveryMethod();

			if (Parent.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				var collection = ((IBusinessObjectInternals)Parent).ParentCollections;
				if (collection != null && collection.Length > 0)
				{
					var recipients = collection[0] as ReportScheduleTaskRecipientDependentCollection;

					long registrySizeLimitInBytes = (long)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value * 1024 * 1024;

					if (recipients != null)
					{
						var command = Parent.Factory.Load<ReportCommand>(recipients.RptScheduleTask.S5_ParentID);
						if (command != null)
						{
							using (var pack = new DocumentPack(command))
							{
								var attachment = pack.GetFirstReport() as IDeliveryEmailAttachment;
								if (attachment?.FileSizeInBytes > registrySizeLimitInBytes)
								{
									Parent.S6_DeliveryMethodInfo.AddError(Res.GetString("9507893D-BCF5-46B9-B7FB-6EFEFC390E3D",
										"The report exceeds the {0}MB attachment limit and cannot be sent. The limit is defined in the Registry at {1}.",
										SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value,
										((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location));
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckS6_EmailFromAddress()
		{
			base.CheckS6_EmailFromAddress();

			if (!Parent.S6_EmailFromAddress.IsEmpty && Parent.EmailFromAddressList.GetCodeFromDescription(Parent.S6_EmailFromAddress) == null)
			{
				Parent.S6_EmailFromAddressInfo.AddError(Res.GetString("5AC8EAAA-8E4D-4A04-BE4F-408D482BBBFA", "This email address is not on the selected Print User's staff record."));
			}
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk = new (() => new PhoneNumberFormatterAndValidator());
	}
}
