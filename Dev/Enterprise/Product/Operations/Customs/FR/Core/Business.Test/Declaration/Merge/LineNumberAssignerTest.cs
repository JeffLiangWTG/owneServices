using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class LineNumberAssignerTest : TestCaseWithFactory
	{
		public void TestAssign0toDeletePendingLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_HighestLineNumber = 2;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;

			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;

			var entryLine5 = entryHeader.AllEntryLines.AddNew();
			entryLine5.CL_LineNumber = 5;
			entryLine5.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;

			var entryLine6 = entryHeader.AllEntryLines.AddNew();
			entryLine6.CL_LineNumber = 6;
			entryLine6.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;

			var entryLine7 = entryHeader.AllEntryLines.AddNew();
			entryLine7.CL_LineNumber = 7;

			var entryLine8 = entryHeader.AllEntryLines.AddNew();
			entryLine8.CL_LineNumber = 8;

			var lineNumberAssigner = new LineNumberAssigner(entryHeader);
			lineNumberAssigner.Execute();

			CombineAssertions(() =>
			{
				AssertEquals("line 1 not change", (ZShort)1, entryLine1.CL_LineNumber);
				AssertEquals("line 2 not change", (ZShort)2, entryLine2.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 3", ZShort.Zero, entryLine3.CL_LineNumber);
				AssertEquals("line 4 number assigned to 3", (ZShort)3, entryLine4.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 5", ZShort.Zero, entryLine5.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 6.", ZShort.Zero, entryLine6.CL_LineNumber);
				AssertEquals("line 7 number assigned to 4.", (ZShort)4, entryLine7.CL_LineNumber);
				AssertEquals("line 8 number assigned to 5.", (ZShort)5, entryLine8.CL_LineNumber);
			});

			var entryLine10 = entryHeader.AllEntryLines.AddNew();
			entryLine10.CL_LineNumber = 10;
			entryLine10.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;

			lineNumberAssigner = new LineNumberAssigner(entryHeader);
			lineNumberAssigner.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("line 1 not change", (ZShort)1, entryLine1.CL_LineNumber);
				AssertEquals("line 2 not change", (ZShort)2, entryLine2.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 3", ZShort.Zero, entryLine3.CL_LineNumber);
				AssertEquals("line 4 number assigned to 3", (ZShort)3, entryLine4.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 5", ZShort.Zero, entryLine5.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 6.", ZShort.Zero, entryLine6.CL_LineNumber);
				AssertEquals("line 7 number assigned to 4.", (ZShort)4, entryLine7.CL_LineNumber);
				AssertEquals("line 8 number assigned to 5.", (ZShort)5, entryLine8.CL_LineNumber);
				AssertEquals("line 10 number assigned to 6.", (ZShort)6, entryLine10.CL_LineNumber);
			});

			var entryLine11 = entryHeader.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 11;
			entryLine11.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;

			lineNumberAssigner = new LineNumberAssigner(entryHeader);
			lineNumberAssigner.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("line 1 not change", (ZShort)1, entryLine1.CL_LineNumber);
				AssertEquals("line 2 not change", (ZShort)2, entryLine2.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 3", ZShort.Zero, entryLine3.CL_LineNumber);
				AssertEquals("line 4 number assigned to 3", (ZShort)3, entryLine4.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 5", ZShort.Zero, entryLine5.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 6.", ZShort.Zero, entryLine6.CL_LineNumber);
				AssertEquals("line 7 number assigned to 4.", (ZShort)4, entryLine7.CL_LineNumber);
				AssertEquals("line 8 number assigned to 5.", (ZShort)5, entryLine8.CL_LineNumber);
				AssertEquals("line 10 number assigned to 6.", (ZShort)6, entryLine10.CL_LineNumber);
				AssertEquals("line 11 number assigned to 0.", (ZShort)0, entryLine11.CL_LineNumber);
			});

			entryLine3.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;
			entryLine5.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;
			entryLine6.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;

			var entryLine12 = entryHeader.AllEntryLines.AddNew();
			entryLine12.CL_LineNumber = 12;
			entryLine12.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;

			lineNumberAssigner = new LineNumberAssigner(entryHeader);
			lineNumberAssigner.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("line 1 not change", (ZShort)1, entryLine1.CL_LineNumber);
				AssertEquals("line 2 not change", (ZShort)2, entryLine2.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 3", ZShort.Zero, entryLine3.CL_LineNumber);
				AssertEquals("line 4 number assigned to 3", (ZShort)3, entryLine4.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 5", ZShort.Zero, entryLine5.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 6.", ZShort.Zero, entryLine6.CL_LineNumber);
				AssertEquals("line 7 number assigned to 4.", (ZShort)4, entryLine7.CL_LineNumber);
				AssertEquals("line 8 number assigned to 5.", (ZShort)5, entryLine8.CL_LineNumber);
				AssertEquals("line 10 number assigned to 6.", (ZShort)6, entryLine10.CL_LineNumber);
				AssertEquals("line 11 number assigned to 0.", (ZShort)0, entryLine11.CL_LineNumber);
				AssertEquals("line 12 number assigned to 7.", (ZShort)7, entryLine12.CL_LineNumber);
			});
		}

		public void TestEntryLinesNotInOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_HighestLineNumber = 2;

			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var lineNumberAssigner = new LineNumberAssigner(entryHeader);
			lineNumberAssigner.Execute();
			CombineAssertions(() =>
			{
				AssertEquals("line 1 not change", (ZShort)1, entryLine1.CL_LineNumber);
				AssertEquals("line 2 not change", (ZShort)2, entryLine2.CL_LineNumber);
				AssertEquals("Should assign 0 to DPD status line 3", ZShort.Zero, entryLine3.CL_LineNumber);
				AssertEquals("line 4 number assigned to 3", (ZShort)3, entryLine4.CL_LineNumber);
			});
		}
	}
}
