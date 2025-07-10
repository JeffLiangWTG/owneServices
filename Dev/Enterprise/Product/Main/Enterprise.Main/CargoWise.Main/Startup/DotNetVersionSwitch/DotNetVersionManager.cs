using System;
using System.Diagnostics;
using System.IO;
using CargoWise.Application;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
#if WINZOR
using CargoWiseNext.Infrastructure.Authentication;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework;
using WinzorFramework.RemoteClientServices;
#endif

namespace CargoWise.Main.Startup.DotNetVersionSwitch;

interface IDotNetVersionManager
{
	MultilingualString MenuCaption { get; }

	void LaunchCurrentVersion();
	bool IsVersionCurrentRunning();
}

abstract class DotNetVersionManager : IDotNetVersionManager
{
	public abstract MultilingualString MenuCaption { get; }
	public abstract bool IsVersionCurrentRunning();

#if WINZOR
	public void LaunchCurrentVersion()
	{
		CargoWiseClientInvoker.Invoke((cws) =>
		{
			var query = BuildQueryString();
			var persist = WindowPersister.GetOpenFormUrls();
			if (!string.IsNullOrEmpty(persist))
			{
				query.Add(QueryParameters.Persist, persist);
			}

			var startupUrl = new UriBuilder(cws.ServerBaseUri)
			{
				Query = query.ToString()
			}.Uri;
			return cws.LifecycleService.StartNewApplicationAsync(startupUrl);
		});

		if (!Globals.IsTest)
		{
			System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait one seconds to give the new process a chance to start
			Enterprise.Environment.Env.ExitApplication();
		}
	}

	public abstract Common.QueryString BuildQueryString();
#else
	readonly IProgramRestarter programRestarter;
	public DotNetVersionManager() : this(ObjectFactory.Get<IProgramRestarter>())
	{
	}
	public DotNetVersionManager(IProgramRestarter programRestarter)
	{
		this.programRestarter = programRestarter ?? throw new ArgumentNullException(nameof(programRestarter));
	}

	public void LaunchCurrentVersion()
	{
		var exePath = GetExePath();
		if (!File.Exists(exePath))
		{
			throw new ApplicationException($"The executable for {MenuCaption} does not exist.");
		}

		var arguements = CommandLineArguments.UsedToLaunchApplication.Clone();
		arguements.OptionalArgs[ApplicationArguments.OptionSkipDotNetVersionSwitch] = true;
		programRestarter.Restart(exeFilePath: exePath, arguments: arguements);
	}

	public abstract string GetExePath();

	protected string GetExePath(Func<string, string, string> getFullExePath)
	{
		var currentProcessFullPath = Process.GetCurrentProcess().MainModule.FileName;
		var currentExeName = Path.GetFileName(currentProcessFullPath);
		var currentExeDirectory = Path.GetDirectoryName(currentProcessFullPath);
		var finalExePath = getFullExePath(currentExeDirectory, currentExeName);
		if (string.Equals(currentProcessFullPath, finalExePath, StringComparison.OrdinalIgnoreCase))
		{
			throw new ApplicationException($"The executable for {MenuCaption} is the same as the current executable.");
		}
		return finalExePath;
	}
#endif
}
