#if DEBUG

using System.Collections.Generic;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public static partial class GLJournalLineHelper
	{
		public static List<IRegistryItem> ControlOrLinkAccountsForTest
		{
			get
			{
				if (controlOrLinkAccounts == null)
				{
					controlOrLinkAccounts = AccountingConfigurationRegistry.Instance.GetRegistryItemsByCategoryName(
						MasterFiles.Business.AccountingMasterFilesRegistry.Categories
							.Accounting_GeneralLedgerDefaults_LinkAccount
						, AccountingConfigurationRegistry.Categories.Accounting_GeneralLedgerDefaults_ControlAccount);
				}

				return controlOrLinkAccounts;
			}

			set
			{
				controlOrLinkAccounts = value;
			}
		}
	}
}

#endif
