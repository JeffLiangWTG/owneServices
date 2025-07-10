using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AppDomainWrappers.Net
{
	public class AppDomainWrapper : IAppDomainWrapper
	{
		const string NET8_WRAPPER_EXECUTABLE = "AppDomainWrappers.Net8.exe";
		const string NET48_WRAPPER_EXECUTABLE = "AppDomainWrappers.Net48.exe";

		AppDomain appDomain;
		bool isDisposed;

		public AppDomainWrapper() { }

		public AppDomainWrapper(string domainName, bool isTestAppDomain = false)
		{
			appDomain = isTestAppDomain ? CreateTestAppDomain(domainName) : AppDomain.CreateDomain(domainName);
		}

		public Assembly[] GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();

		public object CreateInstanceAndUnwrap(string assemblyFile, string typeName, params object[] args)
		{
			if (assemblyFile == null)
			{
				throw new ArgumentNullException(nameof(assemblyFile));
			}

			if (typeName == null)
			{
				throw new ArgumentNullException(nameof(typeName));
			}

			var assembly = Assembly.Load(assemblyFile) ?? throw new Exception("Could not load assembly");
			var type = assembly.GetType(typeName) ?? throw new Exception("Could not find type");
			return args.Length > 0 ? Activator.CreateInstance(type) : Activator.CreateInstance(type, args);
		}

		public AppDomain CreateTestAppDomain(string name)
		{
			var setupInfo = new AppDomainSetup
			{
				//required to get the tests running in Visual Studio Test Explorer
				ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			};

			var appDomain = AppDomain.CreateDomain(name, null, setupInfo);

			//resolve any unknown assemblies by looking in the main cargowiseone folder for the assembly.
			appDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
			{
				var assemblyName = new AssemblyName(args.Name);

				var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, assemblyName.Name + ".dll");

				if (File.Exists(path))
				{
					return Assembly.LoadFile(path);
				}

				return null;
			};

			return appDomain;
		}

		public void RunActionInTask(Action action, CancellationToken cancellationToken)
		{
			try
			{
				var task = Task.Run(() =>
				{
					cancellationToken.ThrowIfCancellationRequested();
					action();
				}, cancellationToken);

				task.Wait();
			}
			catch (AggregateException ae)
			{
				foreach (var innerException in ae.InnerExceptions)
				{
					throw innerException;
				}
			}
		}

		public string RunCodeInProcess(string codeToRun, List<string> dllFiles = null, List<string> imports = null) => RunInProcessHelpers.RunCodeInProcessNet(codeToRun, NET8_WRAPPER_EXECUTABLE, dllFiles, imports);

		public string RunCodeInProcess48(string codeToRun, List<string> dllFiles = null, List<string> imports = null) => RunInProcessHelpers.RunCodeInProcessNet(codeToRun, NET48_WRAPPER_EXECUTABLE, dllFiles, imports);

		public string RunMethodInProcess(ProcessConfig config) => RunInProcessHelpers.RunMethodInProcess(config, NET8_WRAPPER_EXECUTABLE);

		public string RunMethodInProcess48(ProcessConfig config) => RunInProcessHelpers.RunMethodInProcess(config, NET48_WRAPPER_EXECUTABLE);

		public Dictionary<string, object> RunActionInAppDomain(Action action, Dictionary<string, object> appDomainData = null, Dictionary<string, object> appDomainReturnData = null)
		{
			EnsureAppDomainIsCreated();

			if (appDomainData != null)
			{
				foreach (var property in appDomainData)
				{
					appDomain.SetData(property.Key, property.Value);
				}
			}

			var actionExecutor = (ActionExecutor)appDomain.CreateInstanceAndUnwrap(
				typeof(ActionExecutor).Assembly.FullName,
				typeof(ActionExecutor).FullName
				);

			actionExecutor.Execute(action);

			var returnData = new Dictionary<string, object>();
			if (appDomainReturnData != null)
			{
				foreach (var property in appDomainReturnData)
				{
					returnData.Add(property.Key, appDomain.GetData(property.Key));
				}
			}

			return returnData;
		}

		public void Dispose()
		{
			EnsureIsNotDisposed();

			isDisposed = true;

			if (appDomain != null)
			{
				AppDomain.Unload(appDomain);
				appDomain = null;
			}
		}

		void EnsureAppDomainIsCreated()
		{
			EnsureIsNotDisposed();

			appDomain ??= AppDomain.CreateDomain(Guid.NewGuid().ToString());
		}

		void EnsureIsNotDisposed()
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("AppDomainWrapper", "Do not reuse the AppDomainWrapper once Dispose() has been called.");
			}
		}
	}
}
