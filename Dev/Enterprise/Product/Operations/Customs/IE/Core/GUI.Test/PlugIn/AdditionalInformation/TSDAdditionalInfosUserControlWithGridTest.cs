using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class TSDAdditionalInfosUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestAdditionalInfosGridColumnsVisible()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			_ = bill.AdditionalInfos.AddNew();

			using (var form = new ZForm(header))
			using (var control = new TSDAdditionalInfosUserControlWithGrid())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				AssertEquals("CSI_SubType, CSI_Code, CSI_ReferenceNumber, CSI_Description", string.Join(", ", grid.Columns.Select(x => x.ColumnName)));
			}
		}
	}
}
