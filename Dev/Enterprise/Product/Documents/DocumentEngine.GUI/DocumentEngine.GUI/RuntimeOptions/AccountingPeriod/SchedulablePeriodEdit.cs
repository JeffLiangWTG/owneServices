
using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SchedulablePeriodEdit : ZUserControl, IBindTo
	{
		public SchedulablePeriodEdit()
		{
			InitializeComponent();
		}

		[AttributeProvider("Enterprise.ZArchitecture.BindToPropertyAttributes, Enterprise.ZArchitecture.GUI", nameof(BindTo))]
		public string BindTo
		{
			get { return ValuePeriodEdit.BindTo; }
			set { ValuePeriodEdit.BindTo = value; }
		}

		public AccPeriodSchedule Schedule
		{
			get { return schedule; }
		}

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				ValuePeriodEdit.ReadOnly = value;
				EditButton.ReadOnly = value;
			}
		}

		public void SetSchedule(AccPeriodSchedule value)
		{
			schedule = value;
			bool hasSchedule = (schedule != null);
			EditButton.Visible = hasSchedule;
			UpdateLayout(hasSchedule);
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AccPeriodScheduleForm(schedule));
		}

		void SchedulablePeriodEdit_Resize(object sender, EventArgs e)
		{
			UpdateLayout(EditButton.Visible);
		}

		void UpdateLayout(bool editButtonVisible)
		{
			if (!suspendResizing)
			{
				suspendResizing = true;
				try
				{
					ControlDpiScalingHelper.SetHeight(this, editButtonVisible ? EditButton.Bottom : ValuePeriodEdit.Bottom, false);
					ControlDpiScalingHelper.SetWidth(this, editButtonVisible ? EditButton.Right : EditButton.Left, false);
				}
				finally
				{
					suspendResizing = false;
				}
			}
		}

		AccPeriodSchedule schedule;
		bool suspendResizing;
		bool readOnly;
	}
}
