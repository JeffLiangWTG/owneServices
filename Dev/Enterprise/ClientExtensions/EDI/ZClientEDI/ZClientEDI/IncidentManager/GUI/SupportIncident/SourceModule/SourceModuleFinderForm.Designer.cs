namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class SourceModuleFinderForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SourceModuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.menuSectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zFolderBrowserDialog1 = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.FindReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FindReasonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SourceModuleGrid)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.FindReasonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder);
			// 
			// SourceModuleGrid
			// 
			this.SourceModuleGrid.AllowNavigation = false;
			this.SourceModuleGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SourceModuleGrid, "SourceModules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).ModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).ModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).SourceModulePath)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).SourceModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ModuleMappingWithSourceModule)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).SourceModules)).SyncRoot)).SourceModuleCode)));
			this.SourceModuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("d09f2ee2-1b7e-4877-8824-a396b2b76fc2", "Product Area");
			zTextBoxColumnStyleInfo1.ColumnName = "ProductArea";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("c43645d8-997c-4493-a0d4-97db1b7406f9", "Module Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ModuleCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("b2a68e39-8f0c-4a9e-a674-3623cb88ea64", "Module Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ModuleDescription";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("f0344130-f0b5-4faa-a6bb-0a1b43f8691c", "Path");
			zTextBoxColumnStyleInfo4.ColumnName = "SourceModulePath";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("19bce510-0a9c-4b8a-808f-009205b0408f", "Description");
			zTextBoxColumnStyleInfo5.ColumnName = "SourceModuleDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("fcec25de-c275-4ebb-98d7-acb3262d83cd", "Code");
			zTextBoxColumnStyleInfo6.ColumnName = "SourceModuleCode";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SourceModuleGrid.CopySelectedRowsAllowed = true;
			this.SourceModuleGrid.GridId = "2b255dc8-bee8-4c00-b405-0e0b8e42045d";
			this.SourceModuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SourceModuleGrid.IsWholeRowSelectedOnClick = true;
			this.SourceModuleGrid.LayoutKey = "zGrid1";
			this.SourceModuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 84, true);
			this.SourceModuleGrid.Name = "SourceModuleGrid";
			this.SourceModuleGrid.ReadOnly = true;
			this.SourceModuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 258, true);
			this.SourceModuleGrid.TabIndex = 3;
			this.SourceModuleGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SourceModuleGrid_KeyDown);
			this.SourceModuleGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SourceModuleGrid_MouseDoubleClick);
			// 
			// CancelZButton
			// 
			this.CancelZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelZButton.CaptionResourceString = ZClientEDI.Res.GetData("e5574cfb-e93e-413e-a32f-4d8ac82a4b00", "Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 348, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelZButton.TabIndex = 5;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = ZClientEDI.Res.GetData("cf6abda3-ce9b-4210-bfd4-139de3e9bbbf", "OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 348, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 4;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// menuSectionDropEdit
			// 
			this.menuSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.menuSectionDropEdit, "ModuleFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).ModuleFilter)));
			this.menuSectionDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("8fd19f30-31e6-4947-b895-5919da98b5c7", "Menu Section");
			this.menuSectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 32, true);
			this.menuSectionDropEdit.Name = "menuSectionDropEdit";
			this.menuSectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.menuSectionDropEdit.TabIndex = 1;
			// 
			// productAreaDropEdit
			// 
			this.productAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productAreaDropEdit, "ProductAreaFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).ProductAreaFilter)));
			this.productAreaDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("d628ce12-4d02-476d-b697-6c6a03c2e69f", "Product Area");
			this.productAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 6, true);
			this.productAreaDropEdit.Name = "productAreaDropEdit";
			this.productAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.productAreaDropEdit.TabIndex = 0;
			// 
			// zFolderBrowserDialog1
			// 
			this.zFolderBrowserDialog1.CreateDirectory = false;
			this.zFolderBrowserDialog1.Description = "";
			this.zFolderBrowserDialog1.RequireMappablePath = false;
			this.zFolderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.zFolderBrowserDialog1.ShowNewFolderButton = true;
			// 
			// FindReasonLabel
			// 
			this.BindingSource.SetBindingMember(this.FindReasonLabel, "FindReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder)(null)).FindReason)));
			this.FindReasonLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FindReasonLabel, false);
			this.FindReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.FindReasonLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			this.FindReasonLabel.Name = "FindReasonLabel";
			this.FindReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 54, true);
			this.FindReasonLabel.TabIndex = 6;
			this.FindReasonLabel.Text = "<Find Reason>";
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.DescriptionTextBox);
			this.MainPanel.Controls.Add(this.productAreaDropEdit);
			this.MainPanel.Controls.Add(this.menuSectionDropEdit);
			this.MainPanel.Controls.Add(this.SourceModuleGrid);
			this.MainPanel.Controls.Add(this.OkButton);
			this.MainPanel.Controls.Add(this.CancelZButton);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 377, true);
			this.MainPanel.TabIndex = 7;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("eb9c79ab-4eb1-4929-b90f-ee6a2bf08b4c", "Description");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 58, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.DescriptionTextBox.TabIndex = 2;
			this.DescriptionTextBox.TextChanged += new System.EventHandler(this.DescriptionTextBox_TextChanged);
			// 
			// FindReasonPanel
			// 
			this.FindReasonPanel.Controls.Add(this.FindReasonLabel);
			this.FindReasonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FindReasonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FindReasonPanel.Name = "FindReasonPanel";
			this.FindReasonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 3, 7, 3, true);
			this.FindReasonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 60, true);
			this.FindReasonPanel.TabIndex = 5;
			// 
			// SourceModuleFinderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 461, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.FindReasonPanel);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SourceModuleFinder);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 400, true);
			this.Name = "SourceModuleFinderForm";
			this.Text = "Menu Item";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FindReasonPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SourceModuleGrid)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.FindReasonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid SourceModuleGrid;
		private ZArchitecture.GUI.ZButton CancelZButton;
		private ZArchitecture.GUI.ZButton OkButton;
		private ZArchitecture.GUI.ZDropEdit menuSectionDropEdit;
		private ZArchitecture.GUI.ZDropEdit productAreaDropEdit;
		private ZArchitecture.GUI.ZFolderBrowserDialog zFolderBrowserDialog1;
		private ZArchitecture.ZLabel FindReasonLabel;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel FindReasonPanel;
		private ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
