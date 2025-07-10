using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UnitTestRunnerTest : TestCase
	{
#if !WINZOR
		public void TestGetReflectionTest()
		{
			var unitTestRunner = Type.GetType("Enterprise.ZArchitecture.GUI.Testing.UnitTestRunner, Enterprise.ZArchitecture.GUI.Test");
			var getReflectionTest = unitTestRunner.GetMethod("GetReflectionTest", BindingFlags.Static | BindingFlags.NonPublic);
			var result = getReflectionTest.Invoke(null, new object[] { null });
			AssertNull(result);

			var assemblies = new List<string> { "Enterprise.Foo", "Enterprise.ReflectionTest", "Enterprise.Bar" };
			result = getReflectionTest.Invoke(null, new object[] { assemblies });
			AssertEquals("Enterprise.ReflectionTest", result);

			var allAssemblies = AssembliesUnderTest.AllAssemblies;
			AssertEquals(2, allAssemblies.Length);
			AssertCollectionContains(allAssemblies, assembly => assembly == assemblies[0]);
			AssertCollectionContains(allAssemblies, assembly => assembly == assemblies[2]);
		}

		public void TestNamespaceRegex()
		{
			AssertNamespaceRegexCapturedGroup("Enterprise.ZArchitecture.GUI.Testing.TestNamespace", @"
using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI.Testing.TestNamespace
{
	public class TestClass
	{
	}
}
");

			AssertNamespaceRegexCapturedGroup("Enterprise.ZArchitecture.GUI.Testing.FileScopedNamespace", @"
using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI.Testing.FileScopedNamespace;

public class TestClass
{
}
");
		}

		public void AssertNamespaceRegexCapturedGroup(string expectedNamespace, string content)
		{
			var matches = UnitTestRunner.NamespaceRegex.Match(content);
			AssertEquals(matches.Groups.Count, 2);
			AssertEquals(expectedNamespace, matches.Groups[1].Value);
		}
#endif
	}
}
