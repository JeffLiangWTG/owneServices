using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlBilledUsageCommissionGroupsCalculator : IBilledUsageCommissionGroupsCalculator
	{
		public IEnumerable<IGrouping<BillingCommissionGroupingKey, EdiBilledUsage>> GetGroupedUsages(InvoicingBase invoice)
		{
			var factory = invoice.Factory;
			var billedUsagesQuery = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = factory.Load<EdiBilledUsage>(billedUsagesQuery);

			var usages =
				from usage in billedUsages
				group usage by
				new BillingCommissionGroupingKey
				(
					service: GetService(usage),
					subModule: GetSubModuleCode(usage),
					clientCompanyPk: usage.BU9_LCC,
					licenceDatabasePk: usage.BU9_LD
				);

			return usages;
		}

		static ZString GetService(EdiBilledUsage usage)
		{
			return usage.BU9_UsageCode.IsEmpty ? BillingConstants.BillingSystem.STL : usage.BU9_UsageCode.ToString();
		}

		static ZString GetSubModuleCode(EdiBilledUsage usage)
		{
			var subModule = ZString.Empty;
			if (usage.BU9_UsageCode == BillingConstants.BillingSystem.Fee)
			{
				subModule = usage.BU9_UsageSubCode;
			}
			else
			{
				subModule = usage.BU9_PriceCode;
			}
			return subModule.IsEmpty ? (ZString)OrgCommissionAgreementItemLookups.AllSubModulesCode : subModule;
		}
	}
}

