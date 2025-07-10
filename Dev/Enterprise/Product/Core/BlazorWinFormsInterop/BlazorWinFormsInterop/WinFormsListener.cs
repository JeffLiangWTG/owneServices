using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace Enterprise.BlazorWinFormsInterop
{
	public class WinFormsListener : IDisposable, IWinFormsListener
	{
		readonly WinFormsListenerState winFormsListenerState = new WinFormsListenerState();

		DateTime startTime;

		readonly TimeSpan backchannelConnectionTimeout = Env.Registry.BlazorBackchannelConnectionTimeout;

		public void OpenModule(string id, string queryString, out bool success)
		{
			if (winFormsListenerState.CurrentState != WinFormsListenerState.State.Initialised || !HybridModuleEnabledForUser(id))
			{
				success = false;
				return;
			}

			if (!WaitHybridStart())
			{
				success = false;
				return;
			}

			var hubContext = GlobalHost.ConnectionManager.GetHubContext<BlazorWinFormsInteropHub>();
			hubContext.Clients.All.OpenUrlInWinzorMode(queryString);
			success = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		bool WaitHybridStart()
		{
			if (!BlazorWinFormsInteropHub.Connected)
			{
				using (var progressForm = new ProgressFormManager())
				{
					bool progressFormCancelled = false;
					progressForm.IsProgressBarVisible = false;
					progressForm.Cancelled += (sender, args) => { progressFormCancelled = true; };
					progressForm.IsCancelButtonVisible = true;
					progressForm.UpdateStatus(Res.GetString("10D6DE99-A800-4B92-8718-F76B3F672E57", @"Initiating hybrid mode, please wait as this may take a minute or two"), 0);
					progressForm.Start(); //this kicks off the progressForm background thread which will let us know about cancellation

					while (!progressFormCancelled && !BlazorWinFormsInteropHub.Connected && DateTime.UtcNow - startTime < backchannelConnectionTimeout)
					{
						Thread.Sleep(50);
					}

					if (!BlazorWinFormsInteropHub.Connected)
					{
						return false;
					}
				}
			}

			return true;
		}

		public bool ExitHybridMode()
		{
			if (winFormsListenerState.CurrentState != WinFormsListenerState.State.Initialised || !WaitHybridStart())
			{
				return false;
			}

			var hubContext = GlobalHost.ConnectionManager.GetHubContext<BlazorWinFormsInteropHub>();
			hubContext.Clients.All.ExitWinzorMode();
			return true;
		}

		public const string UrlAclUrlPrefix = HttpServiceConfig.UrlAclUrlPrefix;

		/// <summary>
		/// This method starts the listener WITHOUT launching the client application
		/// used for development, testing, and diagnostic purposes
		/// </summary>
		public void StartListener()
		{
			ExecuteActionAndDisableIfFails(() =>
			{
				lock (webappCreationLock)
				{
					if (webapp == null)
					{
						HttpServiceConfig.EnsureHttpServiceConfig();

						// We listen using +, the strong wildcard, so that this reservation takes highest priority.
						// This stops another process (e.g. some third-party application that also wants port 7070) from registering a conflicting URL.
						// Ref. https://docs.microsoft.com/en-us/windows/win32/http/urlprefix-strings
						// Port 7070 has been chosen because CargoWise Process Controllers also use it, so there is potentially already a firewall
						// rule in place to allow the traffic, rather than requiring customers to configure a different port.
						var listenPrefix = UrlAclUrlPrefix + Guid.NewGuid().ToString();

						var randomSecret = GenerateRandomSecret(32);
						// Change from http://+:7070/... to http://my-hostname-here:7070/... so that other machines can reach this URL
						ListenUrl = listenPrefix.Replace("+", Dns.GetHostName()) + "/" + randomSecret;

						webapp = WebApp.Start(listenPrefix, app => app.MapSignalR($"/{randomSecret}/signalr", new HubConfiguration()));
					}
				}
			});
		}

		static bool HybridModuleEnabledForUser(string moduleId)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return GlbStaff.CurrentUser.CheckWinzorEnabledForUser() && (GlbStaff.CurrentUser.CheckFeatureEnabledForUser(StmFeatureTest.WinzorAllFeaturesCode) || GlbStaff.CurrentUser.CheckFeatureEnabledForUser(moduleId));
			}
		}

		static string GenerateRandomSecret(int size)
		{
			using (var generator = RandomNumberGenerator.Create())
			{
				var randomSecret = new byte[size];
				generator.GetBytes(randomSecret);
				return Convert.ToBase64String(randomSecret).Replace('+', '-').Replace('/', '_');
			}
		}

		/// <summary>
		/// Starts the listener and launches the Blazor client application
		/// This should cause a signal R connection to be established between this instance of the CargoWise WinForms and the Blazor app server
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void Initialise()
		{
			ExecuteActionAndDisableIfFails(() =>
			{
				startTime = DateTime.UtcNow;
				StartListener();
				// Not production ready; protected by feature flag.
				ObjectFactory.Get<IBlazorClientAppLauncher>().Launch(new Uri(Env.Registry.BlazorUrl), ListenUrl);

				winFormsListenerState.Transition(WinFormsListenerState.State.Initialised);
			});
		}

		public void Disable()
		{
			winFormsListenerState.Transition(WinFormsListenerState.State.Disabled);
		}

		void ExecuteActionAndDisableIfFails(Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Disable();
				var message = $"An unexpected error occurred in {nameof(WinFormsListener)}. {nameof(WinFormsListener)} has been marked faulted and {nameof(OpenModule)} disabled."; // This is a developer message
				ExceptionReporter.Instance.ReportDeveloperException("57933393-0c02-4f51-9e22-5ec5323b62b0", message, ex);
			}
		}

		public string ListenUrl { get; private set; }

		volatile IDisposable webapp;
		readonly object webappCreationLock = new object();

		public void Dispose() => webapp?.Dispose();

		class WinFormsListenerState
		{
			internal State CurrentState { get; private set; } = State.Uninitialised;

			internal enum State
			{
				Uninitialised = 0,
				Initialised = 1,
				Disabled = 2
			}

			readonly List<(State StartState, State TargetState)> allowedTransitions = new List<(State, State)>
			{
				(State.Uninitialised, State.Initialised),
				(State.Uninitialised, State.Disabled),
				(State.Initialised, State.Disabled),
				(State.Disabled, State.Disabled),
			};

			internal void Transition(State newState)
			{
				if (!allowedTransitions.Any(i => i.StartState == CurrentState && i.TargetState == newState))
				{
					throw new Exception($"Unallowed transition {CurrentState} -> {newState}");
				}
				else
				{
					CurrentState = newState;
				}
			}
		}
	}
}
