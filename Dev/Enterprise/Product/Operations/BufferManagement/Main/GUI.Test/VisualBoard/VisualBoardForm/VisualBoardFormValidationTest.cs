using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormValidationTest : BMSGUITestCase
	{
		#region Section Validation

		public void TestBoardSectionValidation_WhenSectionInvalidAndBoardIsReloaded_LoadFailedLabelShouldContinueToShow()
		{
			var section = config.BufferSection;
			section.Component.FC_Name = "My Board Section!";
			section.SectionConfiguration.OverdueForegroundColor = "Some trash colour";

			Factory.Save();

			AssertSectionWarningsAndErrors(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: true,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 1
			);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
				AssertNotNull(label);
				AssertEquals("This section (My Board Section!) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", label.Text);

				Factory.Save();
				form.ReloadBoard();
				Application.DoEvents();

				control = form.FindSingle<BMComponentControl>();
				label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
				AssertNotNull(label);
				AssertEquals("This section (My Board Section!) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", label.Text);
			}
		}

		public void TestBoardSectionValidation_EnsureThatValidationDoesNotOccurOnBoardDifferentialRefresh()
		{
			var section = config.BufferSection;

			Factory.Save();

			AssertSectionWarningsAndErrors(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: false,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 0
			);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
				AssertNull(label);

				// Fudge the section value mid test to ensure it is not validating the field again on refresh.
				// This is functionally impossible but a good way to test that we are not validating
				// the board on differential refresh (to save time and hits to database).
				section.RowHeightPercent = 111;

				Factory.Save();
				form.RefreshBoard();

				label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
				AssertNull("Sections on Visual Boards should only be validated on initial load and forced reload.", label);
			}
		}

		public void TestBoardSectionValidation_BufferSectionValid_SectionSuccessfullyDisplays()
		{
			var section = config.BufferSection;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: false,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 0,
				expectSectionToShowError: false
			);
		}

		public void TestBoardSectionValidation_BufferSectionConfigurationValidButWithWarning_SectionSuccessfullyDisplays()
		{
			var section = config.BufferSection;
			section.SectionConfiguration.ShowZones = false;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: true,
				expectedSectionHasErrors: false,
				expectedSectionConfigHasWarnings: true,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 1,
				expectSectionToShowError: false
			);
		}

		public void TestBoardSectionValidation_BufferSectionInvalid_SectionShowsErrorMsg()
		{
			var section = config.BufferSection;
			section.RowHeightPercent = 111;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 1,
				expectedSectionConfigNotificationsCount: 0,
				expectSectionToShowError: true
			);
		}

		public void TestBoardSectionValidation_BufferSectionConfigurationInvalid_SectionShowsErrorMsg()
		{
			var section = config.BufferSection;
			section.SectionConfiguration.MaxOverdueSlots = 1;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: true,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 1,
				expectSectionToShowError: true
			);
		}

		public void TestBoardSectionValidation_BucketSectionValid_SectionSuccessfullyDisplays()
		{
			var section = config.BucketSection;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: false,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 0,
				expectSectionToShowError: false
			);
		}

		public void TestBoardSectionValidation_BucketSectionConfigurationValidButWithWarning_SectionSuccessfullyDisplays()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: true,
				expectedSectionHasErrors: false,
				expectedSectionConfigHasWarnings: true,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 1,
				expectSectionToShowError: false
			);
		}

		public void TestBoardSectionValidation_BucketSectionInvalid_SectionShowsErrorMsg()
		{
			var section = config.BucketSection;
			section.ColWidthPercent = 111;

			AssertBoardSectionValidation(section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: false,
				expectedSectionNotificationsCount: 1,
				expectedSectionConfigNotificationsCount: 0,
				expectSectionToShowError: true
			);
		}

		public void TestBoardSectionValidation_BucketSectionConfigurationInvalid_SectionShowsErrorMsg()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.ShowZones = true;

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: true,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 1,
				expectSectionToShowError: true
			);
		}

		public void TestBoardSectionValidation_ChildrenConfigurationInvalid_SectionShowsErrorMsg()
		{
			var section = config.BucketSection;
			BMSTestHelper.CreateAdditionalComponent(section, config.Bucket); // Make the main component the additional component as well

			AssertBoardSectionValidation(
				section,
				expectedSectionHasWarnings: false,
				expectedSectionHasErrors: true,
				expectedSectionConfigHasWarnings: false,
				expectedSectionConfigHasErrors: true,
				expectedSectionNotificationsCount: 0,
				expectedSectionConfigNotificationsCount: 0,
				expectSectionToShowError: true,
				expectedSectionName: "My Board Section!, My Board Section!"
			);
		}

		#endregion

		#region Implementation

		VisualBoardTestConfig config;
		protected override void SetUp()
		{
			base.SetUp();
			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		void AssertBoardSectionValidation(BMBoardSection section, bool expectedSectionHasWarnings, bool expectedSectionHasErrors,
			bool expectedSectionConfigHasWarnings, bool expectedSectionConfigHasErrors, int expectedSectionNotificationsCount,
			int expectedSectionConfigNotificationsCount, bool expectSectionToShowError, string expectedSectionName = "My Board Section!")
		{
			section.Component.FC_Name = "My Board Section!";
			Factory.Save();

			AssertSectionWarningsAndErrors(
				section,
				expectedSectionHasWarnings,
				expectedSectionHasErrors,
				expectedSectionConfigHasWarnings,
				expectedSectionConfigHasErrors,
				expectedSectionNotificationsCount,
				expectedSectionConfigNotificationsCount
			);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var label = control.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
				AssertEquals("The state of the load failed label should be correct.", !expectSectionToShowError, label == null);

				if (expectSectionToShowError)
				{
					AssertEquals($"This section ({expectedSectionName}) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", label.Text);
				}
			}
		}

		void AssertSectionWarningsAndErrors(BMBoardSection section, bool expectedSectionHasWarnings, bool expectedSectionHasErrors,
			bool expectedSectionConfigHasWarnings, bool expectedSectionConfigHasErrors, int expectedSectionNotificationsCount, int expectedSectionConfigNotificationsCount)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Section warnings", expectedSectionHasWarnings, section.HasWarnings);
				AssertEquals("Section errors", expectedSectionHasErrors, section.HasErrors);
				AssertEquals("Section config warnings", expectedSectionConfigHasWarnings, section.SectionConfiguration.HasWarnings);
				AssertEquals("Section config errors", expectedSectionConfigHasErrors, section.SectionConfiguration.HasErrors);
				AssertEquals("Section notifications count", expectedSectionNotificationsCount, section.Notifications.Count());
				AssertEquals("Section config notifications count", expectedSectionConfigNotificationsCount, section.SectionConfiguration.Notifications.Count());
			});
		}

		#endregion
	}
}
