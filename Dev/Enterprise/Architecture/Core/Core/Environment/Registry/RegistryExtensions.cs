using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class RegistryExtensions
	{
		public static string HumanReadableRegistryPath(this IRegistryItemInternals item) => item?.Location;

		public static object GetValueWithFallbackDefault(this IRegistryItem registryItem, Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			try
			{
				return !Db.DatabaseUpgradedExceptionHasBeenThrownInConnection
					? registryItem?.GetValueWithoutFallback(companyPk, branchPk, departmentPk)
					: registryItem?.DefaultValue;
			}
			catch (Exception ex) when (ex.IsDbUpgradeOrIsInfrastructureDbError())
			{
				return registryItem?.DefaultValue;
			}
		}
	}
}
