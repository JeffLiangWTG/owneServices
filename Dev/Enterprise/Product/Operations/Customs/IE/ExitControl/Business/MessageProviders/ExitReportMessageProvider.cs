using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public abstract class ExitReportMessageProvider : Messaging.MessageProvider
	{
		protected ExitReportMessageProvider(CusExitReport exitReport)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			this.exitHeader = Argument.NotNull(exitReport.Header, nameof(exitReport.Header));
			this.exitConsignment = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
		}
		readonly protected CusExitReport exitReport;
		readonly protected CusExitHeader exitHeader;
		readonly protected CusExitConsignment exitConsignment;

		protected IEnumerable<(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, IEnumerable<(ZString packageType, int? packageQuantity, ZString shippingMarks)> packageData)> GetGoodsItemDetails()
		{
			var packageMapping = new Dictionary<CusExitConsignmentItem, Dictionary<CusExitConsignmentPackage, CusExitReportItem>>();
			foreach (CusExitReportItem reportItem in exitReport.CusExitReportItems)
			{
				if (reportItem.ConsignmentItem is CusExitConsignmentItem consignmentItem && reportItem.Package is CusExitConsignmentPackage package)
				{
					var packages = packageMapping.GetOrAdd(consignmentItem, () => new Dictionary<CusExitConsignmentPackage, CusExitReportItem>());
					try
					{
						packages.Add(package, reportItem);
					}
					catch (ArgumentException)
					{
						// ignore duplicate
					}
				}
			}
			foreach (var mapping in packageMapping)
			{
				var reportItem = mapping.Value.First().Value;
				var packages = new List<(CusExitConsignmentPackage package, CusExitReportItem reportItem)>();
				yield return (mapping.Key, reportItem.ERI_GrossMass, reportItem.ERI_NetMass, mapping.Value.Select(x => (x.Key.CXP_PackageType, x.Value.Package.IsBulk ? (int?)null : x.Value.ERI_Quantity, x.Key.CXP_MarksAndNumbers)));
			}
		}
	}
}
