using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ContainersSelectionForm))]
	public class ContainersSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var containers = new ContainerSelectionBusinessObjectCollection();
			return new ContainersSelectionForm(containers);
		}

		public void TestContainersGrid()
		{
			using (var testForm = (ContainersSelectionForm)GetFormToBash())
			{
				testForm.Show();

				var containersGrids = testForm.Controls.Find("ContainersGrid", false);
				var containersGrid = containersGrids.Cast<ZGrid>().Single();
				AssertEquals(6, containersGrid.Columns.Count);

				AssertEquals("IsSelected", containersGrid.Columns[0].ColumnName);
				Assert(!containersGrid.Columns[0].ColumnStyle.ReadOnly);

				AssertEquals("Number", containersGrid.Columns[1].ColumnName);
				Assert(containersGrid.Columns[1].ColumnStyle.ReadOnly);

				AssertEquals("Weight", containersGrid.Columns[2].ColumnName);
				Assert(containersGrid.Columns[2].ColumnStyle.ReadOnly);

				AssertEquals("Volume", containersGrid.Columns[3].ColumnName);
				Assert(containersGrid.Columns[3].ColumnStyle.ReadOnly);

				AssertEquals("Commodity", containersGrid.Columns[4].ColumnName);
				Assert(containersGrid.Columns[4].ColumnStyle.ReadOnly);

				AssertEquals("Quantity", containersGrid.Columns[5].ColumnName);
				Assert(containersGrid.Columns[5].ColumnStyle.ReadOnly);
			}
		}
	}
}
