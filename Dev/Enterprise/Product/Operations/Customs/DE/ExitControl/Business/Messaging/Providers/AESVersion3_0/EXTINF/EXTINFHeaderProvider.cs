using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTINFHeaderProvider : ExitTransportHeaderProvider, IEXTINFHeader
	{
		public EXTINFHeaderProvider(CusExitReport report) : base(report)
		{
		}

		public string ConsignmentModeOfTransportAtTheBorder
		{
			get
			{
				var transportMode = report.CER_TransportMode;
				return transportMode == TransportTypeList.Codes.OwnPropulsion
					? "9"
					: (string)new TransportModeTranslator().TranslateToWCOCode(transportMode);
			}
		}

		public string ConsignmentReferenceNumberUCR => InformationType != A0132ReportInformationType.Codes.FV ? consignment.CXC_UniqueConsignmentReference : null;

		public new string LRN => IsNotTransferInformationType && MRN.IsEmpty() ? base.LRN : null;

		public new string RegistrationNumberExternal => InformationType != A0132ReportInformationType.Codes.FV ? base.RegistrationNumberExternal : null;

		public new IPartyID Declarant => IsNotTransferInformationType && MRN.IsEmpty() ? base.Declarant : null;

		public new IPartyID Representative => IsNotTransferInformationType && MRN.IsEmpty() && base.Declarant == null ? base.Representative : null;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipment
		{
			get
			{
				if (transportEquipment == null)
				{
					var mapNumberOfSeals = Finalization == "1" && InformationType.In(
						A0132ReportInformationType.Codes.FV,
						A0132ReportInformationType.Codes.FW,
						A0132ReportInformationType.Codes.FP,
						A0132ReportInformationType.Codes.LV,
						A0132ReportInformationType.Codes.LW,
						A0132ReportInformationType.Codes.LP,
						A0132ReportInformationType.Codes.NV);
					var consignmentItems = consignment.CusExitConsignmentItems;
					var containersLinkedToConsignment = consignment.Header.CusExitContainers.Where(c =>
							c.CusExitConsignmentPivots.Any(p => consignmentItems.Contains(p.CNP_CCI_ConsignmentItem)))
						.ToArray();
					if (containersLinkedToConsignment.Any())
					{
						transportEquipment = containersLinkedToConsignment.Select(container => new TransportEquipmentProvider(container, consignment, mapNumberOfSeals)).ToArray();
					}
					else
					{
						transportEquipment = new[] { new TransportEquipmentProvider(consignment, mapNumberOfSeals) };
					}
				}
				return transportEquipment;
			}
		}
		IReadOnlyCollection<ITransportEquipment> transportEquipment;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans
		{
			get
			{
				if (activeBorderTransportMeans == null && InformationType.In(A0132ReportInformationType.Codes.LV, A0132ReportInformationType.Codes.UV))
				{
					activeBorderTransportMeans = new ActiveBorderTransportMeansProvider(report);
				}
				return activeBorderTransportMeans;
			}
		}
		IActiveBorderTransportMeans activeBorderTransportMeans;

		public IReadOnlyCollection<IEXTINFLine> Lines
		{
			get
			{
				if (lines == null)
				{
					if (!InformationType.In(A0132ReportInformationType.Codes.NV, A0132ReportInformationType.Codes.FV, A0132ReportInformationType.Codes.LV))
					{
						lines = report.CusExitReportItems.GroupBy(x => x.ERI_CCI_ConsignmentItem).Select(g => new EXTINFLineProvider(g.First(), InformationType)).ToArray();
					}
					else if (InformationType == A0132ReportInformationType.Codes.LV)
					{
						lines = report.CusExitReportItems.Where(x => !x.ConsignmentItem.CCI_ReferenceNumber.IsEmpty).GroupBy(x => x.ERI_CCI_ConsignmentItem).Select(x => new EXTINFLineProvider(x.First(), InformationType)).ToArray();
					}
					else
					{
						lines = Array.Empty<IEXTINFLine>();
					}
				}
				return lines;
			}
		}
		IReadOnlyCollection<IEXTINFLine> lines;

		bool IsNotTransferInformationType => !InformationType.In(A0132ReportInformationType.Codes.UV, A0132ReportInformationType.Codes.UW, A0132ReportInformationType.Codes.UP);
	}
}
