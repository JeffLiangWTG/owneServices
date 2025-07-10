using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CMRDepotShipmentUserControl
	{
		private void InitializeComponent()
		{
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.impendingArrivalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 8, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.TabIndex = 11;
			this.zLabel3.Text = "Cargo Status Advice:";
			// 
			// ImpendingArrivalLabel
			// 
			this.impendingArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.impendingArrivalLabel.Name = "ImpendingArrivalLabel";
			this.impendingArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.impendingArrivalLabel.TabIndex = 10;
			this.impendingArrivalLabel.Text = "Expected Cargo Arrival Advice:";
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.BindTo = "CSAMessage";
			this.zGuidFindBox2.BindToList = "CSAMessage_List";
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 8, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox2.TabIndex = 9;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.BindTo = "IMPMessage";
			this.zGuidFindBox1.BindToList = "IMPMessage_List";
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 8, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox1.TabIndex = 8;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.zGuidFindBox1);
			this.panel1.Controls.Add(this.impendingArrivalLabel);
			this.panel1.Controls.Add(this.zLabel3);
			this.panel1.Controls.Add(this.zGuidFindBox2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 40, true);
			this.panel1.TabIndex = 12;
			// 
			// CMRDepotShipmentUserControl
			//
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.panel1);
			this.Name = "CMRDepotShipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 472, true);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel impendingArrivalLabel;
		private ZGuidFindBox zGuidFindBox2;
		private ZGuidFindBox zGuidFindBox1;
		private CargoWise.Windows.UI.KPanel panel1;
	}
}
