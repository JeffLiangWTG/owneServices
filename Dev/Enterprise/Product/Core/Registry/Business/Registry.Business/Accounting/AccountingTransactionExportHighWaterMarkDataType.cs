using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AccountingTransactionExportHighWaterMarkDataType : DateTimeRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != (DateTime)registryItem.DefaultValue)
			{
				throw new RegistryValidationException(Res.GetString("21C8BAC1-AD73-48C5-8917-F1619427F5CF", "Only the default value can be set"));
			}
		}
	}
}
