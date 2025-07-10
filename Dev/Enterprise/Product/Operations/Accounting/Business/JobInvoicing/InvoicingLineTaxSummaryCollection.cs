using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoicingLineTaxSummaryCollection : NonPersistentBusinessObjectCollection<InvoicingLineTaxSummary>
	{
		public InvoicingLineTaxSummaryCollection() { }

		public void Load(InvoicingLineBaseCollection collection)
		{
			RemoveAll();
			GroupDataByTaxIDTaxMsg(collection);
		}

		void GroupDataByTaxIDTaxMsg(InvoicingLineBaseCollection collection)
		{
			var lines = collection.Cast<InvoicingLineBase>().Where(c => c.TaxRate != null);
			if (lines.Any())
			{
				var taxSummaryDic = new Dictionary<KeyValuePair<ZGuid, ZGuid>, InvoicingLineTaxSummary>();

				foreach (var line in lines)
				{
					var taxRatePK = line.TaxRate == null ? ZGuid.Empty : line.TaxRate.PK;
					var taxMsgPK = line.VATClass == null ? ZGuid.Empty : line.VATClass.PK;

					var summaryForThisTaxRateAndMsg = taxSummaryDic.GetOrAdd(new KeyValuePair<ZGuid, ZGuid>(taxRatePK, taxMsgPK));

					summaryForThisTaxRateAndMsg.LocalExTaxAmount += line.AL_LocalExTaxAmount;
					summaryForThisTaxRateAndMsg.LocalTaxAmount += line.AL_LocalTaxAmount;
					summaryForThisTaxRateAndMsg.LocalTotalAmount += line.AL_LocalTotalAmount;

					summaryForThisTaxRateAndMsg.TaxID = line.TaxRate.AT_Code;
					summaryForThisTaxRateAndMsg.TaxIDDescription = line.TaxRate.AT_Description;

					if (line.VATClass != null && summaryForThisTaxRateAndMsg.Message.IsEmpty)
					{
						summaryForThisTaxRateAndMsg.Message = line.VATClass.A9_Code;
					}
				}

				foreach (var item in taxSummaryDic)
				{
					item.Value.TaxRate = item.Value.LocalExTaxAmount == 0M ? 0M : item.Value.LocalTaxAmount / item.Value.LocalExTaxAmount * 100;
					Add(taxSummaryDic[item.Key]);
				}
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}
	}
}
