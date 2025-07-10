using System.IO;
using Enterprise.ZArchitecture.Core;
#if WINZOR
using System;
using CargoWiseNext.Infrastructure.Authentication;
using WinzorFramework;
#endif

namespace CargoWise.Main.Startup.DotNetVersionSwitch;

class DotNetCore8VersionManager : DotNetVersionManager
{
	public static DotNetCore8VersionManager Instance { get; } = new();

	public override MultilingualString MenuCaption { get; }
		= ResString.GetMultilingualString("ebfda26e-090b-441f-bc21-4c132dc80204", "Launch .NET 8 Version");

#if WINZOR
	public override bool IsVersionCurrentRunning()
	{
		return string.Equals(Environment.GetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS"), true.ToString(), StringComparison.CurrentCultureIgnoreCase);
	}

	public override Common.QueryString BuildQueryString()
	{
		return new Common.QueryString
		{
			{ QueryParameters.NetCore, true.ToString() },
		};
	}
#else
	public override bool IsVersionCurrentRunning()
#if NET8_0
	=> true;
#else
	=> false;
#endif
	public override string GetExePath()
	{
#pragma warning disable CS0436 // Type conflicts with imported type : CommonAssemblyInfo
		return GetExePath((currentExeDirectory, currentExeName) => Path.Combine(
			currentExeDirectory,
			CommonAssemblyInfo.CWNetCoreSubfolder,
			currentExeName));
	}
#endif
}
