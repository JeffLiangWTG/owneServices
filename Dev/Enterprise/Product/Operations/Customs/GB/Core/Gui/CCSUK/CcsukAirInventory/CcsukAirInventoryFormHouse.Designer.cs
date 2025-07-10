using Enterprise.MasterFiles.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukAirInventoryFormHouse
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
			this.ccsukHawbControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukHawbControl(); 
			this.WorkflowTabPage = new ZWorkflowTabPage();			
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();


			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.WorkflowTabPage.TabIndex = 8;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 277, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 453, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ccsukHawbControl1); 
			MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 426, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// ccsukHawbControl1
			// 
			this.ccsukHawbControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukHawbControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)))));
			this.ccsukHawbControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ccsukHawbControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukHawbControl1.Name = "ccsukHawbControl1";
			this.ccsukHawbControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 426, true);
			this.ccsukHawbControl1.TabIndex = 0;
			// 
			// CcsukAirInventoryFormHouse
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 509, true);
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirInventoryFormHouse|74879936-e6df-48f5-8eef-b8b6f0e4b373", "CCSUK House Bill");
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			this.Name = "CcsukAirInventoryFormHouse";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CcsukHawbControl ccsukHawbControl1; 
		ZWorkflowTabPage WorkflowTabPage;

	}
}
