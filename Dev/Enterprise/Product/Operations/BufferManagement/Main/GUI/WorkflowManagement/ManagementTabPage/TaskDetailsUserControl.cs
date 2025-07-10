using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.RichEdit;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskDetailsUserControl : ZUserControl, IWorkflowTasksControl, ITaskDetailsControl
	{
		public TaskDetailsUserControl()
		{
			InitializeComponent();
			SetupContextMenu();
			SelectionManager = new GridSelectionManager(TasksGrid, () => ParentForm, true);
			TasksGrid.ContextMenu.Popup += OnContextMenu_Popup;

			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			TasksGrid.Entering += (_, __) => processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid);
			TasksGrid.Leave += (_, __) => processTaskStatusChangeModeTracker.Clear();
		}

		protected override void OnLoad(EventArgs e)
		{
			SpellChecker.InitialiseSpellcheck(NotesRichTextBox, nameof(TaskDetailsUserControl) + nameof(NotesRichTextBox));

			base.OnLoad(e);
		}

		#region Context Menu

		void SetupContextMenu()
		{
			var cloneMenuItem = new CloneMenuItem(TasksGrid, () => Tasks);
			var taskNotesMenuItem = new ZMenuItem(Res.GetString("33eb988d-7e2f-4fdd-9096-1d64958841c4", "Notes"), TaskNotes_Click);

			TasksGrid.ContextMenu.MenuItems.Add(cloneMenuItem);
			TasksGrid.ContextMenu.MenuItems.Add(taskNotesMenuItem);
		}

		void OnContextMenu_Popup(object sender, EventArgs e)
		{
			RefreshAssistMenuItemsIfApplicable();
		}

		void RefreshAssistMenuItemsIfApplicable()
		{
			var menuItems = TasksGrid.ContextMenu.MenuItems;

			var assistWithThisTaskMenuItem = menuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
			var addAssistanceTaskForMenuItem = menuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();

			if (assistWithThisTaskMenuItem != null)
			{
				menuItems.Remove(assistWithThisTaskMenuItem);
			}

			if (addAssistanceTaskForMenuItem != null)
			{
				menuItems.Remove(addAssistanceTaskForMenuItem);
			}

			var selectedTask = AssistWithThisTaskMenuItem.GetSingleSelectedTask(TasksGrid);

			if (AssistWithThisTaskMenuItem.ShouldAddMenuItem(selectedTask))
			{
				menuItems.Add(new AssistWithThisTaskMenuItem(selectedTask, () => Tasks));
			}

			if (AddAssistanceTaskForMenuItem.ShouldAddMenuItem(selectedTask))
			{
				menuItems.Add(new AddAssistanceTaskForMenuItem(selectedTask,
						() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(selectedTask as ProcessTask, Tasks),
						() => AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(selectedTask as ProcessTask, Tasks),
						() => Tasks));
			}
		}

		public class TaskCustomZGrid : ZGrid
		{
			protected override IImportCollectionInfoProvider GetImportCollectionInfoProvider()
			{
				return new ImportCollectionInfoProviderWrapper(base.GetImportCollectionInfoProvider());
			}

			class ImportCollectionInfoProviderWrapper : IImportCollectionInfoProvider
			{
				readonly IImportCollectionInfoProvider importCollectionInfoProvider;

				public ImportCollectionInfoProviderWrapper(IImportCollectionInfoProvider importCollectionInfoProvider)
				{
					this.importCollectionInfoProvider = importCollectionInfoProvider;
				}

				public IImportCollectionInfo ImportCollectionInfo => importCollectionInfoProvider.ImportCollectionInfo;

				public string ContextKey => "BMSTaskGrid";
			}

			protected override void OnEnter(EventArgs e)
			{
				// Entering the task grid when the job has no workflows (and no tasks) yet defined
				// will create an uncommitted task, which in turn will create a default workflow for it.
				// The new workflow may be inserted at the top of the workflow list (depending on grid sorting)
				// which changes the current workflow for this grid and cancels the uncommitted task.
				// This grid ends up trying to edit a non-existent row.
				// To avoid this, we create the default workflow first.
				var view = ListManager?.List as ProcessHeaderProcessTaskCollectionView;
				if (view != null)
				{
					var jobHeader = view.ProcessHeader as ProcessJobHeader;
					if (jobHeader != null && jobHeader.ProcessHeaders.Count == 0)
					{
						// add the default ProcessHeader if none
						ProcessJobHeader.GetForParent(jobHeader.Parent, jobHeader.Factory, true);
					}
				}

				base.OnEnter(e);
			}
		}

		void TaskNotes_Click(object sender, EventArgs e)
		{
			var selectedTask = TasksGrid.ListManager.GetCurrent() as ProcessTask;
			if (selectedTask != null)
			{
				using (var form = new ZRichTextBoxPopupForm(this))
				{
#if !WINZOR
					form.Rtf = selectedTask.P9_NotesAsString;
#else
					form.Html = selectedTask.P9_NotesAsString;
#endif
					ZFormModaliser.ShowDialogWithoutDispose(form);

					#region For Test
#if DEBUG
					OnShowingNotesForm(form);
#endif
					#endregion

#if !WINZOR
					selectedTask.P9_NotesAsString = form.Rtf;
#else
					selectedTask.P9_NotesAsString = form.Html;
#endif
				}
			}
		}

		#region For Test
