using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class RequestedDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var userControl = new RequestedDocumentsUserControl())
			{
				AssertEquals(typeof(IRequestedDocumentsProvider), userControl.BindingSource.DataSourceType);
				AssertEquals(nameof(IRequestedDocumentsProvider.RequestedDocuments), userControl.FindSingle<ZGrid>("RequestedDocumentsGrid").GetBindingMember());
			}
		}

		public void TestAvailableColumns()
		{
			using (var userControl = new RequestedDocumentsUserControl())
			{
				AssertSequencesEqual("Columns", new[] { CSI_Code, RequestInformationColumn, CSI_DateOfIssue, CSI_DateOfExpiry, CSI_Status, StatusDescriptionColumn },
					userControl.FindSingle<ZGrid>("RequestedDocumentsGrid").ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestColumnsWidth()
		{
			using (var userControl = new RequestedDocumentsUserControl())
			{
				var documentsRequestedGrid = userControl.FindSingle<ZGrid>("RequestedDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), documentsRequestedGrid.GetColumnWidth(CSI_Code));
					AssertEquals("RequestInformation", ControlDpiScalingHelper.ScaleToCurrentDpiX(200), documentsRequestedGrid.GetColumnWidth(RequestInformationColumn));
					AssertEquals("CSI_DateOfIssue", ControlDpiScalingHelper.ScaleToCurrentDpiX(120), documentsRequestedGrid.GetColumnWidth(CSI_DateOfIssue));
					AssertEquals("CSI_DateOfExpiry", ControlDpiScalingHelper.ScaleToCurrentDpiX(120), documentsRequestedGrid.GetColumnWidth(CSI_DateOfExpiry));
					AssertEquals("CSI_Status", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), documentsRequestedGrid.GetColumnWidth(CSI_Status));
					AssertEquals("StatusDescription", ControlDpiScalingHelper.ScaleToCurrentDpiX(160), documentsRequestedGrid.GetColumnWidth(StatusDescriptionColumn));
				});
			}
		}

		const string RequestInformationColumn = nameof(RequestedDocument.RequestInformation);
		const string StatusDescriptionColumn = nameof(RequestedDocument.StatusDescription);
	}
}
