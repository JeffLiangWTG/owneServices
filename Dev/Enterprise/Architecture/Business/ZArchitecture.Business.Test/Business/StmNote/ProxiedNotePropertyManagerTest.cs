using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(TestBOWithManagedNote))]
	sealed class ProxiedNotePropertyManagerTest : PersistentBusinessObjectTestCase
	{
		public void TestDeletedStmNote()
		{
			TestBOWithManagedNote bo = Factory.NewWithValidTestData<TestBOWithManagedNote>();
			StmNote note = Factory.New<StmNote>();
			note.ST_Description = "Detailed Goods Description";
			bo.Notes.Add(note);

			ProxiedNotePropertyManager property = new ProxiedNotePropertyManager(bo, bo.Notes, PredefinedNoteTypes.Instance.DetailedGoodsDescription);

			note.Delete();

			AssertNoExceptionThrown(() => { var prop = property.Value; });
		}

		public void TestNoteIsRegisteredAsAnEditableChildWithTheParentBO()
		{
			TestBOWithManagedNote bo = Factory.NewWithValidTestData<TestBOWithManagedNote>();
			AssertEquals("bo.HasChanges", false, bo.HasChanges);
			bo.Z0_GoodsDescriptionNote = "FREDDO";
			AssertEquals("bo.HasChanges", true, bo.HasChanges);
			Factory.Save();

			bo = new BusinessObjectFactory().Load<TestBOWithManagedNote>(bo.PK);
			AssertEquals("bo.HasChanges", false, bo.HasChanges);
			bo.Z0_GoodsDescriptionNote = "FROG";
			AssertEquals("bo.HasChanges", true, bo.HasChanges);
			Factory.Save();

			bo = new BusinessObjectFactory().Load<TestBOWithManagedNote>(bo.PK);
			AssertEquals("bo.HasChanges", false, bo.HasChanges);
			bo.Z0_GoodsDescriptionNote = string.Empty;
			AssertEquals("bo.HasChanges", true, bo.HasChanges);
			Factory.Save();

			bo = new BusinessObjectFactory().Load<TestBOWithManagedNote>(bo.PK);
			AssertEquals("bo.HasChanges", false, bo.HasChanges);
			bo.Z0_GoodsDescriptionNote = "CRAPS CHOCOLATE";
			AssertEquals("bo.HasChanges", true, bo.HasChanges);
		}

		public void TestGeneralNoteManagement()
		{
			TestBOWithManagedNote bo = Factory.NewWithValidTestData<TestBOWithManagedNote>();
			AssertEquals("Notes.Count to start with", 0, bo.GetNotes().GetAllNotes().Count);

			bo.Z0_GoodsDescriptionNote = string.Empty;
			AssertEquals("Notes.Count after setting empty string", 0, bo.Notes.GetAllNotes().Count);
			bo.Z0_GoodsDescriptionNote = "Something";
			StmNoteCollection notes = (StmNoteCollection)bo.GetNotes().GetAllNotes();
			AssertEquals("Notes.Count after setting 'Something'", 1, notes.Count);
			AssertEquals("Note Text Value", "Something", notes[0].ST_NoteText);

			bo.Z0_GoodsDescriptionNote = string.Empty;
			AssertEquals("Notes.Count after setting empty string", 0, bo.Notes.GetAllNotes().Count);
			bo.Z0_GoodsDescriptionNote = "Something";
			notes = (StmNoteCollection)bo.Notes.GetAllNotes();
			AssertEquals("Notes.Count after setting 'Something'", 1, notes.Count);
			AssertEquals("Note Text Value", "Something", notes[0].ST_NoteText);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			TestBOWithManagedNote bo2 = factory2.Load<TestBOWithManagedNote>(bo.PK);
			notes = (StmNoteCollection)bo2.Notes.GetAllNotes();
			AssertEquals("Notes.Count after reloading", 1, notes.Count);
			AssertEquals("Note Text Value after reloading", "Something", notes[0].ST_NoteText);
		}

		class TestBOWithManagedNote : DummyEnterpriseBusinessObject
		{
			public new class Schema : DummyBaseBusinessObject.Schema
			{
				public const string Z0_GoodsDescriptionNote = "Z0_GoodsDescriptionNote";
			}

			public TestBOWithManagedNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ProxiedNotePropertyManager GoodsDescriptionNoteManager
			{
				get { return goodsDescriptionNoteManager ?? (goodsDescriptionNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.DetailedGoodsDescription)); }
			}

			ProxiedNotePropertyManager goodsDescriptionNoteManager;

			public ZString Z0_GoodsDescriptionNote
			{
				get
				{
					return GoodsDescriptionNoteManager.Value;
				}

				set
				{
					CheckMaximumLength(Z0_GoodsDescriptionNoteInfo, value);
					GoodsDescriptionNoteManager.Value = value;
					Z0_GoodsDescriptionNoteInfo.RefreshBinding();
				}
			}

			public ZPropertyInfo Z0_GoodsDescriptionNoteInfo
			{
				get { return GetZPropertyInfo(Schema.Z0_GoodsDescriptionNote); }
			}

			public int Z0_GoodsDescriptionNote_MaxLength
			{
				get { return GoodsDescriptionNoteManager.MaxLength; }
			}
		}
	}
}
