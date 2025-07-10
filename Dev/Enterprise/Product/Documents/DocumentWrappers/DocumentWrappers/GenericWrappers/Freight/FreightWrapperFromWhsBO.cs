using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsBO : FreightWrapper, IDocTypeCode, IFreightWrapperFromWhsBO, IPackingParentWrapper, IWhsDocumentInventory
	{
		public FreightWrapperFromWhsBO(BusinessObject whsBO, BusinessObjectFactory factory)
			: base(whsBO, factory)
		{
		}

		#region Common Parent Property Overrides

		protected override ZString GetOrderNumbersWithOwnersReference()
		{
			return WarehouseJob != null ? WarehouseJob.SecondaryReference.Value : ZString.Empty;
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			return WarehouseJob != null ? new CodeAndDescriptionWrapper(WarehouseJob.JobType, Shipment_Type_List, Factory) : null;
		}

		protected override ZString GetTransportReference()
		{
			return WarehouseJob != null ? WarehouseJob.TransportReference.Value : ZString.Empty;
		}

		protected override ZString GetCustomerReference()
		{
			return WarehouseJob != null ? WarehouseJob.CustomerReference.Value : ZString.Empty;
		}

		protected override ZString GetMasterBill()
		{
			return WarehouseJob != null ? WarehouseJob.MasterBill : ZString.Empty;
		}

		protected override ZString GetMasterBillHeading()
		{
			return WarehouseJob != null ? WarehouseJob.MasterBillHeading : ZString.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			return WarehouseJob != null ? WarehouseJob.JobNumberHeading : ZString.Empty;
		}

		protected override ZString GetJobNumber()
		{
			return WarehouseJob != null ? WarehouseJob.JobNumber : ZString.Empty;
		}

		protected override ZString GetSecondaryHeading()
		{
			return WarehouseJob != null ? WarehouseJob.SecondaryHeading : ZString.Empty;
		}

		protected override ZString GetSecondaryNumber()
		{
			return WarehouseJob != null ? WarehouseJob.SecondaryNumber : ZString.Empty;
		}

		protected override ZString GetHouseBill()
		{
			return WarehouseJob != null ? WarehouseJob.HouseBill : ZString.Empty;
		}

		protected override ZString GetHouseBillHeading()
		{
			return WarehouseJob != null ? WarehouseJob.HouseBillHeading : ZString.Empty;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return WarehouseJob != null ? WarehouseJob.RequiredDate.ValueAsDate : ZDateTime.Empty;
		}

		protected override ZString GetShippersReference()
		{
			return OrderNumbersWithOwnersReference;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return WarehouseJob.HandlingInstructions.Value;
		}

		protected override ZString GetOtherReferences()
		{
			return WarehouseJob != null ? WarehouseJob.OtherReferences : ZString.Empty;
		}

		protected override ZString GetVendorID()
		{
			return WarehouseJob != null ? WarehouseJob.VendorID : ZString.Empty;
		}

		protected override SupplierBuyerLinkWrapper GetSupplierBuyerLink()
		{
			return WarehouseJob != null ? WarehouseJob.SupplierBuyerLink : null;
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return WarehouseJob != null ? WarehouseJob.Consignor : null;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return WarehouseJob != null ? WarehouseJob.Consignee : null;
		}

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			PackQTYWrapper result = PackQTYWrapper.Empty;

			if (WarehouseJob.PackagesSent != null)
			{
				if (WarehouseJob.PalletsSent > 0)
				{
					var packagesSent = WarehouseJob.PackagesSent;
					result = new PackQTYWrapper(packagesSent.Value.ToZInt(), packagesSent.Unit.Code, packagesSent.Unit.List, Factory);
				}
				else if (WarehouseJob.PackagesSent.Value > 0)
				{
					result = new PackQTYWrapper(WarehouseJob.UnitsSent.ToZInt(), Constants.PkgUnit.Package,
						WarehouseJob.PackagesSent.Unit.List, Factory);
				}
				else
				{
					result = new PackQTYWrapper(0, Constants.PkgUnit.Package, WarehouseJob.PackagesSent.Unit.List, Factory);
				}
			}

			return result;
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return WarehouseJob != null ? WarehouseJob.ServiceLevel : null;
		}

		protected override OrgCarrierAccountWrapper GetOrgCarrierAccount()
		{
			return WarehouseJob != null ? WarehouseJob.CarrierAccount : null;
		}

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			return WarehouseJob != null ? WarehouseJob.CarrierServiceLevel : null;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return WarehouseJob != null && WarehouseJob.IncoTerm != null ? new IncoTermWrapper(WarehouseJob.IncoTerm.Code, WarehouseJob.IncoTerm.List, Factory) : null;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return WarehouseJob != null ? WarehouseJob.TransportCompany : null;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return WarehouseJob != null ? WarehouseJob.ConsigneeAddress : null;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return WarehouseJob != null && WarehouseJob.Consignor != null ? WarehouseJob.Consignor.MainAddress : null;
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return WarehouseJob != null ? WarehouseJob.TransportCompany : null;
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(WrappedBO as WhsDocket, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return WarehouseJob != null ? WarehouseJob.Containers : null;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return WarehouseJob != null ? WarehouseJob.Services : null;
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return (WarehouseJob != null) ? WarehouseJob.Packages : new PackageWrapperCollection(Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return (WarehouseJob != null) ? WarehouseJob.UNDGs : new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override ZBool GetHazardous() => WarehouseJob?.UNDGs.Count > 0;

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (fDocManagerBarcode == null)
				{
					BarcodeGenerator generator = new BarcodeGenerator();
					fDocManagerBarcode = generator.CreateDocumentBarcode(DocManagerCode, DocManagerUniqueID, ((IDocTypeCode)this).DocTypeCode + ";");
				}
				return fDocManagerBarcode;
			}
		}
		TextBarcode fDocManagerBarcode;

		protected override ZString GetTransportZoneCore()
		{
			return (WarehouseJob != null) ? WarehouseJob.TransportZone : base.GetTransportZoneCore();
		}

		protected override ZBool GetIsAuthorisedToLeave() => WarehouseJob?.IsAuthorisedToLeave ?? ZBool.False;

		#endregion

		#region Warehouse Info

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			if (WrappedBO != null && warehouseJob == null)
			{
				switch (docTypeCode)
				{
					case "WPA":
						if (WrappedBO is WhsPickableDocket)
						{
							warehouseJob = new WarehousePackingSlipWrapper((WhsPickableDocket)WrappedBO, Factory);
						}
						break;

					case "INV":
						if (WrappedBO is WhsPickableDocket)
						{
							warehouseJob = WarehouseJobGenericWrapper.New(WrappedBO, Factory);
						}
						break;

					case "PPL":
						warehouseJob = new WarehouseOrderWrapper(WrappedBO as WhsOrder, Factory);
						break;

					case "WCA":
						if (WrappedBO is WhsOrder)
						{
							warehouseJob = new WarehouseCartageAdviceWrapper((WhsOrder)WrappedBO, Factory);
						}
						break;

					case "WNP":
						if (WrappedBO is WhsPick)
						{
							warehouseJob = new WarehousePickNonPickedItemsWrapper((WhsPick)WrappedBO, Factory);
						}
						break;

					case "WSI":
						if (WrappedBO is WhsPick)
						{
							warehouseJob = new WarehousePickShortfallItemsWrapper((WhsPick)WrappedBO, Factory);
						}
						break;

					case "WPO":
						if (WrappedBO is WhsPick)
						{
							warehouseJob = new WarehousePickOrderSummaryWrapper((WhsPick)WrappedBO, Factory);
						}
						break;

					case "PMP":
					case "PMR":
						var order = GetWhsOrder();
						if (order != null)
						{
							warehouseJob = new WarehouseOrderWrapperForManifest(order, Factory);
						}
						break;

					default:
						if (typeof(WhsOrder).Equals(WrappedObject.GetType()))
						{
							warehouseJob = new WarehouseOrderWrapper(WrappedBO as WhsOrder, Factory);
						}
						else if (typeof(WhsWorkOrder).Equals(WrappedObject.GetType()))
						{
							warehouseJob = new WarehouseWorkOrderWrapper((WhsWorkOrder)WrappedBO, Factory);
						}
						else if (typeof(WhsPickableDocket).Equals(WrappedObject.GetType()))
						{
							warehouseJob = WarehouseJobGenericWrapper.New(WrappedBO, Factory);
						}
						else if (typeof(WhsPick).Equals(WrappedBO.GetType()))
						{
							warehouseJob = new WarehousePickingSlipWrapper((WhsPick)WrappedBO, Factory);
						}
						else if (typeof(WhsReceive).Equals(WrappedBO.GetType()))
						{
							warehouseJob = new WarehouseReceiveWrapper((WhsReceive)WrappedBO, Factory);
						}
						else if (typeof(WhsAdjustment).Equals(WrappedBO.GetType()))
						{
							warehouseJob = new WarehouseAdjustmentConfirmationWrapper((WhsAdjustment)WrappedBO, Factory);
						}
						else if (typeof(WhsTransfer).Equals(WrappedBO.GetType()))
						{
							warehouseJob = new WarehouseTransferWrapper((WhsTransfer)WrappedBO, Factory);
						}
						else if (typeof(WhsStocktake).Equals(WrappedBO.GetType()))
						{
							warehouseJob = new WarehouseStocktakeWrapper((WhsStocktake)WrappedBO, Factory);
						}
						else
						{
							warehouseJob = new WarehouseJobEmptyWrapper(WrappedBO, Factory);
						}
						break;
				}
			}

			if (WhsDocumentInventory != null)
			{
				var iWarehouseJob = warehouseJob as IWhsDocumentInventory;
				if (iWarehouseJob != null)
				{
					iWarehouseJob.SetInventory(WhsDocumentInventory);
				}
			}

			return warehouseJob;
		}

		WarehouseJobGenericWrapper warehouseJob;

		WhsOrder GetWhsOrder()
		{
			var pick = (WrappedBO as WhsPick);
			return pick != null ? pick.CurrentOrder : WrappedBO as WhsOrder;
		}

		#endregion

		#region IDocTypeCode

		ZString IDocTypeCode.DocTypeCode
		{
			get { return docTypeCode; }
			set { docTypeCode = value; }
		}
		ZString docTypeCode;

		#endregion

		#region IPackingParentWrapper

		AddressWrapper IPackingParentWrapper.GetPickupAddress(PkgPackage package)
		{
			return PickupAddress;
		}

		AddressWrapper IPackingParentWrapper.GetDeliveryAddress(PkgPackage package)
		{
			return DeliveryAddress;
		}

		ZDateTime IPackingParentWrapper.GetDeliveryRequiredBy(PkgPackage package)
		{
			return DeliveryRequiredBy;
		}

		ZString IPackingParentWrapper.GetTransportReference(PkgPackage package)
		{
			return TransportReference;
		}

		OrganisationWrapper IPackingParentWrapper.GetTransportCompany(PkgPackage package)
		{
			return DeliveryAgent;
		}

		ZString IPackingParentWrapper.GetCustomerReference(PkgPackage package)
		{
			return CustomerReference;
		}

		ZString IPackingParentWrapper.GetOwnerReference(PkgPackage package)
		{
			return OrderNumbersWithOwnersReference;
		}

		CarrierServiceLevelWrapper IPackingParentWrapper.GetCarrierServiceLevel(PkgPackage package)
		{
			return CarrierServiceLevel;
		}

		#endregion

		#region IWhsDocumentInventory

		void IWhsDocumentInventory.SetInventory(WhsDocumentInventory documentInventory)
		{
			WhsDocumentInventory = documentInventory;
		}

		WhsDocumentInventory WhsDocumentInventory;

		#endregion
	}
}
