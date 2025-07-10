using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Enumeration;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI.AllocateEDocs
{
	[System.ComponentModel.DefaultBindingProperty("Path")]
	public partial class FileBrowserUserControl : ZUserControl
	{
		readonly IFileSystem fileSystem;

		public event EventHandler FileSelected;

		public ZString CurrentPath => CurrentDataItem as ZString? ?? ZString.Empty;
		public IEnumerable<string> SelectedFiles
		{
			get
			{
				var selectedFile = SelectedFile.ToString();
				var selectedElements = filesGrid.GetSelectedElements<FileBusinessObject>().Select(f => f.FullPath.ToString());

				return string.IsNullOrEmpty(selectedFile) ? selectedElements : selectedElements.Append(selectedFile).Distinct();
			}
		}
		public ZString SelectedFile
		{
			get
			{
				var row = filesGrid.CurrentRowIndex;
				if (row >= 0 && row < filesGrid.ListManager.List.Count)
				{
					var file = (FileBusinessObject)filesGrid.ListManager.List[filesGrid.CurrentRowIndex];
					return file?.FullPath ?? ZString.Empty;
				}
				return ZString.Empty;
			}
		}

		internal FileBrowserUserControl(IFileSystem fileSystem)
		{
			this.fileSystem = fileSystem;

			InitializeComponent();

			directoryTreeView.BeforeExpand += (o, e) => PopulateChildren(e.Node);
			directoryTreeView.NodeMouseClick += (o, e) =>
			{
				pathTextBox.Text = ((DirectoryBusinessObject)e.Node.Tag).FullPath;
				EndCurrentEdit();
			};
			
			filesGrid.CurrentCellChanged += (o, e) => OnFileSelected();
			filesGrid.RowsDeleted += (o, e) => OnFileSelected();
		}

		public FileBrowserUserControl()
			: this(new FileSystem(PreviewableDocumentHelper.SupportedFileFormats))
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DoWithFileSystemErrorHandling(CreateRootNodes);
		}

		void OnFileSelected()
		{
			FileSelected?.Invoke(this, EventArgs.Empty);
		}

		string formerPath = string.Empty;
		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (!string.IsNullOrEmpty(CurrentPath) && CurrentPath != formerPath)
			{
				formerPath = CurrentPath;

				DoWithFileSystemErrorHandling(() =>
				{
					using (new ZWaitCursorChanger(ParentForm))
					{
						SelectPath(CurrentPath);
					}

					OnFileSelected();
				});
			}

			base.OnCurrentDataItemChanged(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Placeholder string shouldn't be translated")]
		const string placeholderKey = "**placeholder**";

		void CreateRootNodes()
		{
			directoryTreeView.Nodes.Clear();
			if (!fileSystem.Roots.Any())
			{
				Globals.Message.ShowError(Res.GetString("0D1D9CC2-CE8B-4278-AF59-5FE3C37C4239", "The Remote Desktop connection is unable to access your local drives. Please ensure access is allowed to these drives."));
				return;
			}

			fileSystem.FetchDirectories(fileSystem.Roots.Select(dir => dir.FullPath.ToString()));

			foreach (var root in fileSystem.Roots)
			{
				directoryTreeView.Nodes.Add(CreateDirectoryNode(root));
			}
		}

		readonly Regex rdpClientsSharedDrivesPrefix = new Regex(@"^\\\\(ts)?client\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		void SelectPath(string path)
		{
			FetchEachSubdirectory(path);
			path = rdpClientsSharedDrivesPrefix.Replace(path, string.Empty);

			var parentCollection = directoryTreeView.Nodes;

			TreeNode node = null;
			foreach (var folder in path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
			{
				node = parentCollection[folder];
				if (node == null)
				{
					break;
				}

				PopulateChildren(node);
				node.Expand();
				parentCollection = node.Nodes;
			}

			if (node != null)
			{
				directoryTreeView.SelectedNode = node;
				filesGrid.SetDataBinding(GetDirectory(node), "Files");
			}
		}

		void FetchEachSubdirectory(string path)
		{
			IEnumerable<string> parts;
			try
			{
				parts = ZEnumerable.IterateUntil(path.TrimEnd('\\'), Path.GetDirectoryName, string.IsNullOrEmpty);
				fileSystem.FetchDirectories(parts);
			}
			catch (ArgumentException) { return; } // Probably a badly formed path
		}

		void PopulateChildren(TreeNode root)
		{
			DoWithFileSystemErrorHandling(() =>
			{
				var directories = GetDirectory(root).Directories;
				fileSystem.FetchDirectories(directories.Select(dir => dir.FullPath.ToString()));

				foreach (DirectoryBusinessObject childDirectory in directories)
				{
					if (root.Nodes[childDirectory.Name] == null)
					{
						root.Nodes.Add(CreateDirectoryNode(childDirectory));
					}
				}

				var nodesToRemove = root.Nodes.Cast<TreeNode>().Where(n => directories[n.Name] == null).ToList();
				foreach (var node in nodesToRemove)
				{
					node.Nodes.Remove(node);
				}
#if WINZOR
				root.Nodes.Remove(root.Nodes[placeholderKey]);
#endif
			});
		}

		DirectoryBusinessObject GetDirectory(TreeNode node)
			=> (DirectoryBusinessObject)node.Tag;

		TreeNode CreateDirectoryNode(DirectoryBusinessObject directory)
		{
			var childNode = new TreeNode { Tag = directory, Text = directory.Name, Name = directory.Name };
			if (directory.Directories.Any())
			{
				childNode.Nodes.Add(placeholderKey, placeholderKey);
			}
			return childNode;
		}

		public void AddFileMenuItem(ZMenuItem menuItem)
		{
			filesGrid.ContextMenu.MenuItems.Add(menuItem);
		}

		void DoWithFileSystemErrorHandling(Action m)
		{
			try
			{
				m();
			}
			catch (TimeoutException)
			{
				Globals.Message.ShowError(Res.GetString("9e5d958d-e374-45bc-806a-81bb8937b603", "There was a timeout when trying to access the file-system. Please check your network connection."));
			}
			catch (DirectoryNotFoundException)
			{
				Globals.Message.ShowError(Res.GetString("49ac2921-1d72-4953-a2bc-7d93decc64ca", "The directory can no longer be found."));
			}
			catch (UnauthorizedAccessException)
			{
				Globals.Message.ShowError(Res.GetString("3926e39c-5458-4c6c-bc03-64596c77d925", "You do not have permission to access this file."));
			}
			catch (IOException)
			{
				Globals.Message.ShowError(Res.GetString("e9787236-8cea-42d2-b729-7598950aa4bf", "An error occurred while attempting to access the directory."));
			}
		}
	}
}
