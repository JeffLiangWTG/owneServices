using System;
using Microsoft.SqlServer.Types;

namespace Enterprise.Client.EDI.ReleaseBuilds
{
	public static class SqlVersionStorage
	{
		public static Version ToVersion(this SqlHierarchyId value)
		{
			return Version.Parse(value.ToString().Trim('/').Replace('/', '.'));
		}

		public static SqlHierarchyId ToSqlHierarchyId(this Version value)
		{
			return SqlHierarchyId.Parse("/" + value.ToString().Replace('.', '/') + "/");
		}
	}
}
