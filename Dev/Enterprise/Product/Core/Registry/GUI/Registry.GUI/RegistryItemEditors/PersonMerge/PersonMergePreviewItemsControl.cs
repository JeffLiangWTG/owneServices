using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class PersonMergePreviewItemsControl : RegistryZUserControl
	{
		public PersonMergePreviewItemsControl()
		{
			InitializeComponent();
			PersonMergePreviewItemsGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PersonMergePreviewItemsGrid.ReadOnly = readOnly;
			MoveUpButton.ReadOnly = readOnly;
			MoveDownButton.ReadOnly = readOnly;
		}

		protected void MoveItem(bool moveUp)
		{
			if (!ReadOnly
				&& PersonMergePreviewItemsGrid.List.Count > 0
				&& PersonMergePreviewItemsGrid.SelectedRowCount <= 1
				&& (PersonMergePreviewItemsGrid.CurrentRowIndex != 0 || !moveUp)
				&& (PersonMergePreviewItemsGrid.CurrentRowIndex != PersonMergePreviewItemsGrid.List.Count - 1 || moveUp))
			{
				var oldIndex = PersonMergePreviewItemsGrid.CurrentRowIndex;
				var newIndex = moveUp ? oldIndex - 1 : oldIndex + 1;
				var temp = PersonMergePreviewItemsGrid.List[newIndex];
				PersonMergePreviewItemsGrid.List[newIndex] = PersonMergePreviewItemsGrid.ListManager.GetCurrent();
				PersonMergePreviewItemsGrid.List[oldIndex] = temp;
				PersonMergePreviewItemsGrid.CurrentRowIndex = newIndex;

				if (PersonMergePreviewItemsGrid.SelectedRowCount > 0)
				{
					PersonMergePreviewItemsGrid.UnSelect(oldIndex);
					PersonMergePreviewItemsGrid.Select(newIndex);
				}
			}
		}

		void UpButton_Click(object sender, System.EventArgs e)
		{
			MoveItem(moveUp: true);
		}

		void DownButton_Click(object sender, System.EventArgs e)
		{
			MoveItem(moveUp: false);
		}
	}
}