#if DEBUG
		protected virtual void OnShowingNotesForm(ZRichTextBoxPopupForm form)
		{
		}
#endif
		#endregion

		#endregion

		#region ZUserControl Overrides

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			rebuildHook?.Dispose();
			rebuildHook = null;
			base.SetDataBinding(dataSource, dataMember);

			var taskCollection = GetWorkflowItems(dataSource, dataMember);

			if (taskCollection != null)
			{
				rebuildHook = this.SetupTasksControl(taskCollection, saveOrder: true);
			}
		}

		IDisposable rebuildHook;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (TasksGrid?.ListManager != null)
			{
				TasksGrid.ListManager.ListChanged += SelectionManager.ListChanged;
				TasksGrid.AfterBind += (sender, args) => SelectionManager.ReselectItem();
			}
		}

		internal GridSelectionManager SelectionManager { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				rebuildHook?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Tasks
		internal ProcessTaskCollection Tasks
		{
			get { return GetTasks(BindingSource.DataSource, BindingSource.DataMember); }
		}

		WorkflowItemCollectionView GetWorkflowItems(object dataSource, string dataMember)
		{
			CurrencyManager cm = dataSource == null ? null : (CurrencyManager)BindingContext[dataSource, new KBindingMemberInfo(dataMember, TasksGrid.BindTo).BindingMember];
			WorkflowItemCollectionView workflowProvider = cm == null ? null : cm.List as WorkflowItemCollectionView;
			return workflowProvider;
		}

		ProcessTaskCollection GetTasks(object dataSource, string dataMember)
		{
			return ((IWorkflowProvider)GetWorkflowItems(dataSource, dataMember))?.WorkflowItems;
		}

#if DEBUG
		public
#endif

 void TasksGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GridEntityFormOpener.OpenFormForSavedParent((ZForm)ParentForm, TasksGrid, e, ControllerIDs.ProcessTasks);
		}

		#endregion

		#region IWorkflowTasksControl Members

		KSplitContainer IWorkflowTasksControl.TasksHintSplitContainer => TasksHintSplitContainer;

		ZLabel IWorkflowTasksControl.TasksHintLabel => TasksHintLabel;

		ZGrid IWorkflowItemsControl.TasksGrid => TasksGrid;

		ZRichTextBox ITaskDetailsControl.NotesRichTextBox => NotesRichTextBox;

		#endregion
	}
}
