using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MHeadingMessageDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "BGMMSG    Sender          Secondary";
			IFTMIN5MHeadingMessageDataRow dataRow = new IFTMIN5MHeadingMessageDataRow(testString);
			AssertEquals("MSG", dataRow.MessageID);
			AssertEquals("Sender", dataRow.SenderID);
			AssertEquals("Secondary", dataRow.SecondarySenderID);
		}
	}
}
