using System.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	public class LargeMessageHelperTest : TestCase
	{
		public void TestGetTruncatedInterchangeText()
		{
			string header = "This is a header";
			string footer = "This is a footer";

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Header truncated", "This is a", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 9, null));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Body truncated", "This is a headerThis ", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 21, null));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Footer truncated", "This is a headerThis is a bodyThis", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 34, null));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Not truncated", "This is a headerThis is a bodyThis is a footer", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 1000, null));
			}
		}

		public void TestGetTruncatedInterchangeTextWithTextPadder()
		{
			string header = "This is a header";
			string footer = "This is a footer";

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Header truncated", "HThis is ", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 9, TextPadder));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Body truncated", "HThis is a headerHBTh", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 21, TextPadder));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Footer truncated", "HThis is a headerHBThis is a bodyBFThis ", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 40, TextPadder));
			}

			using (var reader = new StringReader("This is a body"))
			{
				AssertEquals("Not truncated", "HThis is a headerHBThis is a bodyBFThis is a footerF", LargeMessageHelper.GetTruncatedInterchangeText(header, reader, footer, 1000, TextPadder));
			}
		}

		ZString TextPadder(ZString text, LargeMessageHelper.TextPadderDataType dataType)
		{
			ZString result;
			switch (dataType)
			{
				case LargeMessageHelper.TextPadderDataType.Header:
					result = "H" + text + "H";
					break;
				case LargeMessageHelper.TextPadderDataType.Footer:
					result = "F" + text + "F";
					break;
				default:
					result = "B" + text + "B";
					break;
			}
			return result;
		}
	}
}
