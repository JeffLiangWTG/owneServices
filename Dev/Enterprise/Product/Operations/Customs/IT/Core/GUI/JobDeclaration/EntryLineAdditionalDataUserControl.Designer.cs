namespace Enterprise.Customs.IT.GUI
{
	partial class EntryLineAdditionalDataUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.M2LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GroupedPreviousDocumentsUserControl = new Enterprise.Customs.IT.GUI.GroupedPreviousDocumentsUserControl();
			this.ExtendInfoTabControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.M2LinesTabPage.SuspendLayout();
			this.GroupedPreviousDocumentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Controls.Add(this.M2LinesTabPage);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.ExtendInfoTabControl.Controls.SetChildIndex(this.M2LinesTabPage, 0);
			// 
			// M2LinesTabPage
			// 
			this.M2LinesTabPage.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("3B9DB477-9B5F-4A7A-BE04-ADC8B36940CC", "M2 Lines");
			this.M2LinesTabPage.Controls.Add(this.GroupedPreviousDocumentsUserControl);
			this.M2LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.M2LinesTabPage.Name = "M2LinesTabPage";
			this.M2LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.M2LinesTabPage.TabIndex = 1;
			// 
			// GroupedPreviousDocumentsUserControl
			// 
			this.GroupedPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupedPreviousDocumentsUserControl, "CustomsEntryHeaders.AllEntryLines.GroupedPreviousDocuments");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).GroupedPreviousDocuments)));
			this.GroupedPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupedPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupedPreviousDocumentsUserControl.Name = "GroupedPreviousDocumentsUserControl";
			this.GroupedPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.GroupedPreviousDocumentsUserControl.TabIndex = 0;
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.M2LinesTabPage.ResumeLayout(false);
			this.M2LinesTabPage.PerformLayout();
			this.GroupedPreviousDocumentsUserControl.ResumeLayout(true);
			this.GroupedPreviousDocumentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage M2LinesTabPage;
		private GroupedPreviousDocumentsUserControl GroupedPreviousDocumentsUserControl;

	}
}
