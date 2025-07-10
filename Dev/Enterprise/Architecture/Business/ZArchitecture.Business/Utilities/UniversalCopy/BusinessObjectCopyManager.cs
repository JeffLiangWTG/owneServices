using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Utilities.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	public class BusinessObjectCopyManager : ReflectionCopyManagerBase
	{
		public BusinessObjectCopyManager(BusinessObjectFactory defaultFactory = null)
		{
			DefaultFactory = defaultFactory;
			mappinginfos = new Dictionary<object, string[]>();
			universalCopyCustomRelatedEntityManager = new UniversalCopyCustomRelatedEntityManager();
			universalCopyCustomFinishCopyActionManager = new UniversalCopyCustomFinishCopyActionManager();
		}

		#region Copy

		protected BusinessObjectFactory DefaultFactory { get; private set; }
		IDisposable listChangedSuspender;
		bool isCopyFromTemplate;
		readonly Dictionary<object, string[]> mappinginfos;
		readonly UniversalCopyCustomRelatedEntityManager universalCopyCustomRelatedEntityManager;
		readonly UniversalCopyCustomFinishCopyActionManager universalCopyCustomFinishCopyActionManager;

		BusinessObjectFactory GetFactoryForNewEntity(object sourceEntity)
		{
			var sourceBizo = sourceEntity as BusinessObject;
			var factory = sourceBizo != null && !isCopyFromTemplate ? sourceBizo.Factory : DefaultFactory;
			return factory;
		}

		protected override void PrepareForCopy(object source, CopyTemplateTree copyTemplate)
		{
			base.PrepareForCopy(source, copyTemplate);

			isDeletedDataDetected = false;
			mappinginfos.Clear();

			var templateRecordProvider = source as ITemplateRecordProvider;
			isCopyFromTemplate = templateRecordProvider != null && templateRecordProvider.IsTemplateRecord;

			if (DefaultFactory == null)
			{
				var bizo = source as BusinessObject;
				var factory = bizo?.Factory;
				if (isCopyFromTemplate)
				{
					// Use different factory from template record to prevent template records to be saved to business tables.
					factory = factory?.CreateNewFactory();
					if (factory == null || factory == bizo.Factory)
					{
						factory = new BusinessObjectFactory();
					}
				}
				DefaultFactory = factory;
			}

			var workFactory = GetFactoryForNewEntity(source);
			if (workFactory != null)
			{
				listChangedSuspender = ActiveBusinessObjectCollection.DelayListChangedEvents(workFactory);

				if (BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(workFactory))
				{
					ErrorReporter.ReportOnce("BizoCopyManager_PrepareForCopy_ExistingService", "There is other instance of BusinessObjectUniversalCopyFactoryService in BusinessObjectFactory");
				}
				else
				{
					workFactory.ServiceContainer.AddService(new BusinessObjectUniversalCopyFactoryService());
				}
			}
		}

		protected override void FinishCopy(object source, object copy, CopyTemplateTree copyTemplate)
		{
			universalCopyCustomFinishCopyActionManager.PerformUniversalCopyCustomFinishCopyActions(copiedEntities);

			CopyMappingValues();

			if (listChangedSuspender != null)
			{
				listChangedSuspender.Dispose();
			}

			var workFactory = GetFactoryForNewEntity(source);
			if (workFactory != null)
			{
				workFactory.InvalidateCachedProperties();

				var factoryCopyService = BusinessObjectUniversalCopyFactoryService.GetExistingService(workFactory);
				if (factoryCopyService != null)
				{
					try
					{
						factoryCopyService.OnCopyFinished();
					}
					finally
					{
						workFactory.ServiceContainer.RemoveService<BusinessObjectUniversalCopyFactoryService>();
					}
				}
			}

			ReloadAllCollections(copy as IBusiness);

			var bizo = copy as BusinessObject;
			if (bizo != null)
			{
				bizo.HasChanges = true;

				var logParent = bizo as IStmALogParent;
				var sourceBizo = source as BusinessObject;

				if (logParent != null && sourceBizo != null)
				{
					logParent.Logs.AddNew
					(
						AutoEvents.UniversalCopy,
						string.Join(Separator, sourceBizo.HumanReadableName, copyTemplate?.ConfigurationName ?? string.Empty),
						ZDateTimeOffset.Now
					);
				}
			}

			DefaultFactory = null;

			if (isDeletedDataDetected && Globals.IsUserInteractive)
			{
				Globals.Message.ShowWarning(Res.GetString("d00d5538-7216-4943-9551-1af78027635a",
					"Whilst you were working another user has deleted some information you are attempting to copy and copy result can be incomplete. Please cancel this copy, refresh information you are copying, and retry the copy action."));
			}

			base.FinishCopy(source, copy, copyTemplate);
		}

		const string Separator = "|";

		void CopyMappingValues()
		{
			foreach (KeyValuePair<object, string[]> kv in mappinginfos)
			{
				var sourceEntity = kv.Key as BusinessObject;
				var propertyNames = kv.Value;

				var targetEntity = sourceEntity != null && !sourceEntity.IsDeleted ? GetTargetEntity(sourceEntity) as BusinessObject : null;

				if (targetEntity != null && !targetEntity.IsDeleted)
				{
					foreach (var propertyName in propertyNames)
					{
						var sourceKeyValue = GetPropertyValue(sourceEntity, propertyName);
						var mappingKeyValue = GetTargetEntityPK(sourceKeyValue);

						if (mappingKeyValue != null)
						{
							SetPropertyValue(targetEntity, propertyName, mappingKeyValue);
						}
					}
				}
			}

			mappinginfos.Clear();
		}

		void ReloadAllCollections(IBusiness item)
		{
			if (item != null && item.Children != null)
			{
				var bizo = item as BusinessObject;
				if (bizo != null)
				{
					// Prevent cycling
					if (bizo.IsTopLevel)
					{
						return;
					}
					bizo.IsTopLevel = true;
				}

				foreach (var child in item.Children)
				{
					BusinessObjectCollection collection = child as BusinessObjectCollection;
					if (collection != null && collection.IsLoaded && collection.Count == 0)
					{
						collection.Load();
					}

					ReloadAllCollections(child);
				}

				if (bizo != null)
				{
					bizo.IsTopLevel = false;
				}
			}
		}

		#endregion

		#region Entity

		protected override object GetRelatedEntityFromDb(object sourceEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode)
		{
			if (relatedEntityCopyTemplateNode.Name == DocDataEntityName)
			{
				return GetDocDataRelatedEntity(sourceEntity);
			}

			BusinessObject sourceBizo = sourceEntity as BusinessObject;
			BusinessObjectFactory factory = sourceBizo != null ? sourceBizo.Factory : DefaultFactory;
			ITableSchema tableSchema = EnterpriseSchema.GetTableSchema(relatedEntityCopyTemplateNode.RelatedEntityTableName);
			if (factory != null && tableSchema != null)
			{
				object relatedEntityId = GetPropertyValue(sourceEntity, relatedEntityCopyTemplateNode.RelatedPropertyName);
				if (relatedEntityId != null && (relatedEntityId is ZGuid || relatedEntityId is Guid))
				{
					ZGuid fk = relatedEntityId is ZGuid ? (ZGuid)relatedEntityId : new ZGuid(relatedEntityId);
					if (fk.IsValid)
					{
						var universalCopyCustomRelatedEntity = universalCopyCustomRelatedEntityManager.GetUniversalCopyCustomRelatedEntity(
							relatedEntityCopyTemplateNode.RelatedPropertyName);
						if (universalCopyCustomRelatedEntity is not null)
						{
							var customFk = universalCopyCustomRelatedEntity.GetRelatedEntityObjectFK(
								factory, relatedEntityCopyTemplateNode, fk);

							if (customFk is not null)
							{
								fk = (ZGuid)customFk;
							}
							else
							{
								return null;
							} 
						}

						Type bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(relatedEntityCopyTemplateNode.RelatedEntityTableName), false);
						if (bizoType != null)
						{
							return factory.Load(bizoType, fk);
						}

						return ((IBusinessObjectFactoryInternals)factory).RowFactory.LoadFromPK(tableSchema.TableName, fk);
					}
				}
			}

			return null;
		}

		protected override object CreateNewEntityFromCore(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, string propertyName)
		{
			if (copyTemplateNode != null)
			{
				if (copyTemplateNode.Name == DocDataEntityName)
				{
					return CreateNewDocDataEntity(parentEntity, sourceEntity as StmNote);
				}
			}

			var propertyInfo = parentEntity != null && !string.IsNullOrEmpty(propertyName) ? GetPropertyInfo(parentEntity, propertyName) : null;
			var copyRelatedEntityAttribute = propertyInfo != null
				? propertyInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).Cast<UniversalCopyRelatedEntityAttribute>().FirstOrDefault()
				: null;
			if (copyRelatedEntityAttribute != null)
			{
				var result = GetPropertyValue(parentEntity, propertyName);
				if (result != null)
				{
					if (!string.IsNullOrEmpty(copyRelatedEntityAttribute.MakeRelatedEntitySavedByFactoryMethod))
					{
						try
						{
							result.GetType().InvokeMember(copyRelatedEntityAttribute.MakeRelatedEntitySavedByFactoryMethod,
							BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, result, null);
						}
						catch (MissingMethodException) { }
						catch (AmbiguousMatchException) { }
					}
					return result;
				}

				if (!string.IsNullOrEmpty(copyRelatedEntityAttribute.CreationMethodName))
				{
					try
					{
						propertyInfo.DeclaringType.InvokeMember(copyRelatedEntityAttribute.CreationMethodName,
							BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parentEntity, null);

						result = GetPropertyValue(parentEntity, propertyName);
						if (result != null)
						{
							return result;
						}
					}
					catch (MissingMethodException) { }
					catch (AmbiguousMatchException) { }
				}
			}

			var sourceBizo = sourceEntity as BusinessObject;
			var factory = GetFactoryForNewEntity(sourceEntity);
			var creationMethod = sourceEntity.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.CreationMethod;

			if (!string.IsNullOrEmpty(creationMethod))
			{
				return sourceEntity.GetType().InvokeMember(creationMethod,
					BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
					sourceEntity, new[] { factory, parentEntity });
			}

			if (sourceBizo != null && factory != null)
			{
				var newBizo = factory.New(sourceBizo.GetType());
				newBizo.HasChanges = true;
				return newBizo;
			}

			return null;
		}

		protected override object GetEntityPKCore(object entity)
		{
			return ((BusinessObject)entity).PK;
		}

		protected override string GetEntityCodeCore(object entity)
		{
			return CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)entity);
		}

		protected override void SetEntityRelationship(object targetEntity, object relatedEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode)
		{
			var universalCopyCustomRelatedEntity = universalCopyCustomRelatedEntityManager.GetUniversalCopyCustomRelatedEntity(
							relatedEntityCopyTemplateNode.RelatedPropertyName);
			if (universalCopyCustomRelatedEntity is not null)
			{
				var customRelatedEntity = universalCopyCustomRelatedEntity.GetOriginalRelatedEntityFromRelatedEntityObject(
					relatedEntityCopyTemplateNode, relatedEntity, GetPropertyValue);
				base.SetEntityRelationship(targetEntity, customRelatedEntity, relatedEntityCopyTemplateNode);
			}
			else if (relatedEntityCopyTemplateNode.Name == DocDataEntityName)
			{
				SetDocDataRelationship(targetEntity, relatedEntity as StmNote);
			}
			else
			{
				base.SetEntityRelationship(targetEntity, relatedEntity, relatedEntityCopyTemplateNode);
			}
		}

		protected override void PreCopyEntity(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode)
		{
			RunCopyMethod(targetEntity, a => a.StartCopyMethod);
			CopyRequiredProperties(sourceEntity, targetEntity, UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning);
			EnsureElementsOrder(sourceEntity, copyTemplateNode);

			base.PreCopyEntity(sourceEntity, targetEntity, copyTemplateNode);
		}

		protected override CopyTemplateNode GetRelatedCopyTemplateNode(object sourceEntity, string relatedCopyTemplateNode, CopyTemplateNode copyTemplateNode)
		{
			CopyTemplateNode result = null;
			if (!string.IsNullOrEmpty(relatedCopyTemplateNode))
			{
				try
				{
					result = sourceEntity.GetType().InvokeMember(relatedCopyTemplateNode,
						BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, sourceEntity, null) as CopyTemplateNode;
				}
				catch (MissingMethodException) { }
				catch (AmbiguousMatchException) { }
			}
			return result ?? copyTemplateNode;
		}

		protected override void PostCopyEntity(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode)
		{
			base.PostCopyEntity(sourceEntity, targetEntity, copyTemplateNode);
			CopyRequiredProperties(sourceEntity, targetEntity, UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheEnd);
			RunCopyMethod(targetEntity, a => a.FinishCopyMethod);

			var mappingPropertyAttributes = targetEntity.GetType().GetCustomAttributes<UniversalCopyMappingKeysAttribute>(true);
			if (mappingPropertyAttributes?.Any() ?? false)
			{
				var properties = mappingPropertyAttributes
					.Where(c => !string.IsNullOrWhiteSpace(c.RelatedKeyPropertyName))
					.Select(c => c.RelatedKeyPropertyName)
					.Distinct()
					.ToArray();

				mappinginfos[sourceEntity] = properties;
			}

			var targetBizo = targetEntity as IBusiness;
			if (targetBizo != null)
			{
				foreach (var childCollection in targetBizo.Children.OfType<BusinessObjectCollection>())
				{
					if (childCollection.Count == 0)
					{
						var subCollection = childCollection as ISubsetBusinessObjectCollection;
						if (subCollection != null)
						{
							subCollection.Rebuild();
						}
						else if (!childCollection.IsNonPersistent)
						{
							childCollection.Load();
						}
					}
				}
			}

			UpdateOrgPKForZAddressProperties(targetEntity, copyTemplateNode);
		}

		void RunCopyMethod(object targetEntity, Func<UniversalCopyWithExtendedEntitiesAttribute, string> getCopyMethod)
		{
			var type = targetEntity.GetType();

			var extendedEntitiesAttribute = type.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Cast<UniversalCopyWithExtendedEntitiesAttribute>().FirstOrDefault();
			if (extendedEntitiesAttribute != null)
			{
				var copyMethod = getCopyMethod(extendedEntitiesAttribute);
				if (!string.IsNullOrEmpty(copyMethod))
				{
					try
					{
						type.InvokeMember(copyMethod, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, targetEntity, null);
					}
					catch (MissingMethodException) { }
					catch (AmbiguousMatchException) { }
				}
			}
		}

		internal void CopyRequiredProperties(object sourceEntity, object targetEntity, UniversalCopyAlwaysCopyPropertyAttribute.CopyMode mode)
		{
			var componentType = sourceEntity.GetType();
			if (componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length > 0)
			{
				foreach (var propertyInfo in BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType))
				{
					if (propertyInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true).Cast<UniversalCopyAlwaysCopyPropertyAttribute>()
							.Any(attribute => attribute.Mode == mode))
					{
						SetDataPropertyValue(targetEntity, propertyInfo.Name, propertyInfo.GetValue(sourceEntity, null));
					}
				}
			}
		}

		void UpdateOrgPKForZAddressProperties(object targetEntity, CopyTemplateNode copyTemplateNode)
		{
			if (copyTemplateNode is EntityCopyTemplateNode entityCopyTemplateNode && targetEntity is BusinessObject targetBizo)
			{
				var copiedFromSouceNodes = entityCopyTemplateNode.Nodes.OfType<PropertyCopyTemplateNode>()
					.Where(p => p.CopyMethod != CopyMethod.None && (p.PropertyType == nameof(Guid) || p.PropertyType == nameof(ZGuid)))
					.Select(p => p.Name)
					.ToHashSet();

				if (copiedFromSouceNodes.Count > 0)
				{
					foreach (PropertyDescriptor propertyInfo in ZCustomTypeDescriptor.GetProperties(targetBizo.GetType()))
					{
						if (propertyInfo.PropertyType == typeof(ZAddress)
							&& propertyInfo.GetValue(targetBizo) is ZAddress zAddress
							&& zAddress.OrgPKInitialized
							&& copiedFromSouceNodes.Contains(zAddress.AddressFKInfo.InnerInfo.Name)
							&& ((INeedRow)targetBizo).Row.Table.Columns.Contains(zAddress.AddressFKInfo.InnerInfo.Name))
						{
							var addressFK = new ZGuid(((INeedRow)targetBizo).Row[zAddress.AddressFKInfo.InnerInfo.Name]);
							if (addressFK.IsValid)
							{
								var orgAddress = targetBizo.Factory.Load<IOrgAddress>(addressFK);
								if (orgAddress != null)
								{
									zAddress.SetOrgPKForUniversalCopy(orgAddress.OrganisationPK);
								}
							}
						}
					}
				}
			}
		}

		protected override void EnsureElementsOrder(object sourceEntity, CopyTemplateNode copyTemplateNode)
		{
			var componentType = sourceEntity.GetType();
			if (copyTemplateNode is EntityCopyTemplateNode entityCopyTemplateNode)
			{
				var prerequisites = componentType.GetCustomAttributes(typeof(UniversalCopyElementsOrderAttribute), true)
					.Cast<UniversalCopyElementsOrderAttribute>()
					.Select(a => (a.FirstElement, a.SecondElement)).ToArray();
				if (prerequisites.Any())
				{
					var sortedNodes = TopologicalSort(prerequisites);

					var orderNotes = entityCopyTemplateNode.Nodes.OrderBy(e =>
					{
						var index = sortedNodes.IndexOf(e.Name);
						return index != -1 ? index : int.MaxValue;
					}).ToArray();
					entityCopyTemplateNode.Nodes.Clear();
					entityCopyTemplateNode.Nodes.AddRange(orderNotes);
				}
			}
		}

