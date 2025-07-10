using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(HiddenStmNote))]
	sealed class HiddenStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestST_NoteTypeOnNewNoteIsSetToDOC()
		{
			AssertEquals(nameof(StmNoteVisibility.DOC), HiddenNote.ST_NoteType);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Cannot set the ST_NoteType on a HiddenStmNote.")]
		public void TestSetNoteTypeThrowException()
		{
			HiddenNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
		}

		public void TestDocNoteVisibilityPassesValidation()
		{
			HiddenNote.Validation.ValidateST_NoteType();
			AssertEquals("HiddenNote.ValidateST_NoteType() should not add notifications.", false, HiddenNote.ST_NoteTypeInfo.HasNotifications());

			HiddenNote.RunPreSaveValidation();
			AssertEquals("HiddenNote.RunPreSaveValidation() should not add notifications to ST_NoteType.", false, HiddenNote.ST_NoteTypeInfo.HasNotifications());
		}

		public void TestSettingNoteDataOrTextDoesNotReportDevError()
		{
			HiddenNote.ST_NoteData = ZBlob.FromAscii("Carcassonne");
			AssertEquals("HiddenNote.ST_NoteData", ZBlob.FromAscii("Carcassonne"), HiddenNote.ST_NoteData);

			HiddenNote.ST_NoteText = "Catan";
			AssertEquals("HiddenNote.ST_NoteText", "Catan", HiddenNote.ST_NoteText);
		}

		public void TestHiddenNoteUseBothDataAndTextFields()
		{
			HiddenNote.ST_Description = "Games";
			HiddenNote.ST_NoteData = ZBlob.Empty;
			HiddenNote.ST_NoteText = "San Juan";
			Factory.Save();
			AssertEquals("HiddenNote.ST_NoteData", ZBlob.Empty, HiddenNote.ST_NoteData);
			AssertEquals("HiddenNote.ST_NoteText", "San Juan", HiddenNote.ST_NoteText);
		}

		public void TestCanStoreBothTextAndDataInNote()
		{
			HiddenNote.ST_Description = "Games";
			HiddenNote.ST_NoteData = ZBlob.FromAscii("Puerto Rico");
			HiddenNote.ST_NoteText = "San Juan";
			Factory.Save();

			AssertEquals("HiddenNote.ST_NoteData", ZBlob.FromAscii("Puerto Rico"), HiddenNote.ST_NoteData);
			AssertEquals("HiddenNote.ST_NoteText", "San Juan", HiddenNote.ST_NoteText);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return HiddenNote;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			StmNote note = (StmNote)base.GetNewBusinessObjectForDeleteTest(factory);
			note.ST_Table = "DummyBizo";
			return note;
		}

		protected override void SetUp()
		{
			base.SetUp();
			HiddenNote = Factory.New<HiddenStmNote>();
			DummyBizOWithRelatedNotes bizO = Factory.New<DummyBizOWithRelatedNotes>();
			HiddenNote.ST_ParentID = bizO.PK;
			HiddenNote.ST_Table = bizO.TableName;
			bizO.Notes.Add(HiddenNote);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (StmNote)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			result.ST_Table = "DummyBizO";
			return result;
		}

		HiddenStmNote HiddenNote;

		#endregion
	}
}
