namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class EMCSCustomsBrokerageUserControl
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
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.shipmentCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
            this.CustomTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.DeclarationUserControl = new Enterprise.Customs.EU.EMCS.GUI.DeclarationUserControl();
            this.PackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.packingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PackingGrid = new Enterprise.ZArchitecture.ZGrid();
            this.InvoiceLineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.EMCSInvoiceLineControl = new Enterprise.Customs.EU.EMCS.GUI.EMCSInvoiceLineControl();
            this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessageUserControl = new Enterprise.Customs.EU.EMCS.GUI.MessagesUserControl();
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.NoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
            this.EventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.shipmentCustomFieldsControl1.SuspendLayout();
            this.CustomTabPage.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.DeclarationTabPage.SuspendLayout();
            this.DeclarationUserControl.SuspendLayout();
            this.PackingTabPage.SuspendLayout();
            this.packingDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).BeginInit();
            this.PackingGrid.SuspendLayout();
            this.InvoiceLineTabPage.SuspendLayout();
            this.EMCSInvoiceLineControl.SuspendLayout();
            this.MessagesTabPage.SuspendLayout();
            this.MessageUserControl.SuspendLayout();
            this.WorkflowTabPage.SuspendLayout();
            this.NoteTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
            // 
            // shipmentCustomFieldsControl1
            // 
            this.shipmentCustomFieldsControl1.AllowDrop = true;
            this.shipmentCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shipmentCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.shipmentCustomFieldsControl1.Name = "shipmentCustomFieldsControl1";
            this.shipmentCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields" +
    " in Workflow Manager.";
            this.shipmentCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.shipmentCustomFieldsControl1.TabIndex = 0;
            // 
            // CustomTabPage
            // 
            this.CustomTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("8975f81c-9c38-4046-a069-e8e78ec9085b", "Custom");
            this.CustomTabPage.Controls.Add(this.shipmentCustomFieldsControl1);
            this.CustomTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.CustomTabPage.Name = "CustomTabPage";
            this.CustomTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.CustomTabPage.TabIndex = 4;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.MainTabControl.Controls.Add(this.DeclarationTabPage);
            this.MainTabControl.Controls.Add(this.PackingTabPage);
            this.MainTabControl.Controls.Add(this.InvoiceLineTabPage);
            this.MainTabControl.Controls.Add(this.MessagesTabPage);
            this.MainTabControl.Controls.Add(this.WorkflowTabPage);
            this.MainTabControl.Controls.Add(this.CustomTabPage);
            this.MainTabControl.Controls.Add(this.NoteTabPage);
            this.MainTabControl.Controls.Add(this.EventTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 624, true);
            this.MainTabControl.TabIndex = 0;
            // 
            // DeclarationTabPage
            // 
            this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7df5e4a0-e1a3-4fb8-af49-ea5f593b8ad7", "EMCS");
            this.DeclarationTabPage.Controls.Add(this.DeclarationUserControl);
            this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.DeclarationTabPage.Name = "DeclarationTabPage";
            this.DeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.DeclarationTabPage.TabIndex = 0;
            this.DeclarationTabPage.UseVisualStyleBackColor = true;
            // 
            // DeclarationUserControl
            // 
            this.DeclarationUserControl.AllowDrop = true;
            this.DeclarationUserControl.AutoScroll = true;
            this.BindingSource.SetBindingMember(this.DeclarationUserControl, ".");
            this.DeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.DeclarationUserControl.Name = "DeclarationUserControl";
            this.DeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 597, true);
            this.DeclarationUserControl.TabIndex = 0;
            // 
            // PackingTabPage
            // 
            this.PackingTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("b16f3f9c-acc1-49a9-98a6-75b618659601", "Packing");
            this.PackingTabPage.Controls.Add(this.packingDetailsGroupBox);
            this.PackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.PackingTabPage.Name = "PackingTabPage";
            this.PackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.PackingTabPage.TabIndex = 11;
            // 
            // packingDetailsGroupBox
            // 
            this.packingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("410dba47-38c2-4331-9e47-e8ef5a9183a2", "Packing Details");
            this.packingDetailsGroupBox.Controls.Add(this.PackingGrid);
            this.packingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.packingDetailsGroupBox.Name = "packingDetailsGroupBox";
            this.packingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.packingDetailsGroupBox.TabIndex = 0;
            this.packingDetailsGroupBox.TabStop = false;
            // 
            // PackingGrid
            // 
            this.PackingGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.PackingGrid, "EMCSPackages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.EMCS.Business.EMCSPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)).SyncRoot)).B5_UnitCount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)).SyncRoot)).B5_UnitType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)).SyncRoot)).B5_MarksAndNumbers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)).SyncRoot)).B5_SealNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).EMCSPackages)).SyncRoot)).B5_SealComment)));
            this.PackingGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("2431d084-b37f-46c7-b430-59ccd3cec289", "Pack Qty");
            zTextBoxColumnStyleInfo1.ColumnName = "B5_UnitCount";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("a5c6516c-7078-4bb3-ad93-e1eef1466157", "Pack Type");
            zDropEditColumnStyleInfo1.ColumnName = "B5_UnitType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("c37565e9-11b9-4d85-926a-38f119037f39", "Marks");
            zTextBoxColumnStyleInfo2.ColumnName = "B5_MarksAndNumbers";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("df618c35-7022-47ad-b263-b03d8a82054d", "Seal Number");
            zTextBoxColumnStyleInfo3.ColumnName = "B5_SealNumber";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("cb042d18-d6a6-444d-a33b-05d9f30fab5e", "Seal Comment");
            zTextBoxColumnStyleInfo4.ColumnName = "B5_SealComment";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.PackingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.PackingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.PackingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PackingGrid.GridId = "053a8a62-33e1-437a-844b-d3ce2e174d73";
            this.PackingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.PackingGrid.LayoutKey = "PackingGrid";
            this.PackingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.PackingGrid.Name = "PackingGrid";
            this.PackingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 585, true);
            this.PackingGrid.TabIndex = 1;
            // 
            // InvoiceLineTabPage
            // 
            this.InvoiceLineTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("f8869709-c1c1-47df-8285-8d77592cb200", "Inv. Lines");
            this.InvoiceLineTabPage.Controls.Add(this.EMCSInvoiceLineControl);
            this.InvoiceLineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.InvoiceLineTabPage.Name = "InvoiceLineTabPage";
            this.InvoiceLineTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.InvoiceLineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.InvoiceLineTabPage.TabIndex = 1;
            this.InvoiceLineTabPage.UseVisualStyleBackColor = true;
            // 
            // EMCSInvoiceLineControl
            // 
            this.EMCSInvoiceLineControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EMCSInvoiceLineControl, "FilteredInvoiceLines");
            this.EMCSInvoiceLineControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EMCSInvoiceLineControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.EMCSInvoiceLineControl.Name = "EMCSInvoiceLineControl";
            this.EMCSInvoiceLineControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 597, true);
            this.EMCSInvoiceLineControl.TabIndex = 0;
            // 
            // MessagesTabPage
            // 
            this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("4833501a-750c-420e-a168-966d3a089965", "Messages");
            this.MessagesTabPage.Controls.Add(this.MessageUserControl);
            this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.MessagesTabPage.Name = "MessagesTabPage";
            this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.MessagesTabPage.TabIndex = 2;
            this.MessagesTabPage.UseVisualStyleBackColor = true;
            // 
            // MessageUserControl
            // 
            this.MessageUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MessageUserControl, "Messages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).Messages)));
            this.MessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageUserControl.Name = "MessageUserControl";
            this.MessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 597, true);
            this.MessageUserControl.TabIndex = 0;
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 45, true);
            this.WorkflowTabPage.TabIndex = 3;
            // 
            // NoteTabPage
            // 
            this.NoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.NoteTabPage.Name = "NoteTabPage";
            this.NoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.NoteTabPage.TabIndex = 9;
            // 
            // EventTabPage
            // 
            this.EventTabPage.ExcludeFromBindingOnSave = true;
            this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.EventTabPage.Name = "EventTabPage";
            this.EventTabPage.ShouldBeReadOnlyInViewMode = false;
            this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 602, true);
            this.EventTabPage.TabIndex = 10;
            // 
            // EMCSCustomsBrokerageUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainTabControl);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 624, true);
            this.Name = "EMCSCustomsBrokerageUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 624, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.shipmentCustomFieldsControl1.ResumeLayout(true);
            this.shipmentCustomFieldsControl1.PerformLayout();
            this.CustomTabPage.ResumeLayout(false);
            this.CustomTabPage.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.DeclarationTabPage.ResumeLayout(false);
            this.DeclarationTabPage.PerformLayout();
            this.DeclarationUserControl.ResumeLayout(true);
            this.DeclarationUserControl.PerformLayout();
            this.PackingTabPage.ResumeLayout(false);
            this.PackingTabPage.PerformLayout();
            this.packingDetailsGroupBox.ResumeLayout(false);
            this.packingDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).EndInit();
            this.PackingGrid.ResumeLayout(false);
            this.PackingGrid.PerformLayout();
            this.InvoiceLineTabPage.ResumeLayout(false);
            this.InvoiceLineTabPage.PerformLayout();
            this.EMCSInvoiceLineControl.ResumeLayout(true);
            this.EMCSInvoiceLineControl.PerformLayout();
            this.MessagesTabPage.ResumeLayout(false);
            this.MessagesTabPage.PerformLayout();
            this.MessageUserControl.ResumeLayout(true);
            this.MessageUserControl.PerformLayout();
            this.WorkflowTabPage.ResumeLayout(false);
            this.WorkflowTabPage.PerformLayout();
            this.NoteTabPage.ResumeLayout(false);
            this.NoteTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		
		private Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl shipmentCustomFieldsControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage CustomTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DeclarationTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage InvoiceLineTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private EMCSInvoiceLineControl EMCSInvoiceLineControl;
		private DeclarationUserControl DeclarationUserControl;
		private Enterprise.Customs.EU.EMCS.GUI.MessagesUserControl MessageUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		internal Enterprise.ZArchitecture.GUI.ZLogsTabPage EventTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage NoteTabPage;
		private ZArchitecture.GUI.ZTabPage PackingTabPage;
		private ZArchitecture.ZGrid PackingGrid;
		private ZArchitecture.GUI.ZGroupBox packingDetailsGroupBox;
	}
}