#if DEBUG
		internal
#endif
		List<string> TopologicalSort(IEnumerable<(string, string)> prerequisites)
		{
			var res = new List<string>();
			var allItems = prerequisites.Select(p => p.Item2).Union(prerequisites.Select(p => p.Item1));
			var inDegreeMap = allItems.ToDictionary(item => item, iitem => 0);
			foreach (var prerequisite in prerequisites)
			{
				inDegreeMap[prerequisite.Item2]++;
			}

			var queue = new Queue<string>();
			foreach (var kv in inDegreeMap)
			{
				if (kv.Value == 0)
				{
					queue.Enqueue(kv.Key);
				}
			}

			var i = 0;
			while (queue.Count > 0)
			{
				var curr = queue.Dequeue();
				res.Add(curr);
				i++;

				foreach (var pre in prerequisites)
				{
					if (pre.Item1 == curr)
					{
						inDegreeMap[pre.Item2]--;
						var indegree = inDegreeMap[pre.Item2];

						if (indegree == 0)
						{
							queue.Enqueue(pre.Item2);
						}
					}
				}
			}
			return res;
		}

		protected override void EnsureUniversalCopyMappingKeys(object sourceEntity, object newEntity, Dictionary<object, object> keyMappings)
		{
			if (sourceEntity.GetType().GetCustomAttributes(typeof(UniversalCopyMappingKeysAttribute), true).Any())
			{
				keyMappings[GetEntityPK(sourceEntity)] = GetEntityPK(newEntity);
			}
		}

		#endregion

		#region Property

		protected override void CopyProperty(object target, object source, PropertyCopyTemplateNode propertyCopyTemplateNode)
		{
			if (propertyCopyTemplateNode.CopyMethod != CopyMethod.None)
			{
				var sourceBizo = source as BusinessObject;
				var targetBizo = target as BusinessObject;

				if (
#if DEBUG
!AlwaysCopyViaPropertyForTest &&
#endif
 sourceBizo != null && targetBizo != null &&
					((IBusinessObjectInternals)sourceBizo).Row != null && ((IBusinessObjectInternals)sourceBizo).Row.Table != null &&
					((IBusinessObjectInternals)sourceBizo).Row.Table.Columns.Contains(propertyCopyTemplateNode.Name))
				{
					source = ((IBusinessObjectInternals)sourceBizo).Row;
					target = ((IBusinessObjectInternals)targetBizo).Row;
				}
			}

			base.CopyProperty(target, source, propertyCopyTemplateNode);
		}

