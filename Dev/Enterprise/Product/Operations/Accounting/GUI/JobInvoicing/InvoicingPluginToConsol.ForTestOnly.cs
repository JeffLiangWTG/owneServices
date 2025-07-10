#if DEBUG

using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicingPluginToConsol
	{
		public DocumentCommand ProfitShareMenuItem_ForTestOnly => ProfitShareMenuItem;

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;

		public void DiscardCurrentUserControl_ForTestOnly()
		{
			DiscardCurrentUserControl();
		}

		public DocumentPack GetDocumentPackForOrg_ForTestOnly(PrintTask task, DocumentCommand menuItem, OrgHeader organisation)
		{
			return GetDocumentPackForOrg(task, menuItem, organisation);
		}
	}
}

#endif
