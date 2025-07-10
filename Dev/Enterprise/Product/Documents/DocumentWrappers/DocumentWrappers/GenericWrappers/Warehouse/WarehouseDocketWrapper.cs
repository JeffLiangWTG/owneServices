using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <remarks>
	/// THIS CLASS IS A STANDARD INTERFACE FOR DOCBUILDER WAREHOUSE DOCUMENTS ONLY.
	/// </remarks>
	public abstract class WarehouseDocketWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		// todo - refactor out and use the strategy constructor below instead.
		public WarehouseDocketWrapper(WhsDocket whsDocket, BusinessObjectFactory factory)
			: base(whsDocket, factory)
		{
		}

		public WarehouseDocketWrapper(WhsPopulateDocketStrategy strategy)
			: base(strategy)
		{
		}

		#endregion

		#region PalletizedInventory

		protected override WarehouseInventoryWrapperCollection GetPalletizedInventory()
		{
			var docket = WrappedBO as WhsDocket;
			return docket != null && docket.InventoryToPrintPalletLabelFor != null ? PalletizedInventory_ForSelectedInventory : PalletizedInventory_IfInventoryIsNotSelected;
		}

		#region PalletizedInventory_ForSelectedInventory

		WarehouseInventoryWrapperCollection PalletizedInventory_ForSelectedInventory
		{
			get
			{
				var result = new WarehouseInventoryWrapperCollection(Factory);
				var docket = WrappedBO as WhsDocket;
				if (docket != null && docket.InventoryToPrintPalletLabelFor != null)
				{
					if (InventoryRequiresPalletLabel(docket.InventoryToPrintPalletLabelFor.InDocketLine))
					{
						var inventoryWrapper = new WarehouseInventoryWrapper(docket.InventoryToPrintPalletLabelFor.InDocketLine, Factory);
						result.Add(inventoryWrapper);
					}
				}
				return result;
			}
		}

		protected ZBool InventoryRequiresPalletLabel(WhsDocketLine docketLine)
		{
			return docketLine != null && !docketLine.WE_PalletID.IsEmpty && docketLine.WE_StockOnHand > 0;
		}

		#endregion

		#region PalletizedInventory_IfInventoryIsNotSelected

		protected virtual WarehouseInventoryWrapperCollection PalletizedInventory_IfInventoryIsNotSelected => new WarehouseInventoryWrapperCollection(Factory);

		#endregion

		#endregion

		#region Properties 

		#region ArrivalDate

		protected override ZDateTime ArrivalDateCore
			=> DocketBO != null ? DocketBO.WD_ArrivalDate.ToZDateTime() : ZDateTime.Empty;

		#endregion

		#region BookingDate

		protected override ZDateTime BookingDateCore => DocketBO != null ? DocketBO.WD_BookingDate.ToZDateTime() : ZDateTime.Empty;

		#endregion

		#region Client

		public override OrganisationWrapper Client
		{
			get
			{
				if (DocketBO != null && client == null)
				{
					client = new OrganisationWrapper(OrganisationUsageType.Client, DocketBO.Client, ContactType.Warehouse, Factory);
				}
				return client;
			}
		}
		OrganisationWrapper client;

		#endregion

		#region Consignor

		public override OrganisationWrapper Consignor
		{
			get
			{
				if (DocketBO != null && consignor == null)
				{
					consignor = new OrganisationWrapper(OrganisationUsageType.Consignor, DocketBO.Client, ContactType.Warehouse, Factory);
				}
				return consignor;
			}
		}
		OrganisationWrapper consignor;

		#endregion

		#region Branch

		protected override DocBranch BranchCore => DocBranch.New(DocketBO.Warehouse?.RelatedCompanyBranch, Factory);

		#endregion

		#region Supplier

		protected override OrganisationWrapper SupplierCore => new OrganisationWrapper(OrganisationUsageType.Supplier, DocketBO.Supplier, ContactType.Warehouse, Factory);

		#endregion

		#region Forwarder

		protected override OrganisationWrapper ForwarderCore => new OrganisationWrapper(OrganisationUsageType.Forwarder, DocketBO.Forwarder, ContactType.Warehouse, Factory);

		#endregion

		#region Containers

		protected override ContainerWrapperCollection NewWarehouseContainerLineWrapperCollection()
		{
			return DocketBO != null ? new ContainerWrapperCollection(DocketBO, Factory) : null;
		}

		#endregion

		#region Custom Fields

		#region Attributes

		protected override ZString GetCustomAttribute1()
		{
			return DocketBO.WD_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return DocketBO.WD_CustomAttrib2;
		}

		protected override ZString GetCustomAttribute3()
		{
			return DocketBO.WD_CustomAttrib3;
		}

		protected override ZString GetCustomAttribute4()
		{
			return DocketBO.WD_CustomAttrib4;
		}

		protected override ZString GetCustomAttribute5()
		{
			return DocketBO.WD_CustomAttrib5;
		}

		#endregion

		#region Dates

		protected override ZDateTime GetCustomDate1()
		{
			return DocketBO.WD_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return DocketBO.WD_CustomDate2;
		}

		#endregion

		#region Decimals

		protected override ZDecimal GetCustomDecimal1()
		{
			return DocketBO.WD_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return DocketBO.WD_CustomDecimal2;
		}

		protected override ZDecimal GetCustomDecimal3()
		{
			return DocketBO.WD_CustomDecimal3;
		}

		protected override ZDecimal GetCustomDecimal4()
		{
			return DocketBO.WD_CustomDecimal4;
		}

		protected override ZDecimal GetCustomDecimal5()
		{
			return DocketBO.WD_CustomDecimal5;
		}

		#endregion

		#region Flags

		protected override ZBool GetCustomFlag1()
		{
			return DocketBO.WD_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return DocketBO.WD_CustomFlag2;
		}

		protected override ZBool GetCustomFlag3()
		{
			return DocketBO.WD_CustomFlag3;
		}

		protected override ZBool GetCustomFlag4()
		{
			return DocketBO.WD_CustomFlag4;
		}

		protected override ZBool GetCustomFlag5()
		{
			return DocketBO.WD_CustomFlag5;
		}

		#endregion

		#endregion

		#region CustomCompanyLogo

		public override Image CustomCompanyLogo
		{
			get
			{
				Image customCompanyLogo = null;

				if (DocketBO != null && DocketBO.Client != null && DocketBO.Client.MiscServ != null && DocketBO.Client.MiscServ.ClientDocumentLogo != null)
				{
					try
					{
						customCompanyLogo = Image.FromStream(new MemoryStream(DocketBO.Client.MiscServ.ClientDocumentLogo));
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException())
						{ throw; }
						customCompanyLogo = CompanyLogo;
					}
				}

				if (customCompanyLogo == null)
				{
					var warehouse = DocketBO.Warehouse;
					if (warehouse != null && warehouse.RelatedCompanyBranch != null)
					{
						customCompanyLogo = SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(CurrentCompany.PK.ToGuid(), warehouse.RelatedCompanyBranch.PK.ToGuid(), Guid.Empty);
					}
				}

				if (customCompanyLogo == null)
				{
					customCompanyLogo = CompanyLogo;
				}
				return customCompanyLogo;
			}
		}

		#endregion

		#region CustomerReference

		public override LabelValuePairWrapper CustomerReference => DocketBO != null ? new LabelValuePairWrapper(Res.GetString("c11d417e-cd48-4ca8-88a0-c17820bdd760", "Customer Ref"), DocketBO.WD_CustomerReference, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region DropMode

		public override CodeAndDescriptionWrapper DropMode => DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_DropMode, DocketBO.Lookups.DropModes, Factory) : null;

		#endregion

		#region FinalisedDate

		protected override LabelValuePairWrapper FinalisedDateCore => (DocketBO != null) ? new LabelValuePairWrapper(Res.GetString("792e7493-d04e-4174-a3e3-973da46d233e", "Finalized Date"), DocketBO.WD_FinalisedDate, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region IsCustomsTransaction

		public override ZBool IsCustomsTransaction => DocketBO != null && DocketBO.IsCustomsTransaction;

		#endregion

		#region JobClient

		public override OrganisationWrapper JobClient => Client;

		#endregion

		#region JobNumber

		protected override ZString JobNumberCore => DocketBO != null ? DocketBO.WD_DocketID : ZString.Empty;

		#endregion

		#region JobNumberHeading

		protected override ZString JobNumberHeadingCore => DocketBO != null ? Res.GetString("4face6ce-f9bb-4e52-b84e-b4518bd530b7", "Job Number") : "";

		#endregion

		#region PrimaryBarcodeText

		public override ZString PrimaryBarcodeText => DocketBO != null ? new TextBarcode(DocketBO.WD_ExternalReference).TextAs128sFontString : ZString.Empty;

		#endregion

		#region CustomerReferenceBarcode

		protected override ZString CustomerReferenceBarcodeCore => DocketBO != null ? new TextBarcode(DocketBO.WD_CustomerReference).TextAs128sFontString : ZString.Empty;

		#endregion

		#region Registry Additional References

		#region OtherReferencesCore

		protected override ZString OtherReferencesCore => GetRegistryReferencesExtended(excludeWayBills: true);

		#endregion

		#region MasterBillHeading

		protected override ZString MasterBillHeadingCore => WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.MasterBill);

		#endregion

		#region HouseBillHeading

		protected override ZString HouseBillHeadingCore => WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.HouseBill);

		#endregion

		#region MasterBill

		protected override ZString MasterBillCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.MasterBill);

		#endregion

		#region HouseBill

		protected override ZString HouseBillCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.HouseBill);

		#endregion

		protected ZString GetReferenceValue(ZString referenceCode)
		{
			ZString result = ZString.Empty;

			if (DocketBO != null)
			{
				var additionalReference = DocketBO.References.Cast<WhsDocketReference>().FirstOrDefault(r => r.WX_RefType == referenceCode);
				result = additionalReference != null ? additionalReference.WX_Reference : ZString.Empty;
			}

			return result;
		}

		#endregion

		#region References

		protected override ZString ReferencesCore
		{
			get
			{
				var builder = new ZStringBuilder();
				if (DocketBO != null)
				{
					foreach (WhsDocketReference reference in DocketBO.References)
					{
						builder.Append(reference.WX_RefType + ": " + reference.WX_Reference);
					}
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region ReferencesExtended

		protected override ZString ReferencesExtendedCore => GetRegistryReferencesExtended(excludeWayBills: false);

		ZString GetRegistryReferencesExtended(bool excludeWayBills)
		{
			var builder = new ZStringBuilder();
			var referenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			int referencesPrinted = 0;

			if (DocketBO != null)
			{
				foreach (WhsDocketReference reference in DocketBO.References)
				{
					if (!excludeWayBills
						|| (reference.WX_RefType != WarehouseAdditionalReferenceTypes.Codes.MasterBill
							&& reference.WX_RefType != WarehouseAdditionalReferenceTypes.Codes.HouseBill))
					{
						builder.Append(string.Format("{0}: {1}", referenceTypes.GetDescriptionFromCode(reference.WX_RefType), reference.WX_Reference));

						referencesPrinted++;

						if (referencesPrinted >= 4)
						{
							break;
						}
					}
				}
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region SecondaryHeading

		protected override ZString SecondaryHeadingCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (DocketBO != null)
				{
					switch (DocketBO.WD_DocketSubType)
					{
						case "ASS":
							result = Res.GetString("9b65409d-5c05-4aea-a19d-cab130bee7c4", "Work Order Assembly Details");
							break;

						case "DIS":
							result = Res.GetString("2545b2cb-88b9-4100-9df0-f232e56d019b", "Work Order Disassembly Details");
							break;

						case "BAK":
							result = Res.GetString("2ab68091-e523-4c52-a597-272dd7816a59", "BACK ORDER Details");
							break;

						case "CUS":
							result = Res.GetString("e8bfd6db-876f-4243-b9ac-b255d6d6d597", "CUSTOMS RELEASE Details");
							break;

						case "ORD":
							result = Res.GetString("baef8c0a-2417-493e-bea0-a6dc6bbcbed5", "ORDER Details");
							break;

						case "REP":
							result = Res.GetString("a673967a-241f-46b0-8242-323426a1d90c", "REPEAT ORDER Details");
							break;

						case "REC":
							result = Res.GetString("60b4ad2d-56ce-452f-ac4f-ee0299250baf", "RECEIVE Details");
							break;

						default:
							result = (Res.GetString("106122ec-0609-44d3-87b9-4c426023faac", "{0} Details", DocketBO.SubTypeDesc)).Trim();
							break;
					}
				}
				return result;
			}
		}

		#endregion

		#region SubTypeDesc

		protected override ZString SubTypeDescCore => DocketBO.SubTypeDesc;

		#endregion

		#region CarrierServiceLevel

		protected override CarrierServiceLevelWrapper CarrierServiceLevelCore => DocketBO != null ? new CarrierServiceLevelWrapper(DocketBO.WD_PL_NKCarrierServiceLevel, DocketBO.Lookups.CarrierServiceLevels, Factory) : null;

		#endregion

		#region ServiceLevel

		protected override CodeAndDescriptionWrapper ServiceLevelCore => DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_RS_NKServiceLevel, DocketBO.Lookups.ServiceLevels, Factory) : null;

		#endregion

		#region Services

		protected override ServiceWrapperCollection NewServiceLineWrapperCollection()
		{
			return new ServiceWrapperCollection(DocketBO.Services, Factory);
		}

		#endregion

		#region SplitNumberCore

		protected override LabelValuePairWrapper SplitNumberCore
		{
			get
			{
				var labelValuePair = LabelValuePairWrapper.Empty;
				if (DocketBO != null && DocketBO.WD_ExternalReferenceSplit != 0)
				{
					labelValuePair = new LabelValuePairWrapper(Res.GetString("78298634-aa94-435c-8b09-3ad804d25982", "Split No"), DocketBO.WD_ExternalReferenceSplit, Factory);
				}
				return labelValuePair;
			}
		}

		#endregion

		#region Status

		public override LabelValuePairWrapper Status => DocketBO != null ? new LabelValuePairWrapper(Res.GetString("65ef8355-3e04-48f8-b8b3-1406e57cf721", "Status"), DocketBO.WD_DocketStatusDescription, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region TransportCompany

		public override OrganisationWrapper TransportCompany
		{
			get
			{
				if (DocketBO != null && transportCo == null)
				{
					transportCo = new OrganisationWrapper(OrganisationUsageType.TransportCo, DocketBO.TransportCo, ContactType.TransportServices, Factory);
				}
				return transportCo;
			}
		}
		OrganisationWrapper transportCo;

		#endregion

		#region TransportCoAddress

		public override AddressWrapper TransportCoAddress => transportCoAddress ?? (transportCoAddress = TransportCoAddressCore);

		protected abstract AddressWrapper TransportCoAddressCore { get; }

		AddressWrapper transportCoAddress;

		#endregion

		#region DocketStatus

		protected override ZString DocketStatusCore
		{
			get { return DocketBO.WD_DocketStatus; }
		}

		#endregion

		#region DocketType

		protected override ZString DocketTypeCore
		{
			get { return DocketBO.WD_DocketType; }
		}

		#endregion

		#region DocketSubType

		protected override ZString DocketSubTypeCore
		{
			get { return DocketBO.WD_DocketSubType; }
		}

		#endregion

		#region TransportReference

		public override LabelValuePairWrapper TransportReference => DocketBO != null ? new LabelValuePairWrapper(Res.GetString("487e3d3a-8d13-4975-9117-7815333f6b2e", "Transport Ref"), DocketBO.WD_TransportReference, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region UNDGsCore

		protected override UNDGSubstanceWrapperCollection UNDGsCore => new UNDGSubstanceWrapperCollection(DocketBO, Factory);

		#endregion

		#region VendorID

		protected override ZString VendorIDCore
		{
			get
			{
				var vendorID = GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.VendorIDCode);
				if (vendorID.IsEmpty && SupplierBuyerLink != null)
				{
					vendorID = SupplierBuyerLink.VendorID;
				}

				return vendorID;
			}
		}

		#endregion

		#region VehicleNumberCore

		protected override ZString VehicleNumberCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.VehicleNumber);

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				if (warehouseWrapper == null && DocketBO != null)
				{
					var warehouse = DocketBO.Warehouse;
					var warehouseTitle = Res.GetString("8afe9ff1-678d-4200-b624-37016a3e1514", "Warehouse");

					warehouseWrapper = warehouse != null
							? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(warehouseTitle, warehouse, Factory))
							: new WarehouseBOWrapper(warehouseTitle, warehouse, Factory);
				}
				return warehouseWrapper;
			}
		}
		WarehouseBOWrapper warehouseWrapper;

		#endregion

		#region WarehouseCompanyLogo

		public override Image WarehouseCompanyLogo
		{
			get
			{
				Image result = null;
				WhsWarehouse warehouse = (DocketBO != null) ? DocketBO.Warehouse : null;
				if (warehouse != null)
				{
					if (warehouse.WarehouseAddress != null && warehouse.WarehouseAddress.Header != null && warehouse.WarehouseAddress.Header.MiscServ.ClientDocumentLogo.Length > 0)
					{
						try
						{
							MemoryStream stream = new MemoryStream(warehouse.WarehouseAddress.Header.MiscServ.ClientDocumentLogo);
							result = Image.FromStream(stream);
						}
						catch (Exception exception)
						{
							if (exception.IsCriticalException())
							{ throw; }
							result = null;
						}
					}
					if (result == null && warehouse.RelatedCompanyBranch != null)
					{
						result = SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(CurrentCompany.PK.ToGuid(), warehouse.RelatedCompanyBranch.PK.ToGuid(), Guid.Empty);
					}
				}
				if (result == null)
				{
					result = CompanyLogo;
				}

				return result;
			}
		}

		#endregion

		#region WarehouseName

		public override LabelValuePairWrapper WarehouseName =>
			new LabelValuePairWrapper(Res.GetString("1d774691-fa67-489e-8c11-c37ee491c332", "Warehouse"),
				DocketBO.Warehouse?.WW_WarehouseNameMultilingual ?? (NoResString)ZString.Empty,
				Factory);

		#endregion

		#region WarehouseNameAndAddress

		protected override MultilingualString WarehouseNameAndAddressCore
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (DocketBO.Warehouse != null)
				{
					result = MultilingualString.Join(System.Environment.NewLine, (NoResString)WarehouseName.Value, (NoResString)DocketBO.Warehouse.WarehouseAddress.AddressFullFormatted);
				}
				return result;
			}
		}

		#endregion

		#region WarehousePhoneAndFax

		protected override ZString WarehousePhoneAndFaxCore
		{
			get
			{
				var result = ZString.Empty;
				var warehouse = DocketBO.Warehouse;
				if (warehouse != null)
				{
					var warehouseAddress = warehouse.WarehouseAddress;
					if (!warehouseAddress.OA_Phone_Formatted.IsEmpty)
					{
						if (!warehouseAddress.OA_Fax_Formatted.IsEmpty)
						{
							result = Res.GetString("0b725185-2021-44d1-88a5-037703e876be", "Tel: {0}   Fax: {1}", warehouseAddress.OA_Phone_Formatted, warehouseAddress.OA_Fax_Formatted);
						}
						else
						{
							result = Res.GetString("9eb0cb5e-38a6-4daa-9648-2c3fbc7d977f", "Tel: {0}", warehouseAddress.OA_Phone_Formatted);
						}
					}
					else
					{
						if (!warehouseAddress.OA_Fax_Formatted.IsEmpty)
						{
							result = Res.GetString("fb225f55-a530-4d8e-b637-bf443c9e53c5", "Fax: {0}", warehouseAddress.OA_Fax_Formatted);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region TotalCubic

		protected override ZDecimal TotalCubicCore => DocketBO.WD_TotalCubic;

		#endregion

		#region TotalWeight

		protected override ZDecimal TotalWeightCore => DocketBO.WD_TotalWeight;

		#endregion

		#region TotalUnits

		protected override ZDecimal TotalUnitsCore => DocketBO.WD_TotalUnits;

		#endregion

		#region TotalPallets

		protected override ZShort TotalPalletsCore => DocketBO.WD_TotalPallets;

		#endregion

		#region WhoCreatedCore

		protected override LabelValuePairWrapper WhoCreatedCore => (DocketBO != null) ? new LabelValuePairWrapper(Res.GetString("05aced14-3647-4657-9ba9-6bf2ecc54763", "Created By"), DocketBO.Logs.CreatedByUserInitials, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region WhoFinalisedCore

		protected override LabelValuePairWrapper WhoFinalisedCore
			=> (DocketBO != null && DocketBO.IsFinalised)
				? new LabelValuePairWrapper(Res.GetString("954c167d-17d4-4b73-88dc-ad94eec24d96", "Finalized By"), UserWhoFinalised, Factory)
				: LabelValuePairWrapper.Empty;

		string UserWhoFinalised => DocketBO.Logs.MostRecentLogByPostedTime(AutoEvents.ItemDocumentJobFinalised)?.SL_GS_NKUser ?? DocketBO.Logs.CreatedByUserInitials;

		#endregion

		#endregion

		#region Implementation

		protected WhsDocket DocketBO => docketBO ?? (docketBO = WrappedObject as WhsDocket);
		WhsDocket docketBO;

		#endregion
	}
}
