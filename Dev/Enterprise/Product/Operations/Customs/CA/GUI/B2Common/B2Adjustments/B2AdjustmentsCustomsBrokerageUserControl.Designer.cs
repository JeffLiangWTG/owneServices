namespace Enterprise.Customs.CA.GUI
{
	partial class B2AdjustmentsCustomsBrokerageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.B2LineAsAccountedForTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.b2LineAsAccountedForUserControl = new Enterprise.Customs.CA.GUI.B2LineAsAccountedForUserControl();
			this.b2LineAsClaimedForUserControl = new Enterprise.Customs.CA.GUI.B2LineAsClaimedForUserControl();
			this.B2LineAsClaimedForTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.B2LineAsAccountedForTabPage.SuspendLayout();
			this.b2LineAsAccountedForUserControl.SuspendLayout();
			this.b2LineAsClaimedForUserControl.SuspendLayout();
			this.B2LineAsClaimedForTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.B2LineAsAccountedForTabPage);
			this.MainTabControl.Controls.Add(this.B2LineAsClaimedForTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.EventTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.B2LineAsClaimedForTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.EntryInstructionDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MiscOptionsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceLinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoicesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceGroupingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainerTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.B2LineAsAccountedForTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DeclarationTabPage, 0);
			// 
			// B2LineAsAccountedForTabPage
			// 
			this.B2LineAsAccountedForTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f75e1947-afcb-4912-ab1e-26e9129d2f4f", "As Accounted");
			this.B2LineAsAccountedForTabPage.Controls.Add(this.b2LineAsAccountedForUserControl);
			this.B2LineAsAccountedForTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.B2LineAsAccountedForTabPage.Name = "B2LineAsAccountedForTabPage";
			this.B2LineAsAccountedForTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.B2LineAsAccountedForTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.B2LineAsAccountedForTabPage.TabIndex = 11;
			this.B2LineAsAccountedForTabPage.UseVisualStyleBackColor = true;
			// 
			// b2LineAsAccountedForUserControl
			// 
			this.b2LineAsAccountedForUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.b2LineAsAccountedForUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobDeclaration)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)))));
			this.b2LineAsAccountedForUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.b2LineAsAccountedForUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.b2LineAsAccountedForUserControl.Name = "b2LineAsAccountedForUserControl";
			this.b2LineAsAccountedForUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 623, true);
			this.b2LineAsAccountedForUserControl.TabIndex = 0;
			// 
			// b2LineAsClaimedForUserControl
			// 
			this.b2LineAsClaimedForUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.b2LineAsClaimedForUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobDeclaration)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)))));
			this.b2LineAsClaimedForUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.b2LineAsClaimedForUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.b2LineAsClaimedForUserControl.Name = "b2LineAsClaimedForUserControl";
			this.b2LineAsClaimedForUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 623, true);
			this.b2LineAsClaimedForUserControl.TabIndex = 0;
			// 
			// B2LineAsClaimedForTabPage
			// 
			this.B2LineAsClaimedForTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("af13c7b7-26ef-4e29-be43-f65d53ea794e", "As Claimed");
			this.B2LineAsClaimedForTabPage.Controls.Add(this.b2LineAsClaimedForUserControl);
			this.B2LineAsClaimedForTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.B2LineAsClaimedForTabPage.Name = "B2LineAsClaimedForTabPage";
			this.B2LineAsClaimedForTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.B2LineAsClaimedForTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.B2LineAsClaimedForTabPage.TabIndex = 12;
			this.B2LineAsClaimedForTabPage.UseVisualStyleBackColor = true;
			// 
			// B2AdjustmentsCustomsBrokerageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "B2AdjustmentsCustomsBrokerageUserControl";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.B2LineAsAccountedForTabPage.ResumeLayout(false);
			this.B2LineAsAccountedForTabPage.PerformLayout();
			this.b2LineAsAccountedForUserControl.ResumeLayout(true);
			this.b2LineAsAccountedForUserControl.PerformLayout();
			this.b2LineAsClaimedForUserControl.ResumeLayout(true);
			this.b2LineAsClaimedForUserControl.PerformLayout();
			this.B2LineAsClaimedForTabPage.ResumeLayout(false);
			this.B2LineAsClaimedForTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage B2LineAsAccountedForTabPage;
		private B2LineAsAccountedForUserControl b2LineAsAccountedForUserControl;
		private B2LineAsClaimedForUserControl b2LineAsClaimedForUserControl;
		public ZArchitecture.GUI.ZTabPage B2LineAsClaimedForTabPage;
	}
}
