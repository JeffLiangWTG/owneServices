using System;
using System.ComponentModel;
using System.Text;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrderedListControl : ZUserControl
	{
		public OrderedListControl()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Value
		{
			get { return BuildValueString(); }
			set { PopulateListControl(value); }
		}

		#region Implementation

		void MoveUpButton_Click(object sender, System.EventArgs e)
		{
			MovePriority(-1);
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			MovePriority(1);
		}

		void DeleteItemButton_Click(object sender, EventArgs e)
		{
			int selectedItemIndex = ItemListBox.SelectedIndex;
			ItemListBox.Items.Remove(ItemListBox.SelectedItem);
			selectedItemIndex--;
			if (selectedItemIndex >= 0)
			{
				ItemListBox.SelectedIndex = selectedItemIndex;
			}
		}

		void AddItemButton_Click(object sender, EventArgs e)
		{
			string text = ItemValue.Text.Trim();

			if (String.IsNullOrEmpty(text))
			{
				return;
			}

			if (ItemListBox.Items.Contains(text))
			{
				MessageText.Text = Res.GetString("0d657c90-b39f-46ec-ac78-ded982caad33", "Value already exists in list");
				return;
			}
			else
			{
				MessageText.Text = string.Empty;
			}

			object selectedItem = ItemListBox.SelectedItem;
			int selectedItemIndex = ItemListBox.SelectedIndex;
			ItemListBox.Items.Insert(selectedItemIndex + 1, text);
			ItemListBox.SelectedIndex = selectedItemIndex + 1;
		}

		 void ItemListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateControls();
		}

		public const char ItemDelimiter = '|';

		#region Setting Priorities

		protected void PopulateListControl(string value)
		{
			ItemListBox.Items.Clear();
			string[] itemList = value.Split(new char[] { ItemDelimiter }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string text in itemList)
			{
				ItemListBox.Items.Add(text);
			}

			UpdateControls();
		}

		protected string BuildValueString()
		{
			StringBuilder builder = new StringBuilder();
			foreach (string text in ItemListBox.Items)
			{
				if (builder.Length != 0 )
				{
					builder.Append(ItemDelimiter);
				}

				builder.Append(text);
			}
			return builder.ToString();
		}

			#endregion

		protected void MovePriority(int increment)
		{
			object selectedItem = ItemListBox.SelectedItem;
			int selectedItemIndex = ItemListBox.SelectedIndex;

			if (selectedItemIndex + increment < 0)
			{
				return;
			}

			if (ItemListBox.SelectedItem == null)
			{
				return;
			}

			ItemListBox.Items.Remove(ItemListBox.SelectedItem);
			ItemListBox.Items.Insert(selectedItemIndex + increment, selectedItem);
			ItemListBox.SelectedIndex = selectedItemIndex + increment;
		}

		void UpdateControls()
		{
			if (ItemListBox.Items.Count == 0)
			{
				MoveUpButton.Enabled = false;
				MoveDownButton.Enabled = false;
				DeleteItemButton.Enabled = false;
			}
			else
			{
				MoveUpButton.Enabled = true;
				MoveDownButton.Enabled = true;
				DeleteItemButton.Enabled = true;
			}

			if (ItemListBox.SelectedIndex == 0)
			{
				MoveUpButton.Enabled = false;
			}

			if (ItemListBox.SelectedIndex == ItemListBox.Items.Count - 1)
			{
				MoveDownButton.Enabled = false;
			}
		}

		#endregion
	}
}
