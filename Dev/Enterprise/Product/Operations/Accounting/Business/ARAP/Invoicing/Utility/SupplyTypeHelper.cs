using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class SupplyTypeHelper
	{
		public static ZString GetSupplyType(this Job job, AccChargeCode chargeCode, ZString chargeType, ZGuid department)
		{
			if (chargeCode != null && AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				if (chargeType == Core.Constants.ChargeType.Disbursement && AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.Value)
				{
					return AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;
				}

				if (job != null)
				{
					job.InitializeParentFromGenericJobWithSettingDefaults();
					var pluginData = job.PlugInData;
					ISupplyTypeSelector supplyTypeConfiguration = null;

					var consumerTypeCode = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
					var directionCode = Constants.FreightShipmentDirection.Code.All;
					var transportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
					var incoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;

					if (pluginData != null && (pluginData is BusinessObject bizO) && !bizO.IsDeleted)
					{
						if (pluginData.InvoicingSupporter.ConsumerType != null)
						{
							consumerTypeCode = pluginData.InvoicingSupporter.ConsumerType.Code;
						}

						if (!job.Direction.IsEmpty)
						{
							directionCode = job.Direction;
						}

						if (!pluginData.InvoicingSupporter.TransportMode.IsEmpty)
						{
							transportMode = pluginData.InvoicingSupporter.TransportMode;
						}

						if (!pluginData.InvoicingSupporter.PaymentTerm.IsEmpty())
						{
							incoTerm = pluginData.GetINCOTermCode();
						}
					}

					supplyTypeConfiguration = GetSupplyTypeByChargeCode(chargeCode, consumerTypeCode, directionCode, transportMode, incoTerm, department);

					if (supplyTypeConfiguration != null)
					{
						return supplyTypeConfiguration.SupplyType;
					}
				}
			}
			return ZString.Empty;
		}

		public static ZString GetSupplyType(this IJobCostingPlugIn consol, AccChargeCode chargeCode)
		{
			ISupplyTypeSelector result = null;
			var directionCode = Constants.FreightShipmentDirection.Code.All;
			var transportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			var incoTerm = AccountingMasterFilesConstants.INCOTermCodes.All;

			if (consol != null && (consol is BusinessObject bizO) && !bizO.IsDeleted)
			{
				if (!consol.Direction.IsEmpty)
				{
					directionCode = consol.Direction;
				}

				if (!consol.TransportMode.IsEmpty)
				{
					transportMode = consol.TransportMode;
				}
			}
			result = GetSupplyTypeByChargeCode(chargeCode, JobInvoicingConsumerTypes.ForwardingConsolCode, directionCode, transportMode, incoTerm, ZGuid.Empty);

			return result != null ? result.SupplyType : ZString.Empty;
		}

		static ISupplyTypeSelector GetSupplyTypeByChargeCode(AccChargeCode chargeCode, string consumerTypeCode, string directionCode, string transportMode, string incoTerm, ZGuid department)
		{
			ISupplyTypeSelector result = null;
			if (chargeCode != null)
			{
				if (chargeCode.SupplyTypeOverrides != null)
				{
					result = FindSupplyTypeConfiguration(chargeCode.SupplyTypeOverrides, consumerTypeCode, directionCode, transportMode, incoTerm, department);
				}

				if (result == null)
				{
					var supplyTypeConfigurationByChargeGroup = GetSupplyTypeConfigurationByChargeGroups(chargeCode.Factory)
						.Cast<SupplyTypeConfigurationByChargeGroup>()
						.FirstOrDefault(x => x.ChargeGroup == chargeCode.AC_ChargeGroup);

					if (supplyTypeConfigurationByChargeGroup != null)
					{
						result = FindSupplyTypeConfiguration(supplyTypeConfigurationByChargeGroup.ChargeGroupSettings, consumerTypeCode, directionCode, transportMode, incoTerm, department);
					}
				}
			}
			return result;
		}

		static SupplyTypeConfigurationByChargeGroupCollection GetSupplyTypeConfigurationByChargeGroups(BusinessObjectFactory factory) =>
			factory.GetCachedValue(FindboxLookupCollections.CachingKey, () => AccountingConfigurationRegistry.Instance.SupplyTypeConfigurationByChargeGroup.Value);

		static ISupplyTypeSelector FindSupplyTypeConfiguration(IEnumerable<BusinessObject> supplyTypeConfigurations, string jobType, string direction, string mode, string incoTerm, ZGuid department)
		{
			foreach (ISupplyTypeSelector supplyTypeConfiguration in supplyTypeConfigurations)
			{
				if ((jobType == null ||
					 string.Equals(supplyTypeConfiguration.JobType, jobType, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(supplyTypeConfiguration.JobType, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase))
					&&
					(direction == null ||
					 string.Equals(supplyTypeConfiguration.DirectionCode, direction, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(supplyTypeConfiguration.DirectionCode, Constants.FreightShipmentDirection.Code.All, StringComparison.OrdinalIgnoreCase) ||
					 supplyTypeConfiguration.DirectionCode == "")
					&&
					(mode == null ||
					 string.Equals(supplyTypeConfiguration.Mode, mode, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(supplyTypeConfiguration.Mode, JobConfigurationSelectorLookups.ModeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 supplyTypeConfiguration.Mode == "")
					&&
					(incoTerm == null ||
					 string.Equals(supplyTypeConfiguration.Incoterm, incoTerm, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(supplyTypeConfiguration.Incoterm, AccountingMasterFilesConstants.INCOTermCodes.All, StringComparison.OrdinalIgnoreCase))
					&&
					 (supplyTypeConfiguration.LineDepartmentPK.IsEmpty || department == supplyTypeConfiguration.LineDepartmentPK))
				{
					return supplyTypeConfiguration;
				}
			}

			return null;
		}
	}
}
