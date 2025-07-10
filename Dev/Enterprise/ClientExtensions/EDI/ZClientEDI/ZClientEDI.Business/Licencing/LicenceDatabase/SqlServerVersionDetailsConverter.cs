using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public static class SqlServerVersionDetailsConverter
	{
		public static ZString ToCode(DbConnection.SqlServerEdition edition)
		{
			switch (edition)
			{
				case DbConnection.SqlServerEdition.Express: return SqlServerEditionList.Codes.Desktop;
				case DbConnection.SqlServerEdition.EnterpriseDeveloper: return SqlServerEditionList.Codes.Enterprise;
				case DbConnection.SqlServerEdition.StandardWorkgroup: return SqlServerEditionList.Codes.Standard;
				default: return SqlServerEditionList.Codes.Other;
			}
		}
	}
}

