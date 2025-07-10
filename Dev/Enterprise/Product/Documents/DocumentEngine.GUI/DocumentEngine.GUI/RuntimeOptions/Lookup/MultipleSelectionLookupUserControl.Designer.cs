using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class MultipleSelectionLookupUserControl : RuntimeOptionUserControl
	{
		internal ZLabel FieldLabel;
		internal ZModuleButtonGrid Grid;
		FilterFieldNotification Notification;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Grid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.Notification = new Enterprise.DocumentEngine.GUI.RuntimeOptions.FilterFieldNotification();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid.InnerGrid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// FieldLabel
			// 
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FieldLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 4, 3, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 53, true);
			this.FieldLabel.TabIndex = 5;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// Grid
			// 
			this.Grid.AllowDrop = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ZModuleButtonGrid|880386be-6988-4d13-92ab-346c16626e58", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ZModuleButtonGrid|799c3f9b-6956-445f-a533-8fa34c685d07", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.DetachButtonText = Enterprise.DocumentEngine.GUI.Res.GetData("8F1BEA6B-BEDF-44AE-A723-F7234B9F6A8E", "Remove");
			this.Grid.DetachMessage = Enterprise.DocumentEngine.GUI.Res.GetData("D6BF6277-5700-4B1C-B8C9-251458835730", "Are you sure you want to remove the selected record?");
			this.Grid.GridId = "06d0c389-bc17-4bce-97dc-4387a5b00f4a";
			// 
			// 
			// 
			this.Grid.InnerGrid.AllowNavigation = false;
			this.Grid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.Grid.InnerGrid.CaptionVisible = false;
			this.Grid.InnerGrid.GridId = null;
			this.Grid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.InnerGrid.LayoutKey = "Grid";
			this.Grid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Grid.InnerGrid.Name = "Grid";
			this.Grid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 66, true);
			this.Grid.InnerGrid.TabIndex = 0;
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.NameOfAGridElement = Enterprise.DocumentEngine.GUI.Res.GetData("F45921F8-2A6E-4C25-B13A-32B0ADD65939", "Element");
			this.Grid.ReadOnly = false;
			this.Grid.ShowEditButton = false;
			this.Grid.ShowNewButton = false;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 100, true);
			this.Grid.TabIndex = 7;
			// 
			// Notification
			// 
			this.Notification.AllowDrop = true;
			this.Notification.BackColor = System.Drawing.Color.Transparent;
			this.Notification.FilterField = null;
			this.Notification.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 80, true);
			this.Notification.Name = "Notification";
			this.Notification.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 16, true);
			this.Notification.TabIndex = 8;
			// 
			// MultipleSelectionLookupUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Notification);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.FieldLabel);
			this.Name = "MultipleSelectionLookupUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid.InnerGrid)).EndInit();
			this.Grid.ResumeLayout(true);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
