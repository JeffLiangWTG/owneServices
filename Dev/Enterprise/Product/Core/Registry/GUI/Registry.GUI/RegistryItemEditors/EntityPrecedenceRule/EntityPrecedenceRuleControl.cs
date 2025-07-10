using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class EntityPrecedenceRuleControl : RegistryZUserControl
	{
		public EntityPrecedenceRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(ReadOnly);

			foreach (Control control in Controls)
			{
				control.Enabled = !readOnly;
			}
		}

		internal void AddItemButton_Click(object sender, EventArgs e)
		{
			foreach (var element in AvailableItemsGrid.SelectedElements)
			{
				SelectedItemsGrid.List.Add(element);
				AvailableItemsGrid.List.Remove(element);
			}
		}

		internal void RemoveItemButon_Click(object sender, EventArgs e)
		{
			foreach (var element in SelectedItemsGrid.SelectedElements)
			{
				AvailableItemsGrid.List.Add(element);
				SelectedItemsGrid.List.Remove(element);
			}
		}

		internal void MoveUpButton_Click(object sender, EventArgs e)
		{
			if (SelectedItemsGrid.SelectedElements.Length > 0)
			{
				var element = (EntityPrecedenceRuleItem)SelectedItemsGrid.SelectedElements[0];
				var list = (EntityPrecedenceRuleItemCollection)SelectedItemsGrid.List;

				var index = SelectedItemsGrid.List.IndexOf(element);
				if (index > 0)
				{
					list.MoveItem(index, index - 1);
					SelectedItemsGrid.UnSelectAll();
					SelectedItemsGrid.Select(index - 1);
				}
			}
		}

		internal void MoveDownButton_Click(object sender, EventArgs e)
		{
			if (SelectedItemsGrid.SelectedElements.Length > 0)
			{
				var element = (EntityPrecedenceRuleItem)SelectedItemsGrid.SelectedElements[0];
				var list = (EntityPrecedenceRuleItemCollection)SelectedItemsGrid.List;

				var index = SelectedItemsGrid.List.IndexOf(element);
				if (index < SelectedItemsGrid.List.Count - 1)
				{
					list.MoveItem(index, index + 1);
					SelectedItemsGrid.UnSelectAll();
					SelectedItemsGrid.Select(index + 1);
				}
			}
		}

		internal void ResetButton_Click(object sender, EventArgs e)
		{
			var source = (EntityPrecedenceRule)DataSource;
			source.ResetItems();
		}
	}
}
