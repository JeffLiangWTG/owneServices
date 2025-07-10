using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Registry
{
	public class GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType : BooleanRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue)
			{
				var calculator = new AccountingPeriodCalculator(new ReadOnlyBusinessObjectFactory());
				var periodManagement = calculator.GetFirstPeriodManagementFromDate(ZDateTime.Now.ToDateTime(), companyPK);

				if (periodManagement == null || periodManagement.AM_StartDate == DateTime.MinValue)
				{
					throw new RegistryValidationException(Res.GetString("3CA090F5-D733-4BB1-BBB7-1E444036CF22", "The Accounting Year must be configured under Manage > General Ledger > Period Management before this registry can be set to \"Yes\""));
				}
			}
			else
			{
				throw new RegistryValidationException(Res.GetString("B60189F0-F3B4-4EC0-993A-08477F06E0DB", "This registry can't be disabled."));
			}
		}
	}
}
