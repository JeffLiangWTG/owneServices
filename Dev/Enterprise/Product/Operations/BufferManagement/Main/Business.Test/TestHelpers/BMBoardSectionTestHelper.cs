using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BMBoardSectionTestHelper : BMSTestHelper
	{
		public static void RemoveAndDeleteAllPrimaryAxisChannels(BMBoardSection section)
		{
			section.SectionConfiguration.PrimaryAxisChannels.DeleteAll();
		}

		public static void SetBufferZoneColors(BMBoardSection section, ZString zone0, ZString zone1, ZString zone2, ZString zone3)
		{
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.BufferZone0Color = zone0;
			sectionConfiguration.BufferZone1Color = zone1;
			sectionConfiguration.BufferZone2Color = zone2;
			sectionConfiguration.BufferZone3Color = zone3;
		}

		public static void SetCountdownTargetColorAndStyle(BMBoardSection section, string countdownTargetBorderColor, string countdownBorderStyle, string countdownStartableBorderColor, string countdownStartableBorderStyle)
		{
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CountdownTargetBorderColor = countdownTargetBorderColor;
			sectionConfiguration.CountdownTargetBorderStyle = countdownBorderStyle;
			sectionConfiguration.CountdownStartableBorderColor = countdownStartableBorderColor;
			sectionConfiguration.CountdownStartableBorderStyle = countdownStartableBorderStyle;
		}

		public static int GetPrimaryAxisChannelCount(BMBoardSection section)
		{
			return section.SectionConfiguration.PrimaryAxisChannels.Count;
		}

		public static int GetSecondaryAxisChannelCount(BMBoardSection section)
		{
			return section.SectionConfiguration.SecondaryAxisChannels.Count;
		}

		public static int GetPrimaryAxisChannelCount(BMComponentSectionConfiguration sectionConfiguration)
		{
			return sectionConfiguration.PrimaryAxisChannels.Count;
		}

		public static int GetSecondaryAxisChannelCount(BMComponentSectionConfiguration sectionConfiguration)
		{
			return sectionConfiguration.SecondaryAxisChannels.Count;
		}

		public static void SetValuesForSectionConfiguration(
			BMBoardSection section,
			ZGuid releaseGroupPK = default(ZGuid),
			string flowDirection = FlowDirectionList.Codes.Up,
			int cellsPerSubsection = 4,
			string timeProgressionMode = TimeProgressionModeList.Codes.Due,
			ZDateTime timePerCell = default(ZDateTime),
			string timeField = TimeProgressionFieldList.Codes.AgreedDeliveryDate,
			int maxOverdueSlots = 1,
			int subSections = 1)
		{
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = releaseGroupPK;
			sectionConfiguration.FlowDirection = flowDirection;
			sectionConfiguration.CellsPerSubsection = cellsPerSubsection;

			sectionConfiguration.TimeProgressionMode = timeProgressionMode;
			sectionConfiguration.TimePerCell = timePerCell;
			sectionConfiguration.TimeField = timeField;
			sectionConfiguration.MaxOverdueSlots = maxOverdueSlots;

			sectionConfiguration.Subsections = subSections;
		}

		public static ComponentGrid CreateComponentGrid(BMBoardSectionViewModel viewModel, BMBoardSection section)
		{
			return new ComponentGrid(viewModel.PrimaryChannels, viewModel.SecondaryAxisChannels, section, viewModel.IsPreview, viewModel.IsInConstrainedMode);
		}
	}
}
