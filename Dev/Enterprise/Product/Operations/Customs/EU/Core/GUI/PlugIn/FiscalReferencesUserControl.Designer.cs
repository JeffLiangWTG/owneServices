using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class FiscalReferencesUserControl
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
			if (disposing)
			{
				components?.Dispose();
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.FiscalReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FiscalReferencesGrid)).BeginInit();
			this.FiscalReferencesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.ICusFiscalReferenceCollection<CusFiscalReference>);
			// 
			// FiscalReferencesGrid
			// 
			this.FiscalReferencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FiscalReferencesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)).CFR_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)).CFR_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)).OwnerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)).CFR_OA_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusFiscalReference)(null)).CFR_OA_Owner_ZAddress.OrgAddress_List)));
			this.FiscalReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OwnerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("DFFB1DD9-F071-4421-84C6-F50101145C1A", "Owner Address");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zGuidDropEditColumnStyleInfo1.BindToList = "CFR_OA_Owner_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CFR_OA_Owner";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("DFFB1DD9-F071-4421-84C6-F50101145C1A", "Owner Address");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
			this.FiscalReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FiscalReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FiscalReferencesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.FiscalReferencesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.FiscalReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesGrid.GridId = "ba35c235-1324-443a-add8-68972b89acec";
			this.FiscalReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FiscalReferencesGrid.LayoutKey = "FiscalReferencesGrid";
			this.FiscalReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiscalReferencesGrid.Name = "FiscalReferencesGrid";
			this.FiscalReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 105, true);
			this.FiscalReferencesGrid.TabIndex = 0;
			// 
			// FiscalReferencesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FiscalReferencesGrid);
			this.Name = "FiscalReferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FiscalReferencesGrid)).EndInit();
			this.FiscalReferencesGrid.ResumeLayout(false);
			this.FiscalReferencesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid FiscalReferencesGrid;
	}
}
