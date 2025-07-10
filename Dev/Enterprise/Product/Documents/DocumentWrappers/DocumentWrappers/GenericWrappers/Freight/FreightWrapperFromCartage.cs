using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class FreightWrapperFromCartage : FreightWrapper, ILegsOverrider, IDocTypeCode
	{
		public FreightWrapperFromCartage(CommonCartage cartageBO, BusinessObjectFactory factory)
			: base(cartageBO, factory)
		{
			Argument.NotNull(factory, "factory");
			CartageBO = cartageBO ?? factory.GetNull<CommonCartage>();
		}
		readonly CommonCartage CartageBO;

		#region Freight Business Objects used for fallbacks
		BaseJobDeclaration DeclarationBO
		{
			get
			{
				if (object.ReferenceEquals(fDeclarationBO, null))
				{
					fDeclarationBO = CartageBO.CartageParent as BaseJobDeclaration;
					if (fDeclarationBO == null)
					{
						fDeclarationBO = Factory.GetNull<BaseJobDeclaration>();
					}
				}
				return fDeclarationBO;
			}
		}
		BaseJobDeclaration fDeclarationBO;

		ForwardingShipment ShipmentBO
		{
			get
			{
				if (object.ReferenceEquals(fShipmentBO, null))
				{
					fShipmentBO = CartageBO.CartageParent as ForwardingShipment;
					if (fShipmentBO == null)
					{
						fShipmentBO = Factory.GetNull<ForwardingShipment>();
					}
				}
				return fShipmentBO;
			}
		}
		ForwardingShipment fShipmentBO;

		ForwardingConsol ConsolBO
		{
			get
			{
				if (object.ReferenceEquals(fConsolBO, null))
				{
					fConsolBO = ShipmentBO.Consols.GetEarliestConsol();
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

		#region Related Business Objects

		protected override BaseJobDeclaration GetDeclaration()
		{
			return DeclarationBO;
		}

		protected override ForwardingShipment GetShipment()
		{
			return ShipmentBO;
		}

		protected override CommonCartage GetCartage()
		{
			return CartageBO;
		}

		protected override ForwardingConsol GetConsol()
		{
			return ConsolBO;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ShipmentBO;
		}

		protected override Job GetJob()
		{
			return CartageBO == null ? null : (Job)CartageBO.Job;
		}

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(DocCommonCartage.New(CartageBO, Factory), Factory);
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

			if (CartageBO.IsImportOrDestination) // This doesn't look right. Should be just IsImport as it is in other places of the system.
			{
				shipmentTypeCode = "IMP";
			}
			else if (CartageBO.IsExportOrOrigin) // This doesn't look right. Should be just IsExport as it is in other places of the system.
			{
				shipmentTypeCode = "EXP";
			}
			else if (CartageBO.IsDomestic())
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
			var code = CartageBO.JJ_ContainerMode;
			if (code == Core.Constants.ContainerModes.Containerised)
			{
				code = Core.Constants.ContainerModes.FCL;
			}

			return new CodeAndDescriptionWrapper(code, CartageBO.BindToLists.ContainerModes, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(CartageBO.JJ_ShippingTransportMode, CartageBO.BindToLists.ShippingTransportModeList, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(CartageBO.JJ_RS_NKServiceLevel, CartageBO.Lookups.ServiceLevels, Factory);
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_InspectionTypeCode, ShipmentBO.Lookups.InspectionTypes, Factory);
		}

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			return CarrierServiceLevelWrapper.Empty;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			IncoTermWrapper incoWrapper = null;

			if (DeclarationBO != null && DeclarationBO.Invoices.Count > 0)
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

				if (consensusIncoTerm.IsEmpty)
				{
					incoWrapper = new IncoTermWrapper(DeclarationBO.JE_ShipmentIncoTerm, DeclarationBO.Lookups.IncoTermList, IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);
				}
				else
				{
					ZString incoTerm = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(consensusIncoTerm);

					incoWrapper = incoTerm.IsEmpty ? new IncoTermWrapper(consensusIncoTerm, DeclarationBO.Lookups.IncoTermList, IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory) :
						new IncoTermWrapper(incoTerm, Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>(), IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);
				}
			}
			else
			{
				incoWrapper = new IncoTermWrapper(ShipmentBO.JS_INCO, ShipmentBO.Lookups.JS_INCO_List, IncoTermWrapper.Deciders.ByPaymentType(ShipmentBO.JS_PaymentTerm), Factory);
			}

			return incoWrapper;
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
			ZString cartagePickupMode;
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
			return CartageBO.JJ_ConsignmentID;
		}

		protected override ZString GetExportAgentsReference()
		{
			return CartageBO.IsExportOrOrigin ? CartageBO.JJ_ConsignmentID : ZString.Empty;
		}

		protected override ZString GetImportAgentsReference()
		{
			return CartageBO.IsImportOrDestination ? CartageBO.JJ_ConsignmentID : ZString.Empty;
		}

		protected override ZString GetBookingReference()
		{
			return ConsolBO.JK_BookingReference;
		}

		protected override ZString GetMasterBill()
		{
			var mabFromParents = GetMasterBillFromCartageParents();
			var result = mabFromParents == HouseBill ? ZString.Empty : mabFromParents;

			return result.IsEmpty && HouseBill.IsEmpty ? CartageBO.JJ_WaybillNumber : result;
		}

		ZString GetMasterBillFromCartageParents()
		{
			var masterBillRefNo = CartageBO.AdditionalReferenceNumbers.Cast<CusEntryNumber>().OrderBy(x => x.CE_SystemCreateTimeUtc).FirstOrDefault(x => x.CE_EntryType == AdditionalReferenceTypes.Codes.MasterBill);
			var possibleNumbers = new[]
			{
				masterBillRefNo?.CE_EntryNum,
				DeclarationBO?.JE_MasterBill,
				ConsolBO?.JK_MasterBillNum
			};

			return possibleNumbers.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? ZString.Empty;
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

		#region Organisations

		protected override OrganisationWrapper GetCarrier()
		{
			return DeclarationBO != null
				? new OrganisationWrapper(OrganisationUsageType.Carrier, DeclarationBO.ShippingLine, ContactType.ShippingLine, Factory)
				: new OrganisationWrapper(OrganisationUsageType.Carrier, ConsolBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (Cartage.IsExportOrOrigin)
			{
				return new AddressWrapper(CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS), Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			if (Cartage.IsImportOrDestination)
			{
				return new AddressWrapper(CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO), Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			if (Cartage.IsExportOrOrigin)
			{
				return new AddressWrapper(CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO), Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			if (Cartage.IsExportOrOrigin)
			{
				JobDocAddress cfsAddress = CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
				if (cfsAddress != null)
				{
					return new AddressWrapper(cfsAddress, Factory);
				}
				else
				{
					return new AddressWrapper(CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO), Factory);
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
			OrganisationWrapper consignor;
			JobDocAddress cartageExporterAddress = CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			if (cartageExporterAddress == null)
			{
				consignor = ParentJob.Consignor;
			}
			else
			{
				consignor = new OrganisationWrapper(OrganisationUsageType.Consignor, cartageExporterAddress, Factory);
			}

			return consignor;
		}

		protected override FreightWrapper GetParentJob()
		{
			var cartageParentJob = CartageBO.ParentJob;
			var freightWrappers = cartageParentJob != null ? FreightWrapper.New(cartageParentJob, Factory) : Array.Empty<FreightWrapper>();
			var result = freightWrappers.Any() ? freightWrappers[0] : base.GetParentJob();

			if (result == null)
			{
				var name = cartageParentJob != null ? cartageParentJob.GetType().Name : "null";
				var id = "null";
				if (cartageParentJob != null)
				{
					var cartageParent = cartageParentJob as ICartageParent;
					id = cartageParent != null ? cartageParent.CartageParentID.ToString() : cartageParentJob.PK.ToString();
				}

				throw new InvalidOperationException(ZString.Format("Could not create a wrapper for Cartage Parent {0} {1}", name, id));
			}

			return result;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			OrganisationWrapper consignee;
			JobDocAddress cartageImporterAddress = CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			if (cartageImporterAddress == null)
			{
				consignee = ParentJob.Consignee;
			}
			else
			{
				consignee = new OrganisationWrapper(OrganisationUsageType.Consignee, cartageImporterAddress, Factory);
			}

			return consignee;
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

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return DeclarationBO != null
				? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, DeclarationBO.Forwarder, ContactType.FreightAgent, Factory)
				: ConsolBO.IsExport()
					? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory)
					: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsExport
				? new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return DeclarationBO != null && DeclarationBO.IsExport
				? new OrganisationWrapper(OrganisationUsageType.ExportBroker, DeclarationBO.Branch.OrgProxy, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ExportBroker, ShipmentBO.ExportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ImportAgent, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ImportAgent, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ImportBroker, DeclarationBO.Branch.OrgProxy, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ImportBroker, ShipmentBO.ImportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = DeclarationBO != null && DeclarationBO.IsExport
				? new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			JobDocAddress docAddress = DeclarationBO.NotifyPartyDocumentaryAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.NotifyPartyDocumentaryAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, docAddress, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, DeclarationBO.DocsAndCartage.DeliveryCartageCo, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, ShipmentBO.DocsAndCartage.DeliveryCartageCo, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsExport
				? new OrganisationWrapper(OrganisationUsageType.PickupAgent, DeclarationBO.DocsAndCartage.PickupCartageCo, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.PickupAgent, ShipmentBO.DocsAndCartage.PickupCartageCo, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO) != null ? CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO).Organisation : null;
			orgHeader = orgHeader == null && DeclarationBO.IsImport ? DeclarationBO.ContainerTerminalOperatorDocAddress.Organisation : null;
			if (orgHeader == null && ConsolBO != null && ConsolBO.ArrivalCTOAddress != null && ConsolBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = ConsolBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter), Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			if (DeclarationBO != null)
			{
				JobDocAddress docAddress = DeclarationBO.IsExport ? DeclarationBO.SupplierPickupAddress : null;
				if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
				{
					docAddress = ShipmentBO.ConsignorPickupAddress;
				}
				return (docAddress == null || docAddress.IsEmpty) ? null : new AddressWrapper(docAddress, Factory);
			}
			else
			{
				return new AddressWrapper(ShipmentBO.ConsignorPickupAddress, Factory);
			}
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return ConsolBO.Logs.CreatedDateUtc;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return DeclarationBO != null ? DeclarationBO.LogsOfDeclarationOrShipment.CreatedDateUtc : ShipmentBO.Logs.CreatedDateUtc;
		}
		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			return DeclarationBO != null
				? new PlaceAndDateWrapper(DeclarationBO.JE_RL_NKOrigin, DeclarationBO.JE_DateAtOrigin, DeclarationBO.JE_DateAtOrigin, Factory)
				: new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKOrigin, ShipmentBO.JS_E_DEP, ShipmentBO.JS_E_DEP, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			return DeclarationBO != null
				? new PlaceAndDateWrapper(DeclarationBO.JE_RL_NKFinalDestination, DeclarationBO.JE_DateAtFinalDestination, DeclarationBO.JE_DateAtFinalDestination, Factory)
				: new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKDestination, ShipmentBO.JS_E_ARV, ShipmentBO.JS_E_ARV, Factory);
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
			return ParentJob.ShipmentInnerPacksQty;
		}

		#region GetShipmentOuterPacksQty

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(GetPackagesQtyTotal(), GetTotalPackTypes(), BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		ZInt GetPackagesQtyTotal()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				return Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value.ToZInt());
			}
			return CartageBO.JJ_OuterPacks;
		}

		ZString GetTotalPackTypes()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				return Packages.Cast<PackageWrapper>().Select(p => p.Packages.Unit.Code).Distinct().Count() == 1 ? Packages[0].Packages.Unit.Code.ToString() : Core.Constants.PkgUnit.Piece;
			}
			return CartageBO.JJ_F3_NKPackType;
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			return new WeightWrapper(GetTotalWeight(), GetTotalWeightUnit(), WeightWrapper.StandardDecimalPlaces, BindToLists.GetCachedLists(Factory).WeightUnits, Factory);
		}

		ZDecimal GetTotalWeight()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				var packageWrappers = Packages.Cast<PackageWrapper>();
				var distinctWeightUnits = packageWrappers.Select(p => p.Weight.Unit.Code).Distinct();

				return distinctWeightUnits.Count() == 1
					? packageWrappers.Sum(p => p.Weight.Value)
					: packageWrappers.Sum(p => Core.Constants.Weight.ConvertSafe(p.Weight.Value, p.Weight.Unit.Code, Env.Registry.FreightWeightUnit));
			}
			return CartageBO.JJ_Weight;
		}

		ZString GetTotalWeightUnit()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				var packageWrappers = Packages.Cast<PackageWrapper>();
				return packageWrappers.Select(p => p.Weight.Unit.Code).Distinct().Count() == 1 ? packageWrappers.First().Weight.Unit.Code : (ZString)Env.Registry.FreightWeightUnit;
			}
			return CartageBO.JJ_WeightUQ;
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			return new VolumeWrapper(GetTotalVolume(), GetTotalVolumeUnit(), BindToLists.GetCachedLists(Factory).VolumeUnits, Factory);
		}

		ZDecimal GetTotalVolume()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				var packageWrappers = Packages.Cast<PackageWrapper>();
				if (packageWrappers.Select(p => p.Volume.Unit.Code).Distinct().Count() == 1)
				{
					return packageWrappers.Sum(p => p.Volume.Value);
				}

				return packageWrappers.Sum(p => Core.Constants.Volume.ConvertSafe(p.Volume.Value, p.Volume.Unit.Code, CartageBO.CartageVolumeUnit));
			}

			return CartageBO.JJ_Volume;
		}

		ZString GetTotalVolumeUnit()
		{
			if (ShouldGetPackageTotalsFromMovements)
			{
				var packageWrappers = Packages.Cast<PackageWrapper>();
				return packageWrappers.Select(p => p.Volume.Unit.Code).Distinct().Count() == 1 ? (packageWrappers.First()).Volume.Unit.Code : CartageBO.CartageVolumeUnit;
			}

			return CartageBO.JJ_VolumeUQ;
		}

		#endregion

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
			if (DeclarationBO != null && DeclarationBO.Invoices.Count > 0)
			{
				Money totalOfInvoiceAmounts = Money.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in DeclarationBO.Invoices)
				{
					Money invoiceAmount = new Money(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
					totalOfInvoiceAmounts = invoiceHeader.CurrencyConverter.Add(totalOfInvoiceAmounts, invoiceAmount);
				}
				return new MoneyWrapper(totalOfInvoiceAmounts, Factory);
			}
			else
			{
				return new MoneyWrapper(new Money(ShipmentBO.JS_GoodsValue, ShipmentBO.GoodsValueCurr), Factory);
			}
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_UnitFreightRate, ShipmentBO.FrtRateCurrency), Factory);
		}
		#endregion

		#region General Freight References

		protected override ZString GetGoodsDescription()
		{
			return CartageBO.JJ_GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return DeclarationBO != null ? DeclarationBO.JE_MarksAndNumbers : ShipmentBO.JS_MarksAndNumbers;
		}

		protected override ZString GetOwnerReference()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.AppendIfNotEmpty(DeclarationBO.JE_OwnerRef);
			result.AppendIfNotEmpty(CartageBO.JJ_OrderReferenceNumber);
			return result.ToStringWithDelimiterBetweenAppends(", ");
		}
		protected override ZString GetHouseBill()
		{
			var houseBillAddRefNo = CartageBO.AdditionalReferenceNumbers.Cast<CusEntryNumber>().OrderBy(x => x.CE_SystemCreateTimeUtc).FirstOrDefault(x => x.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill);
			var possibleNumbers = new[]
			{
				houseBillAddRefNo?.CE_EntryNum,
				DeclarationBO?.JE_HouseBill,
				ShipmentBO?.JS_HouseBill
			};

			return possibleNumbers.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? ZString.Empty;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return DeclarationBO != null ? DeclarationBO.HouseBillIssuedDate : ShipmentBO.JS_HouseBillIssueDate;
		}

		protected override ZString GetHBLContainerMode()
		{
			ZString result;
			if (DeclarationBO != null)
			{
				result = DeclarationBO.JE_ContainerMode.IsEmpty && DeclarationBO.CusContainers.Count > 0
							? DeclarationBO.CusContainers[0].CO_FCL_LCL_AIR
							: DeclarationBO.JE_ContainerMode;
			}
			else
			{
				result = ShipmentBO.JS_HBLContainerPackModeOverride.IsEmpty
							? ShipmentBO.JS_PackingMode
							: ShipmentBO.JS_HBLContainerPackModeOverride;
			}
			return result;
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
			return DeclarationBO.IsExport && !DeclarationBO.JE_DeliveryOrPickupLabourTime.IsEmpty
				? time.GetTextFromTime(DeclarationBO.JE_DeliveryOrPickupLabourTime)
				: time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupLabourTime);
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
			return DeclarationBO.IsExport && !DeclarationBO.JE_PickupOrDeliveryTruckWaitTime.IsEmpty
				? time.GetTextFromTime(DeclarationBO.JE_PickupOrDeliveryTruckWaitTime)
				: time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		protected override ZString GetShippersReference()
		{
			return ShipmentBO.JS_BookingReference;
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			var docAddress = CartageBO.IsExportOrOrigin ? CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS) : null;
			var orgAddress = docAddress != null ? docAddress.Address : null;
			if (orgAddress == null && DeclarationBO != null && DeclarationBO.IsExport)
			{
				orgAddress = DeclarationBO.DepotDocAddress.Address;
			}
			else if (orgAddress == null && ShipmentBO != null)
			{
				orgAddress = ShipmentBO.ExportReceivingDepot;
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			var docAddress = CartageBO.IsImportOrDestination ? CartageBO.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS) : null;
			var orgAddress = docAddress != null ? docAddress.Address : null;
			if (orgAddress == null && DeclarationBO != null && DeclarationBO.IsImport)
			{
				orgAddress = DeclarationBO.DepotDocAddress.Address;
			}
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
			return CartageBO.IsExportOrOrigin ? PickupCFSAddress : UnpackCFSAddress;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("f2d72983-8465-4c4d-bf78-db7cdbbb3086", "Port Transport");
		}

		protected override ZString GetJobNumber()
		{
			return CartageBO.JJ_ConsignmentID;
		}

		protected override ZString GetSecondaryHeading()
		{
			if (IsParentJobNullForwardingShipment)
			{
				return string.Empty;
			}
			else if (ParentJob is FreightWrapperFromCFSShipment)
			{
				return Res.GetString("6448679a-ebfd-4d00-813c-1456e3bb0dd1", "CFS Shipment");
			}
			else
			{
				return ParentJob?.JobNumberHeading ?? string.Empty;
			}
		}

		protected override ZString GetSecondaryNumber()
		{
			if (IsParentJobNullForwardingShipment)
			{
				return string.Empty;
			}
			else
			{
				return ParentJob?.JobNumber ?? string.Empty;
			}
		}

		bool IsParentJobNullForwardingShipment => ParentJob is FreightWrapperFromShipment wrapperFromShipment && wrapperFromShipment.BusinessObjectForPrintJob.IsNull;

		protected override ZString GetArrivalReference()
		{
			ZString result = CartageBO != null && CartageBO.SailingStandalone != null && CartageBO.SailingStandalone.Destination != null ? CartageBO.SailingStandalone.Destination.JB_ArrivalReference : ZString.Empty;
			return result.IsEmpty && ConsolBO != null && ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null ? ConsolBO.Schedule.Destination.JB_ArrivalReference : result;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			ZString result = CartageBO.SailingStandalone != null && CartageBO.SailingStandalone.Destination != null
				? CartageBO.SailingStandalone.Destination.JB_Berth
				: ZString.Empty;
			return result.IsEmpty && ConsolBO != null && ConsolBO.Schedule != null && ConsolBO.Schedule.Destination != null
				? ConsolBO.Schedule.Destination.JB_Berth
				: result;
		}

		protected override ZString GetMasterBillHeading()
		{
			return !HouseBill.IsEmpty || !GetMasterBillFromCartageParents().IsEmpty ? Res.GetString("c73b14ef-993a-4e16-a490-c5225d0a27ee", "Master Bill") : Res.GetString("5fe18e4f-f858-4841-b0b3-8c714e7f3f94", "Waybill");
		}

		protected override ZString GetHouseBillHeading()
		{
			return !HouseBill.IsEmpty || !GetMasterBillFromCartageParents().IsEmpty ? Res.GetString("045d39bb-b02e-4e93-bdf1-65a9996bd9f6", "House Bill") : string.Empty;
		}

		protected override ZString GetQuoteNumber()
		{
			return CartageBO == null ? ZString.Empty : CartageBO.JJ_QuoteNumber;
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
		protected override ZDateTime GetDeliveryFrom()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_EstimatedDelivery;
			}
			return !CartageBO.JJ_EstimatedDelivery.IsEmpty ? CartageBO.JJ_EstimatedDelivery : result;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_DeliveryRequiredBy;
			}
			return result;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_DeliveryCartageAdvised;
			}
			return result;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				return CartageBO.DocsAndCartageParent.JP_DeliveryCartageCompleted;
			}
			return result;
		}

		protected override ZDateTime GetPickupFrom()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_EstimatedPickup;
			}
			return !CartageBO.JJ_EstimatedPickup.IsEmpty ? CartageBO.JJ_EstimatedPickup : result;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_PickupRequiredBy;
			}
			return result;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_PickupCartageAdvised;
			}
			return result;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_PickupCartageCompleted;
			}
			return result;
		}

		#endregion

		#region Custom Attributes
		protected override ZString GetCustomAttribute1()
		{
			ZString result = ZString.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_CustomAttrib1;
			}
			return result;
		}

		protected override ZString GetCustomAttribute2()
		{
			ZString result = ZString.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_CustomAttrib2;
			}
			return result;
		}

		protected override ZDateTime GetCustomDate1()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_CustomDate1;
			}
			return result;
		}

		protected override ZDateTime GetCustomDate2()
		{
			ZDateTime result = ZDateTime.Empty;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_CustomDate2;
			}
			return result;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			ZDecimal result = ZDecimal.Zero;
			if (CartageBO.DocsAndCartageParent != null)
			{
				result = CartageBO.DocsAndCartageParent.JP_CustomDecimal1;
			}
			return result;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			ZDecimal result = ZDecimal.Zero;
			if (CartageBO.DocsAndCartageParent != null)
			{
				return CartageBO.DocsAndCartageParent.JP_CustomDecimal2;
			}
			return result;
		}

		protected override ZBool GetCustomFlag1()
		{
			ZBool result = ZBool.False;
			if (CartageBO.DocsAndCartageParent != null)
			{
				return CartageBO.DocsAndCartageParent.JP_CustomFlag1;
			}
			return result;
		}

		protected override ZBool GetCustomFlag2()
		{
			ZBool result = ZBool.False;
			if (CartageBO.DocsAndCartageParent != null)
			{
				return CartageBO.DocsAndCartageParent.JP_CustomFlag2;
			}
			return result;
		}
		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(CartageBO, RoutingLevel.Consol, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(CartageBO, RoutingLevel.Shipment, Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return DeclarationBO != null ? new CommercialInvoiceWrapperCollection(DeclarationBO, Factory) : new CommercialInvoiceWrapperCollection(ShipmentBO, Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return DeclarationBO != null ? new CommercialInvoiceLineWrapperCollection(DeclarationBO, Factory) : new CommercialInvoiceLineWrapperCollection(ShipmentBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(this, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(this, Factory);
		}

		protected override LocalTransportLegWrapperCollection GetLocalTransportLegs()
		{
			var legs = new LocalTransportLegWrapperCollection(this, Factory, CartageBO.CartageLegs);
			legs.Sort(LocalTransportLegWrapperCollection.SortBy.DisplayOrder);
			return legs;
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(CartageBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(CartageBO, RoutingLevel.Shipment, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return new FreightWrapperCollection(CartageBO, RoutingLevel.Consol, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			if (DeclarationBO != null)
			{
				return new CustomsEntryWrapperCollection(DeclarationBO, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new CustomsEntryWrapperCollection(ShipmentBO, Factory);
			}
			else
			{
				return new CustomsEntryWrapperCollection(CartageBO, Factory);
			}
		}

		protected override OrderWrapperCollection GetOrders()
		{
			if (DeclarationBO != null)
			{
				return new OrderWrapperCollection(DeclarationBO, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new OrderWrapperCollection(ShipmentBO, Factory);
			}
			else
			{
				return new OrderWrapperCollection(Factory);
			}
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			if (DeclarationBO != null)
			{
				return new OrderLineWrapperCollection(DeclarationBO, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new OrderLineWrapperCollection(ShipmentBO, Factory);
			}
			else
			{
				return new OrderLineWrapperCollection(Factory);
			}
		}

		protected override AddressWrapperCollection GetTransportAddresses()
		{
			var addresses = new AddressWrapperCollection(Factory);

			if (UseLegAddresses)
			{
				foreach (var leg in LegAddressOverrides)
				{
					AddAddressIfNotTheSameAsLast(addresses, leg.PickupFromDocAddress);
					AddAddressIfNotTheSameAsLast(addresses, leg.WaitPointDocAddress);
					AddAddressIfNotTheSameAsLast(addresses, leg.DeliverToDocAddress);
				}
			}
			else
			{
				addresses.Add(new AddressWrapper(CartageBO.FirstDocAddress, Factory));
				addresses.Add(new AddressWrapper(CartageBO.SecondDocAddress, Factory));
				addresses.Add(new AddressWrapper(CartageBO.ThirdDocAddress, Factory));
				addresses.Add(new AddressWrapper(CartageBO.FourthDocAddress, Factory));
			}

			return addresses;
		}

		void AddAddressIfNotTheSameAsLast(AddressWrapperCollection addresses, JobDocAddress address)
		{
			if (address != null)
			{
				var last = addresses.Count > 0 ? addresses[addresses.Count - 1] : null;
				if (last == null || last.WrappedObject != address)
				{
					addresses.Add(new AddressWrapper(address, Factory));
				}
			}
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.NoBusinessContext;
		}

		#endregion

		#region IDocTypeCode Members

		ZString IDocTypeCode.DocTypeCode { get; set; }

		#endregion

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return IsFromCartageLeg ? (BusinessObject)LocalTransportLegs[0].WrappedObject : base.BusinessObjectToLogAgainst; }
		}

		#region ILegsOverrider

		void ILegsOverrider.SetLegs(CommonCartageLeg[] legs, bool useLegAsCartageInfo, bool useLegsAsAddressInfo, bool removeContainers)
		{
			if (legs != null && legs.Any())
			{
				RemoveUnrelatedLocalTransportLegs(legs, useLegAsCartageInfo, useLegsAsAddressInfo);

				var movesToKeep = legs.Select(l => l.BookedCtgMove);
				FreightWrapper.RemoveUnrelatedPackages(this, movesToKeep.ToArray());

				if (removeContainers)
				{
					Containers.RemoveAll();
				}
			}
		}

		protected override void RemoveUnrelatedLocalTransportLegs(CommonCartageLeg[] legsToKeep, bool useFirstLegAsCartageInfo, bool useLegsAsAddressInfo)
		{
			if (useLegsAsAddressInfo)
			{
				LegAddressOverrides = legsToKeep;
			}

			base.RemoveUnrelatedLocalTransportLegs(legsToKeep, useFirstLegAsCartageInfo, useLegsAsAddressInfo);
		}

		ZBool ShouldGetPackageTotalsFromMovements
		{
			get { return UseLegAddresses && Packages.Count > 0; }
		}

		ZBool UseLegAddresses
		{
			get { return LegAddressOverrides != null && LegAddressOverrides.Length > 0; }
		}

		CommonCartageLeg[] LegAddressOverrides;

		#endregion
	}
}
