using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;
using static Enterprise.Integration.Customs;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceCommodityScreeningFeatureHelper
	{
		public static ZBool IsCommodityRiskAssessable(this ComplianceRiskBusinessObject bizO)
		{
			return IsAssessableWithConsolCheck(bizO.HostBusinessEntity, IsCommodityRiskAssessableCore(bizO.HostBusinessEntity, bizO.ComplianceRiskStatus));
		}

		public static ZBool IsCommodityRiskAssessableForServiceTask(this IBusiness hostJob)
		{
			return IsAssessableWithConsolCheck(hostJob, IsCommodityRiskAssessableCore(hostJob, null));
		}

		public static ZBool IsCommodityPanelVisible(this ComplianceRiskBusinessObject bizO)
		{
			return IsCommodityRiskAssessableCore(bizO.HostBusinessEntity, bizO.ComplianceRiskStatus);
		}

		public static bool CommodityScreeningEnabled(this ComplianceRiskBusinessObject bizO)
		{
			return ComplianceRiskAssessmentEnabledBefore(bizO.HostBusinessEntity as IComplianceCommodityRiskStatusProvider, bizO.ComplianceRiskStatus);
		}

		public static bool CommodityScreeningEnabledForLogTab(this IBusiness hostJob)
		{
			return ComplianceRiskAssessmentEnabledBefore(hostJob as IComplianceCommodityRiskStatusProvider, null);
		}

		static ZBool IsCommodityRiskAssessableCore(IBusiness hostJob, ComplianceRiskStatus complianceRiskStatus)
		{
			return ((hostJob as IComplianceJobDirectionProvider)?.IsInternational ?? false)
					&& !IsJobHighVolumeLowValue(hostJob)
					&& ComplianceRiskAssessmentEnabledBefore(hostJob as IComplianceCommodityRiskStatusProvider, complianceRiskStatus);
		}

		public static ZBool IsCommodityRiskAssessableWithoutScreeningEnabledCheck(this IBusiness hostJob)
		{
			var result = ((hostJob as IComplianceJobDirectionProvider)?.IsInternational ?? false)
							&& !IsJobHighVolumeLowValue(hostJob);
			return IsAssessableWithConsolCheck(hostJob, result);
		}

		public static bool ComplianceRiskAssessmentFeatureEnabledForJob(this IBusiness hostJob)
		{
			return ComplianceRiskAssessmentFeatureEnabledForJobCore(hostJob);
		}

		public static bool ComplianceRiskAssessmentFeatureEnabledForJob(this IComplianceCommodityRiskStatusProvider provider)
		{
			return ComplianceRiskAssessmentFeatureEnabledForJobCore(provider);
		}

		static bool ComplianceRiskAssessmentFeatureEnabledForJobCore(object hostJob)
		{
			var isDeclaration = hostJob is IBaseJobDeclaration;
			return !isDeclaration && ComplianceRiskHelper.IsComplianceCommodityScreeningEnable
				|| isDeclaration && ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice;
		}

		static ZBool IsJobHighVolumeLowValue(IBusiness hostJob)
		{
			if (hostJob is IComplianceWiseShipment shipment)
			{
				return shipment.IsHighVolumeLowValue || shipment.IsHighVolumeLowValueMaster;
			}

			return false;
		}

		static ZBool IsAssessableWithConsolCheck(IBusiness hostJob, ZBool result)
		{
			if (result && hostJob is Forwarding.IForwardingConsol consol)
			{
				return consol.Shipments.Any();
			}

			return result;
		}

		static bool ComplianceRiskAssessmentEnabledBefore(IComplianceCommodityRiskStatusProvider provider, ComplianceRiskStatus complianceRiskStatus)
		{
			if (provider == null)
			{
				return false;
			}

			if (provider.ComplianceRiskAssessmentFeatureEnabledForJob())
			{
				return true;
			}

			complianceRiskStatus ??= provider.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, provider.ParentID));

			return complianceRiskStatus != null && (complianceRiskStatus.IsAssessmentInitialized || complianceRiskStatus.IsAssessmentDeclined || IsSubJobAssessmentUsed(provider));
		}

		static bool IsSubJobAssessmentUsed(IComplianceCommodityRiskStatusProvider provider)
		{
			if (provider is Forwarding.IForwardingConsol consol && consol.Shipments.Any())
			{
				var parentIds = consol.Shipments.Select(s => s.PK).Distinct().ToArray();

				var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, parentIds);
				query.AllowTableValuedParameters = true;
				query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.ComplianceRiskInteractionCode);
				query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, new[] { Codes.AssessmentInitialized, Codes.AssessmentDeclined });

				return provider.Factory.Exists(typeof(StmComplianceEvent), query);
			}

			return false;
		}
	}
}
