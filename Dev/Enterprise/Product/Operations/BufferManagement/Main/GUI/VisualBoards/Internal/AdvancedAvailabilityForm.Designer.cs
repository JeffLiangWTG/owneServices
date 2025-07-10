namespace Enterprise.BufferManagement.GUI
{
	partial class AdvancedAvailabilityForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bmsLeaveGrid = new Enterprise.ZArchitecture.ZGrid();
			this.futureLeaveGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.leaveViewNotAllowedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bmsLeaveGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.futureLeaveGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("cdb6d300-f6c5-42dc-83ab-c37031022aa0", "Save");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 408, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.UseVisualStyleBackColor = false;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// bmsLeaveGrid
			// 
			this.bmsLeaveGrid.AllowNavigation = false;
			this.bmsLeaveGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.bmsLeaveGrid, "BMSLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).BMSLeave)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.BMSResourceAvailabilityOverride)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).BMSLeave)).SyncRoot)).GA_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.BMSResourceAvailabilityOverride)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).BMSLeave)).SyncRoot)).GA_EndTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMSResourceAvailabilityOverride)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).BMSLeave)).SyncRoot)).GA_AvailabilityPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMSResourceAvailabilityOverride)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).BMSLeave)).SyncRoot)).GA_LeaveComment)));
			this.bmsLeaveGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4f07144b-ed73-4082-85d4-26b065ad37e7", "Start Time");
			zDateEditColumnStyleInfo1.ColumnName = "GA_StartTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ebd83151-45a3-4bfb-bdd5-05d595d7dddd", "End Time");
			zDateEditColumnStyleInfo2.ColumnName = "GA_EndTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ab863953-1bee-47f9-8540-4d499a476ca5", "Availability Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "GA_AvailabilityPercentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c08d6d02-6502-42bb-a233-59aafdcda95a", "Leave Comment");
			zTextBoxColumnStyleInfo1.ColumnName = "GA_LeaveComment";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.bmsLeaveGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.bmsLeaveGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.bmsLeaveGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.bmsLeaveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.bmsLeaveGrid.CopySelectedRowsAllowed = true;
			this.bmsLeaveGrid.GridId = "8e4c666e-ab8b-4196-a967-4f22338aded0";
			this.bmsLeaveGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.bmsLeaveGrid.LayoutKey = "bmsLeaveGrid";
			this.bmsLeaveGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.bmsLeaveGrid.Name = "bmsLeaveGrid";
			this.bmsLeaveGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 161, true);
			this.bmsLeaveGrid.TabIndex = 2;
			// 
			// futureLeaveGrid
			// 
			this.futureLeaveGrid.AllowNavigation = false;
			this.futureLeaveGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.futureLeaveGrid, "NearFutureLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_WorkHolidayType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_EndTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_DaysLeaveTaken)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_LeaveComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbStaffHoliday)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel)(null)).NearFutureLeave)).SyncRoot)).GA_IsWorkingAway)));
			this.futureLeaveGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("69a4c639-906d-4f92-9d33-0638b42cd5e2", "Holiday Type");
			zTextBoxColumnStyleInfo2.ColumnName = "GA_WorkHolidayType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bcd4c751-9564-4605-8e4b-34e67efa1242", "Holiday Type Description");
			zTextBoxColumnStyleInfo3.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d8eb49d2-b6e2-4f70-80cf-9cd64ea393dd", "Start Time");
			zDateEditColumnStyleInfo3.ColumnName = "GA_StartTime";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("f8107362-f1c3-4bc1-8228-74c8e807cc95", "End Time");
			zDateEditColumnStyleInfo4.ColumnName = "GA_EndTime";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b9b7f4b1-9f7b-4b1d-96d3-a917dd4376dd", "Days Taken");
			zCalcEditColumnStyleInfo2.ColumnName = "GA_DaysLeaveTaken";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8e02dc0a-4322-4771-8725-1b01982b5950", "Leave Comment");
			zTextBoxColumnStyleInfo4.ColumnName = "GA_LeaveComment";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b1693137-2129-4d3c-9f3c-142432e6e658", "Is Working Away");
			zCheckBoxColumnStyleInfo1.ColumnName = "GA_IsWorkingAway";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.futureLeaveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.futureLeaveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.futureLeaveGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.futureLeaveGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.futureLeaveGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.futureLeaveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.futureLeaveGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.futureLeaveGrid.CopySelectedRowsAllowed = true;
			this.futureLeaveGrid.GridId = "c6749b38-23ad-4a99-a2e5-462c236b265e";
			this.futureLeaveGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.futureLeaveGrid.LayoutKey = "futureLeaveGrid";
			this.futureLeaveGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.futureLeaveGrid.Name = "futureLeaveGrid";
			this.futureLeaveGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 181, true);
			this.futureLeaveGrid.TabIndex = 3;
			// 
			// FilterRulesHintLabel
			// 
			this.leaveViewNotAllowedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.leaveViewNotAllowedLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("cfaf0bb0-f7fb-44e2-a346-98e8988fa9a7", "You do not have sufficient privilege to access this section. Contact your system administrator for more details.");
			this.leaveViewNotAllowedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.leaveViewNotAllowedLabel.ForeColor = System.Drawing.Color.Red;
			this.leaveViewNotAllowedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.leaveViewNotAllowedLabel.Name = "LeaveViewNotAllowedLabel";
			this.leaveViewNotAllowedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1173, 16, true);
			this.leaveViewNotAllowedLabel.Visible = false;
			this.leaveViewNotAllowedLabel.TabIndex = 10;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("da924b19-1257-4d2c-8ec1-b76f9453235f", "BMS Leave");
			this.zGroupBox1.Controls.Add(this.bmsLeaveGrid);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 186, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("db2ee232-7c0f-45ec-ad4f-3d007b036c0f", "Leave in the next 30 days");
			this.zGroupBox2.Controls.Add(this.futureLeaveGrid);
			this.zGroupBox2.Controls.Add(this.leaveViewNotAllowedLabel);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 195, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 206, true);
			this.zGroupBox2.TabIndex = 5;
			this.zGroupBox2.TabStop = false;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("a5d8a007-bfc4-4f4c-b393-3f1ffccb12c8", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 408, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// AdvancedAvailabilityForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d1a943f8-2fd1-4986-8899-47e52b0b7bf3", "Advanced BMS Leave");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 461, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.okButton);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.ResourceAvailabilityOverrideViewModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 500, true);
			this.Name = "AdvancedAvailabilityForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bmsLeaveGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.futureLeaveGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.ZGrid bmsLeaveGrid;
		private ZArchitecture.ZGrid futureLeaveGrid;
		private ZArchitecture.ZLabel leaveViewNotAllowedLabel;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
