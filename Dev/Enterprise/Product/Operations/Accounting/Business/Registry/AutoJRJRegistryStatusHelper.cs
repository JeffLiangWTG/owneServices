using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business
{
	public static class AutoJRJRegistryStatusHelper
	{
		public static bool IsAutoJRJEnabled()
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.Value != AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non;
		}

		public static bool IsAutoJRJEnabled(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty) != AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non;
		}

		public static bool IsAutoJRJWithTaxRegistrationNumberEnabled()
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.Value == AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax;
		}

#if DEBUG
		public static IDisposable SetAutoJRJEnabled_ForTestOnly(Guid? companyPK = null)
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(companyPK ?? Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes);
		}

		public static IDisposable SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(Guid? companyPK = null)
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(companyPK ?? Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax);
		}

		public static IDisposable SetAutoJRJDisabled_ForTestOnly(Guid? companyPK = null)
		{
			return AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(companyPK ?? Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non);
		}
#endif
	}
}
