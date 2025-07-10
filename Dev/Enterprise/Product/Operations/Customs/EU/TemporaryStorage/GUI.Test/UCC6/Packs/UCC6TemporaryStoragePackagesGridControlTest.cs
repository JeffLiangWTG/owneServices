using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackagesGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new UCC6TemporaryStoragePackagesGridControl())
			{
				var billsGrid = (ZGrid)control.Controls.Find("gridPacks", true).First();
				var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
				AssertContainsExactElementsInExactOrder("gridPacks Column Names",
				new[] { "APA_LineNo",
						"ContainerPK",
						"APA_PackQty",
						"APA_PackUQ",
						"APA_MarksAndNumbers"
				}, columns.Select(x => x.ColumnName));
			}
		}

		public void TestGridColumns_Transfer()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			using (var form = new ZForm(header))
			using (var control = new UCC6TemporaryStoragePackagesGridControl())
			{
				form.Controls.Add(control);
				form.Show();

				var billsGrid = (ZGrid)control.Controls.Find("gridPacks", true).First();
				var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable);
				AssertContainsExactElementsInExactOrder("gridPacks Column Names",
				new[] { "APA_LineNo",
						"APA_PackQty",
						"APA_PackUQ",
				}, columns.Select(x => x.ColumnName));
			}
		}
	}
}
