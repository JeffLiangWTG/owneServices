using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	static class ComponentGridBuilderProvider
	{
		internal static void Build(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section,
			bool isPreview,
			bool isInConstrainedMode)
		{
			GetBuilder(grid, primaryChannels, secondaryChannels, section, isPreview, isInConstrainedMode).Build();
		}

		internal static void SetBackgroundColoursAndFades(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section,
			CardAllocationMap allocationMap,
			bool isInConstrainedMode)
		{
			GetBuilder(grid, primaryChannels, secondaryChannels, section, false, isInConstrainedMode).SetBackForeColors();
			grid.SetColorFades(section.SectionConfiguration, allocationMap);
		}

		static ComponentGridBuilderBase GetBuilder(ComponentGrid grid,
			IEnumerable<IChannel> primaryChannels,
			IEnumerable<IChannel> secondaryChannels,
			BMBoardSection section,
			bool isPreview,
			bool isInConstrainedMode)
		{
			if (section.SectionConfiguration.IsBuffer)
			{
				return new BufferGridBuilder(grid, primaryChannels, secondaryChannels, section, isPreview, isInConstrainedMode);
			}
			else
			{
				return new UnBufferedGridBuilder(grid, primaryChannels, secondaryChannels, section);
			}
		}
	}
}
