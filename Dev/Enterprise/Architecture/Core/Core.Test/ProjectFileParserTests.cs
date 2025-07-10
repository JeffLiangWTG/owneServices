using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ProjectFileParserTests : TestCase
	{
		string testProjectDirectory;
		string tempPath;
		const string TestContentDirectory = @"Enterprise\Architecture\Core\Core.Test\TestProjects";

		protected override void SetUp()
		{
			base.SetUp();
			tempPath = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			var baseDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName;

			if (IsDATTesting())
			{
				testProjectDirectory = Path.Combine(baseDirectory, @"Content", TestContentDirectory);
			}
			else
			{
				var projectName = $"{Assembly.GetExecutingAssembly().GetName().Name}.csproj";
				var projectPath = Directory.EnumerateFiles(baseDirectory, projectName, SearchOption.AllDirectories).FirstOrDefault();
				testProjectDirectory = Path.Combine(Path.GetDirectoryName(projectPath), "TestProjects");
			}

			CopyTestFilesMatchingPattern("*.csproj.txt", tempPath);
			CopyTestFilesMatchingPattern("*.cs.txt", tempPath);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (!string.IsNullOrWhiteSpace(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
		}

		public void TestOldStyleSDKProject()
		{
			var filePath = $@"{tempPath}\TestProjects\OldStyleSDKProject\Directory\ClassA.cs";
			AssertExpectedProjectAndAssemblyName(filePath, "OldStyleSDKProject.csproj", "OldStyleSDKProjectAssembly");
		}

		public void TestNewStyleSDKProject()
		{
			var filePath = $@"{tempPath}\TestProjects\NewStyleSDKProject\Directory\ClassA.cs";
			AssertExpectedProjectAndAssemblyName(filePath, "NewStyleSDKProject.csproj", "NewStyleSDKProject");
		}

		public void TestNewStyleSDKProjectWithAssemblyName()
		{
			var filePath = $@"{tempPath}\TestProjects\NewStyleSDKProjectWithAssemblyName\Directory\ClassA.cs";
			AssertExpectedProjectAndAssemblyName(filePath, "NewStyleSDKProjectWithAssemblyName.csproj", "AssemblyNameFound");
		}

		public void TestNewStyleSDKProjectNoAssemblyName()
		{
			var filePath = $@"{tempPath}\TestProjects\NewStyleSDKProjectNoAssemblyName\Directory\ClassA.cs";
			AssertExpectedProjectAndAssemblyName(filePath, "NewStyleSDKProjectNoAssemblyName.csproj", "NewStyleSDKProjectNoAssemblyName");
		}

		public void TestBrokenSDKProject()
		{
			var filePath = $@"{tempPath}\TestProjects\BrokenSDKProject\";
			AssertExpectedProjectAndAssemblyName(filePath, null, null);
		}

		public void TestNewStyleSDKProjectWithFileExclusions()
		{
			var filePathA = $@"{tempPath}\TestProjects\NewStyleSDKProjectWithFileExclusions\Directory\ClassA.cs";
			var filePathB = $@"{tempPath}\TestProjects\NewStyleSDKProjectWithFileExclusions\Directory\ClassB.cs";
			var filePathC = $@"{tempPath}\TestProjects\NewStyleSDKProjectWithFileExclusions\Directory\ClassC.cs";
			var filePathD = $@"{tempPath}\TestProjects\NewStyleSDKProjectWithFileExclusions\Directory\ClassD.cs";

			AssertExpectedProjectAndAssemblyName(filePathA, null, null);
			AssertExpectedProjectAndAssemblyName(filePathB, null, null);
			AssertExpectedProjectAndAssemblyName(filePathC, "NewStyleSDKProjectWithFileExclusions.csproj", "NewStyleSDKProjectWithFileExclusions");
			AssertExpectedProjectAndAssemblyName(filePathD, "NewStyleSDKProjectWithFileExclusions.csproj", "NewStyleSDKProjectWithFileExclusions");
		}

		public void TestNewStyleSDKProject_WhenProjectDirectoryHasOverlappingNameWithOtherProject()
		{
			const string projectName1 = "NewStyleSDKProjectPurpleMonkey";
			const string projectName2 = "NewStyleSDKProjectPurpleMonkeyDishwasher";
			var filePath1 = $@"{tempPath}\TestProjects\{projectName1}\Klass.cs";
			var filePath2 = $@"{tempPath}\TestProjects\{projectName2}\Klass.cs";

			var projectParser = new ProjectFileParser();
			var expectedResult1 = new KeyValuePair<string, string>(projectName1 + ".csproj", projectName1);
			var expectedResult2 = new KeyValuePair<string, string>(projectName2 + ".csproj", projectName2);

			AssertEquals(expectedResult1, projectParser.GetProjectAndAssemblyName(filePath1));
			AssertEquals(expectedResult2, projectParser.GetProjectAndAssemblyName(filePath2));
		}

		public void TestNewStyleSDKProject_MultipleItemsInProjectWithoutAssemblyName()
		{
			const string projectName = "NewStyleSDKProjectPurpleMonkey";
			var filePath1 = $@"{tempPath}\TestProjects\{projectName}\Klass1.cs";
			var filePath2 = $@"{tempPath}\TestProjects\{projectName}\Klass2.cs";

			var projectParser = new ProjectFileParser();
			var expectedResult = new KeyValuePair<string, string>(projectName + ".csproj", projectName);

			AssertEquals(expectedResult, projectParser.GetProjectAndAssemblyName(filePath1));
			AssertEquals(expectedResult, projectParser.GetProjectAndAssemblyName(filePath2));
		}

		void CopyTestFilesMatchingPattern(string pattern, string tempPath)
		{
			var allPaths = Directory.EnumerateFiles(testProjectDirectory, pattern, SearchOption.AllDirectories);
			foreach (var path in allPaths)
			{
				var localPath = path.Substring(0, path.Length - 4);
				var relativePath = GetRelativePath(localPath);
				var destination = Path.Combine(tempPath, relativePath);
				var dir = Path.GetDirectoryName(destination);

				if (!string.IsNullOrWhiteSpace(dir))
				{
					Directory.CreateDirectory(dir);
					File.Copy(path, destination);
				}
			}
		}

		static string GetRelativePath(string fullPath)
		{
			var pathArray = fullPath.Split('\\').ToList();
			var index = pathArray.IndexOf("TestProjects");
			var relativePath = string.Join(@"\", pathArray.Skip(index));

			return relativePath;
		}

		static bool IsDATTesting()
		{
			var success = bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting);

			return success && datIsTesting;
		}

		static void AssertExpectedProjectAndAssemblyName(string filePath, string expectedProjectName, string expectedAssemblyName)
		{
			var parser = new ProjectFileParser();
			var kvp = parser.GetProjectAndAssemblyName(filePath);

			AssertEquals(expectedProjectName, kvp.Key);
			AssertEquals(expectedAssemblyName, kvp.Value);
		}
	}
}
