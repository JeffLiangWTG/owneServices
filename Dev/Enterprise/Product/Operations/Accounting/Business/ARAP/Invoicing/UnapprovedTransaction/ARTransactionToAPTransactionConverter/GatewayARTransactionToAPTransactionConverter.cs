using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal class GatewayARTransactionToAPTransactionConverter : ARTransactionToAPTransactionConverterBase
	{
		public GatewayARTransactionToAPTransactionConverter(NotificationBuffer notificationBuffer, List<InvoiceLineAssociatedWithGatewayJob> invoiceLineAssociatedWithGatewayJobs) : base(notificationBuffer)
		{
			CategorizeInvoiceLinesAssociatedWithGatewayJob(invoiceLineAssociatedWithGatewayJobs);
		}

		List<InvoiceLineAssociatedWithGatewayJob> linesWithTargetJobShipments;
		List<InvoiceLineAssociatedWithGatewayJob> linesWithTargetJobNonGatewayConsol;
		List<InvoiceLineAssociatedWithGatewayJob> linesWithTargetJobGatewayConsol;

		void CategorizeInvoiceLinesAssociatedWithGatewayJob(List<InvoiceLineAssociatedWithGatewayJob> invoiceLineAssociatedWithGatewayJobs)
		{
			var categories = IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(invoiceLineAssociatedWithGatewayJobs);
			if (!categories.TryGetValue(TargetJobTypes.Shipment, out linesWithTargetJobShipments))
			{
				linesWithTargetJobShipments = new List<InvoiceLineAssociatedWithGatewayJob>();
			}

			if (!categories.TryGetValue(TargetJobTypes.NonGTWConsol, out linesWithTargetJobNonGatewayConsol))
			{
				linesWithTargetJobNonGatewayConsol = new List<InvoiceLineAssociatedWithGatewayJob>();
			}

			if (!categories.TryGetValue(TargetJobTypes.GTWConsol, out linesWithTargetJobGatewayConsol))
			{
				linesWithTargetJobGatewayConsol = new List<InvoiceLineAssociatedWithGatewayJob>();
			}
		}

		void SetXmlLineInvoicingJobToTargetJob(TxnHeader xmlInvoiceHeader)
		{
			var linesWithTargetJob = new List<InvoiceLineAssociatedWithGatewayJob>(linesWithTargetJobShipments);
			linesWithTargetJob.AddRange(linesWithTargetJobGatewayConsol);
			var matchingInvoiceLinesAndXmlLines = GetMatchingInvoiceLinesAndXmlLines(xmlInvoiceHeader, linesWithTargetJob);

			foreach (var (lineWithTargetJob, xmlLine) in matchingInvoiceLinesAndXmlLines)
			{
				xmlLine.ConsolOrJobNo = lineWithTargetJob.TargetJobNumber;
				xmlLine.ConsolOrJobType = lineWithTargetJob.TargetJobParentTableCode == JobShipmentSchema.Constants.Prefix ? TxnLineConsolOrJobType.SHP : TxnLineConsolOrJobType.GCN;
				xmlLine.Sequence = ZString.Empty;
			}
		}

		void SetXmlLineRelatedJobAndTargetJob(TxnHeader xmlInvoiceHeader)
		{
			var linesWithTargetJob = new List<InvoiceLineAssociatedWithGatewayJob>(linesWithTargetJobNonGatewayConsol);
			linesWithTargetJob.AddRange(linesWithTargetJobGatewayConsol);
			var matchingInvoiceLinesAndXmlLines = GetMatchingInvoiceLinesAndXmlLines(xmlInvoiceHeader, linesWithTargetJob);

			foreach (var (lineWithTargetJob, xmlLine) in matchingInvoiceLinesAndXmlLines)
			{
				xmlLine.TargetJobID = lineWithTargetJob.TargetJobPk.ToString();
				xmlLine.RelatedJobID = lineWithTargetJob.RelatedJobPk.ToString();
				xmlLine.RelatedJobNumber = lineWithTargetJob.RelatedJobNumber;
			}
		}

		IEnumerable<(InvoiceLineAssociatedWithGatewayJob lineWithTargetJob, TxnLine xmlLine)> GetMatchingInvoiceLinesAndXmlLines(TxnHeader xmlInvoiceHeader, List<InvoiceLineAssociatedWithGatewayJob> linesWithTargetJob)
		{
			var xmlLines = xmlInvoiceHeader.TxnLines.Cast<TxnLine>();
			var matchingInvoiceLinesAndXmlLines = from lineWithTargetJob in linesWithTargetJob
							 join xmlLine in xmlLines
							 on lineWithTargetJob.LinePK.ToString() equals xmlLine.TxnLineGUID.ToString()
							 select (lineWithTargetJob, xmlLine);
			return matchingInvoiceLinesAndXmlLines;
		}

		#region Overrides

		protected override bool ShouldCreateConsolCostsForConsol(IJobCostingPlugIn consol) => linesWithTargetJobNonGatewayConsol.Any();

		protected override void SetBusinessContexts(InvoicingBase apInvoicingBase, bool isAutoImport)
		{
			base.SetBusinessContexts(apInvoicingBase, isAutoImport);
			apInvoicingBase.SetContext(BusinessContext.InterCompanyInvoiceImportedFromGatewayConsol);
		}

		protected override void AdjustXmlInvoiceLineValuesBeforeImport(IJobCostingPlugIn consol, TxnHeader xmlInvoiceHeader, InvoicingBase transaction)
		{
			SetXmlLineInvoicingJobToTargetJob(xmlInvoiceHeader);
			SetXmlLineRelatedJobAndTargetJob(xmlInvoiceHeader);
		}

		protected override (IJobCostingPlugIn consol, IEnumerable<InvoicingLineBase> invoiceLines)[] GetConsolsAndRelatedLines(BusinessObjectFactory factory, IJobCostingPlugIn consol, InvoicingBase apInvoicingBase)
		{
			var targetConsolsAndLines = new List<(IJobCostingPlugIn consol, IEnumerable<InvoicingLineBase> invoiceLines)>();
			var targetJobPks = linesWithTargetJobNonGatewayConsol.Select(x => x.TargetJobPk).Distinct();
			if (targetJobPks.Any())
			{
				var targetConsols = factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.PK, targetJobPks));
				foreach (var targetConsol in targetConsols)
				{
					var linesToConvertToConsolCosts = apInvoicingBase.Lines.Cast<InvoicingLineBase>().Where(x => x.TargetJobIDFromIntercompanyInvoiceImport == targetConsol.PK);
					CalculateTaxDateForInvoiceLinesBasedOnRegistryFCN(apInvoicingBase.AH_InvoiceDate.Date, linesToConvertToConsolCosts, targetConsol);
					targetConsolsAndLines.Add(ValueTuple.Create(targetConsol, linesToConvertToConsolCosts));
				}
			}
			return targetConsolsAndLines.ToArray();
		}

		#endregion
	}
}
