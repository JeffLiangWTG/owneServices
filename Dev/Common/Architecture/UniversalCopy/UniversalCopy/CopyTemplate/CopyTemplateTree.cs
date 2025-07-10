using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy
{
	public class CopyTemplateTree : WrappedCopyTemplateNode
	{
		public CopyTemplateTree()
		{
			SetDefaultValue();
		}

		public CopyTemplateTree(Type type, Type componentType = null, ICopyTreeConfiguration copyTreeConfiguration = null, EventHandler<ExceptionThrownEventArgs> exceptionThrownHandle = null)
			: base(CreateEntityNodeFromTypeDefinition(type, componentType, new Dictionary<Type, EntityCopyTemplateNode>(), copyTreeConfiguration, exceptionThrownHandle))
		{
			Argument.NotNull(type, nameof(type));
			TableName = GetTableName(type);
			SetDefaultValue();
		}

		void SetDefaultValue()
		{
			ConfigurationSource = ConfigurationSourceCodes.Selected;
		}

		public string ConfigurationName { get; set; }

		public EntityFilter Filter { get; set; }

		[XmlAttribute("ConfigurationSource")]
		public string ConfigurationSource { get; set; }

		[XmlAttribute("NominatedRecordPk")]
		public Guid NominatedRecordPk { get; set; }

		[XmlAttribute("FilterList")]
		public string FilterList { get; set; }

		[XmlAttribute("TableName")]
		public string TableName { get; set; }

		[XmlAttribute("Active")]
		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}
		bool isActive = true;

		#region Type reflection

		static EntityCopyTemplateNode CreateEntityNodeFromTypeDefinition(Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, ICopyTreeConfiguration copyTreeConfiguration, EventHandler<ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			Argument.NotNull(preProcessedTypes, nameof(preProcessedTypes));
			Argument.NotNull(type, nameof(type));
			var entityNode = new EntityCopyTemplateNode(type);
			preProcessedTypes.Add(type, entityNode);

			var processedProperties = new Dictionary<string, object>();

			if (copyTreeConfiguration != null)
			{
				copyTreeConfiguration.PreInitializeEntity(entityNode, type, componentType);
				foreach (var excludedElement in copyTreeConfiguration.GetExcludedElements(type, componentType))
				{
					processedProperties.Add(excludedElement, null);
				}
			}

			InitializeCollections(entityNode, type, componentType, preProcessedTypes, processedProperties, copyTreeConfiguration, exceptionThrownHandle);
			var properties = GetPropertiesWithUniqueName(type);
			InitializeRelatedEntities(entityNode, type, properties, componentType, preProcessedTypes, processedProperties, copyTreeConfiguration, exceptionThrownHandle);
			InitializeProperties(entityNode, properties, componentType, processedProperties, copyTreeConfiguration);
			InitializeBlobProperties(entityNode, type, processedProperties);
			InitializeValueOnlyProperties(entityNode, componentType, processedProperties, copyTreeConfiguration);

			if (copyTreeConfiguration != null)
			{
				copyTreeConfiguration.AdditionalEntityInitialization(entityNode, type, componentType, preProcessedTypes, exceptionThrownHandle);
			}

			entityNode.FullyBuilt = true;
			return entityNode;
		}

		static void InitializeCollections(EntityCopyTemplateNode entityNode, Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, IDictionary<string, object> processedProperties, ICopyTreeConfiguration copyTreeConfiguration, EventHandler<ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(entityNode, nameof(entityNode));
			Argument.NotNull(processedProperties, nameof(processedProperties));
			Argument.NotNull(preProcessedTypes, nameof(preProcessedTypes));
			foreach (CollectionRelationPropertyAttribute collectionRelationAttribute in type.GetCustomAttributes(typeof(CollectionRelationPropertyAttribute), false))
			{
				PropertyInfo collectionPropertyInfo = GetProperty(type, collectionRelationAttribute.CollectionName);
				if (collectionPropertyInfo != null && !IsSpecialCollection(collectionRelationAttribute) && !processedProperties.ContainsKey(collectionPropertyInfo.Name))
				{
					Type collectionType = collectionPropertyInfo.PropertyType;
					if (collectionType.IsGenericType)
					{
						var args = collectionType.GetGenericArguments();
						Type elementType = args[0];

						Type componentElementType = null;
						if (copyTreeConfiguration != null)
						{
							var componentPropertyInfo = GetCollectionComponentPropertyInfo(componentType, collectionPropertyInfo, copyTreeConfiguration);
							if (componentPropertyInfo != null)
							{
								componentElementType = copyTreeConfiguration.GetCollectionElementTypeFromCollectionType(componentPropertyInfo.PropertyType);
							}

							if (componentElementType == null)
							{
								var tableName = GetTableName(elementType);
								if (!string.IsNullOrEmpty(tableName))
								{
									componentElementType = copyTreeConfiguration.GetEntityTypeFromTableName(tableName);
								}
							}
						}

						var elementNode = InitializeSubEntityNode(elementType, componentElementType, preProcessedTypes, copyTreeConfiguration, exceptionThrownHandle);
						var collectionNode = new CollectionCopyTemplateNode(collectionRelationAttribute, GetTableName(elementType), elementNode);

						var parentTableAttribute = GetCustomAttributes<ParentTableColumnAttribute>(elementType).FirstOrDefault();
						if (parentTableAttribute != null)
						{
							collectionNode.ItemParentTablePropertyName = parentTableAttribute.ColumnName;
						}

						entityNode.Nodes.Add(collectionNode);
						processedProperties.Add(collectionPropertyInfo.Name, null);
					}
				}
			}
		}

		static PropertyInfo GetCollectionComponentPropertyInfo(Type componentType, PropertyInfo collectionPropertyInfo, ICopyTreeConfiguration copyTreeConfiguration)
		{
			PropertyInfo componentPropertyInfo = null;
			if (componentType != null)
			{
				componentPropertyInfo = GetProperty(componentType, collectionPropertyInfo.Name);
				if (componentPropertyInfo == null)
				{
					var associateCollectionPropertyName = copyTreeConfiguration.GetAssociateCollectionPropertyName(componentType, collectionPropertyInfo.Name);
					if (!string.IsNullOrEmpty(associateCollectionPropertyName))
					{
						componentPropertyInfo = GetProperty(componentType, associateCollectionPropertyName);
					}
				}
			}

			return componentPropertyInfo;
		}

		static bool IsSpecialCollection(CollectionRelationPropertyAttribute collectionAttribute)
		{
			Argument.NotNull(collectionAttribute, nameof(collectionAttribute));
			return SpecialCollectionNames.Contains(collectionAttribute.CollectionName);
		}

		static bool IsExcludedField(Type propertyType)
		{
			Argument.NotNull(propertyType, nameof(propertyType));
			return ExcludedInterfaces.Contains(propertyType.Name);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer constant")]
		static void InitializeRelatedEntities(EntityCopyTemplateNode entityNode, Type type, IEnumerable<PropertyInfo> properties, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, IDictionary<string, object> processedProperties, ICopyTreeConfiguration copyTreeConfiguration, EventHandler<ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			Argument.NotNull(entityNode, nameof(entityNode));
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(processedProperties, nameof(processedProperties));
			Argument.NotNull(preProcessedTypes, nameof(preProcessedTypes));

			var customRelatedEntityManager = new UniversalCopyCustomRelatedEntityManager();

			customRelatedEntityManager.InitializeRelatedEntityCopyTemplateTreeNodes(entityNode, type, processedProperties,
				(Type type) => InitializeSubEntityNode(type, componentType,
					preProcessedTypes, copyTreeConfiguration, exceptionThrownHandle));

			foreach (var propertyInfo in properties.Where(info => info.IsDefined(typeof(RelationPropertyAttribute), false) && !processedProperties.ContainsKey(info.Name)))
			{
				var isNodeAdded = false;
				try
				{
					RelationPropertyAttribute relatedPropertyAttribute = null;
					var attrs = (IEnumerable<Attribute>)propertyInfo.GetCustomAttributes(typeof(RelationPropertyAttribute), false);
					if (attrs.Any())
					{
						relatedPropertyAttribute = (RelationPropertyAttribute)attrs.First();
					}
					var relatedPropertyName = relatedPropertyAttribute.PropertyName;
					var relatedPropertyInfo = GetProperty(type, relatedPropertyName);
					if (relatedPropertyInfo != null &&
						IsPropertyWriteable(relatedPropertyInfo) &&
						!IsSpecialField(relatedPropertyInfo) &&
						!processedProperties.ContainsKey(relatedPropertyName) &&
						!IsOnBusinessObjectIgnoreList(propertyInfo.PropertyType) &&
						!IsExcludedField(propertyInfo.PropertyType))
					{
						var componentPropertyInfo = componentType != null ? GetProperty(componentType, propertyInfo.Name) : null;
						var componentPropertyType = componentPropertyInfo != null ? componentPropertyInfo.PropertyType : null;
						if (componentPropertyType == null && copyTreeConfiguration != null)
						{
							var tableName = GetTableName(propertyInfo.PropertyType);
							if (!string.IsNullOrEmpty(tableName))
							{
								componentPropertyType = copyTreeConfiguration.GetEntityTypeFromTableName(tableName);
							}
						}

						if (componentPropertyType == null || !IsOnBusinessObjectIgnoreList(componentPropertyType))
						{
							var infoTypeAttributes = propertyInfo.PropertyType.GetCustomAttributes<InfoTypeForAttribute>();
							var isInfoType = infoTypeAttributes.Any();
							var shouldMoveLinkablesToProperties = copyTreeConfiguration != null && copyTreeConfiguration.ShouldMoveLinkableOnlyEntityToProperties();

							RelatedEntityCopyTemplateNode relatedEntityNode = null;
							if (!isInfoType || !shouldMoveLinkablesToProperties)
							{
								var elementNode = !isInfoType
									? InitializeSubEntityNode(propertyInfo.PropertyType, componentPropertyType, preProcessedTypes, copyTreeConfiguration, exceptionThrownHandle)
									: new EntityCopyTemplateNode(type);

								relatedEntityNode = new RelatedEntityCopyTemplateNode(propertyInfo.Name, relatedPropertyName, GetTableName(propertyInfo.PropertyType), elementNode);
								if (isInfoType)
								{
									relatedEntityNode.DisableCopyMethodCopy = true;
								}
							}

							if (relatedEntityNode == null || (!relatedEntityNode.CanCopy && shouldMoveLinkablesToProperties))
							{
								entityNode.Nodes.Add(new PropertyCopyTemplateNode(relatedPropertyInfo));
								isNodeAdded = true;

								// This block of code is added for [Issue 01244428].
								// If the culprit of this issue is found and the issue is fixed,
								// please remove the added code.
								if (relatedPropertyInfo.Name != relatedPropertyName)
								{
									var errorMessage = new StringBuilder();
									errorMessage.AppendLine((NoResString)"relatedPropertyInfo.Name: " + relatedPropertyInfo.Name);
									errorMessage.AppendLine(nameof(relatedPropertyName) + relatedPropertyName);
									ErrorReporter.ReportOnce("CopyTemplateTree_PreventDuplicatePropertyName", errorMessage.ToString());
								}
							}
							else
							{
								entityNode.Nodes.Add(relatedEntityNode);
								isNodeAdded = true;
							}
						}

						processedProperties.Add(relatedPropertyName, null);
					}
					processedProperties.Add(propertyInfo.Name, null);
				}
				catch (Exception ex)
				{
					const string handled = "handled";

					if (isNodeAdded)
					{
						ErrorReporter.ReportOnce("CopyTemplateTree_PreventDuplicatePropertyName", ex.Message);
					}

					if (ex.Data[handled] == null)
					{
						var args = new ExceptionThrownEventArgs(ex.Message);

						if (exceptionThrownHandle != null)
						{
							exceptionThrownHandle(null, args);
						}

						if (args.Continue)
						{
							continue;
						}

						ex.Data[handled] = true;
						throw;
					}

					throw;
				}
			}
		}

		public class ExceptionThrownEventArgs : EventArgs
		{
			public ExceptionThrownEventArgs(string message)
			{
				Message = message;
			}

			public bool Continue { get; set; }
			public string Message { get; }
		}

		public static PropertyInfo GetProperty(Type componentType, string propertyName)
		{
			Argument.NotNull(propertyName, nameof(propertyName));
			if (componentType == null)
			{
				return null;
			}

			var propertyInfo = componentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (propertyInfo == null && componentType.BaseType != componentType && componentType.BaseType != typeof(object) && componentType.BaseType != null)
			{
				propertyInfo = GetProperty(componentType.BaseType, propertyName);
			}

			if (propertyInfo == null)
			{
				foreach (var interfaceType in componentType.GetInterfaces())
				{
					propertyInfo = GetProperty(interfaceType, propertyName);
					if (propertyInfo != null)
					{
						break;
					}
				}
			}

			return propertyInfo;
		}

		public static CopyTemplateNode InitializeSubEntityNode(Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, ICopyTreeConfiguration copyTreeConfiguration, EventHandler<ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			Argument.NotNull(preProcessedTypes, nameof(preProcessedTypes));
			Argument.NotNull(type, nameof(type));

			EntityCopyTemplateNode node;
			if (preProcessedTypes.TryGetValue(type, out node) && node != null)
			{
				if (node.FullyBuilt && node.Nodes.Count == 0)
				{
					return TemplateCopyTemplateNode.CloneNodeForNotNullSource(node);
				}

				return new TemplateCopyTemplateNode(node);
			}

			return CreateEntityNodeFromTypeDefinition(type, componentType, preProcessedTypes, copyTreeConfiguration, exceptionThrownHandle);
		}

		static string GetTableName(Type type)
		{
			Argument.NotNull(type, nameof(type));
			var tableNameAttribute = (TableNameProviderAttribute)type.GetCustomAttributes(typeof(TableNameProviderAttribute), true).FirstOrDefault();
			string tableName = tableNameAttribute?.TableNames.FirstOrDefault();

			if (string.IsNullOrEmpty(tableName))
			{
				foreach (Type interfaceType in type.GetInterfaces())
				{
					tableName = GetTableName(interfaceType);
					if (!string.IsNullOrEmpty(tableName))
					{
						break;
					}
				}
			}

			return tableName;
		}

		static void InitializeProperties(EntityCopyTemplateNode entityNode, IEnumerable<PropertyInfo> properties, Type componentType, IDictionary<string, object> processedProperties, ICopyTreeConfiguration copyTreeConfiguration)
		{
			Argument.NotNull(processedProperties, nameof(processedProperties));
			Argument.NotNull(entityNode, nameof(entityNode));
			var allProperties = properties;
			if (copyTreeConfiguration != null)
			{
				var componentAllProperties = copyTreeConfiguration.GetAddInfoProperties(componentType);
				if (componentAllProperties != null)
				{
					allProperties = allProperties.Concat(componentAllProperties);
				}
			}

			foreach (PropertyInfo propertyInfo in allProperties)
			{
				if (propertyInfo != null &&
					!processedProperties.ContainsKey(propertyInfo.Name) &&
					IsPropertyWriteable(propertyInfo) &&
					!IsSpecialField(propertyInfo) &&
					!IsOnBusinessObjectIgnoreList(propertyInfo.PropertyType))
				{
					processedProperties.Add(propertyInfo.Name, null);
					if (!IsExcludedField(propertyInfo.PropertyType))
					{
						entityNode.Nodes.Add(new PropertyCopyTemplateNode(propertyInfo, copyTreeConfiguration));
					}
				}
			}
		}

		static bool IsPropertyWriteable(PropertyInfo propertyInfo)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));
			bool writeable = propertyInfo.CanWrite && !propertyInfo.IsDefined(typeof(PrimaryKeyAttribute), false);

			if (writeable)
			{
				ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)propertyInfo.GetCustomAttributes(typeof(ReadOnlyAttribute), false).FirstOrDefault();
				writeable = readOnlyAttribute == null || !readOnlyAttribute.IsReadOnly;
			}

			return writeable;
		}

		public static bool IsSpecialField(PropertyInfo propertyInfo)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));
			string propertyName = propertyInfo.Name;

			if (propertyName.Length > 3 && propertyName[2] == '_')
			{
				return SpecialFieldNames.Contains(propertyName.Substring(3));
			}
			else if (propertyName.Length > 4 && propertyName[3] == '_')
			{
				return SpecialFieldNames.Contains(propertyName.Substring(4));
			}

			return false;
		}

		public static bool IsOnBusinessObjectIgnoreList(Type propertyType)
		{
			Argument.NotNull(propertyType, nameof(propertyType));
			return propertyType.IsDefined(typeof(UniversalCopyIgnoreBusinessObjectAttribute), true);
		}

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		static readonly IReadOnlyList<string> SpecialFieldNames = new[]
		{
			"IsValid",
			"IsActive",
			"IsCancelled",
			"IsSystem",
			"SystemCreateTimeUtc",
			"SystemCreateUser",
			"SystemLastEditTimeUtc",
			"SystemLastEditUser"
		};

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Collection names")]
		static readonly IReadOnlyList<string> SpecialCollectionNames = new[] { "Acknowledgements", "Logs", "JobHeaders", "JobCharges", "RatingHeaderCharges" };

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		static readonly IReadOnlyList<string> ExcludedInterfaces = new[] { "IJobHeader", "IJobCharge" };

		#region GetProperties

		class PropertyInfoNameComparer : IEqualityComparer<PropertyInfo>
		{
			public bool Equals(PropertyInfo x, PropertyInfo y)
			{
				return x.Name == y.Name;
			}

			public int GetHashCode(PropertyInfo obj)
			{
				return obj.Name.GetHashCode();
			}
		}

