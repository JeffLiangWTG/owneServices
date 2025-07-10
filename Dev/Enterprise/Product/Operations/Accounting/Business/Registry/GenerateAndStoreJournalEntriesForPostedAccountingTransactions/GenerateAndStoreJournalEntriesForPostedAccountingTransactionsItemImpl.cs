using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Billing.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl : BooleanRegistryItem
	{
		public GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue, BooleanRegistryDataType registryDataType)
			: base(new RegistryItemImpl(name, category, caption, hint, registryDataType, null, storage, options, defaultValue, useDefaultDefaultValue: false))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			if ((bool)newValue)
			{
				SetLatestUnprocessedFinanceYearStartDate(companyOrOwnerPK, ZDateTime.Now.ToDateTime());
				new AccUsageCollectorProvider().Report(UsageFeatures.Codes.AccGeneralLedgerData, companyOrOwnerPK);
			}

			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		public static bool SetLatestUnprocessedFinanceYearStartDate(Guid companyPK, DateTime processDate)
		{
			var calculator = new AccountingPeriodCalculator(new ReadOnlyBusinessObjectFactory());
			var generateJournalEntriesStartDate = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate;

			var periodManagement = calculator.GetFirstPeriodManagementFromDate(processDate, companyPK);

			if (periodManagement != null && periodManagement.AM_StartDate != DateTime.MinValue)
			{
				generateJournalEntriesStartDate.SetValue(companyPK, Guid.Empty, Guid.Empty, periodManagement.AM_StartDate.ToDateTime());
				return true;
			}

			return false;
		}
	}
}
