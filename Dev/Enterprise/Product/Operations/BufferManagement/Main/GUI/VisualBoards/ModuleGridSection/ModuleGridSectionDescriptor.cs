using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class ModuleGridSectionDescriptor : IBoardSectionDescriptor
	{
		string IBoardSectionDescriptor.Type
		{
			get { return BMConstants.ModuleGridSectionType; }
		}

		string IBoardSectionDescriptor.Description
		{
			get { return Res.GetString("251c8e47-c442-4c14-b114-edde4ffd99c9", "Module Grid"); }
		}

		IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
		{
			Argument.NotNull(section, "section");

			return new ModuleGridSectionConfiguration(section);
		}

		object IBoardSectionDescriptor.GetSectionConfigurationControl()
		{
			return new ModuleGridConfigurationControl();
		}

		IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			Argument.NotNull(sectionViewModel, "sectionViewModel");
			return new ModuleSelectorControl((ModuleGridSectionConfiguration)section.Configuration, sectionViewModel);
		}

		BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			return new BoardSectionViewModel((BMBoardSection)section, boardViewModel);
		}

		IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
		{
			return Enumerable.Empty<TabSpec>();
		}
	}
}
