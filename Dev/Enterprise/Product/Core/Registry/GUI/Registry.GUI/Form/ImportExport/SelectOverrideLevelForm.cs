using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SelectOverrideLevelForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SelectOverrideLevelForm()
		{
			InitializeComponent();
		}

		public SelectOverrideLevelForm(BusinessObjectFactory factory, bool hideInactiveChildren = false)
			: base(null)
		{
			InitializeComponent();

			RegistryItemTreeViewBuilder.AddFallbackNodes(treeView.Nodes, factory, addDefaultLevel: false, hideInactiveFallbacks: hideInactiveChildren);
		}

		public IOverrideLevel SelectedOveride { get { return treeView.SelectedNode != null ? treeView.SelectedNode.Tag as IOverrideLevel : null; } }

		void compareButton_Click(object sender, EventArgs e)
		{
			if (SelectedOveride != null)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("67923452-94E6-4841-B9AD-B350ECFA1909", "Please select a valid level"));
			}
		}
	}
}
