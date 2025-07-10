using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.Module.Testing;

sealed class JobDeclarationFilterStripControlTest : EU.Module.Testing.JobDeclarationFilterStripControlTest
{
	public void TestMessageVersionColumnStyle()
	{
		var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		var filterBO = new JobDeclarationFilterBusinessObject();

		using (var module = new JobDeclarationModule())
		using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
		{
			var grid = userControl.FilteredGrid;

			var messageVersionColumn = grid.GetColumnStyle(JobDeclaration.Schema.MessageVersion);
			AssertNotNull("ColumnStyle", messageVersionColumn);

			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Type", messageVersionColumn);
				AssertEquals("Caption", "Message Version", messageVersionColumn.CaptionResourceString?.Caption);
				AssertEquals("Width", 100, messageVersionColumn.Width);
				AssertEquals("IsVisible", false, messageVersionColumn.IsVisible);
			});
		}
	}
}
