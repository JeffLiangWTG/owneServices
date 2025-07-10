using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTINFLineProvider : ExitTransportLineProvider, IEXTINFLine
	{
		public EXTINFLineProvider(CusExitReportItem reportItem, string informationType) : base(reportItem)
		{
			report = (CusExitReport)reportItem.Report;
			this.informationType = informationType;
		}
		readonly CusExitReport report;
		readonly string informationType;

		public string ReferenceNumberUCR => informationType != A0132ReportInformationType.Codes.FW ? ConsignmentItem.CCI_UniqueConsignmentReference : null;

		public string RegistrationNumberExternal => informationType != A0132ReportInformationType.Codes.FW ? ConsignmentItem.CCI_ReferenceNumber : null;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans
		{
			get
			{
				if (activeBorderTransportMeans == null && informationType.In(A0132ReportInformationType.Codes.LW, A0132ReportInformationType.Codes.UW))
				{
					activeBorderTransportMeans = new ActiveBorderTransportMeansProvider(report);
				}
				return activeBorderTransportMeans;
			}
		}
		IActiveBorderTransportMeans activeBorderTransportMeans;

		public decimal? CommodityGrossMass => ShouldMapCommodity ? reportItem.ERI_GrossMass.Round(3).Normalize() : null;

		public Decimal CommodityNetMass => ShouldMapCommodity ? reportItem.ERI_NetMass.Normalize() : ZDecimal.Zero;

		public IReadOnlyCollection<IPackageActiveBorderTransportMeans> Packaging
		{
			get
			{
				if (packaging == null)
				{
					var relatedPackages = report.CusExitReportItems.Where(x => x.ERI_CCI_ConsignmentItem == ConsignmentItem.PK);

					packaging = (relatedPackages.Any() && informationType.In(A0132ReportInformationType.Codes.FP, A0132ReportInformationType.Codes.LP, A0132ReportInformationType.Codes.UP))
						? relatedPackages.Select(x => new PackageActiveBorderTransportMeansProvider(x, informationType)).ToArray()
						: Array.Empty<IPackageActiveBorderTransportMeans>();
				}
				return packaging;
			}
		}
		IReadOnlyCollection<IPackageActiveBorderTransportMeans> packaging;

		bool ShouldMapCommodity => CachedValueHelper.GetValue(ref shouldMapCommodity, () => !informationType.In(A0132ReportInformationType.Codes.FW, A0132ReportInformationType.Codes.LV, A0132ReportInformationType.Codes.UV)
			|| ConsignmentItem.Consignment.CXC_MovementReference.IsEmpty);
		CachedValue<bool> shouldMapCommodity;
	}
}
