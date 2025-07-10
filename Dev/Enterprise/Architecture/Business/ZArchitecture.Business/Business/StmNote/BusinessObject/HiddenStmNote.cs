using System;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class HiddenStmNote : StmNote
	{
		public HiddenStmNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new HiddenStmNoteValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.ST_NoteType = nameof(StmNoteVisibility.DOC);
		}

		[BusinessObjectTestExclude] // This property is fixed as "DOC" and thus cannot be set here.
		public override ZString ST_NoteType
		{
			get { return base.ST_NoteType; }
			set { throw new NotSupportedException("Cannot set the ST_NoteType on a HiddenStmNote."); }
		}

#if DEBUG
		[BusinessObjectTestExclude] // This property calls through to ST_NoteType which is fixed as "DOC" will report a dev error during testing.
		public new ZString ST_NoteType_DescriptiveText
		{
			get { return base.ST_NoteType_DescriptiveText; }
			set { base.ST_NoteType_DescriptiveText = value; }
		}
#endif

		protected override void ReportDeveloperIfNoteTextShouldNotBeSet(ZString value)
		{
			// DOC note is used for any sort of storage (note data or note text)
		}

		protected override void ReportDeveloperIfNoteDataShouldNotBeSet(ZBlob value)
		{
			// DOC note is used for any sort of storage (note data or note text)
		}

		protected override void ClearUnusedNoteTextOrData()
		{
			// DOC note is used for any sort of storage (note data or note text)
		}
	}

	class HiddenStmNoteValidation : StmNoteValidation
	{
		public HiddenStmNoteValidation(HiddenStmNote parent) : base(parent)
		{
		}

		protected override void CheckST_NoteType()
		{
		}
	}
}
