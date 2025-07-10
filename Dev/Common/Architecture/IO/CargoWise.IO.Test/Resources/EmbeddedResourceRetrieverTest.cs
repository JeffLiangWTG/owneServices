using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	sealed class EmbeddedResourceRetrieverTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EmbeddedResourceRetriever(null));
		}

		public void TestGetBytes()
		{
			AssertEquals(
				new byte[] { 66, 82, 69, 84, 84 },
				new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("CargoWise.IO.Test.Resources.Testing.Test.resources"));
		}

		public void TestGetString()
		{
			AssertEquals(
				"BRETT",
				new EmbeddedResourceRetriever(GetType().Assembly).GetString("CargoWise.IO.Test.Resources.Testing.Test.resources"));
		}

		public void TestGetString_WithEncoding()
		{
			AssertEquals(
				"BRETT",
				new EmbeddedResourceRetriever(GetType().Assembly).GetString("CargoWise.IO.Test.Resources.Testing.Test.resources", Encoding.ASCII));
		}

		public void TestGetString_NullResourceName()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EmbeddedResourceRetriever(GetType().Assembly).GetString(null, Encoding.ASCII));
		}

		public void TestGetString_InvalidResourceName()
		{
			AssertExceptionThrown<IOException>("Invalid Resource",
				@"Missing resource [CargoWise.IO.Test.Resources.Testing.Test.Invalid] in [CargoWise.IO.Test]
First 2 resource name/s from this assembly:
CargoWise.IO.Test.Resources.Testing.Test.resources
CargoWise.IO.Test.Resources.Testing.Test2.resources",
				() => new EmbeddedResourceRetriever(GetType().Assembly).GetString("CargoWise.IO.Test.Resources.Testing.Test.Invalid"));
		}

		public void TestSaveResourceToFile()
		{
			using (var retriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var filePath = retriever.SaveResourceToFile("CargoWise.IO.Test.Resources.Testing.Test.resources");
				AssertEquals("BRETT", File.ReadAllText(filePath));
			}
		}

		public void TestSaveResourceToFile_WhenSubDirectorySupplied()
		{
			using (var retriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var resourceName = "CargoWise.IO.Test.Resources.Testing.Test.resources";
				var filePath = retriever.SaveResourceToFile(resourceName, resourceName, "MYSUBDIR");
				AssertEquals("BRETT", File.ReadAllText(filePath));
				AssertEndsWith("Path should include sub directory", $"MYSUBDIR\\{resourceName}", filePath);
			}
		}

		public void TestSaveAllResourcesToFiles()
		{
			using (var retriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var tmpDirectory = retriever.SaveAllResourcesToFiles();
				var files = Directory.GetFiles(tmpDirectory);
				AssertEquals(2, files.Length);
				AssertContainsExactElementsInExactOrder(
					new string[] { "CargoWise.IO.Test.Resources.Testing.Test.resources", "CargoWise.IO.Test.Resources.Testing.Test2.resources" },
					files.Select(file => Path.GetFileName(file)));
				AssertEquals("BRETT", File.ReadAllText(files[0]));
				AssertEquals("BRETT2", File.ReadAllText(files[1]));
			}
		}
	}
}
