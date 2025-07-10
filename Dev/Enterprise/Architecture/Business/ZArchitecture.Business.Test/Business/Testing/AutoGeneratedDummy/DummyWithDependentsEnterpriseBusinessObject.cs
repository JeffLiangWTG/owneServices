using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyWithDependentsEnterpriseBusinessObject : DummyWithDependentsBusinessObject, IStmNoteParent, IStmALogParent
	{
		public DummyWithDependentsEnterpriseBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IStmNoteParent

		[BusinessObjectTestExclude]
		public virtual Notes Notes
		{
			get { return notes ?? (notes = new Notes(this)); }
		}
		Notes notes;

		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return PK; }
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return TableName; }
		}

		protected virtual BusinessObjectFactory NotesFactory
		{
			get { return Factory; }
		}

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return NotesFactory; }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate { get; set; }

		public virtual bool SupportsNotes
		{
			get { return true; }
		}

		public virtual BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get { return Array.Empty<EnterpriseBusinessObject>(); }
		}

		protected virtual StmNoteContexts NoteContextsForRelatedNotes
		{
			get { return StmNoteContexts.Default; }
		}

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return NoteContextsForRelatedNotes; }
		}

		public NoteTypeCollection NoteTypes
		{
			get { return NoteTypesCore; }
		}

		protected virtual NoteTypeCollection NoteTypesCore
		{
			get { return new NoteTypeCollection(); }
		}

		#endregion

		#region IStmALogParent

		Logs IStmALogProvider.Logs
		{
			get { throw new NotImplementedException(); }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion
	}
}
