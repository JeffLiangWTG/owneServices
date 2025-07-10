namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapeEntityChannelsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZColorDropEditColumnStyleInfo();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChannelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelsGrid)).BeginInit();
			this.ChannelsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape);
			// 
			// groupBox1
			// 
			this.groupBox1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("e870fe9d-a682-4158-8136-110497644d69", "Diagram Channels");
			this.groupBox1.Controls.Add(this.ChannelsGrid);
			this.groupBox1.Controls.Add(this.HintLabel);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 528, true);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			// 
			// ChannelsGrid
			// 
			this.ChannelsGrid.AllowNavigation = false;
			this.ChannelsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChannelsGrid, "Channels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).Channels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNChannel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).Channels)).SyncRoot)).BNL_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNChannel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).Channels)).SyncRoot)).BNL_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNChannel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).Channels)).SyncRoot)).BNL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNChannel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).Channels)).SyncRoot)).Color)));
			this.ChannelsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BNL_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "BNL_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BNL_Height";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "Color";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ChannelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChannelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChannelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChannelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChannelsGrid.GridId = "18df05ac-4174-4f70-9136-32a21f372a83";
			this.ChannelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChannelsGrid.LayoutKey = "TagRulesGrid";
			this.ChannelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 51, true);
			this.ChannelsGrid.Name = "ChannelsGrid";
			this.ChannelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 471, true);
			this.ChannelsGrid.TabIndex = 2;
			// 
			// HintLabel
			// 
			this.HintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HintLabel.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("d1a20aed-f9be-4eec-bbac-eb6317af801e", "Define the channels that will span the width of this diagram. Channels can be used to define the key characteristics of shapes placed within them, and to enforce leveling rules.");
			this.HintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 32, true);
			this.HintLabel.TabIndex = 1;
			// 
			// ShapeEntityChannelsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBox1);
			this.Name = "ShapeEntityChannelsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 528, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelsGrid)).EndInit();
			this.ChannelsGrid.ResumeLayout(false);
			this.ChannelsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

        #endregion

        private ZArchitecture.GUI.ZGroupBox groupBox1;
        private ZArchitecture.ZLabel HintLabel;
        private ZArchitecture.ZGrid ChannelsGrid;
    }
}
