namespace Enterprise.Registry.GUI
{
	partial class PackageTypePairsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackageTypesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackageTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LabelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackageTypesLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackageTypesPanel.SuspendLayout();
			this.GridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageTypesGrid)).BeginInit();
			this.PackageTypesGrid.SuspendLayout();
			this.LabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.US.USPackageTypePairCollection);
			// 
			// PackageTypesPanel
			// 
			this.PackageTypesPanel.Controls.Add(this.GridPanel);
			this.PackageTypesPanel.Controls.Add(this.LabelPanel);
			this.PackageTypesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageTypesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageTypesPanel.Name = "PackageTypesPanel";
			this.PackageTypesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 380, true);
			this.PackageTypesPanel.TabIndex = 0;
			// 
			// GridPanel
			// 
			this.GridPanel.Controls.Add(this.PackageTypesGrid);
			this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 343, true);
			this.GridPanel.TabIndex = 1;
			// 
			// PackageTypesGrid
			// 
			this.PackageTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackageTypesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)).CustomsPackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)).CustomsPackageTypeFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)).CustomsPackageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)).FreightPackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.USPackageTypePair)(null)).FreightPackageTypeDescription)));
			this.PackageTypesGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PackageTypePairsControl|b25cdb2e-fd70-4b72-b6f4-1a235ff76746", "Customs package type");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CustomsPackageType";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CustomsPackageTypeFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PackageTypePairsControl|fe10b38f-0aa1-401e-9bb4-2ef08c155b42", "Customs package type desc.");
			zTextBoxColumnStyleInfo1.ColumnName = "CustomsPackageTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PackageTypePairsControl|e2517462-55b2-4d07-b17a-00d2c0d3c425", "Freight package type");
			zDropEditColumnStyleInfo1.ColumnName = "FreightPackageType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PackageTypePairsControl|47a7c92b-ae8d-4d83-b518-c713d297df9c", "Freight package type desc.");
			zTextBoxColumnStyleInfo2.ColumnName = "FreightPackageTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.PackageTypesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.PackageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackageTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackageTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageTypesGrid.GridId = "b78bc84f-ce04-40f7-9e4c-ff5602a4dbc9";
			this.PackageTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageTypesGrid.LayoutKey = "PackageTypesGrid";
			this.PackageTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageTypesGrid.Name = "PackageTypesGrid";
			this.PackageTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 343, true);
			this.PackageTypesGrid.TabIndex = 0;
			// 
			// LabelPanel
			// 
			this.LabelPanel.Controls.Add(this.PackageTypesLabel);
			this.LabelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LabelPanel.Name = "LabelPanel";
			this.LabelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 37, true);
			this.LabelPanel.TabIndex = 0;
			// 
			// PackageTypesLabel
			// 
			this.PackageTypesLabel.AutoSize = true;
			this.PackageTypesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PackageTypePairsControl|eea54ad8-0de9-4cf7-ba9b-d59d7f33af8d", "Package Types mapping");
			this.PackageTypesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackageTypesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 12, true);
			this.PackageTypesLabel.Name = "PackageTypesLabel";
			this.PackageTypesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 13, true);
			this.PackageTypesLabel.TabIndex = 0;
			// 
			// PackageTypePairsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackageTypesPanel);
			this.Name = "PackageTypePairsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackageTypesPanel.ResumeLayout(false);
			this.PackageTypesPanel.PerformLayout();
			this.GridPanel.ResumeLayout(false);
			this.GridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageTypesGrid)).EndInit();
			this.PackageTypesGrid.ResumeLayout(false);
			this.PackageTypesGrid.PerformLayout();
			this.LabelPanel.ResumeLayout(false);
			this.LabelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel PackageTypesPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel GridPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel LabelPanel;
		internal Enterprise.ZArchitecture.ZGrid PackageTypesGrid;
		private Enterprise.ZArchitecture.ZLabel PackageTypesLabel;
	}
}
