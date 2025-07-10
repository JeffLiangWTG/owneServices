using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.UniversalCopy.Interfaces;

namespace CargoWise.UniversalCopy
{
	public class UniversalCopyCustomRelatedEntityManager
	{
		readonly Dictionary<string, IUniversalCopyCustomRelatedEntity> UniversalCopyCustomRelatedEntities;

		public UniversalCopyCustomRelatedEntityManager()
		{
			UniversalCopyCustomRelatedEntities = [];
			foreach (var universalCopyCustomRelatedEntity in
				ObjectFactory.Get<IEnumerable>("UniversalCopyCustomRelatedEntitiesList"))
			{
				UniversalCopyCustomRelatedEntities.Add(((IUniversalCopyCustomRelatedEntity)universalCopyCustomRelatedEntity).RelatedPropertyName,
					(IUniversalCopyCustomRelatedEntity)universalCopyCustomRelatedEntity);
			}
		}

		public void InitializeRelatedEntityCopyTemplateTreeNodes(EntityCopyTemplateNode entityNode, Type type, IDictionary<string, object> processedProperties, Func<Type, CopyTemplateNode> initializeSubEntityNode)
		{
			foreach (var customRelatedEntity in UniversalCopyCustomRelatedEntities.Values)
			{
				if (customRelatedEntity.SourceTypeName == type.Name)
				{
					customRelatedEntity.InitializeRelatedEntityCopyTemplateTreeNode(entityNode, processedProperties, initializeSubEntityNode);
				}
			}
		}

		public IUniversalCopyCustomRelatedEntity GetUniversalCopyCustomRelatedEntity(string relatedPropertyName)
		{
			if (relatedPropertyName is null)
			{
				return null;
			}

			if (UniversalCopyCustomRelatedEntities.TryGetValue(relatedPropertyName, out var universalCopyCustomRelatedEntity))
			{
				return universalCopyCustomRelatedEntity;
			}

			return null;
		}
	}
}
