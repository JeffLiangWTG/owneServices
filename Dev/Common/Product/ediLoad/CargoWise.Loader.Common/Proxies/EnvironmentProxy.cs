using System;

namespace CargoWise.Loader.Common
{
	public sealed class EnvironmentProxy : IEnvironmentProxy
	{
		public EnvironmentProxy()
		{
		}

		public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;

		public int TickCount
		{
			get { return Environment.TickCount; }
		}

		public string GetEnvironmentVariable(string variable)
		{
			return Environment.GetEnvironmentVariable(variable);
		}

		public string GetFolderPath(Environment.SpecialFolder folder)
		{
			var folderPath = Environment.GetFolderPath(folder);
			if (string.IsNullOrEmpty(folderPath))
			{
				throw new InvalidOperationException(string.Format("Couldn't find path for folder '{0}'.", folder));
			}
			return folderPath;
		}
	}
}
