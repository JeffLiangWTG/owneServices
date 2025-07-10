#if NETFRAMEWORK
using System;
using System.CodeDom.Compiler;
#endif
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using AppDomainWrappers.Net;
#endif
using CargoWise.Data;
using Dat.Integration;
#if NETFRAMEWORK
using Enterprise.Dat.Adapter.Test;
using Enterprise.ZArchitecture.Core;
using Microsoft.CSharp;
#endif
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TestDiscoveryTest : TestCase
	{
		public void TestFrequentlyFailing()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestFrequentlyFailing));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class ATest : TestCase
{
	[FrequentlyFailing]
	public void TestOne()
	{
	}

	public void TestTwo()
	{
	}
}

[FrequentlyFailing]
class BTest : TestCase
{
	public void TestThree()
	{
	}

	public void TestFour()
	{
	}
}

class CTest : TestCase
{
	public void TestFive()
	{
	}
}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "TestOne", "TestTwo", "TestThree", "TestFour", "TestFive" }, tests.Select(t => t.Identifier.TargetName));
				AssertEquals(DatTestFlags.FrequentlyFailing, tests.Single(t => t.Identifier.TargetName == "TestOne").DatTestFlags);
				AssertEquals(DatTestFlags.Default, tests.Single(t => t.Identifier.TargetName == "TestTwo").DatTestFlags);
				AssertEquals(DatTestFlags.FrequentlyFailing, tests.Single(t => t.Identifier.TargetName == "TestThree").DatTestFlags);
				AssertEquals(DatTestFlags.FrequentlyFailing, tests.Single(t => t.Identifier.TargetName == "TestFour").DatTestFlags);
				AssertEquals(DatTestFlags.Default, tests.Single(t => t.Identifier.TargetName == "TestFive").DatTestFlags);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestNewMethodDoesNotDuplicate()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestNewMethodDoesNotDuplicate));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class BaseTest : TestCase
{
	public void TestIt()
	{
	}
}

class ExtendedTest : BaseTest
{
	public new void TestIt()
	{
	}
}
");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "Test;BaseTest.TestIt", "Test;ExtendedTest.TestIt" }, tests.Select(t => t.Identifier.ToString()));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestNewMethodWithDifferentCapabilitiesDoesNotDuplicate()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestNewMethodWithDifferentCapabilitiesDoesNotDuplicate));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class BaseTest : TestCase
{
	public void TestIt()
	{
	}
}

class ExtendedTest : BaseTest
{
	[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
	public new void TestIt()
	{
	}
}
");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "Test;BaseTest.TestIt", "Test;ExtendedTest.TestIt" }, tests.Select(t => t.Identifier.ToString()));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestGetAllScopesWithBuildXml()
		{
			TestGetAllScopesWithDifferentFileName("Build.xml");
		}

		public void TestGetAllScopesWithSolutionsXml()
		{
			TestGetAllScopesWithDifferentFileName("Solutions.xml");
		}

		void TestGetAllScopesWithDifferentFileName(string xmlFileName)
		{
			TestGetAllScopes(xmlFileName);
			TestGetAllScopesMustIncludeAssembliesWithNUnitInTheName(xmlFileName);
			TestGetAllScopesForTargetWithSubmodule(xmlFileName);
			TestScopePathWhenrProjectNameDoesNotMatchAssemblyNameAndProjectDoesNotExist(xmlFileName);
			TestScopePathIsProjectFolderProjectNameDoesNotMatchAssemblyName(xmlFileName);
			TestScopePathIsProjectFolder(xmlFileName);
		}

