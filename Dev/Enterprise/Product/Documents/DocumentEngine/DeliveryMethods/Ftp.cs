using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	class Ftp : OnlineDeliveryBase
	{
		public Ftp(DocDeliveryContact docContact, ZGuid parentGuid)
			: base(docContact)
		{
			this.parentGuid = parentGuid;
		}

		readonly ZGuid parentGuid;

		protected override PrintType PrintType
		{
			get { return PrintType.FTP; }
		}

#if DEBUG
		internal
#endif
		const string ScheduledReportFtp = "SRF";

		protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			base.SetAdditionalProperties(printJob, deliveryInfo);
			printJob.SP_DocumentName = SubstringSafe(deliveryInfo.AttachedFilename, AutoStmPrintJob.Schema.SP_DocumentNameMaxLength);
			printJob.SP_FaxDestination = Contact.FileLocation.SubstringSafe(0, StmPrintJobSchema.SP_FaxDestination.MaxLength);
			printJob.SP_EmailFromAddress = Contact.UserName + (NoResString)"\0" + Contact.Password;
			printJob.SP_ParentGuid = parentGuid;
			printJob.SP_ParentTableName = "StmScheduleTask";
			printJob.SP_RelatedBusinessContext = ScheduledReportFtp;
		}

		static string SubstringSafe(string input, int length)
		{
			if (input != null && input.Length > length)
			{
				return input.Substring(0, length);
			}
			return input;
		}
	}
}
