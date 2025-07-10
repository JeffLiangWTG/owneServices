using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IBilledUsageCommissionGroupsCalculator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<IGrouping<BillingCommissionGroupingKey, EdiBilledUsage>> GetGroupedUsages(InvoicingBase invoice);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct BillingCommissionGroupingKey
	{
		public BillingCommissionGroupingKey(ZString service, ZString subModule, ZGuid clientCompanyPk, ZGuid licenceDatabasePk)
		{
			this.Service = service;
			this.SubModule = subModule;
			this.ClientCompanyPk = clientCompanyPk;
			this.LicenceDatabasePk = licenceDatabasePk;
		}

		public readonly ZString Service;
		public readonly ZString SubModule;
		public readonly ZGuid ClientCompanyPk;
		public readonly ZGuid LicenceDatabasePk;
	}
}
