#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class JobBatchInvoicingPostManagerGUIWrapper
	{
		public ZString DefaultPostedObjectName_ForTestOnly => DefaultPostedObjectName;

		public ZString CurrentObjectCode_ForTestOnly => CurrentObjectCode;

		public Business.JobInvoicing.Job[] JobCollection_ForTestOnly => JobCollection;
	}
}

#endif
