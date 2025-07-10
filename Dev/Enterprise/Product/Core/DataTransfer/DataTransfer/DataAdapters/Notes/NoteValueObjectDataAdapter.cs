using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class NoteValueObjectDataAdapter : ValueObjectDataAdapter<StmNote, Xsd.NotesNote>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootCollectionElementName
		{
			get { return "Notes"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootElementName
		{
			get { return "Note"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.NotesSchema; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleNoteSchema; }
		}

		protected override StmNote FindBusinessObject(Xsd.NotesNote value, IValueObjectImportContext context)
		{
			return null;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// the user doesn't want to see the 100's of notes created
		}

		#region ImportFromValueObjectCore

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a some code abbreviature., System defined type")]
		protected override void ImportFromValueObjectCore(StmNote noteBizObj, Xsd.NotesNote noteValue, IValueObjectImportContext context)
		{
			noteBizObj.ST_NoteType = "PUB";

			string noteType = NoteTypeToXmlCodeMappings.Instance.GetEnterpriseCode(noteValue.NoteType.ToString(), "Note", context);

			if (noteType != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(noteBizObj.ST_DescriptionInfo, noteType);
			}
			else
			{
				context.SetPropertyInfoValueIfValueNotEmpty(noteBizObj.ST_DescriptionInfo, noteValue.CustomNoteTypeName);
				noteBizObj.ST_IsCustomDescription = !CustomNotesProvider.Instance.NoteTypeExistsByName(noteValue.CustomNoteTypeName);
			}
			noteBizObj.ST_NoteDataAsText = noteValue.NoteData;
		}

		#endregion

		#region ExportToValueObjectCore

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System defined type")]
		protected override void ExportToValueObjectCore(StmNote noteBizObj, Xsd.NotesNote noteValue, IValueObjectExportContext context)
		{
			if (!noteBizObj.ST_NoteText.IsEmpty)
			{
				noteValue.NoteData = noteBizObj.ST_NoteText;
			}
			else
			{
				noteValue.NoteData = noteBizObj.ST_NoteDataAsText;
			}

			if (noteBizObj.ST_IsCustomDescription)
			{
				noteValue.NoteType = Xsd.NotesNoteNoteType.Custom;
				noteValue.CustomNoteTypeName = noteBizObj.ST_Description;
			}
			else
			{
				bool isCustomNoteType = false;
				if (noteBizObj.Master != null)
				{
					PredefinedNoteType noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(noteBizObj.ST_Description);
					if ((noteType != null) && noteType.IsCustomNoteType)
					{
						isCustomNoteType = true;
						noteValue.NoteType = Xsd.NotesNoteNoteType.Custom;
						noteValue.CustomNoteTypeName = noteBizObj.ST_Description;
					}
				}
				if (!isCustomNoteType)
				{
					noteValue.NoteType = NoteTypeToXmlCodeMappings.Instance.GetEnumExternalCode(noteBizObj.ST_Description, "Note", context);
				}
			}
			if (!noteBizObj.ST_CreatedDateUtc.IsEmpty)
			{
				noteValue.NoteCreatedDateTime = noteBizObj.ST_CreatedDateUtc;
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System defined type")]
		public void ImportNotesAndAttachToBusinessObjectNotes(Notes bizObjNotes, Xsd.NotesNoteCollection xsdNotes, IValueObjectImportContext context)
		{
			if (xsdNotes != null)
			{
				foreach (Xsd.NotesNote xsdNote in xsdNotes)
				{
					if (!xsdNote.NoteData.IsEmpty)
					{
						string noteType = (xsdNote.NoteType == Xsd.NotesNoteNoteType.Custom && !xsdNote.CustomNoteTypeName.IsEmpty) ?
							xsdNote.CustomNoteTypeName.ToString() :
							NoteTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xsdNote.NoteType, "Note", context);

						StmNote[] notes = bizObjNotes.FindByDescription(noteType);

						foreach (var note in notes.Skip(1))
						{
							note.Delete();
						}
						StmNote stmNote = notes.FirstOrDefault() ?? bizObjNotes.AddNew();
						ImportFromValueObject(stmNote, xsdNote, context);
					}
				}
			}
		}

		public Xsd.NotesNoteCollection ExportToXmlValueObjectCollection(Notes bizObjNotes, IValueObjectExportContext context)
		{
			Xsd.NotesNoteCollection notes = new Xsd.NotesNoteCollection();
			if (bizObjNotes.GetAllNotes().Count > 0)
			{
				StmNote[] publicNotes = bizObjNotes.FindByVisibility(StmNoteVisibility.PUB);
				if (publicNotes.Length > 0)
				{
					foreach (StmNote note in publicNotes)
					{
						if (!note.ST_NoteDataAsText.IsEmpty || !note.ST_NoteText.IsEmpty)
						{
							ExportToValueObject(note, notes.AddNew(), context);
						}
					}
				}
			}

#if DEBUG
			OrderDetailedGoodsDescriptionToFixShittyTesting(notes);
#endif
			return notes;
		}

#if DEBUG

		void OrderDetailedGoodsDescriptionToFixShittyTesting(Xsd.NotesNoteCollection notes)
		{
			Xsd.NotesNoteCollection goodsDescriptionNotes = new Xsd.NotesNoteCollection();
			for (int i = 0; i < notes.Count; i++)
			{
				Xsd.NotesNote note = notes[i];
				if (note.NoteType == Xsd.NotesNoteNoteType.DetailedGoodsDescription)
				{
					notes.RemoveAt(i);
					goodsDescriptionNotes.Add(note);
					i--;
				}
			}

			foreach (Xsd.NotesNote note in goodsDescriptionNotes)
			{
				notes.Add(note);
			}
		}

#endif
	}
}
