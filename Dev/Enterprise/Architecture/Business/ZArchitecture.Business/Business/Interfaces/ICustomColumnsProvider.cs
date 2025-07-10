using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomColumnsProvider
	{
		IEnumerable<IGenCustomColumnDefinition> GetCustomColumnDefinitions(
			BusinessObjectFactory businessObjectFactory,
			string workflowType,
			ZGuid company = default,
			ZGuid branch = default,
			ZGuid department = default);
	}
}
