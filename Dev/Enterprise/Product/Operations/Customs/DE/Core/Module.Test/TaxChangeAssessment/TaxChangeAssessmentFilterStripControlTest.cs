using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.DE.Business.TaxChangeAssessment.Schema;
using static Enterprise.Messaging.Business.AutoEDIMessage.Schema;

namespace Enterprise.Customs.DE.Module.Testing
{
	class TaxChangeAssessmentFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new TaxChangeAssessmentFilterStripControl(new TaxChangeAssessmentCollection(Factory), new TaxChangeAssessmentFilterStripBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertSequencesEqual("Columns", new[] { EM_SystemCreateUser, EM_SystemCreateTimeUtc, EM_SystemLastEditUser, EM_SystemLastEditTimeUtc, Type, ReferenceNumber, LocalReferenceNumber, IssueDate, MaturityDate, EntryStatus, BranchCode }, grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					AssertEquals("Type", 100, grid.GetColumnStyle(Type).Width);
					AssertEquals("ReferenceNumber", 130, grid.GetColumnStyle(ReferenceNumber).Width);
					AssertEquals("LocalReferenceNumber", 130, grid.GetColumnStyle(LocalReferenceNumber).Width);

					var issueDateColumnStyleInfo = grid.GetColumnStyle(IssueDate) as ZDateEditColumnStyleInfo;
					AssertEquals("IssueDate.Width", 80, issueDateColumnStyleInfo.Width);
					AssertEquals("IssueDate.DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, issueDateColumnStyleInfo.DateTimeFormat);

					var maturityDateColumnStyleInfo = grid.GetColumnStyle(MaturityDate) as ZDateEditColumnStyleInfo;
					AssertEquals("MaturityDate.Width", 80, maturityDateColumnStyleInfo.Width);
					AssertEquals("MaturityDate.DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, maturityDateColumnStyleInfo.DateTimeFormat);

					AssertEquals("EntryStatus", 60, grid.GetColumnStyle(EntryStatus).Width);
					AssertEquals("BranchCode", 60, grid.GetColumnStyle(BranchCode).Width);
				});
			}
		}
	}
}
