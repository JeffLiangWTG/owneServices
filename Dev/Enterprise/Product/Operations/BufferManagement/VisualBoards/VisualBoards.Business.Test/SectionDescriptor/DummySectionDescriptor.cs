using System;
using System.Collections.Generic;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business.Test
{
	public class DummySectionDescriptor : IBoardSectionDescriptor, IBoardFactoryServiceProvider
	{
		public DummySectionDescriptor(params IBoardFactoryService[] services)
		{
			this.services = services;
		}

		readonly IBoardFactoryService[] services;

		public string Description => "";

		public string Type => "FRG";

		public IEnumerable<TabSpec> GetAdditionalTabs()
		{
			yield break;
		}

		public IEnumerable<IBoardFactoryService> GetFactoryServices(IVisualBoardProvider source)
		{
			return services;
		}

		public IBoardSectionConfigurationBizo GetSectionConfigurationBizo(IBMBoardSection section)
		{
			throw new NotImplementedException();
		}

		public object GetSectionConfigurationControl()
		{
			throw new NotImplementedException();
		}

		public IBoardSectionControl GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			throw new NotImplementedException();
		}

		public BoardSectionViewModel GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			throw new NotImplementedException();
		}
	}
}
