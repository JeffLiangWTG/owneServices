using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AE.Manifest.GUI
{
	partial class BillsSelectionDialog
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
		protected new void InitializeComponent()
		{
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BillsSubjectUserControl = new BillsSubjectUserControl();
			this.CaptionResourceString = Res.GetData("f5b5e9e8-5784-4f7c-8bd6-4c2914997dc0", "Sending Messages");
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SplitContainer
			//
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ItemsGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.BillsSubjectUserControl);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 200, true);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(212);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(97);
			this.SplitContainer.TabIndex = 1;
			// 
			// BillsSubjectUserControl
			// 
			this.BillsSubjectUserControl.AllowDrop = true;
			this.BillsSubjectUserControl.AutoScroll = true;
			this.BillsSubjectUserControl.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 200, true);
			this.BindingSource.SetBindingMember(this.BillsSubjectUserControl, "ChooserItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AE.Manifest.Business.MessageChooserItem)(((Enterprise.Customs.AE.Manifest.Business.MessageChooserItem)(((System.Collections.IList)(((Enterprise.Customs.AE.Manifest.Business.MessageChooser)(null)).ChooserItems)).SyncRoot)))));
			this.BillsSubjectUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsSubjectUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillsSubjectUserControl.Name = "asycudaBillUserControl";
			this.BillsSubjectUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 200, true);
			this.BillsSubjectUserControl.TabIndex = 2;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.GridId = "1DDED6B0-FD0A-4032-9A5E-62B0FFFCD46B";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 243, true);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// BillsSelectionDialog
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Manifest.Business.MessageChooser);
			this.DataSourceType = typeof(Enterprise.Customs.AE.Manifest.Business.MessageChooser);
			this.DataSourceTypeName = nameof(Enterprise.Customs.AE.Manifest.Business.MessageChooser);
			this.Controls.Add(this.SplitContainer);
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.BillsSubjectUserControl.ResumeLayout(false);
			this.BillsSubjectUserControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		BillsSubjectUserControl BillsSubjectUserControl;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		#endregion
	}
}
