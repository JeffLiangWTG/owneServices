using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyBODocSupportable : DummyWithWorkflow, IDocumentSupportable, IStmALogParent, IStmNoteParent, IDocManagerSupport
	{
		public DummyBODocSupportable(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		DocManagerInfo docManagerInfo;

		public ZString[] ZStringCollection => ZStringList.ToArray();
		public List<ZString> ZStringList { get; set; } = new List<ZString>();

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DummyBODocSupportableDocumentSupporter(this); }
		}

		#endregion

		#region IStmALogParent members
		public ZGuid LogsParentPK { get { return this.PK; } }
		public string LogsParentTableName { get { return DummyBusinessObject.Schema.TableName; } }
		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region IStmNoteParent Members

		Notes IStmNoteParent.Notes
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

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return Factory; }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate
		{
			get { return customNoteTypesDelegate; }
			set { customNoteTypesDelegate = value; }
		}
		GetValueDelegate<NoteTypeCollection> customNoteTypesDelegate;

		bool IStmNoteParent.SupportsNotes
		{
			get { return true; }
		}

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get { return System.Array.Empty<BusinessObject>(); }
		}

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return StmNoteContexts.Default; }
		}

		NoteTypeCollection IStmNoteParent.NoteTypes
		{
			get { return new NoteTypeCollection(); }
		}

		#endregion

		public virtual DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.Shipment)); }
		}
	}
}
