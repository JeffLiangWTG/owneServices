#if DEBUG

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Enumeration;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class InfoDiggerForm : ZChildForm
	{
#if !WINZOR
		readonly ControlInformationOverlayForm overlayForm;
#else
		readonly ControlInformationOverlayComponent overlayForm;
#endif
		public Control ControlToDig { get; }
		public Control SelectedControl => (Control)zTreeView1.SelectedNode?.Tag;

		[Obsolete("Required for designer otherwise infoDigger should only exist with a control to dig", false)]
		public InfoDiggerForm()
		{
			InitializeComponent();
		}

		public InfoDiggerForm(Control control)
		{
			ControlToDig = Argument.NotNull(control, nameof(control));
			overlayForm = ((KForm)control.FindForm()).Overlay;

			InitializeComponent();
			PopulateTreeNode();

			var treeViewSearcher = new NonRecursiveTreeViewSearcher();
			treeViewSearcher.BeforeIteratingChildren += (s, e) => Populate(e.Node);
			zTreeView1.TreeViewSearcher = treeViewSearcher;

			overlayForm.Disposed += (o, e) => Dispose();
		}

		void PopulateTreeNode()
		{
			var controls = ZEnumerable.Iterate(ControlToDig, c => c.Parent, null).Reverse().ToArray();
			var nodeForForm = CreateNodeForControl(controls.First());
			zTreeView1.Nodes.Add(nodeForForm);

			TreeNode node = null;
			var lastRoot = zTreeView1.Nodes;
			foreach (var controlNode in controls) //This is what expands the treeview to the node we are looking for
			{
				node = GetNodeForControl(lastRoot, controlNode);
				lastRoot = node.Nodes;

				Populate(lastRoot, controlNode);
				node.Expand();
			}

			zTreeView1.SelectedNode = node;
			Dig(SelectedControl);
		}

		void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			Dig((Control)e.Node.Tag);
		}

		Control lastControlDug;

		void Dig(Control control)
		{
			if (control != null && !control.IsDisposed && control != lastControlDug)
			{
				SetDataBinding(new ControlInformationDigger(control), "");
				overlayForm.CenterOnControl(control, this);
				lastControlDug = control;
			}
		}

		void OpenGuiSlnButton_Click(object sender, EventArgs e)
		{
			OpenSlnFromType(ParentClassFinder());
		}

		void OpenGuiSlnButton_Hover(object sender, EventArgs e)
		{
			UpdateStatusBar("Open the current GUI solution", null);
		}

		void OpenBusSlnButton_Click(object sender, EventArgs e)
		{
			var bindings = SelectedControl.DataBindings.Cast<Binding>().Where(binding => !binding.BindingMemberInfo.BindingMember.Contains("愛")).ToArray();
			if (bindings.Length > 0)
			{
				OpenSlnFromType(bindings[0].DataSource.GetType());
			}
			else
			{
				var current = SelectedControl.Parent;

				while (current != null)
				{
					if (current is ICompositeControlBindingSourceProvider bindingSource && bindingSource.BindingSource.DataSource != null)
					{
						OpenSlnFromType(bindingSource.BindingSource.DataSource.GetType());
						return;
					}
					current = current.Parent;
				}

				Globals.Message.Show("No data binding found for " + SelectedControl.Name + " or its parents.");
			}
		}

		void OpenBusSlnButton_Hover(object sender, EventArgs e)
		{
			UpdateStatusBar("Opens the Business solution for the selected binding in VS", null);
		}

		void OpenSlnFromType(Type type)
		{
			var fileUrl = SolutionFinder(type);

			if (!string.IsNullOrEmpty(fileUrl))
			{
				FileOpener.Open("vsnet:" + fileUrl);
			}
			else
			{
				Globals.Message.Show("Solution could not be loaded");
			}
		}

		Type ParentClassFinder()
		{
			var currentControl = SelectedControl.Parent;
			var type = currentControl.GetType();
			Type finalType = null;

			while (finalType == null)
			{
				var fileUrl = SolutionFinder(type);

				string line;

				if (fileUrl != null)
				{
					var file = new StreamReader(fileUrl);
					while ((line = file.ReadLine()) != null)
					{
						if (line.Contains(SelectedControl.Name))
						{
							finalType = type;
							break;
						}
					}
				}
				currentControl = currentControl.Parent;
				if (currentControl != null)
				{
					type = currentControl.GetType();
				}
				else
				{
					finalType = type;
					break;
				}
			}
			return finalType;
		}

		string SolutionFinder(Type type)
		{
			try
			{
				var readerParameters = new Mono.Cecil.ReaderParameters { ReadSymbols = true };
				var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(type.Assembly.Location, readerParameters);

				var fileUrl = string.Empty;

				var typeDefinition = assemblyDefinition.MainModule.GetType(type.FullName)
					?? assemblyDefinition.MainModule.GetType(type.BaseType.FullName);

				for (var i = 0; i < typeDefinition.Methods.Count; i++)
				{
					var methodDefinition = typeDefinition.Methods[i];

					if (methodDefinition.Body.Instructions.Count > 0)
					{
						foreach (var instruction in methodDefinition.Body.Instructions)
						{
							var sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(instruction);

							if (sequencePoint != null)
							{
								fileUrl = sequencePoint.Document.Url;
								return fileUrl;
							}
						}
					}

					if (!string.IsNullOrEmpty(fileUrl))
					{
						break;
					}
				}
				return null;
			}

			catch (FileNotFoundException)
			{
				return null;
			}
		}

		void TreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (e.Node.FirstNode.Text == tempNode)
			{
				e.Node.FirstNode.Remove();
				Populate(e.Node.Nodes, (Control)e.Node.Tag);
			}
		}

		void Populate(TreeNodeCollection root, Control c)
		{
			root.Clear();

			foreach (Control child in c.Controls)
			{
				root.Add(CreateNodeForControl(child));
			}
		}

		TreeNode CreateNodeForControl(Control c)
		{
			var nodeDisplayedName = (string.IsNullOrEmpty(c.Name) ? "<Unnamed>" : c.Name) + " - [" + c.GetType().Name + "]"; // NoTranslationNeeded
			var node = new TreeNode(nodeDisplayedName) { Tag = c };
			if (c.Controls.Count > 0)
			{
				node.Nodes.Add(new TreeNode(tempNode));
			}

			return node;
		}

		void Populate(TreeNode node)
			=> Populate(node.Nodes, (Control)node.Tag);

		readonly string tempNode = "Fake child";// NoTranslationNeeded

		TreeNode GetNodeForControl(TreeNodeCollection nodes, Control c)
			=> nodes.Cast<TreeNode>().First(n => n.Tag == c);

