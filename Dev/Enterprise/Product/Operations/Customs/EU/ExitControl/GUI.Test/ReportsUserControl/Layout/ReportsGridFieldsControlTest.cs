using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportsGridFieldsControlTest : TestCaseWithFactory
	{
		public void TestControls() => CombineAssertions(() =>
		{
			TestControlAndCharacterCasing<ZGuidDropEdit>("ConsignmentGuidDropEdit", CharacterCasing.Upper);
			TestControlAndCharacterCasing<ZCodeFindBox>("OfficeOfExitCodeFindBox");
			TestControlAndCharacterCasing<ZDropEdit>("TransportTypeDropEdit", CharacterCasing.Upper);
			TestControlAndCharacterCasing<ZTextBox>("TransportIDTextBox", CharacterCasing.Upper);
			TestControlAndCharacterCasing<ZDropEdit>("TransportNationalityDropEdit", CharacterCasing.Upper);
			TestControlAndCharacterCasing<ZDateEdit>(nameof(ReportsGridFieldsControl.FormattedDateTimeDateEdit));
			TestControlAndCharacterCasing<ZCodeFindBox>(nameof(ReportsGridFieldsControl.LocationCodeFindBox));
			TestControlAndCharacterCasing<ZCheckBox>(nameof(ReportsGridFieldsControl.DiscrepanciesCheckBox));
			TestControlAndCharacterCasing<ZDropEdit>(nameof(ReportsGridFieldsControl.TransportModeDropEdit));
		});

		public void TestDiscrepanciesCheckBox() => CombineAssertions(() =>
		{
			var discrepanciesCheckBox = control.DiscrepanciesCheckBox;
			AssertEquals("CheckAlign", ZContentAlignment.Right, discrepanciesCheckBox.CheckAlign);
			AssertEquals("TextAlign", System.Drawing.ContentAlignment.MiddleRight, discrepanciesCheckBox.TextAlign);
		});

		public void TestLocationOfGoodsUserControl() => AssertNotNull(control.FindSingle<LocationOfGoodsUserControl>("LocationOfGoodsUserControl"));

		void TestControlAndCharacterCasing<T>(string controlName, CharacterCasing? characterCasing = null) where T : Control
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
