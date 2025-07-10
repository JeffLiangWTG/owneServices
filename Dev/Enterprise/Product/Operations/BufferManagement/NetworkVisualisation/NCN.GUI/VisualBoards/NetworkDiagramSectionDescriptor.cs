using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class NetworkDiagramSectionDescriptor : IBoardSectionDescriptor
	{
		string IBoardSectionDescriptor.Type
		{
			get { return CCPMConstants.NetworkDiagramSectionType; }
		}

		string IBoardSectionDescriptor.Description
		{
			get { return Res.GetString("985ba573-8574-429f-a491-daa05d5f5b30", "Network Diagram"); }
		}

		IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
		{
			Argument.NotNull(section, "section");

			return new DiagramBoardSectionConfiguration(section.Factory);
		}

		object IBoardSectionDescriptor.GetSectionConfigurationControl()
		{
			return new BoardSectionDiagramConfigurationControl();
		}

		IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
		{
			yield break;
		}

		IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			return new BoardSectionDiagramControl(section, sectionViewModel);
		}

		BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			return new BoardSectionViewModel(section, boardViewModel);
		}
	}
}
