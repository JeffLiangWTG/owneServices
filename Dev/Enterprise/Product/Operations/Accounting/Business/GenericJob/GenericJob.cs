using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GenericJob
{
	[TestedAsNonPersistentBusinessObject]
	[CodeProperty(AutoViewGenericJob.Schema.VJ_JobNumber)]
	[DescriptionProperty(AutoViewGenericJob.Schema.VJ_JobType)]
	public partial class GenericJob : AutoViewGenericJob, IGenericJob, IJobInvoicingPlugIn, IBusinessObjectReload
	{
		public GenericJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		public override void Delete()
		{
			throw new NotSupportedException("Deletion of the GenericJob is not supported.");
		}

		#endregion

		#region Lookups

		public GlbBranchCollection BranchCollection
		{
			get { return FindboxLookupCollections.GetAllBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public GlbCompanyCollection CompanyCollection
		{
			get { return FindboxLookupCollections.GetCompanyCollection(Factory); }
		}

		#endregion

		#region Properties

		#region JobTypeDescription

		public ZString JobTypeDescription
		{
			get { return JobType != null ? (ZString)JobType.Description : ZString.Empty; }
		}

		public ZPropertyInfo JobTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(JobTypeDescription)); }
		}

		#endregion

		#region Job

		protected JobHeader Job
		{
			get
			{
				if (fJob == null)
				{
					ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
					filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					fJob = Factory.LoadTop1<Job>(filter);
				}
				return fJob;
			}
		}

		JobHeader fJob;

		#endregion

		#region CompanyPK

		public override ZGuid VJ_CompanyPK
		{
			get { return Job != null ? Job.JH_GC : ZGuid.Empty; }
		}

		#endregion

		#region VJ_JH

		public override ZGuid VJ_JH
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		#endregion

		#region VJ_JobStatus

		public override ZString VJ_JobStatus
		{
			get { return Job != null ? Job.JH_Status : base.VJ_JobStatus; }
		}

		#endregion

		#region VJ_JobOpenDate

		public override ZDateTime VJ_JobOpenDate
		{
			get { return Job != null ? Job.JH_A_JOP : base.VJ_JobOpenDate; }
		}

		#endregion

		#region VJ_JobCloseDate

		public override ZDateTime VJ_JobCloseDate
		{
			get { return Job != null ? Job.JH_A_JCL : base.VJ_JobCloseDate; }
		}

		#endregion

		#region VJ_Company

		public override ZGuid VJ_Company
		{
			get { return Job != null ? Job.JH_GC : ZGuid.Empty; }
		}

		#endregion

		#region VJ_Branch

		public override ZGuid VJ_Branch
		{
			get { return Job != null ? Job.JH_GB : ZGuid.Empty; }
		}

		#endregion

		#region VJ_Department

		public override ZGuid VJ_Department
		{
			get { return Job != null ? Job.JH_GE : ZGuid.Empty; }
		}

		#endregion

		#region VJ_ForeignKey

		public override ZGuid VJ_ForeignKey
		{
			get { return Job != null ? Job.JH_ParentID : ZGuid.Empty; }
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GenericJobFetchStrategy(this);
		}

		#endregion

		#region Job Consumer

		#region Consumer Type

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public JobInvoicingConsumerType JobType
		{
			get
			{
				if (fJobType == null)
				{
					fJobType = JobInvoicingConsumerTypes.New()[VJ_JobType];

					if (fJobType == null)
					{
						ErrorReporter.ReportOnce($"There is no entry in JobInvoicingConsumerTypes for job type: '{VJ_JobType}'. Please read the comment above JobInvoicingConsumerTypes for instructions on adding an entry.");
					}
				}

				return fJobType;
			}
		}

		JobInvoicingConsumerType fJobType;

		#endregion

		#region Consumer Business Object Type

		public Type GetConsumerType()
		{
			return JobType != null ? JobType.BizoType : null;
		}

		#endregion

		#region Consumer Controller ID

		public ControllerID GetConsumerController()
		{
			return JobType != null ? JobType.ControllerID : null;
		}

		#endregion

		#region Consumer Object

		public IJobInvoicingPlugIn Consumer
		{
			get
			{
				Type consumerType = GetConsumerType();

				IJobInvoicingPlugIn result = null;
				if (consumerType != null)
				{
					result = Factory.Load(consumerType, PK) as IJobInvoicingPlugIn;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region IJobHeaderParent

		public new string TableName
		{
			get { return VJ_TableName; }
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

		bool IJobHeaderParent.IsDeleted
		{
			get { return this.IsDeleted; }
		}

		#endregion

		public string JobNumber
		{
			get { return VJ_JobNumber; }
		}

		public void SetJobNumberFieldOnSaving()
		{
			Consumer.SetJobNumberFieldOnSaving();
		}

		#region IJobInvoicingPlugIn Members

		GenericJobInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new GenericJobInvoicingSupporter(this)); }
		}

		#endregion

		#region IBusinessObjectReload

		BusinessObject IBusinessObjectReload.Reload(BusinessObjectFactory loadingFactory)
		{
			var query = new ZQuery(ViewGenericJobSchema.PK, PK);
			query.AddToFilter(ViewGenericJobSchema.VJ_TableName, VJ_TableName);
			return loadingFactory.LoadTop1(GetType(), query);
		}

		public SchemaColumn ReloadPerformanceIncreaseColumn => ViewGenericJobSchema.VJ_TableName;

		public object ReloadPerformanceIncreaseColumnValue => VJ_TableName;

#endregion
	}

	class GenericJobInvoicingSupporter : JobInvoicingSupporter
	{
		public GenericJobInvoicingSupporter(GenericJob parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly GenericJob parent;

		public override OrgHeader Consignee
		{
			get { return parent.Consumer.InvoicingSupporter.Consignee; }
		}

		public override RefUNLOCO Destination
		{
			get { return parent.Consumer.InvoicingSupporter.Destination; }
		}

		public override ZString ConsolType
		{
			get { return parent.Consumer.InvoicingSupporter.ConsolType; }
		}

		public override ZString ConsolNumber
		{
			get { return parent.Consumer.InvoicingSupporter.ConsolNumber; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return parent.Consumer.InvoicingSupporter.ConsolRateCurrency; }
		}

		public override ZDecimal ActualChargeable
		{
			get { return parent.Consumer.InvoicingSupporter.ActualChargeable; }
		}

		public override ZString TransportMode
		{
			get { return parent.Consumer.InvoicingSupporter.TransportMode; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return parent.Consumer.InvoicingSupporter.ConsumerType; }
		}

		public override bool IsDirectShipment
		{
			get { return parent.Consumer.InvoicingSupporter.IsDirectShipment; }
		}

		public override OrgHeader SendingAgent
		{
			get { return parent.Consumer.InvoicingSupporter.SendingAgent; }
		}

		public override ZString ContainerMode
		{
			get { return parent.Consumer.InvoicingSupporter.ContainerMode; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return parent.Consumer.InvoicingSupporter.PaymentTerm; }
		}

		public override RefUNLOCO Origin
		{
			get { return parent.Consumer.InvoicingSupporter.Origin; }
		}

		public override bool IsPlugInReadOnly
		{
			get { return parent.Consumer.InvoicingSupporter.IsPlugInReadOnly; }
		}

		public override RefUNLOCO GetTranshipmentPort(CostSell costOrSell)
		{
			return parent.Consumer.InvoicingSupporter.GetTranshipmentPort(costOrSell);
		}

		public override OrgHeader ReceivingAgent
		{
			get { return parent.Consumer.InvoicingSupporter.ReceivingAgent; }
		}

		public override bool IsImport
		{
			get { return parent.Consumer.InvoicingSupporter.IsImport; }
		}

		public override bool IsExport
		{
			get { return parent.Consumer.InvoicingSupporter.IsExport; }
		}

		public override bool IsDomestic
		{
			get { return parent.Consumer.InvoicingSupporter.IsDomestic; }
		}

		public override ZDecimal ConsolExchangeRate
		{
			get { return parent.Consumer.InvoicingSupporter.ConsolExchangeRate; }
		}

		public override OrgHeader Consignor
		{
			get { return parent.Consumer.InvoicingSupporter.Consignor; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return parent.Consumer.InvoicingSupporter.GetDefaultCreditor(defaultCreditorSetting);
		}

		public override OrgHeader Broker
		{
			get { return parent.Consumer.InvoicingSupporter.Broker; }
		}

		public override ZString ShipmentNumberOfColoadMaster
		{
			get { return parent.Consumer.InvoicingSupporter.ShipmentNumberOfColoadMaster; }
		}

		public override ZString MasterBillNumber
		{
			get { return parent.Consumer.InvoicingSupporter.MasterBillNumber; }
		}

		public override ZString HouseBillNumber
		{
			get { return parent.Consumer.InvoicingSupporter.HouseBillNumber; }
		}

		public override ZDateTime ATA
		{
			get { return parent.Consumer.InvoicingSupporter.ATA; }
		}

		public override ZDateTime ATD
		{
			get { return parent.Consumer.InvoicingSupporter.ATD; }
		}

		public override ZDateTime ETA
		{
			get { return parent.Consumer.InvoicingSupporter.ETA; }
		}

		public override ZDateTime ETD
		{
			get { return parent.Consumer.InvoicingSupporter.ETD; }
		}

		public override ZDateTime ArrivalAtLoadPort
		{
			get { return parent.Consumer.InvoicingSupporter.ArrivalAtLoadPort; }
		}

		public override ZDateTime EstimatedArrivalAtLoadPort
		{
			get { return parent.Consumer.InvoicingSupporter.EstimatedArrivalAtLoadPort; }
		}

		public override ZDecimal ActualWeight
		{
			get { return parent.Consumer.InvoicingSupporter.ActualWeight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return parent.Consumer.InvoicingSupporter.ActualWeightUnit; }
		}

		public override ZDecimal ActualVolume
		{
			get { return parent.Consumer.InvoicingSupporter.ActualVolume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return parent.Consumer.InvoicingSupporter.ActualVolumeUnit; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return parent.Consumer.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return parent.Consumer.InvoicingSupporter.OperationsBranch; }
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return parent.Consumer.InvoicingSupporter.GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return parent.Consumer.InvoicingSupporter.GetOperationsSignificantDateByDirection(significantDateCode, direction);
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return parent.Consumer.InvoicingSupporter.EditSecurityCheckpoint;
		}

		public override ZString EditSecurityMessage
		{
			get { return parent.Consumer.InvoicingSupporter.EditSecurityMessage; }
		}

		public override bool EditSecurityLock
		{
			get { return parent.Consumer.InvoicingSupporter.EditSecurityLock; }
		}

		public override int ContainerCount
		{
			get { return parent.Consumer.InvoicingSupporter.ContainerCount; }
		}

		public override ZDecimal TEUCount
		{
			get { return parent.Consumer.InvoicingSupporter.TEUCount; }
		}

		public override int OuterPackTotal
		{
			get { return parent.Consumer.InvoicingSupporter.OuterPackTotal; }
		}
	}
}
