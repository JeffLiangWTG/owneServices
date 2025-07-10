using System;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.UniversalCopy
{
	public interface ICopyTreeConfiguration
	{
		void PreInitializeEntity(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType);
		void AdditionalEntityInitialization(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preprocessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle);
		string GetAssociateCollectionPropertyName(Type componentType, string collectionPropertyName);
		Type GetCollectionElementTypeFromCollectionType(Type collectionType);
		Type GetEntityTypeFromTableName(string tableName);
		IEnumerable<string> GetExcludedElements(Type type, Type componentType);
		bool ShouldMoveLinkableOnlyEntityToProperties();
		IEnumerable<PropertyInfo> GetAddInfoProperties(Type componentType);
		IEnumerable<PropertyInfo> GetValueOnlyProperties(Type componentType);
		Type GetPropertyTypeSubstitute(Type type);
	}
}
