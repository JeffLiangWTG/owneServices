namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class SourceModulesControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SourceModulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AddCurrentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddClientSpecificButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CleanUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SourceModulesGrid)).BeginInit();
			this.SourceModulesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SourceModule);
			// 
			// SourceModulesGrid
			// 
			this.SourceModulesGrid.AllowNavigation = false;
			this.SourceModulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SourceModulesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).Path)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).ModuleListTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).DefaultModule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).IsSelectableForOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).IsSearchable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SourceModule)(null)).Product)));
			this.SourceModulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("7dedd3dc-e074-4a74-9e16-c50541bb5c2a", "Code", "Code", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("54884715-f814-4ecf-9863-349496401e03", "Description", "Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("fbeb085e-dd09-4c86-93c2-6a884bb17a2a", "Path", "Module Tree Path", "");
			zTextBoxColumnStyleInfo3.ColumnName = "Path";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("dab237e6-7fea-4273-af9d-0c6c2ec8d5d5", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "ModuleListTypeDescription";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("aa169e4b-1d19-41e5-a11c-235efede26a6", "Default Sec. / Req. / Srv.", "Default Section / Requirement / Service", "");
			zDropEditColumnStyleInfo2.ColumnName = "DefaultModule";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("1c00b1c6-871f-4244-bca9-753a1b7532ce", "Valid Override?", "Valid Override Option?", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelectableForOverride";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("98735865-3617-4e75-9d20-6f4744c4a5c2", "Searchable?", "Is Searchable?", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSearchable";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDropEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("a4f9f693-e987-40ff-98a0-731679795739", "Product");
			zDropEditColumnStyleInfo3.ColumnName = "Product";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SourceModulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SourceModulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SourceModulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SourceModulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SourceModulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SourceModulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SourceModulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.SourceModulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SourceModulesGrid.CopySelectedRowsAllowed = true;
			this.SourceModulesGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.SourceModulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SourceModulesGrid.LayoutKey = "zGrid1";
			this.SourceModulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 32, true);
			this.SourceModulesGrid.Name = "SourceModulesGrid";
			this.SourceModulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 250, true);
			this.SourceModulesGrid.TabIndex = 2;
			// 
			// AddCurrentButton
			// 
			this.AddCurrentButton.CaptionResourceString = ZClientEDI.Res.GetData("5a5fbb00-bdbf-4e4b-bafc-f91b3ef1463d", "Add Current Menu Items");
			this.AddCurrentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.AddCurrentButton.Name = "AddCurrentButton";
			this.AddCurrentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.AddCurrentButton.TabIndex = 0;
			this.AddCurrentButton.Click += new System.EventHandler(this.AddCurrentButton_Click);
			// 
			// AddClientSpecificButton
			// 
			this.AddClientSpecificButton.CaptionResourceString = ZClientEDI.Res.GetData("bbf0a781-e1e8-4b30-8819-c4b4cb8579a3", "Add Menu Items From All ZCLIENT DLLs");
			this.AddClientSpecificButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 3, true);
			this.AddClientSpecificButton.Name = "AddClientSpecificButton";
			this.AddClientSpecificButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 23, true);
			this.AddClientSpecificButton.TabIndex = 1;
			this.AddClientSpecificButton.Click += new System.EventHandler(this.AddClientSpecificButton_Click);
			// 
			// CleanUpButton
			// 
			this.CleanUpButton.CaptionResourceString = ZClientEDI.Res.GetData("7c3f3b23-3b2e-47d2-8e83-b57dbd648012", "Clean up");
			this.CleanUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 3, true);
			this.CleanUpButton.Name = "CleanUpButton";
			this.CleanUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CleanUpButton.TabIndex = 1;
			this.CleanUpButton.Click += new System.EventHandler(this.CleanUpButton_Click);
			// 
			// SourceModulesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AddClientSpecificButton);
			this.Controls.Add(this.AddCurrentButton);
			this.Controls.Add(this.CleanUpButton);
			this.Controls.Add(this.SourceModulesGrid);
			this.Name = "SourceModulesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SourceModulesGrid)).EndInit();
			this.SourceModulesGrid.ResumeLayout(false);
			this.SourceModulesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid SourceModulesGrid;
		private ZArchitecture.GUI.ZButton AddCurrentButton;
		private ZArchitecture.GUI.ZButton AddClientSpecificButton;
		protected ZArchitecture.GUI.ZButton CleanUpButton;
	}
}
