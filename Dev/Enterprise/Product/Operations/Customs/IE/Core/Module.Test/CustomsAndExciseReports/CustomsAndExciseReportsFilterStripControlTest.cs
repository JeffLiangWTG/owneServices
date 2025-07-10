using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(CustomsAndExciseReportsFilterStripControl))]
	sealed class CustomsAndExciseReportsFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var reports = new CustomsAndExciseReportOutboundMessageCollection(Factory);
			var filterBO = new CustomsAndExciseReportsFilterStripBusinessObject();
			using (var userControl = new CustomsAndExciseReportsFilterStripControl(reports, filterBO))
			{
				var grid = userControl.Grid;
				CombineAssertions(() =>
				{
					AssertColumn("EM_MessageType", grid.GetColumnStyle(CustomsAndExciseReportOutboundMessage.Schema.EM_MessageType), true, 80);
					AssertColumn("EM_MessageNum", grid.GetColumnStyle(CustomsAndExciseReportOutboundMessage.Schema.EM_MessageNum), true, 120);
					AssertColumn("EM_Status", grid.GetColumnStyle(CustomsAndExciseReportOutboundMessage.Schema.EM_Status), true, 80);
					AssertColumn("MessageDate", grid.GetColumnStyle(nameof(CustomsAndExciseReportOutboundMessage.MessageDate)), true, 80);
					AssertColumn("InterchangeNum", grid.GetColumnStyle(nameof(CustomsAndExciseReportOutboundMessage.InterchangeNum)), false, 120);
					AssertColumn("InterchangeType", grid.GetColumnStyle(nameof(CustomsAndExciseReportOutboundMessage.InterchangeType)), false, 80);
					AssertColumn("BodyText", grid.GetColumnStyle(nameof(CustomsAndExciseReportOutboundMessage.BodyText)), false, 80);
				});
			}
		}

		void AssertColumn(string columnName, ZGridColumnInfo column, bool expectedVisible, int expectedWidth)
		{
			AssertNotNull(columnName, column);
			AssertEquals($"{columnName} visible by default", expectedVisible, column.IsVisible);
			AssertEquals($"{columnName} width", expectedWidth, column.Width);
		}
	}
}
