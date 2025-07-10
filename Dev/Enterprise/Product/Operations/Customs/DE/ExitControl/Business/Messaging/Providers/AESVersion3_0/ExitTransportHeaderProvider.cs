using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class ExitTransportHeaderProvider : ExitHeaderProvider, IExitTransportHeader
	{
		public ExitTransportHeaderProvider(CusExitReport report) : base(report)
		{
		}

		public string Finalization => InformationType.In(A0132ReportInformationType.Codes.UV, A0132ReportInformationType.Codes.UW, A0132ReportInformationType.Codes.UP) ? "1" : report.CER_IsFinalized.MapBoolTo10();

		public string InformationType => (informationType ?? (informationType = GetInformationType()));
		string informationType;

		string GetInformationType()
		{
			var reportItems = report.CusExitReportItems;
			var oneOrMoreConsignmentItemsNotSelectedOrNotEqualInWeight = false;
			var oneOrMorePackagesNotSelectedOrNotEqualInQuantity = false;
			foreach (var consignmentItem in report.Consignment.CusExitConsignmentItems)
			{
				if (!reportItems.Any(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK && x.ERI_GrossMass == consignmentItem.CCI_GrossMass && x.ERI_NetMass == consignmentItem.CCI_NetMass))
				{
					oneOrMoreConsignmentItemsNotSelectedOrNotEqualInWeight = true;
					continue;
				}

				foreach (var pivot in consignmentItem.CusExitConsignmentPackagePivots)
				{
					if (!reportItems.Any(x => x.ERI_CXP_Package == pivot.CNP_CXP_Package && x.ERI_Quantity == pivot.Package.CXP_Quantity))
					{
						oneOrMorePackagesNotSelectedOrNotEqualInQuantity = true;
						break;
					}
				}
			}

			var noneSelected = reportItems.Count == 0;
			var isConsignmentLevel = !noneSelected && !oneOrMoreConsignmentItemsNotSelectedOrNotEqualInWeight && !oneOrMorePackagesNotSelectedOrNotEqualInQuantity;
			var isItemLevel = !noneSelected && oneOrMoreConsignmentItemsNotSelectedOrNotEqualInWeight && !oneOrMorePackagesNotSelectedOrNotEqualInQuantity;
			var isPackageLevel = oneOrMorePackagesNotSelectedOrNotEqualInQuantity;

			var cxcStatus = report.Consignment.CXC_Status;

			if (!int.TryParse(cxcStatus, out var consignmentStatus))
			{
				consignmentStatus = 0;
			}

			if (report.CER_Calc_Discrepancies)
			{
				if (consignmentStatus < statusReleasedForExit)
				{
					if (isPackageLevel)
					{
						return A0132ReportInformationType.Codes.FP;
					}
					else if (isConsignmentLevel)
					{
						return A0132ReportInformationType.Codes.FV;
					}
					else if (isItemLevel)
					{
						return A0132ReportInformationType.Codes.FW;
					}
				}
			}
			else if (consignmentStatus == statusReleasedForExit)
			{
				if (isPackageLevel)
				{
					return A0132ReportInformationType.Codes.UP;
				}
				else if (isConsignmentLevel)
				{
					return A0132ReportInformationType.Codes.UV;
				}
				else if (isItemLevel)
				{
					return A0132ReportInformationType.Codes.UW;
				}
			}
			else if (consignmentStatus < statusReleasedForExit)
			{
				if (isPackageLevel)
				{
					return A0132ReportInformationType.Codes.LP;
				}
				else if (isConsignmentLevel)
				{
					return A0132ReportInformationType.Codes.LV;
				}
				else if (isItemLevel)
				{
					return A0132ReportInformationType.Codes.LW;
				}
				else if (noneSelected)
				{
					return A0132ReportInformationType.Codes.NV;
				}
			}

			return A0132ReportInformationType.Codes.NV;
		}

		static readonly int statusReleasedForExit = int.Parse(A0116ATLASStatusCodeList.Codes._353);
	}
}



