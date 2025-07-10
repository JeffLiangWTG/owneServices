using System.IO;
using Enterprise.ZArchitecture.Core;
#if WINZOR
using System;
using WinzorFramework;
#endif

namespace CargoWise.Main.Startup.DotNetVersionSwitch;

class DotNetFrameworkVersionManager : DotNetVersionManager
{
	public static DotNetFrameworkVersionManager Instance { get; } = new();

	public override MultilingualString MenuCaption { get; }
		= ResString.GetMultilingualString("b714d872-5b50-456a-a14c-e4a1abaf75d6", "Launch .Net Framework Version");

#if WINZOR
	public override bool IsVersionCurrentRunning()
	{
		return !string.Equals(Environment.GetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS"), true.ToString(), StringComparison.CurrentCultureIgnoreCase);
	}

	public override Common.QueryString BuildQueryString()
	{
		return new Common.QueryString();
	}
#else
	public override bool IsVersionCurrentRunning()
#if NETFRAMEWORK
	=> true;
#else
	=> false;
#endif

	public override string GetExePath()
	{
		return GetExePath((currentExeDirectory, currentExeName) => Path.Combine(
			new DirectoryInfo(currentExeDirectory).Parent.FullName,
			currentExeName));
	}
#endif
}
