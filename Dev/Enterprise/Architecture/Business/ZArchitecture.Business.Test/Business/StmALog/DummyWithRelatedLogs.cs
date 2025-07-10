using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[PreventDelete(true)]
	class DummyWithRelatedLogs : DummyBaseBusinessObject, IStmALogParent, IStmNoteParent, IAutoAdminLogTarget, ICancellable, IDataVersionLoggingSupported
	{
		public DummyWithRelatedLogs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			isCancelled = ZBool.False;
		}

		public int LogsCount
		{
			get { return Logs.ElementsInternal.Count; }
		}

		public bool IsElementsLoaded
		{
			get { return Logs.IsElementsLoaded; }
		}

		public bool IsAllElementsLoaded
		{
			get { return (bool)Logs.GetType().GetProperty("IsAllElementsLoaded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this.Logs, null); }
		}

		public StmNoteWithCustomHumanReadableName Note
		{
			get { return fNote; }
		}

		public IGlbStaff Staff
		{
			get { return fStaff; }
		}

		public void CreateNoteInDB()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			fNote = newFactory.New<StmNoteWithCustomHumanReadableName>();
			fNote.ST_Table = AutoDummyBizo.Schema.TableName;
			newFactory.Save(); // DB trigger should now add an "ADD" event for this note
		}

		public void CreateStaffInDB()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			fStaff = newFactory.New<IGlbStaff>();
			((BusinessObject)fStaff)[GlbStaffSchema.GS_Code.Name] = "ZAC";
			newFactory.Save(); // DB trigger should now add an "ADD" event for this staff
		}

		public void SetIsCancelled(ZBool value)
		{
			if (isCancelled != value)
			{
				isCancelled = value;
				isCancelledHasChanged = true;
				HasChanges = true;
			}
		}

		public bool IsCancelledHasChanged
		{
			get { return isCancelledHasChanged; }
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				isCancelledHasChanged = false;
			}
		}

		public ZString NonPersistentPropertyWithGetterOnly
		{
			get { return NonPersistentProperty; }
		}

		public ZPropertyInfo NonPersistentPropertyWithGetterOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(NonPersistentPropertyWithGetterOnly)); }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#region Logging

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			return new[] { new BusinessObjectLoggingStrategy(this) };
		}

		#endregion

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
			get { return System.Array.Empty<EnterpriseBusinessObject>(); }
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

		public Logs Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}
		Logs logs;

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return alternateLogsParentTableName ?? TableName; }
		}

		public string AlternateLogsParentTableName
		{
			get => alternateLogsParentTableName;
			set => alternateLogsParentTableName = value;
		}

		string alternateLogsParentTableName;

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		public virtual BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return new BusinessObject[] { Note, (EnterpriseBusinessObject)Staff }; }
		}

		#endregion

		#region IAutoAdminLogTarget Members

		public bool IsAutoLogged => AutoLoggingState == EnterpriseBusinessObject.AutologState.AutoLogged;

		protected EnterpriseBusinessObject.AutologState AutoLoggingState =
			EnterpriseBusinessObject.AutologState.NotLogged;

		public bool IsAutoLogOnlyEnabledForACT => false;
		bool IAutoLog.IsAutoLogOnlyEnabledForACT => IsAutoLogOnlyEnabledForACT;

		public bool IsAutoAdminBusinessObjectLoggerEnabled
			=> ObjectFactory.Get<IAuditStmALogDecider>()
				.IsAutoAdminBusinessObjectLoggerEnabled(this, AutoLoggingState);

		void IAutoAdminLogTarget.OnCreateAutoAdminLog()
		{
		}

		ZString IAutoAdminLogTarget.CustomLogReferenceSuffix
		{
			get { return ""; }
		}

		bool IAutoAdminLogTarget.ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return IsTopLevel && IsAutoLogged; }
		}

		#endregion

		#region IDataVersionLoggingSupported Members

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return this.IsDataVersionsAutoLogged; }
		}

		public bool IsDataVersionsAutoLogged;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter
		{
			get { return this.GetDefaultDataVersionLogFormatter(); }
		}

		#endregion

		#region ICancellable Members

		bool ICancellable.IsCancelled
		{
			get
			{
				return isCancelled;
			}
			set
			{
				isCancelled = value;
			}
		}

		string ICancellable.CanCancel()
		{
			return ZBool.True.ToString();
		}

		string ICancellable.CanReactivate()
		{
			return ZBool.True.ToString();
		}

		#endregion

		#region Implementation

		StmNoteWithCustomHumanReadableName fNote;
		IGlbStaff fStaff;
		ZBool isCancelled;
		ZBool isCancelledHasChanged;

		#endregion
	}
}
