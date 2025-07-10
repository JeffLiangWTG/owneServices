using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyShipment : DocShipment, Integration.DocumentWrappers.IDocAgencyShipment
	{
		protected DocAgencyShipment(AgencyShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static DocAgencyShipment New(AgencyShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return new DocAgencyShipment(shipment, factoryToWrap);
		}

		#region DocWrappers

		public DocServiceLevel ServiceLevel
		{
			get { return DocServiceLevel.New(Shipment.ServiceLevel, Factory); }
		}

		public DocCurrency RateCurr
		{
			get { return DocCurrency.New(Shipment.FrtRateCurrency, Factory); }
		}

		public override DocOrganisation Carrier
		{
			get
			{
				DocOrganisation result = null;

				if (CommonShipment.BookedShippingLine != null)
				{
					result = DocOrganisation.New(Factory, CommonShipment.BookedShippingLine.PK);
				}

				if (result == null)
				{
					result = (Sailing != null && Sailing.Voyage != null) ? Sailing.Voyage.Line : null;
				}

				return result;
			}
		}

		public override DocUNLOCO BookingPortOfLoading
		{
			get { return LoadPort; }
		}

		public override DocUNLOCO BookingPortOfDischarge
		{
			get { return DischargePort; }
		}

		public override DocUNLOCO LoadPort
		{
			get { return DocUNLOCO.New(Factory, Shipment.JS_NKLoadPort); }
		}

		public override DocUNLOCO DischargePort
		{
			get { return DocUNLOCO.New(Factory, Shipment.JS_NKDischargePort); }
		}

		public override DocSailing Sailing
		{
			get { return DocSailing.New(Shipment.Sailing, Factory); }
		}

		public override DocShipmentConsol Consol
		{
			get { return null; }
		}

		public override DocUNLOCO PortOfLoading
		{
			get { return LoadPort; }
		}

		public override DocUNLOCO PortOfDischarge
		{
			get { return DischargePort; }
		}

		public override DocOrganisation ShippingLine
		{
			get { return DocOrganisation.New(Shipment.BookedShippingLine, Factory); }
		}

		public DocOceanBillOfLading BillOfLading
		{
			get { return billOfLading ?? (billOfLading = new DocOceanBillOfLading(Shipment, this)); }
		}
		DocOceanBillOfLading billOfLading;

		protected override IDocContainerCollection NewContainersCollection()
		{
			IDocContainerCollection result = new IDocContainerCollection(Shipment.Factory);

			AgencyShipmentContainerDependentCollection containers = Shipment.IsBillOfLadingStage ? Shipment.RealContainers : Shipment.BookedContainers;
			foreach (AgencyShipmentContainer container in containers)
			{
				result.Add(DocAgencyContainer.New(container, Factory));
			}

			return result;
		}

		public DocAgencyContainerCollection ContainersByTypeAndCommodity
		{
			get
			{
				return GetGroupedContainers(delegate(AgencyShipmentContainer container)
				{
					string part1 = (container.RefContainer != null && !container.RefContainer.RC_Code.IsEmpty)
						? container.RefContainer.RC_Code.ToString() : KeyForEmpty;

					string part2 = (!container.JC_RH_NKContainerCommodityCode.IsEmpty)
						? container.JC_RH_NKContainerCommodityCode.ToString() : KeyForEmpty;

					return part1 + part2;
				});
			}
		}

		public DocAgencyContainerCollection ContainersByReleaseNum
		{
			get
			{
				return GetGroupedContainers(delegate(AgencyShipmentContainer container)
				{
					string part1 = (!container.JC_ReleaseNum.IsEmpty)
						? container.JC_ReleaseNum.ToString() : KeyForEmpty;

					string part2 = (!container.JC_OA_DepartureContainerYardAddress.IsEmpty)
						? container.JC_OA_DepartureContainerYardAddress.ToString() : KeyForEmpty;

					return part1 + part2;
				});
			}
		}

		const string KeyForEmpty = "EMPTY";

		#endregion

		#region ZBool Fields

		public override ZBool PrintAsContainers
		{
			get { return (PackingMode == Core.Constants.ContainerModes.FCL); }
		}

		public ZBool IsAgencyBooking
		{
			get
			{
				return !Shipment.IsBillOfLadingStage;
			}
		}

		public ZBool HasHazardous
		{
			get { return Shipment.HasHazardous; }
		}

		public override ZBool ShowChargeable
		{
			get { return false; }
		}

		public ZBool ShowChargesOnBookingConfirmation
		{
			get { return DocumentsDataRegistry.Instance.ShowChargesOnAgencyBookingConfirmation.Value; }
		}

		public override ZBool ShowChargesOnArrivalNotice
		{
			get { return DocumentsDataRegistry.Instance.ShowChargesOnAgencyArrivalNotice.Value; }
		}

		public new ZBool ShowExchangeRatesOnArrivalNotice
		{
			get { return true; }
		}

		public override ZBool ShowMarksAndNumbersOnBookingConfirmation
		{
			get { return false; }
		}

		public override ZBool ShowEmptyRequiredByHeading
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocAgencyContainer container in Containers)
				{
					if (!container.EmptyRequired.IsEmpty)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}

		public override ZBool ShowFullPickupByHeading
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocAgencyContainer container in Containers)
				{
					if (!container.FullPickDate.IsEmpty)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		#region ZString Fields

		public override ZString EmailSubjectNumber
		{
			get { return BookingReference; }
		}

		public override ZString ETAString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ClientRequestedETA.IsValid ? ClientRequestedETA : ETA); }
		}

		public ZString OrderReferences
		{
			get { return ""; }
		}

		public ZString BookingMode
		{
			get { return Shipment.JS_PackingMode; }
		}

		public ZString VoyageNo
		{
			get
			{
				JobSailing sailing = Shipment.Sailing;
				return sailing == null ? ZString.Empty : sailing.JX_JV_VoyageFlight;
			}
		}

		public ZString VesselName
		{
			get
			{
				JobSailing sailing = Shipment.Sailing;
				return sailing == null ? ZString.Empty : sailing.JX_JV_NKVessel;
			}
		}

		public ZString TotalPackageCountPackType
		{
			get { return Shipment.JS_F3_NKTotalCountPackType; }
		}

		public ZString FCLPickupEquipmentNeeded
		{
			get { return Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public ZString FCLDeliveryEquipmentNeeded
		{
			get { return Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public override ZString BookingTransport
		{
			get
			{
				ZString result = ZString.Empty;
				if (Sailing != null)
				{
					var llloydsNumber = ZString.Empty;
					var vesselName = ZString.Empty;
					if (Sailing.Vessel != null)
					{
						llloydsNumber = Sailing.Vessel.LloydsNumber;
						vesselName = Sailing.Vessel.Code;
					}
					result = FormatTransportDetails(vesselName, Sailing.VoyageFlight, llloydsNumber);
				}
				return result;
			}
		}

		public override ZString EquipmentType
		{
			get { return (!FCLPickupEquipmentNeeded.IsEmpty) ? (FCLPickupEquipmentNeeded + " - " + Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(FCLPickupEquipmentNeeded)) : ""; }
		}

		public override ZString ReceivalDepotReference
		{
			get { return CommonShipment.JS_CFSReference; }
		}

		public override ZString LoadingETDString
		{
			get { return Shipment.Sailing == null ? base.LoadingETDString : BookingETD; }
		}

		public override ZString DischargeETAString
		{
			get { return Shipment.Sailing == null ? base.DischargeETAString : BookingETA; }
		}

		public override ZString PreAlertDocumentHeader
		{
			get { return PackingMode + " " + ReportName; }
		}

		public override ZString BookingContainerReleaseNumber
		{
			get
			{
				foreach (DocAgencyContainer container in Containers)
				{
					if (!container.ReleaseNum.IsEmpty)
					{
						return container.ReleaseNum;
					}
				}
				return ZString.Empty;
			}
		}

		public new ZString Context
		{
			get { return "AgencyShipment"; }
		}

		public override ZString PreAlertReferenceHeading
		{
			get { return Res.GetString("42907237-3bc9-4087-b91e-3c02d0b970c7", "Shippers Reference"); }
		}

		public override ZString PreAlertReference
		{
			get { return Shipment.JS_BookingReference; }
		}

		public override ZString PortDisplayMode
		{
			get { return "LoadDischargeDestination"; }
		}

		public override ZString HouseBillHeading
		{
			get { return Res.GetString("a2205c78-ddcd-481f-8dd5-6e07c756d6a1", "Bill Of Lading"); }
		}

		public override ZString BookingContainerLayout
		{
			get { return "4"; }
		}

		public override ZString Volume
		{
			get { return FormatNumber(Shipment.JS_ActualVolume); }
		}

		public override ZString Weight
		{
			get { return FormatNumber(Shipment.JS_ActualWeight); }
		}

		public override ZString TransportHeading
		{
			get { return BookingTransportHeading; }
		}

		public override ZString TransportInfo
		{
			get { return BookingTransport; }
		}

		public override ZString BookingTransportHeading
		{
			get { return Res.GetString("24cbb04a-f72e-45b9-a444-35c38f61469a", "VESSEL / VOYAGE / IMO(Lloyds)"); }
		}

		public override ZString BookingDepartureReference
		{
			get { return GetDepartureReferenceFromSailing(Sailing); }
		}

		public override ZString BookingDepartureReferenceHeading
		{
			get { return GetDepartureReferenceHeading(BookingPortOfLoading); }
		}

		public override ZString BookingETD
		{
			get { return (Sailing != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Sailing.ETD) : ZString.Empty; }
		}

		public override ZString BookingETA
		{
			get { return (Sailing != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Sailing.ETA) : ZString.Empty; }
		}

		public ZString AdditionalBillClauses
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.AdditionalBillClauses.Description, Shipment); }
		}

		public override ZString BookingReference
		{
			get { return Shipment.JS_CFSReference; }
		}

		public override ZString MasterBillNum
		{
			get
			{
				return Shipment.IsBillOfLadingStage ? Shipment.JS_HouseBill : ZString.Empty;
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime BookingDate
		{
			get { return Shipment.JS_A_BKD; }
		}

		public override ZDateTime BookingContainerEmptyRequiredDate
		{
			get
			{
				foreach (DocAgencyContainer container in Containers)
				{
					if (!container.EmptyRequired.IsEmpty)
					{
						return container.EmptyRequired;
					}
				}
				return ZDateTime.Empty;
			}
		}

		public override ZDateTime BookingContainerPickupFullDate
		{
			get
			{
				foreach (DocAgencyContainer container in Containers)
				{
					if (!container.FullPickDate.IsEmpty)
					{
						return container.FullPickDate;
					}
				}
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalPackageCount
		{
			get { return Shipment.JS_TotalPackageCount; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal UnitFreightRate
		{
			get { return Shipment.JS_UnitFreightRate; }
		}

		#endregion

		#region Booking Party

		public DocContacts BookingParty
		{
			get { return DocContacts.New(Shipment.BookingPartyDocumentaryAddress, Factory); }
		}

		#endregion

		#region Receiving Forwarder

		public override DocOrganisation ReceivingForwarder
		{
			get { return DocOrganisation.New(Shipment.ReceivingForwarderAddress, Factory); }
		}

		#endregion

		#region Sending Forwarder

		public override DocOrganisation SendingForwarder
		{
			get { return DocOrganisation.New(Shipment.SendingForwarderAddress, Factory); }
		}

		#endregion

		#region ReceivingAgentAddress

		public DocAddress ReceivingAgentAddress
		{
			get { return DocAddress.New(Shipment.ReceivingAgentAddress, Factory); }
		}

		#endregion

		#region Notify Parties

		public override ZInt NotifyPartyCount
		{
			get { return WorthyNotifyParties.Length; }
		}

		public override DocContacts NotifyParty
		{
			get { return WorthyNotifyParties.Length < 1 ? null : DocContacts.New(WorthyNotifyParties[0], Factory); }
		}

		public override DocContacts NotifyParty2
		{
			get { return WorthyNotifyParties.Length < 2 ? null : DocContacts.New(WorthyNotifyParties[1], Factory); }
		}

		public override DocContacts NotifyParty3
		{
			get { return WorthyNotifyParties.Length < 3 ? null : DocContacts.New(WorthyNotifyParties[2], Factory); }
		}

		JobDocAddress[] WorthyNotifyParties
		{
			get { return worthyNotifyParties ?? (worthyNotifyParties = GetWorthyNotifyParties()); }
		}
		JobDocAddress[] GetWorthyNotifyParties()
		{
			JobDocAddress[] potentialAddresses = new JobDocAddress[]
			{
				Shipment.NotifyPartyDocumentaryAddress,
				Shipment.NotifyParty2DocumentaryAddress,
				Shipment.NotifyParty3DocumentaryAddress,
			};

			return Array.FindAll(potentialAddresses, IsWorthShowing);
		}
		JobDocAddress[] worthyNotifyParties;

		#endregion

		#region Principal
		public DocOrganisation Principal
		{
			get { return DocOrganisation.New(Factory, Shipment.Principal.PK); }
		}
		#endregion

		#region Freight Debtor

		public DocOrganisation FreightDebtor
		{
			get
			{
				if (freightDebtor == null && JobHeader != null)
				{
					if (FreightCharge != null)
					{
						freightDebtor = FreightCharge.SellAccount;
					}
					else
					{
						freightDebtor = JobHeader.LocalCharges;
					}
				}

				return freightDebtor;
			}
		}
		DocOrganisation freightDebtor;

		#endregion

		#region FreightChargeDebtor

		public DocOrganisation FreightChargeDebtor
		{
			get
			{
				if (freightChargeDebtor == null && FreightCharge != null)
				{
					freightChargeDebtor = FreightCharge.SellAccount;
				}

				return freightChargeDebtor;
			}
		}
		DocOrganisation freightChargeDebtor;

		DocJobCharge FreightCharge
		{
			get
			{
				if (freightCharge == null && JobHeader != null)
				{
					freightCharge = JobHeader.JobCharges.Cast<DocJobCharge>().FirstOrDefault(x => x.JobCharge.JR_AC == Env.Registry.FreightChargeCode && x.JobCharge.SellAccount != null);
				}

				return freightCharge;
			}
		}
		DocJobCharge freightCharge;

		#endregion

		#region TACImage

		public Image TACImage
		{
			get
			{
				Image result = null;

				DeliveryOrder deliveryOrder = GetDeliveryOrderForPrincipal();
				if (deliveryOrder != null)
				{
					result = deliveryOrder.Image;
				}

				return result;
			}
		}

		public ZBool ContainsTACImage
		{
			get { return TACImage != null; }
		}

		public ZBool PrintPerContainer
		{
			get
			{
				ZBool result = false;

				DeliveryOrder deliveryOrder = GetDeliveryOrderForPrincipal();
				if (deliveryOrder != null)
				{
					result = deliveryOrder.PrintParameter == Enterprise.DocumentEngineCore.Registry.DeliveryOrder.PrintConstants.Code.PCT;
				}

				return result;
			}
		}

		public DeliveryOrder GetDeliveryOrderForPrincipal()
		{
			return DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.FindDeliveryOrderForPrincipal(Shipment.JS_OH_DeliveryAgent);
		}

		#endregion

		#region Export / Import

		#region Export

		DocSailing ExportSailing
		{
			get
			{
				if (exportSailing == null)
				{
					var transport = new TransportOrderHelper(Shipment.TransportsIncludingRelated).FirstLeg;
					exportSailing = (transport == null) ? null : DocSailing.New(transport.Sailing, Factory);
				}

				return exportSailing;
			}
		}
		DocSailing exportSailing;

		public override DocDocAddress BookingExportReceivingDepot
		{
			get { return this.ExportReceivingDepot; }
		}

		public override DocDocAddress ExportReceivingDepot
		{
			get { return DocDocAddress.New(Shipment.ExportReceivingDepot, Factory); }
		}

		public ZDateTime ReceivalCommencesDate
		{
			get
			{
				if (ExportSailing != null)
				{
					return HasHazardous ? ExportSailing.HazardousReceivalCommences : ExportSailing.FCLReceivalCommences;
				}

				return ZDateTime.Empty;
			}
		}

		public ZDateTime CutOffDate
		{
			get
			{
				if (ExportSailing != null)
				{
					return HasHazardous ? ExportSailing.HazardousCutOff : ExportSailing.FCLCutOff;
				}

				return ZDateTime.Empty;
			}
		}

		public override ZDateTime BookingCutOffDate
		{
			get { return (ExportSailing == null) ? ZDateTime.Empty : ExportSailing.FCLCutOff; }
		}

		#endregion

		#region Import

		DocSailing ImportSailing
		{
			get
			{
				if (importSailing == null)
				{
					var transport = new TransportOrderHelper(Shipment.TransportsIncludingRelated).LastLeg;
					importSailing = (transport == null) ? null : DocSailing.New(transport.Sailing, Factory);
				}

				return importSailing;
			}
		}
		DocSailing importSailing;

		public override DocDocAddress ImportReleaseDepot
		{
			get { return DocDocAddress.New(Shipment.ImportReleaseDepot, Factory); }
		}

		public ZString CTO
		{
			get
			{
				OrgAddress depot;
				OrgHeader header;
				if ((depot = Shipment.ImportReleaseDepot) != null &&
					(header = depot.Header) != null)
				{
					return header.OH_FullName;
				}

				return ZString.Empty;
			}
		}

		public override ZDateTime AvailableDate
		{
			get { return (ImportSailing == null) ? ZDateTime.Empty : ImportSailing.AvailabilityDate; }
		}

		public override ZDateTime StorageCommenceDate
		{
			get { return (ImportSailing == null) ? ZDateTime.Empty : ImportSailing.StorageDate; }
		}

		#endregion

		#endregion

		#region Sea Legs

		public DocTransportCollection SeaLegs
		{
			get
			{
				if (seaLegs == null)
				{
					seaLegs = new DocTransportCollection(Factory);

					foreach (DocTransport leg in CompleteRouting.Where(x => ((DocTransport)x).TransportModeIsSea))
					{
						SeaLegs.Add(leg);
					}
				}

				return seaLegs;
			}
		}
		DocTransportCollection seaLegs;

		#endregion

		#region Help Methods

		protected ZString FormatTransportDetails(ZString string1, ZString string2, ZString string3)
		{
			ZString result = ZString.Empty;

			if (!string1.IsEmpty)
			{
				result = string1;
			}
			else
			{
				result = "    ";
			}

			if (!string2.IsEmpty)
			{
				result += " / " + string2;
			}
			else
			{
				result += " /    ";
			}

			if (!string3.IsEmpty)
			{
				result += " / " + string3;
			}
			else
			{
				result += " /    ";
			}

			return result;
		}

		#endregion

		#region Implementation

		AgencyShipment Shipment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyShipment)WrappedObject; }
		}

		bool IsWorthShowing(JobDocAddress docAddress)
		{
			if (docAddress.E2_AddressOverride)
			{
				return !docAddress.E2_CompanyName.IsEmpty;
			}
			else
			{
				return !docAddress.E2_OA_Address.IsEmpty;
			}
		}

		DocAgencyContainerCollection GetGroupedContainers(Converter<AgencyShipmentContainer, string> keyProvider)
		{
			DocAgencyContainerCollection result = new DocAgencyContainerCollection(Shipment.Factory);
			Dictionary<string, DocAgencyContainer> lookup = new Dictionary<string, DocAgencyContainer>();

			AgencyShipmentContainerDependentCollection containers = Shipment.IsBillOfLadingStage ? Shipment.RealContainers : Shipment.BookedContainers;

			foreach (AgencyShipmentContainer container in containers)
			{
				string key = keyProvider(container);
				DocAgencyContainer wrapper;

				if (lookup.TryGetValue(key, out wrapper))
				{
					wrapper.QuantityCount += container.JC_ContainerCount;
				}
				else
				{
					wrapper = DocAgencyContainer.New(container, Factory);
					result.Add(wrapper);
					lookup.Add(key, wrapper);
				}
			}

			return result;
		}

		public override string ToString()
		{
			return Shipment.JS_BookingReference;
		}

		#endregion
	}
}
