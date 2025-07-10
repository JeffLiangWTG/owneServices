using System;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	class ValidateWorkflowLoopsMenuItem : ZMenuItem
	{
		readonly IProgressReporterProvider progressReporterProvider;

		public ValidateWorkflowLoopsMenuItem(ZGrid parentGrid)
			: base(ResString.GetMultilingualString("BufferManagement.Workflow.ValidateWorkflowLoops", "Validate Workflow Loops"))
		{
			Click += ValidateWorkflowLoopsMenuItem_Click;

			progressReporterProvider = new DefaultProgressReporterProvider(parentGrid.FindForm());

			this.parentGrid = parentGrid;
		}

		readonly ZGrid parentGrid;

		void ValidateWorkflowLoopsMenuItem_Click(object sender, EventArgs e)
		{
			if (parentGrid.SelectedElements.Any())
			{
				new ProcessHeaderLoopsChecker().CheckLoops(progressReporterProvider,
					parentGrid.SelectedElements.Select(o => o as ProcessHeader).ToArray());
			}
			else
			{
				new ProcessHeaderLoopsChecker().CheckLoops(progressReporterProvider,
					new ProcessHeader[] { parentGrid.GetCurrent() as ProcessHeader });
			}
		}
	}
}
