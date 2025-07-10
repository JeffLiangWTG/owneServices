using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class ComponentUserControl : ZUserControl
	{
#if DEBUG
		/// <summary>
		/// For the form designer only
		/// </summary>
		public ComponentUserControl()
		{
		}

#endif

		public ComponentUserControl(BMBoardSectionViewModel viewModel)
		{
			this.viewModel = viewModel;

			viewModel.FilterManager.FiltersUpdated += Filters_FiltersUpdated;
		}

		public BMBoardSectionViewModel ViewModel
		{
			get { return viewModel; }
		}

		readonly BMBoardSectionViewModel viewModel;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				viewModel.FilterManager.FiltersUpdated -= Filters_FiltersUpdated;
			}
		}

		void Filters_FiltersUpdated(object sender, FiltersChangedEventArgs e)
		{
			OnFiltersChanged(e);
		}

		protected virtual void OnFiltersChanged(FiltersChangedEventArgs e)
		{
		}

		public bool TryReallocateTask(TaskCardControl taskCard)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": TryReallocateTask" };
			var section = factory.Load<BMBoardSection>(ViewModel.SectionPK);

			if (section.SectionConfiguration.IsReleaseScheduler)
			{
				Globals.Message.Show(Res.GetString("9c5c6067-6981-4178-8093-74f209119247", "Task cards cannot be dragged into Release Scheduler board sections."));
				return false;
			}

			var currentMousePosition = GetNewLocationOfTaskCard(taskCard);
			var modifiedWorkflowPKs = TryReallocateTaskCore(factory, taskCard, currentMousePosition).Select(s => s.PK).ToList();

			if (modifiedWorkflowPKs.Count > 0)
			{
				if (BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value)
				{
					var subscriber = viewModel.BoardViewModel.SlideShowViewModel.DataRefreshBusSubscriber;
					if (subscriber is VisualBoardDataRefreshBusSubscriber visualBoardDataRefreshSubscriber)
					{
						visualBoardDataRefreshSubscriber.AddExtraWorkflows(modifiedWorkflowPKs);
					}

					factory.SaveHandlingZSaveExceptions();
				}
				else
				{
					factory.SaveHandlingZSaveExceptions();
					ViewModel.RefreshAll(new WorkflowUpdatedOperation(System.Array.Empty<ZGuid>(), modifiedWorkflowPKs.Distinct().ToArray(), factory));
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual IEnumerable<ProcessHeader> TryReallocateTaskCore(BusinessObjectFactory factory, TaskCardControl taskCard, Point currentMousePosition)
		{
			yield return taskCard.CardContent.GetWorkflow(factory);
		}

		Point GetNewLocationOfTaskCard(TaskCardControl taskCard)
		{
			if (Globals.IsTest)
			{
				return taskCard.Location;
			}
			else
			{
				return FindForm().PointToClient(Cursor.Position);
			}
		}
	}
}
