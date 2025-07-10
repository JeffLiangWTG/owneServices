namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EDIOpportunityValueAnalysisUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TotalForeignValueCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalForeignValueTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLocalValueCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalLocalValueTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity);
			// 
			// TotalForeignValueCurrencyTextBox
			// 
			this.TotalForeignValueCurrencyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalForeignValueCurrencyTextBox, "P8_Calc_TotalForeignValueCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_TotalForeignValueCurrency)));
			this.TotalForeignValueCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 175, true);
			this.TotalForeignValueCurrencyTextBox.Name = "TotalForeignValueCurrencyTextBox";
			this.TotalForeignValueCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 16, true);
			this.TotalForeignValueCurrencyTextBox.TabIndex = 3;
			// 
			// TotalForeignValueTextBox
			// 
			this.TotalForeignValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalForeignValueTextBox, "P8_Calc_TotalForeignValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_TotalForeignValue)));
			this.TotalForeignValueTextBox.CaptionResourceString = ZClientEDI.Res.GetData("97b5bdf6-71d7-48f0-99c9-d7bd7be2951e", "Total Foreign Value");
			this.TotalForeignValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 175, true);
			this.TotalForeignValueTextBox.Name = "TotalForeignValueTextBox";
			this.TotalForeignValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 16, true);
			this.TotalForeignValueTextBox.TabIndex = 2;
			this.TotalForeignValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLocalValueCurrencyTextBox
			// 
			this.TotalLocalValueCurrencyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalLocalValueCurrencyTextBox, "P8_Calc_TotalLocalValueCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_TotalLocalValueCurrency)));
			this.TotalLocalValueCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 175, true);
			this.TotalLocalValueCurrencyTextBox.Name = "TotalLocalValueCurrencyTextBox";
			this.TotalLocalValueCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 16, true);
			this.TotalLocalValueCurrencyTextBox.TabIndex = 5;
			// 
			// TotalLocalValueTextBox
			// 
			this.TotalLocalValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalLocalValueTextBox, "P8_Calc_TotalLocalValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_TotalLocalValue)));
			this.TotalLocalValueTextBox.CaptionResourceString = ZClientEDI.Res.GetData("aa8414e7-3a75-409a-88e8-d6b100006db0", "Total Local Value");
			this.TotalLocalValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 175, true);
			this.TotalLocalValueTextBox.Name = "TotalLocalValueTextBox";
			this.TotalLocalValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 16, true);
			this.TotalLocalValueTextBox.TabIndex = 4;
			this.TotalLocalValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.AllowSorting = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "ValueAnalysisCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_Calc_ModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_UserCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_Calc_ForeignValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_Calc_ForeignValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_Calc_LocalValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgOpportunityValueAnalysis)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).ValueAnalysisCollection)).SyncRoot)).EOV_Calc_LocalValueCurrency)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("b7dbe016-b220-4cb7-bb58-05ca49d614f7", "Module");
			zTextBoxColumnStyleInfo1.ColumnName = "EOV_Calc_ModuleDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("38b38e92-d96e-42fd-9968-040cfa6faa03", "User Count");
			zCalcEditColumnStyleInfo1.ColumnName = "EOV_UserCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("30898743-1197-44b7-8068-9b52be5cb15f", "Foreign Value");
			zCalcEditColumnStyleInfo2.ColumnName = "EOV_Calc_ForeignValue";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("94026137-3e4a-4aa5-b0c1-06db112764fa", "CUR");
			zTextBoxColumnStyleInfo2.ColumnName = "EOV_Calc_ForeignValueCurrency";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("87ff3c48-2a17-42f6-8f8a-1c187c38d120", "Local Value");
			zCalcEditColumnStyleInfo3.ColumnName = "EOV_Calc_LocalValue";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("7984fcff-b60b-4de7-94f5-e048638849ca", "CUR");
			zTextBoxColumnStyleInfo3.ColumnName = "EOV_Calc_LocalValueCurrency";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.GridId = "c1d47836-07ca-44a1-a1de-b346434cd05c";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 164, true);
			this.zGrid1.TabIndex = 1;
			// 
			// EDIOpportunityValueAnalysisUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.TotalLocalValueCurrencyTextBox);
			this.Controls.Add(this.TotalLocalValueTextBox);
			this.Controls.Add(this.TotalForeignValueCurrencyTextBox);
			this.Controls.Add(this.TotalForeignValueTextBox);
			this.Name = "EDIOpportunityValueAnalysisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox TotalForeignValueCurrencyTextBox;
		private ZArchitecture.ZCalcEdit TotalForeignValueTextBox;
		private ZArchitecture.ZTextBox TotalLocalValueCurrencyTextBox;
		private ZArchitecture.ZCalcEdit TotalLocalValueTextBox;
		private ZArchitecture.ZGrid zGrid1;
	}
}
