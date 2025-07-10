using System.Linq;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class SegmentationTest : TestCase
	{
		public void TestSegmentation()
		{
			AssertArrayEqualsByElements(new string[] { "Today is Monday.", "Tomorrow is Tuesday." }, Segmentation.Segment("Today is Monday.\r\nTomorrow is Tuesday.").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Today is Monday.", "Tomorrow is Tuesday." }, Segmentation.Segment("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\n").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Today is Monday.", "Tomorrow is Tuesday." }, Segmentation.Segment("Today is Monday. Tomorrow is Tuesday.").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Today is Monday.", "Tomorrow is Tuesday." }, Segmentation.Segment("	Today is Monday.		Tomorrow is Tuesday. ").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Today is Monday", "Tomorrow is Tuesday" }, Segmentation.Segment("Today is Monday\r\nTomorrow is Tuesday").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "What day is it today?", "Monday" }, Segmentation.Segment("What day is it today? Monday").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Today is Monday, tomorrow is Tuesday." }, Segmentation.Segment("Today is Monday, tomorrow is Tuesday.").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Just dropped in to see what cond. my cond. was in" }, Segmentation.Segment("Just dropped in to see what cond. my cond. was in").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "One second.", "Almost done" }, Segmentation.Segment("One second. Almost done").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "How's the cond?", "Great" }, Segmentation.Segment("How's the cond? Great").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Second.:" }, Segmentation.Segment("Second.:").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "The day (e.g. Monday)" }, Segmentation.Segment("The day (e.g. Monday)").Select(s => s.Text).ToArray());
			AssertArrayEqualsByElements(new string[] { "Information recorded in test systems is not retained long term.", "It is advised you only raise eRequests from your production system." }, Segmentation.Segment("Information recorded in test systems is not retained long term. It is advised you only raise eRequests from your production system.").Select(s => s.Text).ToArray());
		}

		public void TestSegmentStartIndex()
		{
			AssertSegmentStartIndex("Today is Monday.\r\nTomorrow is Tuesday.");
			AssertSegmentStartIndex("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\n");
			AssertSegmentStartIndex("Today is Monday. Tomorrow is Tuesday.");
			AssertSegmentStartIndex("Today is Monday\r\nTomorrow is Tuesday");
			AssertSegmentStartIndex("	Today is Monday.		Tomorrow is Tuesday. ");
			AssertSegmentStartIndex("What day is it today? Monday");
		}

		void AssertSegmentStartIndex(string paragraph)
		{
			foreach (var segment in Segmentation.Segment(paragraph))
			{
				AssertEquals(segment.Text, paragraph.Substring(segment.StartIndex, segment.Text.Length));
			}
		}

		public void TestReplaceSegment()
		{
			AssertEquals("Yesterday was Sunday.\r\nTomorrow is Tuesday.", Segmentation.ReplaceSegment("Today is Monday.\r\nTomorrow is Tuesday.", "Today is Monday.", "Yesterday was Sunday."));
			AssertEquals("Today is Monday.\r\nThe day after tomorrow is Wednesday.", Segmentation.ReplaceSegment("Today is Monday.\r\nTomorrow is Tuesday.", "Tomorrow is Tuesday.", "The day after tomorrow is Wednesday."));
			AssertEquals("Today is Monday.\r\n\r\nLet's work.\r\n", Segmentation.ReplaceSegment("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\n", "Tomorrow is Tuesday.", "Let's work."));
			AssertEquals("Let's work.\r\n\r\nTomorrow is Tuesday.\r\nLet's work.", Segmentation.ReplaceSegment("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\nToday is Monday.", "Today is Monday.", "Let's work."));
			AssertEquals("Yesterday was Sunday.Let's work.\r\n\r\nTomorrow is Tuesday.\r\nYesterday was Sunday.Let's work.", Segmentation.ReplaceSegment("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\nToday is Monday.", "Today is Monday.", "Yesterday was Sunday.Let's work."));
			AssertEquals("\r\n\r\nTomorrow is Tuesday.\r\n", Segmentation.ReplaceSegment("Today is Monday.\r\n\r\nTomorrow is Tuesday.\r\nToday is Monday.", "Today is Monday.", null));
		}
	}
}
