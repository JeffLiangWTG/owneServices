using System;
using System.Diagnostics;
using System.IO;

namespace WTG.TestHelpers.IISExpress
{
	public class SimpleIISExpressHostedApplication : IISExpressHostedBase
	{
		public SimpleIISExpressHostedApplication(int port, string path)
			: base(port)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(path));
			}

			if (!Directory.Exists(path))
			{
				throw new ArgumentException("No directory exists at the specified path.");
			}

			LocationOnDisk = NormalizePath(path);
		}

		public string LocationOnDisk { get; }

		protected override ProcessStartInfo GetProcessStartInfo(string iisProcessPath)
			=> GetProcessStartInfo(
				iisProcessPath,
				"/path:" + Path.GetFullPath(LocationOnDisk),
				"/port:" + Port);

		static string NormalizePath(string path)
		{
			var finalIndex = path.Length - 1;
			var lastPathChar = path[finalIndex];

			if (lastPathChar == Path.DirectorySeparatorChar || lastPathChar == Path.AltDirectorySeparatorChar)
			{
				path = path.Substring(0, finalIndex);
			}

			return path;
		}
	}
}
