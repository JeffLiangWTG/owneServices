using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceRiskStatusFormSynchronizer
	{
		public static void AddModuleActionsMenuItem(IDeniedPartyScreeningActionsProvider provider)
		{
			provider.AddParentModuleActionsMenuItem(new ZMenuItem(ResString.GetMultilingualString("a3953f9b-b483-48d0-bfee-7a90832167d7", "Resynchronize Compliance Risk Status"), delegate
			{
				if (provider.ModuleHasSelectedBusinessObjectsWithShowMessage())
				{
					var providers = provider.ParentModuleFilterGrid.GetSelectedBusinessObjects().Select(u => TryGetValidComplianceRiskProviderBizO(u)).Where(x => x is not null).ToArray();

					if (providers.Length > 0 && ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)providers[0], showErrorWhenNotAllowed: true))
					{
						var resultMessage = new List<string>();
						var complianceRiskPlugInBizOs = ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(providers)
							.Where(x => x is not null)
							.ToArray();

						foreach (var pluginBizO in complianceRiskPlugInBizOs)
						{
							if (ComplianceRiskStatusSynchronizer.Synchronize(pluginBizO))
							{
								resultMessage.Add(Res.GetString("5c40cf43-8e03-416a-a834-f5d5c0a79d37", "{0} resynchronizing completed.", CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)pluginBizO.HostBusinessEntity)));
							}
							else
							{
								resultMessage.Add(Res.GetString("79d96456-0340-410f-8bb5-385241df9e42", "{0} is synchronized, no need for resynchronization.", CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)pluginBizO.HostBusinessEntity)));
							}
						}

						ZExceptionReporting.ProcessWithSaveExceptionHandling(() => complianceRiskPlugInBizOs[0].ComplianceRiskStatus.Factory.Save(), null);

						Globals.Message.ShowInformation(string.Join(System.Environment.NewLine, resultMessage));
					}
				}
			}));
		}

		public static async Task InitializeComplianceAssessmentWhenMenuClick(ComplianceRiskPlugInBusinessObject complianceRiskBizObject, ISupportSwitchTabPage jobForm)
		{
			if (complianceRiskBizObject.ComplianceCommodityRiskStatusProvider != null)
			{
				if (ComplianceRiskSecurityRights.IsAllowedAllowComplianceAssessmentWithShowError(complianceRiskBizObject.HostBusinessEntity, showErrorWhenNotAllowed: true))
				{
					if (complianceRiskBizObject.HostBusinessEntity.HasChanges)
					{
						Globals.Message.ShowInformation(Res.GetString("e78f9ca8-f3bc-4ee6-a0e0-bae070d8c9fd", "Please save form before Initializing Compliance Assessment"));
					}
					else if (!(complianceRiskBizObject.HostBusinessEntity as IComplianceJobDirectionProvider).IsInternational)
					{
						ShowComplianceAssessmentNotAvailableMessage();
					}
					else if (complianceRiskBizObject.ComplianceRiskStatus.InitializeComplianceAssessmentWithErrorHandling())
					{
						jobForm?.SwitchTabPage(ComplianceWiseConstants.ComplianceRiskTabPageName);

						if (complianceRiskBizObject.ComplianceRiskStatus.IsAssessmentInitialized && complianceRiskBizObject.CommodityRiskStatusChecker != null)
						{
							await complianceRiskBizObject.CommodityRiskStatusChecker.CheckAllCommoditiesRiskStatus(true);
						}
					}
				}
			}
		}

		public static bool InitializeComplianceAssessmentWithErrorHandling(this ComplianceRiskStatus complianceRiskStatus)
		{
			var result = false;
			try
			{
				result = complianceRiskStatus.InitializeAssessmentWorkflow();
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowInformation(InitializeComplianceAssessmentConcurrencyErrorMessage);
			}

			return result;
		}

		static string InitializeComplianceAssessmentConcurrencyErrorMessage => Res.GetString("011BC81C-922D-459F-BE75-A04BBE86FC87", "Failed to initialize Compliance Assessment due to concurrency error. Please reload the form and try again.");

		public static void DeclineComplianceAssessmentWhenMenuClick(ComplianceRiskPlugInBusinessObject complianceRiskBizObject)
		{
			if (complianceRiskBizObject.ComplianceCommodityRiskStatusProvider != null)
			{
				if (ComplianceRiskSecurityRights.AllowAndDeclineCommodityRiskAssessmentNotGranted(complianceRiskBizObject.HostBusinessEntity, out var allowAssessment, out var declineAssessment))
				{
					declineAssessment.ShowError();
				}
				else
				{
					if (complianceRiskBizObject.HostBusinessEntity.HasChanges)
					{
						Globals.Message.ShowInformation(Res.GetString("308800a2-5106-4fbd-81d6-db2d8571a7b9", "Please save form before Decline Compliance Assessment"));
					}
					else if (!(complianceRiskBizObject.HostBusinessEntity as IComplianceJobDirectionProvider).IsInternational)
					{
						ShowComplianceAssessmentNotAvailableMessage();
					}
					else
					{
						complianceRiskBizObject.ComplianceRiskStatus.DeclineComplianceAssessmentWithErrorHandling();
					}
				}
			}
		}

		public static bool DeclineComplianceAssessmentWithErrorHandling(this ComplianceRiskStatus complianceRiskStatus)
		{
			var result = false;
			try
			{
				result = complianceRiskStatus.DeclinedAssessmentWorkflow();
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowInformation(DeclineComplianceAssessmentConcurrencyErrorMessage);
			}

			return result;
		}

		static string DeclineComplianceAssessmentConcurrencyErrorMessage => Res.GetString("CA6129F4-96A3-4908-BC8E-37D318322E2B", "Failed to decline Compliance Assessment due to concurrency error. Please reload the form and try again.");

		static void ShowComplianceAssessmentNotAvailableMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("82cd0a73-b9d5-48d0-abbc-c28e7da3c060", "Compliance Assessment is not available on this Job."));
		}

		internal static IComplianceItemRiskStatusProvider TryGetValidComplianceRiskProviderBizO(IBusiness bizO)
		{
			return bizO is IViewComplianceRiskStatusProvider viewComplianceRiskStatusProvider
				? viewComplianceRiskStatusProvider.GetProviderBusinessObject()
				: bizO as IComplianceItemRiskStatusProvider;
		}
	}
}
