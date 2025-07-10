using System;
using System.Windows.Forms;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class LinkedModuleCommodityUserControl : ZUserControl, ILinkedModuleCommodityUserControl
	{
		public LinkedModuleCommodityUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: "");
			}
			else
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (BindingSource.Current is ILinkedModuleComplianceCommodityDetail commodityDetail)
			{
				commodityDetail.InitializeIfNeeded();

				SetButtonReadOnly(commodityDetail);

				if (commodityDetail.SupportInteractionWithCommodities?.Helper?.CpwSideCommodities != null)
				{
					commodityDetail.SupportInteractionWithCommodities.Helper.CpwSideCommodities.AssessmentStatusChanged -= AssessmentStatusChanged;
					commodityDetail.SupportInteractionWithCommodities.Helper.CpwSideCommodities.AssessmentStatusChanged += AssessmentStatusChanged;
				}
			}
		}

		void AssessmentStatusChanged()
		{
			if (BindingSource.Current is ILinkedModuleComplianceCommodityDetail commodityDetail)
			{
				SetButtonReadOnly(commodityDetail);
			}
		}

		void SetButtonReadOnly(ILinkedModuleComplianceCommodityDetail commodityDetail)
		{
			AssessmentInitializeButton.ReadOnly = !commodityDetail.CommodityExists || (commodityDetail.SupportInteractionWithCommodities?.Helper?.SourceSideCommodities?.AssessmentInitialized?.Invoke() ?? true);
		}

		async void CommodityBorderWiseLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (BindingSource.Current is ILinkedModuleComplianceCommodityDetail commodityDetail)
			{
				await commodityDetail.ViewBorderWisePortalIfAvailable();
			}
		}

		void AssessmentInitializeButton_Click(object sender, EventArgs e)
		{
			if (BindingSource.Current is ILinkedModuleComplianceCommodityDetail commodityDetail)
			{
				commodityDetail.SupportInteractionWithCommodities?.Helper?.SourceSideCommodities?.InitializeAssessment?.Invoke();
			}
		}
	}
}
