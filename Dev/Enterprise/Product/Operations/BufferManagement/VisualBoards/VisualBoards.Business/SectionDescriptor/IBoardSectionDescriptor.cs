using System.Collections.Generic;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardSectionDescriptor
	{
		/// <summary>
		/// Used in the MB_SectionType lookup list of available sections.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// The description of this type of section, as displayed in the board section configuration screen.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Used to store specific config for the section.
		/// </summary>
		IBoardSectionConfigurationBizo GetSectionConfigurationBizo(IBMBoardSection section);

		/// <summary>
		/// Used to display additional configuration options within the Configuration group box.
		/// </summary>
		object GetSectionConfigurationControl();

		/// <summary>
		/// Optionally provide extra tabs to add to the section tab control.
		/// </summary>
		IEnumerable<TabSpec> GetAdditionalTabs();

		/// <summary>
		/// The control that is added to the Visual Board form for this BMBoardSection.
		/// </summary>
		IBoardSectionControl GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel);

		/// <summary>
		/// Constructs the view-model used to represent a board section. Just return a BoardSectionViewModel if no sub-class is required.
		/// </summary>
		BoardSectionViewModel GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel);
	}
}
