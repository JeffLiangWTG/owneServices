using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	sealed class MockMasterObjectCollection : ComponentModel.Testing.KBindingList<MockMasterObject>, IDataSourceEvents
	{
		public event EventHandler DataSourcePosting;

		public void FireListChanged(ListChangedEventArgs e)
		{
			OnListChanged(e);
		}

		public void FireDataSourcePosting()
		{
			if (DataSourcePosting != null)
			{
				DataSourcePosting(this, EventArgs.Empty);
			}
		}
	}
}
