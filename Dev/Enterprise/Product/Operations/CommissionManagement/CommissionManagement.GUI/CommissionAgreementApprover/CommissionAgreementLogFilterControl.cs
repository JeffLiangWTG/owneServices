using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionAgreementLogFilterControl : ZStmALogFilterControl
	{
		public CommissionAgreementLogFilterControl(IStmALogParent master, IBusinessObjectCollection collection, FilterStripBusinessObject filterStripBusinessObject, ZStmALogModule module)
			: base(master, collection, filterStripBusinessObject, module)
		{
			InitializeComponent();

			SetAvailableColumns(Grid, new[] { StmALog.Schema.SL_EventTime, StmALog.Schema.SL_PostedTimeUtc, StmALog.Schema.SL_TableFriendlyNameForBinding, "SL_ReferenceForBinding", StmALog.Schema.SL_UserNameAndInitials });
		}

		static void SetAvailableColumns(ZGrid grid, params string[] availableColumns)
		{
			var availableColumnsSet = new HashSet<string>(availableColumns);
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles)
				{
					grid.SetAvailability(availableColumnsSet.Contains(columnStyle.ColumnName), columnStyle.ColumnName);
				}
			}
		}
	}
}
