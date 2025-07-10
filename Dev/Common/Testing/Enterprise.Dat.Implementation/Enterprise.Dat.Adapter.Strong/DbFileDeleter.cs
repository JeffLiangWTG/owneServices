using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	public static class DbFileDeleter
	{
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static bool HandleDbFileExistsException(ITaskLogger logger, string message)
		{
			var fileRegex = new Regex(@"Cannot create file \'(?<file>.*)\' because it already exists");
			var match = fileRegex.Match(message);
			if (match.Success)
			{
				logger.RecordInfo("Attempting to delete file " + match.Groups["file"].Value);
				try
				{
					File.Delete(match.Groups["file"].Value);
					return true;
				}
				catch (Exception ex2) when (!ex2.IsCriticalException())
				{
					logger.RecordInfo("Could not delete file " + match.Groups["file"].Value + "\r\n" + ex2.ToString());
				}
			}
			return false;
		}
	}
}