#if DEBUG
		internal bool AlwaysCopyViaPropertyForTest { get; set; }
#endif

		protected override void SetDataPropertyValue(object target, string propertyName, object value)
		{
			BusinessObject bizo = target as BusinessObject;

			if (
#if DEBUG
!AlwaysCopyViaPropertyForTest &&
#endif
 bizo != null &&
				((IBusinessObjectInternals)bizo).Row != null && ((IBusinessObjectInternals)bizo).Row.Table != null &&
				((IBusinessObjectInternals)bizo).Row.Table.Columns.Contains(propertyName))
			{
				target = ((IBusinessObjectInternals)bizo).Row;
			}

			base.SetDataPropertyValue(target, propertyName, value);
		}

		protected override void SetPropertyValueCore(object target, string propertyName, object value)
		{
			BusinessObject bizo = target as BusinessObject;

			ZPropertyInfo propertyInfo = bizo != null ? bizo.FindPropertyInfo(propertyName) : null;
			if (value != null && bizo != null && propertyInfo != null && propertyInfo.MaxLength > 0 && propertyInfo.MaxLength < int.MaxValue)
			{
				string stringValue = value as string;
				if (stringValue != null && stringValue.Length > propertyInfo.MaxLength)
				{
					value = stringValue.Substring(0, propertyInfo.MaxLength);
				}

				ZString zStringValue = stringValue == null && value is ZString ? (ZString)value : ZString.Empty;
				if (!zStringValue.IsEmpty && zStringValue.Length > propertyInfo.MaxLength)
				{
					value = zStringValue.Substring(0, propertyInfo.MaxLength);
				}
			}

			var objectInternals = target as IBusinessObjectInternals;

			var disposeAction = objectInternals != null
				? new DisposableAction(() => objectInternals.IsCopying = true, () => objectInternals.IsCopying = false)
				: DisposableAction.NoAction;

			using (disposeAction)
			{
				base.SetPropertyValueCore(target, propertyName, value);
			}
		}

		protected override string ProcessMacrosCore(string value, IEnumerable<object> rootEntities)
		{
			return MacroProcessor != null && !string.IsNullOrEmpty(value)
				? MacroProcessor.Replace(value, rootEntities.Where(r => r is BusinessObject).Cast<BusinessObject>().ToArray())
				: value;
		}

		ITextMacroProcessor MacroProcessor
		{
			get { return macroProcessor ?? (macroProcessor = ObjectFactory.Get<ITextMacroProcessor>()); }
		}
		ITextMacroProcessor macroProcessor;

		protected override void EnsureBlobField(DataRow row, string columnName)
		{
			if (DefaultFactory != null && row != null && row.Table != null && LazyLoading.LoadRequired(row[columnName]))
			{
				var tableSchema = EnterpriseSchema.GetTableSchema(row.Table.TableName);
				if (tableSchema != null)
				{
					var schemaColumn = tableSchema.GetSchemaColumn(columnName);
					if (schemaColumn != null)
					{
						try
						{
							((IBusinessObjectFactoryInternals)DefaultFactory).RowFactory.LoadBlobField(row, schemaColumn);
						}
						catch (SqlStreamReaderRowNotFoundException)
						{
							isDeletedDataDetected = true;
						}
					}
				}
			}
		}

		bool isDeletedDataDetected;

		#endregion

		#region Collection

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		protected override IEnumerable GetCollectionCore(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			IEnumerable collection = base.GetCollectionCore(sourceEntity, collectionCopyTemplateNode)
				?? GetWorkflowCollection(sourceEntity as IWorkflowProviderCore, collectionCopyTemplateNode)
				?? GetSpecialCollection(sourceEntity, collectionCopyTemplateNode, JobServiceSchema.Constants.TableName, "Services", typeof(IJobService))
				?? GetSpecialCollection(sourceEntity, collectionCopyTemplateNode, JobDocAddressSchema.Constants.TableName, "DocAddresses", typeof(IJobDocAddress))
				?? GetSpecialCollection(sourceEntity, collectionCopyTemplateNode, OrgAddressSchema.Constants.TableName, "Addresses", typeof(IOrgAddress));

			ZLogsOrNotes logsOrNotes = collection as ZLogsOrNotes;
			if (logsOrNotes != null)
			{
				collection = logsOrNotes.ElementsInternal;
			}

			return collection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Collection name")]
		IEnumerable GetWorkflowCollection(IWorkflowProviderCore workflowProvider, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			if (workflowProvider != null)
			{
				switch (collectionCopyTemplateNode.Name)
				{
					case "WorkflowMilestones":
						return GetWorkflowSubCollection(workflowProvider, "Milestones");
					case "WorkflowExceptions":
						return GetWorkflowSubCollection(workflowProvider, "Exceptions");
				}
			}

			return null;
		}

		IEnumerable GetWorkflowSubCollection(IWorkflowProviderCore workflowProvider, string subCollectionName)
		{
			var workflowItems = GetPropertyValue(workflowProvider, "WorkflowItems");
			if (workflowItems != null)
			{
				return GetPropertyValue(workflowItems, subCollectionName) as IEnumerable;
			}
			return null;
		}

		IEnumerable GetSpecialCollection(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode, string tableName, string collectionName, Type expectedElementType)
		{
			try
			{
				if (string.Equals(collectionCopyTemplateNode.ItemsTableName, tableName, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(collectionCopyTemplateNode.Name, collectionName, StringComparison.Ordinal))
				{
					var specialCollectionNode = new CollectionCopyTemplateNode
					{
						Name = collectionName,
						ItemPropertyName = collectionCopyTemplateNode.ItemPropertyName,
						ItemParentTablePropertyName = collectionCopyTemplateNode.ItemParentTablePropertyName,
						ItemsTableName = collectionCopyTemplateNode.ItemsTableName,
					};

					var collection = base.GetCollectionCore(sourceEntity, specialCollectionNode);
					if (collection != null)
					{
						var elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(collection.GetType());
						if (elementType != null && !expectedElementType.IsAssignableFrom(elementType))
						{
							collection = null;
						}
					}

					return collection;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("BusinessObjectCopyManager.GetSpecialCollection_Exception", $"Universal copy raised an exception on a {collectionName} with tableName {tableName}. The corresponding" +
					$" property may not be suitable for universal copying, and would thus need to be added to the UniversalCopyIgnoreElement property in the relevant business object\r\n" +
					$"collectionCopyTemplateNode is null: {collectionCopyTemplateNode == null}\r\n" +
					$"collectionCopyTemplateNode?.Name: {collectionCopyTemplateNode?.Name}\r\n" +
					$"collectionCopyTemplateNode?.ItemsTableName: {collectionCopyTemplateNode?.ItemsTableName}\r\n" +
					$"collectionCopyTemplateNode?.ItemPropertyName: {collectionCopyTemplateNode?.ItemPropertyName}\r\n" +
					$"collectionCopyTemplateNode?.ItemParentTablePropertyName: {collectionCopyTemplateNode?.ItemParentTablePropertyName}\r\n" +
					$"sourceEntity: {sourceEntity}\r\n" +
					$"expectedElementType: {expectedElementType}", ex);
			}

			return null;
		}

		protected override IEnumerable GetCollectionFromDb(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			var sourceBizo = sourceEntity as BusinessObject;
			var factory = sourceBizo != null ? sourceBizo.Factory : DefaultFactory;
			var tableSchema = EnterpriseSchema.GetTableSchema(collectionCopyTemplateNode.ItemsTableName);
			if (factory != null && tableSchema != null)
			{
				var itemColumn = tableSchema.GetSchemaColumn(collectionCopyTemplateNode.ItemPropertyName);
				if (itemColumn != null)
				{
					var query = new ZQuery(itemColumn, GetEntityPK(sourceEntity));

					if (!string.IsNullOrEmpty(collectionCopyTemplateNode.ItemParentTablePropertyName))
					{
						var tableColumn = tableSchema.GetSchemaColumn(collectionCopyTemplateNode.ItemParentTablePropertyName);
						if (tableColumn != null)
						{
							var parentTableCode = GetEntityTableCodeForCollectionItem(sourceEntity, collectionCopyTemplateNode);
							if (!string.IsNullOrEmpty(parentTableCode))
							{
								query.AddToFilter(tableColumn, parentTableCode);
							}
						}
					}

					var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(collectionCopyTemplateNode.ItemsTableName), false);
					if (bizoType != null)
					{
						if (bizoType == typeof(StmNote) && sourceBizo != null)
						{
							query.AddToFilter(StmNoteSchema.ST_Table, sourceBizo.TableName);
						}

						if (CopyTemplateTree.IsOnBusinessObjectIgnoreList(bizoType))
						{
							return null;
						}

						if (collectionCopyTemplateNode.ItemsTableName == CusEntryNumSchema.Constants.TableName)
						{
							var notHIROrCSRFilter = new ZQuery(CusEntryNumSchema.CE_Category, SQLComparisonOperator.NotEqual, otherCategory);
							notHIROrCSRFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, new[] { hirEntryType, csrEntryType });
							query.AddToFilter(notHIROrCSRFilter);
						}

						return factory.Load(bizoType, query);
					}

					return ((IBusinessObjectFactoryInternals)factory).RowFactory.Load(tableSchema.TableName, query);
				}
			}

			return null;
		}

		#region CusEntryNumbers

		const string otherCategory = "OTH";
		const string hirEntryType = "HIR";
		const string csrEntryType = "CSR";

		#endregion

		protected override void PrepareTargetCollection(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			base.PrepareTargetCollection(targetEntity, collectionCopyTemplateNode);

			ClearCollectionFromAutoCreatedItems(targetEntity, collectionCopyTemplateNode, false);
		}

		protected override void FinishTargetCollection(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			base.FinishTargetCollection(targetEntity, collectionCopyTemplateNode);

			ClearCollectionFromAutoCreatedItems(targetEntity, collectionCopyTemplateNode, true);
		}

		void ClearCollectionFromAutoCreatedItems(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode, bool afterCopy)
		{
			var collection = GetCollectionCore(targetEntity, collectionCopyTemplateNode) as IBusinessObjectCollection;
			if (collection != null)
			{
				var clearAttribute = (UniversalCopyClearCollectionOnCopyAttribute)collection.GetType().GetCustomAttributes(typeof(UniversalCopyClearCollectionOnCopyAttribute), true).FirstOrDefault();
				if (clearAttribute != null && clearAttribute.ClearAfterAllElementsWereCopied == afterCopy)
				{
					foreach (var item in collection.ToArray())
					{
						if (!item.IsInDatabase && !WasCreatedByMe(item))
						{
							item.Delete();
						}
					}
				}
			}
		}

		protected override void SetCollectionRelationship(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			var businessObjectCollection = GetCollectionCore(targetEntity, collectionCopyTemplateNode) as IBusinessObjectCollection;
			if (businessObjectCollection != null && collectionItem is BusinessObject)
			{
				businessObjectCollection.Add(collectionItem); // Set extended relationship

				var fkValue = GetPropertyValue(collectionItem, collectionCopyTemplateNode.ItemPropertyName);
				var currentFK = fkValue != null && fkValue != DBNull.Value && (fkValue is Guid || fkValue is ZGuid) ? new ZGuid(fkValue) : ZGuid.Empty;

				bool hasParentTableField = !string.IsNullOrEmpty(collectionCopyTemplateNode.ItemParentTablePropertyName);
				var currentParentTable = string.Empty;
				if (hasParentTableField)
				{
					object parentTableValue = GetPropertyValue(collectionItem, collectionCopyTemplateNode.ItemParentTablePropertyName);
					if (parentTableValue != null && parentTableValue != DBNull.Value)
					{
						currentParentTable = parentTableValue.ToString();
					}
				}

				if (!currentFK.IsEmpty && currentFK.IsValid && (!hasParentTableField || !string.IsNullOrEmpty(currentParentTable)))
				{
					return;
				}
			}

			base.SetCollectionRelationship(targetEntity, collectionItem, collectionCopyTemplateNode);
		}

		protected override void SetCollectionItemParentTableProperty(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			string parentTableCode = GetEntityTableCodeForCollectionItem(targetEntity, collectionCopyTemplateNode);
			if (!string.IsNullOrEmpty(parentTableCode))
			{
				SetDataPropertyValue(collectionItem, collectionCopyTemplateNode.ItemParentTablePropertyName, parentTableCode);
			}
		}

		string GetEntityTableCodeForCollectionItem(object entity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			string parentTableCode = null;

			if (!string.IsNullOrEmpty(collectionCopyTemplateNode.ItemParentTablePropertyName))
			{
				var itemTableSchema = EnterpriseSchema.GetTableSchema(collectionCopyTemplateNode.ItemsTableName);
				if (itemTableSchema != null)
				{
					var column = itemTableSchema.GetSchemaColumn(collectionCopyTemplateNode.ItemParentTablePropertyName);
					if (column != null)
					{
						var targetBizo = entity as BusinessObject;
						if (targetBizo != null)
						{
							parentTableCode = targetBizo.TableName;
						}
						if (string.IsNullOrEmpty(parentTableCode))
						{
							var targetRow = entity as DataRow;
							if (targetRow != null && targetRow.Table != null)
							{
								parentTableCode = targetRow.Table.TableName;
							}
						}

						if (!string.IsNullOrEmpty(parentTableCode))
						{
							if (column.MaxLength == 2 || column.MaxLength == 3) // 3 is for some pivots tables
							{
								parentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parentTableCode);
							}
						}
					}
				}
			}

			return parentTableCode;
		}

		#endregion

		#region Filters

		protected override IEnumerable FilterCollectionCore(IEnumerable collection, CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable<string> path)
		{
			ZQuery query = null;
			if (collectionCopyTemplateNode != null && collectionCopyTemplateNode.Filter != null && collectionCopyTemplateNode.Filter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter)
			{
				query = new ZQuery();
				query.AddFilterAndZSQLParameterCollection(collectionCopyTemplateNode.Filter.FilterData, null);
			}

			if (query == null && GetFilterBusinessObjectMethod != null)
			{
				FilterBusinessObject filterBizo = GetFilterBusinessObjectMethod(collectionCopyTemplateNode, path);
				if (filterBizo != null)
				{
					query = filterBizo.Filter;
				}
			}

			if (query != null)
			{
				query.IgnoreActiveFilter = true;

				foreach (object o in collection)
				{
					BusinessObject bizo;
					DataRow row;

					if ((bizo = o as BusinessObject) != null && bizo.MatchesFilter(query))
					{
						yield return bizo;
					}
					else if ((row = o as DataRow) != null && new RowFilterComparer(row.Table, query).IsMatch(row))
					{
						yield return row;
					}
				}
			}
			else
			{
				foreach (object o in collection)
				{
					yield return o;
				}
			}
		}

		public delegate FilterBusinessObject GetFilterBusinessObjectDelegate(CopyTemplateNode copyTemplateNode, IEnumerable<string> path);
		public GetFilterBusinessObjectDelegate GetFilterBusinessObjectMethod;

		#endregion

		#region Extended properties

		#region ICopyTreeConfiguration

		public static ICopyTreeConfiguration CopyTreeConfiguration
		{
			get { return new CopyTreeConfigurationImplementation(); }
		}

		internal class CopyTreeConfigurationImplementation : ICopyTreeConfiguration
		{
			public void PreInitializeEntity(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType)
			{
				if (componentType != null)
				{
					PreInitializeEntityNode(GetTypeToProcess(componentType), TypeSubstitutions);
				}
			}

			public void AdditionalEntityInitialization(EntityCopyTemplateNode copyTemplateNode, Type type, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preprocessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
			{
				AddExtendedPropertiesToEntityNode(copyTemplateNode, type, GetTypeToProcess(componentType), preprocessedTypes, exceptionThrownHandle);
			}

			public string GetAssociateCollectionPropertyName(Type componentType, string collectionPropertyName)
			{
				return componentType
					?.GetCustomAttributes<UniversalCopyAssociateElementAttribute>(true)
					.FirstOrDefault(x => string.Equals(x.DefinitionElement, collectionPropertyName, StringComparison.Ordinal))
					?.ComponentElement;
			}

			public Type GetCollectionElementTypeFromCollectionType(Type collectionType)
			{
				return GetTypeToProcess(GetElementTypeFromCollectionType(collectionType));
			}

			public Type GetEntityTypeFromTableName(string tableName)
			{
				return GetTypeToProcess(GetBizoTypeFromTableName(tableName));
			}

			public IEnumerable<string> GetExcludedElements(Type type, Type componentType)
			{
				return GetElementsToIgnore(GetTypeToProcess(componentType));
			}

			public bool ShouldMoveLinkableOnlyEntityToProperties()
			{
				return EnvProxy.Instance.Registry.UCMoveLinkableOnlyEntitiesToProperties;
			}

			Type GetTypeToProcess(Type componentType)
			{
				if (componentType != null)
				{
					Type substituteType;
					if (TypeSubstitutions.TryGetValue(componentType, out substituteType) &&
						substituteType != null && componentType.IsAssignableFrom(substituteType))
					{
						return substituteType;
					}
				}
				return componentType;
			}

			public Type GetPropertyTypeSubstitute(Type type)
			{
				return BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(type);
			}

			Dictionary<Type, Type> TypeSubstitutions
			{
				get { return typeSubstitutions ?? (typeSubstitutions = new Dictionary<Type, Type>()); }
			}
			Dictionary<Type, Type> typeSubstitutions;

			public IEnumerable<PropertyInfo> GetAddInfoProperties(Type componentType)
			{
				PropertyInfo[] result = null;

				var addinfoAttribute = componentType?.GetCustomAttribute<UniversalCopyAddInfoAttribute>();

				if (addinfoAttribute != null)
				{
					if (!AddInfoProperties.TryGetValue(componentType, out result))
					{
						var assembly = componentType.Assembly;

						var addInfoDefinitions = GetAddInfoDefinitions(assembly);
						if (addInfoDefinitions.Length > 0)
						{
							var properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

							var mappingAddInfoDefinitions = addinfoAttribute.HasMapping
								? addInfoDefinitions.Select(c => string.Concat(addinfoAttribute.PropertyPrefix, c.Remove(0, addinfoAttribute.AddInfoPrefix.Length))).ToArray()
								: Array.Empty<string>();

							addInfoDefinitions = addInfoDefinitions.Concat(mappingAddInfoDefinitions).ToArray();

							result = properties.Where(x => addInfoDefinitions.Contains(x.Name) || addInfoDefinitions.Contains(GetAddInfoMappingPropertyName(x))).ToArray();

							AddInfoProperties.Add(componentType, result);
						}
					}
				}

				return result ?? Enumerable.Empty<PropertyInfo>();
			}

			string GetAddInfoMappingPropertyName(PropertyInfo info)
			{
				var mappingAttribute = info.GetCustomAttribute<UniversalCopyAddInfoPropertyMappingAttribute>();
				return mappingAttribute?.AddInfoPropertyInfoName ?? string.Empty;
			}

			public IEnumerable<PropertyInfo> GetValueOnlyProperties(Type componentType)
			{
				PropertyInfo[] result = null;
				if (componentType != null)
				{
					if (!ValueOnlyProperties.TryGetValue(componentType, out result))
					{
						result = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<UniversalCopyValueOnlyPropertyAttribute>(true) != null).ToArray();
						ValueOnlyProperties.Add(componentType, result);
					}
				}
				return result ?? Enumerable.Empty<PropertyInfo>();
			}

			string[] GetAddInfoDefinitions(Assembly assembly)
			{
				var result = Array.Empty<string>();
				var addInfoDef = assembly.GetCustomAttribute<UniversalCopyAddInfoPropertyDefinitionAttribute>();
				if (addInfoDef != null)
				{
					if (!AddInfoDefinitions.TryGetValue(assembly, out result))
					{
						result = addInfoDef.GetPropertyNames().ToArray();
						AddInfoDefinitions.Add(assembly, result);
					}
				}
				return result;
			}

			Dictionary<Type, PropertyInfo[]> AddInfoProperties
			{
				get { return addInfoProperties ?? (addInfoProperties = new Dictionary<Type, PropertyInfo[]>()); }
			}
			Dictionary<Type, PropertyInfo[]> addInfoProperties;

			Dictionary<Assembly, string[]> AddInfoDefinitions
			{
				get { return addInfoDefinitions ?? (addInfoDefinitions = new Dictionary<Assembly, string[]>()); }
			}
			Dictionary<Assembly, string[]> addInfoDefinitions;

			Dictionary<Type, PropertyInfo[]> ValueOnlyProperties
			{
				get { return valueOnlyProperties ?? (valueOnlyProperties = new Dictionary<Type, PropertyInfo[]>()); }
			}
			Dictionary<Type, PropertyInfo[]> valueOnlyProperties;
		}

		static void PreInitializeEntityNode(Type componentType, Dictionary<Type, Type> typeSubstitutions)
		{
			foreach (UniversalCopyChildrenSubstituteTypeAttribute substituteTypeAttribute in componentType.GetCustomAttributes(typeof(UniversalCopyChildrenSubstituteTypeAttribute), true))
			{
				typeSubstitutions[substituteTypeAttribute.DeclaredType] = substituteTypeAttribute.ActualType;
			}
		}

		internal static void AddExtendedPropertiesToEntityNode(EntityCopyTemplateNode entityNode, Type interfaceType, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			if (componentType != null)
			{
				ReflectExtendedPropertiesIfNeeded(entityNode, interfaceType, componentType, preProcessedTypes, exceptionThrownHandle);
			}

			AddDocDataNodeIfNeeded(entityNode, interfaceType, componentType);
		}

		static Type GetElementTypeFromCollectionType(Type collectionType)
		{
			return collectionType != null && typeof(IBusinessObjectCollection).IsAssignableFrom(collectionType) ? BusinessObjectCollection.GetElementTypeFromCollectionType(collectionType) : null;
		}

		static Type GetBizoTypeFromTableName(string tableName)
		{
			return !string.IsNullOrEmpty(tableName) ? BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName), false) : null;
		}

		static IEnumerable<string> GetElementsToIgnore(Type componentType)
		{
			if (componentType != null)
			{
				return
					from UniversalCopyIgnoreElementAttribute ignoreElementAttribute in componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true)
					from elementName in ignoreElementAttribute.ElementNames
					select elementName;
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		#endregion

		#region DocData

		internal const string DocDataEntityName = "DocData";

		static void AddDocDataNodeIfNeeded(EntityCopyTemplateNode entityNode, Type interfaceType, Type componentType)
		{
			if (entityNode.Name == "JobConsol" || entityNode.Name == "JobShipment" || entityNode.Name == "JobDeclaration" ||
				componentType != null &&
				(
					typeof(Enterprise.Integration.Freight.ICommonConsol).IsAssignableFrom(componentType) ||
					typeof(Enterprise.Integration.Freight.ICommonShipment).IsAssignableFrom(componentType) ||
					typeof(Customs.IBaseJobDeclaration).IsAssignableFrom(componentType)
				))
			{
				var docDataCopyTemplateNode =
					new RelatedEntityCopyTemplateNode(
						DocDataEntityName,
						string.Empty,
						string.Empty,
						new EntityCopyTemplateNode { Name = DocDataEntityName })
					{
						CanCopyWithZeroNodes = true,
						DisableCopyMethodLink = true
					};
				entityNode.Nodes.Add(docDataCopyTemplateNode);
			}
		}

		internal object GetDocDataRelatedEntity(object sourceEntity)
		{
			object docData = null;

			if (sourceEntity != null)
			{
				var notesParent = sourceEntity as IStmNoteParent;

				ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, notesParent != null ? notesParent.NotesParentPK : new ZGuid(GetEntityPK(sourceEntity)));
				query.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
				query.AddToFilter(StmNoteSchema.ST_Description, ZString.Empty);
				query.FetchOnlyFromLocalCache = notesParent != null && !notesParent.IsInDatabase;

				docData = (notesParent != null && notesParent.NotesFactory != null ? notesParent.NotesFactory : DefaultFactory).LoadTop1<IDocumentNote>(query);
			}

			return docData;
		}

		StmNote CreateNewDocDataEntity(object parentEntity, StmNote sourceEntity)
		{
			StmNote docData = null;

			if (sourceEntity != null)
			{
				docData = GetDocDataRelatedEntity(parentEntity) as StmNote;
				if (docData != null)
				{
					docData.CopyPersistentValuesFrom(sourceEntity);
				}
				else
				{
					docData = sourceEntity.Clone() as StmNote;
				}
			}

			return docData;
		}

		void SetDocDataRelationship(object targetEntity, StmNote docData)
		{
			if (targetEntity != null && docData != null)
			{
				docData.ST_ParentID = new ZGuid(GetEntityPK(targetEntity));

				var documentNote = docData as IDocumentNote;
				if (documentNote != null)
				{
					documentNote.MainBusinessObject = targetEntity;
				}
			}
		}

		#endregion

		#region JobHeader

