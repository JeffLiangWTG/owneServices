using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniqueIdentifierMessageLinePopulatorTest : TestCaseWithFactory
	{
		public void TestOriginal()
		{
			AssertEquals("Message", "CNI+A1+:::I'CNI+B2+:::I'CNI+C3+:::I'", GetMessage(OriginalLines, null));
		}

		public void TestChangeNonUniqueIdentifierLineValue()
		{
			AssertEquals("Message", "CNI+B2+:::D'CNI+B4+:::I'", GetMessage(LinesWithNonUniquePartChanged, OriginalLines));
		}

		public void TestChangeNonUniqueIdentifierLineValueWhenAmendAllowed()
		{
			AssertEquals("Message", "CNI+B4+:::A'", GetMessage(LinesWithNonUniquePartChanged, OriginalLines, true));
		}

		public void TestChangeUniqueIdentifierLineValue()
		{
			AssertEquals("Message", "CNI+B2+:::D'CNI+D4+:::I'", GetMessage(LinesWithUniquePartChanged, OriginalLines));
		}

		public void TestRemoveLine()
		{
			AssertEquals("Message", "CNI+B2+:::D'", GetMessage(LinesWithLineRemoved, OriginalLines));
		}

		public void TestInsertLine()
		{
			AssertEquals("Message", "CNI+D4+:::I'", GetMessage(LinesWithLineAdded, OriginalLines));
		}

		public void TestLineOrderMakesNoDifference()
		{
			AssertEquals("Message", "CNI+A1+:::I'CNI+B2+:::I'CNI+C3+:::I'", GetMessage(OriginalLinesInReverse, null));
		}

		[ExpectNoExceptions()]
		public void TestPopulateDoesNotBlowUp()
		{
			var message = new CUSCARMessage();
			var populator = new UniqueIdentifierMessageLinePopulator();
			populator.Populate(message, LinesWithDuplicates);

			populator = new UniqueIdentifierMessageLinePopulator();
			populator.Populate(message, LinesWithLineAdded, LinesWithDuplicates);
		}

		string GetMessage(UniqueIdentifierMessageLine[] newLines, UniqueIdentifierMessageLine[] originalLines) => GetMessage(newLines, originalLines, false);

		string GetMessage(UniqueIdentifierMessageLine[] newLines, UniqueIdentifierMessageLine[] originalLines, bool allowAmendment)
		{
			var populator = new UniqueIdentifierMessageLinePopulator();
			var cuscar = new CUSCARMessage();
			if (originalLines == null)
			{
				populator.Populate(cuscar, newLines);
			}
			else
			{
				populator.Populate(cuscar, newLines, originalLines, allowAmendment);
			}
			return cuscar.ToString(new Edifact.UNOCCMRCharacterSet());
		}

		UniqueIdentifierMessageLine[] OriginalLines
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("B", "B2"),
					new UniqueIdentifierMessageLineForTest("C", "C3")
				};
			}
		}

		UniqueIdentifierMessageLine[] OriginalLinesInReverse
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("C", "C3"),
					new UniqueIdentifierMessageLineForTest("B", "B2"),
					new UniqueIdentifierMessageLineForTest("A", "A1")
				};
			}
		}

		UniqueIdentifierMessageLine[] LinesWithLineRemoved
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("C", "C3")
				};
			}
		}

		UniqueIdentifierMessageLine[] LinesWithLineAdded
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("B", "B2"),
					new UniqueIdentifierMessageLineForTest("D", "D4"),
					new UniqueIdentifierMessageLineForTest("C", "C3")
				};
			}
		}

		UniqueIdentifierMessageLine[] LinesWithUniquePartChanged
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("D", "D4"),
					new UniqueIdentifierMessageLineForTest("C", "C3")
				};
			}
		}

		UniqueIdentifierMessageLine[] LinesWithNonUniquePartChanged
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("B", "B4"),
					new UniqueIdentifierMessageLineForTest("C", "C3")
				};
			}
		}

		UniqueIdentifierMessageLine[] LinesWithDuplicates
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("B", "B4"),
					new UniqueIdentifierMessageLineForTest("B", "C3")
				};
			}
		}

		sealed class UniqueIdentifierMessageLineForTest : UniqueIdentifierMessageLine
		{
			public UniqueIdentifierMessageLineForTest(string uniqueIdentifier, string referenceText)
			{
				this.uniqueIdentifier = uniqueIdentifier;
				this.referenceText = referenceText;
			}

			readonly string uniqueIdentifier;
			readonly string referenceText;

			public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
			{
				(segmentGroup as SegmentGroup7).CNI[0].DocumentMessageDetails.LanguageNameCode = lineActionCode;
				(segmentGroup as SegmentGroup7).CNI[0].ConsolidationItemNumber = referenceText;
			}

			public override string UniqueIdentifier => uniqueIdentifier;

			public override SegmentGroup GetNewSegmentGroup(SegmentGroup message) => (message as CUSCARMessage).Group7.InstantiateAChildAndAddItToChildrenCollection();

			protected internal override Type SegmentGroupType => typeof(SegmentGroup7);
		}
	}
}
