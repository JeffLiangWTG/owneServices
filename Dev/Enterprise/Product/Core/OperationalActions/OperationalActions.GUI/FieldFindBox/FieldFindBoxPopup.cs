using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed partial class FieldFindBoxPopup : ZChildForm
	{
		const string ExpandNodeText = "<ExpandMe>";

		public static void Show(IFieldFindBox findbox, Form parentForm, string workflowType = null)
		{
			FieldFindBoxPopup popup = new FieldFindBoxPopup(findbox, workflowType);
			ZFormModaliser.Show(popup, parentForm);
		}

		public FieldFindBoxPopup(IFieldFindBox findbox, string workflowType = null)
			: base(new FieldModule())
		{
			this.findbox = findbox ?? throw new ArgumentNullException(nameof(findbox));
			this.workflowType = workflowType;
			this.generator = new OperationalActionFieldGenerator();
			this.module = (FieldModule)BusinessEntity;

			InitializeComponent();

			Setup();

			var treeViewSearcher = new RecursiveTreeViewSearcher();
			//treeViewSearcher.BeforeIteratingChildren += (s, e) => ExpandNodeIfNeeded(e.Node); //Will be needed in later implementation
			fieldTree.TreeViewSearcher = treeViewSearcher;
		}

		public override string FormHeading
		{
			get { return Res.GetString("0a560cfb-4692-441a-9538-8fb653b03d1d", "Field Selection"); }
		}

		#region Implementation

		static bool NeedsExpanding(TreeNode node)
		{
			return node.Nodes.Count == 1 && node.Nodes[0].Text == ExpandNodeText;
		}

		static int InfoComparison(PropertyInfo info1, PropertyInfo info2)
		{
			int result = string.CompareOrdinal(info1.Name, info2.Name);

			if (result == 0)
			{
				if (info1.DeclaringType == info2.DeclaringType)
				{
					result = 0;
				}
				else if (info1.DeclaringType.IsAssignableFrom(info2.DeclaringType))
				{
					result = 1;
				}
				else if (info2.DeclaringType.IsAssignableFrom(info1.DeclaringType))
				{
					result = -1;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			return result;
		}

		static TreeNode CreateNode(PropertyInfo info)
		{
			TreeNode result = new TreeNode();
			result.Text = info.Name;
			result.Tag = info;
			return result;
		}

		static TreeNode CreateNode(ICustomProperty property)
		{
			var result = new TreeNode();
			result.Text = property.Info?.GetCaption();
			result.Tag = property;
			return result;
		}

		static TreeNode CreateNodeWithExpander(PropertyInfo info)
		{
			TreeNode result = CreateNode(info);
			result.Nodes.Add(CreateExpanderNode());
			result.Collapse();

			return result;
		}

		static TreeNode CreateExpanderNode()
		{
			return new TreeNode(ExpandNodeText);
		}

		static TreeNode FindChild(TreeNodeCollection nodes, string name)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Text.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return node;
				}
			}

			return null;
		}

		static TreeNode FindChildByPrefix(TreeNodeCollection nodes, string prefix)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				{
					return node;
				}
			}

			return null;
		}

		static PropertyInfo[] GetInfoChain(TreeNode node)
		{
			PropertyInfo[] nodePath = new PropertyInfo[node.Level + 1];

			for (int i = node.Level; i >= 0; i--)
			{
				nodePath[i] = (PropertyInfo)node.Tag;
				node = node.Parent;
			}

			return nodePath;
		}

		void Setup()
		{
			fieldTree.SuspendLayout();
			try
			{
				fieldTree.Popup = this;
				fieldTree.Nodes.Clear();
				fieldTree.Nodes.AddRange(CreateNodes(findbox.RootType));
				fieldTree.SelectedNode = FindByPath(fieldTree.Nodes, findbox.Value ?? string.Empty);

				AddCustomFieldNodes();
			}
			finally
			{
				fieldTree.ResumeLayout();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		void AddCustomFieldNodes()
		{
			var customFields = CustomFieldHelper.GetCustomFields(workflowType);

			if (customFields.Any())
			{
				const string text = "Custom Fields";

				for (var i = 0; i <= fieldTree.Nodes.Count; i++)
				{
					if (i == fieldTree.Nodes.Count || string.CompareOrdinal(fieldTree.Nodes[i].Text, text) > 0)
					{
						var customFieldsNode = fieldTree.Nodes.Insert(i, text);
						customFields.ForEach(customField => customFieldsNode.Nodes.Add(CreateNode(customField)));
						return;
					}
				}
			}
		}

		internal void ExpandNodeIfNeeded(TreeNode node)
		{
			if (NeedsExpanding(node))
			{
				PropertyInfo info = (PropertyInfo)node.Tag;
				Type reflectType = ActionFieldFollowAttribute.GetReturnType(info);
				Type collectionType = null;

				if (typeof(IBusinessObjectCollection).IsAssignableFrom(reflectType))
				{
					collectionType = reflectType;
					reflectType = BusinessObjectCollection.GetElementTypeFromCollectionType(reflectType);
				}

				if (reflectType == null)
				{
					throw new ArgumentException("reflectType should not be null. info.name=" + info.Name);
				}

				node.Nodes.Clear();
				if (collectionType != null)
				{
					node.Nodes.AddRange(CreateNodes(collectionType));
				}

				node.Nodes.AddRange(CreateNodes(reflectType));
			}
		}

		void fieldTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			ExpandNodeIfNeeded(e.Node);
		}

		void fieldTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var node = fieldTree.SelectedNode;
			if (node == null || node.Tag == null)
			{
				module.SetInfoChain(null);
			}
			else if (node.Tag is PropertyInfo)
			{
				module.SetInfoChain(GetInfoChain(node));
			}
			else if (node.Tag is ICustomProperty)
			{
				module.SetProperty(node.Tag as ICustomProperty);
			}
		}

		void okButton_Click(object sender, EventArgs e)
		{
			if (fieldTree.SelectedNode != null)
			{
				findbox.Value = module.SelectedFieldName;
				Close();
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		TreeNode FindByPath(TreeNodeCollection nodes, string path)
		{
			string[] pathSegments = path.Split(new char[] { '.', '+' });

			TreeNode currentNode = null;
			TreeNodeCollection currentChildren = nodes;

			for (int i = 0; i < pathSegments.Length; i++)
			{
				if (string.IsNullOrEmpty(pathSegments[i]))
				{
					continue;
				}

				TreeNode nextNode;

				if (i + 1 == pathSegments.Length)
				{
					nextNode = FindChild(currentChildren, pathSegments[i]) ?? FindChildByPrefix(currentChildren, pathSegments[i]);
				}
				else
				{
					nextNode = FindChild(currentChildren, pathSegments[i]);
				}

				if (nextNode == null)
				{
					break;
				}

				ExpandNodeIfNeeded(nextNode);

				currentNode = nextNode;
				currentChildren = currentNode.Nodes;
			}

			return currentNode;
		}

		TreeNode[] CreateNodes(Type type)
		{
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			Array.Sort(properties, InfoComparison);

			List<TreeNode> result = new List<TreeNode>();
			string lastPropertyName = "";

			foreach (PropertyInfo info in properties)
			{
				if (lastPropertyName == info.Name)
				{
					continue;
				}

				lastPropertyName = info.Name;

				switch (ReflectionHelper.Classify(info))
				{
					case PropertyClassification.Updatable:
						OperationalActionFieldSupporter fieldSupporter = generator.CreateField(new PropertyInfo[] { info });

						if (fieldSupporter != null && (!fieldSupporter.ReadOnly || findbox.AllowReadOnly))
						{
							result.Add(CreateNode(info));
						}
						break;

					case PropertyClassification.FollowSingle:
					case PropertyClassification.FollowCollection:
					case PropertyClassification.FollowView:
						result.Add(CreateNodeWithExpander(info));
						break;
				}
			}

			return result.ToArray();
		}

		readonly FieldModule module;
		readonly IFieldFindBox findbox;
		readonly string workflowType;
		readonly OperationalActionFieldGenerator generator;

		#endregion
	}

	internal class FieldTreeView : ZTreeView, ITreeViewWithExpandingNodes
	{
		public FieldTreeView() : base() { }

		internal FieldFindBoxPopup Popup;

		public void ExpandNodeIfNeeded(TreeNode node)
		{
			Popup?.ExpandNodeIfNeeded(node);
		}
	}
}
