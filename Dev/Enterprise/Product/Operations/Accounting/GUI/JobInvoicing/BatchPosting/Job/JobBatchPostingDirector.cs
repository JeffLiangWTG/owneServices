using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public class JobBatchPostingDirector : BaseBatchPostingDirector, IJobBatchPostingDirector
	{
		public JobBatchPostingDirector()
		{
		}

		public void RunBatchPosting(JobInvoicingPostingOption postingOption, BusinessObject[] jobCollection, ZString postedObjectName)
		{
			RunBatchPosting(postingOption, (Job[])jobCollection, postedObjectName);
		}

		void RunBatchPosting(JobInvoicingPostingOption postingOption, Job[] jobCollection, ZString postedObjectName)
		{
			this.JobCollection = jobCollection;
			this.fPostedObjectName = postedObjectName;
			base.RunBatchPostingCore(postingOption);
		}

		Job[] JobCollection;

		public void RunBatchPosting(JobInvoicingPostingOption postingOption, IJobInvoicingPlugIn[] jobCollection, ZString postedObjectName)
		{
			this.JobPluginCollection = jobCollection;
			this.fPostedObjectName = postedObjectName;
			base.RunBatchPostingCore(postingOption);
		}

		IJobInvoicingPlugIn[] JobPluginCollection;

		#region Overrides

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetNewGUIWrapper(JobInvoicingPostingOption postingOption, ZString selectedPostingOptionName)
		{
			if (JobCollection != null)
			{
				return new JobBatchInvoicingPostManagerGUIWrapper(postingOption, JobCollection, selectedPostingOptionName, PostedObjectName);
			}
			else
			{
				return new JobBatchInvoicingPostManagerGUIWrapper(postingOption, JobPluginCollection, selectedPostingOptionName, PostedObjectName);
			}
		}

		protected override ZString NameForCostsInvoicingPostingOption
		{
			get { return Core.Constants.MenuNameConstants.PostCosts; }
		}

		protected override ZString NameForAllInvoicingPostingOption
		{
			get { return Core.Constants.MenuNameConstants.PostAllChargesAndCosts; }
		}

		protected override ZString PostedObjectName
		{
			get { return fPostedObjectName; }
		}
		ZString fPostedObjectName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Plural check")]
		protected override ZString UserQuestion
		{
			get
			{
				ZString question;
				if (JobCollection != null)
				{
					question = Res.GetString("4c98cad7-a2cc-4b63-9af6-ca4411b78a86", "Are you sure you want to {0} for", SelectedPostingOptionName) + " ";
					question += (JobCollection.Length == 1 ? Res.GetString("eb67a622-e4a1-4618-9c03-99f0d8fdee9a", "this {0}?", PostedObjectName.EndsWith("s") ? PostedObjectName.Left(PostedObjectName.Length - 1) : PostedObjectName)
											: Res.GetString("541c0084-b148-43af-af41-2bdf4d036d95", "these {0}?", Grammar.Instance.Pluralize(PostedObjectName)));

					foreach (Job job in JobCollection)
					{
						question += System.Environment.NewLine + job.JH_JobNum;
					}
				}
				else
				{
					question = Res.GetString("4c98cad7-a2cc-4b63-9af6-ca4411b78a86", "Are you sure you want to {0} for", SelectedPostingOptionName) + " ";
					question += ((JobPluginCollection.Length == 1) ? Res.GetString("eb67a622-e4a1-4618-9c03-99f0d8fdee9a", "this {0}?", PostedObjectName.EndsWith("s") ? PostedObjectName.Left(PostedObjectName.Length - 1) : PostedObjectName)
											: Res.GetString("541c0084-b148-43af-af41-2bdf4d036d95", "these {0}?", Grammar.Instance.Pluralize(PostedObjectName)));
					foreach (IJobInvoicingPlugIn job in JobPluginCollection)
					{
						question += System.Environment.NewLine + job.JobNumber;
					}
				}
				return question;
			}
		}

		#endregion
	}
}
