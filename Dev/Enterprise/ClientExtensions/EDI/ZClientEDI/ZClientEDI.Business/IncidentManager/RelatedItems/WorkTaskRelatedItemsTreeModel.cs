using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkTaskRelatedItemsTreeModel : ZTreeModel<WorkTaskRelatedItemsTreeWrapper>
	{
		readonly ZNodeCollection<WorkTaskRelatedItemsTreeWrapper> rootNodes = new ZNodeCollection<WorkTaskRelatedItemsTreeWrapper>();
		readonly IWorkTaskTreeNode relatedItemSource;
		readonly bool loadJustParents;
		public event EventHandler NotifyDeletedItem;

		public WorkTaskRelatedItemsTreeModel(IWorkTaskRelatedItemSource relatedItemSource, bool loadJustParents = false)
			: base(relatedItemSource.Factory)
		{
			this.relatedItemSource = relatedItemSource as IWorkTaskTreeNode;
			this.loadJustParents = loadJustParents;

			BuildTree();
		}

		#region Build Tree

		public void BuildTree(bool reload = false)
		{
			rootNodes.Clear();

			if (loadJustParents)
			{
				BuildTreeParentsCore(reload);
			}
			else
			{
				BuildTreeChildrenCore(reload);
			}
		}

		void BuildTreeParentsCore(bool reload)
		{
			if (relatedItemSource.ParentsOnlyRelatedItems != null)
			{
				if (reload)
				{
					relatedItemSource.ParentsOnlyRelatedItems.Load();
				}

				var childrenFromParent = relatedItemSource.ParentsOnlyRelatedItems.ToArray();

				FetchTables(childrenFromParent);

				foreach (IWorkTaskTreeNode item in childrenFromParent)
				{
					FetchOrgAddress(item);

					var topNodeWrapper = new WorkTaskRelatedItemsTreeWrapper(this, item, null, false);
					var topNode = new WorkTaskRelatedItemsTreeNode(this, topNodeWrapper);
					rootNodes.Add(topNode);
				}

				FetchOrgHeader(childrenFromParent);
			}
		}

		void BuildTreeChildrenCore(bool reload)
		{
			var topNodeRelatedChildren = new List<WorkTaskRelatedItemsTreeWrapper>();

			if (relatedItemSource.ChildrenOnlyRelatedItems != null)
			{
				if (reload)
				{
					relatedItemSource.ChildrenOnlyRelatedItems.Load();
				}

				var childrenFromParent = relatedItemSource.ChildrenOnlyRelatedItems.ToArray();

				FetchTables(childrenFromParent);

				foreach (var item in childrenFromParent)
				{
					item.HasChangesChanged += Item_HasChangesChanged;

					var wrapper = new WorkTaskRelatedItemsTreeWrapper(this, (IWorkTaskTreeNode)item, GetChildren((IWorkTaskTreeNode)item), false);
					topNodeRelatedChildren.Add(wrapper);
				}

				FetchOrgHeader(childrenFromParent);
			}

			// Add itself in the top of the tree
			var topNodeWrapper = new WorkTaskRelatedItemsTreeWrapper(this, relatedItemSource, topNodeRelatedChildren, false, true);
			var topNode = new WorkTaskRelatedItemsTreeNode(this, topNodeWrapper);
			rootNodes.Add(topNode);
		}

		void Item_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (sender is BusinessObject bizO && bizO.IsDeleted)
			{
				bizO.HasChangesChanged -= Item_HasChangesChanged;
				NotifyDeletedItem?.Invoke(sender, e);
			}
		}

		List<WorkTaskRelatedItemsTreeWrapper> GetChildren(IWorkTaskTreeNode item)
		{
			FetchOrgAddress(item);

			var relatedChildren = new List<WorkTaskRelatedItemsTreeWrapper>();

			if (item.ChildrenOnlyRelatedItems != null)
			{
				foreach (IWorkTaskTreeNode child in item.ChildrenOnlyRelatedItems)
				{
					FetchOrgAddress(child);

					if (relatedItemSource != child && child != item) // Ignore itself as child
					{
						relatedChildren.Add(new WorkTaskRelatedItemsTreeWrapper(this, child, null, true));
					}
				}
			}
			return relatedChildren;
		}

		void FetchTables(BusinessObject[] childrenFromParent)
		{
			var parentsChildrenPKs = childrenFromParent.Select(b => b.PK);
			var pivots = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, parentsChildrenPKs));
			var tableTypes = new Dictionary<string, ITableSchema>();

			var childrenPKs = new List<ZGuid>();
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			foreach (var piv in pivots)
			{
				if (!tableTypes.ContainsKey(piv.XX_Relation2TableCode))
				{
					tableTypes.Add(piv.XX_Relation2TableCode, schemaResolver.GetTableSchemaFromColumnNamePrefix(piv.XX_Relation2TableCode));
				}

				childrenPKs.Add(piv.XX_Relation2ID);
				Factory.AddFetchHint(tableTypes[piv.XX_Relation2TableCode].TableName, piv.XX_Relation2ID);
			}

			Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, childrenPKs));
			Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, parentsChildrenPKs));

			Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.FH_ParentId, childrenPKs));
			Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.FH_ParentId, parentsChildrenPKs));

			Factory.AddFetchHint(WorkItemRequestLinkSchema.Instance, new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, parentsChildrenPKs));
		}

		void FetchOrgAddress(IWorkTaskTreeNode item)
		{
			item.AddFetchHintsForOrgAddressIfRequired();
		}

		void FetchOrgHeader(BusinessObject[] childrenFromParent)
		{
			// Need to be executed in the end, after fetch all OrgAddress
			foreach (IWorkTaskTreeNode item in childrenFromParent)
			{
				item.AddFetchHintsForOrgHeaderIfRequired();

				var childrenOnlyRelatedItems = item.ChildrenOnlyRelatedItems;
				if (childrenOnlyRelatedItems != null)
				{
					var children = childrenOnlyRelatedItems.Where(c => c is IWorkTaskTreeNode).Cast<IWorkTaskTreeNode>();
					foreach (var child in children)
					{
						child.AddFetchHintsForOrgHeaderIfRequired();
					}
				}
			}
		}

		#endregion

		protected override ZNode<WorkTaskRelatedItemsTreeWrapper> CreateNewNodeCore(ZTreeModel<WorkTaskRelatedItemsTreeWrapper> treeModel, WorkTaskRelatedItemsTreeWrapper bizObj) =>
			new WorkTaskRelatedItemsTreeNode((WorkTaskRelatedItemsTreeModel)treeModel, bizObj);

		protected override ZNodeCollection<WorkTaskRelatedItemsTreeWrapper> GetRootNodes() => rootNodes;
	}
}
