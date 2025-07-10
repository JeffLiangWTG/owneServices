using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Utility.Testing
{
	public class InvoicingParam : NonPersistentBusinessObject, IImportExport, IJobInvoicingPlugIn, IStmALogParent, IRatingSupporter
	{
		public InvoicingParam()
			: base(new BusinessObjectFactory())
		{
		}

		public string[] ErrorsIncludingChildren
		{
			get { return null; }
		}

		public new ZBool IsInDatabase
		{
			get { return isInDatabase; }
			set { isInDatabase = value; }
		}
		ZBool isInDatabase;

		#region IJobHeaderParent

		public new ZGuid PK
		{
			get { return pk; }
			set { pk = value; }
		}

		public bool HasStorage { get; set; }

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new InvoicingParamJobInvoicingSupporter(this)); }
		}
		InvoicingParamJobInvoicingSupporter invoicingSupporter;

		public new string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		public JobInvoicingConsumerType ConsumerType
		{
			get { return InvoicingSupporter.ConsumerType; }
			set { ((InvoicingParamJobInvoicingSupporter)InvoicingSupporter).consumerType = value; }
		}

		#endregion

		#region Implementation

		ZGuid pk;
		ZString jobNumber;
		ZString tableName;

		#endregion

		#region IStmALogParent

		Logs IStmALogProvider.Logs
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

		#region IJobNumber members

		public string JobNumber
		{
			get { return jobNumber; }
			set { jobNumber = value; }
		}

		public void SetJobNumberFieldOnSaving()
		{
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new InvoicingParamRatingAdaptersProvider(this); }
		}

		#endregion

		public Directions JobDirection
		{
			get
			{
				if (InvoicingSupporter.IsImport)
				{
					return Directions.Import;
				}

				if (InvoicingSupporter.IsExport)
				{
					return Directions.Export;
				}

				if (InvoicingSupporter.IsDomestic)
				{
					return Directions.Domestic;
				}

				return Directions.Unknown;
			}
		}

		public ZBool IsImport
		{
			set { ((InvoicingParamJobInvoicingSupporter)InvoicingSupporter).fIsImport = value; }
		}
	}

	public class InvoicingParamJobInvoicingSupporter : JobInvoicingSupporter
	{
		public InvoicingParamJobInvoicingSupporter(IJobHeaderParent parent) : base(parent) { }

		public JobInvoicingConsumerType consumerType = new TestJobInvoicingConsumerType();

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.MaintainShipmentJobInvoicing;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return consumerType; }
		}

		public bool createAccountingJobOnSavingOfOperationsJob;
		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return createAccountingJobOnSavingOfOperationsJob; }
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		public ZString fMasterBillNumber;
		public override ZString MasterBillNumber
		{
			get
			{
				return fMasterBillNumber;
			}
		}

		public ZString fHouseBillNumber;
		public override ZString HouseBillNumber
		{
			get
			{
				return fHouseBillNumber;
			}
		}

		public RefUNLOCO fOrigin;
		public override RefUNLOCO Origin
		{
			get
			{
				return fOrigin;
			}
		}

		public RefUNLOCO fDestination;
		public override RefUNLOCO Destination
		{
			get
			{
				return fDestination;
			}
		}

		public ZString fTransportMode;
		public override ZString TransportMode
		{
			get
			{
				return fTransportMode;
			}
		}

		public OrgHeader fConsignee;
		public override OrgHeader Consignee
		{
			get
			{
				return fConsignee;
			}
		}

		public OrgHeader fConsignor;
		public override OrgHeader Consignor
		{
			get
			{
				return fConsignor;
			}
		}

		public bool fIsImport;
		public override bool IsImport
		{
			get
			{
				return fIsImport;
			}
		}
	}

	public class InvoicingParamWithoutIAutoRating : NonPersistentBusinessObject, IJobInvoicingPlugIn
	{
		public InvoicingParamWithoutIAutoRating()
			: base(new BusinessObjectFactory())
		{
		}

		public string[] ErrorsIncludingChildren
		{
			get { return null; }
		}

		#region IJobHeaderParent

		public new ZGuid PK
		{
			get { return fPK; }
			set { fPK = value; }
		}

		public new string TableName
		{
			get { return fTableName; }
			set { fTableName = value; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber members

		public string JobNumber
		{
			get { return fJobNumber; }
			set { fJobNumber = value; }
		}

		public void SetJobNumberFieldOnSaving()
		{
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		InvoicingParamWithoutIAutoRatingJobInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new InvoicingParamWithoutIAutoRatingJobInvoicingSupporter(this)); }
		}

		#endregion

		#region Implementation

		protected ZGuid fPK;
		protected ZString fJobNumber;
		protected ZString fTableName;

		#endregion
	}

	public class InvoicingParamWithoutIAutoRatingJobInvoicingSupporter : JobInvoicingSupporter
	{
		public InvoicingParamWithoutIAutoRatingJobInvoicingSupporter(IJobHeaderParent parent) : base(parent) { }

		public OrgHeader fConsignee;
		public override OrgHeader Consignee
		{
			get
			{
				return fConsignee;
			}
		}

		public OrgHeader fConsignor;
		public override OrgHeader Consignor
		{
			get
			{
				return fConsignor;
			}
		}

		public OrgHeader fDefaultCreditor;
		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return fDefaultCreditor;
		}

		public RefCurrency fConsolRateCurrency;
		public override RefCurrency ConsolRateCurrency
		{
			get
			{
				return fConsolRateCurrency;
			}
		}

		public JobInvoicingConsumerType fConsumerType;
		public override JobInvoicingConsumerType ConsumerType
		{
			get
			{
				return fConsumerType;
			}
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}
	}

	class InvoicingParamRatingAdaptersProvider : RatingAdaptersProvider<InvoicingParam>
	{
		public InvoicingParamRatingAdaptersProvider(InvoicingParam parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(InvoicingParam parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating>() { new InvoicingParamRatingAdapter<InvoicingParam>(parent) };
		}
	}

	class InvoicingParamRatingAdapter<T> : RatingAdapter
		where T : InvoicingParam
	{
		public InvoicingParamRatingAdapter(InvoicingParam parent)
		{
			this.parent = parent;
		}

		readonly InvoicingParam parent;

		#region RatingAdapter

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<InvoicingParam>(parent); }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var jobServices = new JobServicesCollection();
				jobServices.Add(new JobServiceInfo(parent.HasStorage, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage, "Storage"));
				return jobServices;
			}
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return parent.ConsumerType; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return parent.InvoicingSupporter; }
		}

		public override Directions JobDirection
		{
			get { return parent.JobDirection; }
		}

		public override ZString JobID => "JobId";

		#endregion
	}

	class TestJobInvoicingConsumerType : JobInvoicingConsumerType
	{
		public TestJobInvoicingConsumerType()
			: base("DUM", (NoResString)"DUM")
		{
		}

		public override Type BizoType
		{
			get { return typeof(DummyBusinessObject); }
		}

		public override ControllerID ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
