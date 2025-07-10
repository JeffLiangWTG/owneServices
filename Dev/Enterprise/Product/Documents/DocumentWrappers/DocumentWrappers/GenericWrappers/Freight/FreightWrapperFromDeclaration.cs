using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDeclaration<T1, T2> : FreightWrapperFromDeclaration
		where T1 : BaseJobDeclaration
		where T2 : DocBaseJobDeclaration
	{
		protected FreightWrapperFromDeclaration(BaseJobDeclaration declarationBO, Transport transportBO, BusinessObjectFactory factory)
			: base(declarationBO, transportBO, factory)
		{
		}

		public new T1 Declaration
		{
			get { return (T1)base.Declaration; }
		}

		public new T2 DocDeclaration
		{
			get { return (T2)base.DocDeclaration; }
		}
	}

	public abstract class FreightWrapperFromDeclaration : FreightWrapper, IDocTypeCode
	{
		public static FreightWrapperFromDeclaration New(BaseJobDeclaration declarationBO, Transport transportBO, BusinessObjectFactory factory)
		{
			var declarationType = declarationBO.GetType();
			var wrapperType = typeof(FreightWrapperFromDeclaration<,>).MakeGenericType(new[] { declarationType, DocBaseJobDeclaration.GetDocDeclarationType(declarationBO) });
			var constructorInfo = wrapperType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(BaseJobDeclaration), typeof(Transport), typeof(BusinessObjectFactory) }, null);
			return (FreightWrapperFromDeclaration)constructorInfo.Invoke(new object[] { declarationBO, transportBO, factory });
		}

		public static FreightWrapperFromDeclaration New(BaseJobDeclaration declarationBO, BusinessObjectFactory factory)
		{
			return New(declarationBO, null, factory);
		}

		protected FreightWrapperFromDeclaration(BaseJobDeclaration declarationBO, Transport transportBO, BusinessObjectFactory factory)
			: base(declarationBO, factory)
		{
			DeclarationBO = declarationBO;
			TransportBO = transportBO;
			if (TransportBO == null)
			{
				TransportBO = Factory.GetNull<Transport>();
			}
		}
		readonly BaseJobDeclaration DeclarationBO;
		readonly Transport TransportBO;

		public new DocBaseJobDeclaration DocDeclaration
		{
			get { return docDeclaration ?? (docDeclaration = DocBaseJobDeclaration.New(Declaration, Factory)); }
		}
		DocBaseJobDeclaration docDeclaration;

		public override BusinessObject BusinessObjectForPrintJob => (BusinessObject)ARInvoice?.WrappedObject ?? base.BusinessObjectForPrintJob;

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return (BusinessObject)ARInvoice?.WrappedObject ?? DeclarationBO.Shipment ?? base.BusinessObjectToLogAgainst; }
		}

		protected override BusinessObject BusinessObjectForCustomFields
		{
			get { return DeclarationBO; }
		}

		#region Freight Business Objects used for fallbacks

		ForwardingShipment ShipmentBO
		{
			get
			{
				if (object.ReferenceEquals(fShipmentBO, null))
				{
					fShipmentBO = DeclarationBO.Shipment;
					if (fShipmentBO == null)
					{
						fShipmentBO = Factory.GetNull<ForwardingShipment>();
					}
				}
				return fShipmentBO;
			}
		}
		ForwardingShipment fShipmentBO;

		FreightWrapperFromShipment WrapperFromShipment
		{
			get
			{
				if (wrapperFromShipment == null && ShipmentBO != null)
				{
					wrapperFromShipment = new FreightWrapperFromShipment(ShipmentBO, Factory);
				}
				return wrapperFromShipment;
			}
		}
		FreightWrapperFromShipment wrapperFromShipment;

		ForwardingConsol ConsolBO
		{
			get
			{
				if (object.ReferenceEquals(fConsolBO, null))
				{
					fConsolBO = GetConsolFromTransport(ShipmentBO, TransportBO);

					if (fConsolBO == null)
					{
						fConsolBO = Factory.GetNull<ForwardingConsol>();
					}
				}
				return fConsolBO;
			}
		}
		ForwardingConsol fConsolBO;

		#endregion

		#region CO2e Properties

		protected override ZString GetFormattedTotalCO2e()
		{
			return ShipmentBO != null ? WrapperFromShipment.FormattedTotalCO2e : base.GetFormattedTotalCO2e();
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			return ShipmentBO != null ? WrapperFromShipment.CO2eCalculationDate : base.GetCO2eCalculationDate();
		}

		#endregion

		#region Related Business Objects

		protected override BaseJobDeclaration GetDeclaration()
		{
			return DeclarationBO;
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

		protected override ZString GetCustomsEntryNumber()
		{
			return DeclarationBO == null ? ZString.Empty : DeclarationBO.DeclarationNumber;
		}

		protected override Job GetJob()
		{
			return DeclarationBO == null ? null : (Job)DeclarationBO.Job;
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
			return new CodeAndDescriptionWrapper(DeclarationBO.JE_MessageType, DeclarationBO.Lookups.MessageTypeList, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			ZString result = Declaration.FreightContainerMode;
			if (result.IsEmpty)
			{
				result = Constants.ContainerModes.Loose;
			}
			return new CodeAndDescriptionWrapper(result, ShipmentBO.Lookups.JS_PackingMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper((DeclarationBO.TransportModeGeneric == TransportTypeGenericList.Codes.Other ? ZString.Empty : DeclarationBO.TransportModeGeneric), DeclarationBO.Lookups.TransportTypeGenericList, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(DeclarationBO.JE_EntryStatus, DeclarationBO.Lookups.EntryStatusList, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(DeclarationBO.JE_RS_NKServiceLevel, DeclarationBO.Lookups.ServiceLevels, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			ZString consensusIncoTerm = ZString.Empty;

			foreach (BaseJobComInvoiceHeader invoiceHeader in DeclarationBO.Invoices)
			{
				if (consensusIncoTerm.IsEmpty)
				{
					consensusIncoTerm = invoiceHeader.JZ_IncoTerm;
				}
				else if (consensusIncoTerm != invoiceHeader.JZ_IncoTerm)
				{
					consensusIncoTerm = ZString.Empty;
					break;
				}
			}

			IncoTermWrapper incoWrapper = null;

			string chargeGroupCode = DeclarationBO.Shipment != null ? ChargeCodeGroupList.Codes.Brokerage : ChargeCodeGroupList.Codes.BrokerageOnly;

			if (consensusIncoTerm.IsEmpty)
			{
				incoWrapper = new IncoTermWrapper(DeclarationBO.JE_ShipmentIncoTerm, DeclarationBO.Lookups.IncoTermList, IncoTermWrapper.Deciders.ByChargeGroup(chargeGroupCode), Factory);
			}
			else
			{
				ZString incoTerm = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(consensusIncoTerm);

				incoWrapper = incoTerm.IsEmpty ? new IncoTermWrapper(consensusIncoTerm, DeclarationBO.Lookups.IncoTermList, IncoTermWrapper.Deciders.ByChargeGroup(chargeGroupCode), Factory) :
					new IncoTermWrapper(incoTerm, Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>(), IncoTermWrapper.Deciders.ByChargeGroup(chargeGroupCode), Factory);
			}

			return incoWrapper;
		}

		protected override MoneyWrapper GetCollectAmount()
		{
			return new MoneyWrapper(Money.Empty, Factory);
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
			ZString cartagePickupMode = ZString.Empty;
			if (DeclarationBO.IsExport && !DeclarationBO.JE_FCLDeliveryOrPickupEquipmentNeeded.IsEmpty)
			{
				cartagePickupMode = DeclarationBO.JE_FCLDeliveryOrPickupEquipmentNeeded;
			}
			else
			{
				cartagePickupMode = ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded;
			}
			return new CodeAndDescriptionWrapper(cartagePickupMode, DeclarationBO.DeliveryOrPickupEquipmentNeededList, Factory);
		}
		#endregion

		#region Consol Level String Fields

		protected override ZString GetLocalForwarderReference()
		{
			return DeclarationBO.JE_DeclarationReference;
		}

		protected override ZString GetExportAgentsReference()
		{
			return DeclarationBO.JE_DeclarationReference;
		}

		protected override ZString GetImportAgentsReference()
		{
			return DeclarationBO.JE_AgentsReference;
		}

		protected override ZString GetBookingReference()
		{
			return ConsolBO.JK_BookingReference;
		}

		protected override ZString GetMasterBill()
		{
			var declarationMasterBill = DeclarationBO.JE_MasterBillForGenericWrapper;
			return !declarationMasterBill.IsEmpty ? declarationMasterBill : ConsolBO != null ? ConsolBO.JK_MasterBillNum : ZString.Empty;
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

		protected override ZString GetConsolAgentsReference()
		{
			var result = ConsolBO.JK_AgentsReference;
			return result.IsEmpty ? (ZString)DeclarationBO.JobNumber : result;
		}

		protected override DocBaseJobDeclaration GetDocDeclaration()
		{
			return this.DocDeclaration;
		}

		#endregion

		#region Organisations
		protected override OrganisationWrapper GetConsignor()
		{
			return ShipmentBO != null
				? new OrganisationWrapper(OrganisationUsageType.Consignor, ShipmentBO.ConsignorDocumentaryAddress, Factory)
				: GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType.Consignor, DeclarationBO.SupplierDocumentaryAddress, DeclarationBO.Supplier, ContactType.Consignor);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return ShipmentBO != null
				? new OrganisationWrapper(OrganisationUsageType.Consignee, ShipmentBO.ConsigneeDocumentaryAddress, Factory)
				: GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType.Consignee, DeclarationBO.ImporterDocumentaryAddress, DeclarationBO.Importer, ContactType.Consignee);
		}

		protected override ZString GetApprovalNumber()
		{
			return CurrentCompany.Country.Code == Constants.CountryCodes.HongKong ? Consignor.MainAddress.AviationSecurity.ApprovalNumber : ZString.Empty;
		}

		protected override OrganisationWrapper GetBuyer()
		{
			JobDocAddress docAddress = DeclarationBO.BuyerDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.BuyerDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.Buyer, docAddress, Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			JobDocAddress docAddress = DeclarationBO.InsuredByDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.InsuredByDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.InsuredBy, docAddress, Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			JobDocAddress docAddress = DeclarationBO.AssuredPartyDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.AssuredPartyDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.AssuredParty, docAddress, Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			JobDocAddress docAddress = DeclarationBO.ClaimsPayableByDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.ClaimsPayableByDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, docAddress, Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			JobDocAddress docAddress = DeclarationBO.SurveyReportPartyDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.SurveyReportPartyDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, docAddress, Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportBroker, DeclarationBO.IsImport ? DeclarationBO.Branch.OrgProxy : null, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ExportBroker, DeclarationBO.IsExport ? DeclarationBO.Branch.OrgProxy : null, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			OrganisationWrapper wrapper;

			var forwarder = DeclarationBO.IsImport ? DeclarationBO.Forwarder : null;
			if (forwarder == null && WrapperFromShipment != null)
			{
				wrapper = wrapperFromShipment.ReceivingForwarder;
			}
			else
			{
				wrapper = new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, forwarder, ContactType.All, Factory);
			}

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			ExportAgentOrganisationWrapper wrapper;

			var forwarder = DeclarationBO.IsExport ? DeclarationBO.Forwarder : null;
			if (forwarder == null && WrapperFromShipment != null)
			{
				wrapper = wrapperFromShipment.SendingForwarder;
			}
			else
			{
				wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, forwarder, ContactType.All, Factory);
			}

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			JobDocAddress docAddress = DeclarationBO.NotifyPartyDocumentaryAddress;

			if ((docAddress == null || docAddress.IsEmpty))
			{
				if (ShipmentBO != null)
				{
					return new OrganisationWrapper(OrganisationUsageType.NotifyParty, ShipmentBO.NotifyPartyDocumentaryAddress, Factory);
				}

				if (DeclarationBO.Importer != null)
				{
					OrgContact orgContact = new DefaultContactFinder(DeclarationBO.Importer).DefaultContact(ContactType.NotifyParty);
					return new OrganisationWrapper(OrganisationUsageType.NotifyParty, orgContact.ParentOrg, ContactType.NotifyParty, Factory);
				}
			}

			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, docAddress, Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, DeclarationBO.Forwarder, ContactType.FreightAgent, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, DeclarationBO.IsExport ? DeclarationBO.Forwarder : null, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			OrganisationWrapper result;

			var importAgent = DeclarationBO.IsImport ? DeclarationBO.Forwarder : null;

			if (importAgent == null && WrapperFromShipment != null)
			{
				result = WrapperFromShipment.ImportAgent;
			}
			else
			{
				result = new OrganisationWrapper(OrganisationUsageType.ImportAgent, importAgent, ContactType.All, Factory);
			}

			return result;
		}

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, DeclarationBO.ShippingLine, ContactType.All, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (DeclarationBO != null && DeclarationBO.DepotDocAddress.Address != null)
			{
				return new AddressWrapper(DeclarationBO.DepotDocAddress.Address, ContactType.Depot, Factory);
			}
			else if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
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
			if (DeclarationBO != null && DeclarationBO.ContainerTerminalOperatorDocAddress.Address != null)
			{
				return new AddressWrapper(DeclarationBO.ContainerTerminalOperatorDocAddress.Address, ContactType.CTO, Factory);
			}
			else if (ConsolBO != null && ConsolBO.ArrivalCTOAddress != null)
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
			if (DeclarationBO != null && DeclarationBO.ContainerTerminalOperatorDocAddress.Address != null)
			{
				return new AddressWrapper(DeclarationBO.ContainerTerminalOperatorDocAddress.Address, ContactType.CTO, Factory);
			}
			else if (ConsolBO != null && ConsolBO.DepartureCTOAddress != null)
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
			if (DeclarationBO != null && DeclarationBO.DepotDocAddress.Address != null)
			{
				if (ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
				{
					return new AddressWrapper(DeclarationBO.DepotDocAddress.Address, ContactType.CTO, Factory);
				}
				else
				{
					return new AddressWrapper(DeclarationBO.DepotDocAddress.Address, ContactType.Depot, Factory);
				}
			}
			else if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
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

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			OrganisationWrapper result;
			var deliveryAgent = DeclarationBO.IsImport ? DeclarationBO.DocsAndCartage.DeliveryCartageCo : null;

			if (deliveryAgent == null && WrapperFromShipment != null)
			{
				result = WrapperFromShipment.DeliveryAgent;
			}
			else
			{
				result = new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, deliveryAgent, ContactType.All, Factory);
			}

			return result;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			var docAddress = DeclarationBO.IsImport ? DeclarationBO.ImporterDeliveryAddress : null;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.ConsigneeDeliveryAddress;
			}
			return GetWrappedJobDocAddressOrFallbackIfEmpty(docAddress, DeclarationBO.Importer, ContactType.Consignee);
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.PickupAgent, DeclarationBO.IsExport ? DeclarationBO.DocsAndCartage.PickupCartageCo : null, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = DeclarationBO.IsImport ? DeclarationBO.ContainerTerminalOperatorDocAddress.Organisation : null;
			if (orgHeader == null && ConsolBO != null && ConsolBO.ArrivalCTOAddress != null && ConsolBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = ConsolBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			var docAddress = DeclarationBO.IsExport ? DeclarationBO.SupplierPickupAddress : null;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.ConsignorPickupAddress;
			}
			return GetWrappedJobDocAddressOrFallbackIfEmpty(docAddress, DeclarationBO.Supplier, ContactType.Consignor);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			OrgAddress orgAddress = DeclarationBO.IsExport ? DeclarationBO.DepotDocAddress.Address : null;
			if (orgAddress == null && ShipmentBO != null)
			{
				orgAddress = ShipmentBO.ExportReceivingDepot;
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			OrgAddress orgAddress = DeclarationBO.IsImport ? DeclarationBO.DepotDocAddress.Address : null;
			if (orgAddress == null && ShipmentBO != null)
			{
				orgAddress = ShipmentBO.ImportReleaseDepot;
			}
			if (orgAddress == null && Consol != null)
			{
				orgAddress = Consol.UnpackDepotAddress;
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			OrgAddress orgAddress = DeclarationBO.DepotDocAddress.Address;
			if (DeclarationBO.JE_ContainerMode == Core.Constants.ContainerModes.FCL)
			{
				orgAddress = DeclarationBO.ContainerTerminalOperatorDocAddress.Address;
			}
			else if (DeclarationBO.JE_ContainerMode == Core.Constants.ContainerModes.Containerised)
			{
				if (DeclarationBO.CusContainers.Count > 0)
				{
					ZString containerType = DeclarationBO.CusContainers[0].CO_FCL_LCL_AIR;
					if (containerType == Core.Constants.ContainerModes.FCL || containerType == Core.Constants.ContainerModes.FCLMixedShipper)
					{
						orgAddress = DeclarationBO.ContainerTerminalOperatorDocAddress.Address;
					}
				}
			}
			if (orgAddress == null && WrapperFromShipment != null)
			{
				return WrapperFromShipment.GoodsAvailableAt;
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		OrganisationWrapper GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType usageType, JobDocAddress jobDocAddress, OrgHeader organisation, ContactType contactType)
		{
			return (jobDocAddress == null || jobDocAddress.IsEmpty) && organisation != null
				? new OrganisationWrapper(usageType, organisation.MainAddress, contactType, Factory)
				: new OrganisationWrapper(usageType, jobDocAddress, Factory);
		}

		AddressWrapper GetWrappedJobDocAddressOrFallbackIfEmpty(JobDocAddress jobDocAddress, OrgHeader organisation, ContactType contactType)
		{
			return (jobDocAddress == null || jobDocAddress.IsEmpty) && organisation != null
				? new AddressWrapper(organisation.MainAddress, contactType, Factory)
				: new AddressWrapper(jobDocAddress, Factory);
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return ConsolBO.Logs.CreatedDateUtc;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return DeclarationBO.LogsOfDeclarationOrShipment.CreatedDateUtc;
		}
		#endregion

		#region PlaceAndDates
		protected override PlaceAndDateWrapper GetOrigin()
		{
			var unloco = DeclarationBO.OriginForDocuments;
			var estDate = DeclarationBO.JE_DateAtOrigin;
			var actDate = DeclarationBO.JE_DateAtOrigin;

			if (ShipmentBO != null)
			{
				var shipmentData = WrapperFromShipment.Origin;

				if (unloco.IsEmpty)
				{
					unloco = shipmentData.Location.UNLOCO;
					estDate = shipmentData.EstimatedDate;
					actDate = shipmentData.ActualDate;
				}
				else if (unloco.EqualsIgnoringCase(shipmentData.Location.UNLOCO))
				{
					if (estDate.IsEmpty)
					{
						estDate = shipmentData.EstimatedDate;
					}

					if (actDate.IsEmpty)
					{
						actDate = shipmentData.ActualDate;
					}
				}
			}

			return new PlaceAndDateWrapper(unloco, estDate, actDate, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			var unloco = DeclarationBO.FinalDestinationForDocuments;
			var estDate = DeclarationBO.JE_DateAtFinalDestination;
			var actDate = DeclarationBO.JE_DateAtFinalDestination;

			if (ShipmentBO != null)
			{
				var shipmentData = WrapperFromShipment.Destination;

				if (unloco.IsEmpty)
				{
					unloco = shipmentData.Location.UNLOCO;
					estDate = shipmentData.EstimatedDate;
					actDate = shipmentData.ActualDate;
				}
				else if (unloco.EqualsIgnoringCase(shipmentData.Location.UNLOCO))
				{
					if (estDate.IsEmpty)
					{
						estDate = shipmentData.EstimatedDate;
					}

					if (actDate.IsEmpty)
					{
						actDate = shipmentData.ActualDate;
					}
				}
			}

			return new PlaceAndDateWrapper(unloco, estDate, actDate, Factory);
		}

		#endregion

		#region SuppressiongBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return ShipmentBO; }
		}

		#endregion

		#region ValueAndUnits
		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			var quantity = DeclarationBO.JE_TotalNoOfPieces;

			if (ShipmentBO != null)
			{
				if (quantity.IsEmpty)
				{
					quantity = WrapperFromShipment.ShipmentInnerPacksQty.Value.ToZInt();
				}
			}

			return new PackQTYWrapper(quantity, ZString.Empty, new CodeDescriptionPairList(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			PackQTYWrapper result = null;
			if (DeclarationBO.JE_TotalNoOfPacks == 0 && ShipmentBO != null)
			{
				result = WrapperFromShipment.ShipmentOuterPacksQty;
			}
			else
			{
				result = new PackQTYWrapper(DeclarationBO.JE_TotalNoOfPacks, DeclarationBO.JE_TotalNoOfPacksPackType
					, DeclarationBO.Lookups.JE_TotalNoOfPacksPackType_List, Factory);
			}
			return result;
		}

		protected override WeightWrapper GetWeight()
		{
			WeightWrapper result = null;

			if (DeclarationBO.JE_TotalWeight == 0 && ShipmentBO != null)
			{
				result = WrapperFromShipment.Weight;
			}
			else
			{
				result = new WeightWrapper(DeclarationBO.JE_TotalWeight, DeclarationBO.JE_TotalWeightUnit, JobDeclarationSchema.JE_TotalWeight.Scale
				, DeclarationBO.Lookups.WeightUnitList, Factory);
			}

			return result;
		}

		protected override VolumeWrapper GetVolume()
		{
			VolumeWrapper result = null;

			if (DeclarationBO.JE_TotalVolume == 0 && ShipmentBO != null)
			{
				result = WrapperFromShipment.Volume;
			}
			else
			{
				result = new VolumeWrapper(DeclarationBO.JE_TotalVolume, DeclarationBO.JE_TotalVolumeUnit
				, DeclarationBO.Lookups.VolumeUnitList, Factory);
			}

			return result;
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			return new ValueAndUnitWrapper(ShipmentBO.JS_ActualChargeable, ShipmentBO.JS_ChargeableUnit, new CodeDescriptionPairList(), Factory);
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			if (DeclarationBO.IsExport && !DeclarationBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours.IsEmpty)
			{
				return new ValueAndUnitWrapper(DeclarationBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours,
					DeclarationBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
			}
			else
			{
				return new ValueAndUnitWrapper(ShipmentBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours,
					ShipmentBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
			}
		}

		#endregion

		#region MoneyWrappers
		protected override MoneyWrapper GetGoodsValue()
		{
			Money totalOfInvoiceAmounts = Money.Empty;
			foreach (BaseJobComInvoiceHeader invoiceHeader in DeclarationBO.Invoices)
			{
				Money invoiceAmount = new Money(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
				totalOfInvoiceAmounts = invoiceHeader.CurrencyConverter.Add(totalOfInvoiceAmounts, invoiceAmount);
			}
			return new MoneyWrapper(totalOfInvoiceAmounts, Factory);
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_UnitFreightRate, ShipmentBO.FrtRateCurrency), Factory);
		}
		#endregion

		#region General Freight References

		protected override ZString GetGoodsDescription()
		{
			return DeclarationBO.GoodsDescriptionForDocuments;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return DeclarationBO.JE_MarksAndNumbers;
		}

		protected override ZString GetOwnerReference()
		{
			var ownerRef = DeclarationBO.GetOwnersRefOverrideForDocuments(ARInvoice != null && ARInvoice.Invoice != null ? ARInvoice.Invoice.Header : null);
			if (!ownerRef.IsEmpty)
			{
				return ownerRef;
			}

			ownerRef = DeclarationBO.JE_OwnerRef;
			if (!ownerRef.IsEmpty)
			{
				var orderItems = DeclarationBO.Shipment != null ? DeclarationBO.Shipment.DocsAndCartage.OrderItems : DeclarationBO.DocsAndCartage.OrderItems;
				if (orderItems.Count == 0
					|| (!orderItems.AsString.EqualsIgnoringCase(ownerRef.Replace(", ", ","))
						&& !orderItems.Cast<OrderItem>().Any(order => order.JT_OrderReference.EqualsIgnoringCase(ownerRef))))
				{
					return ownerRef;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetHouseBill()
		{
			var result = DeclarationBO.JE_HouseBillForGenericWrapper;

			if (result.IsEmpty && ShipmentBO != null)
			{
				result = WrapperFromShipment.HouseBill;
			}

			return result;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return DeclarationBO.HouseBillIssuedDate;
		}

		protected override ZString GetHBLContainerMode()
		{
			return DeclarationBO.JE_ContainerMode.IsEmpty && DeclarationBO.CusContainers.Count > 0
				? DeclarationBO.CusContainers[0].CO_FCL_LCL_AIR
				: DeclarationBO.JE_ContainerMode;
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

		protected override ZString GetWarehouseLocation()
		{
			return ShipmentBO.JS_WarehouseLocation;
		}

		protected override ZDateTime GetActualReceive()
		{
			return ShipmentBO.JS_A_RCV;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return DeclarationBO.IsExport && !DeclarationBO.JE_DeliveryOrPickupLabourCharge.IsEmpty
				? DeclarationBO.JE_DeliveryOrPickupLabourCharge
				: ShipmentBO.DocsAndCartage.JP_PickupLabourCharge;
		}

		protected override ZString GetPickupLabourTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			ZString result = time.GetTextFromTime(DeclarationBO.JE_DeliveryOrPickupLabourTime);
			return DeclarationBO.IsExport && !result.IsEmpty ? result : time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupLabourTime);
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return DeclarationBO.IsExport && !DeclarationBO.JE_PickupOrDeliveryTruckWaitCharge.IsEmpty
				? DeclarationBO.JE_PickupOrDeliveryTruckWaitCharge
				: ShipmentBO.DocsAndCartage.JP_PickupTruckWaitCharge;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			ZString result = time.GetTextFromTime(DeclarationBO.JE_PickupOrDeliveryTruckWaitTime);
			return DeclarationBO.IsExport && !result.IsEmpty ? result : time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		protected override ZString GetShippersReference()
		{
			return ShipmentBO.JS_BookingReference;
		}

		protected override ZString GetJobNumberHeading()
		{
			return ShipmentBO != null ? Res.GetString("5569e6e1-36a1-4915-b2ec-f83000a3de10", "Shipment") : Res.GetString("ef558937-960d-4764-9c66-05786cb93903", "Brokerage");
		}

		protected override ZString GetJobNumber()
		{
			return ShipmentBO != null ? ShipmentBO.JS_UniqueConsignRef : DeclarationBO.JE_DeclarationReference;
		}

		protected override ZString GetSecondaryHeading()
		{
			return ConsolBO != null ? Res.GetString("bae3bb08-e6c4-401a-9a90-45fb1fa8aa79", "Consol") : (!DeclarationBO.DeclarationNumber.IsEmpty ? Res.GetString("9cb4ec8f-e76d-4596-86a6-2355686b6522", "Declaration") : string.Empty);
		}

		protected override ZString GetSecondaryNumber()
		{
			return ConsolBO != null ? ConsolBO.JK_UniqueConsignRef : DeclarationBO.DeclarationNumber;
		}

		protected override TextBarcode DocManagerBarcode
		{
			get { return (DeclarationBO.Shipment != null && ShipmentBO != null) ? FreightWrapperFromShipment.TextBarcodeForShipment(ShipmentBO, DocTypeCode) : base.DocManagerBarcode; }
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

		protected override RouteWrapper GetInterestedRoute()
		{
			return new RouteWrapper(TransportBO, Factory);
		}

		protected override ZString GetMasterBillHeading()
		{
			if (Declaration.IsSea)
			{
				return Res.GetString("3628f958-dc28-46f7-973f-bf0326352dcc", "Ocean Bill Of Lading");
			}
			if (Declaration.IsAir)
			{
				return Res.GetString("be1265a0-2ccc-4fb7-a1c1-6bfd02afba6f", "MAWB");
			}
			return Res.GetString("69d878cd-fa08-4b71-979f-52645485d4bc", "Master Bill");
		}

		protected override ZString GetHouseBillHeading()
		{
			if (Declaration.IsSea)
			{
				return Res.GetString("ae75ccf9-ca80-4999-86a3-9874527d4fba", "House Bill Of Lading");
			}
			if (Declaration.IsAir)
			{
				return Res.GetString("d2cb42ed-85b4-4137-b1f6-de4ae67babab", "HAWB");
			}
			if (Declaration.IsPost)
			{
				return Res.GetString("7ad205b3-5240-4702-b871-e6103d4be139", "Parcel Post Numbers");
			}
			return Res.GetString("3d7d1acb-2c71-4fbb-ac09-e6dd163f8ca0", "House Bill");
		}

		protected override ZString GetAdditionalTerms()
		{
			return ShipmentBO != null ? ShipmentBO.JS_AdditionalTerms : ZString.Empty;
		}

		protected override ZDecimal GetLoadingMeters()
		{
			return ShipmentBO != null ? ShipmentBO.JS_LoadingMeters : ZDecimal.Zero;
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
			return DeclarationBO.DocsAndCartage.JP_EstimatedDelivery;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return DeclarationBO.DocsAndCartage.JP_DeliveryRequiredBy;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			return DeclarationBO.DocsAndCartage.JP_DeliveryCartageAdvised;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			return DeclarationBO.DocsAndCartage.JP_DeliveryCartageCompleted;
		}

		protected override ZDateTime GetPickupFrom()
		{
			return DeclarationBO.DocsAndCartage.JP_EstimatedPickup;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return DeclarationBO.DocsAndCartage.JP_PickupRequiredBy;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return DeclarationBO.DocsAndCartage.JP_PickupCartageAdvised;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			return DeclarationBO.DocsAndCartage.JP_PickupCartageCompleted;
		}

		#endregion

		#region Custom Attributes
		protected override ZString GetCustomAttribute1()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomAttrib2;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return DeclarationBO.DocsAndCartage.JP_CustomFlag2;
		}
		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(DeclarationBO, RoutingLevel.Consol, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(DeclarationBO, RoutingLevel.Shipment, Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return DeclarationBO.ShowInvoiceNumbersOnDocuments ? new CommercialInvoiceWrapperCollection(DeclarationBO, Factory) : new CommercialInvoiceWrapperCollection(Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return new CommercialInvoiceLineWrapperCollection(DeclarationBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			var declarationContainers = new ContainerWrapperCollection(DeclarationBO, Factory);
			if (WrapperFromShipment != null)
			{
				var containers1 = from ContainerWrapper container in declarationContainers orderby container.ContainerNo select container.ContainerNo;
				var containers2 = from ContainerWrapper container in WrapperFromShipment.Containers orderby container.ContainerNo select container.ContainerNo;
				if (DeclarationBO.CusContainers.Count == 0 || containers1.SequenceEqual(containers2))
				{
					return WrapperFromShipment.Containers;
				}
			}
			return declarationContainers;
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(DeclarationBO.DocsAndCartage.RequiredDocuments, Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(DeclarationBO.DocsAndCartage.Services, Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			if (ShipmentBO != null && (!DeclarationBO.IsPackingInformationRelevant || DeclarationBO.Packages.Count == 0))
			{
				return new PackageWrapperCollection(ShipmentBO, Factory);
			}
			else
			{
				return new PackageWrapperCollection(DeclarationBO, Factory);
			}
		}

		protected override ContainerPenaltyWrapperCollection GetImportContainerPenalties()
		{
			return ShipmentBO != null ? WrapperFromShipment.ImportContainerPenalties : new ContainerPenaltyWrapperCollection(DeclarationBO, Factory, ContainerPenaltyDirection.Import);
		}

		protected override ContainerPenaltyWrapperCollection GetExportContainerPenalties()
		{
			return ShipmentBO != null ? WrapperFromShipment.ExportContainerPenalties : new ContainerPenaltyWrapperCollection(DeclarationBO, Factory, ContainerPenaltyDirection.Export);
		}

		protected override ContainerPenaltyWrapperCollection GetContainerPenalties()
		{
			return ShipmentBO != null ? WrapperFromShipment.ContainerPenalties : new ContainerPenaltyWrapperCollection(DeclarationBO, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			if (ShipmentBO != null && (!DeclarationBO.IsPackingInformationRelevant || DeclarationBO.Packages.Count == 0))
			{
				return new UNDGSubstanceWrapperCollection((PackLine[])ShipmentBO.OuterPackLines.ToArray(typeof(PackLine)), Factory);
			}
			else
			{
				return new UNDGSubstanceWrapperCollection(new List<BasePackage>((BasePackage[])DeclarationBO.Packages.ToArray(typeof(BasePackage))), Factory);
			}
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(DeclarationBO, RoutingLevel.Shipment, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return new FreightWrapperCollection(DeclarationBO, RoutingLevel.Consol, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(DeclarationBO, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			return ShipmentBO != null ? new FreightWrapperFromShipment(ShipmentBO, Factory).PickupDeliveryConfirmations : new PickupDeliveryConfirmationsWrapperCollection(Factory);
		}

		protected override OrderWrapperCollection GetOrders()
		{
			return ShipmentBO != null ? WrapperFromShipment.Orders : new OrderWrapperCollection(DeclarationBO, Factory);
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			return ShipmentBO != null ? WrapperFromShipment.OrderLines : new OrderLineWrapperCollection(DeclarationBO, Factory);
		}

		protected override CO2eEmissionWrapperCollection GetCO2eEmissions()
		{
			return ShipmentBO != null ? WrapperFromShipment.CO2eEmissions : base.GetCO2eEmissions();
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Declaration;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return DeclarationBO.PK;
		}
		#endregion

		#region Branding

		protected override Image JobHeaderBranchLogo
		{
			get
			{
				Image result = base.JobHeaderBranchLogo;
				if (result == null)
				{
					DocBranch branch = DocBranch.New(Declaration.Branch, Factory);
					return branch != null ? branch.Logo : null;
				}
				return result;
			}
		}

		#endregion

		#region CA specific properties

		bool IsCAJobDeclaration
		{
			get { return DeclarationBO != null && DeclarationBO is Integration.Customs.CA.IJobDeclaration; }
		}

		protected override DocReleaseStatusCollection GetCAReleaseStatus()
		{
			return IsCAJobDeclaration ? (DocReleaseStatusCollection)Customs.CA.GetCAReleaseStatus() : base.GetCAReleaseStatus();
		}

		protected override ZString GetCAPreviousCCN()
		{
			return IsCAJobDeclaration ? Customs.CA.GetCAPreviousCCN() : base.GetCAPreviousCCN();
		}

		protected override ZString GetCATransactionNo()
		{
			return IsCAJobDeclaration ? Customs.CA.GetCATransactionNo() : base.GetCATransactionNo();
		}

		protected override bool GetCAHideCarrier()
		{
			return false;
		}

		protected override ZString GetCargoControlNumberForCanada()
		{
			return IsCAJobDeclaration ? Customs.CA.GetCargoControlNumberForCanada() : ZString.Empty;
		}

		protected override ZString GetPreviousCargoControlNumberForCanada()
		{
			return IsCAJobDeclaration ? Customs.CA.GetPreviousCargoControlNumberForCanada() : ZString.Empty;
		}

		protected override ZString GetCACarrierName()
		{
			return IsCAJobDeclaration ? Customs.CA.GetCACarrierName() : ZString.Empty;
		}

		protected override ZString GetCAUSPortOfExit()
		{
			return IsCAJobDeclaration ? Customs.CA.GetCAUSPortOfExit() : ZString.Empty;
		}

		protected override ZDateTime GetDateOfFirstArrival()
		{
			return IsCAJobDeclaration ? Customs.CA.GetDateOfFirstArrival() : ZDateTime.Empty;
		}

		protected override ZDateTime GetWarehouseReleaseDate()
		{
			return IsCAJobDeclaration ? Customs.CA.GetWarehouseReleaseDate() : ZDateTime.Empty;
		}

		#endregion

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			if (ShipmentBO == null)
			{
				return DeclarationBO;
			}
			else
			{
				return ShipmentBO;
			}
		}

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return ShipmentBO != null
				? CartageInfoWrapper.New(ShipmentBO, Factory)
				: CartageInfoWrapper.New(DocBaseJobDeclaration.New(DeclarationBO, Factory), Factory);
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
	}
}