#if !WINZOR
		bool isActivating;

		protected override void OnDeactivate(EventArgs e)
		{
			if (!isActivating)
			{
				OverlayForm.Hide();
			}

			base.OnDeactivate(e);
		}
#endif

		protected override void OnActivated(EventArgs e)
		{
#if !WINZOR
			isActivating = true;
#endif
			Dig(SelectedControl ?? ControlToDig);
			base.OnActivated(e);
#if !WINZOR
			isActivating = false;
#endif
		}

		void InvalidateButton_Click(object sender, EventArgs e)
			=> SelectedControl?.Invalidate();

		void PerformLayoutButton_Click(object sender, EventArgs e)
			=> SelectedControl?.PerformLayout();
	}
	public class InfoDiggerProvider : IInfoDiggerProvider
	{
		public void ShowInfoDigger(Control controlToDig)
		{
			var infoDigger = new InfoDiggerForm(controlToDig);
			infoDigger.Show();
		}
	}
}

namespace Enterprise.ZArchitecture.GUI
{
	public partial class InfoDiggerForm
	{
		public void SelectNode(TreeNode n)
			=> n.TreeView.SelectedNode = n;

#if !WINZOR

		public ControlInformationOverlayForm OverlayForm => overlayForm;
#else
		public ControlInformationOverlayComponent OverlayForm => overlayForm;
#endif
	}
}
#endif
