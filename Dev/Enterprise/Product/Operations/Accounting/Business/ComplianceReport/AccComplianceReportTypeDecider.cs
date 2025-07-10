using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var reportType = row[AccComplianceReport.Schema.ACR_ReportType].ToString();
			var companyPK = (Guid)row[AccComplianceReport.Schema.ACR_GC_Company];
		
			var reportTablePrefix = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)
				.Cast<ComplianceReportConfiguration>()
				.FirstOrDefault(x => x.ReportCode == reportType)?.ReportBaseTablePrefix;

			if (reportTablePrefix?.ToString() == ReportBaseTablePrefixListCodes.GeneralLedgerData)
			{
				var eDWServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				if (IsCompliaceReportUsingEDWEnabled && !string.IsNullOrEmpty(eDWServerName))
				{
					return typeof(AccGLDComplianceReportUsingEDW);
				}
				else
				{
					return typeof(AccGLDComplianceReport);
				}
			}
			else
			{
				return typeof(AccComplianceReport);
			}
		}

		bool IsCompliaceReportUsingEDWEnabled
		{
			get
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature);
				if (featureData != null && featureData.TryDeserializeParameterAsJson<GeneralLedgerDataFeatureControlModel>(out var generalLedgerDataFeatureControl))
				{
					if (generalLedgerDataFeatureControl.EnableGLDComplianceReportUsingEDW)
					{
						return true;
					}
				}

				return false;
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(AccComplianceReport);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AccComplianceReport);
		}
	}
}
