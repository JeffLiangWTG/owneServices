using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializeComponent();
			RemoveUnuseBaseControls();
		}

		void RemoveUnuseBaseControls()
		{
			this.CI_UsageCommentTextBox.Visible = false;
			this.ClassificationDescriptionTextBox.Visible = false;
		}

		protected override void ChangeControlsVisibility()
		{
			if (currentPartPivot is CusClassPartPivot currentPivot)
			{
				if (currentPivot.IsExport)
				{
					SetDynamicExportOtherDetailsPanelLayout();
					this.GovernmentAgencyInfoTabPage.TabVisible = true;
					this.OtherDetailsTabPage.TabVisible = false;
				}
				else if (currentPivot.IsImport)
				{
					this.GovernmentAgencyInfoTabPage.TabVisible = false;
					this.OtherDetailsTabPage.TabVisible = true;
				}
				else
				{
					//TODO this should be changed so that it becomes visible and updated with a right layout
					this.DynamicCustomsDetailsCommonPanel.Visible = false;
					this.CertificateOfOriginGroupBox.Visible = false;
					this.GovernmentAgencyInfoTabPage.TabVisible = false;
					this.OtherDetailsTabPage.TabVisible = false;
				}
			}
		}

		void SetDynamicExportOtherDetailsPanelLayout()
		{
			this.DynamicCustomsDetailsCommonPanel.Visible = true;
			this.CertificateOfOriginGroupBox.Visible = true;
			this.DynamicCustomsDetailsCommonPanel.UpdateLayout(new EXPProductsCustomDetailsCommonLayout());
			this.DynamicCustomsDetailsCertificateOfOriginPanel.UpdateLayout(new EXPProductsCustomDetailsCertificateOfOriginLayout());
		}
	}
}
