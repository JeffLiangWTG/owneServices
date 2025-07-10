using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;

namespace Enterprise.PAVE.MENT.GUI
{
	public class WebBrowserSectionDescriptor : IBoardSectionDescriptor
	{
		string IBoardSectionDescriptor.Type
		{
			get { return MENTConstants.WebBrowserSectionType; }
		}

		string IBoardSectionDescriptor.Description
		{
			get { return Res.GetString("380b67aa-d29a-4d4e-b6ba-f1141b36c795", "Web Browser"); }
		}

		IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
		{
			Argument.NotNull(section, "Can only retrieve a section configuration Bizo with a non null section"); // argument exception not required to be translated.

			return new WebBrowserSectionConfiguration(section);
		}

		object IBoardSectionDescriptor.GetSectionConfigurationControl()
		{
			return new WebBrowserSectionConfigurationControl();
		}

		IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
		{
			yield break;
		}

		IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			return new WebBrowserSectionControl(section, sectionViewModel);
		}

		BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			return new BoardSectionViewModel(section, boardViewModel);
		}
	}
}
