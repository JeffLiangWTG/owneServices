using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Modules.AirCargo
{
	public partial class SoundexTesterForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZTextBox cp1Name;
		Enterprise.ZArchitecture.ZTextBox cp1City;
		Enterprise.ZArchitecture.ZTextBox cp2Address1;
		Enterprise.ZArchitecture.ZTextBox cp1Address1;
		Enterprise.ZArchitecture.ZTextBox cp2City;
		Enterprise.ZArchitecture.ZTextBox cp2State;
		Enterprise.ZArchitecture.ZTextBox cp1State;
		ZButton zButton1;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel scoreLabel;
		Enterprise.ZArchitecture.ZTextBox cp1Postcode;
		Enterprise.ZArchitecture.ZTextBox cp2Postcode;
		Enterprise.ZArchitecture.ZTextBox cp1Address2;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel6;
		Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZTextBox cp1Phone;
		Enterprise.ZArchitecture.ZTextBox cp2Phone;
		Enterprise.ZArchitecture.ZTextBox cp2Address2;
		Enterprise.ZArchitecture.ZTextBox cp2Name;

		protected override void InitializeComponent()
		{
			this.cp1Name = new Enterprise.ZArchitecture.ZTextBox();
			this.cp1City = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2Address1 = new Enterprise.ZArchitecture.ZTextBox();
			this.cp1Address1 = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2Name = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2City = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2State = new Enterprise.ZArchitecture.ZTextBox();
			this.cp1State = new Enterprise.ZArchitecture.ZTextBox();
			this.zButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.scoreLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cp1Postcode = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2Postcode = new Enterprise.ZArchitecture.ZTextBox();
			this.cp1Address2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.cp1Phone = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2Phone = new Enterprise.ZArchitecture.ZTextBox();
			this.cp2Address2 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 24, true);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 122, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.zLabel1.Text = "Score:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 20, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 21, true);
			this.zLabel2.Text = "Name";
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 45, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.zLabel3.Text = "Street 1";
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 72, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zLabel4.Text = "Street 2";
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 99, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zLabel5.Text = "City";
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 124, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zLabel6.Text = "State";
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 150, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zLabel7.Text = "Postcode";
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 176, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zLabel8.Text = "Phone";
			// 
			// scoreLabel
			// 
			this.scoreLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 115, true);
			this.scoreLabel.Name = "scoreLabel";
			this.scoreLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 38, true);
			// 
			// cp1Name
			// 
			this.cp1Name.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1Name.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 21, true);
			this.cp1Name.Name = "cp1Name";
			this.cp1Name.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1Name.TabIndex = 1;

			// 
			// cp1Address1
			// 
			this.cp1Address1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1Address1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 47, true);
			this.cp1Address1.Name = "cp1Address1";
			this.cp1Address1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1Address1.TabIndex = 2;
			// 
			// cp1Address2
			// 
			this.cp1Address2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1Address2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 73, true);
			this.cp1Address2.Name = "cp1Address2";
			this.cp1Address2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1Address2.TabIndex = 3;

			// 
			// cp1City
			// 
			this.cp1City.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1City.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 99, true);
			this.cp1City.Name = "cp1City";
			this.cp1City.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1City.TabIndex = 4;
			// 
			// cp1State
			// 
			this.cp1State.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1State.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 125, true);
			this.cp1State.Name = "cp1State";
			this.cp1State.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1State.TabIndex = 5;
			// 
			// cp1Postcode
			// 
			this.cp1Postcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1Postcode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 151, true);
			this.cp1Postcode.Name = "cp1Postcode";
			this.cp1Postcode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1Postcode.TabIndex = 6;
			// 
			// cp1Phone
			// 
			this.cp1Phone.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp1Phone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 177, true);
			this.cp1Phone.Name = "cp1Phone";
			this.cp1Phone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp1Phone.TabIndex = 7;
			// 
			// zButton1
			// 
			this.zButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 79, true);
			this.zButton1.Name = "zButton1";
			this.zButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 29, true);
			this.zButton1.TabIndex = 8;
			this.zButton1.Text = "Compare";
			this.zButton1.UseVisualStyleBackColor = true;
			this.zButton1.Click += new System.EventHandler(this.zButton1_Click);
			// 
			// cp2Name
			// 
			this.cp2Name.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2Name.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 21, true);
			this.cp2Name.Name = "cp2Name";
			this.cp2Name.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2Name.TabIndex = 9;
			// 
			// cp2Address1
			// 
			this.cp2Address1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2Address1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 47, true);
			this.cp2Address1.Name = "cp2Address1";
			this.cp2Address1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2Address1.TabIndex = 10;

			// 
			// cp2Address2
			// 
			this.cp2Address2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2Address2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 73, true);
			this.cp2Address2.Name = "cp2Address2";
			this.cp2Address2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2Address2.TabIndex = 11;
			// 
			// cp2City
			// 
			this.cp2City.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2City.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 99, true);
			this.cp2City.Name = "cp2City";
			this.cp2City.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2City.TabIndex = 12;
			// 
			// cp2State
			// 
			this.cp2State.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2State.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 125, true);
			this.cp2State.Name = "cp2State";
			this.cp2State.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2State.TabIndex = 13;
			// 
			// cp2Postcode
			// 
			this.cp2Postcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2Postcode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 151, true);
			this.cp2Postcode.Name = "cp2Postcode";
			this.cp2Postcode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2Postcode.TabIndex = 14;

			// 
			// cp2Phone
			// 
			this.cp2Phone.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cp2Phone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 177, true);
			this.cp2Phone.Name = "cp2Phone";
			this.cp2Phone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.cp2Phone.TabIndex = 15;

			// 
			// SoundexTesterForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 259, true);
			this.Controls.Add(this.cp2Address2);
			this.Controls.Add(this.cp2Phone);
			this.Controls.Add(this.cp1Phone);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.zLabel7);
			this.Controls.Add(this.zLabel6);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.cp1Address2);
			this.Controls.Add(this.cp2Postcode);
			this.Controls.Add(this.cp1Postcode);
			this.Controls.Add(this.scoreLabel);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zButton1);
			this.Controls.Add(this.cp1State);
			this.Controls.Add(this.cp2State);
			this.Controls.Add(this.cp2City);
			this.Controls.Add(this.cp2Name);
			this.Controls.Add(this.cp1Address1);
			this.Controls.Add(this.cp2Address1);
			this.Controls.Add(this.cp1City);
			this.Controls.Add(this.cp1Name);
			this.Name = "SoundexTesterForm";
			this.Controls.SetChildIndex(this.cp1Name, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.cp1City, 0);
			this.Controls.SetChildIndex(this.cp2Address1, 0);
			this.Controls.SetChildIndex(this.cp1Address1, 0);
			this.Controls.SetChildIndex(this.cp2Name, 0);
			this.Controls.SetChildIndex(this.cp2City, 0);
			this.Controls.SetChildIndex(this.cp2State, 0);
			this.Controls.SetChildIndex(this.cp1State, 0);
			this.Controls.SetChildIndex(this.zButton1, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.scoreLabel, 0);
			this.Controls.SetChildIndex(this.cp1Postcode, 0);
			this.Controls.SetChildIndex(this.cp2Postcode, 0);
			this.Controls.SetChildIndex(this.cp1Address2, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.zLabel6, 0);
			this.Controls.SetChildIndex(this.zLabel7, 0);
			this.Controls.SetChildIndex(this.zLabel8, 0);
			this.Controls.SetChildIndex(this.cp1Phone, 0);
			this.Controls.SetChildIndex(this.cp2Phone, 0);
			this.Controls.SetChildIndex(this.cp2Address2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
