using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ComplianceRisk.GUI
{
	public class CommodityRiskStatusBorderWiseCheckerForGui : ISupportCheckCommodityRiskStatus
	{
		public CommodityRiskStatusBorderWiseCheckerForGui(ComplianceRiskPlugInBusinessObject pluginBizO, CancellationToken cancellationToken)
		{
			this.pluginBizO = pluginBizO;
			CancellationToken = cancellationToken;
			ComplianceCheckHelper = new CommodityRiskStatusBorderWiseChecker(pluginBizO, cancellationToken);
		}

		public async Task CheckCommoditiesRiskStatus(params ComplianceCommodityDetail[] complianceCommodityDetails)
		{
			await CheckCommoditiesRiskStatusCore(complianceCommodityDetails);
		}

		public async Task CheckAllCommoditiesRiskStatus(bool needValidation)
		{
			await CheckCommoditiesRiskStatusCore(null, needValidation);
		}

		async Task CheckCommoditiesRiskStatusCore(ComplianceCommodityDetail[] complianceCommodityDetails, bool needValidation = true)
		{
			if (!pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized)
			{
				return;
			}

			try
			{
				borderWiseCallCount++;
				pluginBizO.ComplianceRiskSpinnerIndicatorVisibility?.Invoke(borderWiseCallCount > 0);

				if (complianceCommodityDetails == null)
				{
					await ComplianceCheckHelper.CheckAllCommoditiesRiskStatus(needValidation);
				}
				else
				{
					await ComplianceCheckHelper.CheckCommoditiesRiskStatus(complianceCommodityDetails);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!CancellationToken.IsCancellationRequested)
				{
					var newLine = System.Environment.NewLine;
					DeniedPartyScreenerAsync.WriteMessage($"{newLine}Failed to call the BorderWise API and get the commodity risks, exception: {ex}{newLine}");
					ShowWarningMessageForBorderWise();
				}
			}
			finally
			{
				borderWiseCallCount--;
				pluginBizO.ComplianceRiskSpinnerIndicatorVisibility?.Invoke(borderWiseCallCount > 0);
			}
		}

		public async Task ViewBorderWisePortal(ComplianceCommodityDetail commodityDetail)
		{
			if (pluginBizO.HostBusinessEntity.HasChanges || !pluginBizO.HostBusinessEntity.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("3e7250bd-fb96-4cc9-801d-fec5cbf4646b", "Please save the form before opening Compliance Alerts link."));
				return;
			}

			if (commodityDetail.AssessmentInitialized)
			{
				try
				{
					var requestId = await ComplianceCheckHelper.GetRequestId();
					if (requestId.HasValue)
					{
						BorderWise.LaunchBorderWiseWebsite(requestId.Value, commodityDetail.CCD_HarmonizedCode);
						new ComplianceRiskStatusSupporter().AddBorderWiseIntegrationLegalBooksViewedEventLog(commodityDetail);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!CancellationToken.IsCancellationRequested)
					{
						var newLine = System.Environment.NewLine;
						DeniedPartyScreenerAsync.WriteMessage($"{newLine}Failed to call the BorderWise API and navigate to BorderWise Portal, exception: {ex}{newLine}");
						ShowWarningMessageForBorderWise();
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("23A0F9E7-9BF2-485C-B5AD-F9CC77BD53FF", "Please initiate the assessment first."));
			}
		}

		void ShowWarningMessageForBorderWise() => Globals.Message.ShowWarning(Res.GetString("2855f6c0-635b-4806-8eda-08bcc4638265", "The Risk Assessment service is unavailable, please try again later. If the issue persists, please raise a Customer Service Incident."));

		public async Task GetSupportedCountriesAndAssignStatusIfNeeded()
		{
			try
			{
				await ComplianceCheckHelper.GetSupportedCountriesAndAssignStatusIfNeeded();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!CancellationToken.IsCancellationRequested)
				{
					DeniedPartyScreenerAsync.WriteMessage($"\r\nFailed to call the BorderWise API and get supported countries, exception: {ex}\r\n");
					ShowWarningMessageForBorderWise();
				}
			}
		}

		CommodityRiskStatusBorderWiseChecker ComplianceCheckHelper { get; }

		CancellationToken CancellationToken { get; }

		IComplianceBorderWiseProvider BorderWise => borderWise ??= ObjectFactory.Get<IComplianceBorderWiseProvider>();
		IComplianceBorderWiseProvider borderWise;

		readonly ComplianceRiskPlugInBusinessObject pluginBizO;
		int borderWiseCallCount;
	}
}
