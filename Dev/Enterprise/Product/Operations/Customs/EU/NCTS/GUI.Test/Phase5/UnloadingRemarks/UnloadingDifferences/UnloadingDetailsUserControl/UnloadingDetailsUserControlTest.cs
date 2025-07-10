using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadingDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(Business.NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestUnloadingDate()
		{
			var unloadingDate = userControl.UnloadingDateDateEdit;
			AssertType<ZDateEdit>(unloadingDate);
			AssertEquals("UnloadingDateDateEdit date format", Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, unloadingDate.DateTimeFormat);
		}

		public void TestUnloadingConform()
		{
			var unloadingConform = userControl.UnloadingConformCheckBox;
			AssertType<ZCheckBox>(unloadingConform);
		}

		public void TestStateOfSeals()
		{
			var stateOfSeals = userControl.StateOfSealsCheckBox;
			AssertType<ZCheckBox>(stateOfSeals);
		}

		public void TestUnloadingCompleted()
		{
			var unloadingCompleted = userControl.UnloadingCompletedCheckBox;
			AssertType<ZCheckBox>(unloadingCompleted);
		}

		public void TestUnloadingRemarks()
		{
			var unloadingRemarks = userControl.UnloadingRemarksTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(unloadingRemarks);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, unloadingRemarks.CharacterCasing);
			});
		}

		public void TestOtherThingsToReport()
		{
			var otherThingsToReport = userControl.OtherThingsToReportTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(otherThingsToReport);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, otherThingsToReport.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new UnloadingDetailsUserControl();
		}
		UnloadingDetailsUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
