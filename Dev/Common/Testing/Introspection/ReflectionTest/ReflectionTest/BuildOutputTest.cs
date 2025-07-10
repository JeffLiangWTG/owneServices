using System;
using System.IO;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	class BuildOutputTest : TestCase
	{
		public void TestNoReferenceAssembliesOnDat()
		{
			AssertArrayEqualsByElements(
				"Reference assemblies should be deleted from DAT builds to avoid bloating the DAT build content package, check the removerefs.cmd post build task",
				Array.Empty<string>(),
				Directory.GetDirectories(AssemblyLoader.GetBinPath(), "refs", SearchOption.AllDirectories));
		}
	}
}
