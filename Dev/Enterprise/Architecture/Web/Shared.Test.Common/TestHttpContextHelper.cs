#if NETFRAMEWORK
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.SessionState;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Moq;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	public static class TestHttpContextHelper
	{
		public static IDisposable DisposableAppDomain(Action<AppDomain> appDomainSetData, Action appDomainCallBack, string physicalPath = null, Action<HostingEnvironment> hostingEnvironmentAction = null)
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var appDomain = AppDomain.CreateDomain(Invariant($@"/LM/W3SVC/3/ROOT-1-123456789012345678"));
			var appPath = physicalPath ?? Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;

			appDomain.SetData("appDomainCallBack", appDomainCallBack);
			appDomain.SetData("ServerName", Db.ServerName);
			appDomain.SetData("DatabaseName", Db.DatabaseName);
			appDomain.SetData(".appDomain", appDomain.FriendlyName);
			appDomain.SetData(".domainId", $"{Guid.NewGuid()}");
			appDomain.SetData(".appPath", appPath);
			appDomain.SetData(".appVPath", "/");
			appDomain.SetData(".hostingVirtualPath", HttpRuntime.AppDomainAppVirtualPath);
			appDomain.SetData(".hostingInstallDir", HttpRuntime.AspInstallDirectory);
			appDomain.SetData("hostingEnvironmentAction", hostingEnvironmentAction);
			appDomainSetData?.Invoke(appDomain);

			appDomain.DoCallBack(() =>
			{
				var currentDomain = AppDomain.CurrentDomain;
				var appDomainDoCallBack = (Action)currentDomain.GetData("appDomainCallBack");
				var serverName = (string)currentDomain.GetData("ServerName");
				var databaseName = (string)currentDomain.GetData("DatabaseName");
				var appPhysicalPath = (string)currentDomain.GetData(".appPath");
				var setHostingEnvironmentAction = (Action<HostingEnvironment>)currentDomain.GetData("hostingEnvironmentAction");

				WebDbConfiguration.SaveConfiguration(
					new WebDbConfigurationInfo()
					{
						ApplicationPath = WebAppPath.ForCurrentAppDomain(),
						ServerName = serverName,
						DatabaseName = databaseName
					});

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

				setHostingEnvironmentAction?.Invoke(hostingEnvironment);

				appDomainDoCallBack?.Invoke();
			});

			return new DisposableAction(() =>
			{
				AppDomain.Unload(appDomain);
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public static IDisposable DisposableHttpContext(
			out HttpRequest httpRequest,
			out HttpResponse httpResponse,
			HttpApplication httpApplication = null,
			string requestPath = "",
			string queryString = "")
		{
			httpRequest = null;
			httpResponse = null;
			if (httpApplication == null)
			{
				httpApplication = new HttpApplication();
			}

			httpRequest = new HttpRequest(requestPath, "http://localhost", queryString);
			httpResponse = new HttpResponse(new StringWriter());
			HttpContext.Current = new HttpContext(httpRequest, httpResponse)
			{
				ApplicationInstance = httpApplication,
			};

			typeof(HttpApplication).InvokeMember(
				"InitInternal",
				BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				httpApplication,
				new object[] { HttpContext.Current, httpApplication.Application, Array.Empty<MethodInfo>() });
			typeof(HttpApplication).InvokeMember(
				"_context",
				BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				httpApplication,
				new object[] { HttpContext.Current });

			return new DisposableAction(() =>
			{
				HttpContext.Current = null;
			});
		}

		public static IDisposable DisposableSession(out Mock<IHttpSessionState> sessionStateMock)
		{
			IDisposable disposableContext = null;
			if (HttpContext.Current == null)
			{
				disposableContext = DisposableHttpContext(out _, out _);
			}

			sessionStateMock = new Mock<IHttpSessionState>();
			var sessionContainer = new SessionStateItemCollection();
			sessionStateMock.Setup(x => x.SessionID).Returns($"{Guid.NewGuid()}");
			sessionStateMock.Setup(x => x.IsNewSession).Returns(true);
			sessionStateMock.Setup(x => x.Timeout).Returns(30);
			sessionStateMock.Setup(x => x.SyncRoot).Returns(new object());
			sessionStateMock.Setup(x => x.Keys).Returns(sessionContainer.Keys);
			sessionStateMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<object>()))
				.Callback((string key, object value) =>
				{
					sessionContainer[key] = value;
				});
			sessionStateMock.Setup(x => x[It.IsAny<string>()])
				.Returns((string key) => sessionContainer[key]);
			sessionStateMock.SetupSet(sb => sb[It.IsAny<string>()] = It.IsAny<object>())
				.Callback((string key, object value) => sessionContainer[key] = value);
			sessionStateMock.Setup(x => x.Remove(It.IsAny<string>()))
				.Callback((string key) => sessionContainer.Remove(key));
			sessionStateMock.Setup(x => x.Abandon()).Verifiable();

			var session = (HttpSessionState)typeof(HttpSessionState)
				.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
				.Invoke(new object[] { sessionStateMock.Object });

			if (HttpContext.Current != null)
			{
				HttpContext.Current.Items["AspSession"] = session;
			}

			return new DisposableAction(() =>
			{
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Items["AspSession"] = null;
				}

				disposableContext?.Dispose();
			});
		}
	}
}
#endif
