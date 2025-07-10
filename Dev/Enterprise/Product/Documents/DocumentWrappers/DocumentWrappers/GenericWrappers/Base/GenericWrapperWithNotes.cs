using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	[AllowNoStaticNew, AllowPublicConstructor]
	public abstract class GenericWrapperWithNotes : GenericWrapper
	{
		protected GenericWrapperWithNotes(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
		}

		#region Notes

		public NoteWrapperCollection Notes
		{
			get { return fFreightNotes ?? (fFreightNotes = GetTextNotes()); }
		}
		NoteWrapperCollection fFreightNotes;

		protected virtual NoteWrapperCollection GetTextNotes()
		{
			return new NoteWrapperCollection(GetParentBOForNoteStorageEDocsAndDocData(), Factory);
		}

		#endregion

		#region Notes Including Related Business Objects

		public NoteWrapperCollection NotesIncludingRelated
		{
			get { return relatedNotes ?? (relatedNotes = GetTextNotesIncludingRelatedNotes()); }
		}
		NoteWrapperCollection relatedNotes;

		NoteWrapperCollection GetTextNotesIncludingRelatedNotes()
		{
			return new NoteWrapperCollection(GetParentBOForNoteStorageEDocsAndDocData(), true, Factory);
		}

		#endregion
	}
}
