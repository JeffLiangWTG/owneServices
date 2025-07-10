using System;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common.Native;

namespace CargoWise.Loader.Common
{
	public sealed class ServiceContainer : IServiceContainer
	{
		public static IServiceContainer Instance
		{
			get
			{
				return instance ?? (instance = new ServiceContainer());
			}
		}

		[ThreadStatic]
		static IServiceContainer instance;

		IDirectoryProxy directory;
		IEnvironmentProxy environment;
		IEventLogProxy eventLog;
		IFileProxy file;
		IMessageBoxProxy messageBox;
		INativeMethods nativeMethods;
		IRegistryProxy registry;
		IServiceControllerProxy serviceController;
		ICargoWiseOneInstanceClass cargoWiseOneInstanceClass;

		ServiceContainer()
		{
		}

		public IDirectoryProxy Directory
		{
			get { return directory ?? (directory = new DirectoryProxy()); }
		}

		public IEnvironmentProxy Environment
		{
			get { return environment ?? (environment = new EnvironmentProxy()); }
		}

		public IEventLogProxy EventLog
		{
			get { return eventLog ?? (eventLog = new EventLogProxy()); }
		}

		public IFileProxy File
		{
			get { return file ?? (file = new FileProxy()); }
		}

		public IMessageBoxProxy MessageBox
		{
			get { return messageBox ?? (messageBox = new MessageBoxProxy()); }
		}

		public INativeMethods NativeMethods
		{
			get { return nativeMethods ?? (nativeMethods = new NativeMethods()); }
		}

		public IRegistryProxy Registry
		{
			get { return registry ?? (registry = new RegistryProxy()); }
		}

		public IFileVersionInfoProxy GetFileVersionInfo(string fileName)
		{
			return new FileVersionInfoProxy(fileName);
		}

		public IServiceControllerProxy ServiceController
		{
			get { return serviceController ?? (serviceController = new ServiceControllerProxy()); }
		}

		public ICargoWiseOneInstanceClass CargoWiseOneInstanceClass
		{
			get { return cargoWiseOneInstanceClass ?? (cargoWiseOneInstanceClass = new CargoWiseOneInstanceClass()); }
		}
	}
}

