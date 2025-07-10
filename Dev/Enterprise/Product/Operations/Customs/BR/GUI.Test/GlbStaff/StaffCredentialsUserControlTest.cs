using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(StaffCredentialsUserControl))]
	class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestNodesGridColumns()
		{
			using (var form = GetFormToBash())
			{
				var nodesGrid = (ZGrid)form.Controls.Find("BRAccUserGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", nodesGrid);
				AssertEquals("Columns Count", 3, nodesGrid.ColumnStyles.Count);
				CombineAssertions("Column Styles", () =>
				{
					AssertColumn(nodesGrid, GlbExternalPassword_BRS.Schema.GP_UserID, expectedVisible: true, expectedReadOnly: false);
					AssertColumn(nodesGrid, GlbExternalPassword_BRS.Schema.StatusDescription, expectedVisible: true, expectedReadOnly: true);
					AssertColumn(nodesGrid, GlbExternalPassword_BRS.Schema.GP_MailBoxID, expectedVisible: true, expectedReadOnly: true);
				});
			}
		}

		void AssertColumn(ZGrid parentGrid, ZString columnName, ZBool expectedVisible, ZBool expectedReadOnly)
		{
			var columnInfo = parentGrid.GetColumnStyle(columnName);
			AssertNotNull($"'{columnName}' not null", columnInfo);
			AssertEquals($"'{columnName}' IsVisible", expectedVisible, columnInfo.IsVisible);
			AssertEquals($"'{columnName}' IsReadOnly", expectedReadOnly, columnInfo.IsReadOnly);
		}
	}
}
