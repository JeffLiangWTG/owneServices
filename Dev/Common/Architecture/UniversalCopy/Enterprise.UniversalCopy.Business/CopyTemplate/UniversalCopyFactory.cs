using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	public class UniversalCopyFactory
	{
		public UniversalCopyFactory(Type elementType, ModuleIdentifier moduleId, EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle = null)
		{
			this.elementType = elementType;
			this.moduleId = moduleId;
			this.exceptionThrownHandle = exceptionThrownHandle;
		}

		readonly ModuleIdentifier moduleId;
		readonly Type elementType;
		readonly EventHandler<CopyTemplateTree.ExceptionThrownEventArgs> exceptionThrownHandle;

		public BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = "UniversalCopyFactory_GetFactory" });
		BusinessObjectFactory factory;

		public UniversalCopyTemplate GetCopyTemplateInLocalFactoryAndExtend(UniversalCopyTemplate sourceTemplate, Type interfaceType)
		{
			var templateInLocalFactory = (UniversalCopyTemplate)sourceTemplate.Factory.CreateNewFactory().ImportFromAnotherFactory(sourceTemplate);
			ExtendTemplate(templateInLocalFactory, interfaceType);
			return templateInLocalFactory;
		}

		public void ExtendTemplate(UniversalCopyTemplate localTemplate, Type interfaceType)
		{
			if (localTemplate.IsExtended)
			{
				return;
			}

			localTemplate.IsExtended = true;
			var newCopyTemplateTree = CreateCopyTemplateTree(interfaceType);
			SyncTemplateIfNeeded(localTemplate, newCopyTemplateTree.Name);
			RemoveIgnoreElementIfNeeded(localTemplate);
			localTemplate.CopyTemplateTree.CopyTemplateNode.Extend(newCopyTemplateTree);
		}

		void SyncTemplateIfNeeded(UniversalCopyTemplate localTemplate, string expectedRootName)
		{
			var instanceTypeAttribute = elementType.GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			var shouldSync = instanceTypeAttribute != null && instanceTypeAttribute.ShouldSyncTreeNodes;
			if (shouldSync && localTemplate.CopyTemplateTree.Name != expectedRootName)
			{
				var rootReplacementNode = localTemplate.CopyTemplateTree.ChildNodes.OfType<EntityCopyTemplateBizo>().FirstOrDefault(e => e.Name == expectedRootName);
				if (rootReplacementNode != null)
				{
					var replacementTree = new CopyTemplateTree
					{
						ConfigurationName = localTemplate.CopyTemplateTree.ConfigurationName,
						InnerNode = rootReplacementNode.CopyTemplateNode.InnerNode,
						Name = rootReplacementNode.CopyTemplateNode.InnerNode.Name
					};

					localTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(replacementTree, localTemplate);
				}
			}
		}

		public void RemoveIgnoreElementIfNeeded(UniversalCopyTemplate localTemplate)
		{
			if (localTemplate?.CopyTemplateTree?.CopyTemplateNode?.InnerNode is EntityCopyTemplateNode node && node.Nodes.Any())
			{
				var ignoreElements = BusinessObjectCopyManager.CopyTreeConfiguration.GetExcludedElements(null, elementType);
				if (ignoreElements.Any())
				{
					var hashSetResults = ignoreElements.ToHashSet();
					node.Nodes.RemoveAll(n => hashSetResults.Contains(n.Name));
				}

				foreach (var child in node.Nodes.ToArray())
				{
					var childElementType = BusinessObjectCopyManager.CopyTreeConfiguration.GetEntityTypeFromTableName(child.GetTableName());
					if (childElementType != null && CopyTemplateTree.IsOnBusinessObjectIgnoreList(childElementType))
					{
						node.Nodes.Remove(child);
						continue;
					}

					RemoveIgnoreElementFromSubtree(child, 0);
				}
			}
		}

		void RemoveIgnoreElementFromSubtree(CopyTemplateNode node, int recursionDepth)
		{
			if (recursionDepth <= 15)
			{
				var innerMostNode = UnwrapNode(node);

				if (innerMostNode is EntityCopyTemplateNode entityNode && entityNode.Nodes.Any())
				{
					var innerElementType = BusinessObjectCopyManager.CopyTreeConfiguration.GetEntityTypeFromTableName(innerMostNode.GetTableName());
					if (innerElementType != null)
					{
						var ignoreElements = BusinessObjectCopyManager.CopyTreeConfiguration.GetExcludedElements(null, innerElementType);
						if (ignoreElements.Any())
						{
							var hashSetResults = ignoreElements.ToHashSet();
							entityNode.Nodes.RemoveAll(n => hashSetResults.Contains(n.Name));
						}

						var ignoredBusinessObjects = entityNode.Nodes.Where(w =>
						{
							var childElementType = BusinessObjectCopyManager.CopyTreeConfiguration.GetEntityTypeFromTableName(w.GetTableName());
							return childElementType != null && CopyTemplateTree.IsOnBusinessObjectIgnoreList(childElementType);
						});

						entityNode.Nodes.RemoveAll(a => ignoredBusinessObjects.Contains(a));

						foreach (var child in entityNode.Nodes)
						{
							RemoveIgnoreElementFromSubtree(child, recursionDepth + 1);
						}
					}
				}
			}
		}

		CopyTemplateNode UnwrapNode(CopyTemplateNode node)
		{
			var currentNode = node;
			while (currentNode is WrappedCopyTemplateNode wrappedNode)
			{
				currentNode = wrappedNode.InnerNode;
			}

			return currentNode;
		}

		public UniversalCopyTemplate GetNewCopyTemplateFrom(UniversalCopyTemplate sourceTemplate, Type interfaceType)
		{
			var newTemplate = Factory.CreateNewFactory().New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = GetContextKey();
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.S9_FilterData = sourceTemplate.S9_FilterData;

			var newCopyTemplateTree = CreateCopyTemplateTree(interfaceType);
			SyncTemplateIfNeeded(newTemplate, newCopyTemplateTree.Name);

			var copyTemplateNode = newTemplate.CopyTemplateTree.CopyTemplateNode;
			copyTemplateNode.Extend(newCopyTemplateTree);

			copyTemplateNode.ConfigurationName = string.Empty;
			if (!newTemplate.IsPublishedGlobal && !string.IsNullOrEmpty(copyTemplateNode.FilterList))
			{
				copyTemplateNode.FilterList = string.Empty;
			}

			return newTemplate;
		}

		public UniversalCopyTemplate GetNewCopyTemplate(Type interfaceType)
		{
			var newTemplate = Factory.CreateNewFactory().New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = GetContextKey();
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(CreateCopyTemplateTree(interfaceType), newTemplate);

			return newTemplate;
		}

		public UniversalCopyTemplate GetNewCopyTemplateFromImport(CopyTemplateTree importedCopyTemplateTree, Type interfaceType)
		{
			UniversalCopyTemplate newTemplate = Factory.CreateNewFactory().New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = GetContextKey();
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			var copyTemplateNode = importedCopyTemplateTree;
			copyTemplateNode.Extend(CreateCopyTemplateTree(interfaceType));
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateNode, newTemplate);
			newTemplate.CopyTemplateTree.ConfigurationName = newTemplate.CopyTemplateTree.ConfigurationName.Replace("&", "");
			RemoveIgnoreElementIfNeeded(newTemplate);

			return newTemplate;
		}

		internal CopyTemplateTree CreateCopyTemplateTree(Type interfaceType)
		{
			if (interfaceType == null)
			{
				var universalCopyWithExtendedEntitiesAttribute =
					elementType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Cast<UniversalCopyWithExtendedEntitiesAttribute>().FirstOrDefault();
				if (universalCopyWithExtendedEntitiesAttribute != null)
				{
					var entityCopyTemplateNode = BusinessObjectToCopyTemplateReflectionHelper.GetEntityCopyTemplateNode(
						elementType, null, new Dictionary<Type, EntityCopyTemplateNode>(), exceptionThrownHandle,
						universalCopyWithExtendedEntitiesAttribute.IgnoreAllElementsExceptSpecificallyMarked ? new[] { "*" } : Array.Empty<string>());
					if (entityCopyTemplateNode != null)
					{
						return new CopyTemplateTree { Name = elementType.Name, InnerNode = entityCopyTemplateNode };
					}
				}
				return null;
			}
			else
			{
				return new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration, exceptionThrownHandle);
			}
		}

		public IEnumerable<UniversalCopyTemplate> LoadCopyTemplates()
		{
			return Factory.Load<UniversalCopyTemplate>(GetCopyContextFilter());
		}

		ZQuery GetCopyContextFilter()
		{
			ZString contextKey = GetContextKey();
			if (contextKey.IsEmpty)
			{
				ErrorReporter.ReportOnce("UniversalCopyFactory_GetFilter_EmptyContext",
					string.Format(CultureInfo.InvariantCulture, "Context is empty. ModuleID: {0}, Element Type: {1}", moduleId != null ? moduleId.Name : "-", elementType != null ? elementType.Name : "-"));

				return ZQuery.NoResultQuery;
			}

			ZQuery query = new ZQuery(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.Equal, contextKey);

			ZQuery companyQuery = new ZQuery(StmModuleFilterSchema.S9_GC, EnvProxy.Instance.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_GC, DBNull.Value);
			query.AddToFilter(companyQuery);

			ZQuery currentUserOrPublishedOrSystem = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, EnvProxy.Instance.CurrentUser.PK);
			currentUserOrPublishedOrSystem.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, true);
			currentUserOrPublishedOrSystem.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsSystem, true);
			query.AddToFilter(currentUserOrPublishedOrSystem);

			return query;
		}

		public IEnumerable<StmUniversalCopyScheduleTask> LoadCopySchedules(BusinessObject scheduleTarget)
		{
			var query = new ZQuery(StmUniversalCopySchema.SUC_CopyObjectId, scheduleTarget.PK);
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectTableCode, scheduleTarget.TablePrefix);
			return Factory.Load<StmUniversalCopy>(query).Select(item => item.ScheduleTask).Where(item => item != null);
		}

		public ZString GetContextKey()
		{
			string contextKey = string.Empty;

			if (moduleId != null && moduleId != ModuleIDs.NotAssigned)
			{
				contextKey = moduleId.Name;
			}

			if (string.IsNullOrEmpty(contextKey) && elementType != null)
			{
				contextKey = elementType.FullName;
			}

			if (!string.IsNullOrEmpty(contextKey))
			{
				contextKey += StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;

				if (contextKey.Length > StmModuleFilterSchema.S9_ModuleID.MaxLength)
				{
					contextKey = contextKey.Substring(contextKey.Length - StmModuleFilterSchema.S9_ModuleID.MaxLength);
				}
			}

			return contextKey;
		}
	}
}


