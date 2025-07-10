using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	class DocumentNoteValidation : StmNoteValidation
	{
		public DocumentNoteValidation(DocumentNote parent)
			: base(parent)
		{
		}

		protected override void CheckST_Description()
		{
			// ignore validation
		}

		protected override void CheckST_NoteType()
		{
			// ignore validation
		}

		protected override void CheckST_NoteData()
		{
			// ignore validation
		}

		protected override void CheckST_NoteDataIsValidZBlobSize()
		{
			// ignore validation
		}
	}
}
