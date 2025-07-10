using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	/// <summary>
	/// Holds a collection of Print Jobs that will eventually be merged into one print job. 
	/// The JobType, RetryAttempts and DeliveryGroup details should all be the same.
	/// </summary>
	public class StmPrintJobMergedCollection : StmPrintJobCollection
	{
		public StmPrintJobMergedCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		/// <summary>
		/// The type of jobs (EML, PRN, etc) in this collection. This property assumes all jobs in this collection share the same Job Type.
		/// </summary>
		public ZString JobType
		{
			get { return (Count > 0) ? this[0].SP_JobType : ZString.Empty; }
		}

		/// <summary>
		/// Whether the jobs in this collection have been saved to eDocs. This property assumes all jobs in this collection share the same EDocsProcessed property.
		/// </summary>
		public ZBool EDocsProcessed
		{
			get { return (Count > 0) ? this[0].SP_EDocsProcessed : ZBool.False; }
		}

		/// <summary>
		/// Whether the jobs in this collection support saving to eDocs. This property assumes all jobs in this collection share the same DocManagerSupportsBusinessContext property.
		/// </summary>
		public bool ShouldSaveToEDocs
		{
			get { return (Count > 0) && this[0].DocManagerSupportsBusinessContext; }
		}

		/// <summary>
		/// The number of times these jobs have been retried. This property assumes all jobs in this collection share the same retry attemtps.
		/// </summary>
		public int RetryAttempts
		{
			get { return (Count > 0) ? (int)this[0].SP_RetryAttempts : 0; }
		}

		/// <summary>
		/// The delivery group these print jobs belong to. This property assumes all jobs in this collection share the same Delivery Group.
		/// </summary>
		public ZGuid DeliveryGroupPK
		{
			get { return (Count > 0) ? this[0].SP_SB_DeliveryGroup : ZGuid.Empty; }
		}

		public StmDeliveryGroup DeliveryGroup
		{
			get { return (StmDeliveryGroup)Factory.Load(typeof(StmDeliveryGroup), DeliveryGroupPK); }
		}

		public bool IsUnderSizeLimit(StmPrintJob printJobToAdd, ZInt sizeLimitInKB)
		{
			return (SizeInKB + printJobToAdd.StoredAttachmentSizeKB < sizeLimitInKB);
		}

		public ZInt SizeInKB
		{
			get
			{
				ZInt totalSize = 0;

				foreach (StmPrintJob printJob in this)
				{
					totalSize += printJob.StoredAttachmentSizeKB;
				}

				return totalSize;
			}
		}

		public void IncrementRetryAttempts()
		{
			DeleteAttachments(true);
		}

		public void DeleteStoredAttachments()
		{
			DeleteAttachments(false);
		}

		public void NotifyAllJobsDelivered()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				this[i].NotifyDelivered();
			}
		}

		public void MarkAllEDocsProcessed()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				this[i].MarkEDocsProcessed();
			}
		}

		void DeleteAttachments(bool incrementRetryAttempts)
		{
			foreach (StmPrintJob job in this)
			{
				if (incrementRetryAttempts)
				{
					job.SP_RetryAttempts++;
				}

				job.DeleteStoredAttachment();
			}
		}

		public void SetFailureReason(string message)
		{
			var failureReason = new ZString(message).SubstringSafe(0, AutoStmPrintJob.Schema.SP_FaxDestinationMaxLength);

			foreach (StmPrintJob job in this)
			{
				if (job.SP_FailureReason.IsEmpty)
				{
					job.SP_FailureReason = failureReason;
				}
			}
		}
	}
}
