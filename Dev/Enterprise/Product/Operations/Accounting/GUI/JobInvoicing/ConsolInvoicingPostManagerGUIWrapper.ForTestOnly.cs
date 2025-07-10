#if DEBUG

using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolInvoicingPostManagerGUIWrapper
	{
		public void ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting_ForTestOnly(object sender, Business.JobInvoicing.ExportAgentPostingEventArgs e)
		{
			ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting(sender, e);
		}

		public DocumentCommand ProfitShareMenuItem_ForTestOnly => ProfitShareMenuItem;

		public void ConsolInvoicingPostManagerGUIWrapper_IncorrectRegistrySetup_ForTestOnly(object sender, Business.JobInvoicing.IncorrectRegistrySetupEventArgs e)
		{
			ConsolInvoicingPostManagerGUIWrapper_IncorrectRegistrySetup(sender, e);
		}

		public IJobCostingPlugIn Consol_ForTestOnly => Consol;
	}
}

#endif
