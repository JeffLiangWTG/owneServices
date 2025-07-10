using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	class CancelWorkflowMenuItem : ZMenuItem
	{
		public CancelWorkflowMenuItem(ZGrid parentGrid)
			: base(ResString.GetMultilingualString("BufferManagement.Workflow.Cancel", "Cancel"))
		{
			Click += CancelMenuItem_Click;

			this.parentGrid = parentGrid;
		}

		readonly ZGrid parentGrid;

		void CancelMenuItem_Click(object sender, EventArgs e)
		{
			using (var cancelReasonForm = new CancelWorkflowForm())
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(cancelReasonForm);
				var reason = cancelReasonForm.CancellationReasonText;

				if (result == DialogResult.OK)
				{
					var currentlySelectedRowCount = parentGrid.CurrentRowIndex;

					var elementsToCancel = new List<BusinessObject>();

					if (parentGrid.ListManager != null && currentlySelectedRowCount < parentGrid.ListManager.Count)
					{
						elementsToCancel.Add((BusinessObject)parentGrid.ListManager.GetCurrent());
					}

					foreach (var element in IEnumerableExtensions.DistinctBy(parentGrid.SelectedElements.Union(elementsToCancel).WhereNotNull(), b => b.PK))
					{
						Cancel(element, reason);
					}
				}
			}
		}

		void Cancel(BusinessObject source, ZString reason)
		{
			var workflow = source as ProcessHeader;
			var scheduleDeactivator = new ScheduleDeactivator();
			if (workflow != null && scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(workflow))
			{
				workflow.CancelAllTasksAndCompletionStatements(reason);
			}
		}
	}
}
