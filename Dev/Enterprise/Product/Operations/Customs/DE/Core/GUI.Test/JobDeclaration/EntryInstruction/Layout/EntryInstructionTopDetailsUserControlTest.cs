using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class EntryInstructionTopDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new EntryInstructionTopDetailsUserControl())
			{
				AssertEquals(typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new EntryInstructionTopDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("StyleDropEdit", () => control.FindSingle<ZDropEdit>("StyleDropEdit"));
					AssertNoExceptionThrown("SubStyleDropEdit", () => control.FindSingle<ZDropEdit>("SubStyleDropEdit"));
					AssertNoExceptionThrown("DescriptionTextBox", () => control.FindSingle<ZTextBox>("DescriptionTextBox"));
					AssertNoExceptionThrown("AdditionalInfoTextBox", () => control.FindSingle<LongTextControl>("AdditionalInfoTextBox"));
					AssertNoExceptionThrown("CPCDropEdit", () => control.FindSingle<ZDropEdit>("CPCDropEdit"));
					AssertNoExceptionThrown("DateForDutyDateEdit", () => control.FindSingle<ZDateEdit>("DateForDutyDateEdit"));
					AssertNoExceptionThrown("ExitDateDateEdit", () => control.FindSingle<ZDateEdit>("ExitDateDateEdit"));
					AssertNoExceptionThrown("AuthorisationNumberDropEdit", () => control.FindSingle<ZDropEdit>("AuthorisationNumberDropEdit"));
					AssertNoExceptionThrown("PartyConstellationDropEdit", () => control.FindSingle<ZDropEdit>("PartyConstellationDropEdit"));
				});
			}
		}
	}
}
