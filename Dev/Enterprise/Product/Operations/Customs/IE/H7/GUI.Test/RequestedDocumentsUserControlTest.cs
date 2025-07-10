using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	sealed class RequestedDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTabPage()
		{
			using (var control = new RequestedDocumentsUserControl() as IAdditionalTabPage)
			{
				CombineAssertions("Additional Tab Page", () =>
				{
					AssertEquals("Caption", "Requested Documents", control.AdditionalTabPageCaption.Caption);
					AssertEquals("Tab page sequence", 60, control.TabPageSequence);
				});
			}
		}

		public void TestRequestedDocumentsGrid_ReferenceNumber()
		{
			using (var control = new RequestedDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("RequestedDocumentsGrid");

				var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(RequestedDocument.CSI_ReferenceNumber));
				AssertEquals("Width", 150, columnInfo.Width);

				var captionResourceString = columnInfo.CaptionResourceString;
				AssertEquals("Short", "Ref. No.", captionResourceString.ShortCaption);
				AssertEquals("Medium", "Reference No.", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Reference Number", captionResourceString.Caption);
				AssertEquals("Full Description", "Requested document reference number.", captionResourceString.FullDescription);
			}
		}
	}
}
