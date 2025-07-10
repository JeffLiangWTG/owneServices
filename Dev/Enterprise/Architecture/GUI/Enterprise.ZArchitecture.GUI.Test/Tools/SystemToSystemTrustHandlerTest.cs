using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Interop;
using Enterprise.Integration.SystemToSystemTrust;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Tools
{
	class SystemToSystemTrustHandlerTest : RemoteDesktopServicesTest
	{
		public void TestRemoteSend()
		{
			var terminalService = new Mock<TerminalService>();
			terminalService.Setup(x => x.IsWTSSession).Returns(true);
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);
			using (ObjectFactory.Substitute(terminalService.Object))
			{
				HttpListener listenerOne;
				string prefix;
				var port = 29000;
				do
				{
					prefix = "http://localhost:" + port;
					listenerOne = new HttpListener();
					listenerOne.Prefixes.Add(prefix + "/PostLogin/");
					listenerOne.Prefixes.Add(prefix + "/Index/");

					try
					{
						listenerOne.Start();
					}
					catch (HttpListenerException) when (port <= 29050)
					{
						port++;
					}
				}
				while (!listenerOne.IsListening);

				var trustMessage = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri(listenerOne.Prefixes.First()));
				// Override Access Token for test. See SystemToSystemMessageTest for how to use SystemToSystemTrustMessage correctly
				trustMessage.AccessToken = "Access Token";
				var handler = ObjectFactory.Get<ISystemToSystemTrustHandler>();
				handler.SendMessage(trustMessage.AccessToken, trustMessage.PostUrl);

				var context = listenerOne.GetContext();
				var authorization = context.Request.Headers["Authorization"];
				AssertEquals("Should contains the bearer token in the header.", "Bearer Access Token", authorization);
				context.Response.Redirect(listenerOne.Prefixes.Last());
				context.Response.Close();

				var newContext = listenerOne.GetContext();
				AssertNull(newContext.Request.Headers["Authorization"]);
			}
		}

		[TestRequiresAdministrativePrivileges("Need to run in CW1 app otherwise AddUrlAcl.EnsureCallbackUrlConfigured will fail due to unloaded dll.")]
		public void TestLocalSend()
		{
			InitializationMessageHandler.RemoteInitializationMessage = null;
			var trustMessage = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri("https://127.0.0.1"));
			// Override these values for test. See SystemToSystemMessageTest for how to use SystemToSystemTrustMessage correctly
			trustMessage.AccessToken = "Access Token";
			trustMessage.PostUrl = "Post Url";

			Task task1 = null, task2 = null;
			var response = "";
			var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
			var systemToSystemTrustMessageLauncher = new SystemToSystemTrustMessageLauncher(endpoint =>
			{
				task1 = Task.Run(() =>
				{
					using (var httpClient = new HttpClient())
					{
						response = httpClient.GetStringAsync(endpoint).Result;
						waitHandle.Set();
					}
				});
			});

			SystemToSystemTrustHandler.SendMessageLocal(trustMessage, systemToSystemTrustMessageLauncher);
			waitHandle.WaitOne(TimeSpan.FromSeconds(5));
			var expectedResult = @"
<!doctype html>
<html>
	<body onload=""document.getElementById('myForm').submit()"">
		<form id=""myForm"" action=""Post Url"" method=""post"">
			<input type=""hidden"" name=""access_token"" value='Access Token'>
		</form>
	</body>
</html>
";
			AssertEquals(expectedResult, response);

			systemToSystemTrustMessageLauncher = new SystemToSystemTrustMessageLauncher(endpoint =>
			{
				task2 = Task.Run(() =>
				{
					using (var httpClient = new HttpClient())
					{
						response = httpClient.GetStringAsync(endpoint + "badendpoint").Result;
						waitHandle.Set();
					}
				});
			});

			SystemToSystemTrustHandler.SendMessageLocal(trustMessage, systemToSystemTrustMessageLauncher);
			waitHandle.WaitOne(TimeSpan.FromSeconds(5));

			AssertEquals("The session is invalid", response);
			Task.WaitAll(task1, task2);
		}

		[TestRequiresAdministrativePrivileges("Need to run in CW1 app otherwise AddUrlAcl.EnsureCallbackUrlConfigured will fail due to unloaded dll.")]
		public void TestLocalSend_CallbackUrlIsChecked()
		{
			using (var httpApi = new HttpApi())
			{
				try
				{
					TestLocalSend();

					AssertNotNull(httpApi.GetHttpServiceConfigUrlAclInfo(SystemToSystemTrustMessageSender.SystemToSystemTrustMessageListenerUrl));
				}
				finally
				{
					httpApi.DeleteHttpServiceConfigUrlAclInfo(SystemToSystemTrustMessageSender.SystemToSystemTrustMessageListenerUrl);
				}
			}
		}

		public void TestRemoteSendRdpPluginNotSupported()
		{
			var terminalService = new Mock<TerminalService>();
			terminalService.Setup(x => x.IsWTSSession).Returns(true);
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);
			InitializationMessageHandler.RemoteInitializationMessage = null;
			using (ObjectFactory.Substitute(terminalService.Object))
			{
				var trustMessage = SystemToSystemTrustMessageFactory.Create(RSA.Create(2048), Guid.NewGuid(), Guid.NewGuid(), new Uri("https://127.0.0.1"));
				var handler = ObjectFactory.Get<ISystemToSystemTrustHandler>();
				handler.SendMessage(trustMessage.AccessToken, trustMessage.PostUrl);

				AssertEquals(ZTerminalService.RDApplicationNotInstalledError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
