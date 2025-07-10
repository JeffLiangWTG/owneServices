using System.Globalization;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	/// <summary>
	/// This delivery method will just copy a document to DocManager.
	/// </summary>
	sealed class DocManager : QueuedForBatchProcessor, ISupportCreateDeliveryInfoStrategyFactory
	{
		public DocManager(DocDeliveryContact contact = null)
		{
			Contact = contact;
		}
		readonly DocDeliveryContact Contact;

		public string AttachmentType
		{
			get
			{
				if (Contact != null)
				{
					var attachmentType = Contact.AttachmentTypeWithFormatSwitching;
					if (!string.IsNullOrEmpty(attachmentType))
					{
						return attachmentType;
					}
				}

				return string.Empty;
			}
		}

		protected override bool ConsolidateReports => Contact == null ? base.ConsolidateReports : !Contact.SendIndividually;

		protected override PrintType PrintType => PrintType.DDS;

		protected override void SetAdditionalProperties(Scheduler.Business.StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			if (deliveryInfo.Instructions != null && deliveryInfo.Instructions.DocPack != null)
			{
				var report = deliveryInfo.Instructions.DocPack.GetFirstReport();

				if (report != null)
				{
					var formatType = GetEmailAttachmentFormat(deliveryInfo, AttachmentType);
					printJob.SP_EmailAttachmentFormat = formatType;
					if (report.IsScheduledReport && report.stmReportRun != null)
					{
						var orgHeaderCode = deliveryInfo.Instructions.OverriddenValueForOrgLookupFilter.IsValid
							? deliveryInfo.Instructions.Recipients.OfType<DocDeliveryContact>().FirstOrDefault().OrgHeader?.OH_Code.ToString()
							: string.Empty;
						var subjectLine = string.Format(CultureInfo.InvariantCulture, "{0}{1} ({2})", printJob.SP_EmailSubjectLine, (string.IsNullOrEmpty(orgHeaderCode) ? string.Empty : " " + orgHeaderCode), formatType);

						printJob.SP_ParentTableName = report.stmReportRun.TableName;
						printJob.SP_ParentGuid = report.stmReportRun.PK;
						printJob.SP_RelatedBusinessContext = report.stmReportRun.DocManagerInfo().DocManagerCode;
						printJob.SP_DocumentType = (report.MenuItem != null && report.MenuItem.SU_IsSystemDefined && report.DocumentTypeCode.IsEmpty)
							? Core.Constants.RefDocTypes.ScheduledReport
							: report.DocumentTypeCode.ToString();
						printJob.SP_EmailSubjectLine = subjectLine;
					}
				}
			}
		}
	}
}
