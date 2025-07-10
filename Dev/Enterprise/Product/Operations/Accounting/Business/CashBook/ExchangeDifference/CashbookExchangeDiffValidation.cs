using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class CashbookExchangeDiffValidation : TransactionHeaderValidation
	{
		public CashbookExchangeDiffValidation(CashbookExchangeDiff parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new CashbookExchangeDiff Parent;

		protected override void CheckAH_AG()
		{
			base.CheckAH_AG();
			if ((!new ZGuid(AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value).IsValid
				|| !new ZGuid(AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value).IsValid)
				&& Parent.ShouldSetAH_AG)
			{
				Parent.AH_AGInfo.AddError(PotentialEmptyAH_AGDueToRegistriesErrorMessage);
			}
		}

		static ResourceString PotentialEmptyAH_AGDueToRegistriesErrorMessage
			=> ResString.GetMultilingualString(
				"FEAFEE32-1899-48A0-81B9-F7E7B160971E"
				, "The transaction does not have a GL account specified.\r\n\r\nPlease check and fill below registry settings:\r\n1.{0}\r\n2.{1}"
				, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.HumanReadableRegistryPath()
				, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.HumanReadableRegistryPath()
				);
	}
}
