using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccReportingBookFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var reportingBookCollection = new AccReportingBookCollection(Factory);
			var filterBO = new AccReportingBookFilterBusinessObject();

			using (var form = new ZForm())
			{
				var filterControl = new AccReportingBookFilterControl(reportingBookCollection, filterBO);
				form.Controls.Add(filterControl);
				form.Show();

				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_Code").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_IsActive").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_Description").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_CategorisWithChildren").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_AAC_AlternateChart").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ARB_IsGlobal").IsVisible);
			}
		}
	}
}
