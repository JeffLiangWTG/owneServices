#if DEBUG

using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalDocumentSupporter
	{
		public DocumentWrapper[] GetDocumentWrappersInternal_ForTestOnly(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}
	}
}

#endif
