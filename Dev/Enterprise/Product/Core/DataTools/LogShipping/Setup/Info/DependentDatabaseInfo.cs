using System.Text.RegularExpressions;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public class DependentDatabaseInfo : DatabaseInfo
	{
		public DependentDatabaseInfo(MainDatabaseInfo masterDatabase, string databaseName)
			: base(masterDatabase.SetupInfo, databaseName)
		{
			Argument.NotNull(databaseName, nameof(databaseName));
			Argument.NotNull(masterDatabase, nameof(masterDatabase));
		}

		public override string SecondaryDatabaseName
		{
			get
			{
				string databaseName = string.Empty;

				if (SetupInfo.MainDatabase != null)
				{
					string eDocsSuffix = Regex.Match(DatabaseName, @"_SD[0-9]{3}").Groups[0].Value;
					databaseName = SetupInfo.MainDatabase.SecondaryDatabaseName + eDocsSuffix;
				}

				return databaseName;
			}
		}
	}
}