#if DEBUG
		internal
#endif
		static Type JobHeaderGlowInterfaceType
		{
			get { return jobHeaderGlowInterfaceType ?? (jobHeaderGlowInterfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(IJobHeader), true)); }
		}
		[ThreadStatic]
		static Type jobHeaderGlowInterfaceType;

		#endregion

		#region Reflected properties

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		internal static void ReflectExtendedPropertiesIfNeeded(EntityCopyTemplateNode entityNode, Type interfaceType, Type componentType, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			if (componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length == 0)
			{
				return;
			}

			var elementAssociations = new Dictionary<string, string>();
			foreach (UniversalCopyAssociateElementAttribute associationAttribute in componentType.GetCustomAttributes(typeof(UniversalCopyAssociateElementAttribute), true))
			{
				if (!string.IsNullOrWhiteSpace(associationAttribute.ComponentElement) && !string.IsNullOrWhiteSpace(associationAttribute.DefinitionElement))
				{
					elementAssociations[associationAttribute.ComponentElement] = associationAttribute.DefinitionElement;
				}
			}

			foreach (UniversalCopyExtraCollectionAttribute extraCollectionAttribute in componentType.GetCustomAttributes(typeof(UniversalCopyExtraCollectionAttribute), true))
			{
				var collectionCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetCollectionCopyTemplateNode(extraCollectionAttribute.GlowInterfaceName, preProcessedTypes,
					extraCollectionAttribute.CollectionName, extraCollectionAttribute.ItemsTableName, extraCollectionAttribute.ItemPropertyName, extraCollectionAttribute.ItemParentTablePropertyName, exceptionThrownHandle);
				if (collectionCopyTemplateNode != null)
				{
					entityNode.Nodes.Add(collectionCopyTemplateNode);
				}
			}

			foreach (var propertyInfo in BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType))
			{
				string definitionElementName;
				if (!elementAssociations.TryGetValue(propertyInfo.Name, out definitionElementName))
				{
					definitionElementName = propertyInfo.Name;
				}

				if (CopyTemplateTree.IsOnBusinessObjectIgnoreList(propertyInfo.PropertyType))
				{
					continue;
				}

				if (entityNode.Nodes.All(node => node.Name != propertyInfo.Name))
				{
					UniversalCopyCollectionEntityAttribute collectionEntityAttribute;
					UniversalCopyExtraPropertyAttribute propertyEntityAttribute;
					UniversalCopyRelatedEntityAttribute relatedEntityAttribute;
					if ((collectionEntityAttribute = propertyInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).FirstOrDefault() as UniversalCopyCollectionEntityAttribute) != null)
					{
						AddCollectionFromProperty(collectionEntityAttribute, propertyInfo, entityNode, preProcessedTypes, exceptionThrownHandle);
					}
					else if ((relatedEntityAttribute = propertyInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).FirstOrDefault() as UniversalCopyRelatedEntityAttribute) != null)
					{
						AddRelatedEntityFromProperty(relatedEntityAttribute, propertyInfo, entityNode, preProcessedTypes, exceptionThrownHandle);
					}
					else if ((propertyEntityAttribute = propertyInfo.GetCustomAttributes(typeof(UniversalCopyExtraPropertyAttribute), true).FirstOrDefault() as UniversalCopyExtraPropertyAttribute) != null)
					{
						AddExtraProperty(propertyEntityAttribute, propertyInfo, entityNode);
					}
				}

				foreach (UniversalCopySplitCollectionAttribute splitCollectionAttributes in propertyInfo.GetCustomAttributes(typeof(UniversalCopySplitCollectionAttribute), true))
				{
					SplitCollection(splitCollectionAttributes, definitionElementName, entityNode);
				}

				CopyExtraMetadata(propertyInfo, entityNode);
			}
		}

		static void AddCollectionFromProperty(UniversalCopyCollectionEntityAttribute collectionEntityAttribute, PropertyInfo propertyInfo, EntityCopyTemplateNode entityNode, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			var collectionCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetCollectionCopyTemplateNode(propertyInfo.PropertyType, preProcessedTypes,
				!string.IsNullOrEmpty(collectionEntityAttribute.OverrideCollectionName) ? collectionEntityAttribute.OverrideCollectionName : propertyInfo.Name,
				collectionEntityAttribute.ItemsTableName, collectionEntityAttribute.ItemPropertyName, collectionEntityAttribute.ItemParentTablePropertyName, exceptionThrownHandle);
			if (collectionCopyTemplateNode != null)
			{
				entityNode.Nodes.Add(collectionCopyTemplateNode);
			}
		}

		static void AddRelatedEntityFromProperty(UniversalCopyRelatedEntityAttribute relatedEntityAttribute, PropertyInfo propertyInfo, EntityCopyTemplateNode entityNode, IDictionary<Type, EntityCopyTemplateNode> preProcessedTypes, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle)
		{
			var skipProperties = !string.IsNullOrEmpty(relatedEntityAttribute.CommaSeparatedSkipPropertiesNames)
				? relatedEntityAttribute.CommaSeparatedSkipPropertiesNames.Split(',')
				: null;

			var relatedEntityCopyTemplateNode = relatedEntityAttribute.DisableCopyMethodCopy
				? new RelatedEntityCopyTemplateNode { Name = propertyInfo.Name }
				: BusinessObjectToCopyTemplateReflectionHelper.GetRelatedEntityCopyTemplateNode(propertyInfo.PropertyType, preProcessedTypes, propertyInfo.Name, exceptionThrownHandle, skipProperties);

			if (relatedEntityCopyTemplateNode != null)
			{
				relatedEntityCopyTemplateNode.RelatedPropertyName = relatedEntityAttribute.RelatedPropertyName;
				relatedEntityCopyTemplateNode.RelatedEntityTableName = relatedEntityAttribute.RelatedEntityTableName;
				relatedEntityCopyTemplateNode.DisableCopyMethodCopy = relatedEntityAttribute.DisableCopyMethodCopy;
				relatedEntityCopyTemplateNode.DisableCopyMethodLink = relatedEntityAttribute.DisableCopyMethodLink;
				relatedEntityCopyTemplateNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled = relatedEntityAttribute.AllowCopyMethodLinkCopiedWhenLinkIsDisabled;

				entityNode.Nodes.Add(relatedEntityCopyTemplateNode);
			}
		}

		static void AddExtraProperty(UniversalCopyExtraPropertyAttribute propertyEntityAttribute, PropertyInfo propertyInfo, EntityCopyTemplateNode entityNode)
		{
			entityNode.Nodes.Add(new PropertyCopyTemplateNode(propertyInfo) { PropertyType = BusinessObjectToCopyTemplateReflectionHelper.GetSystemTypeFromZType(propertyInfo.PropertyType).Name, CustomCopyTemplateNode = propertyEntityAttribute.CustomCopyTemplateNode });
		}

		static void SplitCollection(UniversalCopySplitCollectionAttribute splitCollectionAttribute, string elementName, EntityCopyTemplateNode entityNode)
		{
			CollectionCopyTemplateNode originalCollectionNode = entityNode.Nodes.OfType<CollectionCopyTemplateNode>().FirstOrDefault(node => node.Name == elementName && node.Filter == null);
			if (originalCollectionNode != null)
			{
				var newCollectionCopyNode =
					new CollectionCopyTemplateNode
					{
						Id = Guid.NewGuid().ToString(),
						Name = originalCollectionNode.Name,
						Description = splitCollectionAttribute.Name,
						ItemPropertyName = originalCollectionNode.ItemPropertyName,
						ItemsTableName = originalCollectionNode.ItemsTableName,
						ItemParentTablePropertyName = originalCollectionNode.ItemParentTablePropertyName,
						InnerNode = new TemplateCopyTemplateNode(EntityCopyTemplateNode.GetEntityTemplateNodeFromTopLevelCopyTemplateNode(originalCollectionNode)),
						Filter = new EntityFilter { FilterTypeId = EntityFilterTypeIds.MandatoryExpressionFilter, FilterData = splitCollectionAttribute.Filter }
					};

				entityNode.Nodes.Add(newCollectionCopyNode);
			}
		}

		static void CopyExtraMetadata(PropertyInfo propertyInfo, EntityCopyTemplateNode entityNode)
		{
			var extraMetadata = propertyInfo.GetCustomAttribute<UniversalCopyExtraMetadataAttribute>();
			if (extraMetadata != null)
			{
				var propertyNode = entityNode.Nodes.FirstOrDefault(node => node.Name == propertyInfo.Name);
				if (propertyNode != null)
				{
					propertyNode.IsMandatory = extraMetadata.IsMandatory;
					propertyNode.Priority = extraMetadata.Priority;
				}

				propertyNode = entityNode.Nodes.FirstOrDefault(node => node.Name == propertyInfo.Name && string.IsNullOrEmpty(node.Description));
				if (propertyNode != null && !extraMetadata.HumanReadableName.IsNullOrEmpty())
				{
					propertyNode.Description = extraMetadata.HumanReadableName;
				}
			}
		}

		#endregion

		#endregion

		#region Error Handling

		protected override void ReportError(string message, Exception exception = null)
		{
			if (exception != null && exception.IsCriticalException())
			{
				base.ReportError(message, exception);
			}
			else
			{
				if (Notifications != null)
				{
					if (exception == null || exception is FormatException || exception.InnerException is FormatException)
					{
						Notifications.AddError(message);
						return;
					}
				}

				base.ReportError(message, exception);
			}
		}

		protected override string SetPropertyWrongValueErrorMessage
		{
			get { return Res.GetData("33a35f0d-67a3-4dbb-990d-07276e3d1f93", "Cannot set property {0}.{1} of type {2} with value '{3}'.").Caption; }
		}

		public INotifications Notifications { get; set; }

		#endregion
	}
}
