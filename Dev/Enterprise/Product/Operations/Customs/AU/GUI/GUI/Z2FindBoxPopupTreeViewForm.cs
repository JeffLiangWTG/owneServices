using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class Z2FindBoxPopupTreeViewForm : KForm, IFindBoxPopup
	{
		private readonly BusinessObjectFactory factory;

		public Z2FindBoxPopupTreeViewForm()
		{
			if (!this.IsDesignMode())
			{
				factory = new BusinessObjectFactory();
			}
			InitializeComponent();
		}

		#region Events

		private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			oButtonOK.Enabled = !((IFamilyMember)e.Node.Tag).HasChildren;
		}

		#endregion

		protected virtual BusinessObjectCollection GetTopLevelCollection(BusinessObjectFactory factory)
		{
			return null;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadTopLevelCollection();
		}

		private void AddBusinessObjects(IFamilyMember bizObj, TreeNodeCollection nodes)
		{
			foreach (IFamilyMember child in bizObj.Children)
			{
				AddFamilyMember(child, nodes);
			}
		}

		private void AddFamilyMember(IFamilyMember bizObj, TreeNodeCollection nodes)
		{
			TreeNode node = nodes.Add(bizObj.ShortDescription);
			node.Tag = bizObj;
			if (bizObj.HasChildren)
			{
				node.Nodes.Add("Please wait - Loading");
			}
		}

		protected BusinessObject SelectedElement
		{
			get { return TreeView.SelectedNode == null ? null : (BusinessObject)TreeView.SelectedNode.Tag; }
		}

		private bool topLevelCollectionLoaded;

		public void LoadTopLevelCollection()
		{
			if (!DesignMode && !topLevelCollectionLoaded)
			{
				topLevelCollectionLoaded = true;
				foreach (IFamilyMember familyMember in GetTopLevelCollection(factory))
				{
					AddFamilyMember(familyMember, TreeView.Nodes);
				}
			}
		}

		private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			ShowContentsOfNode(e.Node);
		}

		protected void ShowContentsOfNode(TreeNode node)
		{
			if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Please wait - Loading")
			{
				node.Nodes[0].Remove();
				AddBusinessObjects((IFamilyMember)node.Tag, node.Nodes);
			}
		}

		private void oButtonOK_Click(object sender, EventArgs e)
		{
			AcceptSelection();
		}

		private void oButtonCancel_Click(object sender, EventArgs e)
		{
			CancelSelection();
		}

		protected virtual void NavigateToCode(string code)
		{
			throw new Exception("Pure Virtual (Abstract) call");
		}

		#region IFindBoxPopup Members

		private IFindBox findBox;

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
			// not necessary as can't add a new record to the TreeView
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			NavigateToCode(findBox.Code);

			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		protected virtual void AcceptSelection()
		{
			DialogResult = DialogResult.OK;
			if (SelectedElement != null)
			{
				findBox.Code = CodePropertyAttribute.CodeFromBusinessObject(SelectedElement);
				findBox.Description = DescriptionPropertyAttribute.DescriptionFromBusinessObject(SelectedElement);
			}
			Close();
		}

		protected virtual void CancelSelection()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}
