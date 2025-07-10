using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business;

public class ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem : StringRegistryItem
{
	public ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
		: base(name, category, caption, hint, new ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType(), editorInfo, storage, options, defaultValue)
	{
	}
}

public class ExternalMonitoringSqlExecutionPlanQueryUriRegistryDataType : StringRegistryDataType
{
	protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		if (string.IsNullOrEmpty(proposedValue))
		{
			return;
		}

		if (!UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(proposedValue) || !ContainsQueryTemplate(proposedValue))
		{
			throw new RegistryValidationException(
				@"The uri value should be in a format of http/https URL. It should have ""{0}"" accepting the query hash value, and ""{1}"" accepting the database server instance name in the query string.");
		}
	}

	bool ContainsQueryTemplate(string proposedValue)
	{
		return proposedValue.Contains("={0}")
				&& proposedValue.Contains("={1}");
	}
}