		void TestGetAllScopes(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain("GetAllScopesDomain");

			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(srcDir);
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

			using NUnit.Framework;
			namespace Test {
				public class MyTest : TestCase
				{
					public void TestNotPerformance()
					{
					}

					public void FAT_Performance()
					{
					}
				}
			}
			");
			CompileDll(Path.Combine(binDir, "IndirectTest.dll"), new string[] { Path.Combine(binDir, "Test.dll"), typeof(TestCase).Assembly.Location }, @"
			using Test;

			public class TestClass : MyTest
			{
			}
			");
			CompileDll(Path.Combine(binDir, "NonTestAssembly.dll"), Array.Empty<string>(), @"
			public class NonTest {
				public int Add(int x, int y)
				{
					return x + y;
				}
			}
			");

			try
			{
				var buildxml = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"">

	<Solutions>
		<Solution Filename=""Common\Architecture\TileBar\C.sln"">
			<Bin>Test.dll</Bin>
			<Bin>NonTestAssembly.dll</Bin>
		</Solution>
		<Solution Filename=""SlnPath\Test.sln"">
			<Bin>IndirectTest.dll</Bin>
		</Solution>
	</Solutions>
</Build>

";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), buildxml);

				Directory.CreateDirectory(Path.Combine(srcDir, "Common", "Architecture", "TileBar"));
				File.WriteAllText(Path.Combine(srcDir, "Common", "Architecture", "TileBar", "C.sln"), TestHelper.GetSimpleSolutionFileContents("Foo"));
				Directory.CreateDirectory(Path.Combine(srcDir, "SlnPath"));
				File.WriteAllText(Path.Combine(srcDir, "SlnPath", "Test.sln"), TestHelper.GetSimpleSolutionFileContents());

				appDomain.SetData("binDir", binDir);
				appDomain.SetData("srcDir", srcDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
				});

				var scopes = (IEnumerable<Scope>)appDomain.GetData("scopes");

				var expected = new[] {
					new Scope("Test", Path.Combine(srcDir, "Common", "Architecture", "TileBar")),
					new Scope("IndirectTest", Path.Combine(srcDir, "SlnPath"))
				};
				AssertContainsExactElementsInAnyOrder(expected, scopes);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		void TestGetAllScopesMustIncludeAssembliesWithNUnitInTheName(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain("GetAllScopesDomain");

			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(srcDir);
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			CompileDll(Path.Combine(binDir, "NUnitCore.dll"), null, @"
			public class TestClass
			{
			}
			");

			try
			{
				var buildxml = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"">
	<Solutions>
		<Solution Filename=""Common\Architecture\TileBar\C.sln"">
			<Bin>NUnitCore.dll</Bin>
		</Solution>
	</Solutions>
</Build>
";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), buildxml);

				Directory.CreateDirectory(Path.Combine(srcDir, "Common", "Architecture", "TileBar"));
				File.WriteAllText(Path.Combine(srcDir, "Common", "Architecture", "TileBar", "C.sln"), TestHelper.GetSimpleSolutionFileContents());

				appDomain.SetData("binDir", binDir);
				appDomain.SetData("srcDir", srcDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
				});

				var scopes = (IEnumerable<Scope>)appDomain.GetData("scopes");

				var expected = new[] {
					new Scope("NUnitCore", Path.Combine(srcDir, "Common", "Architecture", "TileBar")),
				};

				AssertContainsExactElementsInAnyOrder(expected, scopes);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		void TestGetAllScopesForTargetWithSubmodule(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(Path.Combine(srcDir, "Root"));
			Directory.CreateDirectory(Path.Combine(srcDir, "Submodule"));
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			try
			{
				CompileTestDll(Path.Combine(binDir, "RootTest.dll"), """
					using NUnit.Framework;
					namespace Root
					{
						public class MyTest1 : TestCase
						{
							public void TestTest1()
							{
							}
						}
					}
					""");
				CompileTestDll(Path.Combine(binDir, "SubmoduleTest.dll"), """
					using NUnit.Framework;
					namespace Submodule
					{
						public class MyTest2 : TestCase
						{
							public void TestTest2()
							{
							}
						}
					}
					""");

				const string rootBuildXml = """
					<Build xmlns="http://wisetechglobal.com/DevTools/Build.xsd" MSBuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe">
						<Solutions>
							<Solution Filename="Root\Root.sln">
								<Bin>RootTest.dll</Bin>
							</Solution>
						</Solutions>
						<Submodules>
							<Submodule>Submodule</Submodule>
						</Submodules>
					</Build>
					""";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), rootBuildXml);

				const string submoduleBuildXml = """
					<Build xmlns="http://wisetechglobal.com/DevTools/Build.xsd" MSBuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe">
						<Solutions>
							<Solution Filename="Submodule.sln">
								<Bin>SubmoduleTest.dll</Bin>
							</Solution>
						</Solutions>
					</Build>
					""";
				File.WriteAllText(Path.Combine(srcDir, "Submodule", xmlFileName), submoduleBuildXml);

				File.WriteAllText(Path.Combine(srcDir, "Root", "Root.sln"), TestHelper.GetSimpleSolutionFileContents("Foo"));
				File.WriteAllText(Path.Combine(srcDir, "Submodule", "Submodule.sln"), TestHelper.GetSimpleSolutionFileContents("Bar"));

				AssertScopes(srcDir, binDir, [new Scope("RootTest", Path.Combine(srcDir, "Root"))]);
				AssertScopes(Path.Combine(srcDir, "Submodule"), binDir, [new Scope("SubmoduleTest", Path.Combine(srcDir, "Submodule"))]);
			}
			finally
			{
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}

			static void AssertScopes(string sourcePath, string binPath, IEnumerable<Scope> expectedScopes)
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var adw = new AppDomainWrapper();
				var appDomain = adw.CreateTestAppDomain("GetAllScopesDomain");
				try
				{
					appDomain.SetData("binDir", binPath);
					appDomain.SetData("srcDir", sourcePath);
					appDomain.DoCallBack(() =>
					{
						AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
					});

					var scopes = (IEnumerable<Scope>)appDomain.GetData("scopes");
					AssertContainsExactElementsInAnyOrder(expectedScopes, scopes);
				}
				finally
				{
					AppDomain.Unload(appDomain);
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}
#endif
		}

		public void TestBaseNamedCapabilities()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestBaseNamedCapabilities));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class ATest : TestCase
{
	public void TestASimpleOne()
	{
	}
}
");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData(
						"tests",
						new TestDiscovery(
							new TestAdapterContext(new Dictionary<string, string>()))
							.GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
				});
				var tests = (TestDescriptor[])appDomain.GetData("tests");
				AssertEquals(1, tests.Length);

				AssertContainsExactElementsInAnyOrder(expectedBaseCapabilities, tests[0].CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestNamedCapabilities()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestNamedCapabilities));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class Test : TestCase
{
	[RequiresSoftware(RequiredSoftware.IsVM)]
	public void TestOne()
	{
	}

	[RequiresSoftware(RequiredSoftware.VisualStudio)]
	public void TestTwo()
	{
	}

	[RequiresSoftware(RequiredSoftware.IsVM | RequiredSoftware.VisualStudio)]
	public void TestThree()
	{
	}

	[RequiresSoftware(RequiredSoftware.DotNetSdk)]
	public void TestFour()
	{
	}

	[RequiresSoftware(RequiredSoftware.DotNetSdk)]
	[RequiresSoftware(RequiredSoftware.IsVM)]
	public void TestFive()
	{
	}
}

