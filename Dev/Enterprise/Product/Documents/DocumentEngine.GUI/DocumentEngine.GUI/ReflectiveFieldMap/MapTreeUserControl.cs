using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public partial class MapTreeUserControl : ZUserControl
	{
		public class MapTreeNotNode
		{
			public MapTreeNotNode(MapTreeUserControl control, MapTreeNotNode parent, MemberDescription memberDescription)
			{
				this.control = control;
				this.parent = parent;
				this.memberDescription = memberDescription;
				this.text = memberDescription.GetFormattedTextLabel();
				this.name = (memberDescription as PropertyDescription)?.Property?.Name;

				if (parent == null)
				{
					control.rootNotNodes.Add(this);
				}

				if (memberDescription.CanHaveChildMembers())
				{
					canHaveChildren = true;
					needsToPopulateChildren = true;
				}
			}

			public void PopulateChildren()
			{
				if (needsToPopulateChildren)
				{
					needsToPopulateChildren = false;
					var reflector = memberDescription.DocDataReflector;
					reflector.ShowIndex = memberDescription.ShowIndex;
					foreach (var childMember in reflector.Members)
					{
						children.Add(new MapTreeNotNode(control, this, childMember));
					}
				}
			}

			public List<MapTreeNotNode> Children
			{
				get
				{
					PopulateChildren();
					return children;
				}
			}

			public MapTreeNode MapTreeNode
			{
				get
				{
					if (mapTreeNode != null)
					{
						return mapTreeNode;
					}

					//recursively go to our parent until we find one that has a mapTreeNode filled out, then tell it to LoadNodesIfNotAlreadyLoaded(),
					//then unwind the recursion calling LoadNodesIfNotAlreadyLoaded() along the way
					RecursivelyCallLoadNodesIfNotAlreadyLoadedOnParents(this.parent);

					if (mapTreeNode == null)
					{
						throw new System.InvalidOperationException("unreachable");
					}
					return mapTreeNode;
				}
			}

			static void RecursivelyCallLoadNodesIfNotAlreadyLoadedOnParents(MapTreeNotNode current)
			{
				if (current == null)
				{
					throw new System.InvalidOperationException("unreachable");
				}

				if (current.mapTreeNode != null)
				{
					current.mapTreeNode.LoadNodesIfNotAlreadyLoaded();
				}
				else if (current.parent != null)
				{
					RecursivelyCallLoadNodesIfNotAlreadyLoadedOnParents(current.parent);
					if (current.mapTreeNode != null)
					{
						current.mapTreeNode.LoadNodesIfNotAlreadyLoaded();
					}
					else
					{
						throw new System.InvalidOperationException("unreachable");
					}
				}
				else
				{
					throw new System.InvalidOperationException("unreachable");
				}
			}

			public readonly MapTreeUserControl control;
			public readonly MapTreeNotNode parent;
			readonly List<MapTreeNotNode> children = new List<MapTreeNotNode>();
			public readonly MemberDescription memberDescription;
			public bool canHaveChildren;
			public bool needsToPopulateChildren;
			public MapTreeNode mapTreeNode;
			public readonly string text;
			public readonly string name;
		}

		internal readonly List<MapTreeNotNode> rootNotNodes = new List<MapTreeNotNode>();
		readonly List<MapTreeNotNode> allNotNodes = new List<MapTreeNotNode>();

		internal List<MapTreeNotNode> AllNotNodes
		{
			get
			{
				LoadAllNotNodes();
				return allNotNodes;
			}
		}

		public void LoadAllNotNodes()
		{
			if (notNodesFullyLoaded)
			{
				return;
			}
			else
			{
				//An optimization I've un-done is that this can't return early whenever we find one of the search results the user is interested in, and pick up from where it left off next time.
				//But the code is way easier to write (in both here and MapTreeFindForm) like this.
				//Even with this un-done optimization everything is faster and snappier so it can be left for a future WI, imo.
				var seenNotNodeText = new HashSet<string>();
				IEnumerable<MapTreeNotNode> currentLayer = rootNotNodes;
				var nextLayer = new List<MapTreeNotNode>();
				for (var i = 0; i < 99; ++i)
				{
					foreach (var notNode in currentLayer)
					{
						allNotNodes.Add(notNode);
						if (seenNotNodeText.Add(notNode.text))
						{
							foreach (var child in notNode.Children)
							{
								nextLayer.Add(child);
							}
						}
					}
					if (i > 90)
					{
						ErrorReporter.ReportOnce("oh god that's a lot of layers");
					}
					if (nextLayer.Any())
					{
						currentLayer = nextLayer;
						nextLayer = new List<MapTreeNotNode>();
					}
					else
					{
						break;
					}
				}
				notNodesFullyLoaded = true;
			}
		}
		bool notNodesFullyLoaded;

		public MapTreeUserControl()
		{
			InitializeComponent();
			mapTreeView = mapFilterTreeView.DisplayTree;
			mapTreeView.BeforeExpand += mapTreeView_BeforeExpand;
			mapTreeView.AfterSelect += mapTreeView_AfterSelect;
			mapTreeView.ItemDrag += mapTreeView_ItemDrag;
			mapTreeView.NodeMouseDoubleClick += mapTreeView_NodeMouseDoubleClick;
			mapTreeView.HideSelection = false;
		}

		internal KTreeView mapTreeView;

		void mapTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			var node = e.Node as MapTreeNode;
			if (node != null && NodeDoubleClicked != null)
			{
				NodeDoubleClicked(node);
			}
		}
		public event NodeSelectEvent NodeDoubleClicked;

		void mapTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			var node = e.Item as MapTreeNode;
			if (node != null)
			{
				string macroText = node.MemberDescription.GetMacro();
				DoDragDrop(macroText, DragDropEffects.All);
			}
		}
		#region FindForm

		public void ShowFindForm()
		{
			if (!FindFormIsOpen)
			{
				currentFindForm = new MapTreeFindForm(mapTreeView, this);
				currentFindForm.Owner = FindForm();
				currentFindForm.Show();
			}
		}
		public void DisposeFindForm()
		{
			if (FindFormIsOpen)
			{
				currentFindForm.Dispose();
			}
		}

		public bool FindFormIsOpen
		{
			get { return currentFindForm != null && !currentFindForm.IsDisposed; }
		}
		MapTreeFindForm currentFindForm;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.F))
			{
				ShowFindForm();

				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion

		void mapTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (NodeSelected != null)
			{
				NodeSelected(e.Node as MapTreeNode);
			}
		}

		public delegate void NodeSelectEvent(MapTreeNode nodeSelected);

		public event NodeSelectEvent NodeSelected;

		public MemberDescription SelectedMember
		{
			get
			{
				MemberDescription selectedMember = null;
				if (mapTreeView.SelectedNode is MapTreeNode mapTreeNode)
				{
					selectedMember = mapTreeNode.MemberDescription;
				}
				return selectedMember;
			}
		}

		public void SetReflectors(IReadOnlyList<DocDataProviderReflector> reflectors, string[] roots = null)
		{
			void AddNode(TreeNodeCollection nodes, MemberDescription member)
			{
				var notNode = new MapTreeNotNode(this, null, member);
				nodes.Add(new MapTreeNode(notNode));
			}

			mapTreeView.Nodes.Clear();

			if (reflectors.Count == 0)
			{
				return;
			}

			if (reflectors.Count > 1)
			{
				var index = 0;
				foreach (var reflector in reflectors)
				{
					var node = new TreeNode();
					if (roots == null)
					{
						node = mapTreeView.Nodes.Add(reflector.TopLevelDataSourceInformation.SubstringSafe(0, reflector.TopLevelDataSourceInformation.IndexOf("\n")));
						node.Tag = reflector.DocDataProviderType;
					}
					else if (roots.Length > index)
					{
						node = mapTreeView.Nodes.Add(roots[index++]);
					}
					foreach (var member in reflector.Members)
					{
						AddNode(node.Nodes, member);
					}
				}
			}
			else
			{
				TreeNode node;
				if (roots?.Length > 0)
				{
					node = mapTreeView.Nodes.Add(roots[0]);
					foreach (var member in reflectors[0].Members)
					{
						AddNode(node.Nodes, member);
					}
				}
				else
				{
					foreach (var member in reflectors[0].Members)
					{
						AddNode(mapTreeView.Nodes, member);
					}
				}
			}
		}

		void mapTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			var mapTreeNode = e.Node as MapTreeNode;
			if (mapTreeNode != null)
			{
				mapTreeNode.LoadNodesIfNotAlreadyLoaded();
			}
		}

		public void ExpandTreeViewNode(int nodeIndex)
		{
			if (mapTreeView.Nodes.Count > nodeIndex)
			{
				mapTreeView.Nodes[nodeIndex].Expand();
			}
		}

		public TreeNode SelectedTreeNode
		{
			get { return mapTreeView.SelectedNode; }
			set { mapTreeView.SelectedNode = value; }
		}
	}
}
