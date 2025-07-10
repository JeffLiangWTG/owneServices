using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public class ARPaymentProcessingFilterBusinessObject : PaymentProcessingFilterBusinessObject
	{
		protected override string OrganisationFilterName
		{
			get { return (NoResString)"Debtor"; }
		}

		protected override MultilingualString OrganisationFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|ARPaymentProcessingFilter|Debtor", "Debtor"); }
		}

		protected override ZString LedgerCode
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		protected override string OrganisationAndAddressFilterName
		{
			get { return (NoResString)"Debtor and Address"; }
		}

		protected override MultilingualString OrganisationAndAddressFilterCaption
		{
			get { return ResString.GetMultilingualString("Accounting|ARPaymentProcessingFilter|DebtorAndAddress", "Debtor and Address"); }
		}

		protected override bool IsDebtor
		{
			get { return true; }
		}
	}
}