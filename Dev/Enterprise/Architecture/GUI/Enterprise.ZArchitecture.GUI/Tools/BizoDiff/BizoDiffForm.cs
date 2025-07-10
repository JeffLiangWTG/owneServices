using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.DevTools
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
	public partial class BizoDiffForm : Form
	{
		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion

		public BizoDiffForm(IBusiness sourceBizObj)
		{
			InitializeComponent();

			var sourceBizObjPK = ((BusinessObject)sourceBizObj).PK;

			BizoType = sourceBizObj.GetType();
			SourceBizObj = Factory.Load(BizoType, sourceBizObjPK);
			ColumnProvider = new BizoDiffColumnProvider();

			BizoCheckAndComponentPropertyUpdate();
			UpdateTreeData(SourceBizObj, zTreeViewSource);
			UpdateTypeMatchingGrid(true, SourceBizObj);
			UpdatePropertyGrids(true);
		}

		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();
		readonly Type BizoType;
		readonly IBusiness SourceBizObj;
		readonly BizoDiffColumnProvider ColumnProvider;
		readonly Dictionary<Type, List<PropertyInfo>> TypeProperties = new Dictionary<Type, List<PropertyInfo>>();
		readonly Dictionary<Type, BizoDiffPropertyCollection> TypeMatchingCollections = new Dictionary<Type, BizoDiffPropertyCollection>();
		readonly StringBuilder LoadErrors = new StringBuilder();

		IBusiness TargetBizObj;

		void BizoCheckAndComponentPropertyUpdate()
		{
			var controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(BizoType);
			if (controller != null && controller.ModuleID != null)
			{
				zGuidFindBoxWithSelectedEvent1.ModuleID = controller.ModuleID;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("00a008fe-c92d-46f8-91b7-4b23747e6e83", "Cannot find Module Id."));
				return;
			}

			zGroupBoxSourceTree.Text = $"Source Bizo ({SourceBizObj.HumanReadableName})";
		}

		void UpdateTreeData(IBusiness bizObj, ZTreeView treeView)
		{
			treeView.BeginUpdate();

			treeView.Nodes.Clear();
			var topNode = new TreeNode(string.Empty);
			LoadErrors.Clear();
			BuildBizoTree(bizObj, topNode);
			topNode = topNode.Nodes[0];
			treeView.Nodes.Add(topNode);
			treeView.ExpandAll();
			treeView.PerformSelect(topNode);

			treeView.EndUpdate();

			if (LoadErrors.Length > 0)
			{
				Globals.Message.ShowError(LoadErrors.ToString());
			}
		}

		void BuildBizoTree(IBusiness bizObjOrCollection, TreeNode currentNode, int deep = 0, string propertyName = null)
		{
			if (bizObjOrCollection is IBusinessObjectCollection)
			{
				if (bizObjOrCollection.Count > 0)
				{
					currentNode = AddNode(bizObjOrCollection, currentNode, propertyName);
				}

				foreach (var child in bizObjOrCollection.Children)
				{
					if (child != null)
					{
						BuildBizoTree(child, currentNode, deep + 1);
					}
				}
			}
			else
			{
				currentNode = AddNode(bizObjOrCollection, currentNode, propertyName);

				var type = bizObjOrCollection.GetType();
				if (!TypeProperties.ContainsKey(type))
				{
					ColumnProvider.RegisterType((INeedRow)bizObjOrCollection);
					GetTypeProperties(type);
				}

				foreach (var property in TypeProperties[type])
				{
					try
					{
						var child = (IBusiness)property.GetValue(bizObjOrCollection);
						if (child != null && deep == 0)
						{
							BuildBizoTree(child, currentNode, deep + 1, property.Name);
						}
					}
					catch (Exception ex)
					{
						LoadErrors.AppendLine($"Unable to load property {type.Name}.{property.Name}  - {ex.Message}");
					}
				}
			}
		}

		void GetTypeProperties(Type type)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(v => v.PropertyType.GetInterfaces().Contains(typeof(IBusiness))).ToList();
			var uniqueProperties = properties.GroupBy(v => v.Name).Select(v =>
			{
				PropertyInfo property;
				if (v.Count() > 1)
				{
					property = v.FirstOrDefault(t => t.DeclaringType == type);
					var parentType = type.BaseType;
					while (parentType != null && property == null)
					{
						property = v.FirstOrDefault(t => t.DeclaringType == parentType);
						parentType = parentType.BaseType;
					}

					if (property == null)
					{
						return null;
					}
				}
				else
				{
					property = v.Single();
				}

				if (property.PropertyType.Name.StartsWith("Ref")
					|| property.PropertyType.IsSubclassOf(typeof(NonPersistentBusinessObject))
					|| property.PropertyType.GetInterfaces().Contains(typeof(INonPersistentBusinessObjectCollection)))
				{
					return null;
				}

				var method = property.GetGetMethod();
				if (method == null)
				{
					return null;
				}

				return property;
			}).Where(v => v != null).OrderBy(v => v.Name).ToList();

			TypeProperties.Add(type, uniqueProperties);
		}

		TreeNode AddNode(IBusiness bizObj, TreeNode nodeToAddTo, string propertyName)
		{
			var nodeName = string.IsNullOrEmpty(propertyName) ? bizObj.GetType().Name : $"{propertyName} - {bizObj.GetType().Name}";
			var node = new TreeNode(nodeName);
			node.ForeColor = Color.Black;
			node.Tag = bizObj;
			node.ContextMenu = new ContextMenu();
			if (bizObj is not IBusinessObjectCollection)
			{
				node.ContextMenu.MenuItems.Add(LoadChildNodesMenuCaption, (sender, e) =>
				{
					node.Nodes.Clear();
					LoadErrors.Clear();
					var topNode = new TreeNode(string.Empty);
					BuildBizoTree(bizObj, topNode);
					node.Nodes.AddRange(topNode.Nodes[0].Nodes.Cast<TreeNode>().ToArray());
					node.ExpandAll();

					if (LoadErrors.Length > 0)
					{
						Globals.Message.ShowError(LoadErrors.ToString());
					}

					DoCompare(null, EventArgs.Empty);
				});
			}
			node.ContextMenu.MenuItems.Add(ExcludeNodeFromCompareMenuCaption, (sender, e) =>
			{
				if (node.ForeColor != Color.Gray)
				{
					((MenuItem)sender).Text = IncludeNodeInCompareMenuCaption;
					node.ForeColor = Color.Gray;
					node.Collapse();
				}
				else
				{
					((MenuItem)sender).Text = ExcludeNodeFromCompareMenuCaption;
					node.ForeColor = Color.Black;
					node.ExpandAll();
				}

				DoCompare(null, EventArgs.Empty);
			});

			nodeToAddTo.Nodes.Add(node);

			return node;
		}

		void UpdatePropertyGrids(bool isBind)
		{
			var sourceBizo = zTreeViewSource.SelectedNode?.Tag as BusinessObject;
			var targetBizo = zTreeViewTarget.SelectedNode?.Tag as BusinessObject;
			var hideSameValue = zCheckBoxHideSameValuePropertiesWhenSameType.Checked;
			var dataSource = new BizoDiffPropertyGridDataSource(sourceBizo, targetBizo, ColumnProvider, hideSameValue);
			dataSource.BuildCollections();
			if (isBind)
			{
				zGridSource.SetDataBinding(dataSource.SourceProperties, "");
				zGridTarget.SetDataBinding(dataSource.TargetProperties, "");
			}
			else
			{
				zGridSource.DataSource = dataSource.SourceProperties;
				zGridTarget.DataSource = dataSource.TargetProperties;
			}
		}

		void zTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (sender is ZTreeView zTreeView && zTreeView?.SelectedNode != null)
			{
				zTreeView.SelectedNode.BackColor = Color.Transparent;
			}
		}

		void zTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			e.Node.BackColor = Color.LightGray;
			var bizoOrCollection = e.Node.Tag as IBusiness;
			UpdateTypeMatchingGrid(false, bizoOrCollection);
			UpdatePropertyGrids(false);
		}

		void zGuidFindBoxWithSelectedEvent1_Selected(object sender, GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			zTreeViewSource.BeginUpdate();
			zTreeViewTarget.BeginUpdate();

			var zGuid = new ZGuid(zGuidFindBoxWithSelectedEvent1.Guid);
			TargetBizObj = Factory.Load(BizoType, zGuid);
			if (TargetBizObj == null)
			{
				Globals.Message.ShowError(Res.GetString("00a008fe-c92d-46f8-91b7-4b23747e6e86", "Cannot find object to compare."));
				return;
			}

			UpdateTreeData(TargetBizObj, zTreeViewTarget);
			UpdatePropertyGrids(false);

			DoCompare(null, EventArgs.Empty);

			zTreeViewSource.EndUpdate();
			zTreeViewTarget.EndUpdate();
			zGroupBoxTargetTree.Text = $"Target Bizo ({TargetBizObj.HumanReadableName})";
		}

		void UpdateTypeMatchingGrid(bool isBind, IBusiness bizoOrCollection)
		{
			Type selectedType;
			if (bizoOrCollection is IBusinessObjectCollection)
			{
				var bizoCollection = bizoOrCollection as IBusinessObjectCollection;
				selectedType = bizoCollection?.Children.FirstOrDefault()?.GetType();
				if (selectedType == null)
				{
					return;
				}
			}
			else
			{
				selectedType = bizoOrCollection.GetType();
			}

			if (!TypeMatchingCollections.TryGetValue(selectedType, out var collection))
			{
				var list = new List<BizoProperty>();
				if (ColumnProvider.RegisteredTypes.TryGetValue(selectedType, out var fields))
				{
					list = fields.OrderBy(v => v).Select(v =>
					{
						var propertyRow = new BizoProperty(v, null);
						propertyRow.IsKeyFieldChanged += DoCompare;
						propertyRow.IsIgnoreFieldChanged += DoCompare;
						return propertyRow;
					}).ToList();
				}

				collection = [.. list];

				TypeMatchingCollections[selectedType] = collection;
			}

			zGroupBoxTypeMatch.Text = $"Matching Key Fields ({selectedType.Name})";
			if (isBind)
			{
				zGridTypeMatching.SetDataBinding(collection, "");
			}
			else
			{
				zGridTypeMatching.DataSource = collection;
			}
		}

		void DoCompare(object sender, EventArgs e)
		{
			if (TargetBizObj != null)
			{
				var typeKeyFieldsDictionary = TypeMatchingCollections.Where(bizoDiffPropertyCollection => bizoDiffPropertyCollection.Value.KeyFields.Count > 0)
					.ToDictionary(bizoDiffPropertyCollection => bizoDiffPropertyCollection.Key.Name, bizoDiffPropertyCollection => bizoDiffPropertyCollection.Value.KeyFields);
				var typeIgnoreFieldsDictionary = TypeMatchingCollections.Where(bizoDiffPropertyCollection => bizoDiffPropertyCollection.Value.IgnoreFields.Count > 0)
					.ToDictionary(bizoDiffPropertyCollection => bizoDiffPropertyCollection.Key, bizoDiffPropertyCollection => bizoDiffPropertyCollection.Value.IgnoreFields);
				var compare = new BizoTreeCompare(zTreeViewSource.Nodes[0], zTreeViewTarget.Nodes[0], ColumnProvider, typeKeyFieldsDictionary, typeIgnoreFieldsDictionary);
				compare.Compare();
			}
		}

		void zCheckBoxHideSameValuePropertiesWhenSameType_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePropertyGrids(false);
		}

		const string LoadChildNodesMenuCaption = "Load child nodes";
		const string ExcludeNodeFromCompareMenuCaption = "Exclude this node from compare";
		const string IncludeNodeInCompareMenuCaption = "Include this node in compare";
	}
}
