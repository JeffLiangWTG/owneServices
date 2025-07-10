using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MonthlyClosingHelperTest : TestCaseWithFactory
	{
		public void TestGetMonthlyClosingLinesNote()
		{
			var message = Factory.New<EDIMessage>();
			MonthlyClosingHelper.CreateMonthlyClosingLinesNote(message, new[] { 1, 3, 5 });
			AssertEquals("1|3|5", message.GetMonthlyClosingLinesNote());
		}

		public void TestGetMonthlyClosingLinesNote_NoNote()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(ZString.Empty, message.GetMonthlyClosingLinesNote());
		}

		public void TestGetMonthlyClosingLinesNote_NullMessage()
		{
			AssertEquals(ZString.Empty, MonthlyClosingHelper.GetMonthlyClosingLinesNote(null));
		}

		public void TestGetMonthlyClosingLinesNote_InvalidDescription()
		{
			var message = Factory.New<EDIMessage>();
			var stmNote = message.Notes.AddNew();
			stmNote.ST_Description = "Test";
			stmNote.ST_NoteDataAsText = "1|3|5";
			AssertEquals(ZString.Empty, message.GetMonthlyClosingLinesNote());
		}

		public void TestCreateMonthlyClosingLinesNote_MessageNULL()
		{
			AssertNoExceptionThrown(() => MonthlyClosingHelper.CreateMonthlyClosingLinesNote(null, new[] { 1, 3, 5 }));
		}

		public void TestCreateMonthlyClosingLinesNote_EmptyLineNumbers()
		{
			AssertNoExceptionThrown(() => MonthlyClosingHelper.CreateMonthlyClosingLinesNote(Factory.New<EDIMessage>(), Enumerable.Empty<int>()));
		}

		public void TestCreateMonthlyClosingLinesNote_NullLineNumbers()
		{
			AssertNoExceptionThrown(() => MonthlyClosingHelper.CreateMonthlyClosingLinesNote(Factory.New<EDIMessage>(), null));
		}

		public void TestCreateMonthlyClosingLinesNote()
		{
			var message = Factory.New<EDIMessage>();
			MonthlyClosingHelper.CreateMonthlyClosingLinesNote(message, new[] { 1, 3, 5 });
			AssertEquals("1|3|5", message.GetMonthlyClosingLinesNote());
		}

		public void TestGetFinalizationFlagNote_NullDeclaration()
		{
			AssertEquals(ZString.Empty, MonthlyClosingHelper.GetFinalizationFlagNote(null));
		}

		public void TestGetFinalizationFlagNote_NoNote()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote = declaration.Notes.AddNew();
			stmNote.ST_Description = "Description";
			stmNote.ST_NoteDataAsText = "Value";
			AssertEquals(ZString.Empty, declaration.GetFinalizationFlagNote());
		}

		public void TestGetFinalizationFlagNote_MultipleNotes()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote1 = declaration.Notes.AddNew();
			stmNote1.ST_Description = MonthlyClosingHelper.FinalizationFlagNoteDescription;
			stmNote1.ST_NoteDataAsText = "Value1";
			var stmNote2 = declaration.Notes.AddNew();
			stmNote2.ST_Description = MonthlyClosingHelper.FinalizationFlagNoteDescription;
			stmNote2.ST_NoteDataAsText = "Value2";
			AssertExceptionThrown<InvalidOperationException>(() => declaration.GetFinalizationFlagNote());
		}

		public void TestGetFinalizationFlagNote_SingleNote()
		{
			var declaration = Factory.New<CusReconDeclaration>();
			var stmNote = declaration.Notes.AddNew();
			stmNote.ST_Description = MonthlyClosingHelper.FinalizationFlagNoteDescription;
			stmNote.ST_NoteDataAsText = "Value";
			AssertEquals("Value", declaration.GetFinalizationFlagNote());
		}

		public void TestCreateFinalizationFlagNote()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<CusReconDeclaration>();
				AssertEquals("No note", ZString.Empty, declaration.GetFinalizationFlagNote());

				declaration.CreateFinalizationFlagNote("1");
				AssertEquals("Note created", "1", declaration.GetFinalizationFlagNote());

				declaration.CreateFinalizationFlagNote("2");
				AssertEquals("Note updated", "2", declaration.GetFinalizationFlagNote());
			});
		}
	}
}
