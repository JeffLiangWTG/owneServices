using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.Module
{
	public partial class ProcessHeaderFilterControl : ZFilterStripControl
	{
		public ProcessHeaderFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveNewReleaseGateColumnsIfDisabledInRegistry();
		}

		void RemoveNewReleaseGateColumnsIfDisabledInRegistry()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
				{
					var columnInfos = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_IsApproved"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "DedicatedBufferName"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveBranchCode"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveDepartmentCode"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GB_Branch"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GE_Department"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_BMT_BufferTimespan"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveBufferDurationString"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "LatestAcceptableReleaseDateUtcString"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_EffectiveAgreedDeliveryDateUtc"));
					grid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_ReleaseSequenceSortDateUtc"));
				}
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ProcessHeaderFilterStrip();
		}
	}
}
