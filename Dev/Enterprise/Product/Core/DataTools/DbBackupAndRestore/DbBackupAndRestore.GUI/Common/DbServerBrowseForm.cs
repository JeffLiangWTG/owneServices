using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	partial class DbServerBrowseForm : Form
	{
		public DbServerBrowseForm()
		{
			InitializeComponent();
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public DbServerBrowseForm(DbServerBrowser.BrowseResult browseResult, string dbServer, bool isFolderOnlyBrowse)
			: this()
		{
			this.DeferredLoadTreeDelegate = new DeferredStartDelegate(DeferredLoadTree);
			this.DbServer = dbServer;
			this.IsFolderOnlyBrowse = isFolderOnlyBrowse;
			SelectedPathTextBox.DataBindings.Add("Text", browseResult, "FullPath", false, DataSourceUpdateMode.OnPropertyChanged);

			InitialiseFilesOfTypeComboBox();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		delegate void DeferredStartDelegate();

		readonly DeferredStartDelegate DeferredLoadTreeDelegate;
		readonly bool IsFolderOnlyBrowse;
		readonly string DbServer;

		DbServerBrowser DirectoryBrowser
		{
			get
			{
				if (fDirectoryBrowser == null)
				{
					fDirectoryBrowser = new DbServerBrowser(DbServer, IsFolderOnlyBrowse);
				}

				return fDirectoryBrowser;
			}
		}

		DbServerBrowser fDirectoryBrowser;

		void InitialiseFilesOfTypeComboBox()
		{
			if (IsFolderOnlyBrowse)
			{
				FilesOfTypeComboBox.Enabled = false;
			}
			else
			{
				FilesOfTypeComboBox.Items.Add("Backup Files (*.bak)");
				FilesOfTypeComboBox.SelectedIndex = 0;
				FilesOfTypeComboBox.Items.Add("All Files (*)");
			}
		}

		/// <summary>
		/// Starts loading the directory tree
		/// Note: This method call is queued as to run after the form is completely loaded.
		/// </summary>
		void DeferredLoadTree()
		{
			InitialiseTree();
			this.DirectoryTreeView.BeforeExpand += new TreeViewCancelEventHandler(this.DirectoryTreeView_BeforeExpand);
			this.DirectoryTreeView.AfterSelect += new TreeViewEventHandler(this.DirectoryTreeView_AfterSelect);
			this.DirectoryTreeView.DoubleClick += new EventHandler(DirectoryTreeView_DoubleClick);
		}

		void InitialiseTree()
		{
			TreeNode rootNodeToBeExpanded = null;
			StringCollection fixedDrives = GetServerFixedDrives();

			foreach (string driveLetter in fixedDrives)
			{
				TreeNode node = DirectoryTreeView.Nodes.Add(driveLetter + @"\", driveLetter, 0);
				node.Nodes.Add("");

				if (SelectedPathTextBox.Text.StartsWith(node.Name))
				{
					rootNodeToBeExpanded = node;
				}
			}

			ExpandTreeToInitialPath(rootNodeToBeExpanded, SelectedPathTextBox.Text.Trim());
		}

		StringCollection GetServerFixedDrives()
		{
			StringCollection result = new StringCollection();

			try
			{
				result = DirectoryBrowser.FixedDrives;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string text = String.Format(
					"There was an error trying to get the list of fixed drives:\r\n  Server - {0}\r\n\r\n{1}",
					DbServer, ex.Message);
				ShowErrorDialog(text, "Error getting server fixed drives");
			}

			return result;
		}

		void ExpandTreeToInitialPath(TreeNode node, string initialPath)
		{
			if (node != null)
			{
				string[] pathParts = initialPath.Split('\\');

				TreeNode currentNode = node;
				TreeNode nextNode = null;

				for (int i = 1; i < pathParts.Length; i++)
				{
					nextNode = null;

					if (currentNode.Nodes.Count > 0)
					{
						ExpandNode(currentNode);

						foreach (TreeNode childNode in currentNode.Nodes)
						{
							if (childNode.Text == pathParts[i])
							{
								nextNode = childNode;
								break;
							}
						}
					}

					if (nextNode == null)
					{
						break;
					}
					else
					{
						currentNode = nextNode;
					}
				}

				DirectoryTreeView.SelectedNode = currentNode;
				SelectedPathTextBox.Text = currentNode.Name;
			}
		}

		void ExpandNode(TreeNode node)
		{
			node.Nodes.Clear();
			DirectoryEntryCollection dirEntries = GetServerDirectoryList(node.Name);

			foreach (DirectoryEntry dirEntry in dirEntries)
			{
				int imageIndex = (int)dirEntry.EntryType;
				TreeNode childNode = node.Nodes.Add(dirEntry.FullPath, dirEntry.Name, imageIndex, imageIndex);

				if (dirEntry.EntryType == DirectoryEntryType.Folder)
				{
					childNode.Nodes.Add("");
				}
			}
		}

		DirectoryEntryCollection GetServerDirectoryList(string directoryPath)
		{
			DirectoryEntryCollection result = new DirectoryEntryCollection();

			try
			{
				result = DirectoryBrowser.GetDirectoryList(directoryPath);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string text = String.Format(
					"There was an error trying to get the directory list of:\r\n  Server - {0}\r\n  Path - {1}\r\n\r\n{2}",
					DbServer, directoryPath, ex.Message);
				ShowErrorDialog(text, "Error getting directory list");
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Cannot use Globals.Show as it hits the database")]
		void ShowErrorDialog(string text, string caption)
		{
			MessageBox.Show(text, caption, MessageBoxButtons.OK);
		}

#if DEBUG
		internal ImageList DirectoryTreeViewImageListForTesting
		{
			get
			{
				if (!ZArchitecture.Environment.Globals.IsTest)
				{
					throw new InvalidOperationException("FOR UNIT TESTS ONLY!");
				}

				return DirectoryTreeView.ImageList;
			}
		}
#endif

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			Control currentControl = ActiveControl;
			SelectedPathTextBox.Focus();
			currentControl.Focus();
		}

		/// <summary>
		/// Queue the call to DeferredStartLoadTree to run after the form is completely loaded.
		/// </summary>
		void DbServerBrowseForm_Load(object sender, EventArgs e)
		{
			if (DeferredLoadTreeDelegate != null)
			{
				this.BeginInvoke(DeferredLoadTreeDelegate);
			}
		}

		void DirectoryTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node != null)
			{
				ExpandNode(e.Node);
			}
		}

		void DirectoryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node != null)
			{
				SelectedPathTextBox.Text = e.Node.Name;
			}
		}

		void DirectoryTreeView_DoubleClick(object sender, EventArgs e)
		{
			if (!IsFolderOnlyBrowse && DirectoryTreeView.SelectedNode != null)
			{
				if (DirectoryTreeView.SelectedNode.ImageIndex == (int)DirectoryEntryType.File && DirectoryTreeView.SelectedNode.Name == SelectedPathTextBox.Text)
				{
					this.DialogResult = DialogResult.OK;
					this.Close();
				}
			}
		}
	}
}
