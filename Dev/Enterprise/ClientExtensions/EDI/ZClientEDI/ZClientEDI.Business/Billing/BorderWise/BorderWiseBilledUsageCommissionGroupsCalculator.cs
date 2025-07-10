using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BorderWiseBilledUsageCommissionGroupsCalculator : IBilledUsageCommissionGroupsCalculator
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
					service: GetService(usage.BU9_UsageCode),
					subModule: GetSubModuleCode(usage.BU9_PriceCode),
					clientCompanyPk: usage.BU9_LCC,
					licenceDatabasePk: usage.BU9_LD
				);

			return usages;
		}

		static ZString GetService(ZString usageCode) => usageCode.IsEmpty ? (ZString)BillingConstants.BillingSystem.BorderWise : usageCode;

		static ZString GetSubModuleCode(ZString priceCode) => priceCode.IsEmpty ? (ZString)OrgCommissionAgreementItemLookups.AllSubModulesCode : priceCode;
	}
}

