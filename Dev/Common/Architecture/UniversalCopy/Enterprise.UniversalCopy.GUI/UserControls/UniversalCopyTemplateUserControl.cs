using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	partial class UniversalCopyTemplateUserControl : ZUserControl
	{
		public UniversalCopyTemplateUserControl()
		{
			InitializeComponent();

			templateTreeView.PathSeparator = "\\";
			templateTreeView.TreeViewNodeSorter = new NodeSorter();

			splitContainer2.Panel1Collapsed = true; // Hide preview functionality till it is completed

			SchemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
		}

		IApplicationSchemaResolver SchemaResolver { get; }

		public UniversalCopyManager CopyManager
		{
			get => copyManager;
			set
			{
				copyManager = value;
				templateTreeView.CopyManager = value;
			}
		}
		UniversalCopyManager copyManager;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				var template = BindingSource.Current as UniversalCopyTemplate;
				if (template != null)
				{
					templateTreeView.Bind(template.CopyTemplateTree);
					templateTreeView_AfterSelect(templateTreeView, new TreeViewEventArgs(templateTreeView.Nodes[0]));
				}
			}
			else
			{
				templateTreeView.Unbind();
			}
		}

		internal EntityNodeDetailsUserControl ElementDetails;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (lastPropertyCopyTemplateBizo != null)
				{
					lastPropertyCopyTemplateBizo.CopyMethodInfo.ValueChanged -= CopyMethodInfo_ValueChanged;
					lastPropertyCopyTemplateBizo.ValueInfo.ValueChanged -= CopyMethodInfo_ValueChanged;
					lastPropertyCopyTemplateBizo = null;
				}
			}
			base.Dispose(disposing);
		}

		void templateTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (ElementDetails != null)
			{
				ElementDetails.SelectedPropertyChanged -= ElementDetails_SelectedPropertyChanged;
				ElementDetails.SetDataBinding(null, "");
				ElementDetails.Dispose();
				ElementDetails = null;
			}

			var selectedBizo = ((UniversalCopyTreeNode)templateTreeView.SelectedNode).BizO;

			if (selectedBizo != null)
			{
				if (selectedBizo is CopyTemplateTreeBizo)
				{
					ElementDetails = new CopyTemplateDetailsUserControl(CopyManager);
				}
				else if (selectedBizo is RelatedEntityCopyTemplateBizo)
				{
					ElementDetails = new RelatedElementNodeDetailsUserControl(CopyManager);
				}
				else if (selectedBizo is CollectionCopyTemplateBizo)
				{
					ElementDetails = new CollectionNodeDetailsUserControl(CopyManager, ((CollectionCopyTemplateBizo)selectedBizo).IsSplitCollection);
					if (((CollectionCopyTemplateBizo)selectedBizo).IsSplitCollection)
					{
						ElementDetails.zTextBox1.Leave += ZTextBox1_Leave;
					}
				}
				else
				{
					ElementDetails = new EntityNodeDetailsUserControl(CopyManager);
				}

				ElementDetails.GetMacroRootTypesMethod = GetMacroRootTypes;
				ElementDetails.GetFilterStripBusinessObjectMethod = GetFilterStripBusinessObject;
				ElementDetails.GetPropertyListModuleIdMethod = GetPropertyListModuleId;
				ElementDetails.SetDataBinding(selectedBizo, "");

				ElementDetails.SelectedPropertyChanged += ElementDetails_SelectedPropertyChanged;

				ElementDetails.Dock = DockStyle.Fill;
				splitContainer2.Panel2.Controls.Add(ElementDetails);

				var collectionDetailsUserControl = ElementDetails as CollectionNodeDetailsUserControl;
				if (collectionDetailsUserControl != null)
				{
					var collectionBizo = (CollectionCopyTemplateBizo)selectedBizo;
					if (collectionBizo.HasJustBeenSplet)
					{
						((CollectionCopyTemplateBizo)selectedBizo).HasJustBeenSplet = false;
						collectionDetailsUserControl.tabControlDetails.SelectTab(collectionDetailsUserControl.tabPageCollectionFilter);
						collectionDetailsUserControl.tabPageCollectionFilter.Focus();
					}
				}

				var entityBizo = selectedBizo as EntityCopyTemplateBizo;
				if (entityBizo != null && ElementDetails.gridProperties.Visible)
				{
					var justAddedPropertyBizo = entityBizo.PropertyNodes.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(propertyBizo => propertyBizo.HasJustBeenAdded);
					if (justAddedPropertyBizo != null)
					{
						justAddedPropertyBizo.HasJustBeenAdded = false;
						ElementDetails.gridProperties.SelectSingleElement(justAddedPropertyBizo);
						ElementDetails.tabControlDetails.SelectTab(ElementDetails.tabPagePropertiesDetails);
					}

					entityBizo.PropertyNodes.Cast<PropertyCopyTemplateBizo>()
						.Where(pctb => pctb.ValueType == PropertyCopyTemplateBizo.GuidTypeName && pctb.ModuleId == null)
						.ForEach(pctb => pctb.ModuleId = GetPropertyListModuleId(pctb));
				}

				if (previewControl != null && panelPreview.Visible)
				{
					previewControl.SelectBoundControl(string.Join(".", templateTreeView.GetElementsNameFullPathToNode((UniversalCopyTreeNode)templateTreeView.SelectedNode)));
				}
			}
		}

		internal void ZTextBox1_Leave(object sender, EventArgs e)
		{
			var textBoxSender = sender as ZTextBox;
			if (textBoxSender != null)
			{
				templateTreeView.SelectedNode.Text = textBoxSender.Text;
			}
		}

		Type[] GetMacroRootTypes(object dataSource)
		{
			var rootTypes = new List<Type>();

			var node = templateTreeView.FindNode(dataSource);
			while (node != null)
			{
				var rootType = CopyManager != null ? CopyManager.GetComponentTypeFromPath(templateTreeView.GetElementsNameFullPathToNode(node).Skip(1), true) : null;
				if (rootType == null)
				{
					string tableName = null;
					CollectionCopyTemplateBizo collectionCopyTemplateBizo;
					RelatedEntityCopyTemplateBizo relatedEntityCopyTemplateBizo;
					if ((collectionCopyTemplateBizo = node.BizO as CollectionCopyTemplateBizo) != null)
					{
						tableName = collectionCopyTemplateBizo.CopyTemplateNode.ItemsTableName;
					}
					else if ((relatedEntityCopyTemplateBizo = node.BizO as RelatedEntityCopyTemplateBizo) != null)
					{
						tableName = relatedEntityCopyTemplateBizo.CopyTemplateNode.RelatedEntityTableName;
					}
					string tablePrefix = !string.IsNullOrEmpty(tableName) ? SchemaResolver.GetColumnNamePrefix(tableName) : null;
					rootType = !string.IsNullOrEmpty(tablePrefix) ? BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix, false) : null;
				}
				if (rootType != null && !rootTypes.Contains(rootType))
				{
					rootTypes.Add(rootType);
				}

				node = node.Parent as UniversalCopyTreeNode;
			}

			rootTypes.Reverse();
			return rootTypes.ToArray();
		}

		FilterStripBusinessObject GetFilterStripBusinessObject(object dataSource)
		{
			var nodeBizo = dataSource as EntityCopyTemplateBizo;
			if (nodeBizo != null)
			{
				if (nodeBizo.FilterStripBizo == null && CopyManager != null)
				{
					var node = templateTreeView.FindNode(dataSource);
					if (node != null)
					{
						nodeBizo.FilterStripBizo = CopyManager.GetFilter(nodeBizo.CopyTemplateNode, templateTreeView.GetElementsNameFullPathToNode(node).Skip(1));
					}
				}
				return (FilterStripBusinessObject)nodeBizo.FilterStripBizo;
			}
			return null;
		}

		public ModuleIdentifier GetPropertyListModuleId(PropertyCopyTemplateBizo propertyCopyTemplateBizo)
		{
			var nodeBizo = propertyCopyTemplateBizo.ParentEntity;
			if (nodeBizo != null && CopyManager != null)
			{
				var node = templateTreeView.FindNode(nodeBizo);
				if (node != null)
				{
					var moduleId = CopyManager.GetComponentPropertyListModuleIdFromPath(templateTreeView.GetElementsNameFullPathToNode(node).Skip(1), propertyCopyTemplateBizo.Name, out var collectionType);
					if (moduleId != ModuleIDs.NotAssigned && collectionType != null && propertyCopyTemplateBizo.ValuesList == null && BindingSource.Current is IBusiness business)
					{
						var collection = (IBusinessObjectCollection)Activator.CreateInstance(collectionType, business.Factory);
						propertyCopyTemplateBizo.ValuesList = collection;
					}
					return moduleId;
				}
			}

			return ModuleIDs.NotAssigned;
		}

		#region Preview Control

		PreviewUserControl previewControl;

		void LinkLabelShowPreviewClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (previewControl == null)
			{
				previewControl = new PreviewUserControl(CopyManager, BindingSource.Current as UniversalCopyTemplate) { Dock = DockStyle.Fill };
				panelPreview.Controls.Add(previewControl);
				previewControl.InitializePreviewControls();
				previewControl.SelectedControlChanged += OnPreviewControlSelectedControlChanged;
			}

			if (panelPreview.Visible)
			{
				panelPreview.Visible = false;
				splitContainer2.FixedPanel = FixedPanel.Panel1;
				splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24);
			}
			else
			{
				splitContainer2.SplitterDistance = splitContainer2.Height / 2;
				splitContainer2.FixedPanel = FixedPanel.None;
				panelPreview.Visible = true;
			}
		}

		void OnPreviewControlSelectedControlChanged(object sender, PreviewUserControl.SelectedControlChangedEventArgs e)
		{
			var path = e.NewPropertyPath.Split('.', '+');
			PropertyCopyTemplateBizo propertyNode;
			var node = FindNode(templateTreeView.Nodes, path, out propertyNode);
			if (node != null)
			{
				templateTreeView.SelectedNode = node;
			}
			if (propertyNode != null)
			{
				ElementDetails.SelectPropertyRow(propertyNode);
			}
		}

		UniversalCopyTreeNode FindNode(TreeNodeCollection nodes, IEnumerable<string> propertyPath, out PropertyCopyTemplateBizo propertyNode)
		{
			UniversalCopyTreeNode result = null;
			propertyNode = null;

			foreach (var propertyName in propertyPath)
			{
				bool found = false;
				foreach (var node in nodes.OfType<UniversalCopyTreeNode>())
				{
					var relatedEntity = node.BizO as RelatedEntityCopyTemplateBizo;
					if ((string.IsNullOrEmpty(node.Name) ? node.Text : node.Name) == propertyName ||
						(relatedEntity != null && relatedEntity.CopyTemplateNode != null && relatedEntity.CopyTemplateNode.RelatedPropertyName == propertyName))
					{
						found = true;
						result = node;
						nodes = node.Nodes;
						break;
					}
				}

				if (!found || nodes.Count == 0)
				{
					if (result != null)
					{
						var entityBizo = result.BizO as EntityCopyTemplateBizo;
						if (entityBizo != null && entityBizo.PropertyNodes.Count > 0)
						{
							propertyNode = entityBizo.PropertyNodes.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(node => node.Name == propertyName);
						}
					}

					break;
				}
			}

			return result;
		}

		void ElementDetails_SelectedPropertyChanged(object sender, EntityNodeDetailsUserControl.SelectedPropertyChangedEventArgs e)
		{
			if (previewControl != null && panelPreview.Visible)
			{
				previewControl.SelectBoundControl(string.Join(".", templateTreeView.GetElementsNameFullPathToNode((UniversalCopyTreeNode)templateTreeView.SelectedNode)) + "." + e.PropertyBizo.Name);

				if (lastPropertyCopyTemplateBizo != null)
				{
					lastPropertyCopyTemplateBizo.CopyMethodInfo.ValueChanged -= CopyMethodInfo_ValueChanged;
					lastPropertyCopyTemplateBizo.ValueInfo.ValueChanged -= CopyMethodInfo_ValueChanged;
				}
				lastPropertyCopyTemplateBizo = e.PropertyBizo;
				if (lastPropertyCopyTemplateBizo != null)
				{
					lastPropertyCopyTemplateBizo.CopyMethodInfo.ValueChanged += CopyMethodInfo_ValueChanged;
					lastPropertyCopyTemplateBizo.ValueInfo.ValueChanged += CopyMethodInfo_ValueChanged;
				}
			}

			if (e.PropertyBizo.ValueType == PropertyCopyTemplateBizo.GuidTypeName && e.PropertyBizo.ModuleId == null)
			{
				var module = GetPropertyListModuleId(e.PropertyBizo);
				if (module == null || module == ModuleIDs.NotAssigned)
				{
					e.PropertyBizo.CopyMethods.RemoveCode(PropertyCopyTemplateBizo.CopyMethodCodes.Value);
				}
				e.PropertyBizo.ModuleId = module;
			}
		}

		void CopyMethodInfo_ValueChanged(object sender, EventArgs e)
		{
			if (previewControl != null && panelPreview.Visible && lastPropertyCopyTemplateBizo != null)
			{
				var propertyPath = templateTreeView.GetElementsNameFullPathToNode((UniversalCopyTreeNode)templateTreeView.SelectedNode);
				propertyPath.Add(lastPropertyCopyTemplateBizo.Name);
				var control = previewControl.GetBoundControl(propertyPath);
				if (control != null)
				{
					previewControl.SetPropertyControlNotification(lastPropertyCopyTemplateBizo.CopyTemplateNode, control);
				}
			}
		}

		PropertyCopyTemplateBizo lastPropertyCopyTemplateBizo;

		#endregion
	}
}
