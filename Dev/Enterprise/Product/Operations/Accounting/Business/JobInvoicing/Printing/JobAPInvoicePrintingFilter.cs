using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobAPInvoicePrintingFilter : JobInvoicePrintingFilter
	{
		public JobAPInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK)
			: base(hostBusinessObject, jobHeaderPK)
		{
		}

		public JobAPInvoicePrintingFilter(ForwardingConsol hostBusinessObject, ZGuid[] jobHeaderPKs)
			: base(hostBusinessObject, jobHeaderPKs)
		{
		}

#if DEBUG
		public JobAPInvoicePrintingFilter(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
			: base(hostBusinessObject, jobHeader.PK, jobHeader.Factory, from, to)
		{
		}
#endif

		protected override OrganisationsFindBoxCollection GetDebtorOrCreditorFindBoxCollectionCore()
		{
			return new CreditorCollection(Factory);
		}

		protected override IEnumerable<string> Ledger
		{
			get
			{
				yield return LedgerTypes.AccountsPayable;
			}
		}
	}
}
