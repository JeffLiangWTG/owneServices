using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public partial class OUPickerControl : ZUserControl
	{
		public OUPickerControl()
		{
			selectedOU = "";
			InitializeComponent();
			DirectoryTreeView.HideSelection = false;
			DirectoryTreeView.AfterSelect += new TreeViewEventHandler(DirectoryTreeView_AfterSelect);
		}

		public void FindAndSelectOU(string rootOU)
		{
			if (DirectoryTreeView.Nodes.Count > 0)
			{
				FindAndSelectNode(DirectoryTreeView.Nodes[0], rootOU);
			}
		}

		public string SelectedOU
		{
			get { return selectedOU; }
			set
			{
				if (SelectedOU != value)
				{
					selectedOU = value;
					SelectedOUTextBox.Text = value;
					if (SelectedOUChanged != null)
					{
						SelectedOUChanged(this, new SelectedOUChangedEventArgs { RootOU = value });
					}
					(FindForm() as RegistryForm)?.UpdateHasChanges();
				}
			}
		}
		string selectedOU;

		public event SelectedOUChangedEventHandler SelectedOUChanged;

		void DirectoryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag != null)
			{
				SelectedOU = e.Node.Tag.ToString();
			}
		}

#if DEBUG
		public void OnDirectoryTreeView_AfterSelect(TreeNode node)
		{
			DirectoryTreeView_AfterSelect(this, new TreeViewEventArgs(node));
		}
#endif

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Scope is restricted to known issue.")]
		public void PopulateTree(IDirectorySearcher directorySearcher, string domainName)
		{
			Argument.NotNullOrEmpty(domainName, "domainName");

			try
			{
				var tree = new OUTree(domainName, directorySearcher);
				PopulateTree(tree);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ReportPopulateTreeError(ex);
			}
		}

		void PopulateTree(OUTree tree)
		{
			DirectoryTreeView.Nodes.Clear();
			DirectoryTreeView.BeginUpdate();

			PopulateTreeFrom(tree.Root, null);

			if (DirectoryTreeView.Nodes.Count > 0)
			{
				DirectoryTreeView.Nodes[0].Expand();
			}
			DirectoryTreeView.EndUpdate();
		}

		void PopulateTreeFrom(INode root, TreeNode parent)
		{
			var node = AddNode(root.Name, parent);
			node.Tag = root.Path;

			foreach (var child in root.Children)
			{
				PopulateTreeFrom(child, node);
			}
		}

		TreeNode AddNode(string nodeText, TreeNode parent = null)
		{
			if (parent == null)
			{
				return DirectoryTreeView.Nodes.Add(nodeText);
			}
			else
			{
				return parent.Nodes.Add(nodeText);
			}
		}

		void FindAndSelectNode(TreeNode root, string targetOU)
		{
			if (string.Equals((string)root.Tag, targetOU, StringComparison.OrdinalIgnoreCase))
			{
				DirectoryTreeView.SelectedNode = root;
				DirectoryTreeView.Focus();
			}
			else
			{
				foreach (TreeNode node in root.Nodes)
				{
					FindAndSelectNode(node, targetOU);
				}
			}
		}

		void ReportPopulateTreeError(Exception ex)
		{
			DirectoryTreeView.Nodes.Clear();
			AddNode(Res.GetString("83bdb01c-f1f9-44c8-875d-52ab53424985", "An error occurred when getting the OU tree: {0}", ex.Message));
			if (!DirectoryExceptionHandler.TryHandleDirectoryException(ex))
			{
				ErrorReporter.ReportOnce("OUPickerControl.PopulateTreeAsync", "An error occurred when getting the OU tree", ex);
			}
		}
	}

	public delegate void SelectedOUChangedEventHandler(object sender, SelectedOUChangedEventArgs e);
	public class SelectedOUChangedEventArgs : EventArgs
	{
		public string RootOU { get; set; }
	}
}
