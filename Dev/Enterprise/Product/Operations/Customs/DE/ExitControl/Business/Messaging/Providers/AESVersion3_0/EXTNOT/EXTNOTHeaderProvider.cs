using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTNOTHeaderProvider : ExitHeaderProvider, IEXTNOTHeader
	{
		public EXTNOTHeaderProvider(CusExitReport report) : base(report)
		{
		}

		public string IntendedExitCustomsOffice => ShipmentType.In(A0131ATLASTypeOfShipment.Codes.WV, A0131ATLASTypeOfShipment.Codes.WW) ? report.CER_OfficeOfExport.ValueOrNullIfEmpty() : null;

		public DateTime? ExitDateAndTime => report.CER_DateTime.IsValid
			&& new[] { A0131ATLASTypeOfShipment.Codes.AP, A0131ATLASTypeOfShipment.Codes.AV, A0131ATLASTypeOfShipment.Codes.AW }.Contains(ShipmentType)
			? report.CER_DateTime.ToUtcDateTime() : null;

		public DateTime? ForwardingDateAndTime => report.CER_DateTime.IsValid
			&& new[] { A0131ATLASTypeOfShipment.Codes.WV, A0131ATLASTypeOfShipment.Codes.WW }.Contains(ShipmentType)
			? report.CER_DateTime.ToUtcDateTime() : null;

		public string Finalization => report.CER_IsFinalized.MapBoolTo10();

		public string ShipmentType => (shipmentType ?? (shipmentType = GetShipmentType()));
		string shipmentType;

		string GetShipmentType()
		{
			var reportItems = report.CusExitReportItems;
			var oneOrMoreConsignmentItemsNotSelected = false;
			var oneOrMorePackagesNotSelected = false;
			foreach (var consignmentItem in report.Consignment.CusExitConsignmentItems)
			{
				if (!reportItems.Any(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK))
				{
					oneOrMoreConsignmentItemsNotSelected = true;
					continue;
				}

				foreach (var pivot in consignmentItem.CusExitConsignmentPackagePivots)
				{
					if (!reportItems.Any(x => x.ERI_CXP_Package == pivot.CNP_CXP_Package))
					{
						oneOrMorePackagesNotSelected = true;
						break;
					}
				}
			}

			var noneSelected = reportItems.Count == 0;
			var isConsignmentLevel = !noneSelected && !oneOrMoreConsignmentItemsNotSelected && !oneOrMorePackagesNotSelected;
			var isItemLevel = !noneSelected && oneOrMoreConsignmentItemsNotSelected && !oneOrMorePackagesNotSelected;
			var isPackageLevel = oneOrMorePackagesNotSelected;

			if (report.CER_OfficeOfExport.IsEmpty)
			{
				if (report.CER_Calc_Discrepancies == false)
				{
					if (isPackageLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.AP;
					}
					else if (isConsignmentLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.AV;
					}
					else if (isItemLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.AW;
					}
					else if (noneSelected)
					{
						return A0131ATLASTypeOfShipment.Codes.NV;
					}
				}
				else
				{
					if (isPackageLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.FP;
					}
					else if (isConsignmentLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.FV;
					}
					else if (isItemLevel)
					{
						return A0131ATLASTypeOfShipment.Codes.FW;
					}
				}
			}
			else if (!report.CER_Calc_Discrepancies)
			{
				if (isConsignmentLevel)
				{
					return A0131ATLASTypeOfShipment.Codes.WV;
				}
				else if (isItemLevel)
				{
					return A0131ATLASTypeOfShipment.Codes.WW;
				}
			}

			return string.Empty;
		}

		public IReadOnlyCollection<IEXTNOTLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = shipmentTypesWithGoodsItemsMapped.Contains(ShipmentType)
						? report.CusExitReportItems.GroupBy(x => x.ERI_CCI_ConsignmentItem)
							.Select(x => new EXTNOTLineProvider(x.First(), ShipmentType)).ToArray()
						: Array.Empty<IEXTNOTLine>();
				}
				return lines;
			}
		}
		IReadOnlyCollection<IEXTNOTLine> lines;

		readonly HashSet<string> shipmentTypesWithGoodsItemsMapped = new HashSet<string>()
		{
			A0131ATLASTypeOfShipment.Codes.FW,
			A0131ATLASTypeOfShipment.Codes.FP,
			A0131ATLASTypeOfShipment.Codes.AW,
			A0131ATLASTypeOfShipment.Codes.AP,
			A0131ATLASTypeOfShipment.Codes.WW,
		};
	}
}
