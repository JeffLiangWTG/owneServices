using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(ESH7BillFilterStripControl))]
	class ESH7BillFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGetReferenceNumberColumns()
		{
			AssertColumnExists(["G3LocalReferenceNumber", "H7MovementReferenceNumber", "G3MovementReferenceNumber"]);
		}

		void AssertColumnExists(string[] columnNames)
		{
			using (var module = new ESH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (ESH7BillFilterStripControl)module.EmbeddedControl;
				var filterColumnNames = filterControl.Grid.ColumnStyles
					.OfType<ZTextBoxColumnStyleInfo>()
					.Where(c => columnNames.Contains(c.ColumnName))
					.Select(x => x.ColumnName);

				AssertEquals(columnNames.Length, filterColumnNames.Count());
				AssertContainsExactElementsInAnyOrder(columnNames, filterColumnNames);
			}
		}
	}
}
