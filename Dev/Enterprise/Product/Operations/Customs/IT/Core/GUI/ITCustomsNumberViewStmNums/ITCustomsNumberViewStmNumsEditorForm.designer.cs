namespace Enterprise.Customs.IT.GUI
{
	partial class ITCustomsNumberViewStmNumsEditorForm
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.YearOfApplicabilityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AppliesToDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TopPanel.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AppliesToDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 33, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.AppliesToDropEdit);
			this.MainPanel.Controls.Add(this.YearOfApplicabilityCalcEdit);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 164, true);
			this.MainPanel.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.FountainNameTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.ValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MinimumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.CountCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MaximumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.YearOfApplicabilityCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.AppliesToDropEdit, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 197, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 30, true);
			// 
			// FountainNameTextBox
			// 
			this.FountainNameTextBox.Enabled = false;
			this.FountainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 168, true);
			this.FountainNameTextBox.TabIndex = 7;
			// 
			// ValueCalcEdit
			// 
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
			this.ValueCalcEdit.TabIndex = 3;
			// 
			// MinimumValueCalcEdit
			// 
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 107, true);
			this.MinimumValueCalcEdit.TabIndex = 4;
			// 
			// MaximumValueCalcEdit
			// 
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 168, true);
			this.MaximumValueCalcEdit.TabIndex = 6;
			// 
			// CountCalcEdit
			// 
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 133, true);
			this.CountCalcEdit.TabIndex = 5;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 227, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.ITCustomsNumberViewStmNumsWrapper);
			// 
			// YearOfApplicabilityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.YearOfApplicabilityCalcEdit, "YearOfApplicability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.Business.ITCustomsNumberViewStmNumsWrapper)(null)).YearOfApplicability)));
			this.YearOfApplicabilityCalcEdit.CaptionResourceString = null;
			this.YearOfApplicabilityCalcEdit.DecimalPlaces = 2;
			this.YearOfApplicabilityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 29, true);
			this.YearOfApplicabilityCalcEdit.Name = "YearOfApplicabilityCalcEdit";
			this.YearOfApplicabilityCalcEdit.ShowGroupSeparators = false;
			this.YearOfApplicabilityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.YearOfApplicabilityCalcEdit.TabIndex = 1;
			this.YearOfApplicabilityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AppliesToDropEdit
			// 
			this.AppliesToDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AppliesToDropEdit, "AppliesTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.ITCustomsNumberViewStmNumsWrapper)(null)).AppliesTo)));
			this.AppliesToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 55, true);
			this.AppliesToDropEdit.Name = "AppliesToDropEdit";
			this.AppliesToDropEdit.PreBoundMaxLength = 16;
			this.AppliesToDropEdit.ShowDescriptionBox = false;
			this.AppliesToDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.AppliesToDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.AppliesToDropEdit.TabIndex = 2;
			// 
			// ITCustomsNumberViewStmNumsEditorForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 251, true);
			this.DataSourceType = typeof(Enterprise.Customs.IT.Business.ITCustomsNumberViewStmNumsWrapper);
			this.Name = "ITCustomsNumberViewStmNumsEditorForm";
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AppliesToDropEdit.ResumeLayout(true);
			this.AppliesToDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit YearOfApplicabilityCalcEdit;
		private ZArchitecture.GUI.ZDropEdit AppliesToDropEdit;
	}
}