[RequiresSoftware(RequiredSoftware.IsVM)]
abstract class BaseTest : TestCase
{
}

[RequiresSoftware(RequiredSoftware.VisualStudio)]
class TwoTest : BaseTest
{
    [RequiresSoftware(RequiredSoftware.DotNetSdk)]
    public void TestSix()
    {
    }
}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "TestOne", "TestTwo", "TestThree", "TestFour", "TestFive", "TestSix" }, tests.Select(t => t.Identifier.TargetName));

				Assert(tests.Select(t => t.ProjectDefinedCapabilityRequirements).All(c => c == 0));

				AssertTestSpecificCapabilities(new[] { "VM" }, tests.Single(t => t.Identifier.TargetName == "TestOne").CapabilityRequirements);
				AssertTestSpecificCapabilities(new[] { "VS2022" }, tests.Single(t => t.Identifier.TargetName == "TestTwo").CapabilityRequirements);
				AssertTestSpecificCapabilities(new[] { "VM", "VS2022" }, tests.Single(t => t.Identifier.TargetName == "TestThree").CapabilityRequirements);
				AssertTestSpecificCapabilities(new[] { "DOTNETSDK8.0" }, tests.Single(t => t.Identifier.TargetName == "TestFour").CapabilityRequirements);
				AssertTestSpecificCapabilities(new[] { "VM", "DOTNETSDK8.0" }, tests.Single(t => t.Identifier.TargetName == "TestFive").CapabilityRequirements);
				AssertTestSpecificCapabilities(new[] { "VM", "VS2022", "DOTNETSDK8.0" }, tests.Single(t => t.Identifier.TargetName == "TestSix").CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGlobalJsonIsUnchanged()
		{
			var globalJson = Path.Combine(BaseSourcePath, "global.json");
			var globalJsonText = new string(File.ReadAllText(globalJson).Where(ch => !char.IsWhiteSpace(ch)).ToArray());
			Assert("When global.json is changed, you MUST consider if the capability that is added when \"requiresSoftware.HasFlag(RequiredSoftware.DotNetSdk)\" in the method TestDiscovery.GetTests() should add a different SDK capability.", globalJsonText == @"{""sdk"":{""version"":""8.0.100"",""rollForward"":""latestPatch""}}");
		}

		public void TestALPOnlyAttribute()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class Test : TestCase
{
	[ALPOnly]
	public void TestSomething()
	{
	}
}
");

			var adw = new AppDomainWrapper();
			var appDomainAlp = adw.CreateTestAppDomain(nameof(TestALPOnlyAttribute) + "Alp");
			var appDomainDpr = adw.CreateTestAppDomain(nameof(TestALPOnlyAttribute) + "Dpr");
			try
			{
				appDomainAlp.SetData("binDir", binDir);
				appDomainAlp.DoCallBack(() =>
				{
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.9.23.1", DateTime.Now, "ALP")))
					{
						AppDomain.CurrentDomain.SetData(
							"tests",
							new TestDiscovery(
								new TestAdapterContext(new Dictionary<string, string>()))
								.GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
					}
				});
				var tests = (TestDescriptor[])appDomainAlp.GetData("tests");
				AssertEquals(1, tests.Length);

				appDomainDpr.SetData("binDir", binDir);
				appDomainDpr.DoCallBack(() =>
				{
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.9.23.1", DateTime.Now, "DPR")))
					{
						AppDomain.CurrentDomain.SetData(
							"tests",
							new TestDiscovery(
								new TestAdapterContext(new Dictionary<string, string>()))
								.GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
					}
				});
				tests = (TestDescriptor[])appDomainDpr.GetData("tests");
				AssertEquals(0, tests.Length);
			}
			finally
			{
				AppDomain.Unload(appDomainAlp);
				AppDomain.Unload(appDomainDpr);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestMissingAssemblyException()
		{
			var ex = AssertExceptionThrown<FileNotFoundException>(() => new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", TempForTest.TempPath, "NonExisting").ToArray());
			AssertEquals("Could not find file or assembly 'NonExisting' in '" + TempForTest.TempPath + "'.", ex.Message);
		}

		public void TestTargetFrameworks()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestTargetFrameworks));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class OneTest : TestCase
{
	[TargetFrameworks(TargetFramework.NetFramework)]
	public void TestNet48()
	{
	}

	[TargetFrameworks(TargetFramework.NetCore)]
	public void TestNet7()
	{
	}

	[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
	public void TestBoth()
	{
	}
}

[TargetFrameworks(TargetFramework.NetCore)]
abstract class BaseTest : TestCase
{
}

class TwoTest : BaseTest
{
	public void TestBaseClassAttributesAreIgnored()
	{
	}
}

[TargetFrameworks(TargetFramework.NetFramework)]
class ThreeTest : BaseTest
{
	public void TestBaseClassAttributesAreOverriden()
	{
	}

	[TargetFrameworks(TargetFramework.NetCore)]
	public void TestAttributeOnMethodOverridesAttributeOnClass()
	{
	}
}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.9.23.1", DateTime.Now, "ALP")))
					{
						AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
					}
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "TestNet48", "TestNet7[NetCore]", "TestBoth", "TestBoth[NetCore]", "TestBaseClassAttributesAreIgnored", "TestBaseClassAttributesAreOverriden", "TestAttributeOnMethodOverridesAttributeOnClass[NetCore]" }, tests.Select(t => t.Identifier.TargetName));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestTargetFramworksNonAlp()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestTargetFrameworks));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class OneTest : TestCase
{
	[TargetFrameworks(TargetFramework.NetFramework)]
	public void TestNet48()
	{
	}

	[TargetFrameworks(TargetFramework.NetCore)]
	public void TestNet7()
	{
	}

	[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
	public void TestBoth()
	{
	}
}

[TargetFrameworks(TargetFramework.NetFramework)]
abstract class BaseTest : TestCase
{
}

[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
class TwoTest : TestCase
{
	public void TestBothOnClass()
	{
	}
}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.9.23.1", DateTime.Now, "DPR")))
					{
						AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
					}
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				AssertContainsExactElementsInAnyOrder(new[] { "TestNet48", "TestBoth", "TestBothOnClass" }, tests.Select(t => t.Identifier.TargetName));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		[SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule", Justification = "Outside of CW1")]
		public void TestNet8RuntimeCapability()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestTargetFrameworks));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

using NUnit.Framework;

class OneTest : TestCase
{
	[TargetFrameworks(TargetFramework.NetCore)]
	public void TestNet8()
	{
	}
}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.9.23.1", DateTime.Now, "ALP")))
					{
						AppDomain.CurrentDomain.SetData("tests", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray());
					}
				});
				var test = ((IEnumerable<TestDescriptor>)appDomain.GetData("tests")).Single();
				AssertCollectionContains("CapabilityRequirements should contain DOTNET_RUNTIME_8.0_COMPAT", "DOTNET_RUNTIME_8.0_COMPAT", test.CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		[DeveloperOnlyTest]
		public void TestDiscoveryOnSingleAssembly_AccountingBusiness()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.GetDirectoryName(this.GetType().Assembly.Location); //"C:\git\wtg\CargoWise\Dev\Bin";
			const string testScope = "Enterprise.Accounting.Business.Testing";

			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDiscoveryOnSingleAssembly_AccountingBusiness));
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					var ctx = new TestAdapterContext(new Dictionary<string, string>());
					var appDomainTests = new TestDiscovery(ctx)
						.GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), testScope)
						.ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				Assert("Test Number should be more then 30,000", tests.Count() > 30_000);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				watch.Stop();
			}
			Assert($"Test should finish within 10 seconds, but takes: {watch.Elapsed}", watch.ElapsedMilliseconds < 10_000);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		[DeveloperOnlyTest]
		public void TestDiscoveryOnSingleAssembly_MasterFilesBusiness()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.GetDirectoryName(this.GetType().Assembly.Location); //"C:\git\wtg\CargoWise\Dev\Bin";
			const string testScope = "Enterprise.MasterFiles.Business.Test";

			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDiscoveryOnSingleAssembly_MasterFilesBusiness));
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					var ctx = new TestAdapterContext(new Dictionary<string, string>());
					var appDomainTests = new TestDiscovery(ctx)
						.GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), testScope)
						.ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});
				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				Assert("Test Number should be more then 30,000", tests.Count() > 30_000);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				watch.Stop();
			}
			Assert($"Test should finish within 5 seconds, but takes: {watch.Elapsed}", watch.ElapsedMilliseconds < 5_000);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestDatCapabilityRequirementAttribute()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);

			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDatCapabilityRequirementAttribute));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

					using NUnit.Framework;

					class TestCaseWithNoAttributes : TestCase
					{
						[DatCapabilityRequirement(""Single"")]
						public void TestSingle()
						{
						}

						[DatCapabilityRequirement(""Multiple1"")]
						[DatCapabilityRequirement(""Multiple2"")]
						public void TestMultiple()
						{
						}

						[DatCapabilityRequirement(""Duplicate"")]
						[DatCapabilityRequirement(""Duplicate"")]
						public void TestDuplicate()
						{
						}

						public void TestNone()
						{
						}
					}

					[DatCapabilityRequirement(""SingleClass"")]
					class TestCaseWithSingleAttribute : TestCase
					{
						[DatCapabilityRequirement(""Method"")]
						public void Test()
						{
						}
					}

					[DatCapabilityRequirement(""MultipleClass1"")]
					[DatCapabilityRequirement(""MultipleClass2"")]
					class TestCaseWithMultipleAttributes : TestCase
					{
						[DatCapabilityRequirement(""Method"")]
						public void Test()
						{
						}
					}

					[DatCapabilityRequirement(""DuplicateClass"")]
					[DatCapabilityRequirement(""DuplicateClass"")]
					class TestCaseWithDuplicateAttributes : TestCase
					{
						[DatCapabilityRequirement(""Method"")]
						public void Test()
						{
						}
					}

					[DatCapabilityRequirement(""InheritedClass"")]
					abstract class BaseTest : TestCase
					{
						[DatCapabilityRequirement(""InheritedMethod"")]
						abstract public void TestVirtual();
					}

					class TestCaseInherited : BaseTest
					{
						override public void TestVirtual()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);

				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");

				var expectedCapabilities = new Dictionary<string, string[]>
				{
					["TestCaseWithNoAttributes.TestSingle"] = new[] { "Single" },
					["TestCaseWithNoAttributes.TestMultiple"] = new[] { "Multiple1", "Multiple2" },
					["TestCaseWithNoAttributes.TestDuplicate"] = new[] { "Duplicate" },
					["TestCaseWithNoAttributes.TestNone"] = Array.Empty<string>(),
					["TestCaseWithSingleAttribute.Test"] = new[] { "SingleClass", "Method" },
					["TestCaseWithMultipleAttributes.Test"] = new[] { "MultipleClass1", "MultipleClass2", "Method" },
					["TestCaseWithDuplicateAttributes.Test"] = new[] { "DuplicateClass", "Method" },
					["TestCaseInherited.TestVirtual"] = new[] { "InheritedClass", "InheritedMethod" },
				};

				AssertContainsExactElementsInAnyOrder(expectedCapabilities.Keys, tests.Select(t => t.Identifier.ElementName + "." + t.Identifier.TargetName));

				foreach (var expectedCapability in expectedCapabilities)
				{
					var testDescriptor = tests.Single(t => expectedCapability.Key == t.Identifier.ElementName + "." + t.Identifier.TargetName);

					AssertTestSpecificCapabilities(expectedCapability.Value, testDescriptor.CapabilityRequirements, $"Testing capability requirements of {expectedCapability.Key}():");
				}
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestDatCapabilityRequirementAttribute_HigherVersion()
		{
			var currentSqlYear = GetCurrentSqlYear();
			var possibleNextVersion = "SQL" + (currentSqlYear + 3);
			var possibleFutureVersion = "SQL" + (currentSqlYear + 6);

			CombineAssertions(() =>
			{
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleNextVersion, null, possibleNextVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(null, possibleNextVersion, possibleNextVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleNextVersion, possibleFutureVersion, possibleFutureVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleFutureVersion, possibleNextVersion, possibleNextVersion);
			});
		}

		public void TestDatCapabilityRequirementAttribute_LowerVersion()
		{
			var currentSqlYear = GetCurrentSqlYear();
			var possibleOldVersion = "SQL" + (currentSqlYear - 2);
			var possibleLegacyVersion = "SQL" + (currentSqlYear - 4);

			CombineAssertions(() =>
			{
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleOldVersion, null, possibleOldVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(null, possibleLegacyVersion, possibleLegacyVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleOldVersion, possibleLegacyVersion, possibleLegacyVersion);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleLegacyVersion, possibleOldVersion, possibleOldVersion);
			});
		}

		public void TestDatCapabilityRequirementAttribute_UnspecifiedVersion()
		{
			DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(null, null, baseSqlCapability);
		}

		public void TestDatCapabilityRequirementAttribute_HigherVersionOrGreater()
		{
			var currentSqlYear = GetCurrentSqlYear();
			var possibleNextVersionPlus = "SQL" + (currentSqlYear + 2) + "+";

			CombineAssertions(() =>
			{
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleNextVersionPlus, null, possibleNextVersionPlus);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(null, possibleNextVersionPlus, possibleNextVersionPlus);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(baseSqlCapability, possibleNextVersionPlus, possibleNextVersionPlus);
			});
		}

		public void TestDatCapabilityRequirementAttribute_LowerVersionOrGreater()
		{
			var currentSqlYear = GetCurrentSqlYear();
			var possibleOldVersionPlus = "SQL" + (currentSqlYear - 2) + "+";

			CombineAssertions(() =>
			{
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore("SQL", null, baseSqlCapability);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(possibleOldVersionPlus, null, baseSqlCapability);
				DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(null, possibleOldVersionPlus, baseSqlCapability);
			});
		}

		static int GetCurrentSqlYear()
		{
			return int.Parse(baseSqlCapability.Substring(3));
		}

		void DatCapabilityRequirementAttribute_OverridesBaseSqlRequirementCore(string classCapability, string testCapability, string expectedCapability)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);

			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDatCapabilityRequirementAttribute));
			try
			{
				var classCapabilityLine = string.IsNullOrEmpty(classCapability) ? string.Empty : "[DatCapabilityRequirement(\"" + classCapability + "\")]";
				var testCapabilityLine = string.IsNullOrEmpty(testCapability) ? string.Empty : "[DatCapabilityRequirement(\"" + testCapability + "\")]";
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"
					using NUnit.Framework;
					" + classCapabilityLine + @"
					class TestCaseWithAttributes : TestCase
					{
						" + testCapabilityLine + @"
						public void TestSql()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);

				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				var testDescriptors = tests.Where(t => t.Identifier.TargetName == "TestSql").ToArray();

				AssertEquals(1, testDescriptors.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "NET48", expectedCapability }, testDescriptors[0].CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestDatCapabilityRequirementAttributeDuplicateMethod()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDatCapabilityRequirementAttribute));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

					using NUnit.Framework;

					class TestCaseWithNoAttributes : TestCase
					{
						[DatCapabilityRequirement(""SQL2012"")]
						[DatCapabilityRequirement(""SQL2013"")]
						public void TestMultiple()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);

				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				var testDescriptors = tests.Where(t => t.Identifier.TargetName == "TestMultiple").ToArray();

				AssertEquals(1, testDescriptors.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "NET48", "SQL2012", "SQL2013" }, testDescriptors[0].CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestDatCapabilityRequirementAttributeDuplicateClass()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDatCapabilityRequirementAttribute));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

					using NUnit.Framework;

					[DatCapabilityRequirement(""SQL2020"")]
					[DatCapabilityRequirement(""SQL2021"")]
					class TestCaseWithMultipleAttributes : TestCase
					{
						public void TestMultipleClass()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);

				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				var testDescriptors = tests.Where(t => t.Identifier.TargetName == "TestMultipleClass").ToArray();

				AssertEquals(1, testDescriptors.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "NET48", "SQL2020", "SQL2021" }, testDescriptors[0].CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestDatCapabilityRequirementAttributeDuplicateInherited()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestDatCapabilityRequirementAttribute));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"

					using NUnit.Framework;

					[DatCapabilityRequirement(""SQL2019"")]
					abstract class BaseTest : TestCase
					{
						[DatCapabilityRequirement(""SQL2015"")]
						abstract public void TestVirtual();
					}

					[DatCapabilityRequirement(""SQL2012"")]
					class TestCaseInherited : BaseTest
					{
						[DatCapabilityRequirement(""SQL2017"")]
						override public void TestVirtual()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);

				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();

					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				var testDescriptors = tests.Where(t => t.Identifier.TargetName == "TestVirtual").ToArray();

				AssertEquals(1, testDescriptors.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "NET48", "SQL2015", "SQL2017" }, testDescriptors[0].CapabilityRequirements);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		public void TestAttibuteForNewMethodInInheritClass()
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var binDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binDir);
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain(nameof(TestAttibuteForNewMethodInInheritClass));
			try
			{
				CompileTestDll(Path.Combine(binDir, "Test.dll"), @"
					using NUnit.Framework;

					abstract class BaseTestCaseWithAttribute : TestCase
					{
						[SnailTest]
						[DatCapabilityRequirement(""SOURCE_CODE"")]
						public virtual void TestVirtual()
						{
						}
					}

					class TestCaseNoOverride : BaseTestCaseWithAttribute
					{
					}

					class TestCaseWithOverrideMethod : BaseTestCaseWithAttribute
					{
						public override void TestVirtual()
						{
						}
					}

					class TestCaseWithNewMethod : BaseTestCaseWithAttribute
					{
						public new void TestVirtual()
						{
						}
					}");

				appDomain.SetData("binDir", binDir);
				appDomain.DoCallBack(() =>
				{
					var appDomainTests = new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetTests("", (string)AppDomain.CurrentDomain.GetData("binDir"), "Test").ToArray();
					AppDomain.CurrentDomain.SetData("tests", appDomainTests);
				});

				var tests = (IEnumerable<TestDescriptor>)appDomain.GetData("tests");
				Assert("Test methods detected", tests.Count() == 3);

				var testCaseNoOverride = tests
					.FirstOrDefault(t => t.Identifier.ElementName == "TestCaseNoOverride");
				AssertNotNull("testCaseNoOverride", testCaseNoOverride);
				Assert("testCaseNoOverride should contains SOURCE_CODE capability",
					testCaseNoOverride.CapabilityRequirements.Contains("SOURCE_CODE"));

				var testCaseWithOverrideMethod = tests
					.FirstOrDefault(t => t.Identifier.ElementName == "TestCaseWithOverrideMethod");
				AssertNotNull("testCaseWithOverrideMethod", testCaseWithOverrideMethod);
				Assert("testCaseWithOverrideMethod should contains SOURCE_CODE capability",
					testCaseWithOverrideMethod.CapabilityRequirements.Contains("SOURCE_CODE"));

				var testCaseWithNewMethod = tests
					.FirstOrDefault(t => t.Identifier.ElementName == "TestCaseWithNewMethod");
				AssertNotNull("testCaseWithNewMethod", testCaseWithNewMethod);
				Assert("testCaseWithNewMethod should NOT contains SOURCE_CODE capability",
					!testCaseWithNewMethod.CapabilityRequirements.Contains("SOURCE_CODE"));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		void TestScopePathIsProjectFolder(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain("TestScopePathIsProjectFolder");

			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(srcDir);
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			CompileTestDll(Path.Combine(binDir, "Test.dll"), @"
			using NUnit.Framework;
			namespace Test {
				public class MyTest : TestCase
				{
					public void TestWhatever()
					{
					}
				}
			}
			");

			try
			{
				var buildxml = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solutions>
		<Solution Filename=""SolutionPath\Solution.sln"">
			<Bin>Test.dll</Bin>
		</Solution>
	</Solutions>
</Build>

";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), buildxml);
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath"));
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath", "ProjectPath"));
				File.WriteAllText(Path.Combine(srcDir, "SolutionPath", "Solution.sln"), TestHelper.GetSimpleSolutionFileContents(projectName: "Test", projectFilePath: @"ProjectPath\Test.csproj"));
				File.WriteAllText(Path.Combine(srcDir, "SolutionPath", "ProjectPath", "Test.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"/>");
				appDomain.SetData("binDir", binDir);
				appDomain.SetData("srcDir", srcDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
				});

				var scope = ((IEnumerable<Scope>)appDomain.GetData("scopes")).Single();
				AssertEquals("Test", scope.Name);
				AssertEquals(Path.Combine(srcDir, "SolutionPath", "ProjectPath"), scope.SourcePath);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		void TestScopePathIsProjectFolderProjectNameDoesNotMatchAssemblyName(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain("TestScopePathIsProjectFolderProjectNameDoesNotMatchAssemblyName");

			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(srcDir);
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			CompileTestDll(Path.Combine(binDir, "Enterprise.Test.dll"), @"
			using NUnit.Framework;
			namespace Test {
				public class MyTest : TestCase
				{
					public void TestWhatever()
					{
					}
				}
			}
			");

			try
			{
				var buildxml = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solutions>
		<Solution Filename=""SolutionPath\Solution.sln"">
			<Bin>Enterprise.Test.dll</Bin>
		</Solution>
	</Solutions>
</Build>

";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), buildxml);
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath"));
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath", "ProjectPath"));
				File.WriteAllText(Path.Combine(srcDir, "SolutionPath", "Solution.sln"), TestHelper.GetSimpleSolutionFileContents(projectName: "Test", projectFilePath: @"ProjectPath\Test.csproj"));
				File.WriteAllText(Path.Combine(srcDir, "SolutionPath", "ProjectPath", "Test.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><AssemblyName>Enterprise.Test</AssemblyName></PropertyGroup></Project>");
				appDomain.SetData("binDir", binDir);
				appDomain.SetData("srcDir", srcDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
				});

				var scope = ((IEnumerable<Scope>)appDomain.GetData("scopes")).Single();
				AssertEquals("Enterprise.Test", scope.Name);
				AssertEquals(Path.Combine(srcDir, "SolutionPath", "ProjectPath"), scope.SourcePath);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		void TestScopePathWhenrProjectNameDoesNotMatchAssemblyNameAndProjectDoesNotExist(string xmlFileName)
		{
#if NET
			Fail("TODO: convert this to AppDomainWrapper");
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var adw = new AppDomainWrapper();
			var appDomain = adw.CreateTestAppDomain("TestScopePathIsProjectFolderProjectNameDoesNotMatchAssemblyName");

			var srcDir = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(srcDir);
			var binDir = Path.Combine(srcDir, "bin");
			Directory.CreateDirectory(binDir);

			CompileTestDll(Path.Combine(binDir, "Enterprise.Test.dll"), @"
			using NUnit.Framework;
			namespace Test {
				public class MyTest : TestCase
				{
					public void TestWhatever()
					{
					}
				}
			}
			");

			try
			{
				var buildxml = @"
<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"">
	<Solutions>
		<Solution Filename=""SolutionPath\Solution.sln"">
			<Bin>Enterprise.Test.dll</Bin>
		</Solution>
	</Solutions>
</Build>

";
				File.WriteAllText(Path.Combine(srcDir, xmlFileName), buildxml);
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath"));
				Directory.CreateDirectory(Path.Combine(srcDir, "SolutionPath", "ProjectPath"));
				File.WriteAllText(Path.Combine(srcDir, "SolutionPath", "Solution.sln"), TestHelper.GetSimpleSolutionFileContents(projectName: "Test", projectFilePath: @"ProjectPath"));
				appDomain.SetData("binDir", binDir);
				appDomain.SetData("srcDir", srcDir);
				appDomain.DoCallBack(() =>
				{
					AppDomain.CurrentDomain.SetData("scopes", new TestDiscovery(new TestAdapterContext(new Dictionary<string, string>())).GetAllScopes((string)AppDomain.CurrentDomain.GetData("srcDir"), (string)AppDomain.CurrentDomain.GetData("binDir")).ToArray());
				});

				var scope = ((IEnumerable<Scope>)appDomain.GetData("scopes")).Single();
				AssertEquals("Enterprise.Test", scope.Name);
				AssertEquals(Path.Combine(srcDir, "SolutionPath"), scope.SourcePath);
			}
			finally
			{
				AppDomain.Unload(appDomain);
				Directory.Delete(binDir, true);
				Directory.Delete(srcDir, true);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

#if NETFRAMEWORK
		static void CompileTestDll(string path, params string[] sources)
		{
			using (var compiler = new CSharpCodeProvider())
			{
				var nunitDllFilePath = typeof(TestCase).Assembly.Location;
				var options = new CompilerParameters();
				options.ReferencedAssemblies.Add("System.dll");
				options.ReferencedAssemblies.Add(nunitDllFilePath);
				options.OutputAssembly = Path.Combine(path);
				var result = compiler.CompileAssemblyFromSource(options, sources);

				var nunitDllFileDestinationPath = Path.Combine(Path.GetDirectoryName(path), "NUnitCore.dll");
				if (!File.Exists(nunitDllFileDestinationPath))
				{
					File.Copy(nunitDllFilePath, nunitDllFileDestinationPath);
				}
				AssertEquals("Test Dll Compilation: " + string.Join(System.Environment.NewLine, result.Output.Cast<string>()), 0, result.Errors.Count);
			}
		}

		static void CompileDll(string path, IEnumerable<string> assemblyReferences = null, params string[] sources)
		{
			using (var compiler = new CSharpCodeProvider())
			{
				var options = new CompilerParameters();
				if (assemblyReferences != null)
				{
					foreach (var assembly in assemblyReferences)
					{
						options.ReferencedAssemblies.Add(assembly);
					}
				}
				options.OutputAssembly = Path.Combine(path);
				var result = compiler.CompileAssemblyFromSource(options, sources);

				AssertEquals("Test Dll Compilation: " + string.Join(System.Environment.NewLine, result.Output.Cast<string>()), 0, result.Errors.Count);
			}
		}
#endif

		static readonly string baseSqlCapability = SqlServerVersionNumber.SupportedVersions.Min().Generation.Name; // this will be something like "SQL2008R2" or "SQL2022", etc.

#if NETFRAMEWORK
		static readonly string[] expectedBaseCapabilities = { "NET48", baseSqlCapability };

		static void AssertTestSpecificCapabilities(IEnumerable<string> expectedCapabilities, IEnumerable<string> actualCapabilities, string message = null)
		{
			AssertContainsExactElementsInAnyOrder(message, expectedCapabilities.Union(expectedBaseCapabilities), actualCapabilities);
		}
#endif
	}
}
