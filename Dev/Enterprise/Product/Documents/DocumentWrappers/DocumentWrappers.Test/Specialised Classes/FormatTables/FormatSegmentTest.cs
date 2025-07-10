using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.FormatTables.Testing
{
	sealed class FormatSegmentTest : TestCase
	{
		#region TestEnumerateSegments

		public void TestEnumerateSegments()
		{
			IEnumerator<FormatSegment> enumerator = FormatSegment.EnumerateSegments("1111111111 22222222 333333333 12345678901234567890\n   \r4444\r\n5555 exactly 10", 10);
			List<string> segmentTexts = new List<string>();

			while (enumerator.MoveNext())
			{
				segmentTexts.Add(enumerator.Current.SegmentText);
			}

			AssertArrayEqualsByElements(
				new string[] {
					"1111111111",
					"22222222",
					"333333333",
					"1234567890",
					"1234567890",
					"   ",
					"4444",
					"5555",
					"exactly 10"
				}, segmentTexts.ToArray());
		}

		#endregion
	}
}
