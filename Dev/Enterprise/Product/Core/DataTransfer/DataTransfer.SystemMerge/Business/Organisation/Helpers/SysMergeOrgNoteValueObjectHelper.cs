using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class SysMergeOrgNoteValueObjectHelper
	{
		public SysMergeOrgNoteValueObjectHelper()
		{
		}

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgNoteCollection xsdOrgNotes, OrgHeaderForDataTransfer org, IValueObjectImportContext context)
		{
			if (xsdOrgNotes.IsSpecified)
			{
				foreach (Xsd.SysMergeOrgNote xsdOrgNote in xsdOrgNotes)
				{
					ImportFromValueObject(xsdOrgNote, org, context);
				}
			}
		}

		void ImportFromValueObject(Xsd.SysMergeOrgNote xsdOrgNote, OrgHeaderForDataTransfer org, IValueObjectImportContext context)
		{
			StmNote orgNote = org.Factory.New<StmNote>();
			orgNote.ST_Table = OrgHeaderSchema.Constants.TableName;
			orgNote.ST_ParentID = org.PK;
			orgNote.Master = org;
			orgNote.ST_Description = xsdOrgNote.Description;
			orgNote.ST_NoteContext = xsdOrgNote.NoteContext;
			orgNote.ST_IsCustomDescription = xsdOrgNote.IsCustomDescription;
			orgNote.ST_ForceRead = xsdOrgNote.ForceRead;
			orgNote.ST_NoteDataAsText = xsdOrgNote.NoteText;
			orgNote.ST_NoteType = xsdOrgNote.NoteType;

			if (orgNote.IsRichTextAsDOCNote)
			{
				string text = xsdOrgNote.NoteText.Length <= 50 ? (string)xsdOrgNote.NoteText : (string)xsdOrgNote.NoteText.Substring(0, 50) + "...";
				context.Notify(new WarningNotification(Res.GetString("f8ace934-4da8-4dfe-bcf1-e06905923008",
				"There is a Note in the XML that would be confused for being a Document Note (DOC and no description) but has a rich text payload. To prevent errors, it will not be saved. Text: {0}", text)));
			}
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgNoteCollection xsdOrgNotes, INotifications notifications)
		{
			ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, org.PK);
			query.AddToFilter(new ZQuery(StmNoteSchema.ST_Table, org.TableName).AddToFilter(JoinCondition.Or, StmNoteSchema.ST_Table, ""));
			StmNote[] notesArray = org.Factory.Load<StmNote>(query);

			for (int i = 0; i < notesArray.Length; i++)
			{
				StmNote note = notesArray[i];
				ExportToValueObject(notesArray[i], xsdOrgNotes.AddNew(), notifications);
			}
		}

		void ExportToValueObject(StmNote note, Xsd.SysMergeOrgNote xsdOrgNote, INotifications notifications)
		{
			if (!note.ST_NoteText.IsEmpty)
			{
				xsdOrgNote.NoteText = note.ST_NoteText;
			}
			else if (!note.ST_NoteData.IsEmpty)
			{
				xsdOrgNote.NoteText = note.ST_NoteDataAsText;
			}
			if (!note.ST_Description.IsEmpty)
			{
				xsdOrgNote.Description = note.ST_Description;
				xsdOrgNote.DescriptionSpecified = true;
			}
			if (!note.ST_NoteContext.IsEmpty)
			{
				xsdOrgNote.NoteContext = note.ST_NoteContext;
				xsdOrgNote.NoteContextSpecified = true;
			}
			if (!note.ST_NoteType.IsEmpty)
			{
				xsdOrgNote.NoteType = note.ST_NoteType;
				xsdOrgNote.NoteTypeSpecified = true;
			}
			xsdOrgNote.IsCustomDescription = note.ST_IsCustomDescription;
			xsdOrgNote.IsCustomDescriptionSpecified = true;
			xsdOrgNote.ForceRead = note.ST_ForceRead;
			xsdOrgNote.ForceReadSpecified = true;
		}

		#endregion
	}
}
