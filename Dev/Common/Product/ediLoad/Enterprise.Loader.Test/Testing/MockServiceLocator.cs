using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Native;
using Moq;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	public class MockServiceContainer : IServiceContainer
	{
		readonly Dictionary<Type, object> mocks = new Dictionary<Type, object>();

		public MockServiceContainer()
		{
		}

		public MockServiceContainer(MockRepository mocker)
		{
			Mocker = mocker;
		}

		public IDirectoryProxy Directory
		{
			get { return GetMock<IDirectoryProxy>(); }
			set { SetMock<IDirectoryProxy>(value); }
		}

		public IEnvironmentProxy Environment
		{
			get { return GetMock<IEnvironmentProxy>(); }
			set { SetMock<IEnvironmentProxy>(value); }
		}

		public IEventLogProxy EventLog
		{
			get { return GetMock<IEventLogProxy>(); }
			set { SetMock<IEventLogProxy>(value); }
		}

		public string ExpectedFileVersionInfoFileName { get; set; }

		public string ExpectedInstallLogDirectory { get; set; }

		public IFileProxy File
		{
			get { return GetMock<IFileProxy>(); }
			set { SetMock<IFileProxy>(value); }
		}

		public IFileVersionInfoProxy FileVersionInfo
		{
			get { return GetMock<IFileVersionInfoProxy>(); }
			set { SetMock<IFileVersionInfoProxy>(value); }
		}

		public IMessageBoxProxy MessageBox
		{
			get { return GetMock<IMessageBoxProxy>(); }
			set { SetMock<IMessageBoxProxy>(value); }
		}

		public MockRepository Mocker { get; set; }

		public INativeMethods NativeMethods
		{
			get { return GetMock<INativeMethods>(); }
			set { SetMock<INativeMethods>(value); }
		}

		public IRegistryProxy Registry
		{
			get { return GetMock<IRegistryProxy>(); }
			set { SetMock<IRegistryProxy>(value); }
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
			get { return GetMock<IServiceControllerProxy>(); }
			set { SetMock<IServiceControllerProxy>(value); }
		}

		public ICargoWiseOneInstanceClass CargoWiseOneInstanceClass
		{
			get { return GetMock<ICargoWiseOneInstanceClass>(); }
			set { SetMock<ICargoWiseOneInstanceClass>(value); }
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

		public Func<Form, DialogResult> ShowDialogWithResult(Action<Form> action)
		{
			return new Func<Form, DialogResult>((Form form) =>
			{
				form.Load += (object sender, EventArgs e) =>
				{
					form.BeginInvoke(new Action(() => action(form)));
				};
				return form.ShowDialog();
			});
		}

		T GetMock<T>() where T : class
		{
			object result;
			if (!mocks.TryGetValue(typeof(T), out result))
			{
				result = Mocker.Create<T>().Object;
				mocks[typeof(T)] = result;
			}
			return (T)result;
		}

		void SetMock<T>(object value)
		{
			mocks[typeof(T)] = value;
		}
	}
}
