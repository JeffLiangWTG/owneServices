using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class RTSTranshipmentForm : Enterprise.Client.UPE.GUI.CustomFlagForm
	{
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZLabel zLabel11;
		Enterprise.ZArchitecture.ZLabel zLabel12;
		Enterprise.ZArchitecture.ZLabel zLabel13;
		Enterprise.ZArchitecture.ZLabel zLabel19;
		Enterprise.ZArchitecture.ZTextBox StateTextBox;
		Enterprise.ZArchitecture.ZTextBox Address1TextBox;
		Enterprise.ZArchitecture.ZTextBox CityTextBox;
		Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		Enterprise.ZArchitecture.ZTextBox NameTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox2;
		Enterprise.ZArchitecture.ZTextBox Address2TextBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 232, true);
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 256, true);
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 280, true);
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 256, true);
			this.zTextBox1.TabIndex = 4;
			// 
			// zTextBox2
			// 
			this.zTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 280, true);
			this.zTextBox2.TabIndex = 5;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 232, true);
			this.zDropEdit1.TabIndex = 3;
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 384, true);
			this.OKButton.TabIndex = 6;
			// 
			// btnCancelButton
			// 
			this.btnCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 384, true);
			this.btnCancelButton.TabIndex = 7;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 416, true);
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel4.TabIndex = 5;
			this.zLabel4.Text = "Origin";
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel5.TabIndex = 5;
			this.zLabel5.Text = "Destination";
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.zLabel8);
			this.zGroupBox2.Controls.Add(this.zLabel10);
			this.zGroupBox2.Controls.Add(this.zLabel11);
			this.zGroupBox2.Controls.Add(this.zLabel12);
			this.zGroupBox2.Controls.Add(this.zLabel13);
			this.zGroupBox2.Controls.Add(this.zLabel19);
			this.zGroupBox2.Controls.Add(this.StateTextBox);
			this.zGroupBox2.Controls.Add(this.Address1TextBox);
			this.zGroupBox2.Controls.Add(this.CityTextBox);
			this.zGroupBox2.Controls.Add(this.PostCodeTextBox);
			this.zGroupBox2.Controls.Add(this.NameTextBox);
			this.zGroupBox2.Controls.Add(this.Address2TextBox);
			this.zGroupBox2.Controls.Add(this.CountryFindBox);
			this.zGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 168, true);
			this.zGroupBox2.TabIndex = 2;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Return Address";
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel8.TabIndex = 1;
			this.zLabel8.Text = "Name";
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel10.TabIndex = 1;
			this.zLabel10.Text = "Address";
			// 
			// zLabel11
			// 
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel11.TabIndex = 1;
			this.zLabel11.Text = "City";
			// 
			// zLabel12
			// 
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 136, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel12.TabIndex = 1;
			this.zLabel12.Text = "Post Code";
			// 
			// zLabel13
			// 
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel13.TabIndex = 1;
			this.zLabel13.Text = "State";
			// 
			// zLabel19
			// 
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel19.TabIndex = 1;
			this.zLabel19.Text = "Country/Region";
			// 
			// StateTextBox
			// 
			this.StateTextBox.BindTo = "State";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).StateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).State)));
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 136, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.StateTextBox.TabIndex = 5;
			// 
			// Address1TextBox
			// 
			this.Address1TextBox.BindTo = "Street";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).StreetInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Street)));
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.Address1TextBox.TabIndex = 1;
			// 
			// CityTextBox
			// 
			this.CityTextBox.BindTo = "City";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).CityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).City)));
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 88, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.CityTextBox.TabIndex = 3;
			// 
			// PostCodeTextBox
			// 
			this.PostCodeTextBox.BindTo = "PostCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).PostCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).PostCode)));
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 136, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PostCodeTextBox.TabIndex = 6;
			// 
			// NameTextBox
			// 
			this.NameTextBox.BindTo = "Name";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).NameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Name)));
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 16, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.BindTo = "Country";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).CountryInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Country)));
			this.CountryFindBox.BindToList = "Lookups+CountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Lookups.CountryList)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 112, true);
			this.CountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 2;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.CountryFindBox.TabIndex = 4;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.BindTo = "OriginPort";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).OriginPortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).OriginPort)));
			this.zCodeFindBox1.BindToList = "Lookups+OriginPortList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Lookups.OriginPortList)));
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 5;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.zCodeFindBox1.TabIndex = 0;
			// 
			// zCodeFindBox2
			// 
			this.zCodeFindBox2.BindTo = "DestinationPort";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).DestinationPortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).DestinationPort)));
			this.zCodeFindBox2.BindToList = "Lookups+DestinationPortList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Lookups.DestinationPortList)));
			this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 32, true);
			this.zCodeFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.zCodeFindBox2.Name = "zCodeFindBox2";
			this.zCodeFindBox2.PreBoundMaxLength = 5;
			this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.zCodeFindBox2.TabIndex = 1;
			// 
			// Address2TextBox
			// 
			this.Address2TextBox.BindTo = "Street2";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Street2Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.RTSTranshipmentDetails)(null)).Street2)));
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 64, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.Address2TextBox.TabIndex = 2;
			// 
			// RTSTranshipmentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 440, true);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zCodeFindBox1);
			this.Controls.Add(this.zCodeFindBox2);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.RTSTranshipmentDetails";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 472, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 472, true);
			this.Name = "RTSTranshipmentForm";
			this.Controls.SetChildIndex(this.zCodeFindBox2, 0);
			this.Controls.SetChildIndex(this.zCodeFindBox1, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.btnCancelButton, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
