using System.Collections;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SalesRelationDirectionRuleControl : ZUserControl
	{
		public SalesRelationDirectionRuleControl()
		{
			InitializeComponent();
		}

		public void SetReadOnly(bool value)
		{
			moveUpButton.ReadOnly = value;
			moveDownButton.ReadOnly = value;
			nodeGrid.ReadOnly = value;
		}

		void moveUpButton_Click(object sender, System.EventArgs e)
		{
			var selectedBizObj = nodeGrid.ListManager.GetCurrent();

			if (selectedBizObj != null)
			{
				var collection = (IList)(((SalesRelationDirectionRule)CurrentDataItem).Nodes);
				var selectedIndex = collection.IndexOf(selectedBizObj);
				var indexToInsert = selectedIndex - 1;
				if (selectedIndex >= 0 && indexToInsert >= 0)
				{
					collection.RemoveAt(selectedIndex);
					collection.Insert(indexToInsert, selectedBizObj);

					nodeGrid.ListManager.Position = indexToInsert;
					nodeGrid.Select(indexToInsert);
				}
			}
		}

		void moveDownButton_Click(object sender, System.EventArgs e)
		{
			var selectedBizObj = nodeGrid.ListManager.GetCurrent();

			if (selectedBizObj != null)
			{
				var collection = (IList)(((SalesRelationDirectionRule)CurrentDataItem).Nodes);
				var selectedIndex = collection.IndexOf(selectedBizObj);
				var indexToInsert = selectedIndex + 1;
				if (selectedIndex >= 0 && indexToInsert < collection.Count)
				{
					collection.RemoveAt(selectedIndex);
					collection.Insert(indexToInsert, selectedBizObj);

					nodeGrid.ListManager.Position = indexToInsert;
					nodeGrid.Select(indexToInsert);
				}
			}
		}
	}
}
