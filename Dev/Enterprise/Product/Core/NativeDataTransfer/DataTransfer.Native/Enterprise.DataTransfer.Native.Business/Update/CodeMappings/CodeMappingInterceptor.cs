using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Xml;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public class CodeMappingInterceptor : BaseInterceptor
	{
		public CodeMappingInterceptor(CodeMappingSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			this.setting = setting;

			factory = setting.Context.ObjectFactory;
			codeMappingRepository = new CodeMappingRepository(factory);
		}

		public override void Invoke(IEntitySet entitySet)
		{
			var ownerCode = setting.OwnerCode;
			ownerPK = codeMappingRepository.FindOwnerOrgPK(ownerCode);

			TransformProperty(entitySet);

			Function(entitySet);
		}

		#region Code Mapping

		internal void TransformProperty(IEntitySet entitySet)
		{
			var root = entitySet.Root;
			root.DepthFirstTraversal(MapCode);
		}

		void MapCode(IEntity entity, IEntity relative)
		{
			if (entity == null)
			{
				return;
			}

			var tableName = entity.TableName;

			foreach (var property in entity.Properties)
			{
				var codeMapping = EDICodeMapper.FindEntityCodeMapping(entity, property) ??
								  EDICodeMapper.FindGlobalCodeMapping(tableName, property.Name);

				if (codeMapping != null)
				{
					string localCode = string.Empty;
					var mappingRelationship = string.IsNullOrEmpty(codeMapping.Relationship) ? RelationshipResolver.GetRelationshipValue(property) : codeMapping.Relationship;

					if (!string.IsNullOrEmpty(mappingRelationship))
					{
						localCode = codeMappingRepository.MapLocalCode(mappingRelationship, property.Value, ownerPK);
					}

					if (!localCode.IsEmpty())
					{
						var foreignCode = (string)property.Value;
						property.Value = localCode;
						sessionServices.Logger.Log(LogType.Information, LogHelper.GetMappingLogMessage(entity, property.Name, foreignCode, localCode));
					}
				}
			}
		}

		#endregion

		Guid ownerPK;
		readonly CodeMappingSetting setting;
		readonly CodeMappingRepository codeMappingRepository;
		readonly BusinessObjectFactory factory;
	}
}
