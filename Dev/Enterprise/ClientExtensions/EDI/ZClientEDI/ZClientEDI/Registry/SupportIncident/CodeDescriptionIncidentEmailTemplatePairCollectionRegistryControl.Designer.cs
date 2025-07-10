namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.IncidentEmailTemplatePairRegistryControl = new Enterprise.Client.EDI.Registry.GUI.IncidentEmailTemplatePairRegistryControl();
			this.categoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CategoryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.categoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePairCollection);
			// 
			// IncidentEmailTemplatePairRegistryControl
			// 
			this.IncidentEmailTemplatePairRegistryControl.AllowDrop = true;
			this.IncidentEmailTemplatePairRegistryControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IncidentEmailTemplatePairRegistryControl, "EmailTemplates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)).EmailTemplates)));
			this.IncidentEmailTemplatePairRegistryControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.IncidentEmailTemplatePairRegistryControl.Name = "IncidentEmailTemplatePairRegistryControl";
			this.IncidentEmailTemplatePairRegistryControl.ReadOnly = false;
			this.IncidentEmailTemplatePairRegistryControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 310, true);
			this.IncidentEmailTemplatePairRegistryControl.TabIndex = 0;
			// 
			// categoryGroupBox
			// 
			this.categoryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.categoryGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("bba76df6-87cd-44c2-9c65-23b44558ce3e", "Category");
			this.categoryGroupBox.Controls.Add(this.CategoryGrid);
			this.categoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.categoryGroupBox.Name = "categoryGroupBox";
			this.categoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 169, true);
			this.categoryGroupBox.TabIndex = 1;
			this.categoryGroupBox.TabStop = false;
			// 
			// CategoryGrid
			// 
			this.CategoryGrid.AllowNavigation = false;
			this.CategoryGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CategoryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionIncidentEmailTemplatePair)(null)).NeedUpgrade)));
			this.CategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.Caption = "Template Description";
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.Caption = "Product";
			zDropEditColumnStyleInfo1.ColumnName = "Product";
			zCheckBoxColumnStyleInfo1.Caption = "Need Upgrade";
			zCheckBoxColumnStyleInfo1.ColumnName = "NeedUpgrade";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CategoryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CategoryGrid.CopySelectedRowsAllowed = true;
			this.CategoryGrid.GridId = "579951ad-3e92-4e6a-9615-3e431472d49d";
			this.CategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CategoryGrid.LayoutKey = "zGrid1";
			this.CategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.CategoryGrid.Name = "CategoryGrid";
			this.CategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 120, true);
			this.CategoryGrid.TabIndex = 0;
			// 
			// CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.categoryGroupBox);
			this.Controls.Add(this.IncidentEmailTemplatePairRegistryControl);
			this.Name = "CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 498, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.categoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.Client.EDI.Registry.GUI.IncidentEmailTemplatePairRegistryControl IncidentEmailTemplatePairRegistryControl;
		private ZArchitecture.GUI.ZGroupBox categoryGroupBox;
		internal ZArchitecture.ZGrid CategoryGrid;
	}
}
