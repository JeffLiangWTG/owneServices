
namespace Enterprise.Customs.KR.GUI
{
	partial class MiscOptionsUserControl
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
      this.ReturnReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.ReturnTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.SouthNorthTradeAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.SouthNorthTradeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.BondedTransportationPeriodUserControl = new Enterprise.Customs.KR.GUI.CalenderUserControl();
      this.LateDecPenaltyDateCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.MissedDecPenaltyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
      this.PercentageLabel = new Enterprise.ZArchitecture.ZLabel();
      this.TaxOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.ReturnReasonDropEdit.SuspendLayout();
      this.ReturnTypeDropEdit.SuspendLayout();
      this.SouthNorthTradeAreaDropEdit.SuspendLayout();
      this.SouthNorthTradeDropEdit.SuspendLayout();
      this.BondedTransportationPeriodUserControl.SuspendLayout();
      this.LateDecPenaltyDateCodeDropEdit.SuspendLayout();
      this.TaxOfficeCodeFindBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
      // 
      // ReturnReasonDropEdit
      // 
      this.ReturnReasonDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.ReturnReasonDropEdit, "JE_ReturnReason");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ReturnReason)));
      this.ReturnReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 2, true);
      this.ReturnReasonDropEdit.Name = "ReturnReasonDropEdit";
      this.ReturnReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.ReturnReasonDropEdit.TabIndex = 3;
      // 
      // ReturnTypeDropEdit
      // 
      this.ReturnTypeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.ReturnTypeDropEdit, "JE_ReturnType");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ReturnType)));
      this.ReturnTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 28, true);
      this.ReturnTypeDropEdit.Name = "ReturnTypeDropEdit";
      this.ReturnTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.ReturnTypeDropEdit.TabIndex = 4;
      // 
      // SouthNorthTradeAreaDropEdit
      // 
      this.SouthNorthTradeAreaDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.SouthNorthTradeAreaDropEdit, "JE_TradeIDWithKP");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TradeIDWithKP)));
      this.SouthNorthTradeAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 78, true);
      this.SouthNorthTradeAreaDropEdit.Name = "SouthNorthTradeAreaDropEdit";
      this.SouthNorthTradeAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.SouthNorthTradeAreaDropEdit.TabIndex = 6;
      // 
      // SouthNorthTradeDropEdit
      // 
      this.SouthNorthTradeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.SouthNorthTradeDropEdit, "JE_TradeIndicatorWithKP");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TradeIndicatorWithKP)));
      this.SouthNorthTradeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 53, true);
      this.SouthNorthTradeDropEdit.Name = "SouthNorthTradeDropEdit";
      this.SouthNorthTradeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.SouthNorthTradeDropEdit.TabIndex = 5;
      // 
      // UCRTextBox
      // 
      this.BindingSource.SetBindingMember(this.UCRTextBox, "JE_UCR");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_UCR)));
      this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 154, true);
      this.UCRTextBox.Name = "UCRTextBox";
      this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.UCRTextBox.TabIndex = 9;
      // 
      // BondedTransportationPeriodUserControl
      // 
      this.BondedTransportationPeriodUserControl.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.BondedTransportationPeriodUserControl, ".");
      this.BondedTransportationPeriodUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 126, true);
      this.BondedTransportationPeriodUserControl.Name = "BondedTransportationPeriodUserControl";
      this.BondedTransportationPeriodUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
      this.BondedTransportationPeriodUserControl.TabIndex = 10;
      // 
      // LateDecPenaltyDateCodeDropEdit
      // 
      this.LateDecPenaltyDateCodeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.LateDecPenaltyDateCodeDropEdit, "JE_LateDecPenaltyDateCode");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LateDecPenaltyDateCode)));
      this.LateDecPenaltyDateCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 190, true);
      this.LateDecPenaltyDateCodeDropEdit.Name = "LateDecPenaltyDateCodeDropEdit";
      this.LateDecPenaltyDateCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.LateDecPenaltyDateCodeDropEdit.TabIndex = 11;
      // 
      // MissedDecPenaltyRateCalcEdit
      // 
      this.BindingSource.SetBindingMember(this.MissedDecPenaltyRateCalcEdit, "JE_MissedDecPenaltyRate");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_MissedDecPenaltyRate)));
      this.MissedDecPenaltyRateCalcEdit.DecimalPlaces = 0;
      this.MissedDecPenaltyRateCalcEdit.Decimals = 0;
      this.MissedDecPenaltyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 223, true);
      this.MissedDecPenaltyRateCalcEdit.Name = "MissedDecPenaltyRateCalcEdit";
      this.MissedDecPenaltyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 15, true);
      this.MissedDecPenaltyRateCalcEdit.TabIndex = 12;
      this.MissedDecPenaltyRateCalcEdit.Text = "0";
      this.MissedDecPenaltyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.MissedDecPenaltyRateCalcEdit.TrackDisposedAccess = true;
      // 
      // PercentageLabel
      // 
      this.PercentageLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
      this.PercentageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 218, true);
      this.PercentageLabel.Name = "PercentageLabel";
      this.PercentageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 20, true);
      this.PercentageLabel.TabIndex = 13;
      this.PercentageLabel.Text = "%";
      this.PercentageLabel.UseMnemonic = false;
      // 
      // TaxOfficeCodeFindBox
      // 
      this.TaxOfficeCodeFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.TaxOfficeCodeFindBox, "JE_TaxOffice");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TaxOffice)));
      this.TaxOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 248, true);
      this.TaxOfficeCodeFindBox.Name = "TaxOfficeCodeFindBox";
      this.TaxOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.TaxOfficeCodeFindBox.ParentType = null;
      this.TaxOfficeCodeFindBox.PreBoundMaxLength = 3;
      this.TaxOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
      this.TaxOfficeCodeFindBox.TabIndex = 14;
      // 
      // MiscOptionsUserControl
      // 
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.TaxOfficeCodeFindBox);
      this.Controls.Add(this.PercentageLabel);
      this.Controls.Add(this.MissedDecPenaltyRateCalcEdit);
      this.Controls.Add(this.LateDecPenaltyDateCodeDropEdit);
      this.Controls.Add(this.BondedTransportationPeriodUserControl);
      this.Controls.Add(this.UCRTextBox);
      this.Controls.Add(this.SouthNorthTradeAreaDropEdit);
      this.Controls.Add(this.SouthNorthTradeDropEdit);
      this.Controls.Add(this.ReturnTypeDropEdit);
      this.Controls.Add(this.ReturnReasonDropEdit);
      this.Name = "MiscOptionsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 273, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.ReturnReasonDropEdit.ResumeLayout(true);
      this.ReturnReasonDropEdit.PerformLayout();
      this.ReturnTypeDropEdit.ResumeLayout(true);
      this.ReturnTypeDropEdit.PerformLayout();
      this.SouthNorthTradeAreaDropEdit.ResumeLayout(true);
      this.SouthNorthTradeAreaDropEdit.PerformLayout();
      this.SouthNorthTradeDropEdit.ResumeLayout(true);
      this.SouthNorthTradeDropEdit.PerformLayout();
      this.BondedTransportationPeriodUserControl.ResumeLayout(true);
      this.BondedTransportationPeriodUserControl.PerformLayout();
      this.LateDecPenaltyDateCodeDropEdit.ResumeLayout(true);
      this.LateDecPenaltyDateCodeDropEdit.PerformLayout();
      this.TaxOfficeCodeFindBox.ResumeLayout(true);
      this.TaxOfficeCodeFindBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit ReturnReasonDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ReturnTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SouthNorthTradeAreaDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SouthNorthTradeDropEdit;
		internal CalenderUserControl BondedTransportationPeriodUserControl;
		internal ZArchitecture.ZTextBox UCRTextBox;
		internal ZArchitecture.GUI.ZDropEdit LateDecPenaltyDateCodeDropEdit;
		internal ZArchitecture.ZCalcEdit MissedDecPenaltyRateCalcEdit;
		public ZArchitecture.ZLabel PercentageLabel;
		internal ZArchitecture.GUI.ZCodeFindBox TaxOfficeCodeFindBox;
	}
}
