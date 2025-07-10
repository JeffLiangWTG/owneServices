using Enterprise.MasterFiles.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirInventoryForm
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
			this.ccsukAirConsignmentUserControlMawb1 = new CcsukAirConsignmentUserControlMawb();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 772, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1303, 24, true);
			// 
			// userControl11
			// 
			this.ccsukAirConsignmentUserControlMawb1.AllowDrop = true;
			this.ccsukAirConsignmentUserControlMawb1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ccsukAirConsignmentUserControlMawb1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukAirConsignmentUserControlMawb1.Name = "CcsukMainMasterUserControlForPlugin1";
			this.ccsukAirConsignmentUserControlMawb1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1303, 796, true);
			this.ccsukAirConsignmentUserControlMawb1.TabIndex = 1;


			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.WorkflowTabPage.TabIndex = 8;


			MainTabPage.Controls.Add(ccsukAirConsignmentUserControlMawb1);
			MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabPage.Controls.SetChildIndex(ccsukAirConsignmentUserControlMawb1, 0);


			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1303, 796, true);
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirInventoryForm|74879936-e6df-48f5-8eef-b8b6f0e4b373", "CCSUK Air Inventory");
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			this.Name = "CcsukAirInventoryForm";
			this.Text = "CcsukAirInventoryForm"; 
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CcsukAirConsignmentUserControlMawb ccsukAirConsignmentUserControlMawb1;
		ZWorkflowTabPage WorkflowTabPage;
	}
}
