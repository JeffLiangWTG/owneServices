namespace Enterprise.Accounting.Registry.GUI
{
	partial class ENettRegisteredBankAccountControl
	{

		#region Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ENettRegisteredBankAccountGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ENettRegisteredBankAccountGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount);
			// 
			// ENettRegisteredBankAccountGrid
			// 
			this.ENettRegisteredBankAccountGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ENettRegisteredBankAccountGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)).BankAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)).BankAccountList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)).BankAccountCurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)).BankAccountDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ENettRegisteredBankAccount)(null)).IsDefault)));
			this.ENettRegisteredBankAccountGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.Caption = "Bank Account Code";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegisteredBankAccountControl|1a6261b3-03ba-4344-bcf9-04cc2a2fc575", "Bank Account Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BankAccountPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ColumnName = "BankAccountCurrencyCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = "Bank Account Description";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegisteredBankAccountControl|bd220f5b-11e0-42ba-9e90-63672bd9caa2", "Bank Account Description");
			zTextBoxColumnStyleInfo2.ColumnName = "BankAccountDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Caption = "Default Receipt Account";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegisteredBankAccountControl|514c0270-e111-47cc-82a8-a6be92c312f2", "Default Receipt Account");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDefault";
			this.ENettRegisteredBankAccountGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ENettRegisteredBankAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ENettRegisteredBankAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ENettRegisteredBankAccountGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ENettRegisteredBankAccountGrid.GridId = "fa799718-cfd3-4e06-a04f-d74b39b25d7a";
			this.ENettRegisteredBankAccountGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ENettRegisteredBankAccountGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ENettRegisteredBankAccountGrid.LayoutKey = "zGrid1";
			this.ENettRegisteredBankAccountGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ENettRegisteredBankAccountGrid.Name = "ENettRegisteredBankAccountGrid";
			this.ENettRegisteredBankAccountGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			this.ENettRegisteredBankAccountGrid.TabIndex = 0;
			// 
			// ENettRegisteredBankAccountControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ENettRegisteredBankAccountGrid);
			this.Name = "ENettRegisteredBankAccountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ENettRegisteredBankAccountGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.ZGrid ENettRegisteredBankAccountGrid;
	}
}
