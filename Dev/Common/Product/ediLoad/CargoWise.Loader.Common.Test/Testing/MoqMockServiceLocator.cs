using System;
using System.Collections.Generic;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common.Native;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	public class MoqMockServiceContainer : IServiceContainer
	{
		readonly Dictionary<Type, object> mocks = new Dictionary<Type, object>();

		public MoqMockServiceContainer()
		{
		}

		public MoqMockServiceContainer(MockRepository mocker)
		{
			Mocker = mocker;
		}

		public IDirectoryProxy Directory
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IDirectoryProxy>), out result))
				{
					result = new Mock<IDirectoryProxy>();
					mocks[typeof(Mock<IDirectoryProxy>)] = result;
				}
				return ((Mock<IDirectoryProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IDirectoryProxy>)] = value is Mock ? (Mock<IDirectoryProxy>)value : Mock.Get(value);
			}
		}

		public IEnvironmentProxy Environment
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IEnvironmentProxy>), out result))
				{
					result = new Mock<IEnvironmentProxy>();
					mocks[typeof(Mock<IEnvironmentProxy>)] = result;
				}
				return ((Mock<IEnvironmentProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IEnvironmentProxy>)] = value is Mock ? (Mock<IEnvironmentProxy>)value : Mock.Get(value);
			}
		}

		public Mock<IEnvironmentProxy> EnvironmentForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IEnvironmentProxy>), out result))
				{
					result = new Mock<IEnvironmentProxy>();
					mocks[typeof(Mock<IEnvironmentProxy>)] = result;
				}
				return (Mock<IEnvironmentProxy>)result;
			}
			set
			{
				mocks[typeof(Mock<IEnvironmentProxy>)] = value;
			}
		}

		public IEventLogProxy EventLog
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IEventLogProxy>), out result))
				{
					result = new Mock<IEventLogProxy>();
					mocks[typeof(Mock<IEventLogProxy>)] = result;
				}
				return ((Mock<IEventLogProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IEventLogProxy>)] = value is Mock ? (Mock<IEventLogProxy>)value : Mock.Get(value);
			}
		}

		public Mock<IEventLogProxy> EventLogForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IEventLogProxy>), out result))
				{
					result = new Mock<IEventLogProxy>();
					mocks[typeof(Mock<IEventLogProxy>)] = result;
				}
				return (Mock<IEventLogProxy>)result;
			}
		}

		public string ExpectedFileVersionInfoFileName { get; set; }

		public IFileProxy File
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IFileProxy>), out result))
				{
					result = new Mock<IFileProxy>();
					mocks[typeof(Mock<IFileProxy>)] = result;
				}
				return ((Mock<IFileProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IFileProxy>)] = value is Mock ? (Mock<IFileProxy>)value : Mock.Get(value);
			}
		}

		public Mock<IFileProxy> FileForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IFileProxy>), out result))
				{
					result = new Mock<IFileProxy>();
					mocks[typeof(Mock<IFileProxy>)] = result;
				}
				return (Mock<IFileProxy>)result;
			}
		}

		public IFileVersionInfoProxy FileVersionInfo
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IFileVersionInfoProxy>), out result))
				{
					result = new Mock<IFileVersionInfoProxy>();
					mocks[typeof(Mock<IFileVersionInfoProxy>)] = result;
				}
				return ((Mock<IFileVersionInfoProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IFileVersionInfoProxy>)] = value is Mock ? (Mock<IFileVersionInfoProxy>)value : Mock.Get(value);
			}
		}

		public IMessageBoxProxy MessageBox
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IMessageBoxProxy>), out result))
				{
					result = new Mock<IMessageBoxProxy>();
					mocks[typeof(Mock<IMessageBoxProxy>)] = result;
				}
				return ((Mock<IMessageBoxProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IMessageBoxProxy>)] = value is Mock ? (Mock<IMessageBoxProxy>)value : Mock.Get(value);
			}
		}

		public Mock<IMessageBoxProxy> MessageBoxForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IMessageBoxProxy>), out result))
				{
					result = new Mock<IMessageBoxProxy>();
					mocks[typeof(Mock<IMessageBoxProxy>)] = result;
				}
				return (Mock<IMessageBoxProxy>)result;
			}
		}

		public MockRepository Mocker { get; set; }

		public INativeMethods NativeMethods
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<INativeMethods>), out result))
				{
					result = new Mock<INativeMethods>();
					mocks[typeof(Mock<INativeMethods>)] = result;
				}
				return ((Mock<INativeMethods>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<INativeMethods>)] = value is Mock ? (Mock<INativeMethods>)value : Mock.Get(value);
			}
		}

		public Mock<INativeMethods> NativeMethodsForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<INativeMethods>), out result))
				{
					result = new Mock<INativeMethods>();
					mocks[typeof(Mock<INativeMethods>)] = result;
				}
				return (Mock<INativeMethods>)result;
			}
		}

		public IRegistryProxy Registry
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IRegistryProxy>), out result))
				{
					result = new Mock<IRegistryProxy>();
					mocks[typeof(Mock<IRegistryProxy>)] = result;
				}
				return ((Mock<IRegistryProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IRegistryProxy>)] = value is Mock ? (Mock<IRegistryProxy>)value : Mock.Get(value);
			}
		}

		public Mock<IRegistryProxy> RegistryForTest
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IRegistryProxy>), out result))
				{
					result = new Mock<IRegistryProxy>();
					mocks[typeof(Mock<IRegistryProxy>)] = result;
				}
				return (Mock<IRegistryProxy>)result;
			}
		}

		public virtual IFileVersionInfoProxy GetFileVersionInfo(string fileName)
		{
			if (ExpectedFileVersionInfoFileName != null)
			{
				Assertion.AssertEquals("FileVersionInfo File Name", ExpectedFileVersionInfoFileName, fileName);
			}
			return FileVersionInfo;
		}

		public IServiceControllerProxy ServiceController
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<IServiceControllerProxy>), out result))
				{
					result = new Mock<IServiceControllerProxy>();
					mocks[typeof(Mock<IServiceControllerProxy>)] = result;
				}
				return ((Mock<IServiceControllerProxy>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<IServiceControllerProxy>)] = value is Mock ? (Mock<IServiceControllerProxy>)value : Mock.Get(value);
			}
		}

		public ICargoWiseOneInstanceClass CargoWiseOneInstanceClass
		{
			get
			{
				object result;
				if (!mocks.TryGetValue(typeof(Mock<ICargoWiseOneInstanceClass>), out result))
				{
					result = new Mock<ICargoWiseOneInstanceClass>();
					mocks[typeof(Mock<ICargoWiseOneInstanceClass>)] = result;
				}
				return ((Mock<ICargoWiseOneInstanceClass>)result).Object;
			}
			set
			{
				mocks[typeof(Mock<ICargoWiseOneInstanceClass>)] = value is Mock ? (Mock<ICargoWiseOneInstanceClass>)value : Mock.Get(value);
			}
		}

		public void PopulateWithRealServices()
		{
			Directory = ServiceContainer.Instance.Directory;
			Environment = ServiceContainer.Instance.Environment;
			File = ServiceContainer.Instance.File;
			MessageBox = ServiceContainer.Instance.MessageBox;
			NativeMethods = ServiceContainer.Instance.NativeMethods;
			CargoWiseOneInstanceClass = ServiceContainer.Instance.CargoWiseOneInstanceClass;
		}
	}
}
