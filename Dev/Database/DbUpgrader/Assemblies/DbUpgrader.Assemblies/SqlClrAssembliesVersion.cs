using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Assemblies
{
	public static class SqlClrAssembliesVersion
	{
		public static readonly VersionLabel Application = new VersionLabel(SqlClrVersionInformation.MajorVersion, SqlClrVersionInformation.MinorVersion);
	}
}
