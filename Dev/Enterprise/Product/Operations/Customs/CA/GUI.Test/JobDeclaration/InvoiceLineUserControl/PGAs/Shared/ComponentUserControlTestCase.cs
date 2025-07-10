using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ComponentUserControlTestCase : TestCaseWithFactory
	{
		public void TestSetComponentColumns()
		{
			using (var control = new ComponentUserControl())
			{
				control.RemoveFromAvailableColumns(null);
				CombineAssertions("Component do not set anything", () =>
				{
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Name)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Type)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.TypeDescription)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Concentration)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Origin)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Qty)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_QualityOrYield)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_UQ)).IsUnavailable);
				});

				control.RemoveFromAvailableColumns
					(
					Component.Schema.CA_Name,
					Component.Schema.CA_Concentration
					);

				CombineAssertions("Component set some columns", () =>
				{
					AssertEquals("Default setting will set unavailable for setting column", true, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Name)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", true, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Concentration)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Type)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.TypeDescription)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Origin)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_Qty)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_QualityOrYield)).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.ComponentGrid.GetColumnStyle(nameof(Component.CA_UQ)).IsUnavailable);
				});
			}
		}

		public void TestReOrderColumns()
		{
			using (var control = new ComponentUserControl())
			{
				control.ReOrderColumns(null);
				CombineAssertions("Component does not reorder anything", () =>
				{
					AssertEquals("Index of CA_Name", 0, control.ComponentGrid.ColumnStyles.IndexOf(control.ComponentGrid.GetColumnStyle(Component.Schema.CA_Name)));
					AssertEquals("Index of CA_Concentration", 5, control.ComponentGrid.ColumnStyles.IndexOf(control.ComponentGrid.GetColumnStyle(Component.Schema.CA_Concentration)));
				});
				control.ReOrderColumns
					(
					Component.Schema.CA_Concentration,
					Component.Schema.CA_Name
					);
				CombineAssertions("Component reorders some columns", () =>
				{
					AssertEquals("Index of CA_Concentration", 0, control.ComponentGrid.ColumnStyles.IndexOf(control.ComponentGrid.GetColumnStyle(Component.Schema.CA_Concentration)));
					AssertEquals("Index of CA_Name", 1, control.ComponentGrid.ColumnStyles.IndexOf(control.ComponentGrid.GetColumnStyle(Component.Schema.CA_Name)));
				});
			}
		}
	}
}
