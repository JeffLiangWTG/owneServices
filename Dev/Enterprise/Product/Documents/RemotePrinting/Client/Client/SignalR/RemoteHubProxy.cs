using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Types;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Enterprise.RemotePrinting.Client
{
	public class RemoteHubProxy : IRemoteServer, IDisposable
	{
		readonly List<IDisposable> disposables = new List<IDisposable>();

		internal IHubConnection Connection { get; }

		readonly IHubProxy hub;
		readonly WebClientConfiguration config;

		public event Action<string> LogInfo;
		public event Action<string> LogError;

		public RemoteHubProxy(HubConnection connection, WebClientConfiguration config)
			: this(connection, connection.CreateHubProxy("RemoteHub"), config)
		{
		}

		public RemoteHubProxy(IHubConnection connection, IHubProxy hub, WebClientConfiguration config)
		{
			Connection = connection;

			this.hub = hub;
			this.config = config;
		}

		public void RegisterClient(IRemoteClient client)
		{
			disposables.AddRange(new[]
			{
				hub.On(nameof(IRemoteClient.Print), new Action<SerialisablePrintJob>(client.Print)),
				hub.On(nameof(IRemoteClient.SetWatermark), new Action<SerialisableWatermark>(client.SetWatermark)),
				hub.On(nameof(IRemoteClient.Nudge), new Action(client.Nudge)),
				hub.On(nameof(IRemoteClient.RetrieveLogsAndPostToServer), new Action<string, DateTime, DateTime, LogTypes>(client.RetrieveLogsAndPostToServer)),
				hub.On(nameof(IRemoteClient.RegisterClientForReconnecting), new Action(client.RegisterClientForReconnecting))
			});
		}

		string registeredName;
		string[] registeredPrinters;

		public void Initialise(string name, string version, string[] printers)
		{
			var task = HubProxyInvoke(new object[] { name, version, printers },
				() =>
				{
					registeredName = name;
					registeredPrinters = printers;
				});

			if (config.RemotePrintingServiceTimeoutInSeconds > 0)
			{
				var timeout = TimeSpan.FromSeconds(config.RemotePrintingServiceTimeoutInSeconds);
				task.Wait(timeout);
			}
		}

		public void SetPrintStatus(Guid jobPk, ProcessedStatus status, string failureInfo)
		{
			var task = HubProxyInvoke(new object[] { jobPk, status, failureInfo });

			if (config.RemotePrintingServiceTimeoutInSeconds > 0)
			{
				var timeout = TimeSpan.FromSeconds(config.RemotePrintingServiceTimeoutInSeconds);
				task.Wait(timeout);
			}
		}

		public void UpdatePrinters(string name, string[] printers)
		{
			if (registeredName == null || !registeredName.Equals(name, StringComparison.InvariantCulture) || !HasSamePrinters(registeredPrinters, printers))
			{
				var task = HubProxyInvoke(new object[] { name, printers },
					() =>
					{
						registeredName = name;
						registeredPrinters = printers;
					});

				if (config.RemotePrintingServiceTimeoutInSeconds > 0)
				{
					var timeout = TimeSpan.FromSeconds(config.RemotePrintingServiceTimeoutInSeconds);
					task.Wait(timeout);
				}
			}
		}

		static bool HasSamePrinters(string[] printers1, string[] printers2)
		{
			if (printers1 == null && printers2 == null)
			{
				return true;
			}

			if (printers1 == null || printers2 == null)
			{
				return false;
			}

			if (printers1.Length != printers2.Length)
			{
				return false;
			}

			return printers1.All(p => printers2.Contains(p, StringComparer.InvariantCulture));
		}

		public bool RequestPrint(string server, SerialisablePrintJob job)
			=> throw new NotImplementedException(); // The client can't request print

		public void RequestRefreshCNSWClientSetting(string name)
			=> HubProxyInvoke(new object[] { name });

		Task HubProxyInvoke(object[] args, Action completedAction = null, [CallerMemberName] string methodName = null)
		{
			var task = hub.Invoke(methodName, args)
				.ContinueWith(
					t =>
					{
						switch (t.Status)
						{
							case TaskStatus.Faulted:
								HandleHubProxyException(methodName, t.Exception);
								break;
							case TaskStatus.RanToCompletion:
								completedAction?.Invoke();
								break;
						}
					});

#if DEBUG
			if (ClientRunningTestingState.IsRunningTests)
			{
				LastInvokedTaskForTest = task;
			}
#endif

			return task;
		}

#if DEBUG
		public Task LastInvokedTaskForTest { get; private set; }
#endif

		void OnShowError(string message)
		{
			LogError?.Invoke(message);
		}

		void OnShowInformation(string message)
		{
			LogInfo?.Invoke(message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "errorMessageBuilder append")]
		void HandleHubProxyException(string methodName, Exception ex)
		{
			var handled = false;

			var errorMessageBuilder = new StringBuilder();
			errorMessageBuilder.Append("Error in SignalR Hub Proxy on ").Append(methodName).Append(".");
			var initialLen = errorMessageBuilder.Length;

			var currentEx = ex;
			while (currentEx != null)
			{
				if (currentEx is InvalidOperationException)
				{
					handled = true;
					break;
				}

				if (HandleHttpClientException(methodName, currentEx as HttpClientException, errorMessageBuilder))
				{
					handled = true;
					break;
				}

				if (currentEx is HttpRequestException requestException && ErrorReporter.HandleHttpRequestException(requestException, errorMessageBuilder))
				{
					handled = true;
					break;
				}

				currentEx = currentEx.InnerException;
			}

			if (errorMessageBuilder.Length == initialLen)
			{
				errorMessageBuilder.Append(" Error message: ").Append(ErrorReporter.GetExceptionMessage(ex));
			}

			var errorMessage = errorMessageBuilder.ToString();
			OnShowError(errorMessage);

			if (!handled)
			{
				throw new SignalRHubProxyException(errorMessage, ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message append")]
		bool HandleHttpClientException(string methodName, HttpClientException clientException, StringBuilder errorMessageBuilder)
		{
			if (clientException == null)
			{
				return false;
			}

			var handled = false;

			if (IsBadRequestError(clientException))
			{
				handled = true;

				errorMessageBuilder.Append(" Error message: Bad Request (Status code 400).");
			}

			if (IsGatewayTimeoutError(clientException))
			{
				handled = true;

				errorMessageBuilder.Append(" Error message: WebPrint suspended due to a slow connection or connection break to gateway. (Status code 504)");
			}

			return handled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "gateway timeout message")]
		static bool IsGatewayTimeoutError(HttpClientException clientException)
		{
			if (clientException == null)
			{
				return false;
			}
			return clientException.Response?.StatusCode == HttpStatusCode.GatewayTimeout || clientException.Message.StartsWith("StatusCode: 504, ReasonPhrase: 'Gateway Time-out'", StringComparison.Ordinal);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Bad Request")]
		static bool IsBadRequestError(HttpClientException clientException)
		{
			if (clientException == null)
			{
				return false;
			}

			if (clientException.Response != null)
			{
				return clientException.Response.StatusCode == HttpStatusCode.BadRequest;
			}
			if (clientException.Message.StartsWith("StatusCode: 400, ReasonPhrase: 'Bad Request'", StringComparison.Ordinal))
			{
				return true;
			}

			return false;
		}

		public void Dispose()
		{
			disposables.ForEach(d => d.Dispose());

			switch (Connection.State)
			{
				case ConnectionState.Connecting:
				case ConnectionState.Reconnecting:
					Connection.Disconnect();
					return;

				case ConnectionState.Connected:
					Connection.Stop();
					return;
			}
		}
	}
}

/*
#region Testing
#if DEBUG

The SignalR .Net 4 libs don't play nicely with the ones located in bin, so these tests are not runnable.
I'll uncomment when I upgrade the solution to .Net 472

namespace Enterprise.RemotePrinting.Client.Testing
{
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;
	using Rhino.Mocks;

	public class RemoteHubProxyTest : TestCase
	{
		public void TestListeningToClientCommands()
		{
			var client = MockExpectingAllMethodsCalledOnce<IRemoteClient>();
			var hub = MockRepository.GenerateMock<IHubProxy>();

			Action<SerialisablePrintJob> print = null;
			hub.Expect(c => c.On("Print", Arg<Action<SerialisablePrintJob>>.Is.Anything))
				.WhenCalled(m => print = (Action<SerialisablePrintJob>)m.Arguments[1]);

			Action<SerialisableWatermark> setWatermark = null;
			hub.Expect(c => c.On("SetWatermark", Arg<Action<SerialisableWatermark>>.Is.Anything))
				.WhenCalled(m => setWatermark = (Action<SerialisableWatermark>)m.Arguments[1]);

			var proxy = new RemoteHubProxy(MockRepository.GenerateMock<IHubConnection>(), hub);
			proxy.RegisterClient(client);

			CombineAssertions(() =>
			{
				AssertNotNull("Expected the OnPrint to be registered", print);
				AssertNotNull("Expected the OnSetWatermark to be registered", setWatermark);
			});

			print(null);
			setWatermark(null);

			AssertNoExceptionThrown("Please test EVERY method is being hooked correctly. Each failed expectation is an untested method.", client.VerifyAllExpectations);
		}

		public void TestInvokesHubRequests()
		{
			var calls = new Dictionary<string, object[]>();
			var mock = MockRepository.GenerateMock<IHubProxy>();
			mock.Stub(m => m.Invoke(null, null))
				.IgnoreArguments()
				.WhenCalled(m => calls.Add((string)m.Arguments[0], (object[])m.Arguments[1]));

			var proxy = new RemoteHubProxy(MockRepository.GenerateMock<IHubConnection>(), mock);
			proxy.RegisterClient(MockRepository.GenerateMock<IRemoteClient>());

			proxy.Initialise("bob", "6.9", new[] { "YourPrinter", "MyPrinter" });
			AssertEquals(calls["Initialise"], new object[] { "bob", "6.9", new[] { "YourPrinter", "MyPrinter" } });

			var jobPk = new Guid("e95bc88d-17d6-47a4-b4ba-b9a53db7eea0");
			proxy.SetPrintStatus(jobPk, ProcessedStatus.Processed, "Good stuff");
			AssertEquals(calls["SetPrintStatus"], new object[] { jobPk, ProcessedStatus.Processed, "Good stuff" });

			proxy.UpdatePrinters("Bill", new[] { "YourPrinter", "MyPrinter", "OurPrinter" });
			AssertEquals(calls["SetPrintStatus"], new object[] { "Bill", new[] { "YourPrinter", "MyPrinter", "OurPrinter" } });

			AssertExceptionThrown<NotImplementedException>(() => proxy.RequestPrint("Whatever", null));

			var missing = typeof(IRemoteServer).GetMethods()
				.Select(m => m.Name)
				.Except(new[] { "RequestPrint" })
				.Except(calls.Keys.Cast<string>());

			AssertArrayEqualsByElements("Please add a test for the missing methods to ensure the arguments are passed through correctly.", new string[0], missing.ToArray());
		}

		T MockExpectingAllMethodsCalledOnce<T>() where T : class
		{
			var mock = MockRepository.GenerateStrictMock<T>();
			foreach (var method in typeof(T).GetMethods())
			{
				var args = method.GetParameters().Select(p => p.ParameterType).Select(Default).ToArray();
				mock.Expect(m => method.Invoke(m, args)).IgnoreArguments().Repeat.Once();
			}

			return mock;
		}

		static object Default(Type t)
			=> t.IsValueType ? Activator.CreateInstance(t) : null;
	}
}

#endif
#endregion

*/
