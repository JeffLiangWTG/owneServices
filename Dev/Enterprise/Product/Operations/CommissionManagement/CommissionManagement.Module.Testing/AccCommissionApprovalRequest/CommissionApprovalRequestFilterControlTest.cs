using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Core.Forms;

namespace Enterprise.CommissionManagement.Module
{
	class CommissionApprovalRequestFilterControlTest : TestCaseWithFactory
	{
		public void TestAuditDetailColumnsVisibleAtEndByDefault()
		{
			var collection = new AccCommissionApprovalRequestCollection(Factory);
			var strip = new CommissionApprovalRequestFilterBusinessObject();

			using (var control = new CommissionApprovalRequestFilterControl(collection, strip))
			{
				var columnStyles = control.Grid.ColumnStyles;
				var last4Columns = columnStyles.Cast<ZGridColumnInfo>().Skip(columnStyles.Count - 4);

				AssertArrayEqualsByElements(
				new[]
				{
					AccCommissionApprovalRequest.Schema.CRQ_SystemCreateUser,
					AccCommissionApprovalRequest.Schema.CRQ_SystemCreateTimeUtc,
					AccCommissionApprovalRequest.Schema.CRQ_SystemLastEditUser,
					AccCommissionApprovalRequest.Schema.CRQ_SystemLastEditTimeUtc,
				},
				last4Columns.Select(x => x.ColumnName).ToArray());
			}
		}
	}
}
