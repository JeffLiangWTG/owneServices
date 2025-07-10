using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionExportSealsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new EntryInstructionExportSealsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("SealsPanel", control.FindSingleOrDefault<ZPanel>("SealsPanel"));
					AssertNotNull("SealsTotalCountCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("SealsTotalCountCalcEdit"));
					AssertNotNull("SealsGridGroupBox", control.FindSingleOrDefault<ZGroupBox>("SealsGridGroupBox"));
					AssertNotNull("SealsGrid", control.FindSingleOrDefault<ZGrid>("SealsGrid"));
					AssertEquals("Count", 1, control.FindSingleOrDefault<ZGrid>("SealsGrid").ColumnStyles.Count);
				});
			}
		}
	}
}
