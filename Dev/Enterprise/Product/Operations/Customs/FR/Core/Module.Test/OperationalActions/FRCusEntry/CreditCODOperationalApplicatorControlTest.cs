using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	public class CreditCODOperationalApplicatorControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var zForm = new ZForm())
			using (var applicatorControl = new CreditCODOperationalApplicatorControl())
			{
				zForm.Controls.Add(applicatorControl);
				zForm.Show();
				var grid = applicatorControl.Controls.Find("CreditCODGrid", true).Single() as ZGrid;
				var gridColumns = new ZString[]
				{
					"CreditMethod",
					"CreditMethodDescription",
					"ReleasingEntryReference",
					"PreviousEntryReference",
					"PreviousEntryLineNo",
					"Amount",
					"Currency"
				};

				AssertEquals(gridColumns.Length, grid.ColumnStyles.Count);
				AssertionWithHtml.CombineAssertions(delegate
				{
					foreach (string item in gridColumns)
					{
						Assertion.AssertNotNull("Column [" + item + "] should NOT be null.", grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == item));
					}
				});
			}
		}
	}
}
