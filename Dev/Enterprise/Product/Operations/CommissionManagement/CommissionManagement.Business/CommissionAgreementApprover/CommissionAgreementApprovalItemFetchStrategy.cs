using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalItemFetchStrategy : BusinessObjectFetchStrategy
	{
		public CommissionAgreementApprovalItemFetchStrategy(CommissionAgreementApprovalItem approvalItem)
			: base(approvalItem)
		{
		}

		#region Fetch For View

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var approvalItem = (CommissionAgreementApprovalItem)BusinessObject;

			var agreementColumns =
				from column in columns
				where column.ColumnName.StartsWith("CommissionAgreement+", StringComparison.OrdinalIgnoreCase)
				let agreementColumnName = column.ColumnName.Substring("CommissionAgreement+".Length)
				select new TableColumn(OrgCommissionAgreementSchema.Constants.TableName, agreementColumnName);

			approvalItem.CommissionAgreement.FetchStrategy.FetchForView(agreementColumns.ToArray());
		}

		#endregion
	}
}
