using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargeRevRecognition : AutoJobChargeRevRecognition
	{
		public JobChargeRevRecognition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (D3_RecognitionType.IsEmpty)
			{
				D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			}
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			base.FillWithValidTestDataCore(kind, propertyPath);
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
		}

#endif

		public override JobHeader Job
		{
			get { return Factory.Load<Job>(D3_JH); }
		}

		public Job InvoicingJob
		{
			get { return Job as Job; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new JobChargeRevRecognitionUniqueIndexFailureHandler(this); }
		}

		public class JobChargeRevRecognitionUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public JobChargeRevRecognitionUniqueIndexFailureHandler(JobChargeRevRecognition jobChargeRevRecognition)
			{
				this.jobChargeRevRecognition = jobChargeRevRecognition;
			}

			readonly JobChargeRevRecognition jobChargeRevRecognition;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobChargeRevRecognitionSchema.Constants.Indexes.FK_UC__D3_JH_D3_RecognitionType; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var query = new ZQuery(JobChargeRevRecognitionSchema.D3_JH, jobChargeRevRecognition.D3_JH);
				query.AddToFilter(JobChargeRevRecognitionSchema.D3_RecognitionType, jobChargeRevRecognition.D3_RecognitionType);
				query.FetchOnlyFromLocalCache = false;
				query.ReLoadExistingRows = true;
				var jobChargeRevRecognitionInDB = jobChargeRevRecognition.Factory.LoadTop1<JobChargeRevRecognition>(query);

				if (jobChargeRevRecognitionInDB != null)
				{
					notifier?.ReportInformation(GetNotificationMessage(jobChargeRevRecognitionInDB), Res.GetString("79C93A66-7AAC-44FD-A624-069148675BEB", "Job Revenue Recognition Date"));
				}
			}

			string GetNotificationMessage(JobChargeRevRecognition jobChargeRevRecognition)
			{
				var result = string.Empty;
				if (jobChargeRevRecognition.D3_RecognitionDate == AccountingConstants.RevenueRecognitionDateConstants.Immediate)
				{
					result = Res.GetString("385c46c1-cb0b-4190-9289-8eb92b0061ab", "Revenue has already been recognized for this job with recognition type {0}. Please reopen the form and try again.", jobChargeRevRecognition.D3_RecognitionType);
				}
				else
				{
					result = Res.GetString("47753612-31d9-42c6-bfcd-37bcd1418011", "Revenue has already been recognized on {0} for this job with recognition type {1}. Please reopen the form and try again.", jobChargeRevRecognition.D3_RecognitionDate.ToString("dd MMM yyyy"), jobChargeRevRecognition.D3_RecognitionType);
				}

				return result;
			}

			#endregion
		}
	}
}
