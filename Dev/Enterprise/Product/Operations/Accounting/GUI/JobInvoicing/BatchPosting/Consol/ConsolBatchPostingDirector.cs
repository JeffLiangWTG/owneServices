using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	class ConsolBatchPostingDirector : BaseBatchPostingDirector, IConsolBatchPostingDirector
	{
		public ConsolBatchPostingDirector()
		{
		}

		public void RunBatchPosting(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] consolCollection)
		{
			this.ConsolCollection = consolCollection;
			base.RunBatchPostingCore(postingOption);
		}
		IJobCostingPlugIn[] ConsolCollection;

		#region Overrides

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetNewGUIWrapper(JobInvoicingPostingOption postingOption, ZString selectedPostingOptionName)
		{
			return new ConsolBatchInvoicingPostManagerGUIWrapper(postingOption, ConsolCollection, selectedPostingOptionName);
		}

		protected override ZString NameForCostsInvoicingPostingOption
		{
			get { return Core.Constants.MenuNameConstants.PostAllCosts; }
		}

		protected override ZString NameForAllInvoicingPostingOption
		{
			get { return Core.Constants.MenuNameConstants.PostWholeConsol; }
		}

		protected override ZString PostedObjectName
		{
			get { return Res.GetString("Accounting|ConsolBatchInvoicingPostManager|PostedObjectName", "Consol"); }
		}

		protected override ZString UserQuestion
		{
			get
			{
				ZString question = Res.GetString("68d880cc-aec5-48d4-88dc-54d0a845a68b", "Are you sure you want to {0}", SelectedPostingOptionName);
				question += " " + Res.GetString("d658cf35-fb76-43ff-b140-392acdcc1559", "for") + " " + ((ConsolCollection.Length == 1) ? Res.GetString("a500b572-31f7-42c7-a72a-455258a6e088", "this consol?") : Res.GetString("742ac475-847d-4169-a26f-2c1c99755e12", "these consols?"));

				foreach (IJobCostingPlugIn consol in ConsolCollection)
				{
					question += System.Environment.NewLine + consol.JK_UniqueConsignRef;
				}

				return question;
			}
		}

		#endregion
	}
}
