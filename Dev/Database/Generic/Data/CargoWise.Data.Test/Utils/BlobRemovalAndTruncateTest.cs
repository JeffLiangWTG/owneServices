using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class BlobRemovalAndTruncateTest : TestCase
	{
		public void TestGetTextWithoutBlobs()
		{
			string testText = string.Format(TestFormat, "0x5814CBD636DA431CF20092", "0x505AECBD0758");
			string expectedTrimmedText = string.Format(TestFormat, "<System.Byte[11]>", "<System.Byte[6]>");
			AssertEquals("BLOBs should have been replaced by <System.Byte[x]>",
				expectedTrimmedText, GetTextWithoutBlobs(testText));

			string testTextWithNoBlobs = string.Format(TestFormat, "92", "758");
			AssertEquals("No BLOBs to be replaced", testTextWithNoBlobs, GetTextWithoutBlobs(testTextWithNoBlobs));
		}

		public void TestGetTextWithoutComments()
		{
			string testText = @"SELECT * FROM TestTable\r\n/* this is comment here:\r\n12<>^%#@()*_-=`^%$\r\n*/";
			AssertEquals(@"SELECT * FROM TestTable\r\n", CommandTextTrimmer.GetTextWithoutMultiLineComments(testText).Trim());
		}

		#region TextFormat

		protected string TestFormat =
			@"
DECLARE @ShouldStop char(1); SET @ShouldStop = 'N'

SET @CurrentPK = '8ffaf45c-b926-4619-a0c2-ccc7b1e35b4e'
exec(N'
INSERT TestTable
( 
TT_PK, 
TT_Col1, 
TT_Col2 
)
VALUES 
(
''8ffaf45c-b926-4619-a0c2-ccc7b1e35b4e'', 
1, 
{0}
)
')

SET @CurrentPK = '8ffaf45c-b926-4619-a0c2-ccc7b1e35b4e'
exec sp_executesql N'
UPDATE TestTable
SET 
TT_Col2 = @D_TT_Col2
WHERE 
TT_PK = @W_TT_PK
AND TT_Col1 = @W_TT_Col1
', N'
@D_TT_Col2 Image, 
@W_TT_PK UniqueIdentifier, 
@W_TT_Col1 Int', 
@D_TT_Col2 = {1}, 
@W_SP_PK = '8ffaf45c-b926-4619-a0c2-ccc7b1e35b4e', 
@W_TT_Col1 = 1";

		#endregion

		protected virtual string GetTextWithoutBlobs(string text)
		{
			return CommandTextTrimmer.GetTextWithoutBlobs(text);
		}

		protected CommandTextTrimmer CommandTextTrimmer
		{
			get
			{
				if (fCommandTextTrimmer == null)
				{
					fCommandTextTrimmer = new CommandTextTrimmer(100000);
				}
				return fCommandTextTrimmer;
			}
		}
		CommandTextTrimmer fCommandTextTrimmer;

		public void TestGetTruncatedText()
		{
			string testText = "SELECT * FROM TestTable";
			string hugeText = testText.PadRight(testText.Length + CommandTextTrimmer.maxCommandTextLength, '*');

			AssertEquals("Huge command length before truncation",
				testText.Length + CommandTextTrimmer.maxCommandTextLength, hugeText.Length);
			AssertEquals("Command smaller then max length should NOT have been truncated",
				testText, GetTruncatedText(testText));

			string truncatedCommand = GetTruncatedText(hugeText);
			AssertEquals("Huge command should have been truncated to max length",
				testText.PadRight(CommandTextTrimmer.maxCommandTextLength, '*'), truncatedCommand);
			AssertEquals("Truncated command length",
				CommandTextTrimmer.maxCommandTextLength, truncatedCommand.Length);
		}

		protected virtual string GetTruncatedText(string text)
		{
			return CommandTextTrimmer.GetTruncatedText(text);
		}
	}
}
