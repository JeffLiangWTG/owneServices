using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Core
{
	public static class ClientDllChecker
	{
		/// <summary>
		/// Checks that the loaded client DLL matches what the database expects.
		/// If a client previously did not have a client DLL, but now has one, the database is updated to indicate that the client DLL is now required.
		/// If a client previously did have a client DLL, but they now have a different one, or none, this is an error.
		/// </summary>
		/// <returns>A Result object. If the IsOKToRun field is false, ErrorMessage will describe the problem.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May not have access to Res Strings")]
		public static Result CheckRegistry()
		{
			var result = new Result();

			var expectedClientDLL = EnvProxy.Instance.Registry.ExpectedClientDLL;
			var loadedClientAssembly = ClientHookLoader.Instance.ClientAssembly;    // this always loads using expected clientdll
			var loadedAssemblyName = (loadedClientAssembly == null) ? null : loadedClientAssembly.GetName().Name;

			if (expectedClientDLL != loadedAssemblyName)
			{
				if (expectedClientDLL == null) // Not Expected + Loaded
				{
					result.ErrorMessage =
						"This " + Constants.ProductName + " database is configured to run without any client-specific module, but it has loaded ('" + loadedAssemblyName + "'). "
						+ "This problem can be caused by deleting files from the " + Constants.ProductName + " program directory, "
						+ "or by installing an upgrade package that does not match this database. "
						+ "If you are unable to resolve this problem, please contact the " + Constants.ProductSupportName + " desk.";
				}
				else if (loadedAssemblyName == null) // Expected + Not Loaded
				{
					if (Enum.IsDefined(typeof(DeletedClientDll), expectedClientDLL))
					{
						EnvProxy.Instance.Registry.ExpectedClientDLL = null;
					}
					else
					{
						result.ErrorMessage =
							"This " + Constants.ProductName + " database is configured to run with the '" + expectedClientDLL + "' client-specific module, but it has not loaded. "
							+ "This problem can be caused by deleting files from the " + Constants.ProductName + " program directory, "
							+ "or by installing an upgrade package that does not match this database. "
							+ "If you are unable to resolve this problem, please contact the " + Constants.ProductSupportName + " desk.";
					}
				}
				else // Expected One + Loaded Another
				{
					result.ErrorMessage =
						"This " + Constants.ProductName + " database is configured to run with the '" + expectedClientDLL + "' client-specific module, but another one has loaded ('" + loadedAssemblyName + "'). "
						+ "This problem can be caused by deleting files from the " + Constants.ProductName + " program directory, "
						+ "or by installing an upgrade package that does not match this database. "
						+ "If you are unable to resolve this problem, please contact the " + Constants.ProductSupportName + " desk.";
				}
			}

			result.IsOKToRun = string.IsNullOrWhiteSpace(result.ErrorMessage);
			return result;
		}

		public class Result
		{
			public bool IsOKToRun;
			public string ErrorMessage;
		}
	}
}
