using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	partial class CustomNoteTypesControl : RegistryBusinessObjectTemplateZUserControl
	{
		public CustomNoteTypesControl()
		{
			InitializeComponent();
			ModuleAndCountryTreeView.AfterSelect += new TreeViewEventHandler(ModuleAndCountryTreeView_AfterSelect);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ModuleAndCountryTreeView.Enabled = !readOnly;
		}

		#region Overrides

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ModuleAndCountryTreeView.Populate(CurrentDataItem.SelectedModule.CountriesOfAllCompanies);
				CurrentDataItem.SelectedModule.CustomNoteTypesList.SetReadOnlyIncludingChildren(true);
			}
		}

		#endregion

		#region BusinessEntity

		public new CustomNoteTypes CurrentDataItem
		{
			get { return (CustomNoteTypes)base.CurrentDataItem; }
		}

		#endregion

		#region Selecting a Node

		protected void ModuleAndCountryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (CurrentDataItem != null)
			{
				ZModulePointNode node = (ZModulePointNode)e.Node;
				CurrentDataItem.CurrentSelectedCountryCode = node.CountryCode;

				if (node.ModuleID != null)
				{
					CurrentDataItem.CurrentSelectedModuleID = node.ModuleID.Name;
					CurrentDataItem.SelectedModule.CustomNoteTypesList.SetReadOnlyIncludingChildren(false);
				}
				else
				{
					CurrentDataItem.CurrentSelectedModuleID = "";
					CurrentDataItem.SelectedModule.CustomNoteTypesList.SetReadOnlyIncludingChildren(true);
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			ModuleAndCountryTreeView.AfterSelect -= new TreeViewEventHandler(ModuleAndCountryTreeView_AfterSelect);
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
