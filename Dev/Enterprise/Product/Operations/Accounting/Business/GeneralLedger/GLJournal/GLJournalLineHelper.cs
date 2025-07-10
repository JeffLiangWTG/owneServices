using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public static partial class GLJournalLineHelper
	{
		public static bool IsGlAccountConfiguredAsControlOrLinkAccount(Guid glHeaderPk)
		{
			foreach (var item in ControlOrLinkAccounts)
			{
				var registryValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var guidVal = Guid.Empty;
				if (Guid.TryParse(registryValue.ToString(), out guidVal))
				{
					if (guidVal == glHeaderPk)
					{
						return true;
					}
				}
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static List<IRegistryItem> controlOrLinkAccounts;

		static List<IRegistryItem> ControlOrLinkAccounts
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
		}

		public static bool IsGlAccountConfiguredAsPlAppropriationAccount(Guid glHeaderPK)
		{
			return glHeaderPK == (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}
	}
}
