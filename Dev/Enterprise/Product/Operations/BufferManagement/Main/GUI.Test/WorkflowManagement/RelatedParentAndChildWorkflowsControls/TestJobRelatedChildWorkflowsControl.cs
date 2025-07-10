using System;
using System.Collections.Generic;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class TestJobRelatedParentWorkflowsControl : JobRelatedParentWorkflowsControl
	{
		protected override void ChooseProcessHeadersAndAddLinks(Action<IReadOnlyCollection<ProcessHeader>> handler)
		{
			handler(SelectedProcessHeadersToAddLink);
		}

		public List<ProcessHeader> SelectedProcessHeadersToAddLink = new List<ProcessHeader>();
	}
}
