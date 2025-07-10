using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Common.CodeMappings
{
	public static class EDICodeMapper
	{
		public static CodeMapping FindGlobalCodeMapping(string tableName, string propertyName)
		{
			var codeMappings = GlobalDefinition.Instance.CodeMappings;
			return codeMappings[tableName, propertyName];
		}

		public static CodeMapping FindEntityCodeMapping(IEntity entity, Property property)
		{
			var codeMappings = entity.Definition.EntitySetDefinition.CodeMappings;
			return codeMappings[entity.TableName, property.Name];
		}

		public static Guid GetDefaultOrgPK()
		{
			var orgHeader = OrgHeader.DefaultOrg;
			return orgHeader == null ? Guid.Empty : orgHeader.PK.ToGuid();
		}

		public static string GetDefaultOrgCode(BusinessObjectFactory factory = null)
		{
			var orgHeader = factory == null ? OrgHeader.DefaultOrg : OrgHeader.GetDefaultOrg(factory);
			return orgHeader == null ? String.Empty : orgHeader.OH_Code.ToString();
		}
	}
}
