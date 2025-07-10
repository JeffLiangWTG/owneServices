using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.PAVE.MENT.Shared;

namespace Enterprise.BufferManagement.Service
{
	internal static class SectionFactory
	{
		internal static ISection Create(BMBoardSection section)
		{
			section.Factory.SuspendValidation();
			var sectionConfiguration = section.SectionConfiguration;

			if (section.MS_SectionType != BMConstants.ComponentSectionType ||
				sectionConfiguration.IsReleaseScheduler)
			{
				//WAVE doesn't implemented other types of section, maybe in future WI
				return null;
			}

			//WAVE Client shows tasks Grouped by Workflows
			sectionConfiguration.CardType = CardTypeList.Codes.Task;

			return new ComponentSectionBuilder(section).Build();
		}

		#region Configuration

		internal static ISectionConfiguration CreateConfiguration(BMBoardSection section)
		{
			var type = GetSectionType(section.MS_SectionType);

			switch (type)
			{
				case SectionType.Component:
					return new ComponentSectionConfigurationBuilder(section).Build();
				case SectionType.WebBrowser:
					return new WebBrowserConfigurationBuilder(section).Build();
				default:
					return new SectionConfigurationBuilder(section, type).Build();
			}
		}

		static SectionType GetSectionType(ZString sectionType)
		{
			switch (sectionType)
			{
				case BMConstants.ComponentSectionType:
					return SectionType.Component;
				case BMConstants.ModuleGridSectionType:
					return SectionType.ModuleGrid;
				case MENTConstants.WebBrowserSectionType:
					return SectionType.WebBrowser;
				case MENTConstants.ChartSectionType:
					return SectionType.Chart;
				case CCPMConstants.NetworkDiagramSectionType:
					return SectionType.NetworkDiagram;
				default:
					return SectionType.Unknown;
			}
		}

		#endregion
	}
}
