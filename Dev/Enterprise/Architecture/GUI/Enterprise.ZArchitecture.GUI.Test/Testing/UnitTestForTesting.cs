using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using TestMethodInfo = NUnit.Framework.TestMethodInfo;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class UnitTestRunner
	{
		#region ShowTestsFromCheckedOutFiles
		public static void ShowTestsFromCheckedOutFiles(bool includeInheritTestClasses)
		{
			ShowUnitTestForm(GetInfosForCheckedOutFileTests(includeInheritTestClasses), "Checked Out Files Unit Tests");
		}

		public static void ShowTestsForBranchChanges(bool includeInheritTestClasses)
		{
			ShowUnitTestForm(GetInfosForBranchChanges(includeInheritTestClasses), $"Unit Tests for Changes in {SourceControl.EnterpriseDatabase.GetCurrentBranchName()}");
		}

		public static void ShowTestsFromUncommittedFiles(bool includeInheritTestClasses)
		{
			ShowUnitTestForm(GetInfosForUncommittedFileTests(includeInheritTestClasses), "Uncommitted Files Unit Tests");
		}

		static void ShowUnitTestForm(TestMethodInfo[] testMethods, string rootNodeName)
		{
			UnitTestForm form = null;
			try
			{
				SetupTestEnvironment();
				form = new UnitTestForm(testMethods, rootNodeName, true);
				AddTestListeners(form);
				form.FormClosed += (sender, e) => { NUnit.Framework.TestingState.TearDown(); };
				form.Show();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (form != null)
				{
					form.Dispose();
				}
				NUnit.Framework.TestingState.TearDown();
				Globals.Message.ShowError(e.Message);
			}
		}

		static TestMethodInfo[] GetInfosForAssemblies(List<string> assemblyNames, string solutionFileName)
		{
			var infos = new Dictionary<string, TestMethodInfo>();
			var assemblies = new Dictionary<string, List<Type>>();

			foreach (var assemblyName in assemblyNames)
			{
				LoadInfosFromAssemblies(infos, assemblyName, assemblies);
			}

			LoadCodeAnalysisTests(infos, assemblies);
			if (!string.IsNullOrEmpty(solutionFileName))
			{
				LoadAnalyzersRunnerTests(infos, GetProjectNames(solutionFileName));
			}
			return new List<TestMethodInfo>(infos.Values).ToArray();
		}

		static IEnumerable<string> GetProjectNames(string solutionFileName)
		{
			var solutionPath = BuildConstants.GetLocalPath(solutionFileName);
			var solution = new SolutionFile(solutionPath);
			var projects = solution.Projects;
			var projectNames = projects.Select(projectInformation => projectInformation.ProjectName);
			return projectNames;
		}

		static TestMethodInfo[] GetInfosForBranchChanges(bool includeInheritTestClasses)
		{
			return GetInfosForFiles(SourceControl.EnterpriseDatabase.GetFilesWithChangesInCurrentBranch(ReleaseInfo.Instance, includeDeletedFiles: false), includeInheritTestClasses);
		}

		static TestMethodInfo[] GetInfosForUncommittedFileTests(bool includeInheritTestClasses)
		{
			return GetInfosForFiles(SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges(includeDeletedFiles: false), includeInheritTestClasses);
		}

		static TestMethodInfo[] GetInfosForCheckedOutFileTests(bool includeInheritTestClasses)
		{
			return GetInfosForFiles(SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges(includeDeletedFiles: false), includeInheritTestClasses);
		}

		static TestMethodInfo[] GetInfosForFiles(IEnumerable<string> files, bool includeInheritTestClasses)
		{
			Dictionary<string, TestMethodInfo> infos = new Dictionary<string, TestMethodInfo>();
			ProjectFileParser projectFileParser = new ProjectFileParser();
			Dictionary<string, List<Type>> assemblies = new Dictionary<string, List<Type>>();
			var projectAssemmblyNames = new Dictionary<string, string>();
			var projectsList = new List<string>();

			foreach (string fileName in files)
			{
				if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					var projectAssemmblyName = projectFileParser.GetProjectAndAssemblyName(fileName);
					LoadInfosFromCheckedOutFile(infos, fileName, projectAssemmblyName.Value, assemblies, includeInheritTestClasses);
					if (string.IsNullOrEmpty(projectAssemmblyName.Key) || projectAssemmblyNames.ContainsKey(projectAssemmblyName.Key))
					{
						continue;
					}

					projectsList.Add(projectAssemmblyName.Key);
					projectAssemmblyNames.Add(projectAssemmblyName.Key, projectAssemmblyName.Value);
				}
			}

			LoadReflectionTest(infos, assemblies);
			LoadCodeAnalysisTests(infos, assemblies);
			LoadAnalyzersRunnerTests(infos, projectsList);

			return new List<TestMethodInfo>(infos.Values).ToArray();
		}

		static void LoadReflectionTest(Dictionary<string, TestMethodInfo> infos, Dictionary<string, List<Type>> assemblies)
		{
			var reflectionTestAssembly = GetReflectionTest(assemblies.Keys);
			var reflectionTestClasses = GetAssemblyClasses(assemblies, reflectionTestAssembly);
			if (reflectionTestClasses != null)
			{
				var applicableReflectionTests = new string[] { "ReflectionTest", "DeadCodeAnalyzerTest", "ProjectSettingsTest" }; // Exclude other test sets which include tests either too slow or DAT targeted only.
				foreach (var reflectionType in reflectionTestClasses)
				{
					if (!infos.ContainsKey(reflectionType.FullName) && applicableReflectionTests.Contains(reflectionType.Name))
					{
						infos.Add(reflectionType.FullName, new TestMethodInfo(reflectionTestAssembly, reflectionType.FullName, null));
					}
				}
			}
		}

		static void LoadAnalyzersRunnerTests(Dictionary<string, TestMethodInfo> infos, IEnumerable<string> projectAssemblyNames)
		{
			const string analyzersRunnerAssembly = "AnalyzersRunner";
			const string analyzersRunnerTestClassName = "RunAnalyzers";
			LoadAssemblySpecificStaticAnalyzersRunnerTests(infos, projectAssemblyNames, analyzersRunnerAssembly, $"{analyzersRunnerAssembly}.{analyzersRunnerTestClassName}");
		}

		static void LoadAssemblySpecificStaticAnalyzersRunnerTests(Dictionary<string, TestMethodInfo> infos, IEnumerable<string> projectAssemblyName, string testAssembly, string fullyQualifiedTestClassName)
		{
			var testClassType = Type.GetType($"{fullyQualifiedTestClassName}, {testAssembly}");

			if (testClassType != null && !infos.ContainsKey(fullyQualifiedTestClassName))
			{
				foreach (var projectName in projectAssemblyName)
				{
					var testName = $"Test{projectName.Replace(".csproj", "").Replace('.', '_')}";
					var key = $"{testAssembly}.{testName}";

					if (!infos.ContainsKey(key) && testClassType.GetMethod(testName) != null)
					{
						infos.Add(key, new TestMethodInfo(testAssembly, fullyQualifiedTestClassName, testName));
					}
				}
			}
		}

		static void LoadCodeAnalysisTests(Dictionary<string, TestMethodInfo> infos, Dictionary<string, List<Type>> assemblies)
		{
			const string codeAnalysisAssembly = "Enterprise.CodeAnalysis.Runner";
			const string codeAnalysisCustomTestClassName = "RunCustomEnterpriseCodeAnalysisRules";
			const string codeAnalysisSystemTestClassName = "RunSystemCodeAnalysisRules";

			LoadAssemblySpecificStaticAnalysisTests(infos, assemblies, codeAnalysisAssembly, $"{codeAnalysisAssembly}.{codeAnalysisCustomTestClassName}");
			LoadAssemblySpecificStaticAnalysisTests(infos, assemblies, codeAnalysisAssembly, $"{codeAnalysisAssembly}.{codeAnalysisSystemTestClassName}");
		}

		static void LoadAssemblySpecificStaticAnalysisTests(Dictionary<string, TestMethodInfo> infos, Dictionary<string, List<Type>> assemblies, string testAssembly, string fullyQualifiedTestClassName)
		{
			var testClassType = Type.GetType($"{fullyQualifiedTestClassName}, {testAssembly}");

			if (testClassType != null && !infos.ContainsKey(fullyQualifiedTestClassName))
			{
				foreach (var assembly in assemblies)
				{
					var testName = $"Test{assembly.Key.Replace('.', '_')}";
					var key = $"{testAssembly}.{testClassType.Name}.{testName}";

					if (!infos.ContainsKey(key) && testClassType.GetMethod(testName) != null)
					{
						infos.Add(key, new TestMethodInfo(testAssembly, fullyQualifiedTestClassName, testName));
					}
				}
			}
		}

		static void LoadAllAssemblies(Dictionary<string, List<Type>> assemblies)
		{
			foreach (string assemblyName in AssemblyNamesInSolution(null))
			{
				GetAssemblyClasses(assemblies, assemblyName);
			}
		}

		static void LoadInfosFromAssemblies(Dictionary<string, TestMethodInfo> infos, string assemblyName, Dictionary<string, List<Type>> assemblies)
		{
			List<Type> assemblyClasses = GetAssemblyClasses(assemblies, assemblyName);

			if (assemblyClasses != null)
			{
				foreach (Type assemblyClass in assemblyClasses)
				{
					AddTestClass(infos, assemblyName, assemblies, false, assemblyClass);
				}
			}
		}

		internal static Regex NamespaceRegex { get; } = new (@"namespace\s+([^;\s]+)");

		static void LoadInfosFromCheckedOutFile(Dictionary<string, TestMethodInfo> infos, string fileName, string assemblyName, Dictionary<string, List<Type>> assemblies, bool includInheritTestClasses)
		{
			List<Type> assemblyClasses = GetAssemblyClasses(assemblies, assemblyName);
			if (assemblyClasses != null)
			{
				if (File.Exists(fileName))
				{
					string dataContents = File.ReadAllText(fileName);
					Match namespaceMatch = NamespaceRegex.Match(dataContents);
					if (namespaceMatch.Success)
					{
						string currentNamespace = namespaceMatch.Groups[1].Value;
						dataContents = dataContents.Substring(namespaceMatch.Index + namespaceMatch.Value.Length);
						Regex classRegex = new Regex("\\s+class\\s+([\\w\\d]+)");
						Match match = classRegex.Match(dataContents);
						List<string> previousClasses = new List<string>();
						string buffer;
						while (match.Success)
						{
							string currentClass = match.Groups[1].Value;
							buffer = dataContents.Substring(0, match.Index);
							namespaceMatch = NamespaceRegex.Match(buffer);
							if (namespaceMatch.Success)
							{
								currentNamespace = namespaceMatch.Groups[1].Value;
								previousClasses.Clear();
								dataContents = dataContents.Substring(namespaceMatch.Index + namespaceMatch.Length);
								match = classRegex.Match(dataContents);
								continue;
							}

							AddTestClassIfMatch(infos, assemblyName, assemblies, includInheritTestClasses, assemblyClasses, currentNamespace, previousClasses, currentClass);
							previousClasses.Add(currentClass);
							match = match.NextMatch();
						}
					}
				}
			}
		}

		static bool AddTestClassIfMatch(Dictionary<string, TestMethodInfo> infos, string assemblyName, Dictionary<string, List<Type>> assemblies, bool includInheritTestClasses, List<Type> assemblyClasses, string currentNamespace, List<string> previousClasses, string currentClass)
		{
			string currentNamespaceWithTestingAdded = currentNamespace + ".Testing";
			string currentClassWithTestAdded = currentClass + "Test";
			bool isPreviousClassesEmpty = previousClasses.Count == 0;
			foreach (Type assemblyClass in assemblyClasses)
			{
				if (assemblyClass.Name.Equals(currentClass, StringComparison.Ordinal) || assemblyClass.Name.Equals(currentClassWithTestAdded, StringComparison.Ordinal))
				{
					string assemblyClassName = assemblyClass.FullName;
					if (assemblyClass.Namespace.Equals(currentNamespace, StringComparison.Ordinal) || assemblyClass.Namespace.Equals(currentNamespaceWithTestingAdded, StringComparison.Ordinal))
					{
						if (isPreviousClassesEmpty || assemblyClass.DeclaringType == null || (assemblyClass.IsNested && previousClasses.Contains(assemblyClass.DeclaringType.Name)))
						{
							AddTestClass(infos, assemblyName, assemblies, includInheritTestClasses, assemblyClass);
							return true;
						}
					}
				}
			}
			return false;
		}

		static void AddTestClass(Dictionary<string, TestMethodInfo> infos, string assemblyName, Dictionary<string, List<Type>> assemblies, bool includInheritTestClasses, Type assemblyClass)
		{
			if (!infos.ContainsKey(assemblyClass.FullName))
			{
				infos.Add(assemblyClass.FullName, new TestMethodInfo(assemblyName, assemblyClass.FullName, null));
			}
			if (includInheritTestClasses)
			{
				LoadInfosForInheritTestClasses(infos, assemblies, assemblyClass);
			}
		}

		static void LoadInfosForInheritTestClasses(Dictionary<string, TestMethodInfo> infos, Dictionary<string, List<Type>> assemblies, Type assemblyClass)
		{
			// Load all assemblies here to get inherit test classes from
			Dictionary<string, List<Type>> assembliesAll = new Dictionary<string, List<Type>>();
			LoadAllAssemblies(assembliesAll);

			foreach (List<Type> classTypes in assembliesAll.Values)
			{
				foreach (Type classType in classTypes)
				{
					if (classType.IsSubclassOf(assemblyClass) && ShouldBeTested(classType, true) && !infos.ContainsKey(classType.FullName))
					{
						assemblies.Add(classType.FullName, classTypes);
						infos.Add(classType.FullName, new TestMethodInfo(classType.Assembly.FullName, classType.FullName, null));
					}
				}
			}
		}

		static List<Type> GetAssemblyClasses(Dictionary<string, List<Type>> assemblies, string assemblyName)
		{
			List<Type> assemblyClasses = null;
			if (!string.IsNullOrEmpty(assemblyName) && !assemblies.TryGetValue(assemblyName, out assemblyClasses))
			{
				Assembly currentAssembly = null;
				try
				{
					currentAssembly = Assembly.Load(assemblyName);
				}
				catch (FileNotFoundException) { }
				catch (FileLoadException) { }
				catch (BadImageFormatException) { }

				if (currentAssembly != null)
				{
					assemblyClasses = new List<Type>();
					foreach (Type testType in currentAssembly.GetTypes())
					{
						if (ShouldBeTested(testType, true))
						{
							assemblyClasses.Add(testType);
						}
					}
					assemblies.Add(assemblyName, assemblyClasses);
				}
			}
			return assemblyClasses;
		}

		static bool ShouldBeTested(Type type, bool shouldIncludeAbstract)
		{
			return (shouldIncludeAbstract || !type.IsAbstract) &&
				type.IsSubclassOf(typeof(TestCase)) &&
				type.GetCustomAttributes(typeof(DoNotAddToTestTreeAttribute), false).Length == 0;
		}
		#endregion

		#region ShowTestsFromCheckedOutProjects
		public static void ShowTestsFromCheckedOutProjects()
		{
			ShowUnitTestForm(GetInfosForCheckedOutProjectTests(), "Checked Out Projects Unit Tests");
		}

		public static void ShowTestsFromProjectsWithBranchChanges()
		{
			ShowUnitTestForm(GetInfosForProjectBranchChanges(), $"Unit Tests for Project Changes in {SourceControl.EnterpriseDatabase.GetCurrentBranchName()}");
		}

		public static void ShowTestsFromProjectsWithUncommittedChanges()
		{
			ShowUnitTestForm(GetInfosForUncommittedProjectTests(), "Uncommitted Projects Unit Tests");
		}

		static TestMethodInfo[] GetInfosForCheckedOutProjectTests()
		{
			return GetInfosForProjects(SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges(includeDeletedFiles: false));
		}

		static TestMethodInfo[] GetInfosForProjectBranchChanges()
		{
			return GetInfosForProjects(SourceControl.EnterpriseDatabase.GetFilesWithChangesInCurrentBranch(ReleaseInfo.Instance, includeDeletedFiles: false));
		}

		static TestMethodInfo[] GetInfosForUncommittedProjectTests()
		{
			return GetInfosForProjects(SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges(includeDeletedFiles: false));
		}

		static TestMethodInfo[] GetInfosForProjects(IEnumerable<string> files)
		{
			Dictionary<string, TestMethodInfo> infos = new Dictionary<string, TestMethodInfo>();
			ProjectFileParser projectFileParser = new ProjectFileParser();
			Dictionary<string, List<Type>> assemblies = new Dictionary<string, List<Type>>();
			var projectsList = new List<string>();
			foreach (string fileName in files)
			{
				if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					var projectAssemmblyName = projectFileParser.GetProjectAndAssemblyName(fileName);
					List<Type> assemblyClasses = GetAssemblyClasses(assemblies, projectAssemmblyName.Value);
					if (string.IsNullOrEmpty(projectAssemmblyName.Key) || projectsList.Contains(projectAssemmblyName.Key))
					{
						continue;
					}

					projectsList.Add(projectAssemmblyName.Key);
					if (assemblyClasses != null)
					{
						foreach (Type classType in assemblyClasses)
						{
							string classTypefullName = classType.FullName;
							if (!infos.ContainsKey(classTypefullName))
							{
								infos.Add(classTypefullName, new TestMethodInfo(projectAssemmblyName.Value, classTypefullName, null));
							}
						}
					}
				}
			}
			LoadReflectionTest(infos, assemblies);
			LoadCodeAnalysisTests(infos, assemblies);
			LoadAnalyzersRunnerTests(infos, projectsList);
			return new List<TestMethodInfo>(infos.Values).ToArray();
		}
		#endregion

		static void SetupTestEnvironment()
		{
			TestingState.Setup();
			SqlSynonymNameResolver.Initialize();
		}

		public static void ShowUserSubmittedFailures(Guid userTestPK, bool sortTestsAlphabetically = true)
		{
			SetupTestEnvironment();

			var form = new UnitTestForm();
			form.SortTestsAlphabeticallyCheckBox.Checked = sortTestsAlphabetically;
			form.FormClosed += (sender, e) => { TestingState.TearDown(); };
			AddTestListeners(form);
			form.LoadUserSubmittedDATFailures(userTestPK);
			form.Show();
		}

		public static void ShowASpecificListOfUnitTests(TestMethodInfo[] infos, bool groupTestsByAssembly)
		{
			SetupTestEnvironment();

			var form = new UnitTestForm(infos, Constants.ProductName + " Unit Tests", groupTestsByAssembly);
			form.FormClosed += (sender, e) => { TestingState.TearDown(); };
			AddTestListeners(form);
			form.Show();
		}

		public static void ShowUnitTestsInSolution(string solution, ErrorDescriptionList errorDescriptions)
		{
			ShowUnitTestsFromAssemblies(AssemblyNamesInSolution(solution), solution, errorDescriptions);
		}

		public static void ShowUnitTestsFromAssemblies(IEnumerable<string> assemblies, string rootName, ErrorDescriptionList errorDescriptions, bool showDialog = false)
		{
			string solution = CargoWise.BuildTools.BuildXml.Instance.GetFileNameOfSolution(rootName);
			List<string> assemblyList = new List<string>(assemblies);
			SetupTestEnvironment();
			string reflectionTest = GetReflectionTest(assemblies);
			if (!string.IsNullOrEmpty(reflectionTest) && !assemblyList.Contains(reflectionTest))
			{
				assemblyList.Add(reflectionTest);
			}
			if (rootName == null)
			{
				rootName = Constants.ProductName + " Unit Tests";
			}

			var form = new UnitTestForm(GetInfosForAssemblies(assemblyList, solution), rootName, true);
			form.FormClosed += (sender, e) => { TestingState.TearDown(); };
			AddTestListeners(form);
			if (showDialog)
			{
				form.ShowDialog();
			}
			else
			{
				form.Show();
			}
		}

		static string GetReflectionTest(IEnumerable<string> assemblies)
		{
			if (assemblies == null || !assemblies.Any())
			{
				return null;
			}

			var assembliesToReflectionTest = new List<string>(assemblies);
			assembliesToReflectionTest.RemoveAll(a => a.Contains("ReflectionTest"));
			AssembliesUnderTest.Assemblies = assembliesToReflectionTest.ToArray();
			return "Enterprise.ReflectionTest";
		}

		static void AddTestListeners(UnitTestForm form)
		{
			foreach (ITestListener listener in UnitTestListenersFactory.GetTestListeners())
			{
				form.AddTestListener(listener);
			}
		}

		#region Implementation

		static string[] AssemblyNamesInSolution(string solution)
		{
			string[] result = CargoWise.BuildTools.BuildXml.Instance.GetAssembliesTestedInSolution(CargoWise.BuildTools.BuildXml.Instance.GetFileNameOfSolution(solution)).ToArray();
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = Path.ChangeExtension(result[i], null);
			}

			return result;
		}
		#endregion

		public class UnitTestForTesting : TestCase
		{
			[ExpectNoExceptions]
			public void TestDisposedTreeAccess()
			{
				var assemblyList = new List<string>(AssemblyNamesInSolution("EventReference"));
#if NETFRAMEWORK
				assemblyList.RemoveAll(item => item.StartsWith("net8.0"));
#endif
				SetupTestEnvironment();

				using (var form = new UnitTestForm(assemblyList, "CargoWise One Unit Tests", new ErrorDescriptionList()))
				{
					AddTestListeners(form);
					var closingThread = new Thread(() => { Thread.Sleep(1000); form.Close(); });
					closingThread.Start();
					form.Show();
					Application.DoEvents();
				}
			}
		}
	}
}
