using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	 partial class CreditCardFeeControl
	{


		#region Designer generated code

		ZArchitecture.ZGrid CreditCardFeeGrid;

		System.ComponentModel.IContainer components = null;

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CreditCardFeeGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditCardFeeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CreditCardFee);
			// 
			// CreditCardFeeGrid
			// 
			this.CreditCardFeeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CreditCardFeeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CreditCardFee)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CreditCardFee)(null)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CreditCardFee)(null)).ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CreditCardFee)(null)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CreditCardFee)(null)).Percentage)));
			this.CreditCardFeeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardFeeControl|af2f5f2f-bbce-4035-9964-df32b0fc4e40", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardFeeControl|71211f45-2444-45b3-890a-56fd7da09bc4", "Charge Code Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditCardFeeControl|58805429-8b1f-45f9-a22f-b270cd9b59ad", "Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "Percentage";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			this.CreditCardFeeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CreditCardFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CreditCardFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CreditCardFeeGrid.GridId = "dfe99cf0-837f-44bb-9bbe-84e21b7015ea";
			this.CreditCardFeeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditCardFeeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditCardFeeGrid.LayoutKey = "zGrid1";
			this.CreditCardFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CreditCardFeeGrid.Name = "CreditCardFeeGrid";
			this.CreditCardFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			this.CreditCardFeeGrid.TabIndex = 0;
			// 
			// CreditCardFeeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CreditCardFeeGrid);
			this.Name = "CreditCardFeeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditCardFeeGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}