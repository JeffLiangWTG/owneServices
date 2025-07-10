using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Web.Shared
{
	// Enterprise.ZArchitecture.Web.Shared.Tester.exe is used only in one test
	// /Enterprise/Services/ServiceHost.Tests/GlobalTest.cs
	// Multi-target it when needed in the future
	internal class Program
	{
		static void Main(string[] args)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			try
			{
				ValidateArgs(args);

				Console.WriteLine(string.Join(" ", args));

				var applicationHost =
					(TestApplicationHost)ApplicationHost.CreateApplicationHost(typeof(TestApplicationHost), "/", ApplicationPath);
				var appDomain = applicationHost.GetAppDomain();

				appDomain.SetData(".serverName", ServerName);
				appDomain.SetData(".databaseName", DatabaseName);
				appDomain.SetData(".assemblyPath", AssemblyPath);
				appDomain.SetData(".typeName", TypeName);
				appDomain.SetData(".methodName", MethodName);
				appDomain.SetData(".applicationPath", ApplicationPath);

				appDomain.SetData(".appDomain", appDomain.FriendlyName);
				appDomain.SetData(".domainId", $"{Guid.NewGuid()}");
				appDomain.SetData(".appVPath", "/");
				appDomain.SetData(".hostingVirtualPath", HttpRuntime.AppDomainAppVirtualPath);
				appDomain.SetData(".hostingInstallDir", HttpRuntime.AspInstallDirectory);

				appDomain.DoCallBack(() =>
				{
					var currentDomain = AppDomain.CurrentDomain;

					AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
					{
						if (!(e.ExceptionObject is Exception unhandledException))
						{
							unhandledException = new Exception($"{currentDomain} thrown an unhandledException from appDomain.DoCallBack().");
						}

						AppDomain.CurrentDomain.SetData("exception", unhandledException);
					};

					try
					{
						var appPhysicalPath = (string)currentDomain.GetData(".applicationPath");

						ConfigurationManager.AppSettings[".serverName"] = (string)currentDomain.GetData(".serverName");
						ConfigurationManager.AppSettings[".databaseName"] = (string)currentDomain.GetData(".databaseName");

						var assemblyPath = (string)currentDomain.GetData(".assemblyPath");
						var typeName = (string)currentDomain.GetData(".typeName");
						var methodName = (string)currentDomain.GetData(".methodName");

						var theRuntime = (HttpRuntime)typeof(HttpRuntime)
							.GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static)
							?.GetValue(null);
						typeof(HttpRuntime)
							?.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance)
							?.Invoke(theRuntime, null);

						HostingEnvironment hostingEnvironment;
						if (HostingEnvironment.IsHosted)
						{
							hostingEnvironment = (HostingEnvironment)typeof(HostingEnvironment)
								.GetField("_theHostingEnvironment", BindingFlags.NonPublic | BindingFlags.Static)
								?.GetValue(null);
						}
						else
						{
							hostingEnvironment = new HostingEnvironment();
							var waitCallback = new WaitCallback(state => { });
							typeof(HostingEnvironment)
								?.GetField("_initiateShutdownWorkItemCallback", BindingFlags.NonPublic | BindingFlags.Instance)
								?.SetValue(hostingEnvironment, waitCallback);
						}

						typeof(HostingEnvironment)
							.GetField("_appPhysicalPath", BindingFlags.NonPublic | BindingFlags.Instance)
							?.SetValue(hostingEnvironment, appPhysicalPath);

						var applicationManager = (ApplicationManager)typeof(ApplicationManager)
							.GetField("_theAppManager", BindingFlags.NonPublic | BindingFlags.Static)?
							.GetValue(null)
							?? ApplicationManager.GetApplicationManager();

						var appId = (string)typeof(HostingEnvironment)
								.GetField("_appId", BindingFlags.NonPublic | BindingFlags.Instance)
								?.GetValue(hostingEnvironment);
						currentDomain.SetData("appId", appId);

						Invoke(assemblyPath, typeName, methodName);
					}
					catch (Exception ex)
					{
						AppDomain.CurrentDomain.SetData("exception", ex);
					}
				});

				var exception = (Exception)appDomain.GetData("exception");

				// we are having problem to unload the appDomain created from ApplicationHost.CreateApplicationHost
				// just skip to end the process
				// AppDomain.Unload(appDomain);

				if (exception != null)
				{
					throw exception;
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"{ex}");
			}
			finally
			{
				Environment.Exit(0);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		static void ValidateArgs(string[] args)
		{
			if (args?.Length != 6)
			{
				throw new NotSupportedException($@"{nameof(args)} is required to have: serverName, databaseName, assemblyPath, typeName, methodName, and applicationPath provided so that the tester can invoke callback.");
			}

			ServerName = args[0];
			DatabaseName = args[1];
			AssemblyPath = args[2];
			TypeName = args[3];
			MethodName = args[4];
			ApplicationPath = Path.GetFullPath(args[5]);

			ValidateAssemblyFileExists();
			ValidateApplicationPathExists();

			void ValidateAssemblyFileExists()
			{
				if (string.IsNullOrEmpty(AssemblyPath) || !File.Exists(AssemblyPath))
				{
					throw new ArgumentException($"Assembly file not found: '{AssemblyPath}'.");
				}
			}

			void ValidateApplicationPathExists()
			{
				if (string.IsNullOrEmpty(ApplicationPath) || !Directory.Exists(ApplicationPath))
				{
					throw new ArgumentException($"Application path not found: '{ApplicationPath}'");
				}
			}
		}

		static void Invoke(string assemblyPath, string typeName, string methodName)
		{
			var assembly = Assembly.LoadFrom(assemblyPath);
			var type = assembly.GetType(typeName)
				?? throw new ArgumentException($"Type: '{typeName}' not found in assembly: '{assemblyPath}'.");

			var methodInfo = type.GetMethod(methodName)
				?? throw new ArgumentException($"Method: '{methodName}' not found in type: '{typeName}', assembly: '{assemblyPath}'.");

			var instance = Activator.CreateInstance(type, true)
				?? throw new ArgumentNullException(nameof(typeName), $"Type: '{type}' cannot be instantiated from assembly: '{assemblyPath}'.");

			var parameters = methodInfo.GetParameters();
			if (parameters.Length == 0)
			{
				methodInfo.Invoke(instance, null);
			}
			else
			{
				methodInfo.Invoke(instance, null);
			}
		}

		[ThreadSafe]
		static string ServerName;
		[ThreadSafe]
		static string DatabaseName;
		[ThreadSafe]
		static string AssemblyPath;
		[ThreadSafe]
		static string TypeName;
		[ThreadSafe]
		static string MethodName;
		[ThreadSafe]
		static string ApplicationPath;
	}
}
