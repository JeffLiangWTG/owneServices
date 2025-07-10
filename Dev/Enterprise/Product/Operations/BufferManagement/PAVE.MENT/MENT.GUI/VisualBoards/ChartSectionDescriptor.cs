using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;

namespace Enterprise.PAVE.MENT.GUI
{
	public class ChartSectionDescriptor : IBoardSectionDescriptor
	{
		string IBoardSectionDescriptor.Type
		{
			get { return MENTConstants.ChartSectionType; }
		}

		string IBoardSectionDescriptor.Description
		{
			get { return Res.GetString("786b1311-86aa-4e5a-a331-4cbfc5e45be3", "MENT Chart Section"); }
		}

		IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
		{
			Argument.NotNull(section, "Can only retrieve a section configuration Bizo with a non null section"); // argument exception not required to be translated.

			return new ChartSectionConfiguration(section);
		}

		object IBoardSectionDescriptor.GetSectionConfigurationControl()
		{
			return new ChartSectionConfigurationControl();
		}

		IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
		{
			yield break;
		}

		IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			return new ChartSectionControl(sectionViewModel);
		}

		BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			return new BoardSectionViewModel(section, boardViewModel);
		}
	}
}
