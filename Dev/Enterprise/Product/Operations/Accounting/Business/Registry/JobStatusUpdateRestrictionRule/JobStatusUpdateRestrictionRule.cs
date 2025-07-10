using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Registry.Business.JobStatusUpdateRestrictionRuleLookups;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobStatusUpdateRestrictionRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobStatus = "JobStatus";
			public const string Working = "Working";
			public const string WorkOnHold = "WorkOnHold";
			public const string InvoiceOnHold = "InvoiceOnHold";
			public const string CustomsProcessActive = "CustomsProcessActive";
			public const string JobReadyForRevenueAndCostPosting = "JobReadyForRevenueAndCostPosting";
			public const string JobReadyForRevenuePosting = "JobReadyForRevenuePosting";
			public const string JobReadyForCostPosting = "JobReadyForCostPosting";
			public const string JobReadyForDelivery = "JobReadyForDelivery";
			public const string JobInvoiced = "JobInvoiced";
			public const string Complete = "Complete";
			public const string JobReadyForFinancialClosure = "JobReadyForFinancialClosure";
			public const string ScheduledForArchive = "ScheduledForArchive";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobStatusUpdateRestrictionRule();
		}

		#region Properties

		#region JobStatus

		public ZString JobStatus
		{
			get { return fJobStatus; }
			set { SetNonPersistentPropertyValue(JobStatusInfo, ref fJobStatus, value); }
		}
		ZString fJobStatus;

		public ZPropertyInfo JobStatusInfo => GetZPropertyInfo(nameof(JobStatus));

		#endregion

		#region JobStatusDescription

		public ZString JobStatusDescription => string.Format(CultureInfo.InvariantCulture, "{0} ({1})", JobStatus, Lookups.JobStatusList.GetDescriptionFromCode(JobStatus));

		#endregion

		#region RelatedSecurityRight

		public SecurityCheckpoint RelatedSecurityRight => GetRelatedSecurityRight(JobStatus);

		#endregion

		#region RelatedSecurityRightDescription

		public ZString RelatedSecurityRightDescription => RelatedSecurityRight?.DisplayText;

		#endregion

		#region Working

		[List("Lookups.YesOrNoList")]
		public ZString Working
		{
			get { return fWorking; }
			set
			{
				SetNonPersistentPropertyValue(WorkingInfo, ref fWorking, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWorking();
				}
			}
		}
		ZString fWorking;

		public ZPropertyInfo WorkingInfo => GetZPropertyInfo(nameof(Working));

		protected bool Working_ReadOnly => JobStatus == JobHeaderStatus.Codes.Working;

		#endregion

		#region WorkOnHold

		[List("Lookups.YesOrNoList")]
		public ZString WorkOnHold
		{
			get { return fWorkOnHold; }
			set
			{
				SetNonPersistentPropertyValue(WorkOnHoldInfo, ref fWorkOnHold, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWorkOnHold();
				}
			}
		}
		ZString fWorkOnHold;

		public ZPropertyInfo WorkOnHoldInfo => GetZPropertyInfo(nameof(WorkOnHold));

		protected bool WorkOnHold_ReadOnly => JobStatus == JobHeaderStatus.Codes.WorkOnHold;

		#endregion

		#region InvoiceOnHold

		[List("Lookups.YesOrNoList")]
		public ZString InvoiceOnHold
		{
			get { return fInvoiceOnHold; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceOnHoldInfo, ref fInvoiceOnHold, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInvoiceOnHold();
				}
			}
		}
		ZString fInvoiceOnHold;

		public ZPropertyInfo InvoiceOnHoldInfo => GetZPropertyInfo(nameof(InvoiceOnHold));

		protected bool InvoiceOnHold_ReadOnly => JobStatus == JobHeaderStatus.Codes.InvoiceOnHold;

		#endregion

		#region CustomsProcessActive

		[List("Lookups.YesOrNoList")]
		public ZString CustomsProcessActive
		{
			get { return fCustomsProcessActive; }
			set
			{
				SetNonPersistentPropertyValue(CustomsProcessActiveInfo, ref fCustomsProcessActive, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsProcessActive();
				}
			}
		}
		ZString fCustomsProcessActive;

		public ZPropertyInfo CustomsProcessActiveInfo => GetZPropertyInfo(nameof(CustomsProcessActive));

		protected bool CustomsProcessActive_ReadOnly => JobStatus == JobHeaderStatus.Codes.CustomsProcessActive;

		#endregion

		#region JobReadyForRevenuePosting

		[List("Lookups.YesOrNoList")]
		public ZString JobReadyForRevenuePosting
		{
			get { return fJobReadyForRevenuePosting; }
			set
			{
				SetNonPersistentPropertyValue(JobReadyForRevenuePostingInfo, ref fJobReadyForRevenuePosting, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobReadyForRevenuePosting();
				}
			}
		}
		ZString fJobReadyForRevenuePosting;

		public ZPropertyInfo JobReadyForRevenuePostingInfo => GetZPropertyInfo(nameof(JobReadyForRevenuePosting));

		protected bool JobReadyForRevenuePosting_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobReadyForRevenuePosting;

		#endregion

		#region JobReadyForCostPosting

		[List("Lookups.YesOrNoList")]
		public ZString JobReadyForCostPosting
		{
			get { return fJobReadyForCostPosting; }
			set
			{
				SetNonPersistentPropertyValue(JobReadyForCostPostingInfo, ref fJobReadyForCostPosting, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobReadyForCostPosting();
				}
			}
		}
		ZString fJobReadyForCostPosting;

		public ZPropertyInfo JobReadyForCostPostingInfo => GetZPropertyInfo(nameof(JobReadyForCostPosting));

		protected bool JobReadyForCostPosting_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobReadyForCostPosting;

		#endregion

		#region JobReadyForRevenueAndCostPosting

		[List("Lookups.YesOrNoList")]
		public ZString JobReadyForRevenueAndCostPosting
		{
			get { return fJobReadyForRevenueAndCostPosting; }
			set
			{
				SetNonPersistentPropertyValue(JobReadyForRevenueAndCostPostingInfo, ref fJobReadyForRevenueAndCostPosting, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobReadyForRevenueAndCostPosting();
				}
			}
		}
		ZString fJobReadyForRevenueAndCostPosting;

		public ZPropertyInfo JobReadyForRevenueAndCostPostingInfo => GetZPropertyInfo(nameof(JobReadyForRevenueAndCostPosting));

		protected bool JobReadyForRevenueAndCostPosting_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting;

		#endregion

		#region JobInvoiced

		[List("Lookups.YesOrNoList")]
		public ZString JobInvoiced
		{
			get { return fJobInvoiced; }
			set
			{
				SetNonPersistentPropertyValue(JobInvoicedInfo, ref fJobInvoiced, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobInvoiced();
				}
			}
		}
		ZString fJobInvoiced;

		public ZPropertyInfo JobInvoicedInfo => GetZPropertyInfo(nameof(JobInvoiced));

		protected bool JobInvoiced_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobInvoiced;

		#endregion

		#region JobReadyForDelivery

		[List("Lookups.YesOrNoList")]
		public ZString JobReadyForDelivery
		{
			get { return fJobReadyForDelivery; }
			set
			{
				SetNonPersistentPropertyValue(JobReadyForDeliveryInfo, ref fJobReadyForDelivery, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobReadyForDelivery();
				}
			}
		}
		ZString fJobReadyForDelivery;

		public ZPropertyInfo JobReadyForDeliveryInfo => GetZPropertyInfo(nameof(JobReadyForDelivery));

		protected bool JobReadyForDelivery_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobReadyForDelivery;

		#endregion

		#region Complete

		[List("Lookups.YesOrNoList")]
		public ZString Complete
		{
			get { return fComplete; }
			set
			{
				SetNonPersistentPropertyValue(CompleteInfo, ref fComplete, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateComplete();
				}
			}
		}
		ZString fComplete;

		public ZPropertyInfo CompleteInfo => GetZPropertyInfo(nameof(Complete));

		protected bool Complete_ReadOnly => JobStatus == JobHeaderStatus.Codes.Complete;

		#endregion

		#region JobReadyForFinancialClosure

		[List("Lookups.YesOrNoList")]
		public ZString JobReadyForFinancialClosure
		{
			get { return fJobReadyForFinancialClosure; }
			set
			{
				SetNonPersistentPropertyValue(JobReadyForFinancialClosureInfo, ref fJobReadyForFinancialClosure, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobReadyForFinancialClosure();
				}
			}
		}
		ZString fJobReadyForFinancialClosure;

		public ZPropertyInfo JobReadyForFinancialClosureInfo => GetZPropertyInfo(nameof(JobReadyForFinancialClosure));

		protected bool JobReadyForFinancialClosure_ReadOnly => JobStatus == JobHeaderStatus.Codes.JobReadyForFinancialClosure;

		#endregion

		#region ScheduledForArchive

		[List("Lookups.YesOrNoList")]
		public ZString ScheduledForArchive
		{
			get { return fScheduledForArchive; }
			set
			{
				SetNonPersistentPropertyValue(ScheduledForArchiveInfo, ref fScheduledForArchive, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateScheduledForArchive();
				}
			}
		}
		ZString fScheduledForArchive;

		public ZPropertyInfo ScheduledForArchiveInfo => GetZPropertyInfo(nameof(ScheduledForArchive));

		protected bool ScheduledForArchive_ReadOnly => JobStatus == JobHeaderStatus.Codes.ScheduledForArchive;

		#endregion

		#endregion

		#region Lookups

		public JobStatusUpdateRestrictionRuleLookups Lookups => fLookups ?? (fLookups = new JobStatusUpdateRestrictionRuleLookups(this));
		JobStatusUpdateRestrictionRuleLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public JobStatusUpdateRestrictionRuleValidation Validation => new JobStatusUpdateRestrictionRuleValidation(this);

		#endregion

		#region Xml Serialisation

		IEnumerable<string> SerialisedProperties
		{
			get
			{
				yield return Schema.JobStatus;
				yield return Schema.Working;
				yield return Schema.WorkOnHold;
				yield return Schema.InvoiceOnHold;
				yield return Schema.CustomsProcessActive;
				yield return Schema.JobReadyForRevenuePosting;
				yield return Schema.JobReadyForCostPosting;
				yield return Schema.JobReadyForRevenueAndCostPosting;
				yield return Schema.JobInvoiced;
				yield return Schema.JobReadyForDelivery;
				yield return Schema.Complete;
				yield return Schema.JobReadyForFinancialClosure;
				yield return Schema.ScheduledForArchive;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			SerialisedProperties.ForEach(prop => writer.WriteElementString(prop, this[prop].ToString()));
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			SerialisedProperties.ForEach(prop => this[prop] = wrapper.ReadElementString(prop));
		}

		#endregion

		#region Default Values

		public void SetDefaults(string jobStatus)
		{
			JobStatus = jobStatus;
			SetDefaults();
			using (GetValidationSuspender())
			{
				switch (jobStatus)
				{
					case JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting:
						SetJobReadyForRevenueAndCostPostingDefaults();
						break;

					case JobHeaderStatus.Codes.JobReadyForRevenuePosting:
						SetJobReadyForRevenuePostingDefaults();
						break;

					case JobHeaderStatus.Codes.JobReadyForCostPosting:
						SetJobReadyForCostPostingDefaults();
						break;

					case JobHeaderStatus.Codes.Complete:
						SetCompleteDefaults();
						break;

					case JobHeaderStatus.Codes.JobReadyForFinancialClosure:
						SetJobReadyForFinancialClosureDefaults();
						break;
				}
			}
		}

		#region Job Status Defaults

		void SetDefaults()
		{
			Working = WorkingInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			WorkOnHold = WorkOnHoldInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			InvoiceOnHold = InvoiceOnHoldInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			CustomsProcessActive = CustomsProcessActiveInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobReadyForRevenuePosting = JobReadyForRevenuePostingInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobReadyForCostPosting = JobReadyForCostPostingInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobReadyForRevenueAndCostPosting = JobReadyForRevenueAndCostPostingInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobInvoiced = JobInvoicedInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobReadyForDelivery = JobReadyForDeliveryInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			Complete = CompleteInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			JobReadyForFinancialClosure = JobReadyForFinancialClosureInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
			ScheduledForArchive = ScheduledForArchiveInfo.ReadOnly ? string.Empty : YesNoList.Codes.No;
		}

		void SetJobReadyForRevenueAndCostPostingDefaults()
		{
			Working = YesNoList.Codes.Yes;
			WorkOnHold = YesNoList.Codes.Yes;
			InvoiceOnHold = YesNoList.Codes.Yes;
			CustomsProcessActive = YesNoList.Codes.Yes;
			JobReadyForRevenuePosting = YesNoList.Codes.Yes;
			JobReadyForCostPosting = YesNoList.Codes.Yes;
			JobInvoiced = YesNoList.Codes.Yes;
			JobReadyForDelivery = YesNoList.Codes.Yes;
			Complete = YesNoList.Codes.Yes;
			JobReadyForFinancialClosure = YesNoList.Codes.Yes;
			ScheduledForArchive = YesNoList.Codes.Yes;
		}

		void SetJobReadyForRevenuePostingDefaults()
		{
			Working = YesNoList.Codes.Yes;
			WorkOnHold = YesNoList.Codes.Yes;
			InvoiceOnHold = YesNoList.Codes.Yes;
			CustomsProcessActive = YesNoList.Codes.Yes;
			JobReadyForCostPosting = YesNoList.Codes.Yes;
			JobInvoiced = YesNoList.Codes.Yes;
			JobReadyForDelivery = YesNoList.Codes.Yes;
			Complete = YesNoList.Codes.Yes;
			JobReadyForFinancialClosure = YesNoList.Codes.Yes;
			ScheduledForArchive = YesNoList.Codes.Yes;
		}

		void SetJobReadyForCostPostingDefaults()
		{
			Working = YesNoList.Codes.Yes;
			WorkOnHold = YesNoList.Codes.Yes;
			InvoiceOnHold = YesNoList.Codes.Yes;
			CustomsProcessActive = YesNoList.Codes.Yes;
			JobReadyForRevenuePosting = YesNoList.Codes.Yes;
			JobInvoiced = YesNoList.Codes.Yes;
			JobReadyForDelivery = YesNoList.Codes.Yes;
			Complete = YesNoList.Codes.Yes;
			JobReadyForFinancialClosure = YesNoList.Codes.Yes;
			ScheduledForArchive = YesNoList.Codes.Yes;
		}

		void SetCompleteDefaults()
		{
			Working = YesNoList.Codes.Yes;
			WorkOnHold = YesNoList.Codes.Yes;
			InvoiceOnHold = YesNoList.Codes.Yes;
			CustomsProcessActive = YesNoList.Codes.Yes;
			JobReadyForRevenueAndCostPosting = YesNoList.Codes.Yes;
			JobReadyForRevenuePosting = YesNoList.Codes.Yes;
			fJobReadyForCostPosting = YesNoList.Codes.Yes;
			JobInvoiced = YesNoList.Codes.Yes;
			JobReadyForDelivery = YesNoList.Codes.Yes;
			JobReadyForFinancialClosure = YesNoList.Codes.Yes;
			ScheduledForArchive = YesNoList.Codes.Yes;
		}

		void SetJobReadyForFinancialClosureDefaults()
		{
			Working = YesNoList.Codes.Yes;
			WorkOnHold = YesNoList.Codes.Yes;
			InvoiceOnHold = YesNoList.Codes.Yes;
			CustomsProcessActive = YesNoList.Codes.Yes;
			JobReadyForRevenuePosting = YesNoList.Codes.Yes;
			JobReadyForCostPosting = YesNoList.Codes.Yes;
			JobReadyForRevenueAndCostPosting = YesNoList.Codes.Yes;
			JobInvoiced = YesNoList.Codes.Yes;
			JobReadyForDelivery = YesNoList.Codes.Yes;
			Complete = YesNoList.Codes.Yes;
			ScheduledForArchive = YesNoList.Codes.Yes;
		}

		#endregion

		#endregion

		public bool IsRestricted(string newJobStatus) => GetRestricteValueFromJobStatus(newJobStatus) == YesNoList.Codes.Yes;

		public ZString GetRestricteValueFromJobStatus(string jobStatus)
		{
			var result = ZString.Empty;
			switch (jobStatus)
			{
				case JobHeaderStatus.Codes.Working:
					result = Working;
					break;

				case JobHeaderStatus.Codes.WorkOnHold:
					result = WorkOnHold;
					break;

				case JobHeaderStatus.Codes.InvoiceOnHold:
					result = InvoiceOnHold;
					break;

				case JobHeaderStatus.Codes.CustomsProcessActive:
					result = CustomsProcessActive;
					break;

				case JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting:
					result = JobReadyForRevenueAndCostPosting;
					break;

				case JobHeaderStatus.Codes.JobReadyForRevenuePosting:
					result = JobReadyForRevenuePosting;
					break;

				case JobHeaderStatus.Codes.JobReadyForCostPosting:
					result = JobReadyForCostPosting;
					break;

				case JobHeaderStatus.Codes.JobReadyForDelivery:
					result = JobReadyForDelivery;
					break;

				case JobHeaderStatus.Codes.JobInvoiced:
					result = JobInvoiced;
					break;

				case JobHeaderStatus.Codes.Complete:
					result = Complete;
					break;

				case JobHeaderStatus.Codes.JobReadyForFinancialClosure:
					result = JobReadyForFinancialClosure;
					break;

				case JobHeaderStatus.Codes.ScheduledForArchive:
					result = ScheduledForArchive;
					break;
			}

			return result;
		}

		SecurityCheckpoint GetRelatedSecurityRight(string jobStatus)
		{
			SecurityCheckpoint result = null;

			switch (jobStatus)
			{
				case JobHeaderStatus.Codes.Working:
					result = Env.Security.ChangeStatusOfWorkingJobs;
					break;

				case JobHeaderStatus.Codes.WorkOnHold:
					result = Env.Security.ChangeStatusOfWorkOnHoldJobs;
					break;

				case JobHeaderStatus.Codes.InvoiceOnHold:
					result = Env.Security.ChangeStatusOfInvoiceOnHoldJobs;
					break;

				case JobHeaderStatus.Codes.CustomsProcessActive:
					result = Env.Security.ChangeStatusOfCustomsProcessingActiveJobs;
					break;

				case JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting:
				case JobHeaderStatus.Codes.JobReadyForRevenuePosting:
				case JobHeaderStatus.Codes.JobReadyForCostPosting:
					result = Env.Security.ChangeStatusOfReadyToPostJobs;
					break;

				case JobHeaderStatus.Codes.JobReadyForDelivery:
					result = Env.Security.ChangeStatusOfReadyforDeliveryJobs;
					break;

				case JobHeaderStatus.Codes.JobInvoiced:
					result = Env.Security.ChangeStatusOfInvoicedJobs;
					break;

				case JobHeaderStatus.Codes.Complete:
					result = Env.Security.ChangeStatusOfCompleteJobs;
					break;

				case JobHeaderStatus.Codes.JobReadyForFinancialClosure:
					result = Env.Security.ChangeStatusOfReadyForFinancialClosureJobs;
					break;

				case JobHeaderStatus.Codes.ScheduledForArchive:
					result = Env.Security.ChangeStatusOfScheduleForArchiveJobs;
					break;
			}

			return result;
		}
	}
}
