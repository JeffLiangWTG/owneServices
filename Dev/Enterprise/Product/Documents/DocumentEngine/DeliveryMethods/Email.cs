using System;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	class Email : OnlineDeliveryBase
	{
		public Email(DocDeliveryContact contact)
			: base(contact)
		{
			this.contact = contact;
			this.Address = contact.DeliveryAddress;
		}

		readonly DocDeliveryContact contact;

		public readonly string Address;

		#region Implementation

		protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo info)
		{
			base.SetAdditionalProperties(printJob, info);

			printJob.EmailToRecipients.DeleteAll();
			printJob.CarbonCopyRecipients.DeleteAll();
			printJob.BlindCarbonCopyRecipients.DeleteAll();

			if (contact.DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint)
			{
				if (!contact.EPrintEmail.IsEmpty)
				{
					printJob.EmailToRecipients.Value = contact.EPrintEmail;
				}
				else
				{
					throw new EmailHasNoRecipientsException(Res.GetString("99034FB3-1A2C-4347-8056-596CA944F84F", "No ePrint Email Address for [ {0} ]. A ePrint Email Address must be specified when using a delivery method of '{1}'.Please go to the Registry > Documents > ePrint Email Address set configs.", info.DocumentPack?.StmMenuCommand?.SU_MenuName, Core.Constants.ContactNotifyModes.EPrint));
				}
			}
			else
			{
				CopyEmailRecipients(contact.EmailToRecipients, printJob.EmailToRecipients);
				CopyEmailRecipients(contact.EmailCarbonCopyRecipients, printJob.CarbonCopyRecipients);
				CopyEmailRecipients(contact.EmailBlindCarbonCopyRecipients, printJob.BlindCarbonCopyRecipients);
			}

			if (Instructions != null && Instructions.IncludeCoverNote)
			{
				CreateCoverNote(printJob);
			}

			SetAdditionalPropertiesForReportRun(printJob, info);

			ReportContactEmailEmptyErrorIfNeeded(printJob, info);
		}

		void SetAdditionalPropertiesForReportRun(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			if (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.Value
				&& contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Email
				&& deliveryInfo.Instructions != null
				&& deliveryInfo.Instructions.DocPack != null)
			{
				var report = deliveryInfo.Instructions.DocPack.GetFirstReport();
				if (report != null && report.IsScheduledReport && report.stmReportRun != null)
				{
					var attachmentType = string.Empty;
					if (Contact != null && !string.IsNullOrEmpty(Contact.AttachmentTypeWithFormatSwitching))
					{
						attachmentType = Contact.AttachmentTypeWithFormatSwitching;
					}
					var formatType = GetEmailAttachmentFormat(deliveryInfo, attachmentType);
					printJob.SP_ParentTableName = report.stmReportRun.TableName;
					printJob.SP_ParentGuid = report.stmReportRun.PK;
					printJob.SP_RelatedBusinessContext = report.stmReportRun.DocManagerInfo().DocManagerCode;
					printJob.SP_EmailAttachmentFormat = formatType;
					printJob.SP_DocumentType = (report.MenuItem != null && report.MenuItem.SU_IsSystemDefined && report.DocumentTypeCode.IsEmpty)
						? Core.Constants.RefDocTypes.ScheduledReport
						: report.DocumentTypeCode.ToString();
				}
			}
		}

		void ReportContactEmailEmptyErrorIfNeeded(StmPrintJob printJob, DeliveryInfo info)
		{
			if (string.IsNullOrEmpty(contact.Email) && contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				ErrorReporter.ReportOnce("ContactEmailIsEmpty_WhenDeliveryMethodIsEmail", FormattableString.Invariant($@"An Email was created without any recipients.
Email Subject:{printJob.SP_EmailSubjectLine}
Parent Table:{printJob.SP_ParentTableName}
Parent PK:{printJob.SP_ParentGuid}
Delivery Method:{nameof(Email)}
Contact Name:{contact.Name}
OrgHeader:{contact.OrgHeader?.OH_Code}
IsSystemDefaultContact:{contact.IsSystemDefaultContact}
Document:{info.DocumentPack?.StmMenuCommand?.SU_MenuName}
Path:{info.DocumentPack?.StmMenuCommand?.SU_MenuPath}
DocumentGroup:{info.DocumentPack?.DocumentGroup}
UpdateEmailAndFax:{contact.UpdateEmailAndFax}
Contact PK:{contact.Contact?.PK}
Contact Email:{contact.Contact?.OC_Email}
OrgAddress PK:{contact.OrgAddress?.PK}
OrgAddress Email:{contact.OrgAddress?.OA_Email}"));
			}
		}

		void CopyEmailRecipients(NonPersistentCopyRecipientCollection from, StmPrintJobCopyRecipientCollection to)
		{
			foreach (NonPersistentCopyRecipient nonPersistentCopyRecipient in from)
			{
				var emailToRecipient = to.AddNew();
				emailToRecipient.SPR_RecipientType = nonPersistentCopyRecipient.Type;
				emailToRecipient.SPR_EmailAddress = nonPersistentCopyRecipient.EmailAddress;
			}
		}

		protected override PrintType PrintType
		{
			get { return PrintType.EML; }
		}

		protected virtual void CreateCoverNote(StmPrintJob printJob)
		{
			StmNote note = printJob.Notes.AddNew();
			note.ST_NoteType = nameof(StmNoteVisibility.PRV);
			note.ST_NoteText = Instructions.CoverNote;
		}

		#endregion
	}
}
