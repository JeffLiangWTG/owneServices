using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(UpdateProductAdditionalInformationApplicatorControl))]
	public class UpdateProductAdditionalInformationApplicatorControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var zForm = new ZForm())
			using (var applicatorControl = new UpdateProductAdditionalInformationApplicatorControl())
			{
				zForm.Controls.Add(applicatorControl);
				zForm.Show();
				var grid = applicatorControl.Controls.Find("ProductPendingUpdateDataGrid", true).Single() as ZGrid;
				AssertContainsExactElementsInAnyOrder(new[] { "ProductPk", "Description", "CustomsType", "OrganizationPk", }, grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}
	}
}
