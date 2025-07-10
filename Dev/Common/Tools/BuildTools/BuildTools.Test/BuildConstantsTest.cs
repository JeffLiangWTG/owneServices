using System;
using System.IO;
using CargoWise.Shared;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class BuildConstantsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetClientDocumentXmlPath()
		{
			AssertEquals("GetClientDocumentXmlPath(\"ABC\")", Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\ABC\Documents\ABCDocuments.xml"), BuildConstants.GetClientDocumentXmlPath("ABC"));
			AssertEquals("GetClientDocumentXmlPath(\"XYZ\")", Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\XYZ\Documents\XYZDocuments.xml"), BuildConstants.GetClientDocumentXmlPath("XYZ"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetClientDocumentXmlPaths()
		{
			var xmlPaths = BuildConstants.GetClientDocumentXmlPaths();
			AssertCollectionContains("Should contain EDIDocuments.xml.", Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\EDI\Documents\EDIDocuments.xml"), xmlPaths);
			AssertCollectionContains("Should contain WOWDocuments.xml.", Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\WOW\Documents\WOWDocuments.xml"), xmlPaths);
		}

		public void TestGetLocalEnterprisePath()
		{
			var tempDirectory = Path.Combine(TestingState.TempPath, Guid.NewGuid().ToString());
			var tempSubDirectory = Path.Combine(tempDirectory, "Sub");

			Directory.CreateDirectory(tempSubDirectory);

			try
			{
				using (File.Create(Path.Combine(tempDirectory, BuildConstants.FileExpectedAtRootSourceTree)))
				{
				}

				AssertEquals("GetLocalEnterprisePath()", tempDirectory + "\\", BuildConstants.GetLocalEnterprisePath(Path.Combine(tempDirectory, "File.cs")));
				AssertEquals("GetLocalEnterprisePath()", tempDirectory + "\\", BuildConstants.GetLocalEnterprisePath(Path.Combine(tempSubDirectory, "File.cs")));
			}
			finally
			{
				FileIO.DeleteDirectory(tempDirectory);
			}
		}

		[ExpectExceptionMessage(typeof(BaseSourcePathNotFoundException), @"Could not determine the local source path using the filename '\IDoNotExist\File.cs'.")]
		public void TestGetInvalidLocalEnterprisePath_WhenThrowOnErrorTrue()
		{
			BuildConstants.GetLocalEnterprisePath("\\IDoNotExist\\File.cs");
		}

		public void TestGetInvalidLocalEnterprisePath_WhenThrowOnErrorFalse()
		{
			AssertEquals(null, BuildConstants.GetLocalEnterprisePath("\\IDoNotExist\\File.cs", false));
		}

		public void TestGetLocalPath()
		{
			AssertEquals("GetLocalPath()", Path.Combine(BaseSourcePath, "Dir\\File.cs"), BuildConstants.GetLocalPath("Dir\\File.cs"));
		}

		public void TestGetServerPath()
		{
			AssertEquals("GetServerPath()", BuildConstants.ServerEnterprisePath + "/x", BuildConstants.GetServerPath("x"));
			AssertEquals("GetServerPath()", BuildConstants.ServerEnterprisePath + "/x", BuildConstants.GetServerPath("/x"));
		}

		public void TestLocalEnterprisePath()
		{
			AssertEquals("LocalEnterprisePath", BaseSourcePath, BuildConstants.LocalEnterprisePath);
		}

		public void TestServerEnterprisePath()
		{
			if (TestingState.IsRunningOnDAT)
			{
				AssertEquals("ServerEnterprisePath", "$/Dummy", BuildConstants.ServerEnterprisePath);
			}
			else
			{
				Assert(!String.IsNullOrEmpty(BuildConstants.ServerEnterprisePath));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileExpectedAtRootSourceTreeExists()
		{
			var filePath = Path.Combine(BaseSourcePath, BuildConstants.FileExpectedAtRootSourceTree);
			AssertEquals("File.Exists(" + filePath + ")", true, File.Exists(filePath));
		}
	}
}
