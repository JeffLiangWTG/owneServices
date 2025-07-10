using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	class UniversalCopyTemplateTreeView : ZTreeView
	{
		public UniversalCopyTemplateTreeView()
		{
			InitialiseImages();
			Sorted = true;
			ShowNodeToolTips = true;
		}

		public UniversalCopyManager CopyManager { get; set; }

		void InitialiseImages()
		{
			ImageList = new ImageList();
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Error));
			ImageList.Images.Add(Icons.GetIcon(IconTypes.MessageError));
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Warning));
			ImageList.Images.Add(Icons.GetImage(IconTypes.CopyButtonRest)); // Copy
			ImageList.Images.Add(Icons.GetImage(IconTypes.Tag)); // Link
			ImageList.Images.Add(Icons.GetImage(IconTypes.FindButtonRest)); // Filter
			ImageList.Images.Add(Icons.GetIcon(IconTypes.Blank)); // None

			StateImageList = new ImageList();
			StateImageList.Images.Add(Icons.GetIcon(IconTypes.StmNote)); // Related Record
			StateImageList.Images.Add(Icons.GetIcon(IconTypes.SomeBooks)); // Collection
		}

		internal const int ImageIndexError = 0;
		internal const int ImageIndexMessageError = 1;
		internal const int ImageIndexWarning = 2;
		internal const int ImageIndexCopy = 3;
		internal const int ImageIndexLink = 4;
		internal const int ImageIndexFilter = 5;
		internal const int ImageIndexNone = 6;

		internal const int ImageIndexRelatedEntity = 0;
		internal const int ImageIndexCollection = 1;

		public void Bind(CopyTemplateTreeBizo templateTreeBizo)
		{
			Unbind();

			AddNode(Nodes, templateTreeBizo);
			if (Nodes.Count > 0)
			{
				Nodes[0].Expand();
				SelectedNode = Nodes[0];
			}
		}

		public void Unbind()
		{
			Nodes.Clear();
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand(e);

			if (e.Node.Nodes.Count == 1 && !(e.Node.Nodes[0] is UniversalCopyTreeNode))
			{
				e.Node.Nodes.Clear();
				AddNodes(e.Node.Nodes, (EntityCopyTemplateBizo)((UniversalCopyTreeNode)e.Node).BizO);
			}
		}

		public void AddNodes(TreeNodeCollection treeNodes, EntityCopyTemplateBizo entityTemplateBizo)
		{
			var collectionNodes = entityTemplateBizo.ChildNodes.OfType<CollectionCopyTemplateBizo>();
			var otherNodes = entityTemplateBizo.ChildNodes.Except(collectionNodes).Cast<EntityCopyTemplateBizo>();

			AddCollectionNodes(treeNodes, collectionNodes);
			AddOtherNodes(treeNodes, otherNodes);
		}

		void AddCollectionNodes(TreeNodeCollection treeNodes, IEnumerable<CollectionCopyTemplateBizo> collectionNodes)
		{
			var splitGroups = collectionNodes.Where(x => !string.IsNullOrEmpty(x.SplitOwner)).GroupBy(x => x.SplitOwner);
			var unsplitGroup = collectionNodes.Where(x => string.IsNullOrEmpty(x.SplitOwner));

			foreach (var splitGroup in splitGroups)
			{
				var unfilteredRestNode = unsplitGroup.FirstOrDefault(x => x.CollectionId == splitGroup.First().SplitOwner);
				if (unfilteredRestNode != null)
				{
					var visualNode = CreateVisualNode(unfilteredRestNode);
					treeNodes.Add(visualNode);
					foreach (var child in splitGroup)
					{
						AddNode(visualNode.Nodes, child);
					}
					unfilteredRestNode.IsUnfilteredRest = true;
					AddNode(visualNode.Nodes, unfilteredRestNode);
				}
				else
				{
					unsplitGroup.Concat(splitGroup);
				}
			}

			foreach (var child in unsplitGroup)
			{
				if (!child.IsUnfilteredRest)
				{
					AddNode(treeNodes, child);
				}
			}
		}

		void AddOtherNodes(TreeNodeCollection treeNodes, IEnumerable<EntityCopyTemplateBizo> nodes)
		{
			foreach (var node in nodes)
			{
				AddNode(treeNodes, node);
			}
		}

		public UniversalCopyTreeNode AddNode(TreeNodeCollection treeNodes, EntityCopyTemplateBizo entityTemplateBizo)
		{
			var newNode = new UniversalCopyTreeNode(entityTemplateBizo, entityTemplateBizo.Description, GetBizoNodeImageIndex(entityTemplateBizo), ImageIndexError, ImageIndexMessageError, ImageIndexWarning);
			newNode.StateImageIndex = entityTemplateBizo is CollectionCopyTemplateBizo ? ImageIndexCollection : ImageIndexRelatedEntity;
			if (entityTemplateBizo.ChildNodes.Any())
			{
				newNode.Nodes.Add("-");
			}

			if (entityTemplateBizo is RelatedEntityCopyTemplateBizo)
			{
				var relatedTemplateBizo = (RelatedEntityCopyTemplateBizo)entityTemplateBizo;

				if (!relatedTemplateBizo.CopyTemplateNode.CanCopy)
				{
					newNode.Nodes.Clear();
				}
				if (relatedTemplateBizo.CopyTemplateNode.CanLink)
				{
					newNode.ContextMenu =
						new ContextMenu(
							new MenuItem[]
							{
								new ZMenuItem(
									ResString.GetMultilingualString("f60ae1f7-49b1-474f-bc88-d255e17e4813", "Move to properties"),
									(sender, e) => ConvertRelatedEntityToPropertyNodeBizo(newNode))
							});
				}

				relatedTemplateBizo.CopyMethodInfo.ValueChanged += CopyMethodInfo_ValueChanged;
			}
			else if (entityTemplateBizo is CollectionCopyTemplateBizo)
			{
				var collectionTemplateBizo = (CollectionCopyTemplateBizo)entityTemplateBizo;

				newNode.Text = collectionTemplateBizo.IsUnfilteredRest ? unfilteredRest.ToString() : entityTemplateBizo.Description.ToString();

				if (collectionTemplateBizo.IsSplitCollection)
				{
					newNode.ContextMenu =
						new ContextMenu(
							new MenuItem[]
							{
								new ZMenuItem(
									ResString.GetMultilingualString("f2f49142-6620-45ab-a4ef-aaa57fb97c40", "Remove split collection"),
									(sender, e) => RemoveSplitCollectionNodeBizo(newNode))
							});
				}
				else if (collectionTemplateBizo.EntityFilter == null || collectionTemplateBizo.EntityFilter.FilterTypeId != EntityFilterTypeIds.MandatoryExpressionFilter)
				{
					newNode.ContextMenu =
						new ContextMenu(
							new MenuItem[]
							{
								new ZMenuItem(
									ResString.GetMultilingualString("291e512e-5831-44b3-a4af-8a72fa8d8bba", "Split collection for filtering"),
									(sender, e) => DuplicateCollectionNodeBizo(newNode, collectionTemplateBizo.IsUnfilteredRest))
							});
				}

				collectionTemplateBizo.CopyMethodInfo.ValueChanged += CopyMethodInfo_ValueChanged;
			}

			newNode.ToolTipText = GetBizoNodeToolTipText(entityTemplateBizo);

			treeNodes.Add(newNode);
			return newNode;
		}

		internal int GetBizoNodeImageIndex(EntityCopyTemplateBizo entityTemplateBizo)
		{
			var result = ImageIndexNone;

			RelatedEntityCopyTemplateBizo relatedTemplateBizo;
			CollectionCopyTemplateBizo collectionTemplateBizo;
			if (entityTemplateBizo is CopyTemplateTreeBizo)
			{
				result = ImageIndexCopy;
			}
			else if ((relatedTemplateBizo = entityTemplateBizo as RelatedEntityCopyTemplateBizo) != null)
			{
				if (relatedTemplateBizo.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy)
				{
					result = ImageIndexCopy;
				}
				else if (relatedTemplateBizo.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Link || relatedTemplateBizo.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.LinkCopied)
				{
					result = ImageIndexLink;
				}
			}
			else if ((collectionTemplateBizo = entityTemplateBizo as CollectionCopyTemplateBizo) != null)
			{
				if (collectionTemplateBizo.CopyTemplateNode.CopyMethod == CollectionCopyMethod.All)
				{
					result = ImageIndexCopy;
				}
				else if (collectionTemplateBizo.CopyTemplateNode.CopyMethod == CollectionCopyMethod.Filter)
				{
					if (collectionTemplateBizo.EntityFilter != null && collectionTemplateBizo.EntityFilter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter)
					{
						result = ImageIndexCopy;
					}
					else
					{
						result = ImageIndexFilter;
					}
				}
			}

			return result;
		}

		string GetBizoNodeToolTipText(EntityCopyTemplateBizo entityTemplateBizo)
		{
			return entityTemplateBizo.Kind + " " + entityTemplateBizo.Name + ": " + entityTemplateBizo.GetCopyActionDescription();
		}

		void CopyMethodInfo_ValueChanged(object sender, EventArgs e)
		{
			var entityTemplateBizo = sender as EntityCopyTemplateBizo;
			var node = FindNode(sender);
			if (node != null && entityTemplateBizo != null)
			{
				node.NormalImageIndex = GetBizoNodeImageIndex(entityTemplateBizo);
				node.SelectedImageIndex = node.NormalImageIndex;
				node.ToolTipText = GetBizoNodeToolTipText(entityTemplateBizo);
			}
		}

		internal List<string> GetElementsNameFullPathToNode(UniversalCopyTreeNode node)
		{
			if (node == null)
			{
				return null;
			}

			var path = new List<string>();
			while (node != null)
			{
				var copyTemplateNodeBizo = node.BizO as CopyTemplateNodeBizo;
				if (copyTemplateNodeBizo != null)
				{
					path.Insert(0, copyTemplateNodeBizo.Name);
				}
				node = node.Parent as UniversalCopyTreeNode;
			}

			return path;
		}

		public UniversalCopyTreeNode FindNode(object dataSource)
		{
			return FindNode(Nodes, dataSource);
		}

		UniversalCopyTreeNode FindNode(TreeNodeCollection nodes, object dataSource)
		{
			foreach (var node in nodes.OfType<UniversalCopyTreeNode>())
			{
				if (node.BizO == dataSource)
				{
					return node;
				}

				var childResult = FindNode(node.Nodes, dataSource);
				if (childResult != null)
				{
					return childResult;
				}
			}
			return null;
		}

		#region Convert related entity to property node

		void ConvertRelatedEntityToPropertyNodeBizo(UniversalCopyTreeNode node, bool quiet = false)
		{
			RelatedEntityCopyTemplateBizo relatedEntityCopyBizo = node.BizO as RelatedEntityCopyTemplateBizo;
			EntityCopyTemplateBizo parentEntity = ((UniversalCopyTreeNode)node.Parent).BizO as EntityCopyTemplateBizo;
			if (relatedEntityCopyBizo != null && parentEntity != null)
			{
				if (quiet ||
					Globals.Message.Show(
						Res.GetString("d91f2f21-43a2-49e4-9425-a60daa56dded",
							"Are you sure you want to move {0} to Properties?\r\nYou will not be able to configure how to copy each sub-element of {0} and existing configuration will be lost.",
							node.Text),
						Res.GetString("4f8db95b-21cb-48fc-8b1d-2580c2844f0e", "Moving Related Record copy configuration to Properties"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					PropertyCopyTemplateNode newPropertyNode =
						new PropertyCopyTemplateNode
						{
							Id = Guid.NewGuid().ToString(),
							Name = relatedEntityCopyBizo.CopyTemplateNode.RelatedPropertyName,
							PropertyType = nameof(Object)
						};

					var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(newPropertyNode.Name, parentEntity.EntityTableName);
					if (column != null)
					{
						newPropertyNode.PropertyType = column.DotNetType.Name;
					}

					EntityCopyTemplateNode entityCopyNode = EntityCopyTemplateNode.GetEntityTemplateNodeFromTopLevelCopyTemplateNode(parentEntity.CopyTemplateNode);
					if (entityCopyNode != null)
					{
						entityCopyNode.Nodes.Remove(relatedEntityCopyBizo.CopyTemplateNode);
						entityCopyNode.Nodes.Add(newPropertyNode);
					}
					var newPropertyBizo = new PropertyCopyTemplateBizo(newPropertyNode, parentEntity);
					newPropertyBizo.HasJustBeenAdded = true;
					parentEntity.PropertyNodes.Add(newPropertyBizo);
					if (parentEntity.PropertyNodes.SortInformation != null)
					{
						parentEntity.PropertyNodes.Sort(parentEntity.PropertyNodes.SortInformation);
					}

					var parentNode = node.Parent;
					if (SelectedNode == parentNode)
					{
						SelectedNode = node;
					}
					SelectedNode = parentNode;

					parentEntity.ChildNodes.Remove(relatedEntityCopyBizo);
					parentNode.Nodes.Remove(node);

					parentEntity.HasChanges = true;
				}
			}
			else if (!quiet)
			{
				Globals.Message.Show(Res.GetString("c0da0f37-e1ee-4f46-827b-c0766ac0ffcd", "Cannot move {0} to properties.", node.Text));
			}
		}

		#endregion

		#region Duplicate collection node

		internal void DuplicateCollectionNodeBizo(UniversalCopyTreeNode node, bool isUnfilteredRest)
		{
			var collectionCopyBizo = node.BizO as CollectionCopyTemplateBizo ?? node.VisualNodeBizo as CollectionCopyTemplateBizo;
			if (collectionCopyBizo != null && collectionCopyBizo.Parent != null)
			{
				var filterBizo = GetCollectionNodeFilterBizo(node, collectionCopyBizo);
				if (filterBizo == null)
				{
					Globals.Message.ShowWarning(Res.GetString("1e8f3a60-84fd-487f-864e-baf75d14f6af", "Cannot split collection {0} to filtered parts: filters list not found.", collectionCopyBizo.Name));
					return;
				}

				var subCollectionName = Globals.Message.QueryUserResponse(
					new UserResponseArgument
					{
						Message = Res.GetString("7f212b37-11a8-4518-a484-b3a284892ab4", "Please enter name for new sub-collection element."),
						Caption = Res.GetString("112292bf-8d91-40cd-af15-5f830f4dbe94", "Splitting collection to filtered parts"),
						Buttons = ZMessageBoxButtons.OKCancel,
						Icon = ZMessageBoxIcon.Question,
						DefaultButton = ZMessageBoxDefaultButton.Button1,
						MinimumResponseLength = 1,
						UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal
					});

				if (string.IsNullOrWhiteSpace(subCollectionName))
				{
					return;
				}

				subCollectionName = subCollectionName.Trim();

				if (subCollectionName.Equals(unfilteredRest.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					Globals.Message.ShowWarning(
						Res.GetString("47030044-1ad4-44a2-bb97-f1e3596ee316", "May not name element 'Unfiltered rest'.", subCollectionName, collectionCopyBizo.Parent.Description),
						Res.GetString("112292bf-8d91-40cd-af15-5f830f4dbe94", "Splitting collection to filtered parts"));
					DuplicateCollectionNodeBizo(node, isUnfilteredRest);
					return;
				}
				if (collectionCopyBizo.Parent.ChildNodes.Cast<EntityCopyTemplateBizo>().Any(childNode => subCollectionName.Equals(childNode.Description, StringComparison.OrdinalIgnoreCase)))
				{
					Globals.Message.ShowWarning(
						Res.GetString("98c064c3-d358-42a5-ae6e-046a88b8c19e", "There is already an element '{0}' on '{1}'.", subCollectionName, collectionCopyBizo.Parent.Description),
						Res.GetString("112292bf-8d91-40cd-af15-5f830f4dbe94", "Splitting collection to filtered parts"));
					DuplicateCollectionNodeBizo(node, isUnfilteredRest);
					return;
				}

				var newCollectionCopyNode =
					new CollectionCopyTemplateNode
					{
						Id = Guid.NewGuid().ToString(),
						Name = collectionCopyBizo.Name,
						Description = subCollectionName,
						ItemPropertyName = collectionCopyBizo.CopyTemplateNode.ItemPropertyName,
						ItemsTableName = collectionCopyBizo.CopyTemplateNode.ItemsTableName,
						ItemParentTablePropertyName = collectionCopyBizo.CopyTemplateNode.ItemParentTablePropertyName,
						InnerNode = new TemplateCopyTemplateNode(EntityCopyTemplateNode.GetEntityTemplateNodeFromTopLevelCopyTemplateNode(collectionCopyBizo.CopyTemplateNode)),
						CopyMethod = CollectionCopyMethod.Filter
					};
				EntityCopyTemplateNode.GetEntityTemplateNodeFromTopLevelCopyTemplateNode(collectionCopyBizo.Parent.CopyTemplateNode).Nodes.Add(newCollectionCopyNode);

				var newCollectionCopyBizo = new CollectionCopyTemplateBizo(newCollectionCopyNode, ((CopyTemplateNodeBizo)((UniversalCopyTreeNode)Nodes[0]).BizO).CopyTemplateNode, collectionCopyBizo.Parent);
				newCollectionCopyBizo.IsSplitCollection = true;
				newCollectionCopyBizo.HasJustBeenSplet = true;
				newCollectionCopyBizo.ParentPropertyName = collectionCopyBizo.ParentPropertyName;
				collectionCopyBizo.Parent.ChildNodes.Add(newCollectionCopyBizo);

				if (collectionCopyBizo.CollectionId == null)
				{
					collectionCopyBizo.CollectionId = Guid.NewGuid().ToString();
				}

				newCollectionCopyBizo.SplitOwner = collectionCopyBizo.CollectionId;

				if (node.VisualNodeBizo != null || isUnfilteredRest)
				{
					SelectedNode = AddNode((isUnfilteredRest ? node.Parent.Nodes : node.Nodes), newCollectionCopyBizo);
				}
				else
				{
					var visualNode = CreateVisualNode(collectionCopyBizo);

					node.Parent.Nodes.Add(visualNode);
					node.Remove();

					collectionCopyBizo.IsUnfilteredRest = true;

					AddNode(visualNode.Nodes, collectionCopyBizo);
					SelectedNode = AddNode(visualNode.Nodes, newCollectionCopyBizo);
				}
			}
		}

		FilterStripBusinessObject GetCollectionNodeFilterBizo(UniversalCopyTreeNode node, CollectionCopyTemplateBizo bizo)
		{
			if (CopyManager == null)
			{
				return null;
			}

			if (bizo.FilterStripBizo is FilterStripBusinessObject filterStripBizo)
			{
				return filterStripBizo;
			}

			var filter = CopyManager.GetFilter(bizo.CopyTemplateNode, GetElementsNameFullPathToNode(node).Skip(1));
			bizo.FilterStripBizo = filter;
			return filter;
		}

		UniversalCopyTreeNode CreateVisualNode(CollectionCopyTemplateBizo collectionCopyBizo)
		{
			var visualNode = new UniversalCopyTreeNode(null, collectionCopyBizo.Description, GetBizoNodeImageIndex(collectionCopyBizo), ImageIndexError, ImageIndexMessageError, ImageIndexWarning, visualNodeBizo: collectionCopyBizo);
			visualNode.StateImageIndex = ImageIndexCollection;
			visualNode.ContextMenu =
				new ContextMenu(
					new MenuItem[]
					{
								new ZMenuItem(
									ResString.GetMultilingualString("697e97a4-c821-406f-ada2-ed6db36b6ddd", "Split collection for filtering"),
									(sender, e) => DuplicateCollectionNodeBizo(visualNode, false)),
								new ZMenuItem(
									ResString.GetMultilingualString("6ab5c5cd-fd78-410f-b93a-de7a8d50b0a0", "Remove all split collections"),
									(sender, e) => RemoveAllSplitCollections(visualNode))
					});
			return visualNode;
		}

		void RemoveSplitCollectionNodeBizo(UniversalCopyTreeNode node, bool skipCheck = false)
		{
			var collectionCopyBizo = node.BizO as CollectionCopyTemplateBizo;
			var parentBizo = ((UniversalCopyTreeNode)node.Parent.Nodes[node.Parent.Nodes.Count - 1]).BizO as CollectionCopyTemplateBizo;
			if (collectionCopyBizo != null &&
				(skipCheck ||
				Globals.Message.Show(
					Res.GetString("8084fa81-68d8-4600-9cc6-a487a56762ae", "Are you sure you want to remove split part {0} of collection {1}?", collectionCopyBizo.Description, collectionCopyBizo.Name),
					Res.GetString("be342d88-b1f8-4949-be35-c9fd5d49afd3", "Removing split collection"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation) == DialogResult.Yes))
			{
				collectionCopyBizo.CopyMethod = "";

				EntityCopyTemplateNode.GetEntityTemplateNodeFromTopLevelCopyTemplateNode(collectionCopyBizo.Parent.CopyTemplateNode).Nodes.Remove(collectionCopyBizo.CopyTemplateNode);
				collectionCopyBizo.Parent.ChildNodes.Remove(collectionCopyBizo);
				if (node.Parent.Nodes.Count <= 2)
				{
					parentBizo.IsUnfilteredRest = false;
					parentBizo.IsSplitCollection = false;
					var unfiltered = AddNode(node.Parent.Nodes, parentBizo);
					var changeSelectedNode = SelectedNode == node;
					var parent = node.Parent.Parent;
					node.Parent.Remove();
					parent.Nodes.Add(unfiltered);
					SelectedNode = changeSelectedNode ? unfiltered : SelectedNode;
				}
				else
				{
					SelectedNode = SelectedNode == node ? SelectedNode.Parent : SelectedNode;
					node.Parent.Nodes.Remove(node);
				}
			}
		}

		void RemoveAllSplitCollections(UniversalCopyTreeNode node)
		{
			if (Globals.Message.Show(
					Res.GetString("827ad366-4b79-4459-81dc-52e27a16a5f4", "Are you sure you want to remove all split collections from {0}?", node.Text),
					Res.GetString("73d78e8a-143d-4b54-9085-41e0b09f6db4", "Removing all split collection"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation) == DialogResult.Yes
					)
			{
				var nodesToDelete = new List<UniversalCopyTreeNode>();
				foreach (UniversalCopyTreeNode child in node.Nodes)
				{
					if (child.Text != unfilteredRest)
					{
						nodesToDelete.Add(child);
					}
				}
				foreach (var a in nodesToDelete)
				{
					RemoveSplitCollectionNodeBizo(a, skipCheck: true);
				}
			}
		}

		readonly ZString unfilteredRest = Res.GetString("bcfa0f97-8340-4bc7-87c9-3cd60694c161", "Unfiltered rest");

		#endregion
	}

	public class UniversalCopyTreeNode : ZBusinessObjectTreeNode
	{
		public BusinessObject VisualNodeBizo;

		public UniversalCopyTreeNode(BusinessObject bizO, string initialText, int imageIndex, int errorIndex, int messageErrorIndex, int warningIndex, BusinessObject visualNodeBizo = null)
			: base(bizO, initialText, imageIndex, errorIndex, messageErrorIndex, warningIndex)
		{
			VisualNodeBizo = visualNodeBizo;
			if (bizO is CopyTemplateNodeBizo templateNodeBizo)
			{
				templateNodeBizo.DescriptionInfo.ValueChanged += DescriptionInfo_ValueChanged;
			}
		}

		public UniversalCopyTreeNode(BusinessObject bizO, string initialText, BusinessObject visualNodeBizo = null)
			: this(bizO, initialText, DefaultImageIndex, DefaultErrorIndex, DefaultMessageErrorIndex, DefaultWarningText, visualNodeBizo)
		{
		}

		#region Sort split collection nodes

		void DescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			if (sender is CollectionCopyTemplateBizo collection && collection.IsSplitCollection)
			{
				var needInvoking = BizO is CopyTemplateNodeBizo templateNodeBizo && !templateNodeBizo.Description.EqualsIgnoringCase(Text); // Happens when editing a description and click crtl+S instead lost focus, then it will save before update the TreeNode text

				if (needInvoking && TreeView != null && !TreeView.IsDisposed && !TreeView.Disposing)
				{
					TreeView.BeginInvoke(new MethodInvoker(Reorder));
				}
				else
				{
					Reorder();
				}
			}
		}

		void Reorder()
		{
			if (TreeView == null || TreeView.IsDisposed || TreeView.Disposing || Parent == null)
			{
				return;
			}

			var parentNode = Parent;
			var newIndex = NewIndexByAlphabeticalOrder(parentNode);
			if (newIndex != Index)
			{
				TreeView.SuspendLayout();
				try
				{
					parentNode.Nodes.Remove(this);
					parentNode.Nodes.Insert(newIndex, this);
					TreeView.SelectedNode = this;
				}
				finally
				{
					TreeView.ResumeLayout();
				}
			}
		}

		int NewIndexByAlphabeticalOrder(TreeNode parentNode)
		{
			var orderList = parentNode.Nodes.Cast<TreeNode>().OrderBy(a => a.Text).ToList();
			return orderList.IndexOf(this);
		}

		#endregion

		#region Image Indexes

		const int DefaultImageIndex = 0;
		const int DefaultErrorIndex = 1;
		const int DefaultMessageErrorIndex = 2;
		const int DefaultWarningText = 3;

		#endregion
	}
}
