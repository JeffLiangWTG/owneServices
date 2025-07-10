namespace Enterprise.Registry.GUI
{
	public partial class BankAccountBasedOnCurrencyControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid BankAccountBasedOnCurrencyGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.BankAccountBasedOnCurrencyGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BankAccountBasedOnCurrencyGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BankAccountBasedOnCurrencyCollection);
			// 
			// BankAccountBasedOnCurrencyGrid
			// 
			this.BankAccountBasedOnCurrencyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BankAccountBasedOnCurrencyGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)).CurrencyList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)).CurrencyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)).BankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.BankAccountBasedOnCurrency)(null)).BankAccountList)));
			this.BankAccountBasedOnCurrencyGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "CurrencyList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BankAccountBasedOnCurrencyControl|a3fdee5d-5b80-4971-94d8-8b38bdf27019", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Currency";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BankAccountBasedOnCurrencyControl|67b66bc6-4e71-4e14-a3e3-2c7ff0829051", "Currency Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CurrencyDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.BindToList = "BankAccountList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BankAccountBasedOnCurrencyControl|fa82d02b-c60b-4afa-ad8d-168634d6f06d", "Bank Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BankAccount";
			this.BankAccountBasedOnCurrencyGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.BankAccountBasedOnCurrencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BankAccountBasedOnCurrencyGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BankAccountBasedOnCurrencyGrid.GridId = "7e42c36e-7b50-4bfc-9fc4-f67f0818da6e";
			this.BankAccountBasedOnCurrencyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BankAccountBasedOnCurrencyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BankAccountBasedOnCurrencyGrid.LayoutKey = "BankAccountBasedOnCurrencyGrid";
			this.BankAccountBasedOnCurrencyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BankAccountBasedOnCurrencyGrid.Name = "BankAccountBasedOnCurrencyGrid";
			this.BankAccountBasedOnCurrencyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.BankAccountBasedOnCurrencyGrid.TabIndex = 0;
			// 
			// BankAccountBasedOnCurrencyControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BankAccountBasedOnCurrencyGrid);
			this.Name = "BankAccountBasedOnCurrencyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BankAccountBasedOnCurrencyGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
