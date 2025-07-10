using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public class ARAccQueryClaimFilterBusinessObject : AccQueryClaimFilterBusinessObject
	{
		protected override ZString GetLedger()
		{
			return LedgerTypes.AccountsReceivable;
		}

		public override ZString CreditorDebtorName
		{
			get { return (NoResString)"Debtor"; }
		}

		public override MultilingualString CreditorDebtorCaption
		{
			get { return ResString.GetMultilingualString("Accounting|ARAccQueryClaimFilter|Debtor", "Debtor"); }
		}

		public override MultilingualString InvoiceFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|ARAccQueryClaimFilter|InvoiceNumber", "Invoice Number"); }
		}
	}
}
