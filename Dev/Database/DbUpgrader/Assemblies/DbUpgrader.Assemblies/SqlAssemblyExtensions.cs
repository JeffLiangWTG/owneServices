using System;
using CargoWise.Data.SqlClr.Registration;

namespace Enterprise.DbUpgrader.Assemblies
{
	static class SqlAssemblyExtensions
	{
		public static AssemblyPermission GuessPermissionSet(this SqlAssembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			switch (assembly.Name)
			{
				case "CargoWise.Data.SqlClr.ExtAccess": return AssemblyPermission.EXTERNAL_ACCESS;
				case "CargoWise.Data.SqlClr.SafeAccess": return AssemblyPermission.SAFE;
				default: return AssemblyPermission.SAFE;
			}
		}
	}
}
