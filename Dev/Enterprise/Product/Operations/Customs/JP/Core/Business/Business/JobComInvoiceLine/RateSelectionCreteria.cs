using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Customs.Business.BaseJobComInvoiceLine;

namespace Enterprise.Customs.JP.Business
{
	public class RateSelectionCreteria : RateSelectionCriteria<JobComInvoiceLine>
	{
		public RateSelectionCreteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode) : base(invoiceLine, rateType, rateCode)
		{
		}

		protected override ISet<ZString> GetAdditionalCodes(JobComInvoiceLine invoiceLine)
		{
			return new HashSet<ZString> { invoiceLine.JI_SecondaryPreference };
		}
	}
}
