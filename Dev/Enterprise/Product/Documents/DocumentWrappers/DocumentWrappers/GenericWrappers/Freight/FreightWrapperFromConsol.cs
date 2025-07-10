using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromConsol : FreightWrapper, IConsolDeliveryAgent
	{
		public FreightWrapperFromConsol(ForwardingConsol consolBO, BusinessObjectFactory factory)
			: base(consolBO, factory)
		{
			Argument.NotNull(factory, "factory");
			ConsolBO = consolBO;
		}
		readonly ForwardingConsol ConsolBO;

		#region Co2e Properties

		protected override ZString GetFormattedTotalCO2e()
		{
			if (ConsolBO.GetCO2eStatus() == CO2eStatusList.Codes.Current)
			{
				return CO2eHelper.GetFormattedCO2e(ConsolBO.GetTotalCO2e());
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			if (ConsolBO.HaveJobCO2e)
			{
				return (ConsolBO.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region Related Business Objects

		protected override ForwardingConsol GetConsol()
		{
			return ConsolBO;
		}

		protected override Job GetJob()
		{
			return ConsolBO == null ? null : (Job)ConsolBO.Job;
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return ConsolBO.Logs.CreatedDateUtc;
		}

		#endregion

		#region General Freight References/Fields

		protected override ZString GetLocalForwarderReference()
		{
			return ConsolBO.JK_AgentsReference;
		}

		protected override ZString GetExportAgentsReference()
		{
			return ConsolBO.JK_AgentsReference;
		}

		protected override ZString GetImportAgentsReference()
		{
			return ConsolBO.JK_AgentsReference;
		}

		protected override ZString GetGoodsDescription()
		{
			return Res.GetString("3b6d89e9-4788-4ec7-ab21-a03a0ee3460a", "Consolidated Cargo");
		}

		protected override ZString GetOwnerReference()
		{
			return Consol.JK_AgentsReference;
		}

		#region GetTransportReference

		protected override ZString GetTransportReference()
		{
			var result = ZString.Empty;

			var vesselName = Consol.JK_JX_JV_NKVessel;
			var voyageNumber = Consol.JK_JX_JV_VoyageFlight;

			switch (Consol.TransportMode)
			{
				case Constants.TransportModes.Air:

					result = Consol.Transports.Count == 1
						? GetAirTransportAndLegDetails()
						: GetFirstAndSecondLegAirTransportDetails();

					break;

				case Constants.TransportModes.Rail:
				case Constants.TransportModes.Road:

					result = string.Concat(vesselName, " / ", voyageNumber);

					break;

				case Constants.TransportModes.Sea:

					var vessel = Consol.Vessel;
					var lloydsNumber = vessel == null ? ZString.Empty : vessel.RV_LloydsNumber;

					result = FormatTransportDetails(vesselName, voyageNumber, lloydsNumber);

					break;
			}

			return result;
		}

		#region GetAirTransportAndLegDetails

		ZString GetAirTransportAndLegDetails()
		{
			var loadPort = Consol.Transports[0].JW_RL_NKLoadPort;
			var dischargePort = Consol.Transports[0].JW_RL_NKDiscPort;

			var departureTime = Consol.JK_JX_JA_A_DEP.IsEmpty ? Consol.JK_JX_JA_E_DEP : Consol.JK_JX_JA_A_DEP;

			if (dischargePort == Consol.JK_JX_JB_RL_NKPortOfDischarge && loadPort == Consol.JK_JX_JA_RL_NKPortOfLoading)
			{
				return FormatFlightDetails(Consol.JK_JX_JV_VoyageFlight, Consol.JK_JX_JB_RL_NKPortOfDischarge, departureTime);
			}
			else
			{
				return string.Concat
				(
					FormatFlightDetails(Consol.JK_JX_JV_VoyageFlight, Consol.JK_JX_JB_RL_NKPortOfDischarge, departureTime),
					" -> ",
					FormatFlightDetails(Consol.Transports[0].JW_VoyageFlight, dischargePort, Consol.Transports[0].JW_ETD)
				);
			}
		}

		ZString FormatFlightDetails(ZString voyageNo, ZString portOfDischarge, ZDateTime etd)
		{
			var departureDate = Suppression.GetValue(etd.ToShortDateString(), Consol, SuppressFields.ETD, "*", ContactType.Consignor);
			return FormatTransportDetails(voyageNo, portOfDischarge, departureDate);
		}

		#endregion

		#region GetFirstAndSecondLegAirTransportDetails

		ZString GetFirstAndSecondLegAirTransportDetails()
		{
			var result = ZString.Empty;

			var query = new ZQuery(JobConsolTransportSchema.JW_TransportType, SQLComparisonOperator.NotEqual, Constants.TransportPlanningType.Other)
			{
				OrderBy = JobConsolTransportSchema.Constants.JW_TransportType
			};

			var legs = Consol.Transports.Find(query);

			var firstLeg = legs.FirstOrDefault();
			var secondLeg = legs.Length > 1 ? legs[1] : null;

			if (firstLeg == null || secondLeg == null)
			{
				query = new ZQuery(JobConsolTransportSchema.JW_TransportType, Constants.TransportPlanningType.Other)
				{
					OrderBy = JobConsolTransportSchema.Constants.JW_ETD
				};

				legs = Consol.Transports.Find(query);

				if (firstLeg == null)
				{
					firstLeg = legs.FirstOrDefault();
				}
				else
				{
					secondLeg = legs.FirstOrDefault();
				}

				secondLeg = secondLeg ?? (legs.Length > 1 ? legs[1] : null);
			}

			var firstTransport = firstLeg as Transport;
			if (firstTransport != null)
			{
				result = FormatFlightDetails(firstTransport.JW_VoyageFlight, firstTransport.JW_RL_NKDiscPort, firstTransport.JW_ETD);
			}

			var secondTransport = secondLeg as Transport;
			if (secondTransport != null)
			{
				result += " -> ";
				result += FormatFlightDetails(secondTransport.JW_VoyageFlight, secondTransport.JW_RL_NKDiscPort, secondTransport.JW_ETD);
			}

			return result;
		}

		#endregion

		#region FormatTransportDetails

		ZString FormatTransportDetails(ZString string1, ZString string2, ZString string3)
		{
			ZString result;

			if (!string.IsNullOrWhiteSpace(string1))
			{
				result = string1;
			}
			else
			{
				result = "    ";
			}

			if (!string.IsNullOrWhiteSpace(string2))
			{
				result += " / " + string2;
			}
			else
			{
				result += " /    ";
			}

			if (!string.IsNullOrWhiteSpace(string3))
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

		#endregion

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return ConsolBO.JK_ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return ConsolBO.JK_NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return ConsolBO.JK_NoCopyBills;
		}

		protected override ZString GetShippersReference()
		{
			if (ConsolBO.IsDirect)
			{
				var directShipment = ConsolBO.DirectShipment;
				if (directShipment != null)
				{
					return (!directShipment.JS_BookingReference.IsEmpty) ? directShipment.JS_BookingReference : directShipment.JS_UniqueConsignRef;
				}
			}

			return ZString.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("04808842-5966-43f8-95f4-0b83f58890dd", "Consol");
		}

		protected override ZString GetJobNumber()
		{
			return ConsolBO.JK_UniqueConsignRef;
		}

		protected override ZString GetSecondaryHeading()
		{
			return OverriddenSecondaryHeading;
		}

		internal ZString OverriddenSecondaryHeading { get; set; }

		protected override ZString GetSecondaryNumber()
		{
			return OverriddenSecondaryJobNumber;
		}

		internal ZString OverriddenSecondaryJobNumber { get; set; }

		protected override ZString GetArrivalReference()
		{
			return ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null
				? ConsolBO.Schedule.Destination.JB_ArrivalReference
				: ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null
				? ConsolBO.Schedule.Destination.JB_Berth
				: ZString.Empty;
		}

		protected override ZString GetMasterBillHeading()
		{
			switch (ConsolTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("b813b254-acda-4304-9023-3c054fb214e9", "MAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("796a182c-7d8e-47ef-aa95-1401ccca0f5c", "Ocean Bill Of Lading");

				default:
					return Res.GetString("0347e369-92ca-4455-9276-d259edbfdfb8", "Master Bill");
			}
		}

		protected override ZString GetHouseBillHeading()
		{
			switch (ConsolTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("337e0e5e-fe5b-4aba-a39e-2de99b2bfee4", "HAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("9cac46f5-90a0-407b-ad83-0a942124fd4b", "House Bill Of Lading");

				default:
					return Res.GetString("d9239b14-fac4-432d-99d1-3741dc9e63e5", "House Bill");
			}
		}

		protected override ZDecimal GetLoadingMeters()
		{
			return ConsolBO.GetTotalShipmentLoadingMetersForDoc(WeightVolumeDisplay);
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return CartageInfo.FullHandlingInstructions;
		}

		protected override ZString GetFullCartageInstructions()
		{
			return CartageInfo.FullCartageInstructions;
		}

		#endregion

		#region Consol Level String Fields

		protected override ZString GetFreightDepotType()
		{
			switch (ConsolBO.JK_ConsolMode)
			{
				case Core.Constants.ContainerModes.FCL:
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
				case Core.Constants.ContainerModes.BreakBulk:
					return "CTO";

				case Core.Constants.ContainerModes.LCL:
				case Core.Constants.ContainerModes.Other:
				default:
					return "CFS";
			}
		}

		protected override ZString GetBookingReference()
		{
			ZString[] carrierBookingReferenceNumbers = ConsolBO.Numbers.Cast<CusEntryNumber>()
				.Where(number => number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG && !number.CE_EntryNum.Trim().IsEmpty)
				.Select(number => number.CE_EntryNum.Trim())
				.ToArray();

			if (carrierBookingReferenceNumbers.Length == 0)
			{
				return ConsolBO.JK_BookingReference;
			}
			else if (ConsolBO.JK_BookingReference.IsEmpty)
			{
				return ZString.Join(", ", carrierBookingReferenceNumbers);
			}
			else
			{
				return ConsolBO.JK_BookingReference + ", " + ZString.Join(", ", carrierBookingReferenceNumbers);
			}
		}

		protected override ZString GetMasterBill()
		{
			return ConsolBO.JK_MasterBillNum;
		}

		protected override ZDateTime GetMasterBillIssue()
		{
			return ConsolBO.IsAir ? ConsolBO.JK_MasterBillIssueDate : ZDateTime.Empty;
		}

		protected override ZString GetConsolPaymentType()
		{
			return ConsolBO.JK_PrepaidCollect;
		}

		protected override ZString GetConsolNumber()
		{
			return ConsolBO.JK_UniqueConsignRef;
		}

		protected override ZString GetCarrierContractNumber()
		{
			if (!ConsolBO.JK_CarrierContractNumber.IsEmpty)
			{
				return ConsolBO.JK_CarrierContractNumber;
			}
			else
			{
				return ConsolBO.Numbers.Cast<CusEntryNumber>()
				.Where(number => number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON && !number.CE_EntryNum.Trim().IsEmpty)
				.Select(number => number.CE_EntryNum.Trim()).FirstOrDefault();
			}
		}

		#endregion

		#region CodeAndDescriptions
		protected override CodeAndDescriptionWrapper GetConsolType()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_AgentType, ConsolBO.JK_AgentType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolContainerMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_ConsolMode, ConsolBO.JK_ConsolMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolTransportMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_TransportMode, ConsolBO.JK_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			ZString consolTypeCode = ZString.Empty;
			if (ConsolBO.IsImport())
			{
				consolTypeCode = "IMP";
			}
			else if (ConsolBO.IsExport())
			{
				consolTypeCode = "EXP";
			}
			else if (ConsolBO.IsDomestic())
			{
				consolTypeCode = "DOM";
			}

			return new CodeAndDescriptionWrapper(consolTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_ConsolStatus, new CodeDescriptionPairList(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_ConsolMode, ConsolBO.JK_ConsolMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_TransportMode, ConsolBO.JK_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			if (Consol.ShippingLine != null)
			{
				return new CodeAndDescriptionWrapper(Consol.JK_AWBServiceLevel, Consol.ShippingLine.MiscServ.CarrierServiceLevels, Factory);
			}
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(ConsolBO.JK_PrepaidCollect, ConsolBO.JK_PrepaidCollect_List, IncoTermWrapper.Deciders.ByPaymentType(ConsolBO.JK_PrepaidCollect), Factory);
		}

		protected override MoneyWrapper GetCollectAmount()
		{
			if (ConsolBO.JK_PrepaidCollect == Constants.PaymentType.Collect)
			{
				RefCurrency currency = ConsolBO.FreightCostsCurrency ?? GlbCompany.CurrentCompany.LocalCurrency;
				return new MoneyWrapper(new Money(ConsolBO.FreightCostsAmount, currency), Factory);
			}
			else
			{
				return new MoneyWrapper(Money.Empty, Factory);
			}
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_ReleaseType, ConsolBO.JK_ReleaseType_List, Factory);
		}

		#endregion

		#region Organisations

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, ConsolBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (ConsolBO != null && ConsolBO.PackDepotAddress != null)
			{
				return new AddressWrapper(OrganisationUsageType.PackingLocation, ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			if (ConsolBO != null && ConsolBO.ArrivalCTOAddress != null)
			{
				return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			if (ConsolBO != null && ConsolBO.DepartureCTOAddress != null)
			{
				return new AddressWrapper(OrganisationUsageType.CTODeparture, ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			if (ConsolBO != null)
			{
				if (ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
				{
					if (ConsolBO.DepartureCTOAddress != null)
					{
						return new AddressWrapper(OrganisationUsageType.CTODeparture, ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
					}
				}
				else
				{
					if (ConsolBO.PackDepotAddress != null)
					{
						return new AddressWrapper(OrganisationUsageType.PackingLocation, ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
					}
				}
			}
			return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			return new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, ConsolBO.CreditorAddress, ContactType.Payables, Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			if (ConsolBO.DirectShipment != null)
			{
				DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(ConsolBO.DirectShipment);
				ZString temp = docNote.GetFieldValueAsString((NoResString)"Consignor - Shipper");
				return temp.IsEmpty
					? new OrganisationWrapper(OrganisationUsageType.Consignor, ConsolBO.DirectShipment.ConsignorDocumentaryAddress, Factory)
					: new OrganisationWrapper(OrganisationUsageType.Consignor, temp, Factory);
			}
			return new OrganisationWrapper(OrganisationUsageType.Consignor, ConsolBO.SendingForwarderAddress, ContactType.Consignor, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			if (ConsolBO.DirectShipment != null)
			{
				DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(ConsolBO.DirectShipment);
				ZString temp = docNote.GetFieldValueAsString((NoResString)"Consignee - Importer");

				return temp.IsEmpty
					? new OrganisationWrapper(OrganisationUsageType.Consignee, ConsolBO.DirectShipment.ConsigneeDocumentaryAddress, Factory)
					: new OrganisationWrapper(OrganisationUsageType.Consignee, temp, Factory);
			}

			return new OrganisationWrapper(OrganisationUsageType.Consignee, ConsolBO.ReceivingForwarderAddress, ContactType.Consignee, Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return ConsolBO.IsExport()
				? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory)
				: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportAgent, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetMasterBillShipperOverride()
		{
			if (ConsolBO.IsSea && !ConsolBO.IsDirect)
			{
				return new OrganisationWrapper(OrganisationUsageType.MasterBillShipperOverride, ConsolBO.MasterBillShipperOverrideDocumentaryAddress, Factory);
			}
			return base.GetMasterBillShipperOverride();
		}

		protected override OrganisationWrapper GetMasterBillConsigneeOverride()
		{
			if (ConsolBO.IsSea && !ConsolBO.IsDirect)
			{
				return new OrganisationWrapper(OrganisationUsageType.MasterBillConsigneeOverride, ConsolBO.MasterBillConsigneeOverrideDocumentaryAddress, Factory);
			}
			return base.GetMasterBillConsigneeOverride();
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			OrganisationWrapper result = null;

			if (ConsolBO.DirectShipment != null)
			{
				DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(ConsolBO.DirectShipment);
				ZString notifyPartyUDF = docNote.GetFieldValueAsString((NoResString)"Notify Party");

				result = !notifyPartyUDF.IsEmpty
					? new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory) { ContactNameText = notifyPartyUDF }
					: GetNotifyOrganisation(ConsolBO.DirectShipment.NotifyPartyDocumentaryAddress);
			}
			else
			{
				result = GetNotifyOrganisation(ConsolBO.NotifyPartyDocumentaryAddress) ?? new OrganisationWrapper(OrganisationUsageType.NotifyParty, ConsolBO.ReceivingForwarderAddress, ContactType.NotifyParty, Factory);
			}

			return result ?? new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetNotifyParty2()
		{
			OrganisationWrapper result = null;

			result = ConsolBO.DirectShipment != null
				? GetNotifyOrganisation(ConsolBO.DirectShipment.NotifyParty2DocumentaryAddress)
				: GetNotifyOrganisation(ConsolBO.NotifyParty2DocumentaryAddress);

			return result ?? new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetNotifyParty3()
		{
			OrganisationWrapper result = null;

			result = ConsolBO.DirectShipment != null
				? GetNotifyOrganisation(ConsolBO.DirectShipment.NotifyParty3DocumentaryAddress)
				: GetNotifyOrganisation(ConsolBO.NotifyParty3DocumentaryAddress);

			return result ?? new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		OrganisationWrapper GetNotifyOrganisation(JobDocAddress notifyPartyDocumentaryAddress)
		{
			OrganisationWrapper result = null;

			if (notifyPartyDocumentaryAddress != null && !notifyPartyDocumentaryAddress.IsEmpty)
			{
				result = new OrganisationWrapper(OrganisationUsageType.NotifyParty, notifyPartyDocumentaryAddress, Factory);
			}

			return result;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			OrgAddress address = deliveryAgent == null ? null : deliveryAgent.MainAddress;
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, address, ContactType.All, Factory);
		}
		DeliveryAgentOrgHeader deliveryAgent;

		public void SetDeliveryAgent(DeliveryAgentOrgHeader deliveryAgent)
		{
			this.deliveryAgent = deliveryAgent;
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = null;
			if (ConsolBO.ArrivalCTOAddress != null && ConsolBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = ConsolBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetArrivalCFSTransport()
		{
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCFSTransport, Consol.ArrivalUnpackCFSTransport, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetDepartureCFSTransport()
		{
			return new OrganisationWrapper(OrganisationUsageType.DepartureCFSTransport, Consol.DeparturePackCFSTransport, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCarrierBookingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.CarrierBookingAgent, ConsolBO.CarrierBookingAgentDocumentaryAddress ?? Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetCarrierHandlingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.CarrierHandlingAgent, ConsolBO.CarrierHandlingAgentDocumentaryAddress ?? Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#region Addresses

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return new AddressWrapper(Consol.PackDepotAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			return new AddressWrapper(OrganisationUsageType.UnpackingLocation, Consol.UnpackDepotAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			if (ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.BreakBulk || ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.RollOnRollOff
				|| (Consol.JK_TransportMode == Core.Constants.TransportModes.Sea && (Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL || Consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol)))
			{
				return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.All, Factory);
			}
			else
			{
				return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
			}
		}

		protected override AddressWrapper GetContainerYardEmptyPickupAddress()
		{
			return new AddressWrapper((ConsolBO.ContainerYardEmptyPickupAddress), ContactType.All, Factory);
		}

		protected override AddressWrapper GetContainerYardEmptyReturnAddress()
		{
			return new AddressWrapper((ConsolBO.ContainerYardEmptyReturnAddress), ContactType.All, Factory);
		}

		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			foreach (Transport transport in ConsolBO.Transports)
			{
				if (transport.JW_RL_NKLoadPort == ConsolBO.JK_RL_NKLoadPort)
				{
					return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKLoadPort, transport.JW_ETD, transport.JW_ATD, Factory);
				}
			}
			return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKLoadPort, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			foreach (Transport transport in ConsolBO.Transports)
			{
				if (transport.JW_RL_NKDiscPort == ConsolBO.JK_RL_NKDischargePort)
				{
					return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKDischargePort, transport.JW_ETA, transport.JW_ATA, Factory);
				}
			}
			return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKDischargePort, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override LocationWrapper GetPickupLocation()
		{
			if (ConsolBO.PickupLocation != null)
			{
				return new LocationWrapper(ConsolBO.PickupLocation.RL_Code, Factory);
			}
			else
			{
				return new LocationWrapper(null, Factory);
			}
		}

		protected override LocationWrapper GetDeliveryLocation()
		{
			if (ConsolBO.DeliveryLocation != null)
			{
				return new LocationWrapper(ConsolBO.DeliveryLocation.RL_Code, Factory);
			}
			else
			{
				return new LocationWrapper(null, Factory);
			}
		}

		protected override LocationWrapper GetFreightPayableAt()
		{
			return new LocationWrapper((ConsolBO.FreightPayableAt == null ? null : ConsolBO.FreightPayableAt.RL_Code), Factory);
		}

		protected override PlaceAndDateWrapper GetFirstForeignPort()
		{
			return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKFirstForeignPort, Consol.JK_DateFirstForeignPort, Consol.JK_DateFirstForeignPort, Factory);
		}

		protected override PlaceAndDateWrapper GetLastForeignPort()
		{
			return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKLastForeignPort, Consol.JK_DateLastForeignPort, Consol.JK_DateLastForeignPort, Factory);
		}

		protected override PlaceAndDateWrapper GetPortOfFirstArrival()
		{
			return new PlaceAndDateWrapper(ConsolBO.JK_RL_NKPortOfFirstArrival, Consol.JK_DatePortOfFirstArrival, Consol.JK_DatePortOfFirstArrival, Factory);
		}

		#endregion

		#region SuppressiongBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return ConsolBO; }
		}

		#endregion

		#region ValueAndUnitWrappers

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(ConsolBO.JK_TotalShipmentPackageCount.ToZInt(), ConsolBO.JK_ShipmentTotalPackageCountPackType, new CodeDescriptionPairList(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(ConsolBO.JK_TotalShipmentQuantity.ToZInt(), ConsolBO.JK_ShipmentTotalQuantityPackType, new CodeDescriptionPairList(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			Tuple<ZDecimal, ZByte> weightWithScaleForDoc = ConsolBO.GetTotalShipmentWeightWithScaleForDoc(WeightVolumeDisplay) ?? new Tuple<ZDecimal, ZByte>(0, 0);
			int decimals = (int)MetaData.GetMetaData(ConsolBO, ConsolBO.JK_TotalShipmentWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			return new WeightWrapper(weightWithScaleForDoc.Item1, ConsolBO.JK_TotalShipmentWeightUnit, decimals, ConsolBO.WeightUnits, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			int decimals = (int)MetaData.GetMetaData(ConsolBO, ConsolBO.JK_TotalShipmentVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			if (ConsolBO != null && ConsolBO.IsExport() && ConsolBO.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air && !Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir && ReportName.ToUpper().Contains("MANIFEST"))
			{
				return VolumeWrapper.Empty;
			}
			return new VolumeWrapper(ConsolBO.GetTotalShipmentVolumeForDoc(WeightVolumeDisplay), ConsolBO.JK_TotalShipmentVolumeUnit, decimals, ConsolBO.VolumeUnits, Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			int decimals = (int)MetaData.GetMetaData(ConsolBO, ConsolBO.JK_ConsolChargeableInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new ValueAndUnitWrapper(ConsolBO.GetTotalShipmentChargeableForDoc(WeightVolumeDisplay), ConsolBO.JK_ConsolChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region Child Collections

		protected sealed override CostWrapperCollection GetCosts()
		{
			return new CostWrapperCollection(new JobConsolCostCollection(Factory, ConsolBO), Factory);
		}

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(ConsolBO, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(ConsolBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			if (deliveryAgent != null)
			{
				return new FreightWrapperCollection(ConsolBO, deliveryAgent, Factory);
			}

			return ConsolBO.IsMultiAWBMaster
				? GetFreightWrapperCollectionForMultiAWB(ConsolBO, true, Factory)
				: new FreightWrapperCollection(ConsolBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return ConsolBO.IsMultiAWBMaster
				? GetFreightWrapperCollectionForMultiAWB(ConsolBO, false, Factory)
				: new FreightWrapperCollection(null);
		}

		#region FreightWrapperCollectionForMultiAWB

		FreightWrapperCollection GetFreightWrapperCollectionForMultiAWB(ForwardingConsol masterConsol, bool wrapperFromShipment, BusinessObjectFactory factory)
		{
			var collection = new FreightWrapperCollection(factory);

			if (masterConsol != null)
			{
				if (wrapperFromShipment)
				{
					foreach (ForwardingConsol consol in masterConsol.ColoadConsols)
					{
						foreach (ForwardingShipment shipment in consol.Shipments)
						{
							if (IsShipmentPackedInMasterConsol(masterConsol, shipment))
							{
								collection.Add(new FreightWrapperFromShipment(shipment, factory));
							}
						}
					}
				}
				else
				{
					foreach (ForwardingConsol consol in masterConsol.ColoadConsols)
					{
						collection.Add(new FreightWrapperFromConsol(consol, factory));
					}
				}
			}

			return collection;
		}

		bool IsShipmentPackedInMasterConsol(ForwardingConsol consol, ForwardingShipment shipment)
		{
			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				if (IsPackLinePackedInConsolContainer(packLine, consol))
				{
					return true;
				}
			}

			return false;
		}

		bool IsPackLinePackedInConsolContainer(PackLine packLine, ForwardingConsol consol)
		{
			CommonContainer container = packLine.GetContainer(consol);
			return container != null;
		}

		#endregion

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ConsolBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			if (deliveryAgent != null)
			{
				return new ContainerWrapperCollection(ConsolBO, deliveryAgent, Factory);
			}
			else
			{
				return new ContainerWrapperCollection(ConsolBO, Factory);
			}
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			List<FreightWrapper> shipments = new List<FreightWrapper>();
			foreach (ForwardingShipment shipment in ConsolBO.Shipments)
			{
				shipments.AddRange(FreightWrapper.New(shipment, Factory));
			}

			return new RequiredDocumentsWrapperCollection(ConsolBO.RequiredDocuments, shipments.ToArray(), Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			ServiceWrapperCollection result = new ServiceWrapperCollection(this, Factory);
			foreach (ContainerWrapper containerWrapper in Containers)
			{
				result.AddRange(containerWrapper.Services);
			}

			return result;
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			List<CommonPickupDeliveryConfirm> confirmations = new List<CommonPickupDeliveryConfirm>();
			if (Consol.JK_ConsolMode == Constants.ContainerModes.FCL)
			{
				confirmations.AddRange(Array.ConvertAll(Consol.Containers.ToArray<CommonContainer>(), c => c.OriginConfirm));
				confirmations.AddRange(Array.ConvertAll(Consol.Containers.ToArray<CommonContainer>(), c => c.DestinationConfirm));
			}
			else
			{
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					confirmations.AddRange(shipment.PickupConfirms);
					confirmations.AddRange(shipment.DeliveryConfirms);
				}
			}

			return new PickupDeliveryConfirmationsWrapperCollection(this, Factory, confirmations);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			if (ReportName.Contains((NoResString)"Container Manifest"))
			{
				return new PackageWrapperCollection(Consol, true, false, Factory);
			}

			if (ReportName.Contains((NoResString)"Cargo Manifest"))
			{
				return new PackageWrapperCollection(Consol, false, true, Factory);
			}

			if (ConsolBO.IsMultiAWBMaster)
			{
				return GetPackageWrapperCollectionForMasterConsol();
			}

			return new PackageWrapperCollection(Consol, Factory);
		}

		PackageWrapperCollection GetPackageWrapperCollectionForMasterConsol()
		{
			var collection = new PackageWrapperCollection(Factory);

			foreach (ForwardingConsol consol in ConsolBO.ColoadConsols)
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					foreach (PackLine package in shipment.OuterPackLines)
					{
						if (IsPackLinePackedInConsolContainer(package, ConsolBO))
						{
							var wrapper = new PackageWrapperFromFreightPackage(package, Factory);
							wrapper.SetParentConsol(ConsolBO);
							collection.Add(wrapper);
						}
					}
				}
			}

			return collection;
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection(Packages.Select(x => ((PackageWrapper)x).FreightPackLine).ToArray(), Factory);
		}

		protected override ContainerPenaltyWrapperCollection GetImportContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(Consol, Factory, ContainerPenaltyDirection.Import);
		}

		protected override ContainerPenaltyWrapperCollection GetExportContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(Consol, Factory, ContainerPenaltyDirection.Export);
		}

		protected override ContainerPenaltyWrapperCollection GetContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(Consol, Factory);
		}

		protected override CO2eEmissionWrapperCollection GetCO2eEmissions()
		{
			return new CO2eEmissionWrapperCollection(Consol, Factory);
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.NoBusinessContext;
		}

		#endregion

		protected override ZString GetCustomsEntryNumber()
		{
			return ConsolBO.JK_CRN;
		}

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(DocForwardingConsol.New(ConsolBO, Factory), Factory);
		}
	}
}
