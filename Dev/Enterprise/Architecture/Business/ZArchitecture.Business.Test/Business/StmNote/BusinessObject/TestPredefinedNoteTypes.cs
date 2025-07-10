using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class TestPredefinedNoteTypes : PredefinedNoteTypes
	{
		public new static TestPredefinedNoteTypes Instance
		{
			get { return (TestPredefinedNoteTypes)PredefinedNoteTypes.Instance; }
		}

		public static void Register()
		{
			OverrideNewDelegate(New);
		}

		public static void Unregister()
		{
			ResetNewDelegate();
		}

		static PredefinedNoteTypes New()
		{
			return new TestPredefinedNoteTypes();
		}

		public PredefinedNoteType TextOnlyNoteWithMaxLength100
		{
			get
			{
				if (textOnlyNoteWithMaxLength100 == null)
				{
					textOnlyNoteWithMaxLength100 = new PredefinedNoteType((NoResString)"TextOnlyNoteWithMaxLength100", StmNoteVisibility.PRV, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, 100, false);
				}
				return textOnlyNoteWithMaxLength100;
			}
		}
		PredefinedNoteType textOnlyNoteWithMaxLength100;

		public PredefinedNoteType TextOnlyNoteWithMaxLength200
		{
			get
			{
				if (textOnlyNoteWithMaxLength200 == null)
				{
					textOnlyNoteWithMaxLength200 = new PredefinedNoteType((NoResString)"TextOnlyNoteWithMaxLength200", StmNoteVisibility.PRV, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, 200, false);
				}
				return textOnlyNoteWithMaxLength200;
			}
		}
		PredefinedNoteType textOnlyNoteWithMaxLength200;

		public PredefinedNoteType TextOnlyNote
		{
			get
			{
				if (textOnlyNote == null)
				{
					textOnlyNote = new PredefinedNoteType((NoResString)"TextOnlyNote", StmNoteVisibility.PRV, !IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return textOnlyNote;
			}
		}
		PredefinedNoteType textOnlyNote;

		public PredefinedNoteType RichTextNote
		{
			get
			{
				if (richTextNote == null)
				{
					richTextNote = new PredefinedNoteType((NoResString)"RichTextNote", StmNoteVisibility.PRV, !IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return richTextNote;
			}
		}
		PredefinedNoteType richTextNote;

		public PredefinedNoteType PopupLogNote
		{
			get
			{
				if (popupLogNote == null)
				{
					popupLogNote = new PredefinedNoteType((NoResString)"PopupLogNote", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, IsPopupLog, false);
				}
				return popupLogNote;
			}
		}
		PredefinedNoteType popupLogNote;

		public PredefinedNoteType ReadOnlyAfterAddNote
		{
			get
			{
				if (readOnlyAfterAddNote == null)
				{
					readOnlyAfterAddNote = new PredefinedNoteType((NoResString)"ReadOnlyAfterAddNote", StmNoteVisibility.INT, IsUniqueInCollection, IsReadOnlyAfterAdd, IsTextOnly, IsPopupLog, false);
				}
				return readOnlyAfterAddNote;
			}
		}
		PredefinedNoteType readOnlyAfterAddNote;
	}
}
