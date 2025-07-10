namespace Enterprise.Customs.DE.GUI
{
	public partial class TaxChangeAssessmentForm
	{
		new void InitializeComponent()
		{
			this.TaxChangeAssessmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MaturityDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TaxChangeAssessmentGroupBox.SuspendLayout();
			this.IssueDateEdit.SuspendLayout();
			this.MaturityDateEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 412, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1A86ED1C-4EE8-4585-86C5-F82D633B78C5", "Assessment");
			this.MainTabPage.Controls.Add(this.TaxChangeAssessmentGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 385, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 385, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 385, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 412, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.TaxChangeAssessment);
			// 
			// TaxChangeAssessmentGroupBox
			// 
			this.TaxChangeAssessmentGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9E383686-7087-47C2-960F-744B44805C5B", "Tax Change Assessment");
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.TypeTextBox);
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.ReferenceTextBox);
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.LRNTextBox);
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.IssueDateEdit);
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.MaturityDateEdit);
			this.TaxChangeAssessmentGroupBox.Controls.Add(this.StatusDropEdit);
			this.TaxChangeAssessmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxChangeAssessmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxChangeAssessmentGroupBox.Name = "TaxChangeAssessmentGroupBox";
			this.TaxChangeAssessmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 385, true);
			this.TaxChangeAssessmentGroupBox.TabIndex = 0;
			this.TaxChangeAssessmentGroupBox.TabStop = false;
			// 
			// TypeTextBox
			// 
			this.TypeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeTextBox, "Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).Type)));
			this.TypeTextBox.CaptionResourceString = null;
			this.TypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 34, true);
			this.TypeTextBox.Name = "TypeTextBox";
			this.TypeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TypeTextBox.TabIndex = 0;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).ReferenceNumber)));
			this.ReferenceTextBox.CaptionResourceString = null;
			this.ReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 60, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ReferenceTextBox.TabIndex = 1;
			// 
			// LRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNTextBox, "LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).LocalReferenceNumber)));
			this.LRNTextBox.CaptionResourceString = null;
			this.LRNTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 86, true);
			this.LRNTextBox.Name = "LRNTextBox";
			this.LRNTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LRNTextBox.TabIndex = 2;
			// 
			// IssueDateEdit
			// 
			this.IssueDateEdit.AllowDrop = true;
			this.IssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.IssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.IssueDateEdit, "IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).IssueDate)));
			this.IssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 112, true);
			this.IssueDateEdit.Name = "IssueDateEdit";
			this.IssueDateEdit.TabIndex = 3;
			// 
			// MaturityDateEdit
			// 
			this.MaturityDateEdit.AllowDrop = true;
			this.MaturityDateEdit.AutoCompleteMonthThreshold = 1;
			this.MaturityDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MaturityDateEdit, "MaturityDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).MaturityDate)));
			this.MaturityDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 138, true);
			this.MaturityDateEdit.Name = "MaturityDateEdit";
			this.MaturityDateEdit.TabIndex = 4;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.TaxChangeAssessment)(null)).EntryStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 164, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.ShowDescriptionBox = false;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.StatusDropEdit.TabIndex = 5;
			// 
			// TaxChangeAssessmentForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 468, true);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.TaxChangeAssessment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			this.Name = "TaxChangeAssessmentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TaxChangeAssessmentGroupBox.ResumeLayout(false);
			this.TaxChangeAssessmentGroupBox.PerformLayout();
			this.IssueDateEdit.ResumeLayout(true);
			this.IssueDateEdit.PerformLayout();
			this.MaturityDateEdit.ResumeLayout(true);
			this.MaturityDateEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox TaxChangeAssessmentGroupBox;
		Enterprise.ZArchitecture.ZTextBox TypeTextBox;
		Enterprise.ZArchitecture.ZTextBox ReferenceTextBox;
		Enterprise.ZArchitecture.ZTextBox LRNTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit IssueDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit MaturityDateEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
	}
}
