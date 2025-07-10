using System;
using System.Linq;
using Enterprise.CommissionManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.Module
{
	public partial class CommissionApprovalRequestFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public CommissionApprovalRequestFilterControl()
		{
			InitializeComponent();
		}

		public CommissionApprovalRequestFilterControl(AccCommissionApprovalRequestCollection collection, CommissionApprovalRequestFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();

			MakeAuditDetailColumnsVisibleAtEnd();
		}

		void MakeAuditDetailColumnsVisibleAtEnd()
		{
			var auditColumns = Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.GroupName.Equals(FilterStripAuditDetails.AuditDetailsGroupText)).ToArray();
			foreach (var auditColumn in auditColumns)
			{
				Grid.ColumnStyles.Remove(auditColumn);
				auditColumn.IsVisible = true;
			}

			Grid.ColumnStyles.AddRange(auditColumns);
		}
	}
}
