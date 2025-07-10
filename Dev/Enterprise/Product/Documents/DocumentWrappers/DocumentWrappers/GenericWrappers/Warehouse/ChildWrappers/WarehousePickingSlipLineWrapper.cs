using System;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickingSlipLineWrapper : WarehouseGenericLineWrapper, IPickingSlipLineWrapper
	{
		#region Constructors

		public WarehousePickingSlipLineWrapper(WhsPickLine pickingLineBO, BusinessObjectFactory factory)
			: base(pickingLineBO, factory)
		{
			if (pickingLineBO != null)
			{
				units = pickingLineBO.WZ_Units;
			}
		}

		#endregion

		#region Static

		public static WarehousePickingSlipLineWrapper New(WhsPickLine pickLine, BusinessObjectFactory factoryToWrap)
		{
			return (pickLine == null) ? null : new WarehousePickingSlipLineWrapper(pickLine, factoryToWrap);
		}

		#endregion

		#region Properties

		#region OrgPartRelation

		protected override OrgPartRelation OrgPartRelationCore
		{
			get
			{
				return SupplierPart != null && Client != null
					? SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(Client, OrgPartRelation.RelationshipTypes.Owner)
					: null;
			}
		}

		#endregion

		#region PickArea

		protected override ZString PickAreaCore => IsPickByArea ? Location?.PickingArea?.WA_NameMultilingual ?? "" : string.Empty;

		bool IsPickByArea
		{
			get
			{
				var branchPK = Guid.Empty;
				if (PickingLineBO != null)
				{
					var warehouse = PickingLineBO.Pick?.Warehouse;
					if (warehouse != null && warehouse.WW_GB_RelatedCompanyBranch.IsValid)
					{
						branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
					}
				}
				return WarehouseDataRegistry.Instance.BreakSystemDefinedPickSlipByArea.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty);
			}
		}

		#endregion

		#region PickMethod

		protected override ZString PickMethodCore
		{
			get
			{
				var location = Location;
				return location?.PickMethods.GetDescriptionFromCode(location.WLV_PickMethod) ?? "";
			}
		}

		#endregion

		#region PickGroup

		protected override ZString PickGroupCore
		{
			get
			{
				var result = base.PickGroupCore;

				var orderLine = OrderLine;
				if (orderLine != null)
				{
					result = orderLine.WE_PickGroup.IsEmpty
						? (ZString)Res.GetString("38194e43-a17a-4e7f-8936-6af6aee91f8e", "None")
						: orderLine.PickGroupDescription;
				}

				return result;
			}
		}

		#endregion

		#region PickGroupNumber

		protected override ZShort PickGroupNumberCore
		{
			get
			{
				var result = base.PickGroupNumberCore;

				var orderLine = OrderLine;
				if (orderLine != null)
				{
					// Group by in Documents also sorts, we want pick groups of 0 to be last
					result = orderLine.WE_PickGroup.IsEmpty ? (ZShort)short.MaxValue : orderLine.WE_PickGroup;
				}

				return result;
			}
		}

		#endregion

		#region RowPathSequence

		protected override ZShort RowPathSequenceCore
		{
			get { return Location?.RowPathSequence ?? base.RowPathSequenceCore; }
		}

		#endregion

		#region PickPathSequence

		protected override ZInt PickPathSequenceCore
		{
			get { return Location?.WLV_PickPathSequence ?? base.PickPathSequenceCore; }
		}

		#endregion

		#region ClientName

		protected override ZString ClientNameCore
		{
			get { return Client != null ? Client.OH_FullName : ZString.Empty; }
		}

		#endregion

		#region ClientCode

		protected override ZString ClientCodeCore
		{
			get { return Client != null ? Client.OH_Code : ZString.Empty; }
		}

		#endregion

		#region ProductCodeCore

		protected override ZString ProductCodeCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductDescriptionCore

		protected override ZString ProductDescriptionCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region Product

		protected override ProductWrapper ProductCore
		{
			get { return new ProductWrapper(SupplierPart, Factory); }
		}

		#endregion

		#region PalletID

		protected override ZString PalletIDCore
		{
			get { return InventoryBO?.WE_PalletID ?? ZString.Empty; }
		}

		#endregion

		#region LocationStringCore

		protected override ZString LocationStringCore
		{
			get { return InventoryBO?.LocationString ?? ZString.Empty; }
		}

		#endregion

		#region UnitsUQCore

		protected override ZString UnitsUQCore
		{
			get { return InventoryBO?.ProductUQ ?? ZString.Empty; }
		}

		#endregion

		#region PartAttribute1WithLabelCore

		protected override MultilingualString PartAttribute1WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				if (Client != null)
				{
					attributeName = Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
				}
				return BuildAttribute(PartAttribute1, attributeName, ResString.GetMultilingualString("28143cfd-ffe7-405e-9cc8-eca8099174c5", "Attribute 1"));
			}
		}

		#endregion

		#region PartAttribute2WithLabelCore

		protected override MultilingualString PartAttribute2WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				if (Client != null)
				{
					if (Client.MiscServ != null)
					{
						attributeName = Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute2, attributeName, ResString.GetMultilingualString("ae0ddfa1-fa0e-4f43-82e3-b368a6e603ac", "Attribute 2"));
			}
		}

		#endregion

		#region PartAttribute3WithLabelCore

		protected override MultilingualString PartAttribute3WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				if (Client != null)
				{
					if (Client.MiscServ != null)
					{
						attributeName = Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute3, attributeName, ResString.GetMultilingualString("a2498b0d-3669-40af-a22b-7ad7eb7e2f10", "Attribute 3"));
			}
		}

		#endregion

		#region TrackedSerialWithLabelCore

		protected override ZString TrackedSerialWithLabelCore => BuildAttribute(TrackedSerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), string.Empty);

		#endregion

		#region PackingDateWithLabelCore

		protected override ZString PackingDateWithLabelCore
		{
			get { return BuildAttribute(PackingDate.ToShortDateString(), Res.GetString("ae724006-4987-41be-b07a-5f232f46d6f6", "Packing Date"), ""); }
		}

		#endregion

		#region ExpiryDateWithLabelCore

		protected override ZString ExpiryDateWithLabelCore
		{
			get { return BuildAttribute(ExpiryDate.ToShortDateString(), Res.GetString("56869154-c6a3-4d3e-9089-02b1159bdd7f", "Expiry Date"), ""); }
		}

		#endregion

		#region SupplierProductDesc

		protected override ZString SupplierProductDescCore
		{
			get
			{
				var supplierPart = this.SupplierPart;

				ZString result = ZString.Empty;
				if (supplierPart != null)
				{
					OrgPartRelation partRelation = supplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Client.PK, OrgPartRelation.RelationshipTypes.Owner);
					result = partRelation != null ? partRelation.OU_LocalPartDescription : result;
				}
				return result;
			}
		}

		#endregion

		#region ExtraDetails

		protected override ZString ExtraDetailsCore
		{
			get
			{
				ZString releaseUnitsAndUQ = ReleaseUnitsAndUQ;
				return
						Weight.ToString() + " " + WeightUQ.ToString() + "; " +
						Volume.ToString() + " " + VolumeUQ.ToString() + "; " +
						(releaseUnitsAndUQ.IsEmpty ? "" : releaseUnitsAndUQ + "; ") +
						Pallets.ToString() + " PLT";
			}
		}

		#endregion

		#region Pallets

		protected override ZDecimal PalletsCore
		{
			get
			{
				ZDecimal result = 0m;
				var supplierPart = this.SupplierPart;
				if (supplierPart != null)
				{
					if (!supplierPart.OP_StockKeepingUnitPerPallet.IsEmpty && supplierPart.OP_StockKeepingUnitPerPallet != ZDecimal.Zero)
					{
						result = ZArchitecture.Core.Utilities.Round(Units / supplierPart.OP_StockKeepingUnitPerPallet, 1);
					}
				}
				return result;
			}
		}

		#endregion

		#region Weight

		protected override ZDecimal WeightCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? ZArchitecture.Core.Utilities.Round(Units * supplierPart.OP_Weight, 2) : 0m;
			}
		}

		#endregion

		#region Volume

		protected override ZDecimal VolumeCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? ZArchitecture.Core.Utilities.Round(Units * supplierPart.OP_Cubic, 4) : 0m;
			}
		}

		#endregion

		#region WeightUQ

		protected override ZString WeightUQCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_WeightUQ : ZString.Empty;
			}
		}

		#endregion

		#region VolumeUQ

		protected override ZString VolumeUQCore
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_CubicUQ : ZString.Empty;
			}
		}

		#endregion

		#region UnitsCore

		protected override ZDecimal UnitsCore
		{
			get { return units; }
			set { units = value; }
		}

		ZDecimal units;

		#endregion

		#region Part Attributes

		#region PackingDateCore

		protected override ZDateTime PackingDateCore
		{
			get
			{
				var result = ZDateTime.Empty;
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_PackingDate
						: PickingLineBO.Inventory.WI_PackingDate;
				}
				return result;
			}
		}

		#endregion

		#region ExpiryDateCore

		protected override ZDateTime ExpiryDateCore
		{
			get
			{
				var result = ZDateTime.Empty;
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_ExpiryDate
						: PickingLineBO.Inventory.WI_ExpiryDate;
				}
				return result;
			}
		}

		#endregion

		#region PartAttribute1Core

		protected override ZString PartAttribute1Core
		{
			get
			{
				ZString result = "";
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_PartAttrib1
						: PickingLineBO.Inventory.WI_PartAttrib1;
				}
				return result;
			}
		}

		#endregion

		#region PartAttribute2Core

		protected override ZString PartAttribute2Core
		{
			get
			{
				ZString result = "";
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_PartAttrib2
						: PickingLineBO.Inventory.WI_PartAttrib2;
				}
				return result;
			}
		}

		#endregion

		#region PartAttribute3Core

		protected override ZString PartAttribute3Core
		{
			get
			{
				ZString result = "";
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_PartAttrib3
						: PickingLineBO.Inventory.WI_PartAttrib3;
				}
				return result;
			}
		}

		#endregion

		#region TrackedSerialNumberlCore

		protected override ZString TrackedSerialNumberCore
		{
			get
			{
				ZString result = "";
				if (PickingLineBO != null)
				{
					result = IsAttributeNeutralAndDocumentRollUp
						? PickingLineBO.DocketLine.WE_SerialNumber
						: PickingLineBO.Inventory.WI_SerialNumber;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region ReleaseUnitsAndUQ

		protected override ZString ReleaseUnitsAndUQCore
		{
			get
			{
				ZString result = "";

				if (InventoryBO != null && PickingLineBO != null)
				{
					var supplierPart = this.SupplierPart;
					var product = InventoryBO.Product;
					var orderLine = PickingLineBO.DocketLine;

					if (supplierPart != null && product != null && orderLine != null)
					{
						var docket = orderLine.Docket;
						var productParams = product.GetParamsByWhsAndClient(docket.Warehouse, docket.Client);
						if (productParams != null && !productParams.W3_F3_NKReleasedPackType.IsEmpty)
						{
							var convertedUnits = supplierPart.UnitConverter.Convert(Units, supplierPart.OP_StockKeepingUnit, productParams.W3_F3_NKReleasedPackType);
							var unitOfQty = (convertedUnits > 1) ? Grammar.Instance.Pluralize(productParams.W3_F3_NKReleasedPackType) : productParams.W3_F3_NKReleasedPackType.ToString();

							result = convertedUnits.Round(1) + " " + unitOfQty;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region IsLocationEmptyAfterFinalisingPick

		protected override ZBool IsLocationEmptyAfterPickFinalisationCore
		{
			get { return PickingLineBO != null && ((IEmptyLocationAfterPickFinalisation)PickingLineBO).IsLocationEmptyAfterFinalisingPick; }
		}

		#endregion

		#region ArrivalDateCore

		protected override ZDateTime ArrivalDateCore => InventoryBO?.WE_AdjustmentArrivalDate.ToZDateTime() ?? ZDateTime.Empty;

		#endregion

		#endregion

		#region Implementation

		#region PickingLineBO

		WhsPickLine PickingLineBO
		{
			get { return pickingLineBO ?? (pickingLineBO = (WhsPickLine)WrappedBO); }
		}

		WhsPickLine pickingLineBO;

		#endregion

		#region InventoryBO

		WhsDocketLine InventoryBO
		{
			get { return inventoryBO ?? (inventoryBO = PickingLineBO?.InventoryLineForAvailableInventory); }
		}

		WhsDocketLine inventoryBO;

		#endregion

		WhsLocation Location
		{
			get { return InventoryBO?.Location; }
		}

		WhsOrderLine OrderLine
		{
			get
			{
				WhsOrderLine result = null;

				if (PickingLineBO != null)
				{
					result = PickingLineBO.DocketLine as WhsOrderLine;
				}

				return result;
			}
		}

		OrgHeader Client
		{
			get { return InventoryBO?.Docket?.Client; }
		}

		OrgSupplierPart SupplierPart
		{
			get { return (InventoryBO != null) ? InventoryBO.SupplierPart : null; }
		}

		bool IsAttributeNeutralAndDocumentRollUp
		{
			get
			{
				bool result = false;
				var client = Client;
				var supplierPart = SupplierPart;
				if (client != null && supplierPart != null)
				{
					result = client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(supplierPart);
				}
				return result;
			}
		}

		#endregion
	}
}
