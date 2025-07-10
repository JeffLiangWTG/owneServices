using Enterprise.Registry.GUI;

namespace Enterprise.Client.ELG
{
	public partial class SageAccountCodeMappingRegistryItemControl : RegistryZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid zGrid1;

		protected virtual void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject);
			//
			// zGrid1
			//
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).OrgHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).Organisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).RefCurrencyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).LedgerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).LedgerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.ELG.SageAccountCodeMappingRegistryBusinessObject)(null)).SageAccountCode)));
			this.zGrid1.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Organisations";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Organisation Code";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrgHeaderPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Currencies";
			zGuidFindBoxColumnStyleInfo1.Caption = "Currency";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RefCurrencyPK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zDropEditColumnStyleInfo1.BindToList = "LedgerTypes";
			zDropEditColumnStyleInfo1.Caption = "Ledger Type";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "LedgerType";
			zDropEditColumnStyleInfo1.MaxDropDownItems = 2;
			zTextBoxColumnStyleInfo1.Caption = "Sage Account Code";
			zTextBoxColumnStyleInfo1.ColumnName = "SageAccountCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.GridId = "9579da23-bfa8-4fec-b1b0-32dab0e0d7fb";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			//
			// SageAccountCodeMappingRegistryItemControl
			//
			this.Controls.Add(this.zGrid1);
			this.Name = "SageAccountCodeMappingRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
