using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	public sealed class ClusterKeyMappingsProvider : IClusterKeyMappingsProvider
	{
		public IClusterKeyMappingData[] GetAllClusterKeyMappings() => mappingDataItems ?? (mappingDataItems = GetClusterKeyMappingData());
		IClusterKeyMappingData[] mappingDataItems;

		IClusterKeyMappingData[] GetClusterKeyMappingData()
		{
			var clusterKeyMetaDataAttributes = AssemblyMetaDataReader.GetAttributes<ClusterKeyMetaDataAttribute>(retrieveForAllClients: true);
			var dataItems = new List<IClusterKeyMappingData>();
			foreach (var clusterKeyMetadataAttribute in clusterKeyMetaDataAttributes)
			{
				var entityType = clusterKeyMetadataAttribute.Type;
				var mappingData = CreateMappingDataItem(clusterKeyMetadataAttribute, entityType);
				dataItems.Add(mappingData);
			}

			return dataItems.ToArray();
		}

		static ClusterKeyMappingData CreateMappingDataItem(ClusterKeyMetaDataAttribute clusterKeyMetadataAttribute, Type entityType)
		{
			var entityTableName = string.IsNullOrEmpty(clusterKeyMetadataAttribute.TableName) ? entityType.Name : clusterKeyMetadataAttribute.TableName;
			var mappingData = new ClusterKeyMappingData(entityTableName, clusterKeyMetadataAttribute.ParentTableName, clusterKeyMetadataAttribute.ParentFkColumnName);
			mappingData.SetSecondaryParentInformation(clusterKeyMetadataAttribute.SecondaryParentTableName, clusterKeyMetadataAttribute.SecondaryParentFkColumnName);
			if (typeof(IClusterKeyMasterEntity).IsAssignableFrom(entityType))
			{
				mappingData.SetCanBeTopmostTableToTrue();
			}

			return mappingData;
		}
	}
}
