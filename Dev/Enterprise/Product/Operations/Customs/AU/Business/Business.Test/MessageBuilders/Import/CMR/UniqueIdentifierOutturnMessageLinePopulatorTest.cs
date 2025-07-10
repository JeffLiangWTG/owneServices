using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniqueIdentifierOutturnMessageLinePopulatorTest : TestCaseWithFactory
	{
		public void TestOriginal()
		{
			AssertEquals("Message", "CNI+A1+:::I'CNI+B2+:::I'CNI+C3+:::I'", GetMessage(OriginalLines, null));
		}

		public void TestChangeNonUniqueIdentifierLineValue()
		{
			AssertEquals("Non unique changes should only amend, not delete and insert.", "CNI+B4+:::A'", GetMessage(LinesWithNonUniquePartChanged, OriginalLines));
		}

		public void TestChangeUniqueIdentifierLineValue()
		{
			AssertEquals("Message", "CNI+B2+:::D'CNI+D4+:::I'", GetMessage(LinesWithUniquePartChanged, OriginalLines));
		}

		public void TestChangeUniqueIdentifierOfNewCARSTLineAdded()
		{
			AssertEquals("Message should only have the new 'insert' line for the new line, does not need a delete line even though unique identifier was changed, as it has not been reported to Customs yet.", "CNI+D4+:::I'", GetMessage(LinesWithUniquePartChangedOnNewLine, OriginalLines));
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

		public void TestOriginalWhenSplitMessage()
		{
			AssertEquals("SplitMessage1", "CNI+A1+:::I'CNI+B2+:::I'", GetMessage(OriginalLinesSplit1, null));
			AssertEquals("SplitMessage2", "CNI+C3+:::I'", GetMessage(OriginalLinesSplit2, null));
		}

		public void TestChangeNonUniqueIdentifierLineValueWhenSplitMessage()
		{
			AssertEquals("ChangeNonUniqueIdentifier: Message should just amend line details - Split Message", "CNI+B4+:::A'", GetMessage(SplitLinesWithNonUniquePartChanged, LinesWithNonUniquePartChanged, OriginalLines));
		}

		public void TestChangeUniqueIdentifierLineValueWhenSplitMessages()
		{
			AssertEquals("Amended SplitMessage1", "CNI+HB2 - GOODS+:::D'CNI+HB3 - GOODS+:::D'CNI+HB5 - GOODS+:::D'", GetMessage(ChangedLinesForSplitting1, SplitLinesWithUniquePartChanged, OriginalLinesForSplitting));
			AssertEquals("Amended SplitMessage2", "CNI+HB2A - GOODS+:::I'CNI+HB6 - GOODS+:::D'CNI+HB7 - GOODS+:::D'", GetMessage(ChangedLinesForSplitting2, SplitLinesWithUniquePartChanged, OriginalLinesForSplitting));
			AssertEquals("Amended SplitMessage3", "CNI+HB3A - GOODS+:::I'CNI+HB5A - GOODS+:::I'CNI+HB6A - GOODS+:::I'", GetMessage(ChangedLinesForSplitting3, SplitLinesWithUniquePartChanged, OriginalLinesForSplitting));
			AssertEquals("Amended SplitMessage3", "CNI+HB7A - GOODS+:::I'", GetMessage(ChangedLinesForSplitting4, SplitLinesWithUniquePartChanged, OriginalLinesForSplitting));
		}

		[ExpectNoExceptions()]
		public void TestPopulateDoesNotBlowUp()
		{
			CUSCARMessage message = new CUSCARMessage();
			UniqueIdentifierOutturnMessageLinePopulator populator = new UniqueIdentifierOutturnMessageLinePopulator();
			populator.Populate(message, LinesWithDuplicates);

			populator = new UniqueIdentifierOutturnMessageLinePopulator();
			populator.Populate(message, LinesWithLineAdded, LinesWithDuplicates);
		}

		string GetMessage(UniqueIdentifierMessageLine[] newLines, UniqueIdentifierMessageLine[] originalLines)
		{
			UniqueIdentifierOutturnMessageLinePopulator populator = new UniqueIdentifierOutturnMessageLinePopulator();
			CUSCARMessage cuscar = new CUSCARMessage();
			if (originalLines == null)
			{
				populator.Populate(cuscar, newLines);
			}
			else
			{
				populator.Populate(cuscar, newLines, originalLines);
			}
			return cuscar.ToString(new Edifact.UNOCCMRCharacterSet());
		}

		string GetMessage(UniqueIdentifierMessageLine[] splitLines, UniqueIdentifierMessageLine[] newLines, UniqueIdentifierMessageLine[] originalLines)
		{
			var populator = new UniqueIdentifierOutturnMessageLinePopulator();
			var cuscar = new CUSCARMessage();
			populator.PopulateSplitMsgLines(cuscar, splitLines, newLines, originalLines);
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

		UniqueIdentifierMessageLine[] OriginalLinesSplit1
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("A", "A1"),
					new UniqueIdentifierMessageLineForTest("B", "B2")
				};
			}
		}

		UniqueIdentifierMessageLine[] OriginalLinesSplit2
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
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

		UniqueIdentifierMessageLine[] LinesWithUniquePartChangedOnNewLine
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new CreateUniqueIdentifierMessageLineForTest("A", "A1", ZDateTime.Today),
					new CreateUniqueIdentifierMessageLineForTest("B", "B2", ZDateTime.Today),
					new CreateUniqueIdentifierMessageLineForTest("C", "C3", ZDateTime.Today),
					new CreateUniqueIdentifierMessageLineForTest("D", "D4", ZDateTime.Empty)
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

		UniqueIdentifierMessageLine[] SplitLinesWithNonUniquePartChanged
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("B", "B4")
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

		UniqueIdentifierMessageLine[] OriginalLinesForSplitting
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB1", "HB1 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB2", "HB2 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB3", "HB3 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB4", "HB4 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB5", "HB5 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB6", "HB6 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB7", "HB7 - GOODS")
				};
			}
		}

		UniqueIdentifierMessageLine[] SplitLinesWithUniquePartChanged
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB1", "HB1 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB2A", "HB2A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB3A", "HB3A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB4", "HB4 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB5A", "HB5A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB6A", "HB6A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB7A", "HB7A - GOODS")
				};
			}
		}

		UniqueIdentifierMessageLine[] ChangedLinesForSplitting1
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB2", "HB2 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB3", "HB3 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB5", "HB5 - GOODS"),
				};
			}
		}

		UniqueIdentifierMessageLine[] ChangedLinesForSplitting2
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB6", "HB6 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB7", "HB7 - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB2A", "HB2A - GOODS")
				};
			}
		}

		UniqueIdentifierMessageLine[] ChangedLinesForSplitting3
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB3A", "HB3A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB5A", "HB5A - GOODS"),
					new UniqueIdentifierMessageLineForTest("MAWB1/HB6A", "HB6A - GOODS")
				};
			}
		}

		UniqueIdentifierMessageLine[] ChangedLinesForSplitting4
		{
			get
			{
				return new UniqueIdentifierMessageLine[]
				{
					new UniqueIdentifierMessageLineForTest("MAWB1/HB7A", "HB7A - GOODS")
				};
			}
		}

		sealed class UniqueIdentifierMessageLineForTest : UniqueIdentifierMessageLine
		{
			public UniqueIdentifierMessageLineForTest(string uniqueIdentifier, string referenceText)
			{
				this.uniqueIdentifier = uniqueIdentifier;
				this.referenceText = referenceText;
				lastMessageDate = ZDateTime.Today;
			}

			readonly string uniqueIdentifier;
			readonly string referenceText;
			readonly ZDateTime lastMessageDate;

			public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
			{
				(segmentGroup as SegmentGroup7).CNI[0].DocumentMessageDetails.LanguageNameCode = lineActionCode;
				(segmentGroup as SegmentGroup7).CNI[0].ConsolidationItemNumber = referenceText;
			}

			public override string UniqueIdentifier => uniqueIdentifier;

			public override ZDateTime LastMessageDate => lastMessageDate;

			public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
				=> (message as CUSCARMessage).Group7.InstantiateAChildAndAddItToChildrenCollection();

			protected internal override Type SegmentGroupType => typeof(SegmentGroup7);
		}

		sealed class CreateUniqueIdentifierMessageLineForTest : UniqueIdentifierMessageLine
		{
			public CreateUniqueIdentifierMessageLineForTest(string uniqueIdentifier, string referenceText, ZDateTime lastMessageDate)
			{
				this.uniqueIdentifier = uniqueIdentifier;
				this.referenceText = referenceText;
				this.lastMessageDate = lastMessageDate;
			}

			readonly string uniqueIdentifier;
			readonly string referenceText;
			readonly ZDateTime lastMessageDate;

			public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
			{
				(segmentGroup as SegmentGroup7).CNI[0].DocumentMessageDetails.LanguageNameCode = lineActionCode;
				(segmentGroup as SegmentGroup7).CNI[0].ConsolidationItemNumber = referenceText;
			}

			public override string UniqueIdentifier => uniqueIdentifier;

			public override ZDateTime LastMessageDate => lastMessageDate;

			public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
				=> (message as CUSCARMessage).Group7.InstantiateAChildAndAddItToChildrenCollection();

			protected internal override Type SegmentGroupType => typeof(SegmentGroup7);
		}
	}
}
