using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	public static class BusinessObjectToCopyTemplateReflectionHelper
	{
		public static IEnumerable<PropertyInfo> GetProperties(Type componentType, bool includeInterfaces = true, bool useUniversalCopyIgnoreElementAttribute = false)
		{
			if (componentType == null)
			{
				yield break;
			}

			foreach (PropertyInfo propertyInfo in GetPropertiesWithAttribute(componentType, useUniversalCopyIgnoreElementAttribute))
			{
				yield return propertyInfo;
			}

			foreach (PropertyInfo propertyInfo in GetProperties(componentType.BaseType, includeInterfaces, useUniversalCopyIgnoreElementAttribute))
			{
				yield return propertyInfo;
			}

			if (includeInterfaces)
			{
				foreach (var interfaceType in componentType.GetInterfaces())
				{
					foreach (PropertyInfo propertyInfo in GetProperties(interfaceType, useUniversalCopyIgnoreElementAttribute: useUniversalCopyIgnoreElementAttribute))
					{
						yield return propertyInfo;
					}
				}
			}
		}

		static PropertyInfo[] GetPropertiesWithAttribute(Type componentType, bool useUniversalCopyIgnoreElementAttribute)
		{
			var propertyInfos = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			var excludedElements = useUniversalCopyIgnoreElementAttribute ? BusinessObjectCopyManager.CopyTreeConfiguration.GetExcludedElements(null, componentType) : Enumerable.Empty<string>();
			return propertyInfos.Where(x => !excludedElements.Contains(x.Name)).ToArray();
		}

		public static CollectionCopyTemplateNode GetCollectionCopyTemplateNode(string elementGlowInterfaceName, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, string collectionName,
			string itemsTableName, string itemPropertyName, string itemParentTablePropertyName, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterface(elementGlowInterfaceName);
			return GetCollectionCopyTemplateNode(collectionName, null, interfaceType, preProcessedTypes, itemsTableName, itemPropertyName, itemParentTablePropertyName, exceptionThrownHandle);
		}

		public static CollectionCopyTemplateNode GetCollectionCopyTemplateNode(Type collectionType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, string collectionName,
			string itemsTableName, string itemPropertyName, string itemParentTablePropertyName, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			var elementType = typeof(IBusinessObjectCollection).IsAssignableFrom(collectionType) ? BusinessObjectCollection.GetElementTypeFromCollectionType(collectionType) : null;
			return GetCollectionCopyTemplateNode(collectionName, elementType, null, preProcessedTypes, itemsTableName, itemPropertyName, itemParentTablePropertyName, exceptionThrownHandle);
		}

		static CollectionCopyTemplateNode GetCollectionCopyTemplateNode(string collectionName, Type elementType, Type interfaceType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes,
			string itemsTableName, string itemPropertyName, string itemParentTablePropertyName, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			var elementCopyTemplateNode = elementType != null || interfaceType != null
				? GetEntityCopyTemplateNode(elementType, interfaceType, preProcessedTypes, exceptionThrownHandle, itemPropertyName, itemParentTablePropertyName)
				: null;

			if (elementCopyTemplateNode != null)
			{
				var collectionCopyTemplateNode = new CollectionCopyTemplateNode
				{
					InnerNode = elementCopyTemplateNode,
					Name = collectionName,
					ItemsTableName = itemsTableName,
					ItemPropertyName = itemPropertyName,
					ItemParentTablePropertyName = itemParentTablePropertyName
				};

				return collectionCopyTemplateNode;
			}

			return null;
		}

		public static RelatedEntityCopyTemplateNode GetRelatedEntityCopyTemplateNode(Type entityType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, string entityName, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle, params string[] skipProperties)
		{
			var entityCopyTemplateNode = GetEntityCopyTemplateNode(entityType, null, preProcessedTypes, exceptionThrownHandle, skipProperties);

			if (entityCopyTemplateNode != null)
			{
				var relatedEntityCopyTemplateNode = new RelatedEntityCopyTemplateNode
				{
					InnerNode = entityCopyTemplateNode,
					Name = entityName
				};

				return relatedEntityCopyTemplateNode;
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		public static CopyTemplateNode GetEntityCopyTemplateNode(Type elementType, Type interfaceType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle, params string[] skipProperties)
		{
			CopyTemplateNode copyTemplateNode = null;
			if (preProcessedTypes == null)
			{
				preProcessedTypes = new Dictionary<Type, EntityCopyTemplateNode>();
			}

			interfaceType = interfaceType ?? (elementType != null ? GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true) : null);

			if (interfaceType != null && (elementType == null || skipProperties == null || !skipProperties.Contains("*")))
			{
				copyTemplateNode = CopyTemplateTree.InitializeSubEntityNode(interfaceType, elementType, preProcessedTypes, BusinessObjectCopyManager.CopyTreeConfiguration, exceptionThrownHandle);

				if (skipProperties != null)
				{
					var entityCopyTemplateNode = copyTemplateNode as EntityCopyTemplateNode;
					if (entityCopyTemplateNode == null)
					{
						preProcessedTypes.TryGetValue(interfaceType, out entityCopyTemplateNode);
					}

					entityCopyTemplateNode?.Nodes.RemoveAll(node => node is PropertyCopyTemplateNode && skipProperties.Contains(node.Name, StringComparer.Ordinal));
				}
			}
			else if (elementType != null)
			{
				EntityCopyTemplateNode entityCopyTemplateNode;
				if (preProcessedTypes.TryGetValue(elementType, out entityCopyTemplateNode))
				{
					return new TemplateCopyTemplateNode(entityCopyTemplateNode);
				}

				entityCopyTemplateNode = new EntityCopyTemplateNode(elementType);
				preProcessedTypes.Add(elementType, entityCopyTemplateNode);

				var allProperties = skipProperties != null && skipProperties.Contains("*")
					? Array.Empty<PropertyInfo>()
					: GetProperties(elementType, useUniversalCopyIgnoreElementAttribute: true)
						.Where(propertyInfo => typeof(ZPropertyInfo).IsAssignableFrom(propertyInfo.PropertyType) ||
							(typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) &&
								propertyInfo.CanWrite &&
								propertyInfo.Name != "PK" &&
								(skipProperties == null || !skipProperties.Contains(propertyInfo.Name)))
							)
						.ToArray();

				foreach (var propertyInfo in allProperties)
				{
					if (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) &&
						!CopyTemplateTree.IsSpecialField(propertyInfo) &&
						!CopyTemplateTree.IsOnBusinessObjectIgnoreList(propertyInfo.PropertyType) &&
						entityCopyTemplateNode.Nodes.All(node => node.Name != propertyInfo.Name) &&
						allProperties.Any(info => info.Name == (propertyInfo.Name + "Info") && typeof(ZPropertyInfo).IsAssignableFrom(info.PropertyType)))
					{
						entityCopyTemplateNode.Nodes.Add(new PropertyCopyTemplateNode(propertyInfo) { PropertyType = GetSystemTypeFromZType(propertyInfo.PropertyType).Name });
					}
				}

				BusinessObjectCopyManager.ReflectExtendedPropertiesIfNeeded(entityCopyTemplateNode, interfaceType, elementType, preProcessedTypes, exceptionThrownHandle);

				copyTemplateNode = entityCopyTemplateNode;
			}

			return copyTemplateNode;
		}

		public static Type GetSystemTypeFromZType(Type type)
		{
			if (type == typeof(ZString))
			{
				return typeof(string);
			}

			if (type == typeof(ZInt))
			{
				return typeof(int);
			}

			if (type == typeof(ZDecimal))
			{
				return typeof(decimal);
			}

			if (type == typeof(ZBool))
			{
				return typeof(bool);
			}

			if (type == typeof(ZDateTime))
			{
				return typeof(DateTime);
			}

			if (type == typeof(ZDate))
			{
				return typeof(DateTime);
			}

			if (type == typeof(ZGuid))
			{
				return typeof(Guid);
			}

			if (type == typeof(ZBlob))
			{
				return typeof(byte[]);
			}

			if (type == typeof(ZByte))
			{
				return typeof(byte);
			}

			if (type == typeof(ZShort))
			{
				return typeof(short);
			}

			return type;
		}
	}
}
