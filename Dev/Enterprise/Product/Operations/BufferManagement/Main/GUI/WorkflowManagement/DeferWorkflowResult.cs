using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class DeferWorkflowResult
	{
		internal DeferWorkflowResult(DialogResult dialogResult, ICollection<ProcessHeader> workflowsDeferred)
		{
			DialogResult = dialogResult;
			WorkflowsDeferred = workflowsDeferred;
		}

		public DialogResult DialogResult { get; private set; }
		public ICollection<ProcessHeader> WorkflowsDeferred { get; private set; }
	}
}
