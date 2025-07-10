using NUnit.Framework;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class CellWithFormattingTest : TestCase
	{
		public void TestConstructors()
		{
			var cell = new CellWithFormatting();

			AssertNotNull("HtmlAttributes", cell.HtmlAttributes);
			AssertEquals("CellValue", string.Empty, cell.CellValue);

			cell = new CellWithFormatting("TestValue", true);
			AssertNotNull("HtmlAttributes", cell.HtmlAttributes);
			AssertEquals("Right number of attributes", 0, cell.HtmlAttributes.Count);
			AssertEquals("CellValue", "TestValue", cell.CellValue);
			AssertEquals("IsTitle", true, cell.IsTitle);

			cell = new CellWithFormatting("TestValue", "att1", "attval1");
			AssertNotNull("HtmlAttributes", cell.HtmlAttributes);
			AssertEquals("CellValue", "TestValue", cell.CellValue);
			AssertEquals("IsTitle", false, cell.IsTitle);
			AssertEquals("Right number of attributes", 1, cell.HtmlAttributes.Count);
			AssertEquals("Att1 name", "att1", cell.HtmlAttributes.Keys[0]);
			AssertEquals("Att1 value", "attval1", cell.HtmlAttributes[0]);
		}
	}
}
