using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.UniversalCopy.Interfaces
{
	public interface IUniversalCopyCustomRelatedEntity
	{
		string SourceTypeName { get; }

		string RelatedPropertyName { get; }

		void InitializeRelatedEntityCopyTemplateTreeNode(EntityCopyTemplateNode entityNode,
			IDictionary<string, object> processedProperties, Func<Type, CopyTemplateNode> initializeSubEntityNode);

		ZGuid? GetRelatedEntityObjectFK(BusinessObjectFactory factory,
			RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode, ZGuid relatedEntityFK);

		object GetOriginalRelatedEntityFromRelatedEntityObject(RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode,
			object relatedEntityObject, Func<object, string, object> getPropertyValue);
	}
}
