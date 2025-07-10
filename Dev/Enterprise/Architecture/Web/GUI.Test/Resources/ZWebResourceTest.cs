using System;
using System.IO;
using System.Reflection;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZWebResourceTest : TestCase
	{
		#region TestFileName

		public void TestFileName()
		{
			ZWebResource resource = new ZWebResource(GetType(), "TestFile1.htm", null);
			string assemblyPath = String.Format("/Runtime/Enterprise.ZArchitecture.Web.GUI_Test/{0}/ZWebResourceTest/", resource.AssemblyVersion).Replace(".", "_");
			AssertEquals("Filename does not match. Should use normal slashes", assemblyPath + "TestFile1.htm", resource.FileName);

			resource = new ZWebResource(GetType(), "TestFile1.htm", null, "AlternativeLocation");
			AssertEquals("Test location doesn't depend on alternative location", assemblyPath + "TestFile1.htm", resource.FileName);

			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assembly = Assembly.LoadFile(Path.Combine(binPath, "Enterprise.ZArchitecture.Business.dll"));

			string zArchitectureAssemblyPath = String.Format(@"/Runtime/Enterprise.ZArchitecture.Business/{0}/ZWebResourceTest/", resource.AssemblyVersion).Replace(".", "_");
			resource = new ZWebResource(GetType(), "TestFile1.htm", null, "AlternativeLocation", assembly);
			AssertEquals("Assembly affects the file name", zArchitectureAssemblyPath + "TestFile1.htm", resource.FileName);
		}
		#endregion

		#region TestExtractToFile_ThreadSafe

		[ExpectNoExceptions]
		public void TestExtractToFile_ThreadSafe()
		{
			using (TempDirectory temp = new TempDirectory())
			{
				TestResource.ServerMappedPathForTesting = temp.DirectoryName;
				ThreadSafeAccessTestCase.RunTestOnMultipleThreads(delegate { TestResource.Extract(); }, 15, ThreadSafeAccessTestCase.EndThreadTestAction.Join);
			}
		}
		#endregion

		#region TestExtractToFile

		public void TestExtractToFile()
		{
			using (TempDirectory temp = new TempDirectory())
			{
				TestResource.ServerMappedPathForTesting = temp.DirectoryName;
				string expectedFilePath = GetExpectedFilePath(temp.DirectoryName, TestResource);
				MultipleExtractInternal(expectedFilePath);
			}
		}

		[ExpectNoExceptions]
		public void TestExtractToFile_LongPath()
		{
			using (TempDirectory temp = new TempDirectory())
			{
				var longPath = temp.DirectoryName;
				while (longPath.Length < 260)
				{
					longPath = Path.Combine(longPath, "1234567890");
				}

				TestResource.ServerMappedPathForTesting = longPath;
				string expectedFilePath = @"\\?\" + GetExpectedFilePath(longPath, TestResource);
				MultipleExtractInternal(expectedFilePath);
				Directory.Delete(@"\\?\" + temp.DirectoryName, true);
			}
		}

		#endregion

		ZString GetExpectedFilePath(string tempPath, ZWebResource resource)
		{
			string assemblyPath = String.Format(@"Runtime\Enterprise.ZArchitecture.Web.GUI_Test\{0}\ZWebResourceTest", resource.AssemblyVersion).Replace(".", "_");
			return Path.Combine(Path.Combine(tempPath, assemblyPath), resource.fName);
		}

		#region TestExtractUnknownResource

		[ExpectException(typeof(ApplicationException))]
		public void TestExtractUnknownResource()
		{
			ZWebResource notExists = new ZWebResource(GetType(), "NotExist", null);
			string newPath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());
			notExists.Extract();
		}
		#endregion

		#region Assertion Methods

		void MultipleExtractInternal(string expectedFilePath)
		{
			TestResource.OnExtract += new ZWebResourceExtractEventHandler(TestResource_OnExtract);
			AssertEquals("event not yet fired", false, IsEventFired);
			TestResource.Extract();
			AssertEquals("File should exist", true, File.Exists(expectedFilePath));
			AssertEquals("Event is fired", true, IsEventFired);
			IsEventFired = false;
			TestResource.Extract();
			AssertEquals("event won't be fired, because file already extracted and exists on disk", false, IsEventFired);
		}

		public void SingleExtractInternal()
		{
			using (TempDirectory temp = new TempDirectory())
			{
				TestResource.ServerMappedPathForTesting = temp.DirectoryName;
				string expectedFilePath = GetExpectedFilePath(temp.DirectoryName, TestResource);
				TestResource.OnExtract += new ZWebResourceExtractEventHandler(TestResource_OnExtract);
				AssertEquals("Event should not have been fired", false, IsEventFired);
				AssertEquals("File should npt exist", false, File.Exists(expectedFilePath));
				TestResource.Extract();
				AssertEquals("File should exist", true, File.Exists(expectedFilePath));
				AssertEquals("Event is fired", true, IsEventFired);
			}
		}

		#endregion

		#region Implementation

		protected virtual ZWebResource GetNewResource()
		{
			return new ZWebResource(GetType(), "TestFile1.htm", null, "Enterprise.ZArchitecture.Web.GUI.Test.Resources.Test");
		}

		ZWebResource TestResource
		{
			get
			{
				if (fTestResource == null)
				{
					fTestResource = GetNewResource();
				}
				return fTestResource;
			}
		}
		ZWebResource fTestResource;

		#region OnExtract EventHandler
		bool IsEventFired;
		void TestResource_OnExtract(object sender, ZWebResourceExtractEventArgs e)
		{
			IsEventFired = true;
		}
		#endregion

		#region DeleteDirRecursively

		void DeleteDirRecursively(string directoryToClean)
		{
			if (Directory.Exists(directoryToClean))
			{
				string[] directories = Directory.GetDirectories(directoryToClean);
				foreach (string dir in directories)
				{
					DeleteDirRecursively(dir);
				}
				string[] files = Directory.GetFiles(directoryToClean);

				foreach (string filename in files)
				{
					FileAttributes attrib = File.GetAttributes(filename);
					if ((attrib & FileAttributes.ReadOnly) != 0)
					{
						attrib -= FileAttributes.ReadOnly;
						File.SetAttributes(filename, attrib);
					}
					File.Delete(filename);
				}
				Directory.Delete(directoryToClean);
			}
		}

		#endregion

		#endregion
	}
}
