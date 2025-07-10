using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DataSaveWithColumnsTest : TestCase
	{
		public void TestDataExportedRegardlessOfHeaderOrder()
		{
			List<String> output = new List<String>(10);
			using (TempFile testFileName = TempFile.New())
			{
				DataSaveWithColumnsTestForTesting dataLoad = new DataSaveWithColumnsTestForTesting();
				dataLoad.ExportData(testFileName.Filename, "TestingType");
				using (StreamReader sr = new StreamReader(testFileName.Filename))
				{
					AssertHeadingLine(sr.ReadLine());
				}
			}
		}

		void AssertHeadingLine(string actualHeadingLine)
		{
			AssertEquals("Heading line", "\"Column 1\",\"Column 2\",\"Column 3\"", actualHeadingLine);
		}
	}
}
