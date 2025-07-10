using CargoWise.Types;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukAirConsignmentUserControlMawb
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
			this.GoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WeightOfGoods = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackagesReceived = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackagesExpected = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DescriptionOfGoodsTextBox = new Enterprise.Customs.GB.GUI.EdifactUNOATextBox();
			this.ccsukMasterControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMasterControl();
			// The below line must pass TRUE in the ctor of CcsukAirportsAndPartiesControl in order to show the IATA status code box.  The corresponding call for the Hawb control uses the default/empty ctor and so hides the Iata control. 
			this.ccsukAirportsAndPartiesControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirportsAndPartiesControl(true);
			this.ccsukMainMasterUserControlHelpers1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMainMasterUserControlHelpers();
			this.awbStatusUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.AwbStatusUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB);
			// 
			// GoodsGroupBox
			//
			this.GoodsGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("67bbef5f-7704-49ca-a347-dc75c5fe3e07", "Goods");
			this.GoodsGroupBox.Controls.Add(this.WeightOfGoods);
			this.GoodsGroupBox.Controls.Add(this.PackagesReceived);
			this.GoodsGroupBox.Controls.Add(this.PackagesExpected);
			this.GoodsGroupBox.Controls.Add(this.DescriptionOfGoodsTextBox);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).WeightCode)));
			this.WeightOfGoods.BindToAmount = "Weight";
			this.WeightOfGoods.BindToUnit = "WeightCode";
			this.WeightOfGoods.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirConsignmentUserControlMawb|970c374f-23a3-4a2a-a8d9-3ddbd15e0ba9", "Weight");
			this.WeightOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 77, true);
			this.WeightOfGoods.Name = "WeightOfGoods";
			this.WeightOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.WeightOfGoods.TabIndex = 3;
			this.WeightOfGoods.UnitPreBoundMaxLength = 2;
			// 
			// PackagesReceived
			// 
			this.BindingSource.SetBindingMember(this.PackagesReceived, "NumberOfPiecesReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZShort)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).NumberOfPiecesReceived)));
			
			this.PackagesReceived.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirConsignmentUserControlMawb|d6a89a66-5df2-4b39-9e1e-884de468f5c9", "NPR",   "Number of Pieces Received (NPR)", "To set NPR, complete the \'Receipts\' grid. NPR is the number of pieces checked-in so far.");
			this.PackagesReceived.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 55, true);
			this.PackagesReceived.Name = "PackagesReceived";
			this.PackagesReceived.ReadOnly = true;
			this.PackagesReceived.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackagesReceived.TabIndex = 2;
			PackagesReceived.ReadOnly = true; // Do not allow this line to get removed by the designer. It is vital. 
			PackagesReceived.DecimalPlaces = 0;
			// 
			// PackagesExpected
			// 
			this.BindingSource.SetBindingMember(this.PackagesExpected, "NumberOfPiecesExpected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZShort)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).NumberOfPiecesExpected)));
			
			this.PackagesExpected.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirConsignmentUserControlMawb|684974d9-1243-448d-9292-82b7323b78fc", "NPX", "Number of Pieces Expected (NPX)", "Number of pieces expected or manifested on the consignment (NPX)");
			this.PackagesExpected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 32, true);
			this.PackagesExpected.Name = "PackagesExpected";
			this.PackagesExpected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackagesExpected.TabIndex = 1;
			PackagesExpected.DecimalPlaces = 0;
			// 
			// DescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "DescriptionOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).DescriptionOfGoods)));
			this.DescriptionOfGoodsTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirConsignmentUserControlMawb|b0ae375f-41d4-4bd0-b26e-62435d2b1c2a", "Desc.", "Description", "Goods\' Description", "Description of Goods");
			this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 10, true);
			this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
			this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.DescriptionOfGoodsTextBox.TabIndex = 0;
			// 
			// ccsukMasterControl1
			// 
			this.ccsukMasterControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukMasterControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ccsukMasterControl1.CaptionResourceString = null;
			this.ccsukMasterControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukMasterControl1.Name = "ccsukMasterControl1";
			this.ccsukMasterControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 98, true);
			this.ccsukMasterControl1.TabIndex = 0;
			// 
			// ccsukAirportsAndPartiesControl1
			// 
			this.ccsukAirportsAndPartiesControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukAirportsAndPartiesControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)))));
			this.ccsukAirportsAndPartiesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, -1, true);
			this.ccsukAirportsAndPartiesControl1.Name = "ccsukAirportsAndPartiesControl1";
			this.ccsukAirportsAndPartiesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 158, true);
			this.ccsukAirportsAndPartiesControl1.TabIndex = 1;
			// 
			// ccsukMainMasterUserControlHelpers1
			// 
			this.ccsukMainMasterUserControlHelpers1.AllowDrop = true;
			this.ccsukMainMasterUserControlHelpers1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ccsukMainMasterUserControlHelpers1, ".");
			this.ccsukMainMasterUserControlHelpers1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 268, true);
			this.ccsukMainMasterUserControlHelpers1.Name = "ccsukMainMasterUserControlHelpers1";
			this.ccsukMainMasterUserControlHelpers1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 236, true);
			this.ccsukMainMasterUserControlHelpers1.TabIndex = 4;
			// 
			// awbStatusUserControl1
			// 
			this.awbStatusUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.awbStatusUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)))));
			this.awbStatusUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 158, true);
			this.awbStatusUserControl1.Name = "awbStatusUserControl1";
			this.awbStatusUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 105, true);
			this.awbStatusUserControl1.TabIndex = 3;
			// 
			// CcsukAirConsignmentUserControlMawb
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.awbStatusUserControl1);
			this.Controls.Add(this.ccsukMainMasterUserControlHelpers1);
			this.Controls.Add(this.ccsukAirportsAndPartiesControl1);
			this.Controls.Add(this.ccsukMasterControl1);
			this.Controls.Add(this.GoodsGroupBox);
			this.Name = "CcsukAirConsignmentUserControlMawb";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 507, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsGroupBox.ResumeLayout(false);
			this.GoodsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox GoodsGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit WeightOfGoods;
		private ZArchitecture.ZCalcEdit PackagesReceived;
		private ZArchitecture.ZCalcEdit PackagesExpected;
		private EdifactUNOATextBox DescriptionOfGoodsTextBox;
		private CcsukMasterControl ccsukMasterControl1;
		private CcsukAirportsAndPartiesControl ccsukAirportsAndPartiesControl1;
		internal CcsukMainMasterUserControlHelpers ccsukMainMasterUserControlHelpers1;
		private AwbStatusUserControl awbStatusUserControl1;
	}
}
