using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class ClientInTemplateSelectionControl : RegistryZUserControl
	{
		public ClientInTemplateSelectionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (Control control in this.Controls)
			{
				control.Enabled = !readOnly;
			}
		}

		internal void AddOrgTypeButton_Click(object sender, EventArgs e)
		{
			foreach (var element in ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.SelectedElements)
			{
				ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.Add(element);
				ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.List.Remove(element);
			}
		}

		internal void RemoveOrgTypeButon_Click(object sender, EventArgs e)
		{
			foreach (var element in ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements)
			{
				ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.List.Add(element);
				ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.Remove(element);
			}
		}

		internal void MoveUpButton_Click(object sender, EventArgs e)
		{
			if (ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements.Length > 0)
			{
				var element = (ClientInTemplateSelectionCriteriaOrgType)ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements[0];
				var list = (ClientInTemplateSelectionCriteriaOrgTypesCollection)ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List;

				var index = ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.IndexOf(element);
				if (index > 0)
				{
					list.MoveItem(index, index - 1);
					ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.UnSelectAll();
					ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(index - 1);
				}
			}
		}

		internal void MoveDownButton_Click(object sender, EventArgs e)
		{
			if (ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements.Length > 0)
			{
				var element = (ClientInTemplateSelectionCriteriaOrgType)ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements[0];
				var list = (ClientInTemplateSelectionCriteriaOrgTypesCollection)ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List;

				var index = ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.IndexOf(element);
				if (index < ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.Count - 1)
				{
					list.MoveItem(index, index + 1);
					ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.UnSelectAll();
					ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(index + 1);
				}
			}
		}

		internal void ResetButton_Click(object sender, EventArgs e)
		{
			var source = (ClientInTemplateSelectionCriteriaOrgTypesCollection)ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List;

			if (source.Parent != null)
			{
				source.Parent.Reset();
			}
		}
	}
}
