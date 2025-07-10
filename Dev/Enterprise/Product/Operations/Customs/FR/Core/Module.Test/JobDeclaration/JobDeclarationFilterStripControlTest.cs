using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	public class JobDeclarationFilterStripControlTest : EU.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFRAdditionalColumnsExist()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.FallbackEntryNumber));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.FallbackEntryDate));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.FallbackEntryStatus));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_CustomsProfile));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_EntryStatus));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_DeltaMode));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.IsDeltaDStepOneSentOK));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.IsDeltaDStepTwoSentOK));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.IsDeltaDStepTwoSentOKButZeroLiquidation));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.EntryReleaseDate));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.EntryExitedStatus));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.AssessmentDate));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.CustomsLastEntryStatusDate));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.TriggeringPointForValidation));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_ExportExitType));
				AssertNotNull(grid.GetColumnStyle(CusEntryHeader.Schema.CorrelationID));
			}
		}
	}
}
