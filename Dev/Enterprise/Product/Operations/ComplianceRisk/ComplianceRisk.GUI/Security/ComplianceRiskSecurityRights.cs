using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceRiskSecurityRights
	{
		public static bool IsAllowedEditHarmonizedCodeWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.EditHarmonizedCodeSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedEditComplianceAssessmentWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.EditComplianceAssessmentSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedAllowComplianceAssessmentWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.AllowComplianceAssessmentSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedDeclineComplianceAssessmentWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.DeclineComplianceAssessmentSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedOverrideOverallRiskStatusWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceItemRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.AllowOverrideOverallRiskStatusSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedResynchronizeComplianceRiskStatusWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceItemRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.AllowResynchronizeRiskStatusSecurity, showErrorWhenNotAllowed);
			}

			return isAllowed;
		}

		public static bool IsAllowedOverrideFreightMovementRestrictionsWithShowError(IBusiness hostBusinessEntity, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostBusinessEntity is IComplianceItemRiskStatusProvider provider)
			{
				isAllowed = IsAllowedSecurityWithShowError(provider.AllowOverrideFreightMovementRestrictionsSecurity, showErrorWhenNotAllowed);
			}

			//This is not added for IAgencyBooking and IBillOfLading because it is not called anywhere else
			return isAllowed;
		}

		public static SecurityCheckpoint GetSecurityCheckPointEditHarmonizedCode(IBusiness hostBusinessEntity)
		{
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				return provider.EditHarmonizedCodeSecurity;
			}

			return null;
		}

		public static SecurityCheckpoint GetSecurityCheckPointEditComplianceAssessment(IBusiness hostBusinessEntity)
		{
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				return provider.EditComplianceAssessmentSecurity;
			}

			return null;
		}

		static SecurityCheckpoint GetSecurityCheckPointAllowComplianceAssessment(IBusiness hostBusinessEntity)
		{
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				return provider.AllowComplianceAssessmentSecurity;
			}

			return null;
		}

		static SecurityCheckpoint GetSecurityCheckPointDeclineComplianceAssessment(IBusiness hostBusinessEntity)
		{
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider provider)
			{
				return provider.DeclineComplianceAssessmentSecurity;
			}

			return null;
		}

		public static bool AllowOrDeclineCommodityRiskAssessmentGranted(IBusiness hostBusinessEntity) => !AllowAndDeclineCommodityRiskAssessmentNotGranted(hostBusinessEntity, out _, out _);

		public static bool AllowAndDeclineCommodityRiskAssessmentNotGranted(IBusiness hostBusinessEntity, out SecurityCheckpoint allowAssessment, out SecurityCheckpoint declineAssessment)
		{
			allowAssessment = GetSecurityCheckPointAllowComplianceAssessment(hostBusinessEntity);
			declineAssessment = GetSecurityCheckPointDeclineComplianceAssessment(hostBusinessEntity);
			return !allowAssessment.IsAllowed && !declineAssessment.IsAllowed;
		}

		static bool IsAllowedSecurityWithShowError(SecurityCheckpoint hostSecurityCheckpoint, bool showErrorWhenNotAllowed)
		{
			var isAllowed = false;
			if (hostSecurityCheckpoint != null)
			{
				isAllowed = hostSecurityCheckpoint.IsAllowed;
				if (showErrorWhenNotAllowed && !isAllowed && hostSecurityCheckpoint is not DeniedSecurityCheckpoint)
				{
					hostSecurityCheckpoint.ShowError();
				}
			}

			return isAllowed;
		}
	}
}
