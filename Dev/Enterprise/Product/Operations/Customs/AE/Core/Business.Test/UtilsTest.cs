using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class UtilsTest : TestCaseWithFactory
{
	public void TestGetFormattedEDIFactText()
	{
		CombineAssertions(() =>
		{
			var testString = "  ABC123'456-XYZ'LMN|789'  ";
			var expectedString = @"ABC123'
456-XYZ'
LMN|789'";
			AssertEquals("Test string with Segment delimiter", expectedString, Utils.GetFormattedEDIFactText(testString));

			testString = "ABC123456-XYZLMN|789";
			AssertEquals("Test string without Segment delimiter", testString, Utils.GetFormattedEDIFactText(testString));
		});
	}

	public void TestSplitString()
	{
		var testString = new string('A', 11);
		var chunkSize = 5;
		var chunks = testString.SplitString(chunkSize).ToList();
		var count = chunks.Count;
		CombineAssertions(() =>
		{
			AssertEquals("Count of chunks", 3, count);
			AssertEquals("String is split by chunk size", expected: true, chunks.Take(count - 1).All(x => x.Length == chunkSize));
			AssertEquals("Last chunk", "A", chunks.Last());
		});
	}

	public void TestSplitSegment()
	{
		var segmentDelimiter = AECharacterSet.New().SegmentDelimiter;
		var messageText = $"UNB+UNOA:1+SENDER+RECEIVER+{segmentDelimiter}UNH+1+IFTMIN:D:95B:UN:2.1A{segmentDelimiter}UNT+3+1{segmentDelimiter}";
		var characterSet = AECharacterSet.New();
		var segments = Utils.SplitSegment(messageText, characterSet).ToList();
		var count = segments.Count;
		CombineAssertions(() =>
		{
			AssertEquals("Count of segments", 3, count);
			AssertEquals("First segment", "UNB+UNOA:1+SENDER+RECEIVER+'", segments[0]);
			AssertEquals("Second segment", "UNH+1+IFTMIN:D:95B:UN:2.1A'", segments[1]);
			AssertEquals("Third segment", "UNT+3+1'", segments[2]);
		});
	}

	public void TestRetrieveRFFSegment()
	{
		var message = @"UNB+UNOA:1+SENDER+RECEIVER+'
UNH+1+IFTMIN:D:95B:UN:2.1A'
RFF+DM:1234567890::1'";
		var segment = Utils.RetrieveRFFSegment(message);
		AssertEquals("1234567890", segment.Reference.ReferenceIdentifier);
	}

	public void TestRetrieveGEISegment()
	{
		var message = @"UNB+UNOA:1+SENDER+RECEIVER+'
GEI++RFI'
UNH+1+IFTMIN:D:95B:UN:2.1A'
RFF+DM:1234567890::1'";
		var segment = Utils.RetrieveGEISegment(message);
		AssertEquals("RFI", segment.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
	}
}
