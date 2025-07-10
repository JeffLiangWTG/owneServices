
namespace Enterprise.Registry.GUI
{
	partial class MinimumIntervalSubLedgerTakeUpControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.IntervalTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.MinimumIntervalSubLedgerTakeUp);
			//
			// GroupBox
			//
			this.GroupBox.Controls.Add(this.IntervalTypeDropEdit);
			this.GroupBox.Controls.Add(this.IntervalCalcEdit);
			this.GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("MinimumIntervalSubLedgerTakeUpControl|f3e9f983-df8b-4c0a-a16d-b7d31c098615", "Minimum Interval for Sub Ledger Take Up");
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 147, true);
			this.GroupBox.TabIndex = 0;
			this.GroupBox.TabStop = false;
			//
			// IntervalTypeDropEdit
			//
			this.IntervalTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntervalTypeDropEdit, "IntervalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.MinimumIntervalSubLedgerTakeUp)(null)).IntervalType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.MinimumIntervalSubLedgerTakeUp)(null)).IntervalTypeList)));
			this.IntervalTypeDropEdit.BindToList = "IntervalTypeList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IntervalTypeDropEdit, false);
			this.IntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 17, true);
			this.IntervalTypeDropEdit.Name = "IntervalTypeDropEdit";
			this.IntervalTypeDropEdit.ShowDescriptionBox = false;
			this.IntervalTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.IntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 18, true);
			this.IntervalTypeDropEdit.TabIndex = 2;
			//
			// IntervalCalcEdit
			//
			this.BindingSource.SetBindingMember(this.IntervalCalcEdit, "Interval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.MinimumIntervalSubLedgerTakeUp)(null)).Interval)));
			this.IntervalCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("MinimumIntervalSubLedgerTakeUpControl|2c09a379-021e-41ef-8ac9-092fff45c889", "Interval");
			this.IntervalCalcEdit.DecimalPlaces = 0;
			this.IntervalCalcEdit.Decimals = 0;
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 17, true);
			this.IntervalCalcEdit.Name = "IntervalCalcEdit";
			this.IntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.IntervalCalcEdit.TabIndex = 1;
			this.IntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// MinimumIntervalSubLedgerTakeUpControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "MinimumIntervalSubLedgerTakeUpControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.IntervalTypeDropEdit.ResumeLayout(true);
			this.IntervalTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox GroupBox;
		ZArchitecture.GUI.ZDropEdit IntervalTypeDropEdit;
		ZArchitecture.ZCalcEdit IntervalCalcEdit;
	}
}
