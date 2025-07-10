using System;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Environment
{
	public class DedicatedSqlServerInstanceDefinition : IDedicatedSqlServerInstanceDefinition
	{
		readonly string clientEnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
		public bool IsDedicated(string serverInstanceName)
		{
			const string instancePrefix = "INSTANCE"; // This is our internal prefix for SQL instances
			return serverInstanceName.Equals(instancePrefix + clientEnterpriseCode, StringComparison.OrdinalIgnoreCase);
		}
	}
}
