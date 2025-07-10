
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Integration
{
	public interface IInvoiceBulkOperationWorkflowFilterStripsHelper : IFilterStripsHelper
	{
		void AddRelatedParentJoiningQuery(ZDBOnlySubQuery query);
	}
}
