#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Module
{
	public partial class BulkPostingModuleHelper
	{
		public SecurityCheckpoint[] GetSecurityCheckPoints_ForTestOnly(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements)
		{
			return GetSecurityCheckPoints(postingOption, selectedElements);
		}
	}
}

#endif
