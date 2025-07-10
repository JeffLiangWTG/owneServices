#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class BatchPostingBusinessObject
	{
		public ZString PostOperationDescription_ForTestOnly => PostOperationDescription;
	}
}

#endif
