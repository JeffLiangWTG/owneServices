using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Shared
{
	public static class OidcLoginDatabaseSchemaHelper
	{
		public static (SchemaColumn schemaColumn, Func<string, IZType> converter) GetWhitelistedSchemaColumnFromName(string schemaColumnName)
		{
			switch (schemaColumnName)
			{
				case $"{GlbStaffSchema.Constants.TableName}.{GlbStaffSchema.Constants.GS_ActiveDirectoryObjectGuid}":
					return (GlbStaffSchema.GS_ActiveDirectoryObjectGuid, ParseAsZGuid);
				case $"{GlbStaffSchema.Constants.TableName}.{GlbStaffSchema.Constants.GS_LoginName}":
					return (GlbStaffSchema.GS_LoginName, (val) => new ZString(val));
				case $"{GlbStaffSchema.Constants.TableName}.{GlbStaffSchema.Constants.GS_EmailAddress}":
					return (GlbStaffSchema.GS_EmailAddress, (val) => new ZString(val));
				default:
					throw new ArgumentOutOfRangeException(nameof(schemaColumnName));
			}

			static IZType ParseAsZGuid(string val)
			{
				if (ZGuid.TryParse(val, out var result))
				{
					return result;
				}
				else
				{
					throw new InvalidCastException($"Cannot parse {val} as {nameof(ZGuid)}");
				}
			}
		}
	}
}
