using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockSolutionEvents : MarshalByRefObject, EnvDTE.SolutionEvents
	{
		public event EnvDTE._dispSolutionEvents_AfterClosingEventHandler AfterClosing;
		public void FireAfterClosing()
		{
			AfterClosing?.Invoke();
		}

		public event EnvDTE._dispSolutionEvents_BeforeClosingEventHandler BeforeClosing;
		public void FireBeforeClosing()
		{
			BeforeClosing?.Invoke();
		}

		public event EnvDTE._dispSolutionEvents_OpenedEventHandler Opened;
		public void FireOpened()
		{
			Opened?.Invoke();
		}

		public event EnvDTE._dispSolutionEvents_ProjectAddedEventHandler ProjectAdded;
		public void FireProjectAdded(EnvDTE.Project project)
		{
			ProjectAdded?.Invoke(project);
		}

		public event EnvDTE._dispSolutionEvents_ProjectRemovedEventHandler ProjectRemoved;
		public void FireProjectRemoved(EnvDTE.Project project)
		{
			ProjectRemoved?.Invoke(project);
		}

		public event EnvDTE._dispSolutionEvents_ProjectRenamedEventHandler ProjectRenamed;
		public void FireProjectRenamed(EnvDTE.Project project, string oldName)
		{
			ProjectRenamed?.Invoke(project, oldName);
		}

		public event EnvDTE._dispSolutionEvents_QueryCloseSolutionEventHandler QueryCloseSolution;
		public void FireQueryCloseSolution(ref bool cancel)
		{
			QueryCloseSolution?.Invoke(ref cancel);
		}

		public event EnvDTE._dispSolutionEvents_RenamedEventHandler Renamed;
		public void FireRenamed(string oldName)
		{
			Renamed?.Invoke(oldName);
		}
	}
}
