using System;
using System.Collections;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KListBoxTest : ControlTestCase<KListBox>
	{
		public void TestDataSourceNullifiedOnDispose()
		{
			KListBox box = new KListBox();
			box.DataSource = new ArrayList();
			box.Disposed += new EventHandler(OnBox_Disposed);
			box.Dispose();
		}

		void OnBox_Disposed(object sender, EventArgs e)
		{
			ListBox box = (ListBox)sender;
			AssertNull("The data source should be nullified now", box.DataSource);
		}
	}
}
