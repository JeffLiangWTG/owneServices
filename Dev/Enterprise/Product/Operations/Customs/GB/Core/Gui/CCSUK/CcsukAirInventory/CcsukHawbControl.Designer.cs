namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukHawbControl
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
			this.HouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LinkLabelMawb = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.IsSplitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HawbTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WeightOfGoods = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackagesReceived = new ZArchitecture.ZCalcEdit();
			this.PackagesExpected = new ZArchitecture.ZCalcEdit();
			this.DescriptionOfGoods = new Enterprise.Customs.GB.GUI.EdifactUNOATextBox();
			this.ccsukMasterControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMasterControl();
			this.ccsukAirportsAndPartiesControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirportsAndPartiesControl();
			this.ccsukMainHouseUserControlForPlugin1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMainHouseUserControlHelpers();
			this.awbStatusUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.AwbStatusUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HouseGroupBox.SuspendLayout();
			this.GoodsGroupBox.SuspendLayout(); 
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// HouseGroupBox
			// 
			this.HouseGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("39862d78-86b5-4b2e-a4ea-65f68bb93ae1", "House");
			this.HouseGroupBox.Controls.Add(this.LinkLabelMawb);
			this.HouseGroupBox.Controls.Add(this.IsSplitCheckBox);
			this.HouseGroupBox.Controls.Add(this.HawbTextBox);
			this.LabelCaptionRenderProvider.SetLabelTop(this.HouseGroupBox, 0);
			this.HouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 95, true);
			this.HouseGroupBox.Name = "HouseGroupBox";
			this.HouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 63, true);
			this.HouseGroupBox.TabIndex = 1;
			this.HouseGroupBox.TabStop = false;
			// 
			// LinkLabelMawb
			// 
			this.LinkLabelMawb.AutoSize = true;
			this.LinkLabelMawb.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("02c4af3d-bed8-4122-9a56-c2f7b886e03e", "MAWB");
			this.LinkLabelMawb.IsFontBold = false;
			this.LinkLabelMawb.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 41, true);
			this.LinkLabelMawb.Name = "LinkLabelMawb";
			this.LinkLabelMawb.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.LinkLabelMawb.TabIndex = 7;
			this.LinkLabelMawb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelMawb_LinkClicked);
			// 
			// IsSplitCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSplitCheckBox, "HasSplits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).HasSplits)));
			this.IsSplitCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c51cbfc7-ddfb-4e43-b2e2-5100755703fb", "Is Split?");
			this.IsSplitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSplitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 40, true);
			this.IsSplitCheckBox.Name = "IsSplitCheckBox";
			this.IsSplitCheckBox.ReadOnly = true;
			this.IsSplitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 19, true);
			this.IsSplitCheckBox.TabIndex = 1;
			this.IsSplitCheckBox.UseVisualStyleBackColor = true;
			// 
			// HawbTextBox
			// 
			this.BindingSource.SetBindingMember(this.HawbTextBox, "CS_HAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_HAWB)));
			this.HawbTextBox.CaptionResourceString = null;
			this.HawbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 13, true);
			this.HawbTextBox.Name = "HawbTextBox";
			this.HawbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.HawbTextBox.TabIndex = 0;
			// 
			// GoodsGroupBox
			// 
			this.GoodsGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d2465f5d-29b8-4880-ab3a-74e8d42c1760", "Goods");
			this.GoodsGroupBox.Controls.Add(this.WeightOfGoods);
			this.GoodsGroupBox.Controls.Add(this.PackagesReceived);
			this.GoodsGroupBox.Controls.Add(this.PackagesExpected);
			this.GoodsGroupBox.Controls.Add(this.DescriptionOfGoods);
			this.LabelCaptionRenderProvider.SetLabelTop(this.GoodsGroupBox, 0);
			this.GoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 160, true);
			this.GoodsGroupBox.Name = "GoodsGroupBox";
			this.GoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 101, true);
			this.GoodsGroupBox.TabIndex = 2;
			this.GoodsGroupBox.TabStop = false;
			// 
			// WeightOfGoods
			// 
			this.WeightOfGoods.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightOfGoods, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_WeightUQ)));
			this.WeightOfGoods.BindToAmount = "CS_Weight";
			this.WeightOfGoods.BindToUnit = "CS_WeightUQ";
			this.WeightOfGoods.CaptionResourceString = null;
			this.WeightOfGoods.Decimals = 2;
			this.WeightOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 77, true);
			this.WeightOfGoods.Name = "WeightOfGoods";
			this.WeightOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.WeightOfGoods.TabIndex = 3;
			this.WeightOfGoods.UnitPreBoundMaxLength = 2;
			// 
			// PackagesReceived
			// 
			this.BindingSource.SetBindingMember(this.PackagesReceived, "CS_PiecesLanded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_PiecesLanded)));
			this.PackagesReceived.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukHawbControl|d6a89a66-5df2-4b39-9e1e-884de468f5c9", "NPR", "Number of Pieces Received (NPR)", "To set NPR, complete the \'Receipts\' grid. NPR is the number of pieces checked-in so far.");
			this.PackagesReceived.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 55, true);
			this.PackagesReceived.Name = "PackagesReceived";
			this.PackagesReceived.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackagesReceived.TabIndex = 2;
			PackagesReceived.DecimalPlaces = 0;
			PackagesReceived.ReadOnly = true; // Do not allow this line to get removed by the designer. It is vital. 

			// 
			// PackagesExpected
			// 
			this.BindingSource.SetBindingMember(this.PackagesExpected, "CS_PiecesManifested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_PiecesManifested)));
			this.PackagesExpected.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukHawbControl|684974d9-1243-448d-9292-82b7323b78fc", "NPX", "Number of Pieces Expected (NPX)", "Number of pieces expected or manifested on the consignment (NPX)");
			this.PackagesExpected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 32, true);
			this.PackagesExpected.Name = "PackagesExpected";
			this.PackagesExpected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackagesExpected.TabIndex = 1;
			PackagesExpected.DecimalPlaces = 0;
			// 
			// DescriptionOfGoods
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoods, "CS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).CS_GoodsDescription)));
			this.DescriptionOfGoods.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukHawbControl|b0ae375f-41d4-4bd0-b26e-62435d2b1c2a", "Desc.", "Description", "Goods\' Description", "Description of Goods");			
			this.DescriptionOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 10, true);
			this.DescriptionOfGoods.Name = "DescriptionOfGoods";
			this.DescriptionOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.DescriptionOfGoods.TabIndex = 0;
			// 
			// ccsukMasterControl1
			// 
			this.ccsukMasterControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukMasterControl1, ".");
			this.ccsukMasterControl1.CaptionResourceString = null;
			this.ccsukMasterControl1.Enabled = false;
			this.ccsukMasterControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukMasterControl1.Name = "ccsukMasterControl1";
			this.ccsukMasterControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 158, true);
			this.ccsukMasterControl1.TabIndex = 0;
			// 
			// ccsukAirportsAndPartiesControl1
			// 
			this.ccsukAirportsAndPartiesControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukAirportsAndPartiesControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)))));
			this.ccsukAirportsAndPartiesControl1.CaptionResourceString = null;
			this.ccsukAirportsAndPartiesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, -1, true);
			this.ccsukAirportsAndPartiesControl1.Name = "ccsukAirportsAndPartiesControl1";
			this.ccsukAirportsAndPartiesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 158, true);
			this.ccsukAirportsAndPartiesControl1.TabIndex = 4;
			// 
			// ccsukMainHouseUserControlForPlugin1
			// 
			this.ccsukMainHouseUserControlForPlugin1.AllowDrop = true;
			this.ccsukMainHouseUserControlForPlugin1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ccsukMainHouseUserControlForPlugin1, ".");
			this.ccsukMainHouseUserControlForPlugin1.CaptionResourceString = null;
			this.ccsukMainHouseUserControlForPlugin1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 267, true);
			this.ccsukMainHouseUserControlForPlugin1.Name = "ccsukMainHouseUserControlForPlugin1";
			this.ccsukMainHouseUserControlForPlugin1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 331, true);
			this.ccsukMainHouseUserControlForPlugin1.TabIndex = 5;
			// 
			// awbStatusUserControl1
			// 
			this.awbStatusUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.awbStatusUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)))));
			this.awbStatusUserControl1.CaptionResourceString = null;
			this.awbStatusUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 158, true);
			this.awbStatusUserControl1.Name = "awbStatusUserControl1";
			this.awbStatusUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 105, true);
			this.awbStatusUserControl1.TabIndex = 6;
			// 
			// CcsukHawbControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.awbStatusUserControl1);
			this.Controls.Add(this.ccsukMainHouseUserControlForPlugin1);
			this.Controls.Add(this.ccsukAirportsAndPartiesControl1);
			this.Controls.Add(this.HouseGroupBox);
			this.Controls.Add(this.ccsukMasterControl1);
			this.Controls.Add(this.GoodsGroupBox);
			this.Name = "CcsukHawbControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 612, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseGroupBox.ResumeLayout(false);
			this.HouseGroupBox.PerformLayout();
			this.GoodsGroupBox.ResumeLayout(false);
			this.GoodsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox GoodsGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit WeightOfGoods;
		private ZArchitecture.ZCalcEdit PackagesReceived;
		private ZArchitecture.ZCalcEdit PackagesExpected;
		private EdifactUNOATextBox DescriptionOfGoods;
		protected ZArchitecture.GUI.ZGroupBox HouseGroupBox;
		private ZArchitecture.ZTextBox HawbTextBox;
		public CcsukMasterControl ccsukMasterControl1;
		private ZArchitecture.GUI.ZCheckBox IsSplitCheckBox;
		ZArchitecture.GUI.ZLinkLabel LinkLabelMawb;
		private CcsukAirportsAndPartiesControl ccsukAirportsAndPartiesControl1;
		private CcsukMainHouseUserControlHelpers ccsukMainHouseUserControlForPlugin1;
		private AwbStatusUserControl awbStatusUserControl1;		
	}
}
