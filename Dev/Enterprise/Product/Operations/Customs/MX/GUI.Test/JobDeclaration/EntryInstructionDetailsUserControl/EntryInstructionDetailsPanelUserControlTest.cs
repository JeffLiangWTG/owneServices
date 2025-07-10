using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class EntryInstructionDetailsPanelUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new EntryInstructionDetailsPanelUserControl())
			{
				AssertEquals(typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new EntryInstructionDetailsPanelUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("UCRNumberTextBox", () => control.FindSingle<ZTextBox>("UCRNumberTextBox"));
				});
			}
		}
	}
}
