using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public class APAccQueryClaimFilterBusinessObject : AccQueryClaimFilterBusinessObject
	{
		protected override ZString GetLedger()
		{
			return LedgerTypes.AccountsPayable;
		}

		public override ZString CreditorDebtorName
		{
			get { return (NoResString)"Creditor"; }
		}

		public override MultilingualString CreditorDebtorCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APAccQueryClaimFilter|Creditor", "Creditor"); }
		}

		public override MultilingualString InvoiceFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APAccQueryClaimFilter|InvoiceNumber", "Invoice Number"); }
		}
	}
}
