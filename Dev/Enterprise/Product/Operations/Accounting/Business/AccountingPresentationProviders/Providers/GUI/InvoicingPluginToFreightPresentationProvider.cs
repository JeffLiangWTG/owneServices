using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IInvoicingPluginToFreightPresentationProvider
	{
		PreSaveActionsResult PreSaveActions();
		string FixRevenueRecognitionData(Func<ZString, ZBool> savingDecisionFunc);
	}

	public class InvoicingPluginToFreightPresentationProvider : IInvoicingPluginToFreightPresentationProvider
	{
		public InvoicingPluginToFreightPresentationProvider(IClosedJobReopener closedJobReopener, IJobRevRecognitionDataRetriever jobRevRecognitionDataRetriever)
		{
			ClosedJobReopener = Argument.NotNull(closedJobReopener, nameof(closedJobReopener));
			JobRevRecognitionDataRetriever = Argument.NotNull(jobRevRecognitionDataRetriever, nameof(jobRevRecognitionDataRetriever));
		}

		readonly IClosedJobReopener ClosedJobReopener;
		readonly IJobRevRecognitionDataRetriever JobRevRecognitionDataRetriever;

		PreSaveActionsResult IInvoicingPluginToFreightPresentationProvider.PreSaveActions()
		{
			if (!ClosedJobReopener.ReopenClosedJobs())
			{
				return PreSaveActionsResult.Failure(Res.GetString("67364CC0-6F85-4BD2-B09F-D4828077228A", "Saving requires job reopening."));
			}

			return PreSaveActionsResult.Success();
		}

		#region Fix Revenue Recognition Data

		string IInvoicingPluginToFreightPresentationProvider.FixRevenueRecognitionData(Func<ZString, ZBool> savingDecisionFunc)
		{
			Argument.NotNull(savingDecisionFunc, nameof(savingDecisionFunc));

			var shouldSave = false;
			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(JobRevRecognitionDataRetriever.JobPk);

			var revRecognitionData = JobRevRecognitionDataRetriever.GetRevenueRecognitionData();

			if (revRecognitionData.Any())
			{
				var groupedRevenueRecognitions = revRecognitionData
					.Where(x => !string.IsNullOrEmpty(x.recognitionType))
						.GroupBy(x => x.recognitionType)
						.Select(g => new
						{
							RecognitionType = g.Key,
							RecognizedDate = g.Select(p => p.recognizedDate).Min()
						}).ToArray();

				var jobChargeRevRecognitions = jobInNewFactory.RevenueRecognitionCollection;
				var beforeFixDataMessage = Job.GetRevenueRecognitionDatesAsString(jobChargeRevRecognitions);

				var existingTypes = jobChargeRevRecognitions.Select(x => x.D3_RecognitionType).ToHashSet();
				foreach (var revenueRecognition in groupedRevenueRecognitions)
				{
					if (!existingTypes.Contains(revenueRecognition.RecognitionType))
					{
						var newRevenueRecognition = jobChargeRevRecognitions.AddNew();
						newRevenueRecognition.D3_JH = jobInNewFactory.PK;
						newRevenueRecognition.D3_RecognitionType = revenueRecognition.RecognitionType;
						newRevenueRecognition.D3_RecognitionDate = revenueRecognition.RecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate ? AccountingConstants.RevenueRecognitionDateConstants.Immediate : revenueRecognition.RecognizedDate;

						shouldSave = true;
					}
				}

				if (shouldSave)
				{
					var afterFixDataMessage = Job.GetRevenueRecognitionDatesAsString(jobChargeRevRecognitions);
					var message = GetFurtherActionMessage(beforeFixDataMessage, afterFixDataMessage);

					if (savingDecisionFunc(message))
					{
						newFactory.Save();
						return Res.GetString("10611E24-5EC1-463A-BD8E-50206BE21525", "Job revenue recognition data fixed.");
					}

					return string.Empty;
				}
			}

			return Res.GetString("2BCF61DE-CB7B-43B9-8C30-8AC8C4A4AB7C", "No broken job revenue recognition data detected.");
		}

		string GetFurtherActionMessage(ZString beforeFixMessage, ZString afterFixMessage)
		{
			return Res.GetString("22EA938F-CBD3-489C-B2F1-F7113CABE88B", @"Before applying the data fix, the Job Revenue Recognition Date is

{0}

After applying the fix, the Job Revenue Recognition Date should be

{1}

Do you want to proceed?", string.IsNullOrEmpty(beforeFixMessage) ? (NoResString)"Empty" : beforeFixMessage, afterFixMessage);
		}

		#endregion
	}
}