#if DEBUG
		public
#endif
		static IEnumerable<PropertyInfo> GetPropertiesWithUniqueName(Type componentType)
		{
			return GetProperties(componentType).Distinct(new PropertyInfoNameComparer());
		}

		static IEnumerable<PropertyInfo> GetProperties(Type componentType)
		{
			if (componentType == null)
			{
				yield break;
			}

			foreach (PropertyInfo propertyInfo in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				yield return propertyInfo;
			}

			foreach (PropertyInfo propertyInfo in GetProperties(componentType.BaseType))
			{
				yield return propertyInfo;
			}

			foreach (var interfaceType in componentType.GetInterfaces())
			{
				foreach (PropertyInfo propertyInfo in GetProperties(interfaceType))
				{
					yield return propertyInfo;
				}
			}
		}

		#endregion GetProperties

		static void InitializeBlobProperties(EntityCopyTemplateNode entityNode, Type type, IDictionary<string, object> processedProperties)
		{
			Argument.NotNull(processedProperties, nameof(processedProperties));
			Argument.NotNull(entityNode, nameof(entityNode));
			foreach (ResourceStreamAttribute resourceAttribute in GetCustomAttributes<ResourceStreamAttribute>(type))
			{
				if (!processedProperties.ContainsKey(resourceAttribute.Name))
				{
					var propertyNode = new PropertyCopyTemplateNode { Name = resourceAttribute.Name, PropertyType = resourceAttribute.DefaultContentType };
					entityNode.Nodes.Add(propertyNode);
					processedProperties.Add(resourceAttribute.Name, propertyNode);
				}
			}
		}

		static void InitializeValueOnlyProperties(EntityCopyTemplateNode entityNode, Type componentType, IDictionary<string, object> processedProperties, ICopyTreeConfiguration copyTreeConfiguration)
		{
			Argument.NotNull(processedProperties, nameof(processedProperties));
			Argument.NotNull(entityNode, nameof(entityNode));
			if (copyTreeConfiguration != null)
			{
				var properties = copyTreeConfiguration.GetValueOnlyProperties(componentType);
				if (properties != null)
				{
					foreach (PropertyInfo propertyInfo in properties)
					{
						if (propertyInfo != null && !processedProperties.ContainsKey(propertyInfo.Name))
						{
							var propertyNode = new PropertyCopyTemplateNode(propertyInfo, copyTreeConfiguration);
							entityNode.ValueOnlyNodes.Add(propertyNode);
							processedProperties.Add(propertyInfo.Name, propertyNode);
						}
					}
				}
			}
		}

		static IEnumerable<T> GetCustomAttributes<T>(Type componentType)
			where T : Attribute
		{
			if (componentType == null)
			{
				yield break;
			}

			foreach (T attirbute in componentType.GetCustomAttributes(typeof(T), false))
			{
				yield return attirbute;
			}

			foreach (T attribute in GetCustomAttributes<T>(componentType.BaseType))
			{
				yield return attribute;
			}

			foreach (var interfaceType in componentType.GetInterfaces())
			{
				foreach (T attribute in GetCustomAttributes<T>(interfaceType))
				{
					yield return attribute;
				}
			}
		}

		#endregion

		#region Serialization

		public void Serialize(Stream stream)
		{
			new XmlSerializer(typeof(CopyTemplateTree)).Serialize(new XmlTextWriter(stream, Encoding.Unicode), this);
		}

		public static CopyTemplateTree Deserialize(Stream stream)
		{
			return (CopyTemplateTree)new XmlSerializer(typeof(CopyTemplateTree)).Deserialize(new XmlTextReader(stream));
		}

		#endregion

		#region Compacting

		#region Compact

		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		[SuppressMessage("Microsoft.Contracts", "EnsuresInMethod-Contract.Result<CargoWise.UniversalCopy.CopyTemplateTree>() != null")] // copy is allowed to be null as CloneNode() can return null
		public CopyTemplateTree GetCompactCopy()
		{
			CopyTemplateTree copy = (CopyTemplateTree)TemplateCopyTemplateNode.CloneNode(this, false);
			if (copy != null)
			{
				copy.Compact();
			}
			return copy;
		}

		void Compact()
		{
			CompactEntityNode(this);
		}

		static void CompactEntityNode(CopyTemplateNode node)
		{
			WrappedCopyTemplateNode wrappedNode = node as WrappedCopyTemplateNode;
			if (wrappedNode != null)
			{
				if (wrappedNode.InnerNode != null && !wrappedNode.HasData())
				{
					wrappedNode.InnerNode = null;
				}
				else
				{
					CompactEntityNode(wrappedNode.InnerNode);
				}
			}
			else
			{
				EntityCopyTemplateNode entityNode = node as EntityCopyTemplateNode;
				if (entityNode != null)
				{
					foreach (CopyTemplateNode childNode in entityNode.Nodes.ToArray())
					{
						if (!childNode.HasData())
						{
							entityNode.Nodes.Remove(childNode);
						}
						else
						{
							CompactEntityNode(childNode);
						}
					}
				}
			}
		}

		#endregion

		#region Extend

		public void Extend(CopyTemplateTree fullEmptyTemplate)
		{
			Argument.NotNull(fullEmptyTemplate, nameof(fullEmptyTemplate));
			Argument.NotNull(fullEmptyTemplate.InnerNode, nameof(fullEmptyTemplate.InnerNode));
			if (InnerNode == null)
			{
				InnerNode = fullEmptyTemplate.InnerNode;
			}
			else
			{
				ExtendNode(InnerNode, fullEmptyTemplate.InnerNode, true);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static void ExtendNode(CopyTemplateNode targetNode, CopyTemplateNode sourceNode, bool copyId)
		{
			var targetTemplateNode = targetNode as TemplateCopyTemplateNode;
			var sourceTemplateNode = sourceNode as TemplateCopyTemplateNode;
			if (targetNode != null && sourceNode != null && (targetNode.GetType() == sourceNode.GetType() || targetTemplateNode != null || sourceTemplateNode != null))
			{
				targetNode.Id = copyId ? sourceNode.Id : Guid.NewGuid().ToString();

				if (targetTemplateNode != null && sourceTemplateNode != null)
				{
					targetTemplateNode.TemplateNodeId = sourceTemplateNode.TemplateNodeId;
					if (targetTemplateNode.InnerNode != null && sourceTemplateNode.TemplateNode != null)
					{
						// Do not copy Id - template node will be copied to target tree in other place
						ExtendNode(targetTemplateNode.InnerNode, sourceTemplateNode.TemplateNode, false);
					}
				}
				else if (targetTemplateNode != null)
				{
					// Change Id - value of sourceNode.Id will be copied to targetTemplateNode.InnerNode.Id
					targetNode.Id = Guid.NewGuid().ToString();
					targetTemplateNode.TemplateNodeId = sourceNode.Id;
					if (targetTemplateNode.InnerNode == null)
					{
						if (copyId)
						{
							targetTemplateNode.InnerNode = sourceNode;
						}
						else
						{
							targetTemplateNode.FindTemplateAndInitializeInnerNode(sourceNode);
						}
					}
					else
					{
						// Copy Id - this may be used as template if needed
						ExtendNode(targetTemplateNode.InnerNode, sourceNode, copyId);
					}
				}
				else if (sourceTemplateNode != null)
				{
					if (sourceTemplateNode.TemplateNode != null)
					{
						// Extend from TemplateNode, copy Id - this node may be used as new template if needed
						ExtendNode(targetNode, sourceTemplateNode.TemplateNode, copyId);
					}
				}
				else if (targetNode is WrappedCopyTemplateNode wrappedNode)
				{
					WrappedCopyTemplateNode sourceWrappedNode = (WrappedCopyTemplateNode)sourceNode;
					ExtendNodeWrappedTemplate(wrappedNode, sourceWrappedNode, copyId);
				}
				else if (targetNode is EntityCopyTemplateNode targetEntityNode)
				{
					EntityCopyTemplateNode sourceEntityNode = (EntityCopyTemplateNode)sourceNode;

					foreach (CopyTemplateNode targetChildNode in targetEntityNode.Nodes)
					{
						if (FindTargetNode(sourceEntityNode.Nodes, targetChildNode) == null)
						{
							var propertyNode = targetChildNode as PropertyCopyTemplateNode;
							if (propertyNode != null)
							{
								propertyNode.PropertyType = nameof(Object);
							}

							// Extend duplicates (e.g. split collections, or with updated description) that are missing in empty template
							var sourceChildNode = FindSimilarNode(sourceEntityNode.Nodes, targetChildNode);
							if (sourceChildNode != null)
							{
								ExtendNode(targetChildNode, sourceChildNode, false); // Do not copy id to duplicates from common source
							}
						}
					}

					foreach (var sourceChildNode in sourceEntityNode.Nodes)
					{
						if (sourceChildNode == null)
						{
							continue;
						}

						// Skip RelatedEntityCopyTemplateNode converted to PropertyCopyTemplateNode
						RelatedEntityCopyTemplateNode relatedEntitySourceNode = sourceChildNode as RelatedEntityCopyTemplateNode;
						if (relatedEntitySourceNode != null &&
							targetEntityNode.Nodes.Any(node => node.Name == relatedEntitySourceNode.RelatedPropertyName && node is PropertyCopyTemplateNode))
						{
							continue;
						}

						var targetChildNode = FindTargetNode(targetEntityNode.Nodes, sourceChildNode);
						if (targetChildNode != null)
						{
							ExtendNode(targetChildNode, sourceChildNode, copyId);
						}
						else
						{
							targetEntityNode.Nodes.Add(copyId ? sourceChildNode : TemplateCopyTemplateNode.CloneNode(sourceChildNode));
						}
					}

					foreach (var sourceValueOnlyNode in sourceEntityNode.ValueOnlyNodes)
					{
						if (sourceValueOnlyNode == null)
						{
							continue;
						}

						var targetValueOnlyNode = FindTargetNode(targetEntityNode.ValueOnlyNodes, sourceValueOnlyNode);
						if (targetValueOnlyNode == null)
						{
							targetEntityNode.Nodes.Add(copyId ? sourceValueOnlyNode : TemplateCopyTemplateNode.CloneNode(sourceValueOnlyNode));
						}
					}
				}

				targetNode.CopyTransientData(sourceNode);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "RequiresAtCall-sourceWrappedNode.InnerNode != null")] // sourceWrappedNode.InnerNode has been proven not null on Line 536
		static void ExtendNodeWrappedTemplate(WrappedCopyTemplateNode wrappedNode, WrappedCopyTemplateNode sourceWrappedNode, bool copyId)
		{
			Argument.NotNull(wrappedNode, nameof(wrappedNode));
			Argument.NotNull(sourceWrappedNode, nameof(sourceWrappedNode));

			if (wrappedNode.InnerNode == null)
			{
				wrappedNode.InnerNode = copyId ? sourceWrappedNode.InnerNode : TemplateCopyTemplateNode.CloneNode(sourceWrappedNode.InnerNode);
			}
			else
			{
				ExtendNode(wrappedNode.InnerNode, sourceWrappedNode.InnerNode, copyId);
			}
		}

		static CopyTemplateNode FindTargetNode(IEnumerable<CopyTemplateNode> nodes, CopyTemplateNode sourceNode)
		{
			Argument.NotNull(nodes, nameof(nodes));
			return nodes.FirstOrDefault(node => node.Name == sourceNode.Name && node.Description == sourceNode.Description && node.GetType() == sourceNode.GetType());
		}

		static CopyTemplateNode FindSimilarNode(IEnumerable<CopyTemplateNode> nodes, CopyTemplateNode sourceNode)
		{
			Argument.NotNull(nodes, nameof(nodes));
			return nodes.Where(node => node.Name == sourceNode.Name && node.GetType() == sourceNode.GetType()).OrderBy(node => node.Description).FirstOrDefault(); // Find node with empty description first
		}

		#endregion

		#endregion

		#region Debug Stuff
#if DEBUG

		public static string GetTreeXml(CopyTemplateNode node)
		{
			Argument.NotNull(node, nameof(node));
			StringBuilder sb = new StringBuilder();
			BuildXml(node, sb, 0);
			return sb.ToString();
		}

		static void BuildXml(CopyTemplateNode node, StringBuilder sb, int spaces)
		{
			Argument.NotNull(sb, nameof(sb));
			Argument.NotNull(node, nameof(node));
			sb.Append(' ', spaces).Append("<").Append(node.GetType().Name).Append(" name='").Append(node.Name).Append("' id='").Append(node.Id).Append("'");

			var templateNode = node as TemplateCopyTemplateNode;
			if (templateNode != null)
			{
				sb.Append(" templateId='").Append(templateNode.TemplateNodeId).Append("'");
			}

			sb.Append(">");

			bool hasChildren = false;

			var wrappedNode = node as WrappedCopyTemplateNode;
			if (wrappedNode != null && wrappedNode.InnerNode != null)
			{
				sb.AppendLine();
				BuildXml(wrappedNode.InnerNode, sb, spaces + 2);
				hasChildren = true;
			}

			var entityNode = node as EntityCopyTemplateNode;
			if (entityNode != null && entityNode.Nodes.Count > 0)
			{
				sb.AppendLine();
				foreach (var childNode in entityNode.Nodes)
				{
					if (childNode != null)
					{
						BuildXml(childNode, sb, spaces + 2);
					}
				}
				hasChildren = true;
			}

			if (hasChildren)
			{
				sb.Append(' ', spaces);
			}
			sb.Append("</").Append(node.GetType().Name).AppendLine(">");
		}

#endif
		#endregion
	}
}
