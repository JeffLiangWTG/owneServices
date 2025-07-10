using System.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	sealed class MockDetailObjectCollection : ComponentModel.Testing.KBindingList<MockDetailObject>
	{
		public MockDetailObjectCollection(MockMasterObject parent)
		{
			this.Parent = parent;
		}

		public MockMasterObject Parent { get; private set; }

		protected override void OnAddingNew(AddingNewEventArgs e)
		{
			base.OnAddingNew(e);
			MockDetailObject newObject = (MockDetailObject)e.NewObject;
			if (newObject != null)
			{
				newObject.Parent = Parent;
			}
		}

		protected override void InsertItem(int index, MockDetailObject item)
		{
			item.Parent = Parent;
			base.InsertItem(index, item);
		}

		protected override object AddNewCore()
		{
			MockDetailObject result = (MockDetailObject)base.AddNewCore();
			result.Parent = Parent;
			return result;
		}
	}
}
