namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZDateRangeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDateEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDateEdit();
			this.PropertySearchDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			this.FromOffsetHoursTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ToOffsetHoursTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.FromOffsetDaysCalcEdit = new ZCalcEdit();
			this.ToOffsetDaysCalcEdit = new ZCalcEdit();
			this.FilterOptionDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToDateEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.PropertySearchDropEdit.SuspendLayout();
			this.FilterOptionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.ToDateEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZDateRangeControl|51df9c84-a77e-4a3a-b856-4a83cf50d135", "To");
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 25, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 5;
			this.ToDateEdit.Visible = false;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.FromDateEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZDateRangeControl|3cebc90b-93a8-415c-a023-41f5f747da29", "From");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 25, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 3;
			this.FromDateEdit.Visible = false;
			// 
			// PropertySearchDropEdit
			// 
			this.PropertySearchDropEdit.AllowDrop = true;
			this.PropertySearchDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PropertySearchDropEdit, false);
			this.PropertySearchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 3, true);
			this.PropertySearchDropEdit.MaxItemsToShowInDropDown = 28;
			this.PropertySearchDropEdit.Name = "PropertySearchDropEdit";
			this.PropertySearchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.PropertySearchDropEdit.TabIndex = 0;
			// 
			// FromOffsetHoursTimeEditEx
			// 
			this.FromOffsetHoursTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.FromOffsetHoursTimeEditEx.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("718bb480-7f44-42c5-aa1d-0133500f1a51", "At least");
			this.FromOffsetHoursTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 25, true);
			this.FromOffsetHoursTimeEditEx.Name = "FromOffsetHoursTimeEditEx";
			this.FromOffsetHoursTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.FromOffsetHoursTimeEditEx.TabIndex = 6;
			this.FromOffsetHoursTimeEditEx.Visible = false;
			// 
			// ToOffsetHoursTimeEditEx
			// 
			this.ToOffsetHoursTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.ToOffsetHoursTimeEditEx.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ef7aa62d-25cc-4a8e-b522-75e854f60918", "At most");
			this.ToOffsetHoursTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 25, true);
			this.ToOffsetHoursTimeEditEx.Name = "ToOffsetHoursTimeEditEx";
			this.ToOffsetHoursTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ToOffsetHoursTimeEditEx.TabIndex = 7;
			this.ToOffsetHoursTimeEditEx.Visible = false;
			// 
			// FromOffsetDaysCalcEdit
			// 
			this.FromOffsetDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.FromOffsetDaysCalcEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("792d8eba-4051-4d68-8209-a31981d87376", "At least");
			this.FromOffsetDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 25, true);
			this.FromOffsetDaysCalcEdit.Name = "FromOffsetDaysCalcEdit";
			this.FromOffsetDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.FromOffsetDaysCalcEdit.TabIndex = 6;
			this.FromOffsetDaysCalcEdit.Visible = false;
			// 
			// ToOffsetDaysCalcEdit
			// 
			this.ToOffsetDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.ToOffsetDaysCalcEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("bff525f6-e587-4fc4-8e78-2c098e1a3a8a", "At most");
			this.ToOffsetDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 25, true);
			this.ToOffsetDaysCalcEdit.Name = "ToOffsetDaysCalcEdit";
			this.ToOffsetDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ToOffsetDaysCalcEdit.TabIndex = 7;
			this.ToOffsetDaysCalcEdit.Visible = false;
			// 
			// FilterOptionDropEdit
			// 
			this.FilterOptionDropEdit.AllowDrop = true;
			this.FilterOptionDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("09479a90-2a1b-4288-a773-87cdd95e838d", "In the");
			this.FilterOptionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 25, true);
			this.FilterOptionDropEdit.MaxItemsToShowInDropDown = 28;
			this.FilterOptionDropEdit.Name = "FilterOptionDropEdit";
			this.FilterOptionDropEdit.PreBoundMaxLength = 3;
			this.FilterOptionDropEdit.ShowDescriptionBox = false;
			this.FilterOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.FilterOptionDropEdit.TabIndex = 8;
			this.FilterOptionDropEdit.Visible = false;
			// 
			// ZDateRangeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterOptionDropEdit);
			this.Controls.Add(this.ToOffsetHoursTimeEditEx);
			this.Controls.Add(this.FromOffsetHoursTimeEditEx);
			this.Controls.Add(this.FromOffsetDaysCalcEdit);
			this.Controls.Add(this.ToOffsetDaysCalcEdit);
			this.Controls.Add(this.PropertySearchDropEdit);
			this.Controls.Add(this.ToDateEdit);
			this.Controls.Add(this.FromDateEdit);
			this.Name = "ZDateRangeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.PropertySearchDropEdit.ResumeLayout(true);
			this.PropertySearchDropEdit.PerformLayout();
			this.FilterOptionDropEdit.ResumeLayout(true);
			this.FilterOptionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZFilterStripDropEdit PropertySearchDropEdit;
		public ZFilterStripDateEdit FromDateEdit;
		public ZFilterStripDateEdit ToDateEdit;
		private ZTimeEditEx FromOffsetHoursTimeEditEx;
		private ZTimeEditEx ToOffsetHoursTimeEditEx;
		private ZCalcEdit FromOffsetDaysCalcEdit;
		private ZCalcEdit ToOffsetDaysCalcEdit;
		public ZFilterStripDropEdit FilterOptionDropEdit;
	}
}
