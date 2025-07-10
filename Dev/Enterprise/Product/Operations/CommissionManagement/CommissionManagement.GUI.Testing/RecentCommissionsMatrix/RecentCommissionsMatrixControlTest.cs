using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class RecentCommissionsMatrixControlTest : TestCaseWithFactory
	{
		public void TestColumnCaptions()
		{
			var lineCollection = new ViewCommissionLineCollection(Factory);
			var matrix = new RecentCommissionsMatrix(new ZDateTime(2002, 2, 2), lineCollection);

			using (var form = new ZForm(matrix))
			using (var control = new RecentCommissionsMatrixControlForTesting())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("January", control.GetColumnCaption(RecentCommissionsRow.Schema.TotalPreviousMonth));
				AssertEquals("December", control.GetColumnCaption(RecentCommissionsRow.Schema.Total2MonthsAgo));
				AssertEquals("November", control.GetColumnCaption(RecentCommissionsRow.Schema.Total3MonthsAgo));
			}
		}

		class RecentCommissionsMatrixControlForTesting : RecentCommissionsMatrixControl
		{
			public string GetColumnCaption(string columnName)
			{
				var caption = base.RecentCommissionsMatrixGrid.GetColumnCaption(columnName);
				return caption.Replace("\u001f", "").Trim(); //control characters
			}

			public RecentCommissionsMatrixGrid RecentCommissionsMatrixGrid_Exposed
			{
				get { return base.RecentCommissionsMatrixGrid; }
			}
		}
	}
}
