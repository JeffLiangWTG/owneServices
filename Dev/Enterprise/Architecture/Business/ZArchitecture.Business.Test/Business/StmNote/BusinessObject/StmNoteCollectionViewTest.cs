using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteCollectionView))]
	sealed class StmNoteCollectionViewTest : BusinessObjectCollectionViewTestCase<StmNoteCollectionView>
	{
		protected override StmNoteCollectionView GetCollectionToTest()
		{
			return new StmNoteCollectionView(DummyWithNotes);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var note = Factory.New<StmNote>();
			note.ST_Table = DummyWithNotes.TableName;
			note.ST_ParentID = DummyWithNotes.PK;
			return note;
		}

		public void TestIsNoteOnDeclarationReadOnly()
		{
			EnvProxy.Instance.Security.NotesModifyAllCompany.IsAllowed = false;
			var factory1 = new BusinessObjectFactory();

			var declaration = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.US.IJobDeclaration)));
			var notes = declaration.GetNotes();
			var note1 = notes.AddNew();

			note1.ST_GC_RelatedCompany = ZGuid.NewZGuid();
			var note2 = notes.AddNew();
			note2.ST_GC_RelatedCompany = ZGuid.NewZGuid();

			StmNoteCollectionView view = new StmNoteCollectionView(declaration as IStmNoteParent);
			view.ShowNotesForAllCompanies = true;
			view.ShowRelatedNotes = true;

			AssertEquals("", 2, view.Count);

			AssertEquals(note1.ReadOnly, true);
			AssertEquals(note2.ReadOnly, true);
		}

		public void TestConstructor()
		{
			StmNoteCollectionView view = new StmNoteCollectionView(DummyWithNotes);
			AssertNotNull(view);
		}

		public void TestAllowNew()
		{
			bool oldValue = EnvProxy.Instance.Security.NotesNew.IsAllowed;
			StmNoteCollectionView view = new StmNoteCollectionView(DummyWithNotes);
			try
			{
				EnvProxy.Instance.Security.NotesNew.IsAllowed = true;
				Assert(view.AllowNew);

				EnvProxy.Instance.Security.NotesNew.IsAllowed = false;
				Assert(!view.AllowNew);
			}
			finally
			{
				EnvProxy.Instance.Security.NotesNew.IsAllowed = oldValue;
			}
		}

		public void TestRebuildingView()
		{
			View.ShowRelatedNotes = false;
			AssertEquals("StmNoteCollectionView.Count", 0, View.Count);

			DummyWithNotes.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			DummyWithNotes.RelatedDummy.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			View.ShowRelatedNotes = true;
			AssertEquals("StmNoteCollectionView.Count", 2, View.Count);

			DummyWithNotes.RelatedDummy.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 3, View.Count);

			View.ShowRelatedNotes = false;
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);
		}

		public void TestShowNotesForAllCompany()
		{
			View.ShowNotesForAllCompanies = false;
			AssertEquals("StmNoteCollectionView.Count", 0, View.Count);

			var note = DummyWithNotes.Notes.AddNew();
			note.ST_GC_RelatedCompany = Guid.NewGuid();
			AssertEquals("StmNoteCollectionView.Count. Not belongs to current company but not saved to DB yet.", 1, View.Count);

			note.ST_GC_RelatedCompany = Guid.Empty;
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			Factory.Save();
			note.ST_GC_RelatedCompany = Guid.NewGuid();
			AssertEquals("StmNoteCollectionView.Count. Should be hidden since note not belongs to current company and have been saved to DB.", 0, View.Count);

			note.ST_GC_RelatedCompany = Environment.EnvProxy.Instance.CurrentCompany.PK;
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			var note2 = DummyWithNotes.Notes.AddNew();
			Factory.Save();
			note2.ST_GC_RelatedCompany = Guid.NewGuid();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);
			Assert("Note belongs to current company is not readonly", !View[0].ReadOnly);

			EnvProxy.Instance.Security.NotesModifyAllCompany.IsAllowed = false;
			View.ShowNotesForAllCompanies = true;
			AssertEquals("StmNoteCollectionView.Count", 2, View.Count);
			Assert("Note belongs to a different company is readonly when dont have premission to modify", View[1].ReadOnly);

			View.ShowNotesForAllCompanies = false;
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);
		}

		public void TestAttemptToDeleteRelatedNoteFails()
		{
			View.ShowRelatedNotes = true;

			DummyWithNotes.RelatedDummy.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			View.RemoveAndDeleteAll();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			DummyWithNotes.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 2, View.Count);

			View.RemoveAndDeleteAll();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);
		}

		public void TestHasChangesFromDeleteOnViewAndCollection()
		{
			var note = DummyWithNotes.Notes.AddNew();
			Factory.Save();
			AssertEquals(false, ((IBusinessObjectCollectionInternals)View).HasChangesFromDelete);
			AssertEquals(false, ((IBusinessObjectCollectionInternals)DummyWithNotes.Notes.ElementsInternal).HasChangesFromDelete);
			View.RemoveAndDelete(note);
			AssertEquals(true, ((IBusinessObjectCollectionInternals)View).HasChangesFromDelete);
			AssertEquals(true, ((IBusinessObjectCollectionInternals)DummyWithNotes.Notes.ElementsInternal).HasChangesFromDelete);
			Factory.Save();
			//AssertEquals(false, ((IBusinessObjectCollectionInternals)View).HasChangesFromDelete); //It's not set false because it's not the child of any BizO that got saved. (Which incidentally means no one really cares if this is set true or false.)
			AssertEquals(false, ((IBusinessObjectCollectionInternals)DummyWithNotes.Notes.ElementsInternal).HasChangesFromDelete);
		}

		public void TestAttemptToDeleteNonEditableNoteAfterSavingFails()
		{
			StmNote note = DummyWithNotes.Notes.AddNew();
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			View.RemoveAndDelete(note);
			AssertEquals("StmNoteCollectionView.Count", 0, View.Count);

			StmNote nonEditableNote1 = DummyWithNotes.Notes.AddNew();
			nonEditableNote1.ST_Description = DummyBizOWithRelatedNotes.BlackNoteTypeIsNotEditable.Description;
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);

			View.RemoveAndDelete(nonEditableNote1);
			AssertEquals("StmNoteCollectionView.Count", 0, View.Count);

			StmNote nonEditableNote2 = DummyWithNotes.Notes.AddNew();
			nonEditableNote2.ST_Description = PredefinedNoteTypes.Instance.ProjectLog.Description;
			Factory.Save();
			View.RemoveAndDelete(nonEditableNote2);
			AssertEquals("StmNoteCollectionView.Count", 1, View.Count);
		}

		public void TestNoteAddedInternalEventFired()
		{
			StmNote note = DummyWithNotes.Notes.AddNew();
			AssertEquals("Note Added event not fired as it was not hooked up", false, NoteAddedEventFired);

			DummyWithNotes.Notes.VisibleNotes.NoteAddedInternal += new NoteAddedEventHandler(VisibleNotes_NoteAdded);
			StmNote note2 = DummyWithNotes.Notes.AddNew();

			AssertEquals("Note Added event fired as it was hooked up", true, NoteAddedEventFired);
			AssertEquals("Note exists in eventargs", note2, NoteAddedEventNote);

			NoteAddedEventFired = false;
			NoteAddedEventNote = null;
		}

		#region Henry

		class StmNoteSpecial : StmNote
		{
			public StmNoteSpecial(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override StmNoteValidation GetNewValidation()
			{
				return new StmNoteSpecialValidation(this);
			}
		}

		class StmNoteSpecialValidation : StmNoteValidation
		{
			public StmNoteSpecialValidation(StmNoteSpecial parent)
				: base(parent)
			{
			}

			protected override void CheckST_Description()
			{
				// any description (even empty) will do
			}
		}

		class SpecialNotes : Notes
		{
			public SpecialNotes(BusinessObject parentBizO)
				: base((IStmNoteParent)parentBizO)
			{
			}

			protected override Type ElementType
			{
				get { return typeof(StmNoteSpecial); }
			}

			protected override BusinessObjectCollection GetNewElementsCollection()
			{
				return new SpecialStmNoteDependantCollection(Parent, ((BusinessObject)Parent).Factory);
			}
		}

		class SpecialStmNoteDependantCollection : StmNoteCollection
		{
			public SpecialStmNoteDependantCollection(IStmNoteParent parentBizO, BusinessObjectFactory factory)
				: base(parentBizO, factory)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(StmNoteSpecial);
			}
		}

		class DummyBizOWithRelatedNotesUsingStmNoteSpecial : DummyEnterpriseBusinessObject
		{
			public DummyBizOWithRelatedNotesUsingStmNoteSpecial(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override Notes Notes
			{
				get { return notes ?? (notes = new SpecialNotes(this)); }
			}
			Notes notes;
		}

		public void TestTypeOfElements()
		{
			var specialDummyWithNotes = Factory.New<DummyBizOWithRelatedNotesUsingStmNoteSpecial>();

			var note = (StmNoteSpecial)specialDummyWithNotes.Notes.AddNew();
			var testView = new StmNoteCollectionView(specialDummyWithNotes, typeof(StmNoteSpecial));

			AssertEquals("Count", 1, testView.Count);
			AssertEquals("Type", typeof(StmNoteSpecial), testView[0].GetType());
		}

		#endregion

		#region Implemention

		DummyBizOWithRelatedNotes DummyWithNotes
		{
			get
			{
				if (fDummyWithNotes == null)
				{
					fDummyWithNotes = Factory.New<DummyBizOWithRelatedNotes>();
				}

				return fDummyWithNotes;
			}
		}
		DummyBizOWithRelatedNotes fDummyWithNotes;

		StmNoteCollectionView View;

		protected override void SetUp()
		{
			View = new StmNoteCollectionView(DummyWithNotes);
		}

		bool NoteAddedEventFired;
		StmNote NoteAddedEventNote;

		void VisibleNotes_NoteAdded(NoteAddedEventArgs args)
		{
			NoteAddedEventFired = true;
			NoteAddedEventNote = args.NoteAdded;
		}

		#endregion
	}
}
