using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ViewCommissionLineFetchStrategy(ViewCommissionLine commissionLine)
			: base(commissionLine)
		{
		}

		new ViewCommissionLine BusinessObject
		{
			get { return (ViewCommissionLine)base.BusinessObject; }
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(col => col.ColumnName.StartsWith("CommissionHeader")))
			{
				Factory.AddFetchHint(AccCommissionHeaderSchema.PK, BusinessObject.VCL_CH0);
			}

			if (columns.Any(col => col.ColumnName.StartsWith("ChargeCode")))
			{
				Factory.AddFetchHint(AccChargeCodeSchema.PK, BusinessObject.VCL_AC);
			}

			if (columns.Any(col => col.ColumnName.StartsWith("ApprovalRequest")))
			{
				Factory.AddFetchHint(AccCommissionApprovalRequestItemSchema.CRI_CL0, BusinessObject.PK);
			}
		}

		#endregion
	}
}
