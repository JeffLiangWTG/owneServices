using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class JobHeaderView : NonPersistentBusinessObject<JobHeaderViewValidation>
	{
		internal JobHeaderView(BusinessObject scheduledEntity, BusinessObjectFactory factory)
			: base(factory)
		{
			if (scheduledEntity is IWorkflowProvider workflowProvider)
			{
				ProcessHeader = ProcessJobHeader.GetForParent(workflowProvider, Factory);
			}
			else if (scheduledEntity is ProcessHeader processHeader)
			{
				ProcessHeader = Factory.ImportFromAnotherFactorySafe(processHeader);
			}
			else
			{
				throw new ArgumentException("scheduledEntity must be a ProcessHeader, or must implement IWorkflowProvider", nameof(scheduledEntity));
			}

			if (ProcessHeader != null)
			{
				earliestStartDateLocal = ProcessHeader.DoNotStartBeforeDateLocal;
				agreedDeliveryDateLocal = ProcessHeader.AgreedDeliveryDateLocal;
				dateAcceptability = ProcessHeader.FH_DateAcceptability;
			}
		}

		#region BusinessObject Overrides

		public override JobHeaderViewValidation GetNewValidation()
		{
			return new JobHeaderViewValidation(this);
		}

		#endregion

		#region Properties

		[ResourceStringData("JobHeaderView.Job", Caption = "Job")]
		public ZString Job
		{
			get { return ProcessHeader != null ? ProcessHeader.ParentJobDescription : ZString.Empty; }
		}

		[ResourceStringData("JobHeaderView.JobDescription", Caption = "Job Description")]
		public ZString JobDescription
		{
			get { return ProcessHeader != null ? ProcessHeader.ProviderJobDescription : ZString.Empty; }
		}

		[ResourceStringData("JobHeaderView.Description", Caption = "Workflow Description")]
		public ZString Description
		{
			get { return ProcessHeader != null ? ProcessHeader.FH_CompletionStatement : ZString.Empty; }
		}

		[ResourceStringData("JobHeaderView.EarliestStartDateLocal", Caption = "Earliest Start Date", ShortCaption = "Start Date", FullDescription = "The earliest date this job or workflow can start.")]
		public ZDateTime EarliestStartDateLocal
		{
			get { return earliestStartDateLocal; }
			set { SetNonPersistentPropertyValue(EarliestStartDateLocalInfo, ref earliestStartDateLocal, value); }
		}

		ZDateTime earliestStartDateLocal;

		public ZPropertyInfo EarliestStartDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(EarliestStartDateLocal)); }
		}

		[ResourceStringData("JobHeaderView.AgreedDeliveryDateLocal", Caption = "Agreed Delivery Date", ShortCaption = "Delivery Date", FullDescription = "The date this job or workflow should be completed by.")]
		public ZDateTime AgreedDeliveryDateLocal
		{
			get { return agreedDeliveryDateLocal; }
			set { SetNonPersistentPropertyValue(AgreedDeliveryDateLocalInfo, ref agreedDeliveryDateLocal, value); }
		}

		ZDateTime agreedDeliveryDateLocal;

		public ZPropertyInfo AgreedDeliveryDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(AgreedDeliveryDateLocal)); }
		}

		[List(nameof(DateAcceptabilityList))]
		[ResourceStringData("JobHeaderView.DateAcceptability", Caption = "Date Acceptability", ShortCaption = "Date Acceptability", FullDescription = "The 'hardness' or 'softness' for a late or early delivery against the agreed delivery date")]
		public ZString DateAcceptability
		{
			get { return dateAcceptability; }
			set { SetNonPersistentPropertyValue(DateAcceptabilityInfo, ref dateAcceptability, value); }
		}

		ZString dateAcceptability;

		public ZPropertyInfo DateAcceptabilityInfo
		{
			get { return GetZPropertyInfo(nameof(DateAcceptability)); }
		}

		public ICodeDescriptionPairList DateAcceptabilityList => Factory.GetCachedValue<DateAcceptabilityList>();

		[List(nameof(DeadlineTypeList))]
		[ResourceStringData("JobHeaderView.DeadlineType", Caption = "Deadline Type", ShortCaption = "Deadline Type", FullDescription = "Indicates whether it is a hard deadline or a soft deadline")]
		public ZString DeadlineType
		{
			get { return deadlineType; }
			set { SetNonPersistentPropertyValue(DeadlineTypeInfo, ref deadlineType, value); }
		}

		ZString deadlineType;

		public ZPropertyInfo DeadlineTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DeadlineType)); }
		}

		public ICodeDescriptionPairList DeadlineTypeList => Factory.GetCachedValue<DeadlineTypeList>();

		#endregion

		#region Related Business Objects

		public ProcessHeader ProcessHeader { get; }

		#endregion
	}
}
