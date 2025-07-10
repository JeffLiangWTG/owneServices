using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using static System.Environment;

namespace Enterprise.ComplianceRisk.Business
{
	public class CommodityRiskStatusBorderWiseChecker
	{
		public CommodityRiskStatusBorderWiseChecker(ComplianceRiskPlugInBusinessObject pluginBizO, CancellationToken cancellationToken)
		{
			PluginBizO = pluginBizO;
			CancellationToken = cancellationToken;
		}

		public async Task CheckCommoditiesRiskStatus(params ComplianceCommodityDetail[] complianceCommodityDetails)
		{
			await CheckCommodityRiskStatusCore(complianceCommodityDetails, commodityDetailCollection: null, processAllCommodities: false);
		}

		public async Task CheckAllCommoditiesRiskStatus(bool needValidation = true)
		{
			await CheckCommodityRiskStatusCore(complianceCommodityDetails: null, PluginBizO.ComplianceRiskStatus.CommodityDetailCollection, processAllCommodities: true, needValidation);
		}

		public async Task<Guid?> GetRequestId()
		{
			return (await CheckCommodityRiskStatusCore(null, PluginBizO.ComplianceRiskStatus.CommodityDetailCollection, processAllCommodities: true))?.RequestId;
		}

		async Task<ComplianceCheckResponseModel> CheckCommodityRiskStatusCore(ComplianceCommodityDetail[] complianceCommodityDetails, ComplianceCommodityDetailCollection commodityDetailCollection, bool processAllCommodities, bool needValidation = true)
		{
			if (ComplianceCommodityRiskStatusProvider != null
				&& PluginBizO.IsCommodityRiskAssessable())
			{
				var assessmentPointPairInfo = ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo;
				if (assessmentPointPairInfo.SupportAssessmentByBorderWise)
				{
					complianceCommodityDetails = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(commodityDetailCollection?.Cast<ComplianceCommodityDetail>() ?? complianceCommodityDetails).ToArray();

					if (complianceCommodityDetails.Length > 0)
					{
						var requestModel = ComplianceCheckRequestModelBuilder.GetRequestModel(ComplianceItemRiskStatusProvider as BusinessObject, assessmentPointPairInfo.PointPairs, complianceCommodityDetails);

						if (ComplianceCheckRequestModelBuilder.RequestIsValid(requestModel))
						{
							complianceCommodityDetails.ForEach(detail =>
							{
								detail.NeedResetStatus = false;
								detail.BorderWiseCheckInProgress = true;
								if (needValidation)
								{
									detail.Validation.CheckCommodityStatus();
								}
							});

							var postCommoditiesResult = await PostCommoditiesRequest(requestModel, CancellationToken, processAllCommodities);

							if (postCommoditiesResult.Response != null)
							{
								var hostBusinessEntityHasChanges = PluginBizO.HostBusinessEntity.HasChanges;
								var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(commodityDetailCollection?.Cast<ComplianceCommodityDetail>() ?? complianceCommodityDetails)
									.Where(u => !u.IsDeleted && !u.IsDeleting && !u.IsRowDeletedOrNull)
									.ToArray(); // Got the up-to-date data since commodities can be changed during the request.

								if (ResponseIsValidToProcess(postCommoditiesResult.UseCachedResponse, requestModel, commodities))
								{
									var alertHelper = new RefComplianceCommodityAlertHelper(PluginBizO.HostBusinessEntity.Factory, postCommoditiesResult.Response, ComplianceCommodityRiskStatusProvider.RiskCalculateFactor);

									commodities.ForEach(commodityDetail =>
									{
										var responseCommodities = ComplianceCheckResponseModelUpdater.UpdateCommoditiesRiskStatus(commodityDetail, postCommoditiesResult.Response, alertHelper);
										if (responseCommodities.Length > 0)
										{
											ComplianceCheckResponseModelUpdater.UpdateCommodityDetailExtension(commodityDetail);

											if (needValidation)
											{
												commodityDetail.Validation.CheckCommodityStatus();
											}
										}
									});

									ComplianceMaterialChangesDetector.UpdateComplianceMaterialChangeSnapshot(PluginBizO, requestModel, processAllCommodities);

									PluginBizO.ComplianceRiskStatus.CommodityDetailCollection.NotifyChangesFromCpwSide(commodities);
								}

								var commoditiesHasChanges = commodities.Any(commodityDetail => commodityDetail.CCD_RiskStatusInfo.HasChanges
									|| commodityDetail.CCD_NomenclatureConditionInfo.HasChanges
									|| commodityDetail.CCD_SpecificConditionInfo.HasChanges);

								// Save the risk status to the database when only commodities risk status changes and form not been closed (CancellationToken not cancelled).
								if (commoditiesHasChanges && !hostBusinessEntityHasChanges && !CancellationToken.IsCancellationRequested)
								{
									ZExceptionReporting.ProcessWithSaveExceptionHandling(PluginBizO.ComplianceRiskStatus.Factory.Save, null);
								}
							}

							return postCommoditiesResult.Response;
						}
					}
				}
			}

			return null;
		}

