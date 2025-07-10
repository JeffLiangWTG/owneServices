using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	abstract class MaybeMutatingDependencySubMenuItemBase : ZMenuItem
	{
		protected MaybeMutatingDependencySubMenuItemBase(ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy, string text)
			: base(text)
		{
			SourceWorkflow = sourceWorkflow;
			Strategy = strategy;
			Name = GetType().Name;

			Click += MenuItem_Click;
		}

		protected ProcessHeader SourceWorkflow { get; }
		protected DependencyMenuItemStrategy Strategy { get; }

		void MenuItem_Click(object sender, EventArgs e)
		{
			OnMenuItemClicked();

			if (Strategy.RefreshOpenPrerequisitesAfterEdits)
			{
				SourceWorkflow.RefreshOpenPrerequisiteStatus();
			}
		}

		protected abstract void OnMenuItemClicked();
	}
}
