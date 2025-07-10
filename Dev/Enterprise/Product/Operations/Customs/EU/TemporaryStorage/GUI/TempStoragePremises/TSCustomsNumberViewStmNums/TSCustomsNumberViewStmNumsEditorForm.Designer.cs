using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TSCustomsNumberViewStmNumsEditorForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.PrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaddingCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.PrefixTextBox);
			this.MainPanel.Controls.Add(this.PaddingCalcEdit);
			this.MainPanel.Controls.Add(this.SuffixTextBox);
			this.MainPanel.Controls.Add(this.IsActiveCheckBox);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 252, true);
			this.MainPanel.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.SuffixTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.PaddingCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.PrefixTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.FountainNameTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.ValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MinimumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.CountCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MaximumValueCalcEdit, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 30, true);
			// 
			// FountainNameTextBox
			// 
			this.FountainNameTextBox.Enabled = false;
			this.FountainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 168, true);
			this.FountainNameTextBox.TabIndex = 7;
			// 
			// ValueCalcEdit
			// 
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 185, true);
			this.ValueCalcEdit.TabIndex = 15;
			// 
			// MinimumValueCalcEdit
			// 
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 29, true);
			// 
			// MaximumValueCalcEdit
			// 
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 81, true);
			// 
			// CountCalcEdit
			// 
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 55, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 282, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TSCustomsNumberViewStmNumsWrapper);
			// 
			// PrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrefixTextBox, "NumberPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TSCustomsNumberViewStmNumsWrapper)(null)).NumberPrefix)));
			this.PrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 107, true);
			this.PrefixTextBox.Name = "PrefixTextBox";
			this.PrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.PrefixTextBox.TabIndex = 12;
			this.PrefixTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PaddingCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaddingCalcEdit, "NumberPadding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TSCustomsNumberViewStmNumsWrapper)(null)).NumberPadding)));
			this.PaddingCalcEdit.DecimalPlaces = 0;
			this.PaddingCalcEdit.Decimals = 0;
			this.PaddingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 133, true);
			this.PaddingCalcEdit.MaxValue = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.PaddingCalcEdit.Name = "PaddingCalcEdit";
			this.PaddingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.PaddingCalcEdit.TabIndex = 13;
			this.PaddingCalcEdit.Text = "0";
			this.PaddingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaddingCalcEdit.TrackDisposedAccess = true;
			this.PaddingCalcEdit.AllowNegative = false;
			// 
			// SuffixTextBox
			// 
			this.BindingSource.SetBindingMember(this.SuffixTextBox, "NumberSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TSCustomsNumberViewStmNumsWrapper)(null)).NumberSuffix)));
			this.SuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 159, true);
			this.SuffixTextBox.Name = "SuffixTextBox";
			this.SuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.SuffixTextBox.TabIndex = 14;
			this.SuffixTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((TSCustomsNumberViewStmNumsWrapper)(null)).IsActive)));
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 211, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 24, true);
			this.IsActiveCheckBox.TabIndex = 15;
			// 
			// TSCustomsNumberViewStmNumsEditorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 306, true);
			this.DataSourceType = typeof(TSCustomsNumberViewStmNumsWrapper);
			this.Name = "TSCustomsNumberViewStmNumsEditorForm";
			this.Text = "TSCustomsNumberViewStmNumsEditorForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox PrefixTextBox;
		internal ZArchitecture.ZCalcEdit PaddingCalcEdit;
		internal ZArchitecture.ZTextBox SuffixTextBox;
		internal ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
	}
}
