using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class TestingMakeFilenameSafe : TestCase
	{
		public void TestSafeMakerTest()
		{
			string dodgyInput = @"crap:file/name""with<Dodgy>Characters(and)*good#chars_too.<txt";
			string safeFileName = MakeFilenameSafe.MakeSafe(dodgyInput);
			AssertEquals("crap file name with Dodgy Characters(and) good#chars_too. txt", safeFileName);
			safeFileName = MakeFilenameSafe.MakeSafe(dodgyInput, 'x');
			AssertEquals("crapxfilexnamexwithxDodgyxCharacters(and)xgood#chars_too.xtxt", safeFileName);
			safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, ' ');
			AssertEquals("crap file name with Dodgy Characters(and) good#chars_too.txt", safeFileName);
		}

		public void TestSafeMakerHandlesShortStringsOk()
		{
			{
				string dodgyInput = @".*txt";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals(".txt", safeFileName);
			}
			{
				string dodgyInput = @"*txt";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals("_txt", safeFileName);
			}
			{
				string dodgyInput = @"..*txt";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals("..txt", safeFileName);
			}
			{
				string dodgyInput = @".";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals(".", safeFileName);
			}
			{
				string dodgyInput = @"txt*.";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals("txt_.", safeFileName);
			}
			{
				string dodgyInput = @"";
				string safeFileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(dodgyInput, '_');
				AssertEquals("", safeFileName);
			}
		}

		public void TestMakeSafe_HandlesSpaces()
		{
			string[,] tests = new string[,] {
				{ "Nothing to remove. String should remain the same", "blah.txt", "blah.txt" },
				{ "Should remove space at start of file name", " blah.txt", "blah.txt" },
				{ "Should remove space at start of file name", "blah.txt ", "blah.txt" },
				{ "Should ignore spaces within name", "The gray cat.txt", "The gray cat.txt" },
				{ "Should remove space before filetype", "The gray cat .txt", "The gray cat.txt" },
				{ "Should remove space before filetype", ":The gray cat:.txt:", "The gray cat.txt" },
			};

			CombineAssertions(() =>
			{
				for (var i = 0; i < tests.GetLength(0); i++)
				{
					AssertEquals(tests[i, 0], tests[i, 2], MakeFilenameSafe.MakeSafe(tests[i, 1], ' '));
				}
			});
		}

		public void TestIsSafe()
		{
			string dodgyInput = @"crap:file/name""with<Dodgy>Characters(and)*good#chars_too.txt";
			string dodgyInput2 = @":file.txt";
			string safeFileName = MakeFilenameSafe.MakeSafe(dodgyInput, '_');
			Assert(@"crap:file/name""with<Dodgy>Characters(and)*good#chars_too.txt", !MakeFilenameSafe.IsSafe(dodgyInput));
			Assert(@":file.txt", !MakeFilenameSafe.IsSafe(dodgyInput2));
			Assert(@"crap_file_name__with_Dodgy_Characters(and)_good#chars_too.txt", MakeFilenameSafe.IsSafe(safeFileName));
		}
	}
}
