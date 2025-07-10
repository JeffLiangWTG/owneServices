using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderLookups : AutoAccCollectionOrderLookups
	{
		public AccCollectionOrderLookups(AutoAccCollectionOrder parent)
			: base(parent)
		{
		}

		public RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (bankAccounts == null)
				{
					bankAccounts = AccountingUtils.GetAccBankAccountCollection(Factory);
				}

				return bankAccounts;
			}
		}
		AccBankAccountCollection bankAccounts;
	}
}

