using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

#if DEBUG

#endif

namespace Enterprise.Accounting.GUI.ARAP
{
	public class BankAccountSelectionObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BankAccountSelectionObject(AccBankAccountCollection defaultBankAccounts) : base()
		{
			fDefaultBankAccounts = defaultBankAccounts;
		}

		public BankAccountSelectionObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		[List("DefaultBankAccounts")]
		public ZGuid SelectedBankAccount
		{
			get { return fSelectedBankAccount; }
			set
			{
				fSelectedBankAccount = value;
				SelectedBankAccountInfo.RefreshBinding();
			}
		}
		ZGuid fSelectedBankAccount;

		public ZPropertyInfo SelectedBankAccountInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedBankAccount)); }
		}

		public void SetNothingSelected()
		{
			SelectedBankAccount = ZGuid.Empty;
		}

		public AccBankAccountCollection DefaultBankAccounts
		{
			get
			{
				if (fDefaultBankAccounts == null)
				{
					fDefaultBankAccounts = new AccBankAccountCollection(Factory);
				}
				return fDefaultBankAccounts;
			}
		}
		AccBankAccountCollection fDefaultBankAccounts;
	}
}
