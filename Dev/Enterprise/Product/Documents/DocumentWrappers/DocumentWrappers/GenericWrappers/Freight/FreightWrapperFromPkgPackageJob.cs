using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPkgPackageJob : FreightWrapper, IDocTypeCode, IPackageOverrider
	{
		public FreightWrapperFromPkgPackageJob(PkgPackageJob pkgPackageJob, BusinessObjectFactory factory)
			: base(pkgPackageJob, factory)
		{
			Argument.NotNull(factory, "factory");
		}

		PkgPackageJob PackageJob
		{
			get { return (PkgPackageJob)WrappedBO; }
		}

		#region Related Objects

		#region ParentJob

		protected override FreightWrapper GetParentJob()
		{
			if (parentJob == null)
			{
				var freightWrappers = FreightWrapper.New((BusinessObject)PackageJob.ParentJob, Factory);
				parentJob = freightWrappers.Length > 0 ? freightWrappers[0] : null;

				var iDocTypeCode = parentJob as IDocTypeCode;
				if (iDocTypeCode != null)
				{
					iDocTypeCode.DocTypeCode = docTypeCode;
				}
			}

			return parentJob;
		}

		FreightWrapper parentJob;

		#endregion

		#region PackingParentWrapper

		IPackingParentWrapper PackingParentWrapper
		{
			get { return ParentJob as IPackingParentWrapper; }
		}

		#endregion

		#endregion

		#region Common Parent Property Overrides

		protected override OrgCarrierAccountWrapper GetOrgCarrierAccount()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CarrierAccount : null;
		}

		protected override BaseJobDeclaration GetDeclaration()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Declaration : null;
		}

		protected override ForwardingShipment GetShipment()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightShipment : null;
		}

		protected override CFSShipment GetCFSShipment()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CFSShipment : null;
		}

		protected override ForwardingConsol GetConsol()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Consol : null;
		}

		protected override CFSLoadListConsol GetCFSLoadList()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CFSLoadList : null;
		}

		protected override CommonCartage GetCartage()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Cartage : null;
		}

		protected override Order GetOrder()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? ParentJob.Order : null;
		}

		protected override AgencyShipment GetAgencyShipment()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.AgencyShipment : null;
		}

		protected override CommonShipment GetBaseShipment()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.BaseShipment : null;
		}

		protected override Job GetJob()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Job : null;
		}

		protected override QueryClaimWrapper GetQueryClaim()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.QueryClaim : null;
		}

		protected override ICusISFHeader GetImporterSecurityFiling()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ImporterSecurityFiling : null;
		}

		protected override Image GetTACImage()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TACImage : null;
		}

		protected override ZString GetFreightDepotType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightDepotType : ZString.Empty;
		}

		protected override ZDateTime GetOrderDate()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.OrderDate : ZDateTime.Empty;
		}

		protected override ZDateTime GetFactoryEx()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FactoryEx : ZDateTime.Empty;
		}

		protected override ZDateTime GetExWorksRequiredBy()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExWorksRequiredBy : ZDateTime.Empty;
		}

		protected override ZDateTime GetConsolDateCreated()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolDateCreated : ZDateTime.Empty;
		}

		protected override ZString GetConsolPaymentType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolPaymentType : ZString.Empty;
		}

		protected override ZString GetBookingReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.BookingReference : ZString.Empty;
		}

		protected override ZString GetMasterBill()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.MasterBill : ZString.Empty;
		}

		protected override ZDateTime GetMasterBillIssue()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.MasterBillIssue : ZDateTime.Empty;
		}

		protected override ZString GetConsolNumber()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolNumber : ZString.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.JobNumberHeading : ZString.Empty;
		}

		protected override ZString GetJobNumber()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.JobNumber : ZString.Empty;
		}

		protected override ZString GetSecondaryHeading()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SecondaryHeading : ZString.Empty;
		}

		protected override ZString GetSecondaryNumber()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SecondaryNumber : ZString.Empty;
		}

		protected override ZString GetOtherReferences()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.OtherReferences : ZString.Empty;
		}

		protected override ZString GetVendorID()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.VendorID : ZString.Empty;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentDateCreated : ZDateTime.Empty;
		}

		protected override ZString GetCustomAttribute1()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomAttribute1 : ZString.Empty;
		}

		protected override ZString GetCustomAttribute2()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomAttribute2 : ZString.Empty;
		}

		protected override ZDateTime GetCustomDate1()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomDate1 : ZDateTime.Empty;
		}

		protected override ZDateTime GetCustomDate2()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomDate2 : ZDateTime.Empty;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomDecimal1 : ZDecimal.Zero;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomDecimal2 : ZDecimal.Zero;
		}

		protected override ZBool GetCustomFlag1()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomFlag1 : ZBool.False;
		}

		protected override ZBool GetCustomFlag2()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomFlag2 : ZBool.False;
		}

		protected override ZString GetLocalForwarderReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.LocalForwarderReference : ZString.Empty;
		}

		protected override ZString GetExportAgentsReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportAgentsReference : ZString.Empty;
		}

		protected override ZString GetImportAgentsReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ImportAgentsReference : ZString.Empty;
		}

		protected override ZString GetGoodsDescription()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.GoodsDescription : ZString.Empty;
		}

		protected override ZString GetMarksAndNumbers()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.MarksAndNumbers : ZString.Empty;
		}

		protected override ZString GetHouseBill()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.HouseBill : ZString.Empty;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.HouseBillIssue : ZDateTime.Empty;
		}

		protected override ZString GetHBLContainerMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.HBLContainerMode : ZString.Empty;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShippedOnBoardDate : ZDateTime.Empty;
		}

		protected override ZInt GetNoOriginalBills()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.NoOriginalBills : ZInt.Zero;
		}

		protected override ZInt GetNoCopyBills()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.NoCopyBills : ZInt.Zero;
		}

		protected override ZDateTime GetDeliveryFrom()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.DeliveryFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.DeliveryCartageAdvised : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.DeliveryGoodsDelivered : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupFrom()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupRequiredBy : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupCartageAdvised : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupGoodsPickedup : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupDateOfReceipt()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupDateOfReceipt : ZDateTime.Empty;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupInterimReceipt : ZString.Empty;
		}

		protected override ZString GetWarehouseLocation()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.WarehouseLocation : ZString.Empty;
		}

		protected override ZDateTime GetActualReceive()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ActualReceive : ZDateTime.Empty;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupLabourCharge : ZDecimal.Zero;
		}

		protected override ZString GetPickupLabourTime()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupLabourTime : ZString.Empty;
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupTruckWaitCharge : ZDecimal.Zero;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupTruckWaitTime : ZString.Empty;
		}

		protected override ZString GetShippersReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShippersReference : ZString.Empty;
		}

		protected override ZString GetArrivalReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ArrivalReference : ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CTOArrivalBerth : ZString.Empty;
		}

		protected override ZString GetMasterBillHeading()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.MasterBillHeading : ZString.Empty;
		}

		protected override ZString GetHouseBillHeading()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.HouseBillHeading : ZString.Empty;
		}

		protected override ZString GetAdditionalTerms()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.AdditionalTerms : ZString.Empty;
		}

		protected override ZString GetTransportZoneCore()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TransportZone : ZString.Empty;
		}

		protected override ZDecimal GetLoadingMeters()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.LoadingMeters : ZDecimal.Zero;
		}

		protected override CodeAndDescriptionWrapper GetConsolType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolType : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetConsolContainerMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolContainerMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetOrderTransportMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.OrderTransportMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetConsolTransportMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolTransportMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.InspectionType : CodeAndDescriptionWrapper.Empty;
		}

		protected override OrganisationWrapper GetCarrier()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Carrier : new OrganisationWrapper(OrganisationUsageType.Carrier, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetMainShipToParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.MainShipToParty : new OrganisationWrapper(OrganisationUsageType.MainShipToParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetSellingParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SellingParty : new OrganisationWrapper(OrganisationUsageType.SellingParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetConsolidator()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Consolidator : new OrganisationWrapper(OrganisationUsageType.Consolidator, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetStuffingLocation()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.StuffingLocation : new OrganisationWrapper(OrganisationUsageType.StuffingLocation, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportReceivingDepotAddress : AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ImportArrivalCTOAddress : AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportReceivingCTOAddress : AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportReceivalAddress : AddressWrapper.Empty(Factory);
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolCreditor : new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentType : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentStatus : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentContainerMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentTransportMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override OrganisationWrapper GetConsignor()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Consignor : new OrganisationWrapper(OrganisationUsageType.Consignor, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetBuyer()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Buyer : new OrganisationWrapper(OrganisationUsageType.Buyer, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetSupplier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Supplier, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.InsuredBy : new OrganisationWrapper(OrganisationUsageType.InsuredBy, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.AssuredParty : new OrganisationWrapper(OrganisationUsageType.AssuredParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ClaimsPayableBy : new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SurveyReportParty : new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.LocalForwarder : new LocalForwarderOrganisationWrapper(OrganisationUsageType.LocalForwarder, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportAgent : new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ExportBroker : new OrganisationWrapper(OrganisationUsageType.ExportBroker, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ImportAgent : new OrganisationWrapper(OrganisationUsageType.ImportAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ImportBroker : new OrganisationWrapper(OrganisationUsageType.ImportBroker, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.NotifyParty : new OrganisationWrapper(OrganisationUsageType.NotifyParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.BookingParty : new OrganisationWrapper(OrganisationUsageType.BookingParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var parentWrapper = ParentJob;
			var wrapper = parentWrapper != null ? parentWrapper.ReceivingForwarder : new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, Factory.GetNull<JobDocAddress>(), Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var parentWrapper = ParentJob;
			var wrapper = parentWrapper != null ? parentWrapper.SendingForwarder : new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, Factory.GetNull<JobDocAddress>(), Factory);

			return wrapper;
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Origin : new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Destination : new PlaceAndDateWrapper(ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentInnerPacksQty : PackQTYWrapper.Empty;
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.StorageTime : ValueAndUnitWrapper.Empty;
		}

		protected override WeightWrapper GetWeight()
		{
			var packageJob = PackageJob;
			return (packageJob != null) ? new WeightWrapper(packageJob.Weight, packageJob.WeightUQ, 2, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory) : WeightWrapper.Empty;
		}

		protected override VolumeWrapper GetVolume()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Volume : VolumeWrapper.Empty;
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.GoodsValue : new MoneyWrapper(Money.Empty, Factory);
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.InsuranceValue : new MoneyWrapper(Money.Empty, Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ChargeableWeight : ValueAndUnitWrapper.Empty;
		}

		protected override MoneyWrapper GetFreightRate()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightRate : new MoneyWrapper(Money.Empty, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ServiceLevel : CodeAndDescriptionWrapper.Empty;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.IncoTerm : IncoTermWrapper.Empty;
		}

		protected override SupplierBuyerLinkWrapper GetSupplierBuyerLink()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SupplierBuyerLink : null;
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ReleaseType : CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetShippedOnBoardType()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShippedOnBoardType : CodeAndDescriptionWrapper.Empty;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			OrganisationWrapper result;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetTransportCompany(packageOverride);
			}
			else
			{
				result = ParentJob != null ? ParentJob.DeliveryAgent : new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, Factory.GetNull<JobDocAddress>(), Factory);
			}

			return result;
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupAgent : new OrganisationWrapper(OrganisationUsageType.PickupAgent, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CTOArrival : new OrganisationWrapper(OrganisationUsageType.CTOArrival, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetCaratagePickupMode()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CaratagePickupMode : CodeAndDescriptionWrapper.Empty;
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupCFSAddress : AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.UnpackCFSAddress : AddressWrapper.Empty(Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.GoodsAvailableAt : AddressWrapper.Empty(Factory);
		}

		protected override RouteWrapper GetInterestedRoute()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.InterestedRoute : new RouteWrapper(Factory.GetNull<Transport>(), Factory);
		}

		protected sealed override CostWrapperCollection GetCosts()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Costs : new CostWrapperCollection(Factory);
		}

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolRoutes : new RouteWrapperCollection(Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ShipmentRoutes : new RouteWrapperCollection(Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CommercialInvoices : new CommercialInvoiceWrapperCollection(Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CommercialInvoiceLines : new CommercialInvoiceLineWrapperCollection(Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightJobs : new FreightWrapperCollection(Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightConsolidations : new FreightWrapperCollection(Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.CustomsEntries : new CustomsEntryWrapperCollection(Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Containers : new ContainerWrapperCollection(Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.RequiredDocuments : new RequiredDocumentsWrapperCollection(Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Services : new ServiceWrapperCollection(this, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.PickupDeliveryConfirmations : new PickupDeliveryConfirmationsWrapperCollection(Factory);
		}

		protected override LocalTransportLegWrapperCollection GetLocalTransportLegs()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.LocalTransportLegs : new LocalTransportLegWrapperCollection(Factory);
		}

		protected override OrderWrapperCollection GetOrders()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Orders : new OrderWrapperCollection(Factory);
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.OrderLines : new OrderLineWrapperCollection(Factory);
		}

		protected override DocJobChargeCollection GetFreightJobsChargesForOverseasAgent()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightJobsChargesForOverseasAgent : DocJobChargeCollection.GetCollection(this, (NoResString)"Empty");
		}

		protected override DocJobChargeCollection GetFreightJobsChargesForLocalClient()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FreightJobsChargesForLocalClient : DocJobChargeCollection.GetCollection(this, (NoResString)"Empty");
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.UNDGs : new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TrackingBusinessContext : TrackingConstants.BusinessContext.NoBusinessContext;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TrackingBusinessObjectPK : ZGuid.Empty;
		}

		protected override OrganisationWrapper GetPrincipal()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Principal : new OrganisationWrapper(OrganisationUsageType.Principal, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override SundryCharges GetSundryCharges()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.SundryCharges : null;
		}

		protected override RatingWrapper GetRating()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.Rating : null;
		}

		protected override TextBarcode DocManagerBarcode
		{
			get { return null; }
		}

		protected override RunSheetWrapper GetRunSheet()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.RunSheet : null;
		}

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.BookingInstructions : new InstructionWrapperCollection(Factory);
		}

		protected override FreightWrapperCollection GetTransportBookings()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TransportBookings : new FreightWrapperCollection(Factory);
		}

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return null; }
		}

		protected override ZString GetConsolReference()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.ConsolReference : ZString.Empty;
		}

		protected override AddressWrapperCollection GetTransportAddresses()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.TransportAddresses : new AddressWrapperCollection(Factory);
		}

		protected override ZString GetQuoteNumber()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.QuoteNumber : ZString.Empty;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FullHandlingInstructions : ZString.Empty;
		}

		protected override ZString GetFullCartageInstructions()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.FullCartageInstructions : ZString.Empty;
		}

		#endregion

		#region Wrapper Properties

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			var parentWrapper = ParentJob;
			return parentWrapper != null ? parentWrapper.WarehouseJob : null;
		}

		protected override PackageWrapperCollection GetPackages()
		{
			if (packages == null)
			{
				if (packageOverride != null)
				{
					packages = new PackageWrapperCollection(Factory);
					var packageWrapper = new PackageWrapperFromPkgPackage(packageOverride, packedItemsForPackage, Factory, 0, 0);
					packages.Add(packageWrapper);
				}
				else if (packageHeaderOverride != null)
				{
					packages = new PackageWrapperCollection(Factory);
					var packageWrapper = new PackageWrapperFromPkgPackageHeader(PackageJob, packageHeaderOverride, Factory);
					packages.Add(packageWrapper);
				}
				else
				{
					var packageJob = WrappedBO as PkgPackageJob;
					packages = new PackageWrapperCollection(new PkgPackageJob[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);
					packages.Sort("RefNumber", ListSortDirection.Ascending);
				}
			}
			return packages;
		}
		PackageWrapperCollection packages;

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			ZString packUnit = "";
			ZInt packCount = 0;
			foreach (var package in GetAllNonContainerOutersAndFirstLevelPackagesOnContainers())
			{
				packCount += package.KP_PackageQty;

				if (packUnit.IsEmpty)
				{
					packUnit = package.KP_F3_NKPackType;
				}
				else if (package.KP_F3_NKPackType != packUnit)
				{
					packUnit = Constants.PkgUnit.Package; // Combination
				}
			}

			if (packUnit.IsEmpty)
			{
				packUnit = PackingRegistry.Instance.OuterPackageUnit.Value; // Default
			}

			return new PackQTYWrapper(packCount, packUnit, Enterprise.Freight.Common.Business.BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		// replace with PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers after Geoffs Checkin
		List<PkgPackage> GetAllNonContainerOutersAndFirstLevelPackagesOnContainers()
		{
			var result = new List<PkgPackage>();

			if (PackageJob != null)
			{
				foreach (var package in PackageJob.Packages)
				{
					if (package.IsContainer)
					{
						result.AddRange(package.Packages);
					}
					else
					{
						result.Add(package);
					}
				}
			}

			return result;
		}

		protected override ZString GetTransportReference()
		{
			var result = ZString.Empty;

			if (packageOverride != null)
			{
				result = packageOverride.KP_TransportRef;
				if (result.IsEmpty && PackingParentWrapper != null)
				{
					result = PackingParentWrapper.GetTransportReference(packageOverride);
				}
			}

			if (result.IsEmpty && ParentJob != null)
			{
				result = ParentJob.TransportReference;
			}

			return result;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			var result = ZDateTime.Empty;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetDeliveryRequiredBy(packageOverride);
			}
			else if (ParentJob != null)
			{
				result = ParentJob.DeliveryRequiredBy;
			}

			return result;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return ParentJob != null ? ParentJob.Consignee : new OrganisationWrapper(OrganisationUsageType.Consignee, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			AddressWrapper result;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetDeliveryAddress(packageOverride);
			}
			else
			{
				result = ParentJob != null ? ParentJob.DeliveryAddress : AddressWrapper.Empty(Factory);
			}

			return result;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			AddressWrapper result;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetPickupAddress(packageOverride);
			}
			else
			{
				result = ParentJob != null ? ParentJob.PickupAddress : AddressWrapper.Empty(Factory);
			}

			return result;
		}

		protected override ZString GetCustomerReference()
		{
			var result = ZString.Empty;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetCustomerReference(packageOverride);
			}

			// fallback to parent if empty
			if (result.IsEmpty && ParentJob != null)
			{
				result = ParentJob.CustomerReference;
			}

			return result;
		}

		protected override ZString GetOwnerReference()
		{
			var result = ZString.Empty;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetOwnerReference(packageOverride);
			}

			// fallback to parent if empty
			if (result.IsEmpty && ParentJob != null)
			{
				result = ParentJob.OwnerReference;
			}

			return result;
		}

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			var result = CarrierServiceLevelWrapper.Empty;

			if (packageOverride != null && PackingParentWrapper != null)
			{
				result = PackingParentWrapper.GetCarrierServiceLevel(packageOverride);
			}
			else if (ParentJob != null)
			{
				result = ParentJob.CarrierServiceLevel;
			}

			return result;
		}

		protected override ZInt GetDocumentNumber() => documentNumber;

		protected override ZInt GetDocumentTotal() => documentTotal;

		protected override ZInt GetUOMTypeNumber() => uomTypeNumber;

		protected override ZInt GetUOMTypeTotal() => uomTypeTotal;

		#endregion

		#region IDocTypeCode

		ZString IDocTypeCode.DocTypeCode
		{
			get { return docTypeCode; }
			set { docTypeCode = value; }
		}
		ZString docTypeCode;

		#endregion

		#region IPackageOverrider Members

		void IPackageOverrider.SetPackageOverride(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems, int docNumber, int docTotal, int uomTypeNumber, int uomTypeTotal)
		{
			this.packageOverride = package;
			this.packedItemsForPackage = packedItems;
			this.documentNumber = docNumber;
			this.documentTotal = docTotal;
			this.uomTypeNumber = uomTypeNumber;
			this.uomTypeTotal = uomTypeTotal;
		}

		void IPackageOverrider.SetPackageCollectionOverride(PkgPackage[] packages, PkgPackageHeader[] packageHeaders)
		{
			if (packages != null && packages.Any())
			{
				throw new ArgumentException("The parameter packages is not supported, try call SetPackageOverride method");
			}

			if (packageHeaders != null && packageHeaders.Length > 0)
			{
				this.packageHeaderOverride = packageHeaders.First();
			}
		}

		ZInt IPackageOverrider.DocumentNumber => GetDocumentNumber();

		ZInt IPackageOverrider.DocumentTotal => GetDocumentTotal();

		PkgPackage packageOverride;
		PkgPackageHeader packageHeaderOverride;
		PkgPackageItemDivotsWrapper[] packedItemsForPackage;
		ZInt documentNumber;
		ZInt documentTotal;
		ZInt uomTypeNumber;
		ZInt uomTypeTotal;

		#endregion

		protected override ZString GetGoodsHandlingInstructions()
		{
			ZString result = "";
			if (ParentJob != null && ParentJob.WrappedObject is WhsOrder)
			{
				var notes = ((WhsOrder)ParentJob.WrappedObject).Notes;
				if (notes != null)
				{
					result = GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, notes);
				}
			}
			return result;
		}
	}
}
