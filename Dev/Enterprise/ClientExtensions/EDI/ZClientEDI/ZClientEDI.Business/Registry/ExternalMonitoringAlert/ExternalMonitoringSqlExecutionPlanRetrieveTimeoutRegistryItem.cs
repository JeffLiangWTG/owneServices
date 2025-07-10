using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business;

public class ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem : IntRegistryItem
{
	public ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue)
		: base(name, category, caption, hint, storage, options, defaultValue)
	{
		DataType = new ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType();
	}
}

public class ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryDataType : IntRegistryDataType
{
	protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

	protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		if (proposedValue <= 0)
		{
			throw new RegistryValidationException("The timeout value should be greater than 0.");
		} 
	}
}
