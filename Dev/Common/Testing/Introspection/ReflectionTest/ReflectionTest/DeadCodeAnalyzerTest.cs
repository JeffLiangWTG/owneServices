using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	class DeadCodeAnalyzerTest : TestCase
	{
		const string assemblyName = "Test.dll";

		string binaryPath;

		protected override void SetUp()
		{
			base.SetUp();

			binaryPath = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(binaryPath);
		}

		protected override void TearDown()
		{
			base.TearDown();

			Directory.Delete(binaryPath, true);
		}

		public void TestNoUnusedTypes()
		{
			//create a single assembly with two types. one uses the other
			CompileTestDll(assemblyName, @"
				namespace Test
				{
					class TypeOne
					{
						void TakeTwo()
						{
							var two = new TypeTwo();
						}
					}

					public class TypeTwo
					{
						void TakeOne()
						{
							var one = new TypeOne();
						}
					}
				}");

			var analyzer = new DeadCodeAnalyzer(binaryPath);
			analyzer.AnalyzeAssembly(assemblyName, BuildXml.Instance);

			AssertContainsExactElementsInAnyOrder(nameof(analyzer.DefinedTypes), new[] { "Test,Test.TypeOne", "Test,Test.TypeTwo" }, analyzer.DefinedTypes);
			AssertContainsExactElementsInAnyOrder(nameof(analyzer.UsedTypes), new[] { "Test,Test.TypeOne", "Test,Test.TypeTwo" }, analyzer.UsedTypes);
		}

		public void TestNoUnusedTypesOnNestedClass()
		{
			CompileTestDll(assemblyName, @"
				namespace Test
				{
					internal class TypeOne
					{
						void TakeTwo()
						{
							var two = TestStaticRootClass.NestedStaticClass.TakeOne();
						}

						public string Path { get; set; }
					}

					internal static class TestStaticRootClass
					{
						static string GetPath(string filename)
						{
							return ""abc: "" + filename;
						}

						internal static class NestedStaticClass
						{
							public static TypeOne TakeOne()
							{
								var one = new TypeOne();
								one.Path = GetPath(""x"");
								return one;
							}
						}
					}
				}");

			var analyzer = new DeadCodeAnalyzer(binaryPath);
			analyzer.AnalyzeAssembly(assemblyName, BuildXml.Instance);

			var expectedTypes = new[]
			{
				"Test,Test.TypeOne",
				"Test,Test.TestStaticRootClass",
				"Test,Test.TestStaticRootClass/NestedStaticClass"
			};

			AssertContainsExactElementsInAnyOrder(nameof(analyzer.DefinedTypes), expectedTypes, analyzer.DefinedTypes);
			AssertContainsExactElementsInAnyOrder(nameof(analyzer.UsedTypes), expectedTypes, analyzer.UsedTypes);
		}

		public void TestUnusedTypes()
		{
			//create a single assembly with three types. nothing uses them
			CompileTestDll(assemblyName, @"
				namespace Test
				{
					class PrivateClass
					{ }

					public class PublicClass
					{ }

					internal class InternalClass
					{ }
				}");

			var analyzer = new DeadCodeAnalyzer(binaryPath);
			analyzer.AnalyzeAssembly(assemblyName, BuildXml.Instance);

			AssertContainsExactElementsInAnyOrder(nameof(analyzer.DefinedTypes), new[] { "Test,Test.PrivateClass", "Test,Test.PublicClass", "Test,Test.InternalClass" }, analyzer.DefinedTypes);
			AssertContainsExactElementsInAnyOrder(nameof(analyzer.UsedTypes), Array.Empty<string>(), analyzer.UsedTypes);
		}

		void CompileTestDll(string assemblyName, params string[] sources)
		{
			using (var compiler = new CSharpCodeProvider())
			{
				var options = new CompilerParameters();
				options.ReferencedAssemblies.Add("System.dll");

				var path = Path.Combine(binaryPath, assemblyName);
				options.OutputAssembly = Path.Combine(path);

				var result = compiler.CompileAssemblyFromSource(options, sources);

				AssertEquals(string.Join(System.Environment.NewLine, result.Output.Cast<string>()), 0, result.Errors.Count);
			}
		}
	}
}
