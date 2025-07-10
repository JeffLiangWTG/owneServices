using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	public static class IEDIMessageExtensions
	{
		public static string GetLogNoteText(this IEDIMessage message)
		{
			var logNoteDescription = PredefinedNoteTypes.Instance.DataImportLogNote.Description;
			var logNotes = ((BusinessObject)message).GetNotes().FindByDescription(logNoteDescription);
			switch (logNotes.Length)
			{
				case 0:
					return "No Notes Found matching [" + logNoteDescription + "]";
				case 1:
					using (var reader = logNotes[0].GetST_NoteTextReader())
					{
						return reader.ReadToEnd();
					}
				default:
					return "Expected 1 but Found " + logNotes.Length.ToString() + " Notes matching [" + logNoteDescription + "]";
			}
		}
	}
}
