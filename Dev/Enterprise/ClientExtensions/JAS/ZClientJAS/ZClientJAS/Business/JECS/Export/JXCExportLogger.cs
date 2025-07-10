using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class JXCExportLogger : INotifications
	{
		public JXCExportLogger(IBusiness exportSource)
		{
			this.ExportSource = exportSource;
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			BusinessObjectFactory noteFactory = new BusinessObjectFactory();
			BusinessObject bizO = noteFactory.Load(ExportSource.GetType(), ExportSource.Identifier);

			if (bizO != null)
			{
				StmNote[] exportLogNotes = bizO.GetNotes().FindByDescription(JASPredefinedNoteTypes.Instance.JXCExportLog.Description);
				StmNote exportLogNote;

				if (exportLogNotes.Length > 0)
				{
					exportLogNote = exportLogNotes[0];
				}
				else
				{
					exportLogNote = bizO.GetNotes().AddNew(false, JASPredefinedNoteTypes.Instance.JXCExportLog.Description, "");
					exportLogNote.ST_NoteType = nameof(StmNoteVisibility.INT);
				}

				StringBuilder stringBuilder = new StringBuilder(exportLogNote.ST_NoteText);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("\r\n");
				}
				stringBuilder.AppendFormat("{0:dd-MMM-yyyy HH:mm:ss}     {1}", ZDateTime.Now, notification.Message);
				exportLogNote.ST_NoteText = stringBuilder.ToString();

				noteFactory.Save();
			}
		}

		#endregion

		public readonly IBusiness ExportSource;
	}
}

#region Implementation
#endregion
