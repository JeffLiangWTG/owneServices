using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class AISDocumentsUploadAddInfoGridUserControlTest : TestCaseWithFactory
	{
		public void TestAddInfosGridGrid()
		{
			using (var control = new AISDocumentsUploadAddInfoGridUserControl())
			{
				var grid = control.AddInfosGrid;
				AssertNotNull("AddInfosGrid", grid);
				AssertEquals("AddInfosGrid Column Count", 2, grid.ColumnStyles.Count);
				AssertEquals("Binding Member", "AddInfoCollection", grid.GetBindingMember());
			}
		}

		public void TestAddInfosIM483GridGrid()
		{
			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true))
			{
				using (var control = new AISDocumentsUploadAddInfoGridUserControl())
				{
					var grid = control.AddInfosIM483Grid;
					AssertNotNull("AddInfosGrid", grid);
					AssertEquals("AddInfosGrid Column Count", 4, grid.ColumnStyles.Count);
					AssertEquals("Binding Member", "AddInfoCollection", grid.GetBindingMember());
				}
			}

			using (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false))
			{
				using (var control = new AISDocumentsUploadAddInfoGridUserControl())
				{
					var grid = control.AddInfosIM483Grid;
					AssertEquals("AddInfosGrid Column Count", 3, grid.ColumnStyles.Count);
				}
			}
		}
	}
}
