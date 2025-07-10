using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Async;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskCardDetailControl : ZUserControl, ISaveableControl
	{
		public TaskCardDetailControl()
		{
			InitializeComponent();
		}

		public TaskCardDetailControl(ProcessTask task, ProcessHeader workflow, TaskCardControl parentCard, BMControlCustomisation detailedCardLayout, BMBoardSectionViewModel boardSectionViewModel)
		{
			InitializeComponent();

			this.parentCard = parentCard;
			parentCard.Disposed += ParentCard_Disposed;
			ProcessTask = task;

			var parentCardContent = parentCard.CardContent;
			var cardContent = parentCardContent.CardType == CardType.Task ? new TaskCardContent(ProcessTask, boardSectionViewModel) : (ICardContent)new WorkflowCardContent(workflow, ProcessTask, boardSectionViewModel);
			customisationViewModel = new ControlCustomisationViewModel(detailedCardLayout, boardSectionViewModel, cardContent, parentCard.Cell, ProcessTask);
			SetupControls();

#if !WINZOR
			DragDropHelper.AddDragDropSupport(this);
#endif
			SetDisposeOnDelete();
		}

		readonly TaskCardControl parentCard;
		readonly ControlCustomisationViewModel customisationViewModel;

		public ProcessTask ProcessTask
		{
			get { return (ProcessTask)base.DataSource; }
			private set { SetDataBinding(value, ""); }
		}

		#region Setup

		void SetupControls()
		{
			customisationViewModel.ParentShown += CustomisationViewModel_ParentShown;
			customisationViewModel.Saved += CustomisationViewModel_Saved;
			customisationViewModel.Closed += CustomisationViewModel_Closed;
			customisationViewModel.CannotUpdateStatus += CustomisationViewModel_CannotUpdateStatus;
			customisationViewModel.AssignedStaffCode = ProcessTask.P9_GS_NKAssignedStaffMember;

			var control = CustomisedControlRenderer.Render(customisationViewModel);

			Size = control.Size;
			Controls.Add(control);
			control.Dock = DockStyle.Fill;
		}

		void SetDisposeOnDelete()
		{
			ProcessTask.DeletedByDataRefresh += Task_DeletedByDataRefresh;
		}

		void Task_DeletedByDataRefresh(object sender, EventArgs e)
		{
			MainThreadRunner.RunOnMainThread(() => this.Dispose());
		}

		#endregion

		#region ITaskCardComponentParent Actions

		void CustomisationViewModel_ParentShown(object sender, ParentShownEventArgs e)
		{
			parentCard.ShowParentWorkflow();
		}

		void ISaveableControl.Save()
		{
			var saveButton = this.FindAll<SaveButton>().FirstOrDefault();
			if (saveButton != null)
			{
				saveButton.Focus();
				customisationViewModel.Save(saveButton);
			}
		}

		void CustomisationViewModel_Saved(object sender, TasksSavedArgs e)
		{
			if (!parentCard.ViewModel.DataRefreshBusSubscriberRefreshesTickets) // Otherwise DataRefreshBus will keep our tickets in line.
			{
				parentCard.ReloadAndRedraw(e.GetTasks());
			}
		}

		void CustomisationViewModel_Closed(object sender, EventArgs e)
		{
			Close();
		}

		void CustomisationViewModel_CannotUpdateStatus(object sender, EventArgs e)
		{
			var caption = Res.GetString("aaae099a-7593-4772-86a5-0274ee1eaa79", "This task is already assigned to {0}.", ProcessTask.P9_GS_NKAssignedStaffMember);
			var message = Res.GetString("cdf445ea-e00d-4efc-ae52-c18a74f70125", "This task has already been assigned");

			Globals.Message.Show(caption, message, ZMessageBoxButtons.OK, ZDialogResult.OK);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (ProcessTask != null)
					{
						ProcessTask.DeletedByDataRefresh -= Task_DeletedByDataRefresh;
					}

					ProcessTask = null;

					if (parentCard != null)
					{
						parentCard.Disposed -= ParentCard_Disposed;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		void ParentCard_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		#endregion

		#region User actions

		public void Close()
		{
			if (!IsDisposed)
			{
				var task = ProcessTask;
				if (!task.IsDeleted && (task.HasChanges || (task.ProcessHeader != null && task.ProcessHeader.HasChanges)))
				{
					var result = UserNotification.Show(Res.GetString("b97723ba-91bc-4402-9f24-247da3ddf80c", "This record has been modified.\r\nWould you like to save the changes?"), Res.GetString("b4d37186-68b5-47db-977c-bdb612e4f221", "Unsaved Changes"), MessageBoxButtons.YesNoCancel, DialogResult.No);
					switch (result)
					{
						case DialogResult.Cancel:
							return;
						case DialogResult.Yes:
							customisationViewModel.Save();
							return;
					}

					task.CancelChanges();
					task.HasChanges = false;
				}

				Dispose();
			}
		}

		#endregion

		public IUserNotification UserNotification
		{
			get { return userNotification ?? Globals.Message; }
			set { userNotification = value; }
		}
		IUserNotification userNotification;
	}
}
