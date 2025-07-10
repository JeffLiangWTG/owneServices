using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using NUnit.Framework;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.Startup.Testing
{
	sealed class IrrelevantReleasesTest : TestCase
	{
		#region Test

		public void TestNoIrrelevantReleases()
		{
			var irrelevantReleasesList = new List<string> {
				"EnvDTE",
				"Microsoft.VisualStudio"
			};

			AssertEquals($"Should not release {string.Join(",", irrelevantReleasesList)}", expected: false, CheckIrrelevantReleases(irrelevantReleasesList));
		}

		#endregion

		#region Implementation

		bool CheckIrrelevantReleases(List<string> irrelevantReleasesList)
		{
#if NET
			var deployedAssemblies = BuildXml.Instance.GetAllAssembliesDeployedToClient(AssemblyLoader.GetParentBinPath());
#else
			var deployedAssemblies = BuildXml.Instance.GetAllAssembliesDeployedToClient(AssemblyLoader.GetBinPath());
#endif
			foreach (var assemblyName in deployedAssemblies)
			{
				if (irrelevantReleasesList.Any((prefix) => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
