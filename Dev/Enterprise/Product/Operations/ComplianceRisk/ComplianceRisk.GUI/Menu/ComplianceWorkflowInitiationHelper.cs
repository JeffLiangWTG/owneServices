using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.DocumentDeliveryResultForComplianceWorkflow;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceWorkflowInitiationHelper
	{
		public static void InitializeComplianceWorkflowPopupIfNeeded(this ICreditControlledBusinessObject creditControlledBizObject)
		{
			if (creditControlledBizObject is IComplianceCommodityRiskStatusProvider commodityRiskProvider
				&& commodityRiskProvider.IsEnabledComplianceWise
				&& commodityRiskProvider.ComplianceRiskSupport.IsSupportInitialization()
				&& ((commodityRiskProvider as IComplianceJobDirectionProvider)?.IsInternational ?? false)
				&& commodityRiskProvider.ComplianceRiskAssessmentFeatureEnabledForJob())
			{
				var complianceRiskStatus = commodityRiskProvider.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, commodityRiskProvider.ParentID));

				if (complianceRiskStatus != null
					&& complianceRiskStatus.COR_CommodityRisk != ComplianceRiskStatusCodeList.Codes.Clear
					&& complianceRiskStatus.COR_CommodityRisk != ComplianceRiskStatusCodeList.Codes.NotAssessed
					&& !complianceRiskStatus.IsAssessmentDeclined
					&& !complianceRiskStatus.IsAssessmentInitialized)
				{
					commodityRiskProvider.InitializeComplianceWorkflowPopupIfNeeded = () =>
					{
						if (!complianceRiskStatus.IsAssessmentDeclined && !complianceRiskStatus.IsAssessmentInitialized && complianceRiskStatus.ShouldDoAssessmentByBorderWise)
						{
							if (!complianceRiskStatus.Factory.IsInTransaction)
							{
								var jobForm = ZApplication.GetOpenForms().FirstOrDefault(u => u is ZForm form && form.BusinessEntity == creditControlledBizObject);
								var dialogResult = ZFormModaliser.ShowDialogAndDispose(new ComplianceWorkflowInitiationForm(commodityRiskProvider, complianceRiskStatus), jobForm);

								if (dialogResult != DialogResult.No && dialogResult != DialogResult.Cancel
									&& jobForm != null && jobForm is ISupportSwitchTabPage supportSwitchTabPage)
								{
									supportSwitchTabPage.SwitchTabPage(ComplianceWiseConstants.ComplianceRiskTabPageName);
								}

								return dialogResult == DialogResult.No && ComplianceRiskSecurityRights.AllowOrDeclineCommodityRiskAssessmentGranted((IBusiness)commodityRiskProvider)
									? ContinueDocumentDelivery : StopDocumentDelivery;
							}

							return ContinueDocumentDelivery;
						}

						return complianceRiskStatus.IsAssessmentInitialized ? StopDocumentDelivery : ContinueDocumentDelivery;
					};
				}
			}
		}
	}
}
