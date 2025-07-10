using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.WorkflowManagement;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class MultiJobHeaderEditorUserControl : ZUserControl
	{
		public MultiJobHeaderEditorUserControl()
		{
			InitializeComponent();

			DateAcceptabilityControl.SetDataBinding(DataSource, nameof(MultiJobHeaderEditorViewModel.FH_DateAcceptability));
		}

		protected new MultiJobHeaderEditorViewModel DataSource
		{
			get { return (MultiJobHeaderEditorViewModel)base.DataSource; }
		}

		internal ZGrid SchedulesGrid => SchedulesZGrid;

		#region Mass Update

		void UpdateESDButton_Click(object sender, EventArgs e)
		{
			MassUpdate(MultiJobHeaderEditorViewModel.SetProperty.EarliestStartDate);
		}

		void UpdateADDButton_Click(object sender, EventArgs e)
		{
			MassUpdate(MultiJobHeaderEditorViewModel.SetProperty.AgreedDeliveryDate);
		}

		void UpdateDateAcceptabilityButton_Click(object sender, EventArgs e)
		{
			MassUpdate(MultiJobHeaderEditorViewModel.SetProperty.FH_DateAcceptability);
		}

		void MassUpdate(MultiJobHeaderEditorViewModel.SetProperty property)
		{
			PerformUpdateAction(() => DataSource.MassUpdate(GetSelectedRows(), property));
		}

		void IncrementESDButton_Click(object sender, EventArgs e)
		{
			MassUpdateIncrementalValues(MultiJobHeaderEditorViewModel.SetProperty.EarliestStartDate);
		}

		void IncrementADDButton_Click(object sender, EventArgs e)
		{
			MassUpdateIncrementalValues(MultiJobHeaderEditorViewModel.SetProperty.AgreedDeliveryDate);
		}

		void MassUpdateIncrementalValues(MultiJobHeaderEditorViewModel.SetProperty dateProperty)
		{
			PerformUpdateAction(() => DataSource.MassUpdateIncrementalValues(GetSelectedRows(), dateProperty));
		}

		JobHeaderView[] GetSelectedRows()
		{
			return SchedulesZGrid.GetSelectedElements<JobHeaderView>().ToArray();
		}

		void PerformUpdateAction(Action action)
		{
			DataSource.Validation.ValidateAll();

			if (DataSource.Notifications.Any(n => n.Type == CargoWise.ComponentModel.NotificationType.Error))
			{
				((MultiJobHeaderEditorForm)FindForm()).ShowErrorsDialog();
			}
			else if (GetSelectedRows().Length == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("0822f06e-2037-4a51-89d3-dca0436c66bd", "Please select one or more rows in the grid."));
			}
			else
			{
				action();
			}
		}

		public void SchedulesZGrid_DoubleClick(object sender, EventArgs e)
		{
			var selectedItem = SchedulesZGrid.ListManager.GetCurrent() as JobHeaderView;
			var processHeader = selectedItem.ProcessHeader;
			var formGetter = new WorkflowParentFormGetter();

			formGetter.GetForm(processHeader, null);
		}

		#endregion
	}
}
