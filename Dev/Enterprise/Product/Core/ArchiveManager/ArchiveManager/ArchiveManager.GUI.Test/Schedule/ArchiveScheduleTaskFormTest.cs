using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.GUI.Schedule;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.GUI.Test
{
	sealed class ArchiveScheduleTaskFormTest : TestCaseWithFactory
	{
		public void TestNextScheduledRunDateControlDateTimeFormatIsLong()
		{
			using var form = new ArchiveScheduleTaskForm(Factory.New<ArchiveScheduleTask>());
			AssertEquals("DateTime format for Next Scheduled Run Date should be a Long.", ZDateTimePickerFormat.Long, form.nextScheduledRunDateEdit.DateTimeFormat);
		}

		public void TestNextScheduledRunDateControlShowsLocalTimeOfLoggedInCompany()
		{
			using var form = new ArchiveScheduleTaskForm(Factory.New<ArchiveScheduleTask>());
			AssertEquals("DateTime for Next Scheduled Run Date should use local DateTime of current company", "CalcNextRunTimeLocal", form.nextScheduledRunDateEdit.BindTo);
		}

		public void TestSavingForm_WhenErrorDisplayed_FormDoesNotSave()
		{
			var archiveRecordsOnOrBeforeMinimum = SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Value;
			var errorMessage = $"Only records that are {archiveRecordsOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {archiveRecordsOnOrBeforeMinimum} year(s) or more to continue.";

			var factoryForSchedule = new BusinessObjectFactory();
			var archiveSchedule = factoryForSchedule.New<ArchiveScheduleTask>();
			archiveSchedule.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(archiveSchedule, -archiveRecordsOnOrBeforeMinimum + 1);

			AssertEquals("Precondition", 0, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, ArchiveManagerConstants.Codes.OPS)).Length);

			using (var form = new ArchiveScheduleTaskForm(archiveSchedule))
			{
				form.Show();
				var formSaved = form.FireSaveButton();

				CombineAssertions("An error message should be displayed and the form should not have saved.", () =>
				{
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ContinueWithSave.No, formSaved);
				});
			}

			AssertEquals("The schedule should not exist in the database since the form was not saved.", 0, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, ArchiveManagerConstants.Codes.OPS)).Length);

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestSavingForm_WhenConfirmationDialogIsDisplayed_AndCancelIsSelected_FormDoesNotSave()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 2;

			using (SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempOnOrBeforeMinimum))
			{
				var archiveRecordsOnOrBeforeMinimum = SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Value;
				string userNotification = $"You have selected to archive records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default} year(s) old. Please confirm the data retention requirements for this data before archiving.";

				var factoryForSchedule = new BusinessObjectFactory();
				var archiveSchedule = factoryForSchedule.New<ArchiveScheduleTask>();
				archiveSchedule.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
				PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(archiveSchedule, -archiveRecordsOnOrBeforeMinimum - 1);

				AssertEquals("Precondition", 0, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, ArchiveManagerConstants.Codes.OPS)).Length);

				using (var form = new ArchiveScheduleTaskForm(archiveSchedule))
				{
					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Cancel);

					form.Show();
					var formSaved = form.FireSaveButton();

					CombineAssertions("The confirmation dialog should be displayed and the form is not saved.", () =>
					{
						AssertEquals(userNotification, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(ContinueWithSave.No, formSaved);
					});
				}

				AssertEquals("The schedule should not exist in the database since the form was not saved.", 0, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, ArchiveManagerConstants.Codes.OPS)).Length);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestREDAvailableOnlyWhenExposeRatesArchiveSystemRegistryIsTrue()
		{
			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.Value, false);
				var archiveSystemDescriptorsList = new ArchiveSystemDescriptorLoader().Load();
				Assert(!archiveSystemDescriptorsList.Any(descriptor => descriptor.Code == ArchiveManagerConstants.Codes.RED));
			}

			using (SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(SystemDataRegistry.Instance.ExposeExpiredRatesArchiveSystem.Value, true);
				var archiveSystemDescriptorsList = new ArchiveSystemDescriptorLoader().Load().ToList();
				Assert(archiveSystemDescriptorsList.Any(descriptor => descriptor.Code == ArchiveManagerConstants.Codes.RED));
			}
		}

		public void TestPALAvailableOnlyWhenExposeActivityLogsArchiveSystemRegistryIsTrue()
		{
			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.Value, false);
				var archiveSystemDescriptorsList = new ArchiveSystemDescriptorLoader().Load();
				Assert("PAL should not be found in the archive system descriptors list", !archiveSystemDescriptorsList.Any(descriptor => descriptor.Code == ArchiveManagerConstants.Codes.PAL));
			}

			using (SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(SystemDataRegistry.Instance.ExposeActivityLogsArchiveSystem.Value, true);
				var archiveSystemDescriptorsList = new ArchiveSystemDescriptorLoader().Load();
				Assert("PAL should be found in the archive system descriptors list", archiveSystemDescriptorsList.Any(descriptor => descriptor.Code == ArchiveManagerConstants.Codes.PAL));
			}
		}

		#region AlertLabels

		public void TestAlertLabelNotShownForIPS()
		{
			using var form = new ArchiveScheduleTaskForm(TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS));
			form.Show();
			Assert("Alert label is not shown as IPS is not a purge system", !form.alertLabel.Visible);
		}

		public void TestAlertLabelIsShownForPurgeSystems()
		{
			void TestCase(string purgeSystem)
			{
				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, purgeSystem);

				using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
				form.Show();
				AssertEquals("Warning: You have selected a Purge System. Proceed with caution.", form.alertLabel.Text);
			}

			TestCase(ArchiveManagerConstants.Codes.PAR);
			TestCase(ArchiveManagerConstants.Codes.PDR);
			TestCase(ArchiveManagerConstants.Codes.PDO);
			TestCase(ArchiveManagerConstants.Codes.EST);
			TestCase(ArchiveManagerConstants.Codes.PAL);
			TestCase(ArchiveManagerConstants.Codes.RED);
		}

		#endregion

		#region ArchiveDeclarationCheckBox

		public void TestArchiveDeclarationCheckboxCorrectlyUpdates()
		{
			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();
			Assert("Precondition", !form.archiveDecCheckBox.Visible);

			var systemSelected = ArchiveManagerConstants.Codes.OPS;
			archiveScheduleTask.S5_ScheduleType = systemSelected;
			Assert($"archiveDecCheckbox should be visible for '{systemSelected}' system.", form.archiveDecCheckBox.Visible);

			systemSelected = ArchiveManagerConstants.Codes.PAR;
			archiveScheduleTask.S5_ScheduleType = systemSelected;
			Assert($"archiveDecCheckbox should not be visible for '{systemSelected}' system.", !form.archiveDecCheckBox.Visible);
		}

		public void TestWithOrWithoutJobsSettingsHasCorrectVisibility()
		{
			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();
			Assert("Precondition", !form.JobsOptionsPanel.Visible);

			var systemSelected = ArchiveManagerConstants.Codes.OPS;
			archiveScheduleTask.S5_ScheduleType = systemSelected;
			CombineAssertions($"With or without jobs settings should not be visible for '{systemSelected}' system.", () =>
			{
				Assert("Group box is not visible", !form.JobsOptionsPanel.Visible);
				Assert("WithJobsRadioButton is not visible", !form.WithJobsRadioButton.Visible);
				Assert("WithoutJobsRadioButton is not visible", !form.WithoutJobsRadioButton.Visible);
			});

			systemSelected = ArchiveManagerConstants.Codes.PDO;
			archiveScheduleTask.S5_ScheduleType = systemSelected;
			CombineAssertions($"With or without jobs settings should be visible for '{systemSelected}' system.", () =>
			{
				Assert("Group box is visible", form.JobsOptionsPanel.Visible);
				Assert("WithJobsRadioButton is visible", form.WithJobsRadioButton.Visible);
				Assert("WithoutJobsRadioButton is visible", form.WithoutJobsRadioButton.Visible);
			});
		}

		#endregion

		#region UseOnOrBeforeDateWhenWatermarkResetCheckBox

		public void TestUseOnOrBeforeDateWhenWatermarkResetCheckBoxHasCorrectVisibility()
		{
			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();

			var systemSelected = ArchiveManagerConstants.Codes.OPS;
			archiveScheduleTask.S5_ScheduleType = systemSelected;

			Assert($"Use on or before date when watermark reset checkbox is not visible for '{systemSelected}' system.", !form.useOnOrBeforeDateWhenWatermarkResetCheckBox.Visible);

			systemSelected = ArchiveManagerConstants.Codes.PDO;
			archiveScheduleTask.S5_ScheduleType = systemSelected;

			Assert($"Use on or before date when watermark reset checkbox is visible for '{systemSelected}' system.", form.useOnOrBeforeDateWhenWatermarkResetCheckBox.Visible);
		}

		#endregion

		#region ParameterCaptions

		public void TestSwitchBetweenIPSAndPurgeUpdatesCaptionsCorrectly()
		{
			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
				AssertEquals("Purge System Parameters", form.parameterGroupbox.Text);
				Assert("Alert label is is visible for purge systewm", form.alertLabel.Visible);
			});

			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.IPS;
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("Archive Records On or Before (Create Date):", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Archive Records On or Before (Create Date):", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
				AssertEquals("Archive System Parameters", form.parameterGroupbox.Text);
				Assert("Alert label is not shown as IPS is not a purge system", !form.alertLabel.Visible);
			});

			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.PAR;
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
				AssertEquals("Purge System Parameters", form.parameterGroupbox.Text);
				Assert("Alert label is is visible for purge systewm", form.alertLabel.Visible);
			});
		}

		public void TestSwitchBetweenIPSAndArchiveUpdatesCaptionsCorrectly()
		{
			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Archive Records On or Before:", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Archive Records On or Before:", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
			});

			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.IPS;
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("Archive Records On or Before (Create Date):", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Archive Records On or Before (Create Date):", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
			});

			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("Archive Records On or Before:", form.archiveOnOrBeforeDateRadioButton.Text);
				AssertEquals("Archive Records On or Before:", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
			});
		}

		public void TestParameterCaptionsAreCorrectForArchiveSystems()
		{
			void TestCase(string archiveSystem)
			{
				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, archiveSystem);
				using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Archive System Parameters", form.parameterGroupbox.Text);
					AssertContains("Archive Records On or Before", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
					AssertContains("Archive Records On or Before", form.archiveOnOrBeforeDateRadioButton.Text);
					Assert("Archive alert label should not show for archive system.", !form.alertLabel.Visible);
				});
			}

			TestCase(ArchiveManagerConstants.Codes.OPS);
			TestCase(ArchiveManagerConstants.Codes.IPS);
			TestCase(ArchiveManagerConstants.Codes.STA);
		}

		public void TestParameterCaptionsAreCorrectForPurgeSystems()
		{
			void TestCase(string purgeSystgem)
			{
				var purgeScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, purgeSystgem);
				using var form = new ArchiveScheduleTaskForm(purgeScheduleTask);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Purge System Parameters", form.parameterGroupbox.Text);

					if (purgeScheduleTask.S5_ScheduleType == ArchiveManagerConstants.Codes.PDO)
					{
						AssertEquals("On or Before (Job Create Date):", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
						AssertEquals("On or Before (Job Create Date):", form.archiveOnOrBeforeDateRadioButton.Text);

						purgeScheduleTask.ShouldIncludeRecordsWithJobs = false;

						AssertEquals("On or Before (Create Date):", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
						AssertEquals("On or Before (Create Date):", form.archiveOnOrBeforeDateRadioButton.Text);
					}
					else
					{
						AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeRelativeDateRadioButton.Text);
						AssertEquals("Purge Records On or Before:", form.archiveOnOrBeforeDateRadioButton.Text);
					}

					Assert("Purge alert label should display for purge system.", form.alertLabel.Visible);
				});
			}

			TestCase(ArchiveManagerConstants.Codes.PAR);
			TestCase(ArchiveManagerConstants.Codes.PDR);
			TestCase(ArchiveManagerConstants.Codes.PDO);
		}

		#endregion

		#region DateParameterDropEdit

		public void TestDateParameterDropEditHasCorrectVisibility()
		{
			var archiveSystemDescriptors = new ArchiveSystemDescriptorLoader().Load();

			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using var form = new ArchiveScheduleTaskForm(archiveScheduleTask);
			form.Show();
			Assert("Precondition", !form.dateParameterDropEdit.Visible);

			foreach (var system in archiveSystemDescriptors)
			{
				archiveScheduleTask.S5_ScheduleType = system.Code;

				if (system.AllowDateParameterSelection)
				{
					Assert($"Date Parameter drop down should be visible if AllowDateParameterSelection=true. Current system descriptor was '{system}'.", form.dateParameterDropEdit.Visible);
				}
				else
				{
					Assert("Date Parameter drop down should not be visible if AllowDateParameterSelection=false. Current system descriptor was '{system}'.", !form.dateParameterDropEdit.Visible);
				}
			}
		}

		public void TestDateParameterCaptionsAreCorrectForDateParameterDropEdit()
		{
			using var form = new ArchiveScheduleTaskForm(Factory.New<ArchiveScheduleTask>());
			form.Show();
			AssertEquals("Select Date Parameter", form.dateParameterDropEdit.CaptionResourceString.Caption);
		}

		#endregion

		#region PurgeSecurityRights

		public void TestPurgeSecurityRightsWorkCorrectly()
		{
			void TestCase(string code, bool isArchiveRecordsPurgeAllowed, bool isArchiveSchedulePurgeAllowed, int onOrBeforeMinimum)
			{
				Env.Security.ArchiveRecordsPurge.IsAllowed = isArchiveRecordsPurgeAllowed;
				Env.Security.ArchiveSchedulePurge.IsAllowed = isArchiveSchedulePurgeAllowed;

				var factoryForSchedule = new BusinessObjectFactory();
				var purgeSchedule = factoryForSchedule.New<ArchiveScheduleTask>();
				purgeSchedule.S5_ScheduleType = code;
				PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(purgeSchedule, -onOrBeforeMinimum);

				CombineAssertions(() =>
				{
					AssertEquals("Precondition", 0, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, code)).Length);
					Assert("Should be able to select PAR system as security right is allowed.", !UnitTestUserNotification.Instance.LastMessage.WasError);
				});

				using (var form = new ArchiveScheduleTaskForm(purgeSchedule))
				{
					form.Show();
					var formSaved = form.FireSaveButton();

					CombineAssertions("Form should be saved without errors because purging archived records is allowed.", () =>
					{
						Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals(ContinueWithSave.Yes, formSaved);
					});
				}

				AssertEquals("The form was saved and the schedule should exist in the database.", 1, Factory.Load<ArchiveScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, code)).Length);

				UnitTestUserNotification.Instance.ClearMessages();
			}

			TestCase(ArchiveManagerConstants.Codes.PAR, isArchiveRecordsPurgeAllowed: true, isArchiveSchedulePurgeAllowed: false, SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Value);
			TestCase(ArchiveManagerConstants.Codes.PDR, isArchiveRecordsPurgeAllowed: false, isArchiveSchedulePurgeAllowed: true, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Value);
			TestCase(ArchiveManagerConstants.Codes.PDO, isArchiveRecordsPurgeAllowed: false, isArchiveSchedulePurgeAllowed: true, SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value);
		}

		public void TestCannotSelectScheduleType_WithoutCorrectSecurityRights()
		{
			void TestCase(string notificationText, string code)
			{
				var userNotification = $@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Archive Manager -> {notificationText} -> Purge";

				Env.Security.ArchiveRecordsPurge.IsAllowed = false;
				Env.Security.ArchiveSchedulePurge.IsAllowed = false;

				var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

				using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
				{
					form.Show();
					archiveScheduleTask.S5_ScheduleType = code;

					AssertEquals($"Should not be able to select {code} system without security right.", userNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessages();
			}

			TestCase("Archived Records", ArchiveManagerConstants.Codes.PAR);
			TestCase("Archive Schedule", ArchiveManagerConstants.Codes.PDO);
			TestCase("Archive Schedule", ArchiveManagerConstants.Codes.PDR);
		}

		public void TestFormDoesNotSave_WithoutCorrectSecurityRights()
		{
			void TestCase(string notificationText, string code)
			{
				var userNotification = $@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Archive Manager -> {notificationText} -> Purge";

				Env.Security.ArchiveRecordsPurge.IsAllowed = false;
				Env.Security.ArchiveSchedulePurge.IsAllowed = false;

				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, code);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(archiveScheduleTask, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);

				AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);

				UnitTestUserNotification.Instance.ClearMessages();
			}

			TestCase("Archived Records", ArchiveManagerConstants.Codes.PAR);
			TestCase("Archive Schedule", ArchiveManagerConstants.Codes.PDR);
			TestCase("Archive Schedule", ArchiveManagerConstants.Codes.PDO);
		}

		public void TestPurgeSecurityRights_WithPARScheduleType_ButOnlyPDRScheduleTypeAllowed()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = false;
			Env.Security.ArchiveSchedulePurge.IsAllowed = true;

			const string userNotification = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Archive Manager -> Archived Records -> Purge";

			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.PAR;

				AssertEquals(userNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPurgeSecurityRights_WithPDRScheduleType_ButOnlyPARScheduleTypeAllowed()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = true;
			Env.Security.ArchiveSchedulePurge.IsAllowed = false;

			const string userNotification = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Archive Manager -> Archive Schedule -> Purge";

			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.PDR;

				AssertEquals(userNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPurgeSecurityRightsOnSave_WithPARSchedule_AndPARSecurityRightIsFalse()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = false;
			Env.Security.ArchiveSchedulePurge.IsAllowed = true;

			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = true;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default).AddDays(15);

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				_ = form.FireSaveButton();

				Assert("The security right for PDR should not affect the security right for PAR.", UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPurgeSecurityRightsOnSave_WithPDRSchedule_AndPDRSecurityRightIsFalse()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = true;
			Env.Security.ArchiveSchedulePurge.IsAllowed = false;

			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = true;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default).AddDays(15);

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				_ = form.FireSaveButton();

				Assert("The security right for PAR should not affect the security right for PDR.", UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPurgeSecurityRightsOnSave_WithPDRSchedule_AndPDRSecurityRightIsTrue()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = false;
			Env.Security.ArchiveSchedulePurge.IsAllowed = true;

			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = true;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-8);
			PopulateArchiveScheduleDateParameterIfScheduleTypeAllows(archiveScheduleTask);

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				_ = form.FireSaveButton();

				Assert("The security right for PAR should not affect the security right for PDR.", !UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPurgeSecurityRightsOnSave_WithPARSchedule_AndPARSecurityRightIsTrue()
		{
			Env.Security.ArchiveRecordsPurge.IsAllowed = true;
			Env.Security.ArchiveSchedulePurge.IsAllowed = false;

			var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = true;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-8);

			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				_ = form.FireSaveButton();

				Assert("The security right for PDR should not affect the security right for PAR.", !UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		#region ShowPreSaveDialogsForArchive

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsTooRecentForRegistryValue_AndRegistryValueIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 2;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {tempOnOrBeforeMinimum} year(s) or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate is too recent for registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsEqualToMostRecentAllowedDateInRegistry_AndRegistryValueIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 1;
			var userNotification = $"You have selected to archive records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default} year(s) old. Please confirm the data retention requirements for this data before archiving.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate equal to most recent allowed date in registry", ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum), archiveScheduleTask.ArchiveRecordsOnOrBeforeDate);
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsOlderThanMostRecentAllowedDateInRegistry_AndArchiveRecordsOnOrBeforeDateIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 2;
			var userNotification = $"You have selected to archive records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default} year(s) old. Please confirm the data retention requirements for this data before archiving.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate older than most recent allowed date in registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate not old enough for minimum data retention requirement years", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsOldEnoughForMinimumDataRetention_AndRegistryValueIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 1;

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 2, null, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate is old enough for minimum data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsTooRecentForRegistry_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {tempOnOrBeforeMinimum} year(s) or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate is too recent to be allowed by the registry value", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default, tempOnOrBeforeMinimum);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsTheExactMostRecentAllowedDate_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, null, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate is the exact most recent date allowed by the registry value", ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum), archiveScheduleTask.ArchiveRecordsOnOrBeforeDate);
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default, tempOnOrBeforeMinimum);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsOldEnoughForMinimumDataRetention_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, null, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate is older than the minimum data retention date", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default, tempOnOrBeforeMinimum);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsTooRecentForMinimumDataRetention_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {tempOnOrBeforeMinimum} year(s) or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate is too recent for minimum data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsEqualToMinimumDataRetentionRequirementDate_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {tempOnOrBeforeMinimum} year(s) or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate is too recent for the registry value", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateSatisfiesDataRetentionButNotRegistryValue()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 2;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} year(s) or older may be archived. Please enter in a value of {tempOnOrBeforeMinimum} year(s) or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate not old enough registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateIsEqualToRegistryDate_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, null, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate equal to registry value", ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum), archiveScheduleTask.ArchiveRecordsOnOrBeforeDate);
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_WhenArchiveRecordsOnOrBeforeDateOlderThanRegistryDate_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, null, ArchiveManagerConstants.Codes.OPS, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate older than registry value", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForArchive_ForHVLArchiveSystem_WhenArchiveRecordsOnOrBeforeDateIsLessThanMinimumDataRetentionRequirementYears()
		{
			using (SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.HAR - 1))
			{
				Assert("Precondition", SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Value < ArchiveManagerConstants.MinimumDataRetentionRequirementYears.HAR);

				var userNotification = $"You have selected to archive records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.HAR} year(s) old. Please confirm the data retention requirements for this data before archiving.";

				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(archiveScheduleTask, -SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Value);
				var archiveScheduleTaskRelativeDate = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(archiveScheduleTaskRelativeDate, 1, "M");

				CombineAssertions(() =>
				{
					AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);
					AssertUserNotificationIsCorrectOnSave(archiveScheduleTaskRelativeDate, userNotification);
				});
			}
		}

		#endregion

		#region ShowPreSaveDialogsForPurge

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsTooRecentForRegistry_AndRegistryValueIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 2;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} years or older may be purged. Please enter in a value of {tempOnOrBeforeMinimum} years or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate more recent than what registry allows", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsEqualToRegistryDate_AndRegistryValueIsLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 1;
			var userNotification = $"You have selected to purge records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default} years old. This action is not reversible. Please confirm the data retention requirements for this data before purging.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate equal to what registry allows", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsAllowedByRegistryButNotOldEnoughForMinimumDataRetention()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 2;
			var userNotification = $"You have selected to purge records that are less than {ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default} years old. This action is not reversible. Please confirm the data retention requirements for this data before purging.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate old enough to be allowed be registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate not old enough for minimum data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsOldEnoughForMinimumDataRetention_AndRegistryIsValueLessThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default - 1;
			var userNotification = $"You have selected to purge records that are older than {tempOnOrBeforeMinimum} years. This action is not reversible; Please confirm the targeted records are no longer needed.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 2, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate old enough for minimum data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
				AssertLessThan("Precondition - Registry value less than minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsTooRecentForRegistry_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} years or older may be purged. Please enter in a value of {tempOnOrBeforeMinimum} years or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate too recent for registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsJustOldEnough_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			var userNotification = $"You have selected to purge records that are older than {tempOnOrBeforeMinimum} years. This action is not reversible; Please confirm the targeted records are no longer needed.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate just old enough for registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsOldEnoughForMinimumDataRetention_AndRegistryValueIsEqualToMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			var userNotification = $"You have selected to purge records that are older than {tempOnOrBeforeMinimum} years. This action is not reversible; Please confirm the targeted records are no longer needed.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate old enough for data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
				AssertEquals("Precondition - Registry value equal to minimum data retention requirement years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsNotOldEnoughForDataRetention_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} years or older may be purged. Please enter in a value of {tempOnOrBeforeMinimum} years or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 2, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertGreaterThan("Precondition - ArchiveRecordsOnOrBeforeDate not old enough for data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsEqualToMinimumDataRetentionRequirement_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} years or older may be purged. Please enter in a value of {tempOnOrBeforeMinimum} years or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate just old enough for data retention", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsOlderThanDataRetentionRequirement_AndRegistryValueIsGreaterThanPurgeRecordsOnOrBeforeMinimum()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 2;
			var userNotification = $"Only records that are {tempOnOrBeforeMinimum} years or older may be purged. Please enter in a value of {tempOnOrBeforeMinimum} years or more to continue.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum - 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate older than data retention requirement", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateJustOldEnoughForRegistry_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"You have selected to purge records that are older than {tempOnOrBeforeMinimum} years. This action is not reversible; Please confirm the targeted records are no longer needed.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertEquals("Precondition - ArchiveRecordsOnOrBeforeDate just old enough for registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPurge_WhenPurgeRecordsOnOrBeforeDateIsOldEnoughForRegistry_AndRegistryValueIsGreaterThanMinimumDataRetentionRequirementYears()
		{
			const int tempOnOrBeforeMinimum = ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1;
			var userNotification = $"You have selected to purge records that are older than {tempOnOrBeforeMinimum} years. This action is not reversible; Please confirm the targeted records are no longer needed.";

			AssertPreSaveDialogs(tempOnOrBeforeMinimum, tempOnOrBeforeMinimum + 1, userNotification, ArchiveManagerConstants.Codes.PDR, (archiveScheduleTask) =>
			{
				AssertLessThan("Precondition - ArchiveRecordsOnOrBeforeDate old enough for registry", archiveScheduleTask.ArchiveRecordsOnOrBeforeDate, ZDateTime.Today.AddYears(-tempOnOrBeforeMinimum));
				AssertGreaterThan("Precondition - Registry value greater than minimum data retention years", tempOnOrBeforeMinimum, ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}, SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum);
		}

		public void TestShowPreSaveDialogsForPDO_WhenErrorMessageIsShown()
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var minimumYears = SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value;
				var userNotification = $"Only documents of records that are {minimumYears} years or older may be purged. Please enter in a value of {minimumYears} years or more to continue.";

				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(archiveScheduleTask, minimumYears - 1, "Y");

				AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);
			}
		}

		public void TestShowPreSaveDialogsForPDO_WhenConfirmationMessageIsShown()
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			{
				var minimumYears = SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value;
				const string userNotification = "You have selected to purge documents of records that are less than 10 years old, and the documents may be less than 10 years old. This action is not reversible; Please confirm the documents are no longer needed.";

				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(archiveScheduleTask, minimumYears, "Y");

				AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);
			}
		}

		public void TestShowPreSaveDialogsForPDO_WhenConfirmationMessageIsShown2()
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var minimumYears = SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Value;
				var userNotification = $"You have selected to purge documents of records that are older than {minimumYears} years, and the documents may be less than {minimumYears} years old. This action is not reversible; Please confirm the documents are no longer needed.";

				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(archiveScheduleTask, minimumYears + 1, "Y");

				AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);
			}
		}

		#endregion

		void AssertPreSaveDialogs(int minimumYears, int relativeYears, string userNotification, string archiveSystemCode, Action<ArchiveScheduleTask> assertPreconditionsAction, IntRegistryItem registryItemToSet)
		{
			using (registryItemToSet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, minimumYears))
			{
				var archiveScheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, archiveSystemCode);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(archiveScheduleTask, -relativeYears);

				var archiveScheduleTaskRelativeDate = TestHelpers.GetArchiveScheduleTask(Factory, archiveSystemCode);
				PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(archiveScheduleTaskRelativeDate, relativeYears, "Y");

				assertPreconditionsAction(archiveScheduleTask);

				CombineAssertions("Precondition: both archive schedule tasks should have the same effective on or before date", () =>
				{
					AssertEquals("Using years for on or before relative type", "Y", archiveScheduleTaskRelativeDate.ArchiveRecordsOnOrBeforeType);
					AssertEquals("The effective date is equal", ZDateTime.Today.AddYears(-archiveScheduleTaskRelativeDate.ArchiveRecordsOnOrBeforeNumber), archiveScheduleTask.ArchiveRecordsOnOrBeforeDate);
				});

				CombineAssertions(() =>
				{
					AssertUserNotificationIsCorrectOnSave(archiveScheduleTask, userNotification);
					AssertUserNotificationIsCorrectOnSave(archiveScheduleTaskRelativeDate, userNotification);
				});
			}
		}

		void AssertUserNotificationIsCorrectOnSave(ArchiveScheduleTask archiveScheduleTask, string userNotification = null)
		{
			using (var form = new ArchiveScheduleTaskForm(archiveScheduleTask))
			{
				form.Show();
				_ = form.FireSaveButton();
				AssertEquals(userNotification, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		void PopulateArchiveScheduleTaskOnOrBeforeMinimumDate(ArchiveScheduleTask archiveScheduleTask, int years)
		{
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = true;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(years);
			PopulateArchiveScheduleDateParameterIfScheduleTypeAllows(archiveScheduleTask);
		}

		void PopulateArchiveScheduleTaskOnOrBeforeMinimumRelativeDate(ArchiveScheduleTask archiveScheduleTask, int onOrBeforeValue, string onOrBeforeType)
		{
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate = false;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeNumber = onOrBeforeValue;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeType = onOrBeforeType;
			PopulateArchiveScheduleDateParameterIfScheduleTypeAllows(archiveScheduleTask);
		}

		void PopulateArchiveScheduleDateParameterIfScheduleTypeAllows(ArchiveScheduleTask archiveScheduleTask)
		{
			var archiveSystemDescriptors = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader()).ArchiveSystemDescriptors;
			var currentDescriptor = archiveSystemDescriptors.First(descriptor => descriptor.Code == archiveScheduleTask.S5_ScheduleType);

			if (currentDescriptor.AllowDateParameterSelection)
			{
				archiveScheduleTask.DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);
			}
		}
	}
}
