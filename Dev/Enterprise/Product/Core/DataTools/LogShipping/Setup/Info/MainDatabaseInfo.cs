using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public class MainDatabaseInfo : DatabaseInfo
	{
		public MainDatabaseInfo(LogShippingInfo setupInfo, string databaseName)
			: base(setupInfo, databaseName)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));
			Argument.NotNull(databaseName, nameof(databaseName));
		}

		public override string SecondaryDatabaseName
		{
			get
			{
				return DatabaseName;
			}
		}

		public List<DependentDatabaseInfo> DependentDatabases { get { return fDependentDatabases ?? (fDependentDatabases = new List<DependentDatabaseInfo>()); } }
		List<DependentDatabaseInfo> fDependentDatabases;
	}
}
