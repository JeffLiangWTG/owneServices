using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickingSlipWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WarehousePickingSlipWrapper(WhsPick pick, BusinessObjectFactory factory)
			: base(pick, factory)
		{
		}

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore => Res.GetString("5b597cf0-8e3f-4e50-8186-21db9ab63433", "Pick No");

		protected override ZString JobNumberCore => PickingBO.WP_PickNo;

		public override LabelValuePairWrapper PickNo
		{
			get
			{
				if (PickingBO != null)
				{
					return new LabelValuePairWrapper(Res.GetString("5b597cf0-8e3f-4e50-8186-21db9ab63433", "Pick No"), ((PickingBO.WP_PickNo.ToString() != null) ? PickingBO.WP_PickNo : ZString.Empty), Factory);
				}
				else
				{
					return new LabelValuePairWrapper(Factory);
				}
			}
		}

		public override ZString PrimaryBarcodeText
		{
			get
			{
				TextBarcode barcode = new TextBarcode(this.PickNo.Value);
				return barcode.TextAs128sFontString;
			}
		}

		protected override LabelValuePairWrapper SecondaryReferenceCore => SOPOrderNumber;

		protected override ZString SecondaryHeadingCore => Res.GetString("ef7797d0-55f6-40bc-aa27-8c2ff434825b", "Order Details");

		public override LabelValuePairWrapper RequiredDate => SOPRequiredDate;

		public override LabelValuePairWrapper SOPOrderNumber => new LabelValuePairWrapper(sopOrderNumberLabel, sopOrderNumber, Factory);

		public override LabelValuePairWrapper SOPRequiredDate => new LabelValuePairWrapper(sopRequiredDateLabel, sopRequiredDate, Factory);

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				if (warehouseWrapper == null && PickingBO != null)
				{
					var warehouse = PickingBO.Warehouse;

					warehouseWrapper = warehouse != null
							? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(Res.GetString("1d774691-fa67-489e-8c11-c37ee491c332", "Warehouse"), warehouse, Factory))
							: null;
				}
				return warehouseWrapper;
			}
		}
		WarehouseBOWrapper warehouseWrapper;

		#endregion

		public override LabelValuePairWrapper WarehouseName => new LabelValuePairWrapper(Res.GetString("1d774691-fa67-489e-8c11-c37ee491c332", "Warehouse"), (PickingBO.Warehouse != null) ? PickingBO.Warehouse.WW_WarehouseNameMultilingual : ZString.Empty, Factory);

		public override ZString SOPConsigneeAddressLabel => IsSOP ? new ZString(Res.GetString("977a6dc3-7802-4acb-85fa-1247ffbcedc4", "Consignee")) : ZString.Empty;

		public override DocDocAddress SOPConsigneeAddress => IsSOP ? DocDocAddress.New(PickingBO.Orders[0].ConsigneeDocAddress, Factory) : null;

		public override LabelValuePairWrapper SOPTransportCompany => new LabelValuePairWrapper(sopTransportCompanyLabel, sopTransportCompany, Factory);

		public override LabelValuePairWrapper SOPCarrierServiceLevel => new LabelValuePairWrapper(sopCarrierServiceLevelLabel, sopCarrierServiceLevel, Factory);

		public override LabelValuePairWrapper SOPStagingAreaName
		{
			get
			{
				var ddl = PickingBO?.DockDoorLocation;
				return ddl != null
					? new LabelValuePairWrapper(Res.GetString("1b942d81-1782-4b85-9462-a68f8db86a27", "Dock Door Location"), ddl.ToLocationString(), Factory)
					: LabelValuePairWrapper.Empty;
			}
		}

		public override LabelValuePairWrapper SOPSpecialInstructions => new LabelValuePairWrapper(Res.GetString("70dbf29e-b916-491b-a9ab-411618fb5c4e", "Special Inst."), sopSpecialInstructions, Factory);

		#region SplitNumberCore

		protected override LabelValuePairWrapper SplitNumberCore
		{
			get
			{
				var labelValuePair = LabelValuePairWrapper.Empty;
				if (IsSOP)
				{
					var parentOrder = PickingBO.Orders[0];

					if (parentOrder != null && parentOrder.WD_ExternalReferenceSplit != 0)
					{
						labelValuePair = new LabelValuePairWrapper(Res.GetString("78298634-aa94-435c-8b09-3ad804d25982", "Split No"), parentOrder.WD_ExternalReferenceSplit, Factory);
					}
				}
				return labelValuePair;
			}
		}

		#endregion

		public override LabelValuePairWrapper PickingInstructions => new LabelValuePairWrapper(Res.GetString("a132975f-ce90-47fb-bc9c-3d28e17423c1", "Picking Inst."), (PickingBO.PickingInstructions.ToString() != null) ? PickingBO.PickingInstructions : ZString.Empty, Factory);

		public override WarehousePickableDocketWrapperCollection Orders => new WarehousePickableDocketWrapperCollection(PickingBO.Orders, Factory);

		#region local properties for header

		ZString sopOrderNumberLabel
		{
			get
			{
				var label = ZString.Empty;

				if (IsSOP)
				{
					if (IsWorkOrderPick)
					{
						label = new ZString(Res.GetString("f49b0e00-2487-479a-b6af-7bdbf45e926b", "Work Order No"));
					}
					else
					{
						label = new ZString(Res.GetString("567fab76-bce2-4a73-b5fb-09985ed89e28", "Order No"));
					}
				}

				return label;
			}
		}

		ZString sopOrderNumber => IsSOP ? PickingBO.Orders[0].WD_ExternalReference : ZString.Empty;

		ZString sopRequiredDateLabel => IsSOP ? new ZString(Res.GetString("af9690c8-53d9-4fb7-8298-5488ac99b418", "Order Required Date")) : ZString.Empty;

		ZDateTime sopRequiredDate => IsSOP ? PickingBO.Orders.Cast<WhsDocket>().Single().WD_RequiredDate.ToZDateTime() : ZDateTime.Empty;

		ZString sopTransportCompanyLabel => IsSOP ? new ZString(Res.GetString("1aa883bc-c50e-4e61-a675-408497343b54", "Transport Company")) : ZString.Empty;

		ZString sopTransportCompany
		{
			get
			{
				ZString result = null;
				if (IsSOP)
				{
					JobDocAddress transportCoDocAddress = PickingBO.Orders[0].TransportCoDocAddress;
					if (transportCoDocAddress.E2_AddressOverride)
					{
						result = transportCoDocAddress.E2_CompanyName;
					}
					else if (transportCoDocAddress.Organisation != null)
					{
						result = transportCoDocAddress.Organisation.OH_FullName;
					}
				}
				return result;
			}
		}

		ZString sopCarrierServiceLevelLabel => IsSOP ? new ZString(Res.GetString("WarehousePickingSlipWrapper|SopServiceLevelLabel", "Carrier Service Level")) : ZString.Empty;

		ZString sopCarrierServiceLevel => (IsSOP && PickingBO.Orders[0].CarrierServiceLevel != null) ? PickingBO.Orders[0].CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty;

		ZString sopSpecialInstructions => IsSOP ? PickingBO.Orders[0].WD_HandlingInstructions : ZString.Empty;

		// Is Single Order Pick
		bool IsSOP => PickingBO.Orders.Count == 1;

		#endregion

		#endregion

		#region Lines Generic Wrapper Collection

		protected override WarehousePickingSlipLineWrapperCollection NewWarehousePickingSlipLineWrapperCollection()
		{
			return pickingLines;
		}

		WarehousePickingSlipLineWrapperCollection pickingLines
		{
			get
			{
				if (pickLines == null)
				{
					var helper = new PickingLineWrapperCollectionHelperGeneric(PickingBO, Factory, IsPickByBiggestEnabled);
					pickLines = helper.PickingLines as WarehousePickingSlipLineWrapperCollection;
				}
				return pickLines;
			}
		}
		WarehousePickingSlipLineWrapperCollection pickLines;

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			var packageJobs = new List<PkgPackageJob>();

			if (!PickingBO.Orders.ContainsWorkOrders)
			{
				foreach (WhsOrder order in PickingBO.Orders)
				{
					var packageJob = order.PackageJob;
					if (packageJob != null)
					{
						packageJobs.Add(packageJob);
					}
				}
			}

			var packages = new PackageWrapperCollection(packageJobs, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory);
			packages.Sort("RefNumber", ListSortDirection.Ascending);

			return packages;
		}

		#endregion

		#region GetJobLines

		// When WarehousePickingSlipLineWrapperCollection is changed to inherit from WarehouseGenericWrapperCollection then change this function to return NewWarehousePickingSlipLineWrapperCollection();
		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return new WarehouseEmptyWrapperCollection(Factory);
		}

		#endregion

		#region BOMStagingLocationParts

		public override WarehouseBOMStagingLocationPartWrapperCollection BOMStagingAreaParts
		{
			get
			{
				if (IsWorkOrderPick && bomStagingLocationParts == null)
				{
					var collection = new WarehouseBOMStagingLocationPartCollection(PickingBO, Factory);
					bomStagingLocationParts = new WarehouseBOMStagingLocationPartWrapperCollection(collection, Factory);
				}

				return bomStagingLocationParts;
			}
		}

		WarehouseBOMStagingLocationPartWrapperCollection bomStagingLocationParts;

		#endregion

		#region IsWorkOrderPick

		public override ZBool IsWorkOrderPick => PickingBO.IsWorkOrderPick;

		#endregion

		#region Implementation

		WhsPick PickingBO => pickingBO ?? (pickingBO = (WhsPick)WrappedBO);
		WhsPick pickingBO;

		#endregion

		#region HasMultipleUnits

		protected override ZBool HasMultipleStockKeepingUnitsCore => !SupplierPartsOnDocketLines.AllSame(p => p.OP_StockKeepingUnit.ToString(), StringComparer.OrdinalIgnoreCase);

		protected override ZBool HasMultipleWeightUnitsCore => !SupplierPartsOnDocketLines.AllSame(p => p.OP_WeightUQ.ToString(), StringComparer.OrdinalIgnoreCase);

		protected override ZBool HasMultipleVolumeUnitsCore => !SupplierPartsOnDocketLines.AllSame(p => p.OP_CubicUQ.ToString(), StringComparer.OrdinalIgnoreCase);

		IEnumerable<OrgSupplierPart> SupplierPartsOnDocketLines => PickingBO.Orders.Cast<WhsPickableDocket>().SelectMany(order => order.Lines).Select(dl => dl.SupplierPart).WhereNotNull().Distinct();

		#endregion

		#region HasShortfallItems

		public override ZBool HasShortfallItems => PickingBO.HasShortfallItems;

		#endregion

		#region HasNonPickedItems

		public override ZBool HasNonPickedItems => PickingBO.HasNonAllocatedOrderedInventory;

		#endregion
	}
}
