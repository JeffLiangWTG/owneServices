using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing.FaxRouting
{
	class InternetFaxDeliverer : IFaxDeliverer
	{
		public bool Deliver(StmPrintJob fax)
		{
			Mailer.Fax.FaxDef faxDoc = Mailer.Fax.FaxDef.New();
			faxDoc.FaxNumber = fax.SP_FaxDestination;
			faxDoc.FaxFileName = fax.StoredAttachmentFilename;
			faxDoc.SysId = "ediEnterprise";
			faxDoc.SendingCompanyCode = fax.Branch.Company.GC_Code;
			faxDoc.SysFaxJobId = fax.PK.ToString();
			faxDoc.Send();

			fax.SP_JobType = nameof(PrintJobType.FAA);

			return true;
		}
	}
}
