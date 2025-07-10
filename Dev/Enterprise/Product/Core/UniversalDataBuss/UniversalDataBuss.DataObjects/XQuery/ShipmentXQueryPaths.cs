using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	public class ShipmentXQueryPaths : IShipmentXQueryPaths
	{
		public const string TransportMode = nameof(TransportMode);
		public const string ShipmentType = nameof(ShipmentType);

		public const string PortOfOrigin = nameof(PortOfOrigin);
		public const string PortOfDestination = nameof(PortOfDestination);
		public const string PortOfDischarge = nameof(PortOfDischarge);
		public const string PortOfLoading = nameof(PortOfLoading);

		public const string AWBServiceLevel = nameof(AWBServiceLevel);
		public const string ContainerMode = nameof(ContainerMode);
		public const string OuterPacksPackageType = nameof(OuterPacksPackageType);
		public const string ServiceLevel = nameof(ServiceLevel);

		public const string AdditionalTerms = nameof(AdditionalTerms);
		public const string AgentsReference = nameof(AgentsReference);
		public const string BookingConfirmationReference = nameof(BookingConfirmationReference);
		public const string CoLoadBookingConfirmationReference = nameof(CoLoadBookingConfirmationReference);
		public const string GoodsDescription = nameof(GoodsDescription);
		public const string IsCancelled = nameof(IsCancelled);
		public const string IsDirectBooking = nameof(IsDirectBooking);
		public const string IsForwardRegistered = nameof(IsForwardRegistered);
		public const string ShipmentIncoTerm = nameof(ShipmentIncoTerm);
		public const string ShippedOnBoard = nameof(ShippedOnBoard);
		public const string WayBillNumber = nameof(WayBillNumber);
		public const string WayBillType = nameof(WayBillType);

		public const string ConsignorDocumentaryAddress = nameof(ConsignorDocumentaryAddress);
		public const string ConsignorPickupDeliveryAddress = nameof(ConsignorPickupDeliveryAddress);
		public const string ConsigneeDocumentaryAddress = nameof(ConsigneeDocumentaryAddress);
		public const string ConsigneePickupDeliveryAddress = nameof(ConsigneePickupDeliveryAddress);
		public const string ControllingCustomer = nameof(ControllingCustomer);
		public const string PickupAgent = nameof(PickupAgent);
		public const string DeliveryAgent = nameof(DeliveryAgent);
		public const string ControllingAgent = nameof(ControllingAgent);
		public const string ExportBroker = nameof(ExportBroker);
		public const string ImportBroker = nameof(ImportBroker);
		public const string DepartureCFSAddress = nameof(DepartureCFSAddress);
		public const string PickupLocalCartage = nameof(PickupLocalCartage);
		public const string ShipmentControllingParty = nameof(ShipmentControllingParty);
		public const string ShippingLineAddress = nameof(ShippingLineAddress);

		public const string DeliveryRequiredBy = nameof(DeliveryRequiredBy);
		public const string EstimatedDelivery = nameof(EstimatedDelivery);
		public const string EstimatedPickup = nameof(EstimatedPickup);
		public const string FCLDeliveryEquipmentNeeded = nameof(FCLDeliveryEquipmentNeeded);
		public const string FCLPickupEquipmentNeeded = nameof(FCLPickupEquipmentNeeded);
		public const string InsuranceRequired = nameof(InsuranceRequired);
		public const string PickupRequiredBy = nameof(PickupRequiredBy);

		public static string GetNamespace(string namespaceVersion, string path)
		{
			if (namespaceVersion == UniversalXmlInfo.Namespace_2012_11)
			{
				return GetNamespace_2012_11(path);
			}
			else
			{
				return GetNamespace_2011_11(path);
			}
		}

		string IShipmentXQueryPaths.GetNamespace(string namespaceVersion, string path)
		{
			return GetNamespace(namespaceVersion, path);
		}

		static string GetNamespace_2011_11(string path)
		{
			if (Namespace_2011_11.TryGetValue(path, out string result))
			{
				return result;
			}
			else
			{
				return null;
			}
		}

		static string GetNamespace_2012_11(string path)
		{
			if (Namespace_2012_11.TryGetValue(path, out string result))
			{
				return result;
			}
			else
			{
				return GetNamespace_2011_11(path);
			}
		}

		#region SuppressResourceStringsCheckRegion

		static readonly string rootElementName = typeof(Shipment).GetAttribute<RootElementAttribute>().RootElementName;
		static Dictionary<string, string> Namespace_2011_11
		{
			get
			{
				if (namespace_2011_11 == null)
				{
					namespace_2011_11 = new Dictionary<string, string>();
					namespace_2011_11.Add(AdditionalTerms, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.AdditionalTerms)}");
					namespace_2011_11.Add(AgentsReference, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.AgentsReference)}");
					namespace_2011_11.Add(AWBServiceLevel, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.AWBServiceLevel)}/{nameof(Shipment.AWBServiceLevel.Code)}");
					namespace_2011_11.Add(BookingConfirmationReference, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.BookingConfirmationReference)}");
					namespace_2011_11.Add(CoLoadBookingConfirmationReference, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.CoLoadBookingConfirmationReference)}");
					namespace_2011_11.Add(ContainerMode, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ContainerMode)}/{nameof(Shipment.ContainerMode.Code)}");
					namespace_2011_11.Add(GoodsDescription, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.GoodsDescription)}");
					namespace_2011_11.Add(IsCancelled, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.IsCancelled)}");
					namespace_2011_11.Add(IsDirectBooking, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.IsDirectBooking)}");
					namespace_2011_11.Add(IsForwardRegistered, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.IsForwardRegistered)}");
					namespace_2011_11.Add(OuterPacksPackageType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OuterPacksPackageType)}/{nameof(Shipment.OuterPacksPackageType.Code)}");
					namespace_2011_11.Add(ServiceLevel, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ServiceLevel)}/{nameof(Shipment.ServiceLevel.Code)}");
					namespace_2011_11.Add(ShipmentIncoTerm, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShipmentIncoTerm)}/{nameof(Shipment.ShipmentIncoTerm.Code)}");
					namespace_2011_11.Add(ShippedOnBoard, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShippedOnBoard)}/{nameof(Shipment.ShippedOnBoard.Code)}");
					namespace_2011_11.Add(TransportMode, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.TransportMode)}/{nameof(Shipment.TransportMode.Code)}");
					namespace_2011_11.Add(ShipmentType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShipmentType)}/{nameof(Shipment.ShipmentType.Code)}");
					namespace_2011_11.Add(PortOfOrigin, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfOrigin)}/{nameof(Shipment.PortOfOrigin.Code)}");
					namespace_2011_11.Add(PortOfDestination, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfDestination)}/{nameof(Shipment.PortOfDestination.Code)}");
					namespace_2011_11.Add(PortOfDischarge, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfDischarge)}/{nameof(Shipment.PortOfDischarge.Code)}");
					namespace_2011_11.Add(PortOfLoading, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfLoading)}/{nameof(Shipment.PortOfLoading.Code)}");
					namespace_2011_11.Add(WayBillNumber, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.WayBillNumber)}");
					namespace_2011_11.Add(WayBillType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.WayBillType)}/{nameof(Shipment.WayBillType.Code)}");

					namespace_2011_11.Add(DeliveryRequiredBy, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/DeliveryRequiredBy");
					namespace_2011_11.Add(EstimatedDelivery, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/EstimatedDelivery");
					namespace_2011_11.Add(EstimatedPickup, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/EstimatedPickup");
					namespace_2011_11.Add(FCLDeliveryEquipmentNeeded, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/FCLDeliveryEquipmentNeeded/Code");
					namespace_2011_11.Add(FCLPickupEquipmentNeeded, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/FCLPickupEquipmentNeeded/Code");
					namespace_2011_11.Add(InsuranceRequired, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/InsuranceRequired");
					namespace_2011_11.Add(PickupRequiredBy, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/PickupRequiredBy");

					namespace_2011_11.Add(ConsignorDocumentaryAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ConsignorDocumentaryAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ConsigneeDocumentaryAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ConsigneeDocumentaryAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ConsignorPickupDeliveryAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ConsignorPickupDeliveryAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ConsigneePickupDeliveryAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ConsigneePickupDeliveryAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ControllingCustomer, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ControllingCustomer)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(PickupAgent, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.PickupAgent)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(DeliveryAgent, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.DeliveryAgent)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ControllingAgent, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ControllingAgent)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ExportBroker, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ExportBroker)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ImportBroker, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ImportBroker)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(DepartureCFSAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.DepartureCFSAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(PickupLocalCartage, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"PickupLocalCartage\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ShipmentControllingParty, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"ShipmentControllingParty\"]/{nameof(OrganizationAddress.OrganizationCode)}");
					namespace_2011_11.Add(ShippingLineAddress, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OrganizationAddressCollection)}/{nameof(OrganizationAddress)}[./{nameof(OrganizationAddress.AddressType)}=\"{nameof(DocAddressType.ShippingLineAddress)}\"]/{nameof(OrganizationAddress.OrganizationCode)}");
				}
				return namespace_2011_11;
			}
		}
		[ThreadStatic]
		static Dictionary<string, string> namespace_2011_11;

		static Dictionary<string, string> Namespace_2012_11
		{
			get
			{
				if (namespace_2012_11 == null)
				{
					namespace_2012_11 = new Dictionary<string, string>();
					namespace_2012_11.Add(ContainerMode, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ContainerMode)}");
					namespace_2012_11.Add(TransportMode, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.TransportMode)}");
					namespace_2012_11.Add(ShipmentType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShipmentType)}");
					namespace_2012_11.Add(PortOfOrigin, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfOrigin)}");
					namespace_2012_11.Add(PortOfDestination, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfDestination)}");
					namespace_2012_11.Add(PortOfDischarge, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfDischarge)}");
					namespace_2012_11.Add(PortOfLoading, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.PortOfLoading)}");
					namespace_2012_11.Add(AWBServiceLevel, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.AWBServiceLevel)}");
					namespace_2012_11.Add(OuterPacksPackageType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.OuterPacksPackageType)}");
					namespace_2012_11.Add(ServiceLevel, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ServiceLevel)}");
					namespace_2012_11.Add(ShipmentIncoTerm, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShipmentIncoTerm)}");
					namespace_2012_11.Add(ShippedOnBoard, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.ShippedOnBoard)}");
					namespace_2012_11.Add(WayBillType, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.WayBillType)}");

					namespace_2012_11.Add(FCLDeliveryEquipmentNeeded, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/FCLDeliveryEquipmentNeeded");
					namespace_2012_11.Add(FCLPickupEquipmentNeeded, $"{rootElementName}/{nameof(Shipment)}/{nameof(Shipment.LocalProcessing)}/FCLPickupEquipmentNeeded");
				}
				return namespace_2012_11;
			}
		}
		[ThreadStatic]
		static Dictionary<string, string> namespace_2012_11;

		#endregion
	}
}
