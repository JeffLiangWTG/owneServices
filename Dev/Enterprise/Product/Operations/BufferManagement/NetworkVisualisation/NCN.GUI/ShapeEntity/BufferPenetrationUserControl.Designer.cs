namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class BufferPenetrationUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PenetrationItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PenetrationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PenetrationCalculationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PenetrationItemsGrid)).BeginInit();
			this.PenetrationDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel);
			// 
			// HintLabel
			// 
			this.HintLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HintLabel, "PenetrationItemsHint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsHint)));
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.HintLabel.TabIndex = 0;
			// 
			// PenetrationItemsGrid
			// 
			this.PenetrationItemsGrid.AllowNavigation = false;
			this.PenetrationItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PenetrationItemsGrid, "PenetrationItemsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).BufferedItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).BufferedItemRelatedEntityName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).BufferName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).PenetrationPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).IsOverflow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).ScheduledStartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).TimeSinceStartable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).PlannedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).RemainingEstimatedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).BufferDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferedItemBufferPenetrationViewModel)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationItemsCollection)).SyncRoot)).Status)));
			this.PenetrationItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "BufferedItemName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "BufferedItemRelatedEntityName";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "BufferName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "PenetrationPercent";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsOverflow";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "ScheduledStartTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "TimeSinceStartable";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTimeEditExColumnStyleInfo2.AllowNegative = false;
			zTimeEditExColumnStyleInfo2.ColumnName = "PlannedDuration";
			zTimeEditExColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTimeEditExColumnStyleInfo3.AllowNegative = false;
			zTimeEditExColumnStyleInfo3.ColumnName = "RemainingEstimatedDuration";
			zTimeEditExColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTimeEditExColumnStyleInfo4.AllowNegative = false;
			zTimeEditExColumnStyleInfo4.ColumnName = "BufferDuration";
			zTimeEditExColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo5.ColumnName = "Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PenetrationItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PenetrationItemsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo2);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo3);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo4);
			this.PenetrationItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PenetrationItemsGrid.CopySelectedRowsAllowed = true;
			this.PenetrationItemsGrid.GridId = "0864561b-b57c-4d3c-aad6-9d615781dd66";
			this.PenetrationItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PenetrationItemsGrid.LayoutKey = "PenetrationItemsGrid";
			this.PenetrationItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 97, true);
			this.PenetrationItemsGrid.Name = "PenetrationItemsGrid";
			this.PenetrationItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 218, true);
			this.PenetrationItemsGrid.TabIndex = 1;
			// 
			// PenetrationDetailsGroupBox
			// 
			this.PenetrationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PenetrationDetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("7acb0afe-10ac-4d3b-b92c-985fc4bb0bb5", "Penetration Details");
			this.PenetrationDetailsGroupBox.Controls.Add(this.PenetrationCalculationLabel);
			this.PenetrationDetailsGroupBox.Controls.Add(this.HintLabel);
			this.PenetrationDetailsGroupBox.Controls.Add(this.PenetrationItemsGrid);
			this.PenetrationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PenetrationDetailsGroupBox.Name = "PenetrationDetailsGroupBox";
			this.PenetrationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 321, true);
			this.PenetrationDetailsGroupBox.TabIndex = 2;
			this.PenetrationDetailsGroupBox.TabStop = false;
			// 
			// PenetrationCalculationLabel
			// 
			this.PenetrationCalculationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PenetrationCalculationLabel, "PenetrationCalculationHint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BufferPenetrationViewModel)(null)).PenetrationCalculationHint)));
			this.PenetrationCalculationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 37, true);
			this.PenetrationCalculationLabel.Name = "PenetrationCalculationLabel";
			this.PenetrationCalculationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 53, true);
			this.PenetrationCalculationLabel.TabIndex = 2;
			// 
			// BufferPenetrationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PenetrationDetailsGroupBox);
			this.Name = "BufferPenetrationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 327, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PenetrationItemsGrid)).EndInit();
			this.PenetrationDetailsGroupBox.ResumeLayout(false);
			this.PenetrationDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZLabel HintLabel;
		private ZArchitecture.ZGrid PenetrationItemsGrid;
		private ZArchitecture.GUI.ZGroupBox PenetrationDetailsGroupBox;
		private ZArchitecture.ZLabel PenetrationCalculationLabel;
	}
}
