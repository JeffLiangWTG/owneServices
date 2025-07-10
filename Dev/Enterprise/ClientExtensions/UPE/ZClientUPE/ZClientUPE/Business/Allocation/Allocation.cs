using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class Allocation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public Allocation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateQueue();
			ValidateReason();
		}

		#region Queue

		[MaxLength(ProcessQueue.Schema.P4_CustomsQueueMaxLength)]
		public ZString Queue
		{
			get { return fQueue; }
			set
			{
				if (fQueue != value)
				{
					CheckMaximumLength(QueueInfo, value);
					SetNonPersistentPropertyValue(QueueInfo, ref fQueue, value);
					if (!IsValidationSuspended)
					{
						ValidateQueue();
					}
				}
			}
		}
		ZString fQueue;

		public ZPropertyInfo QueueInfo
		{
			get { return GetZPropertyInfo(nameof(Queue)); }
		}

		public void ValidateQueue()
		{
			QueueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(QueueInfo);
			ListValidation.ErrorIfInvalidCode(QueueInfo, QueueList);
		}

		#endregion

		#region Reason

		[MaxLength(ProcessQueue.Schema.P4_CustomsStatusMaxLength)]
		public ZString Reason
		{
			get { return fReason; }
			set
			{
				if (fReason != value)
				{
					CheckMaximumLength(ReasonInfo, value);
					SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value);
					if (!IsValidationSuspended)
					{
						ValidateReason();
					}
				}
			}
		}
		ZString fReason;

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		public void ValidateReason()
		{
			ReasonInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ReasonInfo, ReasonList);
		}

		#endregion

		#region Bind To Lists

		#region Queue List

		public DeclarationQueueCodeDescriptionPairList QueueList
		{
			get
			{
				if (fQueueList == null)
				{
					fQueueList = new DeclarationQueueCodeDescriptionPairList();
				}

				return fQueueList;
			}
		}
		DeclarationQueueCodeDescriptionPairList fQueueList;

		#endregion

		#region Reason List

		public ReasonCodeDescriptionPairList ReasonList
		{
			get
			{
				if (fReasonList == null)
				{
					fReasonList = new ReasonCodeDescriptionPairList();
				}

				return fReasonList;
			}
		}
		ReasonCodeDescriptionPairList fReasonList;

		#endregion

		#region Classifier Allocation

		public ClassifierAllocationCollection ClassifierAllocationList
		{
			get
			{
				if (fClassifierAllocationList == null)
				{
					fClassifierAllocationList = new ClassifierAllocationCollection(Factory);
				}

				return fClassifierAllocationList;
			}
		}
		ClassifierAllocationCollection fClassifierAllocationList;

		virtual public void ReLoadClassifierAllocation()
		{
			ClassifierAllocationList.RemoveAndDeleteAll();

			DynamicBusinessObjectCollection dynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(Factory);

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			ZString sqlText =
				"select AllocatedTo," +
				" (select count(*)" +
				" from " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " " +
				" inner join " + ProcessQueueSchema.Constants.SqlSchemaName + "." + ProcessQueueSchema.Constants.TableName + " on " + ProcessQueueSchema.Constants.P4_ParentID + " = " + JobDeclarationSchema.Constants.PK +
				" where " + ProcessQueueSchema.Constants.P4_CustomsQueue + " like @P4_CustomsQueue" +
				" and " + ProcessQueueSchema.Constants.P4_CustomsStatus + " like @P4_CustomsStatus" +
				" and " + ProcessQueueSchema.Constants.P4_GS_NKCustomsTaskAssignedTo + " = AllocatedTo" +
				" ) as NumberAllocated" +
				" from" +
				" (" +
				" select " + ProcessQueueSchema.Constants.P4_GS_NKCustomsTaskAssignedTo + " as AllocatedTo" +
				"	 from dbo.JobDeclaration" +
				"	 inner join " + ProcessQueueSchema.Constants.SqlSchemaName + "." + ProcessQueueSchema.Constants.TableName + " on " + ProcessQueueSchema.Constants.P4_ParentID + " = " + JobDeclarationSchema.Constants.PK +
				"  where " + ProcessQueueSchema.Constants.P4_CustomsQueue + " like @P4_CustomsQueue" +
				"  and " + ProcessQueueSchema.Constants.P4_CustomsStatus + " like @P4_CustomsStatus" +
				"	 group by " + ProcessQueueSchema.Constants.P4_GS_NKCustomsTaskAssignedTo +
				"	union" +
				" select " + GlbStaffSchema.Constants.GS_Code + " as AllocatedTo" +
				"	 from " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName + " " +
				"	 inner join " + GlbGroupLinkSchema.Constants.SqlSchemaName + "." + GlbGroupLinkSchema.Constants.TableName + "  on " + GlbGroupLinkSchema.Constants.GK_GS + " = " + GlbStaffSchema.Constants.PK +
				"	 inner join " + GlbGroupSchema.Constants.SqlSchemaName + "." + GlbGroupSchema.Constants.TableName + "  on " + GlbGroupLinkSchema.Constants.GK_GG + " = " + GlbGroupSchema.Constants.PK +
				"	 where " + GlbGroupSchema.Constants.GG_Code + " = @GG_Code) AssignedTo" +
				" order by NumberAllocated";

			@params.Add("@P4_CustomsQueue", "%" + Queue + "%", ProcessQueueSchema.P4_CustomsQueue);
			@params.Add("@P4_CustomsStatus", "%" + Reason + "%", ProcessQueueSchema.P4_CustomsStatus);
			@params.Add("@GG_Code", UPEDataRegistry.Instance.ClassifierStaffGroupCode.Value, GlbGroupSchema.GG_Code);

			dynamicBusinessObjectCollection.Load(sqlText, @params);

			foreach (DynamicBusinessObject dynamicBusinessObject in dynamicBusinessObjectCollection)
			{
				ClassifierAllocation classifierAllocation = new ClassifierAllocation(Factory);
				classifierAllocation.AllocatedTo = dynamicBusinessObject["AllocatedTo"].ToString();
				classifierAllocation.NumberAllocated = ZInt.Parse(dynamicBusinessObject["NumberAllocated"].ToString());
				classifierAllocation.IncludeForAllocation = classifierAllocation.NextWorkingDay == ZDateTime.Empty;

				ClassifierAllocationList.Add(classifierAllocation);
			}
		}

		virtual public void ReAllocate()
		{
			ReAllocateExcludedStaffJobs();
			ReAllocateIncludedStaffJobs();
		}

		void ReAllocateExcludedStaffJobs()
		{
			ClassifierAllocationCollection excluded = ClassifierAllocationList.GetExcludedForAllocation();
			ClassifierAllocationCollection included = ClassifierAllocationList.GetIncludedForAllocation();

			foreach (ClassifierAllocation excludedClassifierAllocation in excluded)
			{
				int numberToReallocate = excludedClassifierAllocation.NumberAllocated;

				if (numberToReallocate > 0)
				{
					ProcessQueue[] excludedProcessQueues = Factory.Load<ProcessQueue>(GetProcessQueueQuery(excludedClassifierAllocation));

					for (int i = 0; i < numberToReallocate; i++)
					{
						ClassifierAllocation includedClassifierAllocation = included.ClassifierWithLowestAllocation;
						if (includedClassifierAllocation != null)
						{
							if (excludedProcessQueues.Length > i)
							{
								excludedProcessQueues[i].P4_GS_NKCustomsTaskAssignedTo = includedClassifierAllocation.AllocatedTo;
							}

							includedClassifierAllocation.NumberAllocated++;
							excludedClassifierAllocation.NumberAllocated--;
						}
					}
				}
			}
			Factory.Save();
		}

		void ReAllocateIncludedStaffJobs()
		{
			ClassifierAllocationCollection included = ClassifierAllocationList.GetIncludedForAllocation();

			ClassifierAllocation classifierWithLowestAllocation = included.ClassifierWithLowestAllocation;
			ClassifierAllocation classifierWithHighestAllocation = included.ClassifierWithHighestAllocation;

			if (classifierWithLowestAllocation != null && classifierWithHighestAllocation != null)
			{
				while (classifierWithLowestAllocation.NumberAllocated < (classifierWithHighestAllocation.NumberAllocated - 1))
				{
					ProcessQueue highestAllocationProcessQueue = Factory.LoadTop1<ProcessQueue>(GetProcessQueueQuery(classifierWithHighestAllocation));
					if (highestAllocationProcessQueue != null)
					{
						highestAllocationProcessQueue.P4_GS_NKCustomsTaskAssignedTo = classifierWithLowestAllocation.AllocatedTo;
					}
					else
					{
						break;
					}

					classifierWithLowestAllocation.NumberAllocated++;
					classifierWithHighestAllocation.NumberAllocated--;

					classifierWithLowestAllocation = included.ClassifierWithLowestAllocation;
					classifierWithHighestAllocation = included.ClassifierWithHighestAllocation;

					if (classifierWithLowestAllocation == null && classifierWithHighestAllocation == null)
					{
						break;
					}
					Factory.Save();
					Factory.ClearQueryCache();
				}
			}
		}

		ZQuery GetProcessQueueQuery(ClassifierAllocation classifierAllocation)
		{
			ZQuery result = new ZQuery(ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo, classifierAllocation.AllocatedTo);
			result.AddToFilter(ProcessQueueSchema.P4_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			if (!Queue.IsEmpty)
			{
				result.AddToFilter(ProcessQueueSchema.P4_CustomsQueue, Queue);
			}
			if (!Reason.IsEmpty)
			{
				result.AddToFilter(ProcessQueueSchema.P4_CustomsStatus, Reason);
			}

			return result;
		}

		#endregion

		#endregion
	}
}
