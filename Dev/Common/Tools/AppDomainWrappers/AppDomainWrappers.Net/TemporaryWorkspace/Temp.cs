using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AppDomainWrappers.Net
{
	/// <summary>
	/// A stand-in class, meant to be replaced, once CargoWise.IO.Temp is available.
	/// </summary>
	internal static class Temp
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		static extern uint GetTempPath(int nBufferLength, StringBuilder lpBuffer);

		static bool TryRetrieveTempDirectory(out string tempPath)
		{
			// Allocate buffer for the path
			var tempPathBuilder = new StringBuilder(260); // Max path length
			tempPath = string.Empty;

			try
			{
				// Call the Windows API GetTempPath function
				var result = GetTempPath(tempPathBuilder.Capacity, tempPathBuilder);

				if (result > 0 && result <= tempPathBuilder.Capacity)
				{
					tempPath = tempPathBuilder.ToString();
					return true;
				}
			}
			catch
			{
				throw;
			}

			return false;
		}

		internal static string TempPathWithoutCreating
		{
			get
			{
				const string path = "WiseTechGlobal";
				return TryRetrieveTempDirectory(out var tempPath) ? Path.Combine(tempPath, path) : string.Empty;
			}
		}
	}
}
