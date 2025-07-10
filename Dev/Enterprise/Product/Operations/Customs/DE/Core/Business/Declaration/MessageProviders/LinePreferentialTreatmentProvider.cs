using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class LinePreferentialTreatmentProvider : ILinePreferentialTreatment
	{
		public LinePreferentialTreatmentProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		protected readonly JobComInvoiceLine invoiceLine;

		public string RequestedPreferentialTreatment => invoiceLine.JI_PrimaryPreference;

		public IReadOnlyCollection<string> ContingentNumber => contingentNumber ?? (contingentNumber = new string[] { invoiceLine.JI_ConcessionOrder.SubstringSafe(2, 4), invoiceLine.ZG_SecondQuota.SubstringSafe(2, 4) });
		IReadOnlyCollection<string> contingentNumber;

		public IAmount Quantity => CachedValueHelper.GetValue(ref quantity, () => invoiceLine.ZG_QuotaQty > 0 ? new AmountProvider((ZDecimal)invoiceLine.ZG_QuotaQty, invoiceLine.ZG_QuotaUQ) : null);
		CachedValue<IAmount> quantity;
	}
}
