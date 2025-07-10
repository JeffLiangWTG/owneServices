using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public class SharedBoardFactoryProvider : BoardFactoryProvider
	{
		public SharedBoardFactoryProvider(BoardSlideshowViewModel viewModel = null)
		{
			this.viewModel = viewModel;

			sharedBoardGuiThreadFactory = new ReadOnlyBusinessObjectFactory();
			sharedBoardGuiThreadFactory.SuspendValidation();

			Initialise(sharedBoardGuiThreadFactory, (NoResString)"Shared Board Factory"); // Used in BusinessObjectFactory.NameForDebugging
		}

		readonly ReadOnlyBusinessObjectFactory sharedBoardGuiThreadFactory;
		readonly BoardSlideshowViewModel viewModel;

		public override BusinessObjectFactory GetBoardGUIThreadFactory()
		{
			return sharedBoardGuiThreadFactory;
		}

		public override ReadOnlyBusinessObjectFactory GetNewBackgroundThreadLoaderFactory(string nameForDebugging)
		{
			ReadOnlyBusinessObjectFactory factory;
			if (ObjectFactory.Get<IBMSRegistry>().BoardOnSecondaryServer)
			{
				factory = new ReadOnlyBusinessObjectFactory(GetSecondaryServerConnectionProvider().GetNewConnectionWrapper().Connection);
#pragma warning disable CW1161 // Res.GetString Analyzer, just for debugging
				nameForDebugging += " (Secondary)";
#pragma warning restore CW1161 
			}
			else
			{
				factory = new ReadOnlyBusinessObjectFactory();
			}

			factory.SuspendValidation();
			factory.RefreshEnabled = false;

			Initialise(factory, nameForDebugging);

			return factory;
		}

		public override BusinessObjectFactory GetNewEditFactory(string nameForDebugging)
		{
			var result = new BusinessObjectFactory();
			Initialise(result, nameForDebugging);

			return result;
		}

		public override ISecondaryServerConnectionProvider GetSecondaryServerConnectionProvider()
		{
			return SecondaryServerConnectionProviderProvider.GetProvider();
		}

		void Initialise(BusinessObjectFactory factory, string name)
		{
			factory.NameForDebugging = name;

			if (viewModel != null)
			{
				lock (viewModel.ServiceLocker)
				{
					foreach (var service in viewModel.Services)
					{
						factory.ServiceContainer.AddService(service, service.GetType());
					}
				}
			}
		}

		public override BoardFactoryProvider Clone(BoardSlideshowViewModel slideshowViewModel)
		{
			return new SharedBoardFactoryProvider(slideshowViewModel);
		}
	}
}
