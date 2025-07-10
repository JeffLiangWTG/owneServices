using System;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniqueIdentifierMessageLineComparerTest : TestCase
	{
		public void TestCompare()
		{
			var comparer = new UniqueIdentifierMessageLineComparer();

			var line1 = new UniqueIdentifierMessageLineForTest("A");
			var line2 = new UniqueIdentifierMessageLineForTest("B");
			var line3 = new UniqueIdentifierMessageLineForTest("B");

			AssertEquals(-1, comparer.Compare(line1, line2));
			AssertEquals(1, comparer.Compare(line2, line1));

			AssertEquals(0, comparer.Compare(line1, line1));
			AssertEquals(0, comparer.Compare(line2, line3));
			AssertEquals(0, comparer.Compare(line3, line2));
		}

		sealed class UniqueIdentifierMessageLineForTest : UniqueIdentifierMessageLine
		{
			public UniqueIdentifierMessageLineForTest(string uniqueIdentifier)
			{
				this.uniqueIdentifier = uniqueIdentifier;
			}

			public override string UniqueIdentifier => uniqueIdentifier;

			public override void Populate(Edifact.Auto.SegmentGroup segmentGroup, string lineActionCode)
			{
			}

			public override Edifact.Auto.SegmentGroup GetNewSegmentGroup(Edifact.Auto.SegmentGroup message) => null;

			protected internal override Type SegmentGroupType => null;

			readonly string uniqueIdentifier;
		}
	}
}
