using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.GUI
{
	public partial class DeclarationFromAirCargoForm : ZChildForm, INotifications
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.ZLabel MasterBillLabel;
		Enterprise.ZArchitecture.ZLabel MasterBillNumLabel;
		CargoWise.Windows.UI.KProgressBar ProgressBar;
		Enterprise.ZArchitecture.ZTextBox ProgressTextBox;
		Enterprise.ZArchitecture.GUI.ZButton CreateButton;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;

		protected override void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillNumLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MasterBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.ProgressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.DeclarationsFromCusMAWBCreator);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.MasterBillNumLabel);
			this.zGroupBox1.Controls.Add(this.MasterBillLabel);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 37, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// MasterBillNumLabel
			// 
			this.MasterBillNumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MasterBillNumLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 15, true);
			this.MasterBillNumLabel.Name = "MasterBillNumLabel";
			this.MasterBillNumLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 19, true);
			this.MasterBillNumLabel.TabIndex = 1;
			// 
			// MasterBillLabel
			// 
			this.MasterBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.MasterBillLabel.Name = "MasterBillLabel";
			this.MasterBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 19, true);
			this.MasterBillLabel.TabIndex = 0;
			this.MasterBillLabel.Text = "Masterbill:";
			// 
			// ProgressBar
			// 
			this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 21, true);
			this.ProgressBar.TabIndex = 2;
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProgressTextBox, "ProgressLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.DeclarationsFromCusMAWBCreator)(null)).ProgressLog)));
			this.ProgressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 202, true);
			this.ProgressTextBox.TabIndex = 3;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 268, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Text = "Cl&ose";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 268, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CreateButton.TabIndex = 4;
			this.CreateButton.Text = "&Create";
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// DeclarationFromAirCargoForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 317, true);
			this.Controls.Add(this.CreateButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.ProgressBar);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceAssemblyName = "ZClientTNT";
			this.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.DeclarationsFromCusMAWBCreator);
			this.DataSourceTypeName = "Enterprise.Client.TNT.AirCargo.DeclarationsFromCusMAWBCreator";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 319, true);
			this.Name = "DeclarationFromAirCargoForm";
			this.Text = "Auto Create Declarations for Master";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DeclarationFromAirCargoForm_Closing);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CreateButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
