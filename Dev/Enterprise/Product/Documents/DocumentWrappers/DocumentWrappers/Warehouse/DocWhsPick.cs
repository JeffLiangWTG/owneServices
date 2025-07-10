using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPick : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsPick(WhsPick pick, BusinessObjectFactory factoryToWrap)
			: base(pick, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsPick New(WhsPick pick, BusinessObjectFactory factoryToWrap)
		{
			return (pick == null) ? null : new DocWhsPick(pick, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

		#region Collections

		#region Orders

		public DocWhsPickableDocketCollection Orders
		{
			get { return new DocWhsPickableDocketCollection(Pick.Orders, Factory); }
		}

		#endregion

		#region Lines

		public DocWhsPickLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					var helper = new PickingLineWrapperCollectionHelperLegacy(Pick, Factory, isPickByBiggestEnabled: false);
					lines = helper.PickingLines as DocWhsPickLineCollection;
				}
				return lines;
			}
		}

		#endregion

		#region NonPickedItems

		public DocWhsPickOrderedInventoryCollection NonPickedItems
		{
			get
			{
				DocWhsPickOrderedInventoryCollection result = new DocWhsPickOrderedInventoryCollection(Factory);
				foreach (DocWhsPickOrderedInventory docItem in ItemsToPick)
				{
					if (docItem.QuantityPicked == 0)
					{
						result.Add(docItem);
					}
				}
				return result;
			}
		}

		#endregion

		#region ShortfallItems

		public DocWhsPickOrderedInventoryCollection ShortfallItems
		{
			get
			{
				DocWhsPickOrderedInventoryCollection result = new DocWhsPickOrderedInventoryCollection(Factory);
				foreach (DocWhsPickOrderedInventory docItem in ItemsToPick)
				{
					if (docItem.QuantityShort != 0)
					{
						result.Add(docItem);
					}
				}
				return result;
			}
		}

		#endregion

		#region ItemsToPick

		public DocWhsPickOrderedInventoryCollection ItemsToPick
		{
			get { return new DocWhsPickOrderedInventoryCollection(Pick.OrderedInventories, Factory); }
		}

		#endregion

		#endregion

		#region Properties

		public DocDocAddress SOPConsigneeAddress
		{
			get
			{
				DocDocAddress result = null;
				if (IsSOP)
				{
					if (Pick.Orders.Count > 0)
					{
						result = DocDocAddress.New(Pick.Orders[0].ConsigneeDocAddress, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region ZDecimal

		public ZDecimal OrdersCount
		{
			get { return Pick.Orders.Count; }
		}

		public ZDecimal ProductLinesCount
		{
			get
			{
				return Pick.OrderedInventories.Cast<WhsPickOrderedInventory>().DistinctBy(i => i.SupplierPart).Count(v => v.SupplierPart != null);
			}
		}

		#endregion

		#region ZString

		public ZString Method
		{
			get
			{
				ZString result = ZString.Empty;
				switch (Pick.WP_PickOption)
				{
					case WhsPickOption.Codes.Auto:
						result = WhsPickOption.Descriptions.Auto;
						break;

					case WhsPickOption.Codes.Manual:
						result = WhsPickOption.Descriptions.Manual;
						break;

					case WhsPickOption.Codes.ManualWithAutoAllocate:
						result = WhsPickOption.Descriptions.ManualWithAutoAllocate;
						break;
				}
				return result.ToUpper();
			}
		}

		#region DockDoorLocation

		public ZString DockDoorLocationLabel
		{
			get
			{
				var ddl = Pick?.DockDoorLocation;
				return ddl != null ? Res.GetString("bc45e4a8-45f5-4aff-b6ca-6a058c598f71", "Dock Door Location:") : "";
			}
		}

		public ZString DockDoorLocation
		{
			get
			{
				var ddl = Pick?.DockDoorLocation;
				return ddl != null ? ddl.ToLocationString() : ZString.Empty;
			}
		}

		#endregion

		public ZString SOPConsigneeAddressLabel
		{
			get { return IsSOP ? new ZString(Res.GetString("d4863d51-4ae6-450b-9585-e7f02791db81", "Consignee:")) : ZString.Empty; }
		}

		public ZString SOPRequiredDate
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsSOP)
				{
					if (Pick.Orders.Count > 0)
					{
						result = Pick.Orders[0].WD_RequiredDate.ToShortDateString();
					}
				}
				return result;
			}
		}

		public ZString SOPRequiredDateLabel
		{
			get { return IsSOP ? new ZString(Res.GetString("5807ea2e-bd3f-47d3-89d9-68a7e8d8bc20", "Required Date:")) : ZString.Empty; }
		}

		public ZString SOPSpecialInstructionsLabel
		{
			get { return (IsSOP && (SOPSpecialInstructions != "")) ? new ZString(Res.GetString("49085caa-a871-4e44-b3cc-7f86cef39942", "Special Inst:")) : ZString.Empty; }
		}

		public ZString SOPOrderNumLabel
		{
			get { return IsSOP ? new ZString(Res.GetString("8b6872ae-46ba-48d4-99d9-71c2760b5c66", "Order No.:")) : ZString.Empty; }
		}

		public ZString SOPOrderNum
		{
			get { return IsSOP ? Pick.Orders[0].WD_ExternalReference : ZString.Empty; }
		}

		public ZString SOPSpecialInstructions
		{
			get { return IsSOP ? Pick.Orders[0].WD_HandlingInstructions : ZString.Empty; }
		}

		public ZString WarehouseName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Pick.Warehouse != null)
				{
					result = Pick.Warehouse.WW_WarehouseNameMultilingual;
				}
				return result;
			}
		}

		public ZString PickNo
		{
			get { return Pick.WP_PickNo; }
		}

		public ZString BarcodeTextForPickNo
		{
			get
			{
				TextBarcode barcode = new TextBarcode(this.PickNo);
				return barcode.TextAs128sFontString;
			}
		}

		public ZString PickingInstructions
		{
			get { return Pick.PickingInstructions; }
		}

		public ZString SOPTransportCompanyLabel
		{
			get { return IsSOP ? Res.GetString("654039f9-6f68-474f-8e7c-fef25e5a7e84", "Transport Company:") : ""; }
		}

		public ZString SOPTransportCompany
		{
			get
			{
				ZString result = null;
				if (IsSOP)
				{
					JobDocAddress transportCoDocAddress = Pick.Orders[0].TransportCoDocAddress;
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

		public ZString SOPServiceLevelLabel
		{
			get { return IsSOP ? Res.GetString("fde311f1-15d6-496d-9011-c806ef441e0d", "Service Level:") : ""; }
		}

		public ZString SOPServiceLevel
		{
			get { return (IsSOP && Pick.Orders[0].CarrierServiceLevel != null) ? Pick.Orders[0].CarrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual : ZString.Empty; }
		}

		#endregion

		#region AutoPrintFields

		public ZBool AutoPrintPickingSlip
		{
			get { return Pick.Warehouse != null ? Pick.Warehouse.WW_AutoPrintPickingSlip : ZBool.False; }
		}

		public ZBool AutoPrintOrderSummary
		{
			get { return Pick.Warehouse != null ? Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick : ZBool.False; }
		}

		public ZBool AutoPrintNonPickedItems
		{
			get { return Pick.Warehouse != null ? Pick.Warehouse.WW_AutoPrintPickingNonPickedItems : ZBool.False; }
		}

		public ZBool AutoPrintShortfallItems
		{
			get { return Pick.Warehouse != null ? Pick.Warehouse.WW_AutoPrintPickingShortfallItems : ZBool.False; }
		}

		#endregion

		#endregion

		#region Implementation

		bool IsSOP
		{
			get { return (Pick.Orders.Count == 1); }
		}

		WhsPick Pick
		{
			get { return (WhsPick)WrappedObject; }
		}

		DocWhsPickLineCollection lines;

		#endregion
	}
}
