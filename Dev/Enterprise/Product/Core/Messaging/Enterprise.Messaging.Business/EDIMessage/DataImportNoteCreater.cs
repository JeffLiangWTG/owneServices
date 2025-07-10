using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	//Object Factory getter
	public class EDIMessageDataImportNoteCreator : IEDIMessageDataImportNoteCreator
	{
		public void AddNew(IEDIMessage message, Action<Stream> getNoteTextIntoStream)
		{
			DataImportNoteCreater.AddNew(message, getNoteTextIntoStream);
		}
	}

	public static class DataImportNoteCreater
	{
		public const int NoteSwitchToFileLimitInBytes = 32768;

		public static StmNote AddNew(IEDIMessage message, Action<Stream> getNoteTextIntoStream)
		{
			var noteStream = new VirtualMemoryStream(switchToFileLimitInBytes: NoteSwitchToFileLimitInBytes);
			
			getNoteTextIntoStream(noteStream);

			var factory = message.Factory;
			var note = factory.New<StmNote>();
			var noteType = PredefinedNoteTypes.Instance.DataImportLogNote;

			var row = ((INeedRow)note).Row;

			row[StmNoteSchema.ST_GC_RelatedCompany.Name] = DBNull.Value;
			row[StmNoteSchema.ST_ParentID.Name] = message.PK.ToGuid();
			row[StmNoteSchema.ST_Table.Name] = EDIMessageSchema.Constants.TableName;
			row[StmNoteSchema.ST_Description.Name] = noteType.Description;
			row[StmNoteSchema.ST_NoteType.Name] = noteType.DefaultVisibility.ToString();

			if (noteStream.IsSwitchedToFile)
			{
				note.SetST_NoteTextSource(new StreamReaderSource(noteStream));
				note.Factory.SubscribeForDispose(noteStream);
			}
			else
			{
				row[StmNoteSchema.ST_NoteText.Name] = new StreamReader(noteStream).ReadToEnd();
				noteStream.Dispose();
			}

			return note;
		}
	}
}
