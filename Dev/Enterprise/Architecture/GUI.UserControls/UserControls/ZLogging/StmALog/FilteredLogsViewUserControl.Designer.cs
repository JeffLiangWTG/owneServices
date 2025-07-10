namespace Enterprise.ZArchitecture.GUI
{
	partial class FilteredLogsViewUserControl
	{
		#region Component Designer generated code

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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.eventsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitter = new CargoWise.Windows.UI.KSplitter();
			this.sourceInfoUserControl = new Enterprise.ZArchitecture.GUI.EventSourceInfoUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eventsGrid)).BeginInit();
			this.eventsGrid.SuspendLayout();
			this.sourceInfoUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.FilteredLogsView);
			// 
			// eventsGrid
			// 
			this.eventsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.eventsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.StmALog)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_SE_NKEvent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_EventTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ZArchitecture.Business.StmALog)(null)).PostedLocalBranchTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_EventDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_IsCancelled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_IsEstimate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_ReferenceForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).DisplayEventReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SL_UserNameAndInitials)));
			this.eventsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "SL_SE_NKEvent";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDateEditColumnStyleInfo1.ColumnName = "SL_EventTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("7b0451e8-4c99-43ee-b004-870a0be256e3", "Posted Time (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "PostedLocalBranchTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ad6b3832-2b8d-4e56-9d0b-6f1be0775140", "Event Name");
			zTextBoxColumnStyleInfo2.ColumnName = "SL_EventDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "SL_IsCancelled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zCheckBoxColumnStyleInfo2.ColumnName = "SL_IsEstimate";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxColumnStyleInfo3.ColumnName = "SL_ReferenceForBinding";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("69b038ad-1d43-466f-8f75-df5080427546", "Event Details");
			zTextBoxColumnStyleInfo4.ColumnName = "DisplayEventReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("bb415d5a-b609-4874-8970-48fe57fa79ab", "User");
			zTextBoxColumnStyleInfo5.ColumnName = "SL_UserNameAndInitials";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.eventsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.eventsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.eventsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.eventsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.eventsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.eventsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.eventsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.eventsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.eventsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.eventsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventsGrid.GridId = "fb6391d4-1f2d-4f02-8d99-26f911fb6b4a";
			this.eventsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.eventsGrid.LayoutKey = "zGrid1";
			this.eventsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eventsGrid.Name = "eventsGrid";
			this.eventsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 416, true);
			this.eventsGrid.TabIndex = 0;
			// 
			// splitter
			//
			splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			splitter.Name = "splitter";
			// 
			// sourceInfoUserControl
			// 
			this.sourceInfoUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sourceInfoUserControl, ".");
			this.sourceInfoUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.sourceInfoUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 239, true);
			this.sourceInfoUserControl.Name = "sourceInfoUserControl";
			this.sourceInfoUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 177, true);
			this.sourceInfoUserControl.TabIndex = 3;
			this.sourceInfoUserControl.Visible = false;
			// 
			// FilteredEventsEventsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitter);
			this.Controls.Add(this.sourceInfoUserControl);
			this.Controls.Add(this.eventsGrid);
			this.Name = "FilteredEventsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.eventsGrid)).EndInit();
			this.eventsGrid.ResumeLayout(false);
			this.eventsGrid.PerformLayout();
			this.sourceInfoUserControl.ResumeLayout(true);
			this.sourceInfoUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public EventSourceInfoUserControl sourceInfoUserControl;
		private ZArchitecture.ZGrid eventsGrid;		
		private CargoWise.Windows.UI.KSplitter splitter;
	}
}
