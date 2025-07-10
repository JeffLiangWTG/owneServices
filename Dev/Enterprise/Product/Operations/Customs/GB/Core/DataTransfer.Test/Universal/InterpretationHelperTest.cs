using System.Xml;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class InterpretationHelperTest : TestCase
	{
		public void TestRemoveAllNamespaces()
		{
			const string testXml = "<err:ErrorResult xmlns:err=\"x\" xmlns:erx=\"y\"><erx:Error></erx:Error></err:ErrorResult>";
			var removed1 = InterpretationHelper.RemoveAllNamespaces(testXml);
			CombineAssertions("Valid remove, don't care about the formatting", () =>
			{
				AssertContains("<ErrorResult>", removed1);
				AssertContains("<Error></Error>", removed1);
				AssertContains("</ErrorResult>", removed1);
			});

			const string notXml = "This is not xml.";
			var removed2 = InterpretationHelper.RemoveAllNamespaces(notXml);

			AssertEquals("Attempt to remove namespaces from invalid xml should just return the input string", notXml, removed2);

			var removed3 = InterpretationHelper.RemoveAllNamespaces(null);
			AssertEquals("Attempt to remove namespaces from empty xml should return empty string", ZString.Empty, removed3);
		}

		public void TestGetHtmlTableCreator()
		{
			CombineAssertions(() =>
			{
				var tableCreator = InterpretationHelper.GetHtmlTableCreator();
				tableCreator.WriteRow(new CellWithFormatting("<i>html tags are not escaped</i>"));
				AssertEquals("Default", "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><i>html tags are not escaped</i></td></tr></table>", tableCreator.ToHtml());

				tableCreator = InterpretationHelper.GetHtmlTableCreator("50%");
				AssertEquals("50%", "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"50%\" class=\"table\"></table>", tableCreator.ToHtml());
			});
		}

		public void TestCreateTableOfErrorsFromNodeList()
		{
			var builder = new ZStringBuilder();
			var columns = new[]
			{
				("Column 1", "Path1"),
				("Column B", "Path2"),
				("Column 3", "AnotherPath"),
			};

			var document = new XmlDocument();
			document.LoadXml("<Root><Error><Path1>1</Path1><Path2>2</Path2><AnotherPath>3</AnotherPath></Error><Error><Path1>4</Path1><Path2>5</Path2><AnotherPath>6</AnotherPath></Error></Root>");
			var errors = document.SelectNodes("//Root/Error");

			InterpretationHelper.CreateTableOfErrorsFromNodeList(builder, errors, columns);

			var result = builder.ToString();

			CombineAssertions(() =>
			{
				AssertContains("First error", "<tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Column 1</td><td>1</td></tr><tr><td>Column B</td><td>2</td></tr><tr><td>Column 3</td><td>3</td></tr>", result);
				AssertContains("Second error", "<tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Column 1</td><td>4</td></tr><tr><td>Column B</td><td>5</td></tr><tr><td>Column 3</td><td>6</td></tr>", result);
			});
		}
	}
}
