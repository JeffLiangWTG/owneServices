using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Module.Testing
{
	sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilteredGridFields()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var grid = filterControl.FilteredGrid;
				var columnStyle = grid.GetColumnStyle("JE_MessageStatus");
				var columnStyle2 = grid.GetColumnStyle("JE_MessageStatusDescription");
				AssertEquals(columnStyle.GroupName, columnStyle2.GroupName);
			}
		}

		public void TestImporterAndSupplierColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var grid = filterControl.FilteredGrid;
				var importerColumn = grid.GetColumnStyle("JE_OH_Importer");
				var supplierColumn = grid.GetColumnStyle("JE_OH_Supplier");

				CombineAssertions(() =>
				{
					AssertEquals("Importer Caption", importerColumn.CaptionResourceString.Caption, "Importer/Consignee");
					AssertEquals("Supplier Caption", supplierColumn.CaptionResourceString.Caption, "Shipper/Exporter");
					AssertEquals("Importer Width", importerColumn.Width, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
					AssertEquals("Supplier Width", supplierColumn.Width, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120));
				});
			}
		}
	}
}
