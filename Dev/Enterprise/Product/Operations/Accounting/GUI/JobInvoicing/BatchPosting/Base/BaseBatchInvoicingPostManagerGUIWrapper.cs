using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public abstract partial class BaseBatchInvoicingPostManagerGUIWrapper : PostManagerGUIWrapper
	{
		public BaseBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, ZString selectedPostingOptionName)
			: this(postingOption, selectedPostingOptionName, ZString.Empty)
		{
		}

		public BaseBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, ZString selectedPostingOptionName, ZString postedObjectName)
			: base(null, postingOption, new BusinessObjectFactory(), null)
		{
			PlugInFactory.RefreshEnabled = false;
			this.postedObjectName = postedObjectName;
			this.SelectedPostingOptionName = selectedPostingOptionName;
		}

		#region Messages

		public static string GetJobOnHoldMessageWithParameter(string workOnHoldCode)
		{
			return Res.GetString("f0899dab-9177-4a86-8255-5d78a9909001", @"The following jobs are on hold and their associated charges will not be posted.
To post these jobs later, change the job status from '{0}'.", workOnHoldCode);
		}

		public ZString PostedObjectName
		{
			get { return !postedObjectName.IsEmpty ? postedObjectName : DefaultPostedObjectName; }
		}

		readonly ZString postedObjectName;

		protected abstract ZString DefaultPostedObjectName { get; }

		protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.Append(GetJobOnHoldMessageWithParameter(JobHeaderStatus.WorkOnHold.Code));

			foreach (Job job in jobs)
			{
				builder.Append(" " + job.JH_JobNum + "\r\n");
			}
			return builder.ToString();
		}

		#endregion

		#region Posting

		protected override BasePostManager PostManager
		{
			get { return fCurrentPostManager; }
		}
		BasePostManager fCurrentPostManager;

		public override void Post()
		{
		}

		/// <summary>
		/// It is absolutely required that the passed in jobs should be already re-loaded in new factory,
		/// because using jobs from Consol or Shipment factory and updating them down the line
		/// causes major problems when the posting process fails.
		/// </summary>
		/// <param name="jobCollectionInNewFactory">Jobs from the new factory</param>
		protected void PostSingleObject(IEnumerable<Job> jobCollectionInNewFactory)
		{
			PrepareForPosting(jobCollectionInNewFactory);
			RaiseOnStartingToPostNextObject(CurrentObjectCode, CurrentObjectNumber);

			ZString validationResult = ZString.Empty;
			if (Jobs != null)
			{
				foreach (Job job in Jobs)
				{
					if (!job.HasErrors)
					{
						foreach (Charge charge in job.Charges)
						{
							charge.Debtors.Load(new ZQuery(OrgHeaderSchema.PK, charge.JR_OH_SellAccount));
							charge.Validation.ValidateAll();
						}

						if (job.HasErrors)
						{
							validationResult = Res.GetString("65bbc861-6c19-4acf-bd48-351e33a35465", "This job cannot be posted because it has errors.");
						}
					}
				}
			}

			if (validationResult.IsEmpty)
			{
				base.Post();
			}
			else
			{
				NotifyPostValidationError(validationResult);
			}

			if (PostManager.CancelPosting)
			{
				if (CurrentObjectMessageStack.IsEmpty)
				{
					AppendMessageToStack(Res.GetString("30ff0283-fef3-4145-8c58-d6dc8989d838", "{0} posting canceled.", PostedObjectName));
				}
				CurrentObjectPosted = ZBool.False;
			}

			RaiseOnObjectPostingFinished(CurrentObjectMessageStack, CurrentObjectPosted);
		}

		protected override bool IsBulkPosting { get { return true; } }
		protected override bool ShouldValidatePostingEditSecurityLock { get { return false; } }
		protected override bool ShouldValidatePostingChargeCodeGLChequeDetails { get { return false; } }

		protected override void NotifyPostingWarningCore(string message)
		{
			AppendMessageToStack(Res.GetString("D884C864-5DAF-4882-98B7-693A94EA2001", "{0} posting has warning. {1}", PostedObjectName, message));
		}

		protected override void NotifyPostValidationError(string error)
		{
			AppendMessageToStack(Res.GetString("e4952693-9bd6-4287-8ac5-f0f6ec9a5ea7", "{0} posting failed. {1}", PostedObjectName, error));
			CurrentObjectPosted = ZBool.False;
		}

		protected override bool ConfirmContinueWithPostValidationWarning(PostManagerNotification notification)
		{
			AppendMessageToStack(Res.GetString("161B71BB-9CC6-469D-A858-6EC42F391C3A", "{0} posting has warning. {1}", PostedObjectName, notification.Message));
			return true;
		}

		void PrepareForPosting(IEnumerable<Job> jobCollectionInNewFactory)
		{
			CurrentObjectPosted = ZBool.True;
			CurrentObjectNumber += 1;
			Jobs = jobCollectionInNewFactory;
			if (OriginalJobs == null)
			{
				var query = new ZQuery(JobShipmentSchema.PK, jobCollectionInNewFactory.Select(element => element.PK).ToArray());
				var jobFilter = new JobCollection(PlugInFactory, query).CompleteFilter;
				OriginalJobs = PlugInFactory.Load<Job>(jobFilter).ToList();
			}
			CurrentObjectMessageStack = ZString.Empty;
			fCurrentPostManager = GetNewPostManager();
		}

		#region BackDateARInvoices

		protected override QueryUserMsgBoxEventArgs GetNewQueryUserEventArgs()
		{
			return new QueryUserYesNoYesAllNoAllEventArgs();
		}

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(Security.SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
		{
			return new ChangeTransactionDatesForBatchPostingBusinessObject(pluginSecurity, factory);
		}

		protected override ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(Security.SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
		{
			return new ChangeTransactionDatesForBatchPostingBusinessObject(pluginSecurity, factory, codes);
		}

		#endregion

		#endregion

		#region Events

		protected override void NothingPostedHandlerCore(string message)
		{
			CurrentObjectPosted = ZBool.False;
			AppendMessageToStack(Res.GetString("c9da5a90-9927-4fc8-81df-dd4db46dd811", "{0} posting failed.", PostedObjectName));
			AppendMessageToStack(message);
		}

		protected override void JobsOnHoldCore(string message)
		{
			CurrentObjectPosted = ZBool.False;
			AppendMessageToStack(message);
		}

		public delegate void ObjectPostingFinishedEventHandler(ZString resultMessage, ZBool isObjectPosted);
		public event ObjectPostingFinishedEventHandler OnObjectPostingFinished;
		void RaiseOnObjectPostingFinished(ZString currentObjectMessagesStack, ZBool isCurrentObjectPosted)
		{
			if (OnObjectPostingFinished != null)
			{
				OnObjectPostingFinished(currentObjectMessagesStack, isCurrentObjectPosted);
			}
#if DEBUG

			ObjectPosted = isCurrentObjectPosted;
#endif
		}

#if DEBUG
		public ZBool ObjectPosted;
#endif

		public event EventHandler OnPostingFinished;
		protected void RaiseOnPostingFinished()
		{
			if (OnPostingFinished != null)
			{
				OnPostingFinished(this, null);
			}
		}

		public delegate void StartingToPostNextObjectEventHandler(ZString objectCode, ZInt objectNumber);
		public event StartingToPostNextObjectEventHandler OnStartingToPostNextObject;
		void RaiseOnStartingToPostNextObject(ZString objectCode, ZInt objectNumber)
		{
			if (OnStartingToPostNextObject != null)
			{
				OnStartingToPostNextObject(objectCode, objectNumber);
			}
		}

		#endregion

		#region Implementation

		protected abstract ZString CurrentObjectCode { get; }
		ZInt CurrentObjectNumber;
		ZString CurrentObjectMessageStack;
		ZBool CurrentObjectPosted;
		public ZBool ShouldStopPostingOnNextObject { get; private set; }
		public abstract ZInt NumberOfObjectsToPost { get; }
		public ZString SelectedPostingOptionName { get; private set; }

		protected void AppendMessageToStack(ZString message)
		{
			if (!message.IsEmpty)
			{
				CurrentObjectMessageStack += System.Environment.NewLine + message;
			}
		}

		protected void RenewTransactionFactory()
		{
			TransactionFactory = new BusinessObjectFactory();
			TransactionFactory.RefreshEnabled = false;
		}

		public void StopPostingOnNextIteration()
		{
			ShouldStopPostingOnNextObject = ZBool.True;
		}

		#endregion

		#region Test
#if DEBUG

		protected override void SavePostingFactoriesForTest(TransactionCreatorHashtable transactions)
		{
			BusinessObjectFactory.SaveTogether(Test_Allocator.GetFactoriesForTest(TransactionFactory, PlugInFactory));
		}

#endif
		#endregion
	}
}
