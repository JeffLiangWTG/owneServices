using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	internal static class GatewayInvoiceTargetJobFinder
	{
		public static IJobInvoicingPlugIn[] GetInvoiceTargets(IJobInvoicingPlugIn relatedShipment)
		{
			var result = new List<IJobInvoicingPlugIn>();
			if (relatedShipment != null && !string.IsNullOrWhiteSpace(relatedShipment.JobNumber))
			{
				var consols = relatedShipment.GetAllLinkedConsols();
				ReportDuplicates(consols, true);

				result.Add(relatedShipment);
				result.AddRange(consols);

				ReportDuplicates(result);
			}

			return result.ToArray();
		}

		static void ReportDuplicates(IEnumerable<IJobInvoicingPlugIn> plugIns, bool amongConsols = false)
		{
			var hs1 = new HashSet<string>();

			foreach (var plugIn in plugIns)
			{
				if (!hs1.Add(plugIn.JobNumber))
				{
					ErrorReporter.ReportOnce("GatewayInvoiceTargetJobFinder.ReportDuplicates", $"Duplicate IJobInvoicingPlugIn ({plugIn.GetType().Name}) found with JobNumber {plugIn.JobNumber}{(amongConsols ? (NoResString)" among consols" : "")}");
				}
			}
		}

		public static ZString GetInvoiceTarget(ZString relatedJobNum, Job invoicingJob)
		{
			var jobType = FindDefaultInvoiceTargetJobTypeBasedOnRelatedJobNumber(relatedJobNum, invoicingJob);
			if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.SameConsol.Code)
			{
				return invoicingJob?.PlugInData?.JobNumber;
			}
			else if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.RelatedShipmentNumber.Code)
			{
				return relatedJobNum;
			}
			else if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.PreviousConsol.Code)
			{
				return FindPreviousConsolNumberBasedOnRelatedJobNumber(relatedJobNum, invoicingJob);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static ZGuid GetInternalJob(JobHeader job, ZString relatedJobNum, Job invoicingJob)
		{
			var jobType = FindDefaultInvoiceTargetJobTypeBasedOnRelatedJobNumber(relatedJobNum, invoicingJob);
			if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.SameConsol.Code)
			{
				return invoicingJob?.PK ?? ZGuid.Empty;
			}
			else if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.RelatedShipmentNumber.Code)
			{
				return job.PK;
			}
			else if (jobType == AccountingMasterFilesConstants.TargetJobDefaultingOptions.PreviousConsol.Code)
			{
				return FindPreviousConsolBasedOnRelatedJobNumber(relatedJobNum, invoicingJob);
			}
			else
			{
				return job.PK;
			}
		}

		static ZString FindDefaultInvoiceTargetJobTypeBasedOnRelatedJobNumber(ZString relatedJobNum, Job invoicingJob)
		{
			var result = ZString.Empty;
			var supporter = invoicingJob?.GetInvoicingSupporter();

			if (supporter != null)
			{
				var ranker = new StringColumnValueRanker();
				ranker.Add(GatewayChargeDefaultInvoiceTargetJobConfiguration.Schema.ConsolDirection, supporter.GetConsolDirectionFallBackCode());
				ranker.Add(GatewayChargeDefaultInvoiceTargetJobConfiguration.Schema.ConsolTransportMode, supporter.GetConsolTransportModeFallBackCode());
				ranker.Add(GatewayChargeDefaultInvoiceTargetJobConfiguration.Schema.PreviousSendingAgentType, supporter.GetPreviousSendingAgentFallBackCode(relatedJobNum));

				var configurations = AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.Value.OfType<GatewayChargeDefaultInvoiceTargetJobConfiguration>();
				var matchedConfigurations = (ranker.GetBestMatch(configurations) ?? Enumerable.Empty<GatewayChargeDefaultInvoiceTargetJobConfiguration>()).ToArray();
				result = matchedConfigurations?.FirstOrDefault()?.InvoiceTargetJobType ?? ZString.Empty;
			}
			return result;
		}

		static ZString FindPreviousConsolNumberBasedOnRelatedJobNumber(ZString relatedJobNum, Job invoicingJob)
		{
			var previousConsol = invoicingJob?.GetInvoicingSupporter()?.GetPreviousConsol(relatedJobNum);
			return previousConsol?.InvoicingSupporter?.ConsolNumber ?? ZString.Empty;
		}

		static ZGuid FindPreviousConsolBasedOnRelatedJobNumber(ZString relatedJobNum, Job invoicingJob)
		{
			var previousConsol = invoicingJob?.GetInvoicingSupporter()?.GetPreviousConsol(relatedJobNum);
			return previousConsol?.InvoicingSupporter?.Job?.PK ?? ZGuid.Empty;
		}
	}
}
