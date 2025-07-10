using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.CA;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromShipment : FreightWrapper, IDocTypeCode, IPackLineOverrider
	{
		public new static FreightWrapperFromShipment[] New(BusinessObject shipmentBO, BusinessObjectFactory factory)
		{
			ForwardingShipment shipment = shipmentBO as ForwardingShipment;
			return shipment == null ? null : new FreightWrapperFromShipment[] { new FreightWrapperFromShipment(shipment, factory) };
		}

		public FreightWrapperFromShipment(ForwardingShipment shipmentBO, BusinessObjectFactory factory, bool registrySayUseBrokerageForDocBuilder = false)
			: this(shipmentBO, null, null, factory, registrySayUseBrokerageForDocBuilder)
		{
		}

		public FreightWrapperFromShipment(ForwardingShipment shipmentBO, OrgHeader debtorBO, BusinessObjectFactory factory, bool registrySayUseBrokerageForDocBuilder = false)
			: this(shipmentBO, null, debtorBO, factory, registrySayUseBrokerageForDocBuilder)
		{
		}

		public FreightWrapperFromShipment(ForwardingShipment shipmentBO, Transport transportBO, BusinessObjectFactory factory, bool registrySayUseBrokerageForDocBuilder = false)
			: this(shipmentBO, transportBO, null, factory, registrySayUseBrokerageForDocBuilder)
		{
		}

		public FreightWrapperFromShipment(ForwardingShipment shipmentBO, Transport transportBO, OrgHeader debtorBO, BusinessObjectFactory factory, bool registrySayUseBrokerageForDocBuilder = false)
			: base(shipmentBO, factory)
		{
			Argument.NotNull(factory, "factory");
			ShipmentBO = shipmentBO ?? Factory.GetNull<ForwardingShipment>();

			DeclarationBO = (ShipmentBO != null && registrySayUseBrokerageForDocBuilder ? (BaseJobDeclaration)ShipmentBO.DeclarationForDocuments : null) ?? Factory.GetNull<BaseJobDeclaration>();

			TransportBO = transportBO ?? Factory.GetNull<Transport>();

			DebtorBO = debtorBO;
		}

		readonly ForwardingShipment ShipmentBO;
		ForwardingConsol ConsolBO
		{
			get
			{
				if (consolBO == null)
				{
					consolBO = GetConsolFromTransport(ShipmentBO, TransportBO) ?? Factory.GetNull<ForwardingConsol>();
				}
				return consolBO;
			}
		}
		ForwardingConsol consolBO;

		readonly BaseJobDeclaration DeclarationBO;
		readonly Transport TransportBO;
		readonly OrgHeader DebtorBO;

		#region Co2e Properties

		protected override ZString GetFormattedTotalCO2e()
		{
			if (ShipmentBO.GetCO2eStatus() == CO2eStatusList.Codes.Current)
			{
				return CO2eHelper.GetFormattedCO2e(ShipmentBO.GetTotalCO2e());
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			if (ShipmentBO.HaveJobCO2e)
			{
				return (ShipmentBO.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region Related Business Objects

		protected override InvoicingJobWrapper GetInvoicingJob()
		{
			return Job != null ? new InvoicingJobWrapper(Job, DebtorBO, Factory) : null;
		}

		protected override LocalTransportLegWrapperCollection GetLocalTransportLegs()
		{
			return new LocalTransportLegWrapperCollection(Factory);
		}

		protected override BaseJobDeclaration GetDeclaration()
		{
			if (ShipmentBO != null)
			{
				return (BaseJobDeclaration)ShipmentBO.DeclarationForDocuments;
			}
			return null;
		}

		protected override ForwardingShipment GetShipment()
		{
			return ShipmentBO;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ShipmentBO;
		}

		protected override ForwardingConsol GetConsol()
		{
			return ConsolBO;
		}

		protected override Job GetJob()
		{
			return ShipmentBO == null ? null : (Job)ShipmentBO.Job;
		}

		protected override DocBaseJobDeclaration GetDocDeclaration()
		{
			var declaration = GetDeclaration();
			if (declaration != null)
			{
				return DocBaseJobDeclaration.New(declaration, Factory);
			}

			return base.GetDocDeclaration();
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return ConsolBO.Logs.CreatedDateUtc;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return ShipmentBO.Logs.CreatedDateUtc;
		}
		#endregion

		#region Custom Attributes

		protected override ZString GetCustomAttribute1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomAttrib2;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomFlag2;
		}
		#endregion

		#region General Freight References/Fields

		protected override CodeAndDescriptionWrapper GetOrderTransportMode()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override ZString GetLocalForwarderReference()
		{
			return ShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetExportAgentsReference()
		{
			return ShipmentBO.IsExport() ? ShipmentBO.JS_UniqueConsignRef : ConsolBO.JK_AgentsReference;
		}

		protected override ZString GetImportAgentsReference()
		{
			return ShipmentBO.IsImport() ? ShipmentBO.JS_UniqueConsignRef : ConsolBO.JK_AgentsReference;
		}

		protected override ZString GetConsolAgentsReference()
		{
			return ConsolBO?.JK_AgentsReference ?? ZString.Empty;
		}

		protected override ZString GetGoodsDescription()
		{
			ZString result = ShipmentBO.DetailedGoodsDescriptionNoteText;
			return !result.IsEmpty ? result : ShipmentBO.JS_GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ShipmentBO.JS_MarksAndNumbers;
		}

		protected override ZString GetOwnerReference()
		{
			return DeclarationBO.JE_OwnerRef;
		}

		protected override ZString GetCustomerReference()
		{
			var result = ZString.Empty;

			if (Orders != null)
			{
				result = ((IBODocDataProviderCollection)Orders).Format("{OrderNo}", (NoResString)"Comma", ZString.Empty, ZString.Empty, 0).Trim(' ', '/', ' ');
			}

			return result;
		}

		protected override ZString GetHouseBill()
		{
			return ShipmentBO.JS_HouseBill;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return ShipmentBO.JS_HouseBillIssueDate;
		}

		protected override ZString GetHBLContainerMode()
		{
			return ShipmentBO.JS_HBLContainerPackModeOverride.IsEmpty ? ShipmentBO.JS_PackingMode : ShipmentBO.JS_HBLContainerPackModeOverride;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return ShipmentBO.JS_ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return ShipmentBO.JS_NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return ShipmentBO.JS_NoCopyBills;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			return ShipmentBO.JS_InterimReceipt;
		}

		protected override ZDateTime GetActualReceive()
		{
			return ShipmentBO.JS_A_RCV;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupLabourCharge;
		}

		protected override ZString GetPickupLabourTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupLabourTime);
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupTruckWaitCharge;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		protected override ZString GetShippersReference()
		{
			return ShipmentBO.JS_BookingReference;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("8507d174-ac63-493e-b4d8-3b7dd1f26279", "Shipment");
		}

		protected override ZString GetJobNumber()
		{
			return ShipmentBO != null ? ShipmentBO.JS_UniqueConsignRef : ZString.Empty;
		}

		protected override ZString GetSecondaryHeading()
		{
			return ConsolBO != null ? Res.GetString("bae3bb08-e6c4-401a-9a90-45fb1fa8aa79", "Consol") : string.Empty;
		}

		protected override ZString GetSecondaryNumber()
		{
			return ConsolBO.JK_UniqueConsignRef;
		}

		protected override ZString GetArrivalReference()
		{
			return ConsolBO != null && ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null
				? ConsolBO.Schedule.Destination.JB_ArrivalReference
				: ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return ConsolBO != null & ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null
				? ConsolBO.Schedule.Destination.JB_Berth
				: ZString.Empty;
		}

		protected override ZString GetConsolReference()
		{
			return ShipmentBO.JS_ConsolReference;
		}

		protected override RouteWrapper GetInterestedRoute()
		{
			return new RouteWrapper(TransportBO, Factory);
		}

		protected override ContainerWrapperCollection GetTranshipmentContainers()
		{
			var result = new ContainerWrapperCollection(Factory);
			if (TranshipmentFreightConsol != null)
			{
				var transhipmentConsolContainers = new ContainerWrapperCollection(TranshipmentFreightConsol.Consol, Factory);
				var packContainers = ShipmentBO.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(x => x.Containers.Cast<ForwardingContainer>().Select(c => c.JC_ContainerNum)).Distinct();
				foreach (ContainerWrapper container in transhipmentConsolContainers)
				{
					if (packContainers.Contains(container.ContainerNo))
					{
						result.Add(container);
					}
				}
			}
			return result;
		}

		protected override FreightWrapperFromConsol GetTranshipmentFreightConsol()
		{
			var currentConsol = ConsolBO;
			if (currentConsol != null)
			{
				foreach (ForwardingConsol consol in ShipmentBO.Consols)
				{
					if (!consol.JK_JX_JA_RL_NKPortOfLoading.IsEmpty && !currentConsol.JK_JX_JB_RL_NKPortOfDischarge.IsEmpty &&
						consol.JK_JX_JA_RL_NKPortOfLoading == currentConsol.JK_JX_JB_RL_NKPortOfDischarge)
					{
						if (!ShipmentBO.JS_JS_ColoadMasterShipment.IsValid)
						{
							return new FreightWrapperFromConsol(consol, Factory);
						}
					}
				}
			}
			return null;
		}

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				return packLineOverride != null && packLineOverride.Any()
					? new TextBarcode(JobNumber)
					: TextBarcodeForShipment(ShipmentBO, DocTypeCode);
			}
		}

		protected override TextBarcode DocManagerBarCodeWithUniqueID
		{
			get
			{
				return packLineOverride != null && packLineOverride.Any()
					? GetUniqueTextBarcodeForPackage()
					: TextBarcodeForShipment(ShipmentBO, DocTypeCode);
			}
		}

		TextBarcode GetUniqueTextBarcodeForPackage()
		{
			const bool USE_OPTIMISED_ENCODING = true; // shortens barcode
			return new TextBarcode(string.Concat(GlbCompany.CurrentCompany.LicenceEnterpriseCode,
				GlbCompany.CurrentCompany.LicenceServerID,
				JobNumber,
				"-",
				DocNumber.ToString().PadLeft(5, '0')), USE_OPTIMISED_ENCODING);
		}

		internal static TextBarcode TextBarcodeForShipment(ForwardingShipment shipmentBO, ZString docTypeCode)
		{
			var barcode = new TextBarcode(ZString.Empty);

			if (shipmentBO.Origin != null && shipmentBO.Destination != null)
			{
				var barcodeGenerator = new BarcodeGenerator();
				ZString originLocation = (shipmentBO.Origin.RL_IATA.IsEmpty) ? shipmentBO.JS_RL_NKOrigin.SubstringSafe(2, 3) : shipmentBO.Origin.RL_IATA;
				ZString destinationLocation = (shipmentBO.Destination.RL_IATA.IsEmpty) ? shipmentBO.JS_RL_NKDestination.SubstringSafe(2, 3) : shipmentBO.Destination.RL_IATA;

				barcode = barcodeGenerator.CreateShipmentBarcode(GlbCompany.CurrentCompany.GC_Code, docTypeCode, originLocation, destinationLocation, shipmentBO.JS_HouseBill);
			}

			return barcode;
		}

		#region Consol Level String Fields
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
			return ConsolBO != null && ConsolBO.IsAir ? ConsolBO.JK_MasterBillIssueDate : ZDateTime.Empty;
		}

		protected override ZString GetConsolPaymentType()
		{
			return ConsolBO.JK_PrepaidCollect;
		}

		protected override ZString GetConsolNumber()
		{
			return ConsolBO.JK_UniqueConsignRef;
		}

		#endregion

		protected override ZString GetMasterBillHeading()
		{
			string transportMode = ZString.Empty;
			transportMode = Consol != null ? ConsolTransportMode.Code : ShipmentTransportMode.Code;
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("19aa3877-80be-44ce-94c5-6be9435aec0a", "MAWB");
				case Core.Constants.TransportModes.Sea:
					return Res.GetString("ffc667fd-debb-4596-96bb-ac0e82767550", "Ocean Bill Of Lading");
				default:
					return Res.GetString("2cc5b387-0135-49a8-9ea0-c5661a18f09d", "Master Bill");
			}
		}

		protected override ZString GetHouseBillHeading()
		{
			switch (ShipmentTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("2520c196-8317-458d-b079-dbd8604f978b", "HAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("d30fd787-fc77-453a-b16d-cc51b525e427", "House Bill Of Lading");

				default:
					return Res.GetString("d97c7d33-c290-4e9c-973a-697c90874d1e", "House Bill");
			}
		}

		protected override ZString GetAdditionalTerms()
		{
			return ShipmentBO.JS_AdditionalTerms;
		}

		protected override ZDecimal GetLoadingMeters()
		{
			return ShipmentBO.GetLoadingMetersForDoc(WeightVolumeDisplay);
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return CartageInfo.FullHandlingInstructions;
		}

		protected override ZString GetFullCartageInstructions()
		{
			return CartageInfo.FullCartageInstructions;
		}

		protected override ZString GetAWBSecurityInspectionStatus()
		{
			ZString result = ZString.Empty;

			if (ShipmentBO.IsAir && !ShipmentBO.IsDomestic())
			{
				result = GetApprovedTextFromRegistry(ShipmentBO.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code);
			}

			return result;
		}

		ZString GetApprovedTextFromRegistry(bool isApproved)
		{
			if (isApproved)
			{
				return (string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value;
			}
			else
			{
				return (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value;
			}
		}

		protected override ZString GetEFreightStatus()
		{
			return ShipmentBO.JS_EFreightStatus;
		}

		#endregion

		#region Critical Dates

		protected override ZDateTimeOffset GetRevisedDeliveryDueDate()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				return ShipmentBO.JS_RevisedDeliveryDueDate;
			}

			return ZDateTimeOffset.Empty;
		}

		protected override ZDateTime GetDeliveryDueDate()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				return ShipmentBO.JS_DeliveryDueDate;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryFrom()
		{
			return ShipmentBO.DocsAndCartage.JP_EstimatedDelivery;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryCartageAdvised;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryCartageCompleted;
		}

		protected override ZDateTime GetPickupFrom()
		{
			return ShipmentBO.DocsAndCartage.JP_EstimatedPickup;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupRequiredBy;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupCartageAdvised;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupCartageCompleted;
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
			ZString shipmentTypeCode = ZString.Empty;
			if (ShipmentBO.IsImport())
			{
				shipmentTypeCode = "IMP";
			}
			else if (ShipmentBO.IsExport())
			{
				shipmentTypeCode = "EXP";
			}
			else if (ShipmentBO.IsDomestic())
			{
				shipmentTypeCode = "DOM";
			}

			return new CodeAndDescriptionWrapper(shipmentTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ShipmentStatus, new CodeDescriptionPairList(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_PackingMode, ShipmentBO.Lookups.JS_PackingMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_TransportMode, ShipmentBO.Lookups.JS_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			if (ShipmentBO.AviationSecurity.IsAviationSecurityApplicableForTransportMode
				&& ShipmentBO.AviationSecurity.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(ShipmentBO)
				&& !ShipmentBO.AviationSecurity.ReasonForAviationSecurityNotBeingAvailable.IsEmpty)
			{
				return null;
			}

			return new CodeAndDescriptionWrapper(ShipmentBO.JS_InspectionTypeCode, ShipmentBO.Lookups.InspectionTypes, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_RS_NKServiceLevel, ShipmentBO.Lookups.ServiceLevels, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(ShipmentBO.JS_INCO, ShipmentBO.Lookups.JS_INCO_List, IncoTermWrapper.Deciders.ByPaymentType(ShipmentBO.JS_PaymentTerm), Factory);
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ReleaseType, ShipmentBO.Lookups.JS_ReleaseType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShippedOnBoardType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ShippedOnBoard, ShipmentBO.Lookups.JS_ShippedOnBoard_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetCaratagePickupMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded, ShipmentBO.DocsAndCartage.Lookups.PickupEquipmentNeededList, Factory);
		}
		#endregion

		#region Organisations

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, ConsolBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
			{
				return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.Depot, Factory);
			}
			else if (ConsolBO != null && ConsolBO.PackDepotAddress != null)
			{
				return new AddressWrapper(ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
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
				return new AddressWrapper(ConsolBO.ArrivalCTOAddress, ContactType.CTO, Factory);
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
				return new AddressWrapper(ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
			{
				return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.Depot, Factory);
			}
			else if (ConsolBO != null)
			{
				if (ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
				{
					if (ConsolBO.DepartureCTOAddress != null)
					{
						return new AddressWrapper(ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
					}
				}
				else
				{
					if (ConsolBO.PackDepotAddress != null)
					{
						return new AddressWrapper(ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
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
			return new OrganisationWrapper(ShipmentHasCoLoadMaster ? OrganisationUsageType.UltimateConsignor : OrganisationUsageType.Consignor, ShipmentBO.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(ShipmentHasCoLoadMaster ? OrganisationUsageType.UltimateConsignee : OrganisationUsageType.Consignee, ShipmentBO.ConsigneeDocumentaryAddress, Factory);
		}

		protected override FreightWrapper GetParentJob()
		{
			return new FreightWrapperFromShipment(ShipmentBO.CoLoadMasterShipment as ForwardingShipment, Factory);
		}

		protected override ZString GetCustomsEntryNumber()
		{
			return ShipmentBO.CusEntryNumbers.Count > 0 ? ZString.Format("{0} {1}", CusEntryNumberTypes.UserFriendlyEntryType(ShipmentBO.CustomsEntryNumberType), ShipmentBO.CustomsEntryNumber).Trim() : ZString.Empty;
		}

		bool ShipmentHasCoLoadMaster
		{
			get
			{
				if (!shipmentHasCoLoadMaster.HasValue)
				{
					shipmentHasCoLoadMaster = false;
					CommonShipment coLoadMasterShipment = ShipmentBO.CoLoadMasterShipment;
					if (coLoadMasterShipment != null)
					{
						OrgHeader coLoadMasterShipmentConsignee = coLoadMasterShipment.Consignee;
						if (coLoadMasterShipmentConsignee != null && !coLoadMasterShipmentConsignee.MiscServ.OM_FWDealDirectlyWithUltimates)
						{
							shipmentHasCoLoadMaster = true;
						}
					}
				}
				return shipmentHasCoLoadMaster.Value;
			}
		}
		bool? shipmentHasCoLoadMaster;

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, ShipmentBO.BuyerDocAddress, Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.InsuredBy, ShipmentBO.InsuredByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.AssuredParty, ShipmentBO.AssuredPartyDocAddress, Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, ShipmentBO.ClaimsPayableByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, ShipmentBO.SurveyReportPartyDocAddress, Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			if (ShipmentBO.IsExport())
			{
				return ShipmentBO.PickupAgent != null
					? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ShipmentBO.PickupAgent, ContactType.FreightAgent, Factory)
					: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);
			}
			else
			{
				return ShipmentBO.DeliveryAgent != null
					? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ShipmentBO.DeliveryAgent, ContactType.FreightAgent, Factory)
					: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
			}
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			if (ShipmentBO.PickupAgent != null || ConsolBO == null)
			{
				return new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, ShipmentBO.PickupAgent, ContactType.ExportFreightAgent, ShipmentBO.JS_TransportMode, Factory);
			}
			else
			{
				return new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, ConsolBO.SendingForwarderAddress, ContactType.ExportFreightAgent, ConsolBO.JK_TransportMode, Factory);
			}
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ExportBroker, ShipmentBO.ExportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			if (ShipmentBO.DeliveryAgent != null || ConsolBO == null)
			{
				return new OrganisationWrapper(OrganisationUsageType.ImportAgent, ShipmentBO.DeliveryAgent, ContactType.ImportFreightAgent, ShipmentBO.JS_TransportMode, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.ImportAgent, ConsolBO.ReceivingForwarderAddress, ContactType.ImportFreightAgent, ConsolBO.JK_TransportMode, Factory);
			}
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportBroker, ShipmentBO.ImportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = ShipmentBO.DeliveryAgent != null
				? new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, ShipmentBO.DeliveryAgent, ContactType.FreightAgent, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, ShipmentBO.NotifyPartyDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetNotifyParty2()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, ShipmentBO.NotifyParty2DocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetNotifyParty3()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, ShipmentBO.NotifyParty3DocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, ShipmentBO.DocsAndCartage.DeliveryCartageCo, ContactType.All, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(ShipmentBO.ConsigneeDeliveryAddress, Factory);
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.PickupAgent, ShipmentBO.DocsAndCartage.PickupCartageCo, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetControllingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingAgent, ShipmentBO.ControllingAgentDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetControllingCustomer()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingCustomer, ShipmentBO.ControllingCustomerAddress, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = null;
			if (ConsolBO != null && ConsolBO.ArrivalCTOAddress != null && ConsolBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = ConsolBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(ShipmentBO.ConsignorPickupAddress, Factory);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			OrgAddress address = Factory.GetNull<OrgAddress>();
			if (ShipmentBO.ExportReceivingDepot != null)
			{
				address = ShipmentBO.ExportReceivingDepot;
			}
			else if (ConsolBO != null)
			{
				address = Consol.PackDepotAddress;
			}
			return new AddressWrapper(address, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			OrgAddress orgAddress = ShipmentBO.ImportReleaseDepot ?? ConsolBO.UnpackDepotAddress;
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			if (ShipmentBO.ImportReleaseDepot != null)
			{
				return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ShipmentBO.ImportReleaseDepot, ContactType.All, Factory);
			}

			return GetGoodsAvailableAtFromConsol();
		}

		AddressWrapper GetGoodsAvailableAtFromConsol()
		{
			if (ConsolBO != null)
			{
				if (ConsolBO.IsSea)
				{
					var hasBCNContainer = HasContainer(Core.Constants.ContainerModes.BuyersConsol);

					if (hasBCNContainer
						|| ConsolBO.IsBuyersConsol
						|| ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.FCL
						|| ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk
						|| ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
					{
						return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.All, Factory);
					}

					if (ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.LCL)
					{
						return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
					}

					if (ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol)
					{
						var hasLCLContainer = HasContainer(Core.Constants.ContainerModes.LCL);
						var hasFCLContainer = HasContainer(Core.Constants.ContainerModes.FCL);
						var hasGRPContainer = HasContainer(Core.Constants.ContainerModes.Groupage);

						if (hasGRPContainer || (hasLCLContainer && hasFCLContainer))
						{
							if (consolBO.UnpackDepotAddress != null)
							{
								return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
							}
							else
							{
								return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.All, Factory);
							}
						}

						if (hasFCLContainer)
						{
							return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.All, Factory);
						}

						if (hasLCLContainer)
						{
							return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
						}
					}
				}
				else if (ConsolBO.IsAir)
				{
					return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
				}

				if (consolBO.UnpackDepotAddress != null)
				{
					return new AddressWrapper(OrganisationUsageType.UnpackingLocation, ConsolBO.UnpackDepotAddress, ContactType.All, Factory);
				}
				else
				{
					return new AddressWrapper(OrganisationUsageType.CTOArrival, ConsolBO.ArrivalCTOAddress, ContactType.All, Factory);
				}
			}

			return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		bool HasContainer(string containerMode)
		{
			foreach (ForwardingPackLine packLine in ShipmentBO.OuterPackLines)
			{
				foreach (ForwardingContainer container in packLine.Containers)
				{
					if (container.JC_JK == ConsolBO.PK && container.JC_ContainerMode == containerMode)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected override OrganisationWrapper GetArrivalCFSTransport()
		{
			return ConsolBO != null
				? new OrganisationWrapper(OrganisationUsageType.ArrivalCFSTransport, ConsolBO.ArrivalUnpackCFSTransport, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ArrivalCFSTransport, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetDepartureCFSTransport()
		{
			return ConsolBO != null
				? new OrganisationWrapper(OrganisationUsageType.DepartureCFSTransport, ConsolBO.DeparturePackCFSTransport, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.DepartureCFSTransport, Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			CommonConsol consol = ShipmentBO.FindCorrectConsol();

			ZDateTime bestDate = ZDateTime.Empty;

			ZDateTime shipmentETD = ShipmentBO.JS_E_DEP;
			ZDateTime consolATD = consol != null && consol.JK_JX_JA_A_DEP.IsValid ? consol.JK_JX_JA_A_DEP : ZDateTime.Invalid;

			if (shipmentETD.IsValid && consolATD.IsValid)
			{
				bestDate = shipmentETD < consolATD ? shipmentETD : consolATD;
			}
			else if (shipmentETD.IsValid)
			{
				bestDate = shipmentETD;
			}
			else if (consolATD.IsValid)
			{
				bestDate = consolATD;
			}

			return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKOrigin, ShipmentBO.JS_E_DEP, bestDate, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			CommonConsol consol = ShipmentBO.FindCorrectConsol();

			ZDateTime bestDate = ZDateTime.Empty;

			ZDateTime shipmentETA = ShipmentBO.JS_E_ARV;
			ZDateTime consolATA = consol != null && consol.JK_JX_JB_A_ARV.IsValid ? consol.JK_JX_JB_A_ARV : ZDateTime.Invalid;

			if (shipmentETA.IsValid && consolATA.IsValid)
			{
				bestDate = shipmentETA > consolATA ? shipmentETA : consolATA;
			}
			else if (shipmentETA.IsValid)
			{
				bestDate = shipmentETA;
			}
			else if (consolATA.IsValid)
			{
				bestDate = consolATA;
			}

			return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKDestination, ShipmentBO.JS_E_ARV, bestDate, Factory);
		}

		protected override LocationWrapper GetFreightPayableAt()
		{
			return new LocationWrapper(ShipmentBO.FreightPayableAt != null ? ShipmentBO.FreightPayableAt.RL_Code : ZString.Empty, Factory);
		}

		#endregion

		#region SuppressingBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return ShipmentBO; }
		}

		#endregion

		#region ValueAndUnitWrappers

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(ShipmentBO.JS_TotalPackageCount, ShipmentBO.JS_F3_NKTotalCountPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(ShipmentBO.JS_OuterPacks, ShipmentBO.JS_F3_NKPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			Tuple<ZDecimal, ZByte> weightWithScaleForDoc = ShipmentBO.GetWeightWithScaleForDoc(WeightVolumeDisplay) ?? new Tuple<ZDecimal, ZByte>(0, 0);
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			return new WeightWrapper(weightWithScaleForDoc.Item1, ShipmentBO.JS_UnitOfWeight, decimals, ShipmentBO.Lookups.JS_UnitOfWeight_List, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			if (ConsolBO != null && ConsolBO.IsExport() && ConsolBO.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air && !Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir && ReportName.ToUpper().Contains("MANIFEST"))
			{
				return VolumeWrapper.Empty;
			}

			return new VolumeWrapper(ShipmentBO.GetVolumeForDoc(WeightVolumeDisplay), ShipmentBO.JS_UnitOfVolume, decimals, ShipmentBO.Lookups.JS_UnitOfVolume_List, Factory);
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_GoodsValue, ShipmentBO.GoodsValueCurr), Factory);
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_InsuranceValue, ShipmentBO.InsuranceCurrency), Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualChargeableInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new ValueAndUnitWrapper(ShipmentBO.GetChargeableForDoc(WeightVolumeDisplay), ShipmentBO.JS_ChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_UnitFreightRate, ShipmentBO.FrtRateCurrency), Factory);
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			return new ValueAndUnitWrapper(ShipmentBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours, ShipmentBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(ConsolBO, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(ShipmentBO, ConsolBO, Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return new CommercialInvoiceWrapperCollection(ShipmentBO, Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return new CommercialInvoiceLineWrapperCollection(ShipmentBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(ShipmentBO, RoutingLevel.Shipment, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return new FreightWrapperCollection(ShipmentBO, RoutingLevel.Consol, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ShipmentBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(ShipmentBO, DocumentDirection, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(ShipmentBO.DocsAndCartage.RequiredDocuments, Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(ShipmentBO.Services, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			List<CommonPickupDeliveryConfirm> confirmations = new List<CommonPickupDeliveryConfirm>();
			if (ShipmentBO.PackingMode == Constants.ContainerModes.FCL
			 || ShipmentBO.PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.OriginConfirm));
				confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.DestinationConfirm));
			}
			else
			{
				confirmations.AddRange(ShipmentBO.PickupConfirms);
				confirmations.AddRange(ShipmentBO.DeliveryConfirms);
			}
			return new PickupDeliveryConfirmationsWrapperCollection(this, Factory, confirmations);
		}

		protected override OrderWrapperCollection GetOrders()
		{
			OrderWrapperCollection result = new OrderWrapperCollection(ShipmentBO, Factory);

			if (ARInvoice != null && ARInvoice.PrintOrderNumbersFromRelatedShipments)
			{
				foreach (ForwardingShipment relatedShipment in ShipmentBO.CoLoadShipments)
				{
					result.AddRange(new OrderWrapperCollection(relatedShipment, Factory));
				}
			}

			return result;
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			var result = new OrderLineWrapperCollection(ShipmentBO, Factory);

			if (ARInvoice != null && ARInvoice.PrintOrderNumbersFromRelatedShipments)
			{
				foreach (ForwardingShipment relatedShipment in ShipmentBO.CoLoadShipments)
				{
					result.AddRange(new OrderLineWrapperCollection(relatedShipment, Factory));
				}
			}

			return result;
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return packLineOverride == null || !packLineOverride.Any()
				? new PackageWrapperCollection(ShipmentBO, Factory)
				: new PackageWrapperCollection(packLineOverride, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection((PackLine[])ShipmentBO.OuterPackLines.ToArray(typeof(PackLine)), Factory);
		}

		protected override ContainerPenaltyWrapperCollection GetImportContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(ShipmentBO, Factory, ContainerPenaltyDirection.Import);
		}

		protected override ContainerPenaltyWrapperCollection GetExportContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(ShipmentBO, Factory, ContainerPenaltyDirection.Export);
		}

		protected override ContainerPenaltyWrapperCollection GetContainerPenalties()
		{
			return new ContainerPenaltyWrapperCollection(ShipmentBO, Factory);
		}

		protected override CO2eEmissionWrapperCollection GetCO2eEmissions()
		{
			return new CO2eEmissionWrapperCollection(ShipmentBO, Factory);
		}

		protected override ZString GetUNDGsSummary()
		{
			var helper = new UNDGSubstanceWrapperHelper();
			return helper.GetUNDGPackagesSummary(UNDGs.ToArray<UNDGSubstanceWrapper>());
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Shipment;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return ShipmentBO.PK;
		}
		#endregion

		#region CA specific properties

		protected override Customs.DocReleaseStatusCollection GetCAReleaseStatus()
		{
			var result = base.GetCAReleaseStatus();
			if (LastReleaseStatus != null)
			{
				result.Add(new Customs.DocReleaseStatus(LastReleaseStatus, Factory));
			}

			return result;
		}

		IReleaseStatus LastReleaseStatus
		{
			get
			{
				if (_lastReleaseStatus == null && ShipmentBO != null)
				{
					_lastReleaseStatus = GetLastReleaseStatus(Factory, ShipmentBO.PK);
				}
				return _lastReleaseStatus;
			}
		}
		IReleaseStatus _lastReleaseStatus;

		internal static IReleaseStatus GetLastReleaseStatus(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			IReleaseStatus lastReleaseStatus = null;

			var types = ObjectFactory.GetType<IReleaseStatus>();
			lastReleaseStatus = (IReleaseStatus)Activator.CreateInstance(types, factory, shipmentPK);

			return lastReleaseStatus;
		}

		protected override ZString GetCAPreviousCCN()
		{
			var previousCCN = CusEntryNumber.Load(ShipmentBO, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Core.Constants.CountryCodes.Canada);
			return previousCCN != null ? previousCCN.CE_EntryNum : base.GetCAPreviousCCN();
		}

		protected override ZString GetCATransactionNo()
		{
			return LastReleaseStatus != null ? LastReleaseStatus.RL_TransactionNumber : base.GetCATransactionNo();
		}

		#endregion

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(ShipmentBO, Factory);
		}

		protected override NotClearedByAgentWrapper GetNotClearedByAgent()
		{
			return new NotClearedByAgentWrapper(ShipmentBO);
		}

		#region IDocTypeCode

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion

		#region IPackageOverrider Members

		void IPackLineOverrider.SetPackageOverride(PackLine packLine)
		{
			packLineOverride = new List<PackLine> { packLine };
		}

		public void SetPackageCollectionOverride(List<PackLine> packLines)
		{
			packLineOverride = packLines;
		}

		List<PackLine> packLineOverride;

		#endregion

		#region ACINo

		protected override ZString GetHouseACIDNo()
		{
			return string.Join(",", ShipmentBO.Numbers
				.Cast<CusEntryNumber>()
				.Where(n => n.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference && n.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt)
				.Select(n => n.CE_EntryNum).Distinct());
		}

		#endregion
	}
}
