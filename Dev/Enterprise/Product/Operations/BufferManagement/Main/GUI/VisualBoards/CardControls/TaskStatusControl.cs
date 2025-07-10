using System;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskStatusControl : ZUserControl
	{
		public TaskStatusControl()
		{
			InitializeComponent();
		}

		public TaskStatusControl(ITaskCardComponentParent parent)
			: this(parent, StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.SaveAndClose)
		{
		}

		public TaskStatusControl(ITaskCardComponentParent parent, string playButtonBehaviour, string suspendButtonBehaviour, string closeTaskButtonBehaviour)
			: this()
		{
			this.parent = parent;

			AddButtons(playButtonBehaviour, suspendButtonBehaviour, closeTaskButtonBehaviour);
			parent.StatusUpdated += parent_StatusUpdated;
			shouldShowActualTimeEditWhenStatusChangesToClose = closeTaskButtonBehaviour == StatusButtonBehaviorOptionsList.Codes.Manual;
		}

		void parent_StatusUpdated(object sender, EventArgs e)
		{
			UpdateActualTimeEdit();
		}

		readonly ITaskCardComponentParent parent;

		public new ProcessTask DataSource
		{
			get { return (ProcessTask)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!IsDisposed && !Disposing)
			{
				UpdateActualTimeEdit();
				UpdateButtonGraphics();
			}
		}

		void UpdateActualTimeEdit()
		{
			var dataSource = DataSource;
			if (dataSource != null)
			{
				ActualTimeEdit.Visible = dataSource.P9_Status == ProcessTaskStatusCodeList.Codes.Closed &&
					(shouldShowActualTimeEditWhenStatusChangesToClose || parent.Task.P9_ActualDurationInfo.HasError(ProcessTaskValidation.ActualDurationErrorMessage));
			}
		}

		readonly bool shouldShowActualTimeEditWhenStatusChangesToClose;

		void UpdateButtonGraphics()
		{
			foreach (GenericStatusChangeButton button in Buttons)
			{
				button.UpdateButtonGraphics();
			}
		}

		void AddButtons(string playButtonBehaviour, string suspendButtonBehaviour, string closeTaskButtonBehaviour)
		{
			ClosedStatusButton = GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.CompletedStatusButton, parent, 185, 0, closeTaskButtonBehaviour);
			SuspendedStatusButton = GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.SuspendStatusButton, parent, 160, 0, suspendButtonBehaviour);
			WorkingStatusButton = GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.WorkingStatusButton, parent, 135, 0, playButtonBehaviour);

			Buttons = new GenericStatusChangeButton[] { ClosedStatusButton, SuspendedStatusButton, WorkingStatusButton };

			this.Controls.AddRange(Buttons);
		}

		public GenericStatusChangeButton ClosedStatusButton;
		public GenericStatusChangeButton SuspendedStatusButton;
		public GenericStatusChangeButton WorkingStatusButton;
		GenericStatusChangeButton[] Buttons;
	}
}
