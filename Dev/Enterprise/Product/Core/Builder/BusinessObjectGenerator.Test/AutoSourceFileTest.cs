using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	internal class AutoSourceFileTest : TestCase
	{
		protected class ConcreteAutoSourceFileClass : AutoSourceFile
		{
			protected override string Body
			{
				get { return "Body of Text"; }
			}
		}

		public void TestIsDifferentToFile()
		{
			ConcreteAutoSourceFileClass sourceFile = new ConcreteAutoSourceFileClass();
			string tempFile = Path.Combine(TestingState.TempPath, "different.txt");
			try
			{
				File.WriteAllText(tempFile, String.Empty);
				Assert(sourceFile.IsDifferentToFile(tempFile));

				File.WriteAllText(tempFile, sourceFile.SourceCode);
				Assert(!sourceFile.IsDifferentToFile(tempFile));
			}
			finally
			{
				DeleteIfExists(tempFile);
			}
		}

		public void TestAutoSourceFile()
		{
			ConcreteAutoSourceFileClass testObject = new ConcreteAutoSourceFileClass();
			AssertNotNull("Failed to create ConcreteAutoSourceFileClass", testObject);
			AssertNotNull("Failed to get source file contents", testObject.SourceCode);
			AssertEquals("Should have a final new line.", true, testObject.SourceCode.EndsWith(System.Environment.NewLine));
		}
	}
}