		Task<(ComplianceCheckResponseModel Response, bool UseCachedResponse)> checkCommoditiesRequestTask;
		ComplianceCheckRequestModel lastRequestModel;
		ComplianceCheckResponseModel lastResponseModel;

		/// <summary>
		/// Prevent multiple BW API requests when the last request is identical to the current request while navigating between the form load and compliance-wise tab.
		/// </summary>
		bool RequestAreEqual(ComplianceCheckRequestModel request)
		{
			return request != null && lastRequestModel != null
				&& JsonConvert.SerializeObject(request) == JsonConvert.SerializeObject(lastRequestModel);
		}

		/// <summary>
		/// If the request was not changed during the web request, the response is valid to process
		/// </summary>
		internal protected virtual bool ResponseIsValidToProcess(bool useCachedResponse, ComplianceCheckRequestModel request, ComplianceCommodityDetail[] commodities)
		{
			return useCachedResponse
				|| JsonConvert.SerializeObject(request) == JsonConvert.SerializeObject(
					ComplianceCheckRequestModelBuilder.GetRequestModel(
						ComplianceItemRiskStatusProvider as BusinessObject, ComplianceCommodityRiskStatusProvider?.AssessmentPointPairInfo?.PointPairs, commodities));
		}

		IComplianceItemRiskStatusProvider ComplianceItemRiskStatusProvider => PluginBizO.ComplianceItemRiskStatusProvider;
		IComplianceCommodityRiskStatusProvider ComplianceCommodityRiskStatusProvider => PluginBizO.ComplianceCommodityRiskStatusProvider;
		CancellationToken CancellationToken { get; }
		ComplianceRiskPlugInBusinessObject PluginBizO { get; }

		async Task<(ComplianceCheckResponseModel Response, bool UseCachedResponse)> PostCommoditiesRequest(ComplianceCheckRequestModel request, CancellationToken cancellationToken, bool processAllCommodities)
		{
			if (processAllCommodities)
			{
				try
				{
					(ComplianceCheckResponseModel Response, bool UseCachedResponse) result;

					if (RequestAreEqual(request)) // When request is the same
					{
						if (lastResponseModel != null) // Already got the response, skip call API 1
						{
							result = (lastResponseModel, true);
						}
						else if (checkCommoditiesRequestTask != null) // Request not finished yet, wait the task finished
						{
							result = await checkCommoditiesRequestTask;
						}
						else // Task is null, call API 1
						{
							checkCommoditiesRequestTask = PostCommoditiesRequest(request, cancellationToken);
							result = await checkCommoditiesRequestTask;
						}
					}
					else // Request not the same, call API 1
					{
						checkCommoditiesRequestTask = PostCommoditiesRequest(request, cancellationToken);
						lastRequestModel = request; // assign lastRequestModel after checkCommoditiesRequestTask is initialized
						result = await checkCommoditiesRequestTask;
					}

					lastResponseModel = result.Response;

					return result;
				}
				finally
				{
					checkCommoditiesRequestTask = null;
				}
			}

			return await PostCommoditiesRequest(request, CancellationToken);
		}

		async Task<(ComplianceCheckResponseModel Response, bool UseCachedResponse)> PostCommoditiesRequest(ComplianceCheckRequestModel singleJobRequest, CancellationToken cancellationToken)
		{
			var response = await PostCommoditiesRequest(new[] { singleJobRequest }, cancellationToken);

			return (response?.Single(), false);
		}

