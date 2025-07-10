using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace CargoWise.BuildTools;

public static class AssembliesUnderTest
{
	public static string[] Assemblies
	{
		set
		{
			fAssemblies = value;
		}
	}

	public static string[] AllAssemblies => fAllAssemblies;

	public static string[] AllAssemblyPaths
	{
		get
		{
			return BuildXml.Instance.GetAllAssembliesToBuild(false)
				//it is from Shared but we have tests for it
				//BUT when updated in Shared and no new tests merged to master yet TestAllClassesRequiringASpecificTestCaseHaveIt
				//is failing for all submissions in combined build and is sent in amnesty
				//.Concat(Enumerable.Repeat("CargoWise.DbUpgrader.Scripts.Definitions.dll", 1))
				.Where(IsAssemblyIncluded)
				.ToArray();
		}
	}

	public static string[] AllNetCoreAssemblyPaths
	{
		get
		{
			return BuildXml.Instance.GetAllAssembliesToBuild(false)
				.Where(IsNetCoreAssembly)
				.ToArray();
		}
	}

	static string[] fAllAssemblies
	{
		get
		{
			if (fAssemblies != null)
			{
				return fAssemblies;
			}

			return AllAssemblyPaths
				.Select(Path.GetFileNameWithoutExtension)
				.ToArray();
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Developer Test")]
	static string[] fAssemblies = null;

	static bool IsAssemblyIncluded(string name)
	{
		return !IgnoredAssemblies.Contains(name)
				&& !AssemblyChecker.IsNotTargetPrefix(name);
	}

	static bool IsNetCoreAssembly(string name)
	{
		return !IgnoredAssemblies.Contains(name)
				&& AssemblyChecker.IsNotTargetPrefix(name);
	}

	static readonly ImmutableHashSet<string> IgnoredAssemblies = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
		"Enterprise.CodeAnalysis.TestCode.dll",
		"Enterprise.ReflectionTest.dll"
	);
}
