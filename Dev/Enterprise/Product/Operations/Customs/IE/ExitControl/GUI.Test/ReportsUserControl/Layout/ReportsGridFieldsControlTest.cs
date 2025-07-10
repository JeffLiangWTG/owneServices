using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	class ReportsGridFieldsControlTest : TestCaseWithFactory
	{
		public void TestDiscrepanciesCheckBox()
		{
			var discrepanciesCheckBox = control.DiscrepanciesCheckBox;
			AssertEquals("CheckAlign", ZContentAlignment.Right, discrepanciesCheckBox.CheckAlign);
			AssertEquals("TextAlign", System.Drawing.ContentAlignment.MiddleRight, discrepanciesCheckBox.TextAlign);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				AssertControlAndCharacterCasing<ZDateEdit>("FormattedDateTimeDateEdit");
				AssertControlAndCharacterCasing<ZDateEdit>("LongFormattedDateTimeDateEdit");
				AssertControlAndCharacterCasing<ZDropEdit>("TypeOfLocationDropEdit", CharacterCasing.Upper);
				AssertControlAndCharacterCasing<ZCodeFindBox>("UNLOCOCodeFindBox");
				AssertControlAndCharacterCasing<ZDropEdit>("DeclarantTypeDropEdit", CharacterCasing.Upper);
				AssertControlAndCharacterCasing<MasterFiles.GUI.ZDocAddressControl>("DeclarantAddressDropEdit");
				AssertControlAndCharacterCasing<MasterFiles.GUI.ZDocAddressControl>("RepresentativeAddressDropEdit");
				AssertControlAndCharacterCasing<ZCodeFindBox>("OfficeOfExportCodeFindBox");
				AssertControlAndCharacterCasing<ZDropEdit>("EnquiryInformationCodeDropEdit", CharacterCasing.Upper);
				AssertControlAndCharacterCasing<ZDropEdit>("AdditionalDeclarationTypeDropEdit", CharacterCasing.Upper);
				AssertControlAndCharacterCasing<ZTextBox>("LocationTextBox", CharacterCasing.Upper);
			});
		}

		void AssertControlAndCharacterCasing<T>(string controlName, CharacterCasing? characterCasing = null) where T : Control
		{
			T field = null;
			AssertNoExceptionThrown($"{controlName} should be found", () => { field = control.FindSingle<T>(controlName); });
			AssertNotNull($"{controlName} should not be bull", field);
			if (characterCasing != null)
			{
				AssertEquals($"{controlName} CharacterCasing", characterCasing, typeof(T).GetProperty("CharacterCasing")?.GetValue(field));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ReportsGridFieldsControl();
		}
		ReportsGridFieldsControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
		}
	}
}
