using System;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China
{
	public class ChinaOverrideTransactionLineSequenceProvider : IOverrideTransactionLineSequenceProvider
	{
		public bool CanOverrideTransactionLineSequence(InvoicingBase invoicingBase)
		{
			return AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				  && AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.GetFallBackValueAtAllLevels(invoicingBase.AH_GC.ToGuid(), invoicingBase.AH_GB.ToGuid(), Guid.Empty)
				  && invoicingBase.Lines.Any(x => ((InvoicingLineBase)x).AL_LineAmount < 0)
				  && invoicingBase.Lines.Any(x => ((InvoicingLineBase)x).AL_AT.IsValid)
				  && !invoicingBase.AH_LocalTotal.IsEmpty
				  && !string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(invoicingBase.Company.PK.ToGuid(), invoicingBase.Branch.PK.ToGuid(), Guid.Empty));
		}
	}
}
