#if DEBUG

using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class ConsolBatchInvoicingPostManagerGUIWrapper
	{
		public ZString CurrentObjectCode_ForTestOnly => CurrentObjectCode;

		public ZString DefaultPostedObjectName_ForTestOnly => DefaultPostedObjectName;

		public IJobCostingPlugIn[] ConsolCollection_ForTestOnly => ConsolCollection;

		public void ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting_ForTestOnly(object sender, Business.JobInvoicing.ExportAgentPostingEventArgs e)
		{
			ConsolInvoicingPostManagerGUIWrapper_ExportAgentPosting(sender, e);
		}

		public DocumentCommand ProfitShareMenuItem_ForTestOnly => ProfitShareMenuItem;
	}
}

#endif
