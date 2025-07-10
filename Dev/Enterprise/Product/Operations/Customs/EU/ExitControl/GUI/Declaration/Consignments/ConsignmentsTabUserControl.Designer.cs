namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentsTabUserControl
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
			this.components = new System.ComponentModel.Container();
			this.ConsignmentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ConsignmentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ConsignmentItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignmentItemsTabUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ConsignmentItemsTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentsSplitContainer)).BeginInit();
			this.ConsignmentsSplitContainer.Panel2.SuspendLayout();
			this.ConsignmentsSplitContainer.SuspendLayout();
			this.ConsignmentTabControl.SuspendLayout();
			this.ConsignmentItemsTabPage.SuspendLayout();
			this.ConsignmentItemsTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment>);
			// 
			// ConsignmentsSplitContainer
			// 
			this.ConsignmentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentsSplitContainer.Name = "ConsignmentsSplitContainer";
			this.ConsignmentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ConsignmentsSplitContainer.Panel2
			// 
			this.ConsignmentsSplitContainer.Panel2.Controls.Add(this.ConsignmentTabControl);
			this.ConsignmentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 747, true);
			this.ConsignmentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(260);
			this.ConsignmentsSplitContainer.TabIndex = 0;
			// 
			// ConsignmentTabControl
			// 
			this.ConsignmentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsignmentTabControl.Controls.Add(this.ConsignmentItemsTabPage);
			this.ConsignmentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentTabControl.Name = "ConsignmentTabControl";
			this.ConsignmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 483, true);
			this.ConsignmentTabControl.TabIndex = 0;
			// 
			// ConsignmentItemsTabPage
			// 
			this.ConsignmentItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("63DC6BC1-8E1D-4A56-997B-D95892612E48", "Items");
			this.ConsignmentItemsTabPage.Controls.Add(this.ConsignmentItemsTabUserControl);
			this.ConsignmentItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsignmentItemsTabPage.Name = "ConsignmentItemsTabPage";
			this.ConsignmentItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 456, true);
			this.ConsignmentItemsTabPage.TabIndex = 1;
			// 
			// ConsignmentItemsTabUserControl
			// 
			this.ConsignmentItemsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentItemsTabUserControl, "CusExitConsignmentItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentItemCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem>)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)));
			this.ConsignmentItemsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentItemsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentItemsTabUserControl.Name = "ConsignmentItemsTabUserControl";
			this.ConsignmentItemsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 456, true);
			this.ConsignmentItemsTabUserControl.TabIndex = 0;
			// 
			// ConsignmentsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsignmentsSplitContainer);
			this.Name = "ConsignmentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignmentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentsSplitContainer)).EndInit();
			this.ConsignmentsSplitContainer.ResumeLayout(false);
			this.ConsignmentsSplitContainer.PerformLayout();
			this.ConsignmentTabControl.ResumeLayout(false);
			this.ConsignmentTabControl.PerformLayout();
			this.ConsignmentItemsTabPage.ResumeLayout(false);
			this.ConsignmentItemsTabPage.PerformLayout();
			this.ConsignmentItemsTabUserControl.ResumeLayout(true);
			this.ConsignmentItemsTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public CargoWise.Windows.UI.KSplitContainer ConsignmentsSplitContainer;
		public ZArchitecture.GUI.ZTabControl ConsignmentTabControl;
		public ZArchitecture.GUI.ZTabPage ConsignmentItemsTabPage;
		internal ConsignmentItemsTabUserControl ConsignmentItemsTabUserControl;
	}
}

