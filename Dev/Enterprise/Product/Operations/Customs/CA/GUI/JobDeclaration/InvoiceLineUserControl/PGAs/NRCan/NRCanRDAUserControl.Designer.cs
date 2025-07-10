namespace Enterprise.Customs.CA.GUI
{
	partial class NRCanRDAUserControl
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
			this.countryOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.caratWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.packQuantityDropEdit3 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.packQuantityDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.packQuantityDropEdit1 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsValueInUsdEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.countryOfOriginFindBox.SuspendLayout();
			this.detailsGroupBox.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.packQuantityDropEdit3.SuspendLayout();
			this.packQuantityDropEdit2.SuspendLayout();
			this.packQuantityDropEdit1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.NRCanPGAHeader);
			// 
			// countryOfOriginFindBox
			// 
			this.BindingSource.SetBindingMember(this.countryOfOriginFindBox, "RN_NKCountryOfOrigin");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).RN_NKCountryOfOrigin)));
			this.countryOfOriginFindBox.AllowDrop = true;
			this.countryOfOriginFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A6E58AE0-C2DF-4A5A-A0FC-C92B5A94C991", "Commodity Ctry/Rgn. of Origin");
			this.countryOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 17, true);
			this.countryOfOriginFindBox.Name = "countryOfOriginFindBox";
			this.countryOfOriginFindBox.PreBoundMaxLength = 3;
			this.countryOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.countryOfOriginFindBox.TabIndex = 0;
			// 
			// caratWeightCalcEdit
			// 
			this.caratWeightCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.caratWeightCalcEdit, "CA_CaratWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_CaratWeight)));
			this.caratWeightCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("42931A5E-F885-4679-9D35-1073F083CD70", "Carat Weight in CTM");
			this.caratWeightCalcEdit.DecimalPlaces = 2;
			this.caratWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 44, true);
			this.caratWeightCalcEdit.Name = "caratWeightCalcEdit";
			this.caratWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.caratWeightCalcEdit.TabIndex = 1;
			this.caratWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.AutoSize = true;
			this.detailsGroupBox.Controls.Add(this.CustomsValueInUsdEdit);
			this.detailsGroupBox.Controls.Add(this.packQuantityDropEdit3);
			this.detailsGroupBox.Controls.Add(this.packQuantityDropEdit2);
			this.detailsGroupBox.Controls.Add(this.packQuantityDropEdit1);
			this.detailsGroupBox.Controls.Add(this.caratWeightCalcEdit);
			this.detailsGroupBox.Controls.Add(this.countryOfOriginFindBox);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 911, true);
			this.detailsGroupBox.TabIndex = 0;
			this.detailsGroupBox.TabStop = false;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("37EA686A-641F-495A-ACBC-48D8D35B8D56", "LPCOs");
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 641, true);
			this.LpcoGroupBox.TabIndex = 1;
			this.LpcoGroupBox.TabStop = false;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 622, true);
			this.LPCOGridUserControl.TabIndex = 11;
			// 
			// packQuantityDropEdit3
			// 
			this.packQuantityDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packQuantityDropEdit3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackQty3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackUQ3)));
			this.packQuantityDropEdit3.BindToAmount = "CA_PackQty3";
			this.packQuantityDropEdit3.BindToUnit = "CA_PackUQ3";
			this.packQuantityDropEdit3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0E206008-5FE2-40F4-8B2A-104E6E16CAC2", "Next Retail Pack Qty");
			this.packQuantityDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 71, true);
			this.packQuantityDropEdit3.Name = "packQuantityDropEdit3";
			this.packQuantityDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.packQuantityDropEdit3.TabIndex = 6;
			// 
			// packQuantityDropEdit2
			// 
			this.packQuantityDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packQuantityDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackUQ2)));
			this.packQuantityDropEdit2.BindToAmount = "CA_PackQty2";
			this.packQuantityDropEdit2.BindToUnit = "CA_PackUQ2";
			this.packQuantityDropEdit2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2849340E-60FA-4D15-BE28-BFF9F9D98406", "Next Retail Pack Qty");
			this.packQuantityDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 44, true);
			this.packQuantityDropEdit2.Name = "packQuantityDropEdit2";
			this.packQuantityDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.packQuantityDropEdit2.TabIndex = 5;
			// 
			// packQuantityDropEdit1
			// 
			this.packQuantityDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packQuantityDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackQty1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_PackUQ1)));
			this.packQuantityDropEdit1.BindToAmount = "CA_PackQty1";
			this.packQuantityDropEdit1.BindToUnit = "CA_PackUQ1";
			this.packQuantityDropEdit1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("FE1AA564-FD1B-4AEC-B173-564DC2D10E1E", "Base Retail Pack Qty");
			this.packQuantityDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 17, true);
			this.packQuantityDropEdit1.Name = "packQuantityDropEdit1";
			this.packQuantityDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.packQuantityDropEdit1.TabIndex = 4;
			// 
			// CustomsValueInUsdEdit
			// 
			this.CustomsValueInUsdEdit.AllowDrop = true;
			this.CustomsValueInUsdEdit.DecimalPlaces = 2;
			this.CustomsValueInUsdEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 71, true);
			this.CustomsValueInUsdEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8d4be754-079a-4ad6-b08e-a65204742dca", "Customs Value In USD");
			this.CustomsValueInUsdEdit.Name = "CustomsValueInUsdEdit";
			this.CustomsValueInUsdEdit.ReadOnly = true;
			this.CustomsValueInUsdEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.CustomsValueInUsdEdit.TabIndex = 3;
			this.CustomsValueInUsdEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BindingSource.SetBindingMember(this.CustomsValueInUsdEdit, "InvoiceLine.JI_CustomsValueInUSD");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_CustomsValueInUSD)));
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.detailsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 796, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LpcoGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(121);
			this.SplitContainer.TabIndex = 8;
			// 
			// NRCanRDAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "NRCanRDAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 911, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.countryOfOriginFindBox.ResumeLayout(true);
			this.countryOfOriginFindBox.PerformLayout();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.packQuantityDropEdit3.ResumeLayout(true);
			this.packQuantityDropEdit3.PerformLayout();
			this.packQuantityDropEdit2.ResumeLayout(true);
			this.packQuantityDropEdit2.PerformLayout();
			this.packQuantityDropEdit1.ResumeLayout(true);
			this.packQuantityDropEdit1.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox countryOfOriginFindBox;
		private ZArchitecture.ZCalcEdit caratWeightCalcEdit;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		protected ZArchitecture.GUI.ZCalcDropEdit packQuantityDropEdit1;
		protected ZArchitecture.GUI.ZCalcDropEdit packQuantityDropEdit3;
		protected ZArchitecture.GUI.ZCalcDropEdit packQuantityDropEdit2;
		private ZArchitecture.ZCalcEdit CustomsValueInUsdEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LpcoGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
