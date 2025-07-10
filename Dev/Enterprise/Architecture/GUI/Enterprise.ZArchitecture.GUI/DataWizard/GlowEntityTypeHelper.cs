using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	static class GlowEntityTypeHelper
	{
		public static IDataTransferMapping[] GetImportMappings(BusinessObjectFactory factory, Type type)
		{
			var glowInterfaceName = GlowDataDefinitionReference.FromType(type)?.DataDefinitionName;

			if (!string.IsNullOrEmpty(glowInterfaceName))
			{
				var sql = string.Format(
					CultureInfo.InvariantCulture,
					@"SELECT {0} AS PK, {1} AS FilterName, {5} AS MappingData, {2} AS ModuleID
FROM {4}
WHERE {2} LIKE @glowMappingPrefix AND {2} LIKE @interfaceName AND ({3} IS NULL OR {3} = CAST(0x0 AS UNIQUEIDENTIFIER))",
					StmModuleFilterSchema.PK.Name,
					StmModuleFilterSchema.S9_FilterName.Name,
					StmModuleFilterSchema.S9_ModuleID.Name,
					StmModuleFilterSchema.S9_RelatedEntityID.Name,
					StmModuleFilterSchema.Constants.TableName,
					StmModuleFilterSchema.S9_FilterData.Name);

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@glowMappingPrefix", DataTransferConstants.ModulePrefix + "%", StmModuleFilterSchema.S9_ModuleID);
				parameters.Add("@interfaceName", "%" + glowInterfaceName, StmModuleFilterSchema.S9_ModuleID);

				var collection = new DynamicBusinessObjectCollection(factory);
				collection.Load(sql, parameters);

				var mappingParser = ObjectFactory.Get<IDataTransferMappingParser>();
				return collection.Select(m => mappingParser.FromModuleFilterInfo(new ModuleFilterInfo(((ZGuid)m["PK"]).ToGuid(), Guid.Empty, (ZBlob)m["MappingData"], m["FilterName"].ToString(), m["ModuleID"].ToString())))
					.ToArray();
			}

			return Enumerable.Empty<IDataTransferMapping>().ToArray();
		}
	}
}
