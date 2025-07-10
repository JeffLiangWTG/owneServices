using System.Windows.Forms;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class CommodityAssessmentRiskUserControl : ZUserControl
	{
		public CommodityAssessmentRiskUserControl()
		{
			InitializeComponent();
		}

		async void CommodityBorderWiseLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (BindingSource.Current is ComplianceCommodityDetail commodityDetail)
			{
				if (commodityDetail.ComplianceRiskStatus != null)
				{
					await commodityDetail.ComplianceRiskStatus.ViewBorderWisePortalIfAvailable(commodityDetail);
				}
			}
		}

		public void SetToDisabledWithoutCommodityBorderWiseLinkLabel()
		{
			foreach (Control control in Controls)
			{
				if (control != CommodityBorderWiseLinkLabel)
				{
					control.Enabled = false;
				}
			}
		}
	}
}
