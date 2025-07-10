using System;

namespace CargoWise.Loader.Common
{
	public interface IEnvironmentProxy
	{
		bool Is64BitOperatingSystem { get; }
		int TickCount { get; }
		string GetEnvironmentVariable(string variable);
		string GetFolderPath(Environment.SpecialFolder folder);
	}
}
