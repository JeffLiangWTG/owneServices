using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public abstract class BoardFactoryProvider : IVisualisationFactoryProvider
	{
		public abstract BusinessObjectFactory GetBoardGUIThreadFactory();

		public abstract ReadOnlyBusinessObjectFactory GetNewBackgroundThreadLoaderFactory(string nameForDebugging);

		public abstract BusinessObjectFactory GetNewEditFactory(string newFactoryNameForDebugging);

		public BusinessObjectFactory GetBestFactory(string newFactoryNameForDebugging)
		{
			var mainThreadFactory = GetBoardGUIThreadFactory();

			if (mainThreadFactory.ThreadSentry.IsOwner)
			{
				return mainThreadFactory;
			}
			else
			{
				return GetNewBackgroundThreadLoaderFactory(newFactoryNameForDebugging);
			}
		}

		public abstract ISecondaryServerConnectionProvider GetSecondaryServerConnectionProvider();

		public virtual BoardFactoryProvider Clone(BoardSlideshowViewModel slideshowViewModel)
		{
			return (BoardFactoryProvider)Activator.CreateInstance(GetType());
		}
	}
}
