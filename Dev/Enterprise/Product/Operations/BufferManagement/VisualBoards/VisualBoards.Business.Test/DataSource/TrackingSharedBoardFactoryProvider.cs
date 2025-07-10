using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.VisualBoards.Business.Test
{
	public class TrackingSharedBoardFactoryProvider : SharedBoardFactoryProvider
	{
		public TrackingSharedBoardFactoryProvider(BoardSlideshowViewModel viewModel)
			: base(viewModel)
		{
		}

		public override ReadOnlyBusinessObjectFactory GetNewBackgroundThreadLoaderFactory(string nameForDebugging)
		{
			var factory = base.GetNewBackgroundThreadLoaderFactory(nameForDebugging);

			createdFactories.Add(factory);

			return factory;
		}

		public override BusinessObjectFactory GetNewEditFactory(string newFactoryNameForDebugging)
		{
			var factory = base.GetNewEditFactory(newFactoryNameForDebugging);

			createdFactories.Add(factory);

			return factory;
		}

		public override BoardFactoryProvider Clone(BoardSlideshowViewModel slideshowViewModel)
		{
			return this; // Don't copy this factory provider since we're holding onto all the factories that get created by this board.
		}

		public IEnumerable<BusinessObjectFactory> CreatedFactories => createdFactories;
		readonly List<BusinessObjectFactory> createdFactories = new List<BusinessObjectFactory>();
	}
}
