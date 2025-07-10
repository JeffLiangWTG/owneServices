using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceRiskStatusWorkflowInitializer
	{
		public static bool InitializeAssessmentWorkflow(this ComplianceRiskStatus complianceRiskStatus)
		{
			return InitializeAssessmentWorkflowCore(complianceRiskStatus, save: true, initializedByUser: true);
		}

		internal static void InitializeAssessmentWorkflowAndApplyComplianceWithoutSave(this ComplianceRiskStatus complianceRiskStatus)
		{
			InitializeAssessmentWorkflowCore(complianceRiskStatus, save: false, initializedByUser: false);
		}

		static bool InitializeAssessmentWorkflowCore(this ComplianceRiskStatus complianceRiskStatus, bool save, bool initializedByUser)
		{
			if (!complianceRiskStatus.IsAssessmentInitialized)
			{
				AddComplianceAssessmentBillingUsageEventLogAndResetCommoditiesRiskStatus(Codes.AssessmentInitialized, complianceRiskStatus, save, initializedByUser);

				if (complianceRiskStatus.Parent is ISupportInteractionWithComplianceWiseCommodities supportInteractionWithCommodities && supportInteractionWithCommodities.Helper != null)
				{
					supportInteractionWithCommodities.Helper.CpwSideCommodities?.AssessmentStatusChanged?.Invoke();
				}

				return true;
			}

			return false;
		}

		public static bool DeclinedAssessmentWorkflow(this ComplianceRiskStatus complianceRiskStatus)
		{
			if (!complianceRiskStatus.IsAssessmentInitialized && !complianceRiskStatus.IsAssessmentDeclined)
			{
				AddComplianceAssessmentBillingUsageEventLogAndResetCommoditiesRiskStatus(Codes.AssessmentDeclined, complianceRiskStatus, save: true, initializedByUser: true);
				return true;
			}

			return false;
		}

		static void AddComplianceAssessmentBillingUsageEventLogAndResetCommoditiesRiskStatus(string assessmentType, ComplianceRiskStatus complianceRiskStatus, bool save, bool initializedByUser)
		{
			AddComplianceAssessmentBillingUsageEventLog(complianceRiskStatus, assessmentType, initializedByUser);

			var allCommodities = complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ToArray();
			var commodities = ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(allCommodities);
			if (commodities.Any())
			{
				commodities.ForEach(c =>
				{
					c.NeedResetStatus = false;
					if (!ComplianceStatusUtils.HasBlockedOrReleased(c.CCD_RiskStatus))
					{
						c.CCD_RiskStatus = assessmentType == Codes.AssessmentInitialized ? ComplianceRiskStatusCodeList.Codes.NotChecked : ComplianceRiskStatusCodeList.Codes.PossibleRisk;
					}
				});
			}

			complianceRiskStatus.SetCommodityRiskStatus(allCommodities);

			complianceRiskStatus.COR_OverallRisk = complianceRiskStatus.GetOverallRiskStatus();

			if (save)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(complianceRiskStatus.Factory.Save, null);
			}
		}

		static void AddComplianceAssessmentBillingUsageEventLog(this ComplianceRiskStatus complianceRiskStatus, string assessmentType, bool initializedByUser)
		{
			var logParameters = GetUpdatedStatusEventParameters(assessmentType, initializedByUser);
			var userCode = ZArchitecture.Environment.User.ServiceUserCode;
			if (complianceRiskStatus.Parent is IStmALogParent logParent)
			{
				var stmALog = logParent.Logs.AddNew(AutoEvents.ComplianceRiskInteraction, logParameters);
				if (!initializedByUser)
				{
					stmALog.SL_GS_NKUser = userCode;
				}
			}

			var eventLog = complianceRiskStatus.AddNewComplianceEventLog(AutoEvents.ComplianceRiskInteractionCode);
			eventLog.SCE_EventSubType = assessmentType;
			eventLog.SCE_EventReference = string.Join(string.Empty, logParameters.Select(u => string.Format(CultureInfo.InvariantCulture, "|{0}={1}", u.Key, u.Value)));

			if (!initializedByUser)
			{
				eventLog.SCE_SystemCreateUser = userCode;
				eventLog.SCE_SystemLastEditUser = userCode;
			}

			if (assessmentType == Codes.AssessmentInitialized)
			{
				complianceRiskStatus.CommodityEventHelper.AddCommodityAdditionLogForBillingWhenAssessmentInitialized(initializedByUser);

				ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>()).ForEach(c =>
				{
					c.CCD_RiskStatusDescriptionInfo.RefreshBinding();
					c.Validation.CheckCommodityStatus();
				});
			}
		}

		static KeyValuePair<string, string>[] GetUpdatedStatusEventParameters(string assessmentType, bool initializedByUser)
		{
			var parameters = new List<KeyValuePair<string, string>>
			{
				new(ParameterCodes.MessageType, (NoResString)"Compliance Assessment"),
				new(ParameterCodes.New, assessmentType),
			};

			if (assessmentType == Codes.AssessmentInitialized)
			{
				parameters.Add(new(ParameterCodes.Type, initializedByUser ? InitializedByType.User : InitializedByType.Rule));
			}

			return parameters.ToArray();
		}

		public static void AssessmentDecisionRequiredWorkflow(this ComplianceRiskStatus complianceRiskStatus)
		{
			AddComplianceAssessmentBillingUsageEventLog(complianceRiskStatus, Codes.AssessmentDecisionRequired, false);
			ZExceptionReporting.ProcessWithSaveExceptionHandling(complianceRiskStatus.Factory.Save, null);
		}
	}
}
