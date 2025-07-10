using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal enum TargetJobTypes
	{
		Shipment,
		GTWConsol,
		NonGTWConsol
	}

	internal static class IntercompanyGatewayARTransactionLineCategorizer
	{
		internal static Dictionary<TargetJobTypes, List<InvoiceLineAssociatedWithGatewayJob>> CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(List<InvoiceLineAssociatedWithGatewayJob> invoiceLinesAssociatedWithGateway)
		{
			var lineCategories = new Dictionary<TargetJobTypes, List<InvoiceLineAssociatedWithGatewayJob>>();
			if (invoiceLinesAssociatedWithGateway.Any())
			{
				var linesGroupedByTargetTableCode = invoiceLinesAssociatedWithGateway.GroupBy(x => x.TargetJobParentTableCode);
				foreach (var lineGroup in linesGroupedByTargetTableCode)
				{
					if (lineGroup.Key == JobShipmentSchema.Constants.Prefix)
					{
						lineCategories.Add(TargetJobTypes.Shipment, lineGroup.ToList());
					}
					else if (lineGroup.Key == JobConsolSchema.Constants.Prefix)
					{
						CategorizeLinesWithTargetJobConsol(lineCategories, lineGroup);
					}
					else
					{
						ErrorReporter.ReportOnce("IntercompanyGatewayARTransaction_UnknownTargetJobParentTableCode", FormattableString.Invariant($"Unknown target parent table code '{lineGroup.Key}' found in gateway AR invoice."));
					}
				}
			}
			return lineCategories;
		}

		static void CategorizeLinesWithTargetJobConsol(Dictionary<TargetJobTypes, List<InvoiceLineAssociatedWithGatewayJob>> lineCategories, IGrouping<ZString, InvoiceLineAssociatedWithGatewayJob> linesWithTargetJobConsol)
		{
			var linesWithTargetJobGatewayConsol = new List<InvoiceLineAssociatedWithGatewayJob>();
			var linesWithTargetJobNonGatewayConsol = new List<InvoiceLineAssociatedWithGatewayJob>();
			var linesGroupedByConsolPk = linesWithTargetJobConsol.GroupBy(x => x.TargetJobPk);
			var consolLoadingFactory = new BusinessObjectFactory();
			foreach (var lineGroup in linesGroupedByConsolPk)
			{
				var consol = consolLoadingFactory.Load<ForwardingConsol>(lineGroup.Key);
				if (consol != null)
				{
					var isGatewayConsolInCurrentCompany = ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled();
					if (isGatewayConsolInCurrentCompany)
					{
						linesWithTargetJobGatewayConsol.AddRange(lineGroup);
					}
					else
					{
						linesWithTargetJobNonGatewayConsol.AddRange(lineGroup);
					}
				}
			}

			if (linesWithTargetJobGatewayConsol.Any())
			{
				lineCategories.Add(TargetJobTypes.GTWConsol, linesWithTargetJobGatewayConsol);
			}

			if (linesWithTargetJobNonGatewayConsol.Any())
			{
				lineCategories.Add(TargetJobTypes.NonGTWConsol, linesWithTargetJobNonGatewayConsol);
			}
		}
	}
}
