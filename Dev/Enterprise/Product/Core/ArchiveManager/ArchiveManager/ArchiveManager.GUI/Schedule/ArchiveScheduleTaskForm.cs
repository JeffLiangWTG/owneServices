using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ArchiveManager.GUI.Schedule
{
	public partial class ArchiveScheduleTaskForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ArchiveScheduleTaskForm()
		{
			InitializeComponent();
		}

		public ArchiveScheduleTaskForm(ArchiveScheduleTask bo)
			: base(bo)
		{
			InitializeComponent();
			archiveScheduleTask = bo;
			bo.S5_ScheduleTypeInfo.ValueChanged += new EventHandler(S5_ScheduleTypeInfo_ValueChanged);
			bo.ShouldIncludeRecordsWithJobsInfo.ValueChanged += new EventHandler(S5_ShouldIncludeRecordsWithJobsForPDOInfoInfo_ValueChanged);
			archiveSystemDescriptors = new ArchiveSystemDescriptorLoader().Load().ToList();
			UpdateCaption();
			UpdateArchiveDecCheckBoxVisibility();
			UpdateDateParameterDropEditVisibility();
			UpdateWithOrWithoutJobsParametersVisibility();
			UpdateUseOnOrBeforeDateWhenWatermarkResetCheckBoxVisibility();
			UpdateOnOrBeforeRadioButtonText();
			MissingResourceStringChecker.ExcludeFromTest(onOrBeforeCalcEdit);
		}

		readonly ArchiveScheduleTask archiveScheduleTask;
		readonly List<IArchiveSystemDescriptor> archiveSystemDescriptors;

		void S5_ScheduleTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateCaption();
			UpdateArchiveDecCheckBoxVisibility();
			UpdateDateParameterDropEditVisibility();
			UpdateWithOrWithoutJobsParametersVisibility();
			UpdateUseOnOrBeforeDateWhenWatermarkResetCheckBoxVisibility();
			_ = HaveCorrectSecurityRightsForScheduleTask();
		}

		void S5_ShouldIncludeRecordsWithJobsForPDOInfoInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateOnOrBeforeRadioButtonText();
		}

		int GetMinimumDataRetentionRequirementsYears()
		{
			switch (archiveSystemDropDown.CodeBox.Text)
			{
				case (ArchiveManagerConstants.Codes.PDO):
					return ArchiveManagerConstants.MinimumDataRetentionRequirementYears.PDO;

				case (ArchiveManagerConstants.Codes.HAR):
					return ArchiveManagerConstants.MinimumDataRetentionRequirementYears.HAR;

				default:
					return ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;
			}
		}

		bool HaveCorrectSecurityRightsForScheduleTask()
		{
			var checkpoint = SecurityCheckpointHelper.GetCheckpoint(archiveScheduleTask.S5_ScheduleType);

			if (checkpoint != null)
			{
				if (!checkpoint.IsAllowed)
				{
					Globals.Message.ShowError(checkpoint.ErrorMessageForNotAllowed);
					return false;
				}

				return true;
			}

			return false;
		}

		void UpdateCaption()
		{
			if (archiveScheduleTask.IsPurgeSystem)
			{
				parameterGroupbox.Text = ArchiveScheduleTaskFormMessages.GetPurgeSystemParametersMessage();
				alertLabel.Visible = true;
				UpdateOnOrBeforeRadioButtonText();
			}
			else
			{
				parameterGroupbox.Text = parameterGroupbox.CaptionResourceString.Caption;
				alertLabel.Visible = false;
				UpdateOnOrBeforeRadioButtonText();
			}
		}

		void UpdateOnOrBeforeRadioButtonText()
		{
			var onOrBeforeRadioButtonText = ArchiveScheduleTaskFormMessages.GetOnOrBeforeRadioButtonText(archiveScheduleTask.S5_ScheduleType, withJobs: archiveScheduleTask.ShouldIncludeRecordsWithJobs);
			archiveOnOrBeforeRelativeDateRadioButton.Text = onOrBeforeRadioButtonText;
			archiveOnOrBeforeDateRadioButton.Text = onOrBeforeRadioButtonText;
		}

		void UpdateDateParameterDropEditVisibility()
		{
			var currentDescriptor = archiveSystemDescriptors.Where(descriptor => descriptor.Code == archiveScheduleTask.S5_ScheduleType);

			dateParameterDropEdit.Visible = currentDescriptor.Any() && currentDescriptor.First().AllowDateParameterSelection;
		}

		void UpdateWithOrWithoutJobsParametersVisibility()
		{
			var currentDescriptor = archiveSystemDescriptors.Where(descriptor => descriptor.Code == archiveScheduleTask.S5_ScheduleType);

			JobsOptionsPanel.Visible = currentDescriptor.Any() && currentDescriptor.First().AllowRecordsWithOrWithoutJobs;
		}

		void UpdateUseOnOrBeforeDateWhenWatermarkResetCheckBoxVisibility()
		{
			var currentDescriptor = archiveSystemDescriptors.Where(descriptor => descriptor.Code == archiveScheduleTask.S5_ScheduleType);

			useOnOrBeforeDateWhenWatermarkResetCheckBox.Visible = currentDescriptor.Any() && currentDescriptor.First().AllowUsingOnOrBeforeDateWhenWatermarkReset;
		}

		void UpdateArchiveDecCheckBoxVisibility()
		{
			var currentDescriptor = archiveSystemDescriptors.Where(descriptor => descriptor.Code == archiveScheduleTask.S5_ScheduleType);

			archiveDecCheckBox.Visible = currentDescriptor.Any() && currentDescriptor.First().AllowShouldArchiveDeclaration;
		}

		void archiveOnOrBeforeTypeRadioButton_CheckedChanged(object sender, EventArgs e)
			=> ToggleOnOrBeforeDateBoxes();

		void archiveOnOrBeforeDateRadioButton_CheckedChanged(object sender, EventArgs e)
			=> ToggleOnOrBeforeDateBoxes();

		void ToggleOnOrBeforeDateBoxes()
		{
			onOrBeforeCalcEdit.Enabled = archiveOnOrBeforeRelativeDateRadioButton.Checked;
			onOrBeforeTypeDropEdit.Enabled = archiveOnOrBeforeRelativeDateRadioButton.Checked;
			onOrBeforeDateEdit.Enabled = archiveOnOrBeforeDateRadioButton.Checked;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes)
			{
				return archiveScheduleTask.IsPurgeSystem
					? ShowPreSaveDialogsForPurge()
					: ShowPreSaveDialogsForArchive();
			}
			else
			{
				return result;
			}
		}

		ContinueWithSave ShowPreSaveDialogsForArchive()
		{
			var archiveRecordsOnOrBeforeDate = GetArchiveOnOrBeforeDate();
			var archiveRecordsOnOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(archiveSystemDropDown.CodeBox.Text);
			var archiveRecordsOnOrBeforeDateRegistryValue = ZDateTime.Today.AddYears(-archiveRecordsOnOrBeforeMinimumValue);

			if (archiveRecordsOnOrBeforeDate > archiveRecordsOnOrBeforeDateRegistryValue)
			{
				var archiveErrorMessage = ArchiveScheduleTaskFormMessages.GetArchiveRecordsAreTooNewErrorMessage(archiveRecordsOnOrBeforeMinimumValue);
				Globals.Message.ShowError(archiveErrorMessage);
				return ContinueWithSave.No;
			}
			else
			{
				if (archiveRecordsOnOrBeforeDate > ZDateTime.Today.AddYears(-GetMinimumDataRetentionRequirementsYears()))
				{
					var archiveConfirmationMessageLessThanRegistryOnOrBeforeDate = ArchiveScheduleTaskFormMessages.GetArchiveConfirmationMessage(GetMinimumDataRetentionRequirementsYears());
					return GetConfirmationDialog(archiveConfirmationMessageLessThanRegistryOnOrBeforeDate);
				}
				else
				{
					return ContinueWithSave.Yes;
				}
			}
		}

		ContinueWithSave ShowPreSaveDialogsForPurge()
		{
			var purgeRecordsOnOrBeforeDate = GetArchiveOnOrBeforeDate();
			var purgeRecordsOnOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(archiveSystemDropDown.CodeBox.Text);
			var purgeRecordsOnOrBeforeDateRegistryValue = ZDateTime.Today.AddYears(-purgeRecordsOnOrBeforeMinimumValue);

			if (!HaveCorrectSecurityRightsForScheduleTask())
			{
				return ContinueWithSave.No;
			}
			else
			{
				if (purgeRecordsOnOrBeforeDate > purgeRecordsOnOrBeforeDateRegistryValue)
				{
					Globals.Message.ShowError(ArchiveScheduleTaskFormMessages.GetPurgeErrorMessage(archiveSystemDropDown.CodeBox.Text, purgeRecordsOnOrBeforeMinimumValue));
					return ContinueWithSave.No;
				}
				else
				{
					var nonArchivedRecordsData = archiveSystemDropDown.CodeBox.Text != ArchiveManagerConstants.Codes.PAR;
					if (purgeRecordsOnOrBeforeDate > ZDateTime.Today.AddYears(-GetMinimumDataRetentionRequirementsYears()) && nonArchivedRecordsData)
					{
						return GetConfirmationDialog(ArchiveScheduleTaskFormMessages.GetPurgeConfirmationMessage(archiveSystemDropDown.CodeBox.Text));
					}
					else
					{
						return GetConfirmationDialog(ArchiveScheduleTaskFormMessages.GetPurgeConfirmationMessage(archiveSystemDropDown.CodeBox.Text, purgeRecordsOnOrBeforeMinimumValue));
					}
				}
			}
		}

		ZDateTime GetArchiveOnOrBeforeDate()
		{
			var archiveBeforeDate = ZDateTime.Empty;

			if (archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate)
			{
				archiveBeforeDate = archiveScheduleTask.GetArchiveOnOrBeforeDateFromRelativeDate();
			}
			else if (archiveScheduleTask.IsArchiveRecordsOnOrBeforeDate)
			{
				archiveBeforeDate = archiveScheduleTask.ArchiveRecordsOnOrBeforeDate;
			}

			return archiveBeforeDate;
		}

		ContinueWithSave GetConfirmationDialog(string confirmationMessage)
		{
			var caption = ArchiveScheduleTaskFormMessages.GetConfirmationCaptionMessage();
			var confirmationString = ArchiveScheduleTaskFormMessages.GetConfirmationMessage();
			var result = Globals.Message.ShowConfirmation(
				confirmationMessage,
				caption,
				ArchiveScheduleTaskFormMessages.GetConfirmationPrompt(archiveScheduleTask),
				confirmationString,
				MessageBoxIcon.Warning) == DialogResult.OK
					? ContinueWithSave.Yes
					: ContinueWithSave.No;

			return result;
		}
	}
}
