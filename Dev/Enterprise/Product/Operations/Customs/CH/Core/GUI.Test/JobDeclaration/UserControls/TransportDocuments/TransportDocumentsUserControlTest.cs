using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

internal class TransportDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using (var control = new TransportDocumentsUserControl())
		{
			AssertEquals(typeof(TransportDocument), control.BindingSource.DataSourceType);
		}
	}

	public void TestGridColumnStyleProperties()
	{
		var transportDocument = Factory.New<TransportDocument>();
		var collection = new SupportingDocumentCollection(transportDocument);
		using (var control = new TransportDocumentsUserControl())
		{
			var grid = control.TransportDocumentsGrid;
			grid.SetDataBinding(collection, "");
			control.Show();

			AssertEquals(2, grid.Columns.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(TransportDocument.Schema.CSI_Code).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(TransportDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
			});
		}
	}
}
