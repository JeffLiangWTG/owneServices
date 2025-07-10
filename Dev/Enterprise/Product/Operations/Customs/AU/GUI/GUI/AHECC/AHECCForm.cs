using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AHECCForm : Z2FindBoxPopupTreeViewForm
	{
		public AHECCForm()
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
					fAUCSectionCollection = new AUCSectionCollection(EntryType.Export);
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
				TreeNodeCollection currentNodes = TreeView.Nodes;
				if (currentNodes != null)
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						foreach (TreeNode childNode in currentNodes)
						{
							if (childNode.Tag == list[i])
							{
								childNode.Expand();
								ShowContentsOfNode(childNode);
								TreeView.SelectedNode = childNode;
								currentNodes = childNode.Nodes;
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
			if (e.Node.Tag is AUCAHECC)
			{
				zTextBoxUnitOfQuantity.Text = ((AUCAHECC)e.Node.Tag).UA_UQ;
			}
			else
			{
				zTextBoxUnitOfQuantity.Text = "";
			}
		}

		void AHECCForm_VisibleChanged(object sender, System.EventArgs e)
		{
			TreeView.Focus();
		}
	}
}
