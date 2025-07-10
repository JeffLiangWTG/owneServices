using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.GUI.Schedule
{
	public static class ArchiveScheduleTaskFormMessages
	{
		public static MultilingualString GetPurgeErrorMessage(string code, int onOrBeforeMinimumValue)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.PDO:
					return ResString.GetMultilingualString("D8735C40-16A2-4FC1-874F-79591C87BB16", "Only documents of records that are {0} years or older may be purged. Please enter in a value of {0} years or more to continue.", onOrBeforeMinimumValue);
				default:
					return ResString.GetMultilingualString("5D3F2A92-54CC-4957-AAF7-3111499E2C53", "Only records that are {0} years or older may be purged. Please enter in a value of {0} years or more to continue.", onOrBeforeMinimumValue);
			}
		}

		public static MultilingualString GetPurgeConfirmationMessage(string code, int onOrBeforeMinimumValue)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.PDO:
					return ResString.GetMultilingualString("5DED0DD8-8093-4CE6-A162-5AA346A31CEE", "You have selected to purge documents of records that are older than {0} years, and the documents may be less than {0} years old. This action is not reversible; Please confirm the documents are no longer needed.", onOrBeforeMinimumValue);
				default:
					return ResString.GetMultilingualString("8B9ED41C-2A74-4AB4-9BF1-3BCF423BBF7E", "You have selected to purge records that are older than {0} years. This action is not reversible; Please confirm the targeted records are no longer needed.", onOrBeforeMinimumValue);
			}
		}

		public static MultilingualString GetPurgeConfirmationMessage(string code)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.PDO:
					return ResString.GetMultilingualString("A03E9DDD-080F-4AB0-8E28-3040294C49AC", "You have selected to purge documents of records that are less than {0} years old, and the documents may be less than {0} years old. This action is not reversible; Please confirm the documents are no longer needed.", ArchiveManagerConstants.MinimumDataRetentionRequirementYears.PDO);
				default:
					return ResString.GetMultilingualString("C988B4BE-18AD-489C-B775-9B59F7439935", "You have selected to purge records that are less than {0} years old. This action is not reversible. Please confirm the data retention requirements for this data before purging.", ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default);
			}
		}

		public static MultilingualString GetConfirmationPrompt(ArchiveScheduleTask archiveScheduleTask)
			=> archiveScheduleTask.IsPurgeSystem
				? ResString.GetMultilingualString("10797051-2BB6-4873-AE89-8667137C273C", "To continue with purging this data please confirm this action.")
				: ResString.GetMultilingualString("595EAA6A-7E24-4F07-B598-A6F09FD38D69", "To continue with archiving this data please confirm this action.");

		public static MultilingualString GetOnOrBeforeRadioButtonText(string code, bool withJobs = false)
		{
			switch (code)
			{
				case ArchiveManagerConstants.Codes.PDO:
					if (withJobs)
					{
						return ResString.GetMultilingualString("4ED53675-EE3D-45D5-8920-331E44DE7587", "On or Before (Job Create Date):");
					}
					else
					{
						return ResString.GetMultilingualString("240B0C25-34A1-4EE4-ACAE-19EAD4645197", "On or Before (Create Date):");
					}
				case ArchiveManagerConstants.Codes.PDR:
				case ArchiveManagerConstants.Codes.PAR:
				case ArchiveManagerConstants.Codes.EST:
				case ArchiveManagerConstants.Codes.PAL:
				case ArchiveManagerConstants.Codes.RED:
					return ResString.GetMultilingualString("ACB64E6E-6CB1-4545-AA1A-393A9F416DEF", "Purge Records On or Before:");
				case ArchiveManagerConstants.Codes.IPS:
					return ResString.GetMultilingualString("05AE97C9-4E45-4C0F-9E6E-412619334688", "Archive Records On or Before (Create Date):");
				default:
					return ResString.GetMultilingualString("21A32D91-119F-45BD-B5CE-2EBBB486FA74", "Archive Records On or Before:");
			}
		}

		public static MultilingualString GetArchiveRecordsAreTooNewErrorMessage(int archiveRecordsOnOrBeforeMinimumValueTooNew)
		{
			return ResString.GetMultilingualString("525317B2-4092-4A3C-BE83-20336C39A703", "Only records that are {0} year(s) or older may be archived. Please enter in a value of {0} year(s) or more to continue.", archiveRecordsOnOrBeforeMinimumValueTooNew);
		}

		public static MultilingualString GetArchiveConfirmationMessage(int getMinimumDataRetentionRequirementsYears)
		{
			return ResString.GetMultilingualString("98008CD5-BCAE-4BA3-A0A9-AC8A5B2E08ED", "You have selected to archive records that are less than {0} year(s) old. Please confirm the data retention requirements for this data before archiving.", getMinimumDataRetentionRequirementsYears);
		}

		public static MultilingualString GetPurgeSystemParametersMessage()
		{
			return ResString.GetMultilingualString("066C88B3-BA5D-4043-97E1-F8D40E44E002", "Purge System Parameters");
		}

		public static MultilingualString GetConfirmationCaptionMessage()
		{
			return ResString.GetMultilingualString("42EEE812-068C-4D31-B0C0-A5113C3C543C", "Confirmation");
		}

		public static MultilingualString GetConfirmationMessage()
		{
			return ResString.GetMultilingualString("1B874C10-DAFD-460A-83BB-4ADB077A487D", "Confirm");
		}
	}
}
