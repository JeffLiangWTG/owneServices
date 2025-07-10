using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module.Testing
{
	class MonthlyClosingFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGridColumnWidths()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		MonthlyClosingFilterStripControl GetNewFilterControl()
		{
			var collection = new CusReconDeclarationCollection(Factory);
			var filterBizO = new MonthlyClosingFilterBusinessObject();
			return new MonthlyClosingFilterStripControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(CusReconDeclaration.Schema.CRD_JobReferenceNumber, "Job Number", 209),
			(CusReconDeclaration.Schema.CRD_PeriodFrom, "Period From", 82),
			(CusReconDeclaration.Schema.CRD_PeriodTo, "Period To", 69),
			(CusReconDeclaration.Schema.AuthorizationNumber, "Authorization Number", 129),
			(CusReconDeclaration.Schema.CRD_DeclarationType, "Declaration Type", 105),
			(CusReconDeclaration.Schema.CRD_CustomsOffice, "Customs Office", 96),
			(CusReconDeclaration.Schema.OfficeDescription, "Office Description", 109),
			(CusReconDeclaration.Schema.RegistrationNumber, "Registration Number", 120),
			(CusReconDeclaration.Schema.CRD_MessageStatus, "Message Status", 98),
			(CusReconDeclaration.Schema.MessageStatusDescription, "Message Status Description", 156),
			(CusReconDeclaration.Schema.CRD_CustomsStatus, "Customs Status", 90),
			(CusReconDeclaration.Schema.CustomsStatusDescription, "Customs Status Description", 156),
			(CusReconDeclaration.Schema.BranchCode, "Branch", 57),
			(CusReconDeclaration.Schema.BranchName, "Branch Name", 120),
			(nameof(CusReconDeclaration.DeclarantCode), "Declarant", 80),
			(nameof(CusReconDeclaration.RepresentativeCode), "Representative", 80),
			(nameof(CusReconDeclaration.BuyingAgentCode), "Represented Party", 120),
			(nameof(CusReconDeclaration.IsFinalized), "Is Finalized?", 60),
			(nameof(CusReconDeclaration.LineStatusSummary), "Line Status", 120),
		};
	}
}
