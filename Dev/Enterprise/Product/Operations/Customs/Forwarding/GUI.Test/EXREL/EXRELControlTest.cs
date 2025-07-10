using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Forwarding.GUI.Testing
{
	class EXRELControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new EXRELControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("EDIMessageGrid", control.FindSingleOrDefault<ZGrid>("EDIMessageGrid"));
					AssertNotNull("MessageTextTextBox", control.FindSingleOrDefault<ZTextBox>("MessageTextTextBox"));
				});
			}
		}

		public void TestMessageGrid()
		{
			using (var control = new EXRELControl())
			{
				var grid = control.FindSingleOrDefault<ZGrid>("EDIMessageGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Grid should have 5 columns", 5, grid.ColumnStyles.Count);
					AssertEquals("EM_MessageNum", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
					AssertEquals("EM_MessageType", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
					AssertEquals("EM_Status", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
					AssertEquals("EM_User", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
					AssertEquals("EM_MessageDateTime", ((ZDateEditColumnStyleInfo)grid.ColumnStyles[4]).ColumnName);
				});
			}
		}

		public void TestDataSourceType()
		{
			using (var control = new EXRELControl())
			{
				AssertEquals(typeof(ForwardingShipment), control.DataSourceType);
			}
		}
	}
}
