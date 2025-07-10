
using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SchedulablePeriodRangeEdit : ZUserControl
	{
		public SchedulablePeriodRangeEdit()
		{
			InitializeComponent();
		}

		[AttributeProvider("Enterprise.ZArchitecture.BindToPropertyAttributes, Enterprise.ZArchitecture.GUI", "BindTo")]
		public string BindToLow
		{
			get { return LowValuePeriodEdit.BindTo; }
			set { LowValuePeriodEdit.BindTo = value; }
		}

		[AttributeProvider("Enterprise.ZArchitecture.BindToPropertyAttributes, Enterprise.ZArchitecture.GUI", "BindTo")]
		public string BindToHigh
		{
			get { return HighValuePeriodEdit.BindTo; }
			set { HighValuePeriodEdit.BindTo = value; }
		}

		public AccPeriodSchedule LowSchedule
		{
			get { return LowValuePeriodEdit.Schedule; }
		}

		public AccPeriodSchedule HighSchedule
		{
			get { return HighValuePeriodEdit.Schedule; }
		}

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				LowValuePeriodEdit.ReadOnly = value;
				HighValuePeriodEdit.ReadOnly = value;
			}
		}

		public void SetSchedules(AccPeriodSchedule lowSchedule, AccPeriodSchedule highSchedule)
		{
			LowValuePeriodEdit.SetSchedule(lowSchedule);
			HighValuePeriodEdit.SetSchedule(highSchedule);
		}

		void SchedulablePeriodRangeEdit_Resize(object sender, EventArgs e)
		{
			UpdateLayout();
		}

		void UpdateLayout()
		{
			ControlDpiScalingHelper.SetHeight(this, HighValuePeriodEdit.Bottom, false);
			ControlDpiScalingHelper.SetWidth(this, HighValuePeriodEdit.Right, false);
		}

		bool readOnly;
	}
}
