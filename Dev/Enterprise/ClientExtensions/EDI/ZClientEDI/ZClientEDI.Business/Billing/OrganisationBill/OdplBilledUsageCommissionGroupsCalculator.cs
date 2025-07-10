using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class OdplBilledUsageCommissionGroupsCalculator : IBilledUsageCommissionGroupsCalculator
	{
		public IEnumerable<IGrouping<BillingCommissionGroupingKey, EdiBilledUsage>> GetGroupedUsages(InvoicingBase invoice)
		{
			var factory = invoice.Factory;
			var billedUsagesQuery = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = factory.Load<EdiBilledUsage>(billedUsagesQuery);

			var feeClientCompanyMapping = BuildLicenceCompanyToClientCompanyMappingForFeeUsages(factory, billedUsages);

			return
				from usage in billedUsages
				group usage by
				new BillingCommissionGroupingKey
				(
					service: usage.BU9_UsageCode,
					subModule: FallbackToAllSubModuleCode((usage.BU9_UsageCode == BillingConstants.BillingSystem.Fee) ? usage.BU9_UsageSubCode : usage.BU9_PriceCode),
					clientCompanyPk: GetClientCompanyPk(usage, feeClientCompanyMapping),
					licenceDatabasePk: GetDatabasePk(usage, feeClientCompanyMapping)
				);
		}

		static ZString FallbackToAllSubModuleCode(ZString subModule)
		{
			return subModule.IsEmpty ? (ZString)OrgCommissionAgreementItemLookups.AllSubModulesCode : subModule;
		}

		static ZGuid GetClientCompanyPk(EdiBilledUsage usage, Dictionary<ZGuid, ClientCompany> feeClientCompanyMapping)
		{
			var result = usage.BU9_LCC;
			if (usage.BU9_UsageCode == BillingConstants.BillingSystem.Fee)
			{
				ClientCompany clientCompany = null;
				if (feeClientCompanyMapping.TryGetValue(usage.BU9_LC, out clientCompany))
				{
					result = clientCompany.PK;
				}
			}
			return result;
		}

		static ZGuid GetDatabasePk(EdiBilledUsage usage, Dictionary<ZGuid, ClientCompany> feeClientCompanyMapping)
		{
			var result = usage.BU9_LD;
			if (usage.BU9_UsageCode == BillingConstants.BillingSystem.Fee)
			{
				ClientCompany clientCompany = null;
				if (feeClientCompanyMapping.TryGetValue(usage.BU9_LC, out clientCompany))
				{
					result = clientCompany.LCC_LD;
				}
			}
			return result;
		}

		static Dictionary<ZGuid, ClientCompany> BuildLicenceCompanyToClientCompanyMappingForFeeUsages(BusinessObjectFactory factory, EdiBilledUsage[] billedUsages)
		{
			var result = new Dictionary<ZGuid, ClientCompany>();
			var feeUsageLicenceCompanyPks = billedUsages.Where(x => x.BU9_UsageCode == BillingConstants.BillingSystem.Fee).Select(x => x.BU9_LC).Distinct();

			foreach (var licenceCompanyPk in feeUsageLicenceCompanyPks)
			{
				var clientCompanyQuery = new ZDBOnlyQuery(typeof(ClientCompany));
				var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientCompanySchema.LCC_LD);
				licenceDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
				var licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.PK, licenceCompanyPk);
				clientCompanyQuery.AddSubQuery(licenceDatabaseSubQuery, JoinCondition.And);
				clientCompanyQuery.AddSubQuery(ClientCompanySchema.LCC_OH, licenceCompanySubQuery, JoinCondition.And);

				var clientCompany = factory.LoadTop1<ClientCompany>(clientCompanyQuery);
				if (clientCompany != null)
				{
					result.Add(licenceCompanyPk, clientCompany);
				}
			}

			return result;
		}
	}
}

