using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.DummyEnterpriseBusinessObject)]
	public class DummyEnterpriseBusinessObject : DummyBusinessObject, IStmNoteParent, IStmALogParent, IAutoAdminLogTarget, IUniversalXMLNoteParent, IDataVersionLoggingSupported
	{
		public DummyEnterpriseBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static DummyEnterpriseBusinessObject New(BusinessObjectFactory factory)
		{
			return factory.New<DummyEnterpriseBusinessObject>();
		}

		public new DummyChildEnterpriseBusinessObjectCollection Collection
		{
			get { return (DummyChildEnterpriseBusinessObjectCollection)base.Collection; }
		}

		protected override DummyChildBusinessObjectCollection NewCollection()
		{
			return new DummyChildEnterpriseBusinessObjectCollection(Factory);
		}

		[List("Lookups.DummyList")]
		public ZGuid Z0_GuidWithListAttribute
		{
			get { return Z0_Guid; }
			set { Z0_Guid = value; }
		}

		public ZPropertyInfo Z0_GuidWithListAttributeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Z0_GuidWithListAttribute), x => Z0_GuidInfo); }
		}

		#region Lookups

		public DummyLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new DummyLookups(this);
				}
				return lookups;
			}
		}
		DummyLookups lookups;

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

		[BusinessObjectTestExclude]
		public GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate { get; set; }

		public virtual bool SupportsNotes
		{
			get { return true; }
		}

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get { return BusinessObjectsWithRelatedNotes; }
		}

		protected virtual BusinessObject[] BusinessObjectsWithRelatedNotes
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
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();
				result.Add(NoteTypesCore);
				if (CustomNoteTypes != null)
				{
					result.Add(CustomNoteTypes);
				}
				return result;
			}
		}

		NoteTypeCollection CustomNoteTypes
		{
			get
			{
				if (customNoteTypes == null && CustomNoteTypesDelegate != null)
				{
					customNoteTypes = CustomNoteTypesDelegate();
				}
				return customNoteTypes;
			}
		}
		NoteTypeCollection customNoteTypes;

		protected virtual NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();
				result.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
				return result;
			}
		}

		#endregion

		#region IStmALogParent

		public Logs Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}
		Logs logs;

		public BusinessObjectFactory LogsFactory
		{
			get { return Factory; }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return BusinessObjectsWithRelatedEvents; }
		}

		protected virtual BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return BusinessObjectsWithRelatedEventsForTest == null ? System.Array.Empty<BusinessObject>() : BusinessObjectsWithRelatedEventsForTest(); }
		}

		public delegate BusinessObject[] GetBusinessObjectsWithRelatedEventsDelegate();

		public GetBusinessObjectsWithRelatedEventsDelegate BusinessObjectsWithRelatedEventsForTest;

		void IStmALogParent.ProcessLog(IStmALog log) => ProcessLogCore(log);

		protected virtual void ProcessLogCore(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region IAutoAdminLogTarget Members

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			return new IBusinessObjectStrategy[] { new BusinessObjectLoggingStrategy(this) };
		}

		protected virtual EnterpriseBusinessObject.AutologState AutoLoggingState =>
			EnterpriseBusinessObject.AutologState.NotLogged;
		public bool IsAutoLogged => AutoLoggingState == EnterpriseBusinessObject.AutologState.AutoLogged;

		bool IAutoLog.IsAutoLogOnlyEnabledForACT => IsAutoLogOnlyEnabledForACT;

		protected virtual bool IsAutoLogOnlyEnabledForACT => false;

		void IAutoAdminLogTarget.OnCreateAutoAdminLog()
		{
			OnCreateAutoAdminLog();
		}

		protected virtual void OnCreateAutoAdminLog()
		{
		}

		public bool IsAutoAdminBusinessObjectLoggerEnabled => ObjectFactory.Get<IAuditStmALogDecider>()
			.IsAutoAdminBusinessObjectLoggerEnabled(this, AutoLoggingState);

		ZString IAutoAdminLogTarget.CustomLogReferenceSuffix
		{
			get { return CustomLogReferenceSuffix; }
		}

		protected virtual ZString CustomLogReferenceSuffix
		{
			get { return ""; }
		}

		bool IAutoAdminLogTarget.ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return ShouldCreateAutoLogIfOnlyChildrenHaveChanges; }
		}

		protected virtual bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return IsTopLevel && IsAutoLogged; }
		}

		#region IsDataVersionsAutoLogged

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return this.IsDataVersionsAutoLogged; }
		}

		public virtual bool IsDataVersionsAutoLogged
		{
			get; set;
		}
		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter
		{
			get { return this.GetDefaultDataVersionLogFormatter(); }
		}

		#endregion

		#endregion
	}
}
