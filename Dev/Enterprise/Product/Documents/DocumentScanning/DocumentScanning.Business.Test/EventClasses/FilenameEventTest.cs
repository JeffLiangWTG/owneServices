using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FilenameEventTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			FilenameEventArgs args = new FilenameEventArgs("abc");
			AssertEquals("Filename should be set to what was passed in", "abc", args.Filename);

			args = new FilenameEventArgs("def");
			AssertEquals("Filename should be set to what was passed in", "def", args.Filename);
		}

		public void TestAction()
		{
			FilenameEventArgs args = new FilenameEventArgs("abc");
			AssertEquals("FileAction none by default", args.Action, FileAction.None);

			args.Action = FileAction.CreateNew;
			AssertEquals("FileAction should now be CreateNew", args.Action, FileAction.CreateNew);
		}
	}
}
