using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class OpportunityValueEstimationUserControl
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
			this.ContractedValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ContractedValueLocalTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContractedValueLocalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LifetimeValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.LifetimeValueLocalTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LifetimeValueLocalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContractedUserCountTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContractedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocalPotentialTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GlobalPotentialTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocalPotentialLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GlobalPotentialLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContractedValueCalcFindBox.SuspendLayout();
			this.LifetimeValueCalcFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity);
			// 
			// ContractedValueCalcFindBox
			// 
			this.ContractedValueCalcFindBox.AllowDrop = true;
			this.ContractedValueCalcFindBox.BindToAmount = "P8_EstimatedValue";
			this.ContractedValueCalcFindBox.BindToUnit = "P8_RX_NKEstimatedValueCurrency";
			this.ContractedValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ContractedValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 1, true);
			this.ContractedValueCalcFindBox.Name = "ContractedValueCalcFindBox";
			this.ContractedValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ContractedValueCalcFindBox.TabIndex = 1;
			// 
			// ContractedValueLocalTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContractedValueLocalTextBox, "P8_Calc_ContractValueLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_ContractValueLocal)));
			this.ContractedValueLocalTextBox.DecimalPlaces = 2;
			this.ContractedValueLocalTextBox.Enabled = false;
			this.ContractedValueLocalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 1, true);
			this.ContractedValueLocalTextBox.Name = "ContractedValueLocalTextBox";
			this.ContractedValueLocalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.ContractedValueLocalTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContractedValueLocalCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContractedValueLocalCurrencyTextBox, "P8_Calc_ContractValueLocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_Calc_ContractValueLocalCurrency)));
			this.ContractedValueLocalCurrencyTextBox.Enabled = false;
			this.ContractedValueLocalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 1, true);
			this.ContractedValueLocalCurrencyTextBox.Name = "ContractedValueLocalCurrencyTextBox";
			this.ContractedValueLocalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			// 
			// LifetimeValueCalcFindBox
			// 
			this.LifetimeValueCalcFindBox.AllowDrop = true;
			this.LifetimeValueCalcFindBox.BindToAmount = "OrgOpportunityEx.EOM_LifetimeValueOver3Years";
			this.LifetimeValueCalcFindBox.BindToUnit = "OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency";
			this.LifetimeValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LifetimeValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 29, true);
			this.LifetimeValueCalcFindBox.Name = "LifetimeValueCalcFindBox";
			this.LifetimeValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LifetimeValueCalcFindBox.TabIndex = 2;
			// 
			// LifetimeValueLocalTextBox
			// 
			this.BindingSource.SetBindingMember(this.LifetimeValueLocalTextBox, "OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocal)));
			this.LifetimeValueLocalTextBox.DecimalPlaces = 2;
			this.LifetimeValueLocalTextBox.Enabled = false;
			this.LifetimeValueLocalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 29, true);
			this.LifetimeValueLocalTextBox.Name = "LifetimeValueLocalTextBox";
			this.LifetimeValueLocalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.LifetimeValueLocalTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LifetimeValueLocalCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LifetimeValueLocalCurrencyTextBox, "OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocalCurrency)));
			this.LifetimeValueLocalCurrencyTextBox.Enabled = false;
			this.LifetimeValueLocalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 29, true);
			this.LifetimeValueLocalCurrencyTextBox.Name = "LifetimeValueLocalCurrencyTextBox";
			this.LifetimeValueLocalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			// 
			// ContractedUserCountTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContractedUserCountTextBox, "P8_DiscountAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_DiscountAmount)));
			this.ContractedUserCountTextBox.DecimalPlaces = 2;
			this.ContractedUserCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 68, true);
			this.ContractedUserCountTextBox.Name = "ContractedUserCountTextBox";
			this.ContractedUserCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.ContractedUserCountTextBox.TabIndex = 3;
			this.ContractedUserCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContractedLabel
			// 
			this.ContractedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 52, true);
			this.ContractedLabel.Name = "ContractedLabel";
			this.ContractedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 12, true);
			// 
			// LocalPotentialTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalPotentialTextBox, "P8_RentalMultiplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).P8_RentalMultiplier)));
			this.LocalPotentialTextBox.DecimalPlaces = 2;
			this.LocalPotentialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 68, true);
			this.LocalPotentialTextBox.Name = "LocalPotentialTextBox";
			this.LocalPotentialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.LocalPotentialTextBox.TabIndex = 4;
			this.LocalPotentialTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GlobalPotentialTextBox
			// 
			this.BindingSource.SetBindingMember(this.GlobalPotentialTextBox, "OrgOpportunityEx.EOM_GlobalPotential");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).OrgOpportunityEx.EOM_GlobalPotential)));
			this.GlobalPotentialTextBox.DecimalPlaces = 2;
			this.GlobalPotentialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 68, true);
			this.GlobalPotentialTextBox.Name = "GlobalPotentialTextBox";
			this.GlobalPotentialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.GlobalPotentialTextBox.TabIndex = 5;
			this.GlobalPotentialTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalPotentialLabel
			// 
			this.LocalPotentialLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 52, true);
			this.LocalPotentialLabel.Name = "LocalPotentialLabel";
			this.LocalPotentialLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 12, true);
			// 
			// GlobalPotentialLabel
			// 
			this.GlobalPotentialLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 52, true);
			this.GlobalPotentialLabel.Name = "GlobalPotentialLabel";
			this.GlobalPotentialLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 12, true);
			// 
			// OpportunityValueEstimationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GlobalPotentialLabel);
			this.Controls.Add(this.LocalPotentialLabel);
			this.Controls.Add(this.ContractedLabel);
			this.Controls.Add(this.LifetimeValueLocalCurrencyTextBox);
			this.Controls.Add(this.ContractedValueLocalCurrencyTextBox);
			this.Controls.Add(this.GlobalPotentialTextBox);
			this.Controls.Add(this.LocalPotentialTextBox);
			this.Controls.Add(this.ContractedUserCountTextBox);
			this.Controls.Add(this.LifetimeValueLocalTextBox);
			this.Controls.Add(this.ContractedValueLocalTextBox);
			this.Controls.Add(this.LifetimeValueCalcFindBox);
			this.Controls.Add(this.ContractedValueCalcFindBox);
			this.Name = "OpportunityValueEstimationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 90, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContractedValueCalcFindBox.ResumeLayout(true);
			this.ContractedValueCalcFindBox.PerformLayout();
			this.LifetimeValueCalcFindBox.ResumeLayout(true);
			this.LifetimeValueCalcFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCalcFindBox ContractedValueCalcFindBox;
		private ZCalcEdit ContractedValueLocalTextBox;
		private ZTextBox ContractedValueLocalCurrencyTextBox;
		private ZArchitecture.GUI.ZCalcFindBox LifetimeValueCalcFindBox;
		private ZCalcEdit LifetimeValueLocalTextBox;
		private ZTextBox LifetimeValueLocalCurrencyTextBox;
		private ZLabel ContractedLabel;
		private ZCalcEdit LocalPotentialTextBox;
		private ZCalcEdit GlobalPotentialTextBox;
		private ZLabel LocalPotentialLabel;
		private ZLabel GlobalPotentialLabel;
		private ZCalcEdit ContractedUserCountTextBox;
	}
}
