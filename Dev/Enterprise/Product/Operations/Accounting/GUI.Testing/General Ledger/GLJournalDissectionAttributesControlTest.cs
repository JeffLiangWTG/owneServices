using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	public class GLJournalDissectionAttributesControlTest : TestCaseWithFactory
	{
		public void TestShowDissectionAttributesControl()
		{
			using (var form = new ZForm())
			{
				var dissectionAttributesControl = new GLJournalDissectionAttributesControl();
				form.Controls.Add(dissectionAttributesControl);
				form.Show();
				var columnNameList = new List<string>(new string[] { "ALD_Attribute", "ALD_AttributeValue", "ALD_AttributeValueID", "ApplicableAlternateChart" });
				var columnStyles = dissectionAttributesControl.FindSingleOrDefault<ZGrid>("DissectionAttributesGrid").ColumnStyles.Cast<ZGridColumnInfo>();
				foreach (var columnName in columnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull("Pre-condition", columnInfo);
					AssertEquals($"The column name '{columnInfo.ColumnName}' in grid should be visible", columnInfo.IsVisible, true);
				}
				AssertEquals("column count should be 4", 4, columnStyles.Count());
			}
		}

		public void TestSubAccountsGridBindingMember()
		{
			using (var dissectionAttributesControl = new GLJournalDissectionAttributesControl())
			{
				var grid = dissectionAttributesControl.FindSingleOrDefault<ZGrid>("DissectionAttributesGrid");
				AssertNotNull("Pre-condition", grid);
				AssertEquals("BindingMember", ExceptedDissectionAttributesGridBindingMemberCore, grid.GetBindingMember());
			}
		}

		protected string ExceptedDissectionAttributesGridBindingMemberCore => "GLJournalLines.AccTransactionLineDissectionAttributes";
	}
}
