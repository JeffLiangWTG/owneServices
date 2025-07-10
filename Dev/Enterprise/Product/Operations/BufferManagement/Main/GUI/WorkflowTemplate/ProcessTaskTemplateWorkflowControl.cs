using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ProcessTaskTemplateWorkflowControl : ZUserControl, IProcessTaskTemplateWorkflowControl
	{
		public ProcessTaskTemplateWorkflowControl()
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
					var columnInfos = WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_IsApproved"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_DeadlineType"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_EffectiveNudge"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GB_Branch"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GE_Department"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_BMT_BufferTimespan"));
				}
			}
		}

		void WorkflowsGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (e.Objects.OfType<IProcessJobHeader>().Any())
			{
				Globals.Message.ShowError(Res.GetString("7f90a86c-06d0-44a8-a57c-4f64494924ec", "The Job-level workflow cannot be deleted."));
				e.Cancel = true;
			}
		}
	}
}
