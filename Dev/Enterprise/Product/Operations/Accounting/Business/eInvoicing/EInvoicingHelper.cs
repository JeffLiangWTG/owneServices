using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class EInvoicingHelper : IEInvoicingHelper
	{
		public bool HasActiveEInvoicingTransactionPivot(IAccTransactionHeader transaction, string actionType)
		{
			var header = Argument.NotNull(transaction as AccTransactionHeader, nameof(transaction));
			return header.Factory.LoadTop1<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, header.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded)) != null;
		}
	}
}
