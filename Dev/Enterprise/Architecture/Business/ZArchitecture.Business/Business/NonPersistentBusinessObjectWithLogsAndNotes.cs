using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class NonPersistentBusinessObjectWithLogsAndNotes : NonPersistentBusinessObject, IStmALogParent, IStmNoteParent
	{
		public NonPersistentBusinessObjectWithLogsAndNotes()
		{
		}

		public NonPersistentBusinessObjectWithLogsAndNotes(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected NonPersistentBusinessObjectWithLogsAndNotes(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Target

		protected abstract BusinessObject LogsAndNotesTarget { get; }

		IStmALogParent LogsTarget
			=> LogsAndNotesTarget as IStmALogParent
					?? throw new NotImplementedException();

		IStmNoteParent NotesTarget
			=> LogsAndNotesTarget as IStmNoteParent
					?? throw new NotImplementedException();

		#endregion

		#region IStmALogParent Members

		public Logs Logs
		{
			get { return LogsTarget.Logs; }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return LogsTarget.LogsParentPK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return LogsTarget.LogsParentTableName; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return LogsTarget.LogsFactory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return LogsTarget.BusinessObjectsWithRelatedEvents; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region IStmNoteParent Members

		public Notes Notes
		{
			get { return NotesTarget.Notes; }
		}

		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return NotesTarget.NotesParentPK; }
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return NotesTarget.NotesParentTableName; }
		}

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return NotesTarget.NotesFactory; }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate { get; set; }

		bool IStmNoteParent.SupportsNotes
		{
			get { return NotesTarget.SupportsNotes; }
		}

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get { return NotesTarget.BusinessObjectsWithRelatedNotes; }
		}

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return NoteContextsForRelatedNotes; }
		}

		protected virtual StmNoteContexts NoteContextsForRelatedNotes
		{
			get { return NotesTarget.NoteContextsForRelatedNotes; }
		}

		NoteTypeCollection IStmNoteParent.NoteTypes
		{
			get { return NotesTarget.NoteTypes; }
		}

		#endregion
	}
}
