using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthAmendStatusCodeProvider : IAmendStatusCodeProvider
	{
		public bool IsSupportAmendStatusCode(BusinessObject sourceBusinessEntity)
		{
			var isARCreditNote = sourceBusinessEntity is ARCreditNote;
			var isAmendingAR = sourceBusinessEntity is ARInvoice aRInvoice && aRInvoice.IsAmendingTransaction;
			return ShouldShowAmendStatusCode() && (isARCreditNote || isAmendingAR);
		}

		public bool ShouldShowAmendStatusCode()
		{
			return AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;
		}

		public ZString AmendStatusCodeReferenceType => AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.KRE;

		public ReadOnlyCodeDescriptionPairList AmendStatusCodeList => AccountingConfigurationRegistry.Instance.KoreaEInvoicingAmendmentStatusCode.Value;
	}
}
