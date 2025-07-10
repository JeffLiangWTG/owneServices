namespace Enterprise.Customs.GB.GUI.CDSDIS
{
	partial class CDSDISQueryForm
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
			this.components = new System.ComponentModel.Container();
			this.RequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageOwnerTextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicationReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationCategoryTextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DateFrom = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateTo = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeclarationStatusTextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PageNumber = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.ResponseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageIntepretationHTML = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RequestGroupBox.SuspendLayout();
			this.MessageOwnerTextBox.SuspendLayout();
			this.DeclarationCategoryTextBox.SuspendLayout();
			this.DateFrom.SuspendLayout();
			this.DateTo.SuspendLayout();
			this.DeclarationStatusTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PageNumber)).BeginInit();
			this.PageNumber.SuspendLayout();
			this.ResponseGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(889, 419, true);
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f2ad4357-cba3-4036-9287-1fbbc09cbfcb", "Query Message");
			this.MainTabPage.Controls.Add(this.RequestGroupBox);
			this.MainTabPage.Controls.Add(this.ResponseGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 396, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 396, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 396, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(889, 419, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 5, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(889, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.CDS.CDSDISQueryMessage);
			// 
			// RequestGroupBox
			// 
			this.RequestGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RequestGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CDSDISQueryForm|RequestGroupBox", "Request");
			this.RequestGroupBox.Controls.Add(this.MessageOwnerTextBox);
			this.RequestGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.RequestGroupBox.Controls.Add(this.MessageNumberTextBox);
			this.RequestGroupBox.Controls.Add(this.ApplicationReferenceTextBox);
			this.RequestGroupBox.Controls.Add(this.DeclarationCategoryTextBox);
			this.RequestGroupBox.Controls.Add(this.DateFrom);
			this.RequestGroupBox.Controls.Add(this.DateTo);
			this.RequestGroupBox.Controls.Add(this.DeclarationStatusTextBox);
			this.RequestGroupBox.Controls.Add(this.PageNumber);
			this.RequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 9, true);
			this.RequestGroupBox.Name = "RequestGroupBox";
			this.RequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 132, true);
			this.RequestGroupBox.TabIndex = 0;
			this.RequestGroupBox.TabStop = false;
			// 
			// MessageOwnerTextBox
			// 
			this.MessageOwnerTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageOwnerTextBox, "EM_MessageOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).EM_MessageOwner)));
			this.MessageOwnerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 24, true);
			this.MessageOwnerTextBox.Name = "MessageOwnerTextBox";
			this.MessageOwnerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.MessageOwnerTextBox.TabIndex = 1;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "EM_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).EM_Status)));
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(761, 49, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.ReadOnly = true;
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 18, true);
			this.MessageStatusTextBox.TabIndex = 4;
			// 
			// MessageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageNumberTextBox, "EM_MessageNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).EM_MessageNum)));
			this.MessageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(761, 24, true);
			this.MessageNumberTextBox.Name = "MessageNumberTextBox";
			this.MessageNumberTextBox.ReadOnly = true;
			this.MessageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 18, true);
			this.MessageNumberTextBox.TabIndex = 3;
			// 
			// ApplicationReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApplicationReferenceTextBox, "EM_ApplicationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).EM_ApplicationReference)));
			this.ApplicationReferenceTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CDSDISQueryForm|ApplicationReference", "Reference");
			this.ApplicationReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 49, true);
			this.ApplicationReferenceTextBox.Name = "ApplicationReferenceTextBox";
			this.ApplicationReferenceTextBox.ReadOnly = true;
			this.ApplicationReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 18, true);
			this.ApplicationReferenceTextBox.TabIndex = 2;
			// 
			// DeclarationCategoryTextBox
			// 
			this.DeclarationCategoryTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationCategoryTextBox, "DeclarationCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).DeclarationCategory)));
			this.DeclarationCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 76, true);
			this.DeclarationCategoryTextBox.Name = "DeclarationCategoryTextBox";
			this.DeclarationCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.DeclarationCategoryTextBox.TabIndex = 5;
			// 
			// DateFrom
			// 
			this.DateFrom.AllowDrop = true;
			this.DateFrom.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateFrom, "DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).DateFrom)));
			this.DateFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 76, true);
			this.DateFrom.Name = "DateFrom";
			this.DateFrom.TabIndex = 6;
			// 
			// DateTo
			// 
			this.DateTo.AllowDrop = true;
			this.DateTo.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateTo, "DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).DateTo)));
			this.DateTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 76, true);
			this.DateTo.Name = "DateTo";
			this.DateTo.TabIndex = 7;
			// 
			// DeclarationStatusTextBox
			// 
			this.DeclarationStatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationStatusTextBox, "DeclarationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).DeclarationStatus)));
			this.DeclarationStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 102, true);
			this.DeclarationStatusTextBox.Name = "DeclarationStatusTextBox";
			this.DeclarationStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.DeclarationStatusTextBox.TabIndex = 8;
			this.DeclarationStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// PageNumber
			// 
			this.BindingSource.SetBindingMember(this.PageNumber, "PageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).PageNumber)));
			this.PageNumber.BindTo = "PageNumber";
			this.PageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 102, true);
			this.PageNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.PageNumber.Name = "PageNumber";
			this.PageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
			this.PageNumber.TabIndex = 9;
			this.PageNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// ResponseGroupBox
			// 
			this.ResponseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResponseGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CDSDISQueryForm|ResponseGroupBox", "Response");
			this.ResponseGroupBox.Controls.Add(this.MessageIntepretationHTML);
			this.ResponseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 146, true);
			this.ResponseGroupBox.Name = "ResponseGroupBox";
			this.ResponseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 219, true);
			this.ResponseGroupBox.TabIndex = 5;
			this.ResponseGroupBox.TabStop = false;
			// 
			// MessageIntepretationHTML
			// 
			this.MessageIntepretationHTML.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageIntepretationHTML, "ResponseInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.CDS.CDSDISQueryMessage)(null)).ResponseInterpretation)));
			this.MessageIntepretationHTML.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 17, true);
			this.MessageIntepretationHTML.Name = "MessageIntepretationHTML";
			this.MessageIntepretationHTML.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 198, true);
			this.MessageIntepretationHTML.TabIndex = 6;
			// 
			// CDSDISQueryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CDSDISQueryForm|66149197-E7C2-4EA1-9FA5-E0453C63E664", "CDS DIS Query");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(889, 475, true);
			this.DataSourceType = typeof(Enterprise.Customs.GB.CDS.CDSDISQueryMessage);
			this.Name = "CDSDISQueryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RequestGroupBox.ResumeLayout(false);
			this.RequestGroupBox.PerformLayout();
			this.MessageOwnerTextBox.ResumeLayout(true);
			this.MessageOwnerTextBox.PerformLayout();
			this.DeclarationCategoryTextBox.ResumeLayout(true);
			this.DeclarationCategoryTextBox.PerformLayout();
			this.DateFrom.ResumeLayout(true);
			this.DateFrom.PerformLayout();
			this.DateTo.ResumeLayout(true);
			this.DateTo.PerformLayout();
			this.DeclarationStatusTextBox.ResumeLayout(true);
			this.DeclarationStatusTextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PageNumber)).EndInit();
			this.PageNumber.ResumeLayout(false);
			this.PageNumber.PerformLayout();
			this.ResponseGroupBox.ResumeLayout(false);
			this.ResponseGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox RequestGroupBox;
		ZArchitecture.GUI.ZDropEdit MessageOwnerTextBox;
		ZArchitecture.ZTextBox MessageStatusTextBox;
		ZArchitecture.ZTextBox MessageNumberTextBox;
		ZArchitecture.ZTextBox ApplicationReferenceTextBox;
		ZArchitecture.GUI.ZDropEdit DeclarationCategoryTextBox;
		ZArchitecture.GUI.ZDateEdit DateFrom;
		ZArchitecture.GUI.ZDateEdit DateTo;
		ZArchitecture.GUI.ZDropEdit DeclarationStatusTextBox;
		ZArchitecture.GUI.ZNumericUpDown PageNumber;

		ZArchitecture.GUI.ZGroupBox ResponseGroupBox;
		Messaging.GUI.HtmlInterpretationBox MessageIntepretationHTML;
	}
}
