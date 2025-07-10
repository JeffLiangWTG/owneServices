using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCClassForm : Z2FindBoxPopupTreeViewForm
	{
		public AUCClassForm()
		{
			InitializeComponent();
		}

		AUCSectionCollection fAUCSectionCollection;
		public AUCSectionCollection AUCSectionCollection
		{
			get
			{
				if (fAUCSectionCollection == null)
				{
					fAUCSectionCollection = new AUCSectionCollection(EntryType.Import);
					fAUCSectionCollection.Load();
				}
				return fAUCSectionCollection;
			}
		}

		protected override BusinessObjectCollection GetTopLevelCollection(BusinessObjectFactory factory)
		{
			return AUCSectionCollection;
		}

		protected override void NavigateToCode(string code)
		{
			ITraversibleNode nearestNode = AUCSectionCollection.GetNearestNodeForCode(code);
			if (nearestNode != null)
			{
				ArrayList list = new ArrayList();
				list.AddRange(nearestNode.GetHierarchy());
				LoadTopLevelCollection();
				TreeNodeCollection nodesToSearch = TreeView.Nodes;
				if (nodesToSearch != null)
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						foreach (TreeNode node in nodesToSearch)
						{
							if (node.Tag == list[i])
							{
								node.Expand();
								ShowContentsOfNode(node);
								TreeView.SelectedNode = node;
								nodesToSearch = node.Nodes;
								break;
							}
						}
					}
				}
			}
		}

		void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			IFamilyMember familyMember = e.Node.Tag as IFamilyMember;
			if (familyMember != null)
			{
				zTextBoxFullDescription.Text = familyMember.LongDescription;
			}
			if (e.Node.Tag is AUCClass)
			{
				zTextBoxUnitOfQuantity.Text = ((AUCClass)e.Node.Tag).UJ_UQ1;
			}
			else
			{
				zTextBoxUnitOfQuantity.Text = "";
			}
		}

		void AUCClassForm_VisibleChanged(object sender, System.EventArgs e)
		{
			TreeView.Focus();
		}
	}
}
