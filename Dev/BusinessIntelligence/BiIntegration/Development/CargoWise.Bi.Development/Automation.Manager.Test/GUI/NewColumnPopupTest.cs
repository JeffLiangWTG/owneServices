using System.Data;
using CargoWise.Bi.Development.Automation.Manager;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	public class NewColumnPopupTest : TestCase
	{
		[RequiresSTA]
		public void TestFormLoad()
		{
			var dummyTable = new DataTable();
			var form = new NewColumnPopup(dummyTable);
			AssertNotNull(form);
			form.Show();
			AssertEquals("Add New Column", form.Title);
			AssertEquals(dummyTable.Columns.Count, 0);
			form.Close();
			form.Dispose();
			dummyTable.Dispose();
		}
	}
}
