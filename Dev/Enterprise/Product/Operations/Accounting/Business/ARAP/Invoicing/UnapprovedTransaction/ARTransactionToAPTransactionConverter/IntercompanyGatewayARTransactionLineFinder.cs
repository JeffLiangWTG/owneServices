using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal static class IntercompanyGatewayARTransactionLineFinder
	{
		internal static List<InvoiceLineAssociatedWithGatewayJob> FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(InvoicingBase arTransaction)
		{
			if (arTransaction == null)
			{
				throw new ArgumentNullException(nameof(arTransaction));
			}

			if (arTransaction.AH_Ledger != LedgerTypes.AccountsReceivable)
			{
				throw new ArgumentException("This method only accpets an AR Transaction.");
			}

			var invoiceLines = FindInvoiceLinesAssociatedWithGatewayJob(arTransaction);
			var gatewayInvoiceLines = new List<InvoiceLineAssociatedWithGatewayJob>();
			if (invoiceLines.Any())
			{
				gatewayInvoiceLines = PopulateInvoiceLinesWithRelatedJobAndTargetJobDetails(invoiceLines);
			}
			return gatewayInvoiceLines;
		}

		static List<InvoicingLineBase> FindInvoiceLinesAssociatedWithGatewayJob(InvoicingBase arTransaction)
		{
			var invoiceLinesGroupedByJob = arTransaction.Lines.Cast<InvoicingLineBase>().Where(x => !x.AL_JH.IsEmpty).GroupBy(x => x.AL_JH);
			var invoiceLinesAssociatedWithGateway = new List<InvoicingLineBase>();
			foreach (var invoiceLineGroup in invoiceLinesGroupedByJob)
			{
				var job = invoiceLineGroup.First().Job;
				if (job != null)
				{
					if (job.Parent == null)
					{
						job.InitializeParentFromGenericJobWithoutSettingDefaults();
					}
					var isGatewayBillingJobInSisterCompany = job.IsGatewayBillingJob(arTransaction.Company);
					if (isGatewayBillingJobInSisterCompany)
					{
						invoiceLinesAssociatedWithGateway.AddRange(invoiceLineGroup);
					}
				}
			}
			return invoiceLinesAssociatedWithGateway;
		}

		static List<InvoiceLineAssociatedWithGatewayJob> PopulateInvoiceLinesWithRelatedJobAndTargetJobDetails(List<InvoicingLineBase> invoiceLines)
		{
			var gatewayInvoiceLines = new List<InvoiceLineAssociatedWithGatewayJob>();
			var jobChargeTargetCollection = GetJobChargeTargetCollection(invoiceLines.Select(x => x.PK));
			foreach (DynamicBusinessObject jobChargeTarget in jobChargeTargetCollection)
			{
				var gatewayInvoiceLine = CreateNewInvoiceLineAssociatedWithGatewayJob(jobChargeTarget);
				gatewayInvoiceLines.Add(gatewayInvoiceLine);
			}

			if (gatewayInvoiceLines.Any(x => x.TargetJobPk == ZGuid.Empty))
			{
				SetBlankTargetJobToGatewayConsol(invoiceLines, gatewayInvoiceLines);
			}

			return gatewayInvoiceLines;
		}

		static InvoiceLineAssociatedWithGatewayJob CreateNewInvoiceLineAssociatedWithGatewayJob(DynamicBusinessObject jobChargeTarget)
		{
			var linePK = (ZGuid)jobChargeTarget["JR_AL_ARLine"];
			var relatedJobId = jobChargeTarget["JRT_RelatedJobID"] != DBNull.Value ? (ZGuid)jobChargeTarget["JRT_RelatedJobID"] : ZGuid.Empty;
			var relatedJobNumber = (ZString)jobChargeTarget["RelatedJobNumber"];
			var targetJobId = jobChargeTarget["JRT_InvoiceTargetID"] != DBNull.Value ? (ZGuid)jobChargeTarget["JRT_InvoiceTargetID"] : ZGuid.Empty;
			var targetJobNumber = (ZString)jobChargeTarget["InvoiceTargetJobNumber"];
			var targetJobTableCode = (ZString)jobChargeTarget["JRT_InvoiceTargetTableCode"];
			var gatewayInvoiceLine = new InvoiceLineAssociatedWithGatewayJob(linePK, relatedJobId, relatedJobNumber, targetJobId, targetJobNumber, targetJobTableCode);
			return gatewayInvoiceLine;
		}

		static DynamicBusinessObjectCollection GetJobChargeTargetCollection(IEnumerable<ZGuid> linePKs)
		{
			var query = @"
SELECT 
JR_AL_ARLine,
JRT_RelatedJobID,
JRT_InvoiceTargetID,
JRT_InvoiceTargetTableCode,
targetJob.VJ_JobNumber AS InvoiceTargetJobNumber,
relatedJob.VJ_JobNumber AS RelatedJobNumber
FROM 
dbo.JobCharge
INNER JOIN dbo.JobChargeTarget ON JRT_JR = JR_PK
INNER JOIN dbo.ViewGenericJob targetJob ON targetJob.VJ_PK = JRT_InvoiceTargetID
INNER JOIN dbo.ViewGenericJob relatedJob ON relatedJob.VJ_PK = JRT_RelatedJobID
WHERE 
JR_AL_ARLine IN (SELECT VALUE FROM @LinePKs) AND
targetJob.VJ_TableName IN ('JobShipment', 'JobConsol') AND
relatedJob.VJ_TableName IN ('JobShipment')

UNION ALL

SELECT
JR_AL_ARLine,
JRT_RelatedJobID,
NULL AS JRT_InvoiceTargetID,
'' AS JRT_InvoiceTargetTableCode,
'' AS InvoiceTargetJobNumber,
VJ_JobNumber AS RelatedJobNumber
FROM 
dbo.JobCharge
INNER JOIN dbo.JobChargeTarget ON JRT_JR = JR_PK
INNER JOIN dbo.ViewGenericJob ON VJ_PK = JRT_RelatedJobID
WHERE
JR_AL_ARLine IN (SELECT VALUE FROM @LinePKs) AND
JRT_InvoiceTargetID IS NULL AND
VJ_TableName IN ('JobShipment')

UNION ALL

SELECT 
JR_AL_ARLine,
NULL AS JRT_RelatedJobID,
NULL AS JRT_InvoiceTargetID,
'' AS JRT_InvoiceTargetTableCode,
'' AS InvoiceTargetJobNumber,
'' AS RelatedJobNumber
FROM 
dbo.JobCharge
WHERE
JR_AL_ARLine IN (SELECT VALUE FROM @LinePKs) AND
NOT EXISTS
(
	SELECT JRT_JR
	FROM dbo.JobChargeTarget
	WHERE JRT_JR = JR_PK
)
";

			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@LinePKs", linePKs.ToArray(), AccTransactionLinesSchema.PK, true)
			};

			var loadingFactory = new BusinessObjectFactory();
			var bizObjCollection = new DynamicBusinessObjectCollection(loadingFactory);
			bizObjCollection.Load(query, parameters);
			return bizObjCollection;
		}

		static void SetBlankTargetJobToGatewayConsol(List<InvoicingLineBase> invoiceLines, List<InvoiceLineAssociatedWithGatewayJob> gatewayInvoiceLines)
		{
			var linePKsWithBlankTargetJob = gatewayInvoiceLines.Where(x => x.TargetJobPk == ZGuid.Empty).Select(x => x.LinePK);
			var matchingARLines = invoiceLines.Where(x => linePKsWithBlankTargetJob.Contains(x.PK));
			var arLinesGroupedByJob = matchingARLines.GroupBy(x => x.Job.JH_ParentID);
			foreach (var arLineGroup in arLinesGroupedByJob)
			{
				var targetJobId = arLineGroup.Key;
				var targetJobNumber = arLineGroup.First().Job.JH_JobNum;
				var targetJobTableCode = arLineGroup.First().Job.JH_ParentTableCode;
				foreach (var line in arLineGroup)
				{
					var gatewayInvoiceLine = gatewayInvoiceLines.First(x => x.LinePK == line.PK);
					gatewayInvoiceLine.TargetJobPk = targetJobId;
					gatewayInvoiceLine.TargetJobNumber = targetJobNumber;
					gatewayInvoiceLine.TargetJobParentTableCode = targetJobTableCode;
				}
			}
		}
	}

	internal class InvoiceLineAssociatedWithGatewayJob
	{
		public InvoiceLineAssociatedWithGatewayJob(ZGuid linePK, ZGuid relatedJobPk, ZString relatedJobNumber, ZGuid targetJobPk, ZString targetJobNumber, ZString targetJobParentTableCode)
		{
			LinePK = linePK;
			RelatedJobPk = relatedJobPk;
			RelatedJobNumber = relatedJobNumber;
			TargetJobPk = targetJobPk;
			TargetJobNumber = targetJobNumber;
			TargetJobParentTableCode = targetJobParentTableCode;
		}

		internal ZGuid LinePK { get; private set; }
		internal ZGuid RelatedJobPk { get; private set; }
		internal ZString RelatedJobNumber { get; private set; }
		internal ZGuid TargetJobPk { get; set; }
		internal ZString TargetJobNumber { get; set; }
		internal ZString TargetJobParentTableCode { get; set; }
	}
}