		async Task<ICollection<ComplianceCheckResponseModel>> PostCommoditiesRequest(IEnumerable<ComplianceCheckRequestModel> request, CancellationToken cancellationToken)
		{
			DeniedPartyScreenerAsync.WriteMessage((NoResString)$"{NewLine}CheckCommodityRiskStatus Started");

			DeniedPartyScreenerAsync.WriteMessage((NoResString)$"{NewLine}CheckCommodityRiskStatus Request:{NewLine}" + JsonConvert.SerializeObject(request, Formatting.Indented));

			var response = await borderWiseApiHelper.ComplianceCheckAsync(request, cancellationToken);

			DeniedPartyScreenerAsync.WriteMessage((NoResString)$"{NewLine}CheckCommodityRiskStatus Response:{NewLine}" + (response != null ? JsonConvert.SerializeObject(response, Formatting.Indented) : (NoResString)"NULL"));

			DeniedPartyScreenerAsync.WriteMessage((NoResString)$"{NewLine}CheckCommodityRiskStatus Ended");

			return response;
		}

		public async Task GetSupportedCountriesAndAssignStatusIfNeeded()
		{
			if (ComplianceCommodityRiskStatusProvider != null
				&& PluginBizO.IsCommodityRiskAssessable())
			{
				var assessmentPointPairInfo = ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo;

				var complianceCommodityDetails = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(PluginBizO.ComplianceRiskStatus.CommodityDetailCollection?.Cast<ComplianceCommodityDetail>()).ToArray();

				if (assessmentPointPairInfo.SupportAssessmentByBorderWise)
				{
					if (!PluginBizO.ComplianceRiskStatus.SupportedCountriesCheckResponseModelExists)
					{
						try
						{
							PluginBizO.ComplianceRiskStatus.SupportedCountriesCheckResponseModel = await SupportedCountriesHelper.GetSupportedCountriesCheckResponseModelAsync(CancellationToken, PluginBizO.ComplianceRiskStatus.Factory);
						}
						catch (ComplianceCheckException)
						{
							PluginBizO.ComplianceRiskStatus.SupportedCountriesCheckResponseModel = null;
							ProcessingCommodities(exceptionOccur: true);
							throw;
						}

						ProcessingCommodities(exceptionOccur: false);
					}
				}
			}
		}

		void ProcessingCommodities(bool exceptionOccur)
		{
			var hostBusinessEntityHasChanges = PluginBizO.HostBusinessEntity.HasChanges;
			var hasChanges = false;
			var allCommodities = PluginBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ToArray();

			if (exceptionOccur)
			{
				// When exception occur, set all commodities risk status to Not Checked
				if (!PluginBizO.ComplianceRiskStatus.IsAssessmentInitialized && !PluginBizO.ComplianceRiskStatus.IsAssessmentDeclined)
				{
					var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(allCommodities).ToArray();

					commodities.ForEach(u =>
					{
						if (u.CCD_RiskStatus != ComplianceRiskStatusCodeList.Codes.NotChecked)
						{
							hasChanges = true;
							u.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;
						}
					});
				}
			}
			else
			{
				hasChanges = PluginBizO.ComplianceRiskStatus.UpdateCommodityRiskStatusBySupportedCountriesModelOrDeclined();
			}

			PluginBizO.ComplianceRiskStatus.SetCommodityRiskStatus(allCommodities);
			hasChanges = hasChanges || PluginBizO.ComplianceRiskStatus.HasChanges;

			var ignoreOverrideClear = hasChanges && !PluginBizO.ComplianceRiskStatus.CopyFromBooking;
			PluginBizO.ComplianceRiskStatus.SetOverallRiskStatus(ignoreOverrideClear);

			// Save the risk status to the database when only commodities risk status changes and form not been closed (CancellationToken not cancelled).
			if (hasChanges && !hostBusinessEntityHasChanges && !CancellationToken.IsCancellationRequested)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
				{
					PluginBizO.ComplianceRiskStatus.Factory.Save();
				}, null);
			}
		}

		readonly BorderWiseApiHelper borderWiseApiHelper = new();
		SupportedCountriesHelper supportedCountriesHelper;
		SupportedCountriesHelper SupportedCountriesHelper => supportedCountriesHelper ??= new(borderWiseApiHelper);
	}
}
