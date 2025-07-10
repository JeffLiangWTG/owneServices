using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsInvoicingSupporter : JobInvoicingSupporter
	{
		public NctsInvoicingSupporter(NctsHeader parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly NctsHeader Parent;

		public override ZGuid OverriddenDepartmentPK => ZGuid.Empty;

		public override ZString ServiceLevel => "STD";

		public override ZString ContainerMode
		{
			get
			{
				var result = ZString.Empty;
				var transportMode = Parent.MovementHeader?.BM_InlandTransportMode ?? ZString.Empty;

				if (Parent.IsPhase5Departure && transportMode == ModeOfTransportList.Codes._1_SeaTransport)
				{
					result = Parent.DepartureHeaderContainers.Any(container => container.BC_Mode == Core.Constants.ContainerModes.Containerised) ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.LCL;
				}
				else
				{
					result = Parent.DepartureHeaderContainers.Any() ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
				}

				return result;
			}
		}

		public override PaymentTermInfos PaymentTerm => new PaymentTermInfos();

		public override OrgHeader Consignee => Parent.Consignee?.Address?.Header;

		public override OrgHeader Consignor => Parent.Consignor?.Address?.Header;

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.CustomsTransitNCTS;

		public override ZString HouseBillNumber
		{
			get
			{
				var shipment = Parent.Shipment;
				return shipment != null ? ((IJobInvoicingPlugIn)shipment).InvoicingSupporter.HouseBillNumber : Parent.BH_JobReference;
			}
		}

		public override ZString MasterBillNumber
		{
			get
			{
				var shipment = Parent.Shipment;
				return shipment != null ? ((IJobInvoicingPlugIn)shipment).InvoicingSupporter.MasterBillNumber : Parent.BH_JobReference;
			}
		}

		public override ZDateTime ATA => ZDateTime.Empty;

		public override ZDateTime ATD => ZDateTime.Empty;

		public override ZDateTime ETA => ZDateTime.Empty;

		public override ZDateTime ETD => ZDateTime.Empty;

		public override ZDecimal ActualChargeable => ActualWeight;

		public override ZString ActualChargeableUnit => ActualWeightUnit;

		public override ZDecimal ActualWeight => Parent.MovementHeader?.TotalGrossMassInKilograms ?? ZDecimal.Zero;

		public override ZString ActualWeightUnit => Core.Constants.Weight.Kilograms;

		public override ZDecimal ActualVolume => ZDecimal.Zero;

		public override ZString ActualVolumeUnit => Core.Constants.Volume.CubicMetres;

		public override bool CreateAccountingJobOnSavingOfOperationsJob => !Parent.IsInDatabaseIncludingChildren && !Parent.IsPluggedIntoShipment && !Parent.IsPluggedIntoConsol;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.EuNctsAudit;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.EuNctsJobInvoicing;
		}

		public override RefUNLOCO Destination => Parent.PortUnlading;

		public ZString ServiceDirection
		{
			get
			{
				var isDepartureMovement = Parent.IsDepartureMovement;
				var isArrivalMovement = Parent.IsArrivalMovement;
				return isDepartureMovement && !isArrivalMovement ? "EXP"
					: !isDepartureMovement && isArrivalMovement ? "IMP"
					: "BTH";
			}
		}

		public override ZString TransportMode
		{
			get
			{
				var result = ZString.Empty;

				if (IsNCTSPhase4)
				{
					result = Parent.TransportModeTranslator.TranslateToCargoWiseCode(Parent.MovementHeader?.BM_ExportTransportMode ?? ZString.Empty);
				}
				else if (Parent.IsDepartureMovement)
				{
					result = GetNCTS5TransportMode(Parent.MovementHeader?.BM_InlandTransportMode ?? ZString.Empty);
				}
				else if (Parent.IsArrivalMovement)
				{
					result = Core.Constants.TransportModes.Other;
				}

				return result;
			}
		}

		ZString GetNCTS5TransportMode(ZString transportMode)
		{
			switch (transportMode)
			{
				case ModeOfTransportList.Codes._1_SeaTransport:
					return Core.Constants.TransportModes.Sea;
				case ModeOfTransportList.Codes._2_RailTransport:
					return Core.Constants.TransportModes.Rail;
				case ModeOfTransportList.Codes._3_RoadTransport:
					return Core.Constants.TransportModes.Road;
				case ModeOfTransportList.Codes._4_AirTransport:
					return Core.Constants.TransportModes.Air;
				case ModeOfTransportList.Codes._5_PostalConsignment:
					return Core.Constants.TransportModes.Mail;
				default:
					return Core.Constants.TransportModes.Unknown;
			}
		}

		public override ZString ConsolType => Parent.IsPluggedIntoConsol ? Core.Constants.JobInvoicingDefaultDepartmentConsolType.All : Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol;

		public override int ContainerCount => Parent.DepartureHeaderContainers.Count;

		public override bool IsNCTSPhase4 => Parent.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS4;

		public override bool IsImport
		{
			get
			{
				var departureMovementHeader = Parent.MovementHeader;
				return departureMovementHeader != null
						&& Parent.BH_RL_NKImportLoadPort != departureMovementHeader.BM_RL_NKDestinationPort
						&& departureMovementHeader.BM_RL_NKDestinationPort == GlbCompany.CurrentCompany.Country.RN_Code;
			}
		}

		public override bool IsExport
		{
			get
			{
				var departureMovementHeader = Parent.MovementHeader;
				var importLoadPort = Parent.BH_RL_NKImportLoadPort;
				return departureMovementHeader != null
						&& importLoadPort != departureMovementHeader.BM_RL_NKDestinationPort
						&& importLoadPort == GlbCompany.CurrentCompany.Country.RN_Code;
			}
		}

		public override bool IsDomestic
		{
			get
			{
				var departureMovementHeader = Parent.MovementHeader;
				return departureMovementHeader != null && Parent.BH_RL_NKImportLoadPort == departureMovementHeader.BM_RL_NKDestinationPort;
			}
		}

		public override RefUNLOCO Origin => Parent.ImportLoadPort;
	}
}
