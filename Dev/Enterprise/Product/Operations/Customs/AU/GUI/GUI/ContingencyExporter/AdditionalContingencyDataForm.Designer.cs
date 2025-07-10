using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.GUI
{
    public partial class AdditionalContingencyDataForm
    {
		protected override void InitializeComponent()
		{
			this.YesButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.noButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.originPremiseIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.originPremiseIDExpain = new Enterprise.ZArchitecture.ZLabel();
			this.originPremiseID = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 197, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 9, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AdditionalContingencyData);
			// 
			// YesButtonX
			// 
			this.YesButtonX.CaptionResourceString = null;
			this.YesButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 155, true);
			this.YesButtonX.Name = "YesButtonX";
			this.YesButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.YesButtonX.TabIndex = 2;
			this.YesButtonX.Text = "Yes";
			this.YesButtonX.Click += new System.EventHandler(this.YesButtonX_Click);
			// 
			// NoButtonX
			// 
			this.noButtonX.CaptionResourceString = null;
			this.noButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 155, true);
			this.noButtonX.Name = "NoButtonX";
			this.noButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.noButtonX.TabIndex = 3;
			this.noButtonX.Text = "No";
			this.noButtonX.Click += new System.EventHandler(this.NoButtonX_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = null;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 122, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.zLabel2.TabIndex = 10;
			this.zLabel2.Text = "If you choose \'No\', the file will be saved to disk.";
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = null;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 102, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 20, true);
			this.zLabel1.TabIndex = 11;
			this.zLabel1.Text = "Do you want to email the file to Customs automatically?";
			// 
			// OriginPremiseIDLabel
			// 
			this.originPremiseIDLabel.CaptionResourceString = null;
			this.originPremiseIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 57, true);
			this.originPremiseIDLabel.Name = "OriginPremiseIDLabel";
			this.originPremiseIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 22, true);
			this.originPremiseIDLabel.TabIndex = 12;
			this.originPremiseIDLabel.Text = "Originating Establishment ID";
			// 
			// Cancel
			// 
			this.cancel.CaptionResourceString = null;
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 155, true);
			this.cancel.Name = "Cancel";
			this.cancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.cancel.TabIndex = 4;
			this.cancel.Text = "Cancel";
			// 
			// OriginPremiseIDExpain
			// 
			this.originPremiseIDExpain.CaptionResourceString = null;
			this.originPremiseIDExpain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 14, true);
			this.originPremiseIDExpain.Name = "OriginPremiseIDExpain";
			this.originPremiseIDExpain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 36, true);
			this.originPremiseIDExpain.TabIndex = 13;
			this.originPremiseIDExpain.Text = "This is the Customs Establishment ID of the location from where the cargo will be" +
	" released.";
			// 
			// OriginPremiseID
			// 
			this.originPremiseID.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originPremiseID, "OriginPremise");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AdditionalContingencyData)(null)).OriginPremise)));
			this.originPremiseID.CaptionResourceString = null;
			this.originPremiseID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 59, true);
			this.originPremiseID.Name = "OriginPremiseID";
			this.originPremiseID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.originPremiseID.TabIndex = 1;
			// 
			// AdditionalContingencyDataForm
			// 

			this.CancelButton = this.cancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 206, true);
			this.Controls.Add(this.originPremiseID);
			this.Controls.Add(this.originPremiseIDExpain);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.originPremiseIDLabel);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.noButtonX);
			this.Controls.Add(this.YesButtonX);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AdditionalContingencyData);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AdditionalContingencyData";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "AdditionalContingencyDataForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Additional Customs Contingency Data";
			this.Controls.SetChildIndex(this.YesButtonX, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.noButtonX, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.originPremiseIDLabel, 0);
			this.Controls.SetChildIndex(this.cancel, 0);
			this.Controls.SetChildIndex(this.originPremiseIDExpain, 0);
			this.Controls.SetChildIndex(this.originPremiseID, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		internal ZButton YesButtonX;
		private ZButton noButtonX;
		private ZLabel zLabel2;
		private ZLabel zLabel1;
		internal ZLabel originPremiseIDLabel;
		private ZButton cancel;
		internal ZLabel originPremiseIDExpain;
		internal ZCodeFindBox originPremiseID;
	}
}
