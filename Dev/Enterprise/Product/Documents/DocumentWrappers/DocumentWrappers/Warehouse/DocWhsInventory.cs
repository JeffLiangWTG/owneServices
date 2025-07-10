using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsInventory : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsInventory(WhsDocketLine docketLine, BusinessObjectFactory factoryToWrap)
			: base(docketLine, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsInventory New(WhsDocketLine docketLine, BusinessObjectFactory factoryToWrap)
		{
			return (docketLine == null) ? null : new DocWhsInventory(docketLine, factoryToWrap);
		}

		#endregion

		#region ZString Fields

		public ZString Status
		{
			get { return InventoryDocketLine.WE_CurrentInventoryStatus; }
		}

		public ZString UnitsUQ
		{
			get { return InventoryDocketLine.ProductUQ; }
		}

		public ZString PacksUQ
		{
			get { return InventoryDocketLine.WE_F3_NKPackType; }
		}

		#region ProductCode

		public ZString ProductCode
		{
			get { return InventoryDocketLine.ProductCode; }
		}

		#endregion

		#region ProductDesc

		public ZString ProductDesc
		{
			get
			{
				var supplierPart = InventoryDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get
			{
				var supplierPart = InventoryDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Brand : ZString.Empty;
			}
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get
			{
				var supplierPart = InventoryDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Model : ZString.Empty;
			}
		}

		#endregion

		public ZString PartAttribute1
		{
			get { return InventoryDocketLine.WE_PartAttrib1; }
		}

		public ZString PartAttribute1Name
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (InventoryDocketLine.Docket.Client != null)
				{
					if (InventoryDocketLine.Docket.Client.MiscServ != null)
					{
						attributeName = InventoryDocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return attributeName;
			}
		}

		public ZString PartAttribute2
		{
			get { return InventoryDocketLine.WE_PartAttrib2; }
		}

		public ZString PartAttribute2Name
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (InventoryDocketLine.Docket.Client != null)
				{
					if (InventoryDocketLine.Docket.Client.MiscServ != null)
					{
						attributeName = InventoryDocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return attributeName;
			}
		}

		public ZString PartAttribute3
		{
			get { return InventoryDocketLine.WE_PartAttrib3; }
		}

		public ZString PartAttribute3Name
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (InventoryDocketLine.Docket.Client != null)
				{
					if (InventoryDocketLine.Docket.Client.MiscServ != null)
					{
						attributeName = InventoryDocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return attributeName;
			}
		}

		public ZString PartAttribute2WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (InventoryDocketLine.Docket.Client != null)
				{
					if (InventoryDocketLine.Docket.Client.MiscServ != null)
					{
						attributeName = InventoryDocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(InventoryDocketLine.WE_PartAttrib2, attributeName, Res.GetString("32b768a2-200e-43d6-9612-d849b563a0ba", "Attribute 2"));
			}
		}

		public ZString PartAttribute3WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (InventoryDocketLine.Docket.Client != null)
				{
					if (InventoryDocketLine.Docket.Client.MiscServ != null)
					{
						attributeName = InventoryDocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(InventoryDocketLine.WE_PartAttrib3, attributeName, Res.GetString("fe6e4809-195e-4077-996c-620fbde5e6e4", "Attribute 3"));
			}
		}

		public ZString TrackedSerialNumber => InventoryDocketLine.WE_SerialNumber;

		public ZString TrackedSerialNumberName => ResString.GetMultilingualString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number");

		public ZString TrackedSerialWithLabel => BuildAttribute(InventoryDocketLine.WE_SerialNumber, TrackedSerialNumberName, ((NoResString)""));

		public ZString LocationString
		{
			get
			{
				ZString result = InventoryDocketLine.LocationString;
				if (result.IsEmpty)
				{
					result = ".................";
				}
				return result;
			}
		}

		public ZString PackingDateWithLabel => BuildAttribute(InventoryDocketLine.WE_PackingDate.ToShortDateString(), Res.GetString("8d1f59d0-f048-4c85-bcb4-46b6b7698260", "Packing Date"), "");

		public ZString ExpiryDateWithLabel => BuildAttribute(InventoryDocketLine.WE_ExpiryDate.ToShortDateString(), Res.GetString("56ed6f80-c2e8-47a4-aa4f-fcf33ce8401b", "Expiry Date"), "");

		public ZString VolumeUQ => InventoryDocketLine.SupplierPart?.OP_CubicUQ ?? ZString.Empty;

		public ZString WeightUQ
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryDocketLine.SupplierPart != null)
				{
					result = InventoryDocketLine.SupplierPart.OP_WeightUQ;
				}
				return result;
			}
		}

		public ZString PalletID
		{
			get { return InventoryDocketLine.WE_PalletID; }
		}

		#endregion

		#region ZShort Fields

		public ZShort LocationColumn
		{
			get
			{
				ZShort result = ZShort.Zero;
				if (InventoryDocketLine.Location != null)
				{
					result = InventoryDocketLine.Location.WLV_Column;
				}
				return result;
			}
		}

		public ZShort LocationLevel
		{
			get
			{
				ZShort result = ZShort.Zero;
				if (InventoryDocketLine.Location != null)
				{
					result = InventoryDocketLine.Location.WLV_Level;
				}
				return result;
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime PackingDate
		{
			get { return InventoryDocketLine.WE_PackingDate; }
		}

		public ZDateTime ExpiryDate
		{
			get { return InventoryDocketLine.WE_ExpiryDate; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal Volume
		{
			get
			{
				var supplierPart = InventoryDocketLine.SupplierPart;
				return (ZDecimal)(supplierPart != null ? InventoryDocketLine.WE_TransactionQuantity * supplierPart.OP_Cubic : 0);
			}
		}

		public ZDecimal Weight
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (InventoryDocketLine.SupplierPart != null)
				{
					result = InventoryDocketLine.WE_TransactionQuantity * InventoryDocketLine.SupplierPart.OP_Weight;
				}
				return result;
			}
		}

		public ZDecimal PackQty
		{
			get { return InventoryDocketLine.WE_PackQuantity; }
		}

		public ZDecimal ExpectedReceiptQuantity
		{
			get { return InventoryDocketLine.WE_ClientOrderedUnits; }
		}

		public ZDecimal Units
		{
			get { return InventoryDocketLine.WE_TransactionQuantity; }
		}

		public ZDecimal Pallets
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (InventoryDocketLine.SupplierPart != null)
				{
					if (!InventoryDocketLine.SupplierPart.OP_StockKeepingUnitPerPallet.IsEmpty && InventoryDocketLine.SupplierPart.OP_StockKeepingUnitPerPallet != ZDecimal.Zero)
					{
						result = ZArchitecture.Core.Utilities.Round(InventoryDocketLine.WE_TransactionQuantity / InventoryDocketLine.SupplierPart.OP_StockKeepingUnitPerPallet, 1);
					}
				}
				return result;
			}
		}

		public ZDecimal GroupedReceiveUnits
		{
			get;
			set;
		}

		public ZDecimal GroupedInventoryUnits
		{
			get;
			set;
		}

		#endregion

		#region Customs Fields

		public ZString CustomsEntryKey
		{
			get
			{
				ZString result = CustomsEntryNo + " / " + CustomsEntryLineNo;
				if (result == " / 0")
				{
					result = "";
				}

				return result;
			}
		}

		public ZString CustomsEntryNo
		{
			get { return InventoryDocketLine.CustomsData.WB_EntryKey; }
		}

		public ZShort CustomsEntryLineNo
		{
			get { return InventoryDocketLine.CustomsData.WB_EntryLineNo; }
		}

		public ZString CustomsTariffItem
		{
			get { return InventoryDocketLine.CustomsTariffItem; }
		}

		public ZDateTime CustomsEntryDate
		{
			get { return InventoryDocketLine.CustomsData.WB_EntryDate; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return InventoryDocketLine.CustomsData.WB_CustomsQty; }
		}

		public ZString CustomsQuantityUQ
		{
			get { return InventoryDocketLine.CustomsData.WB_CustomsUnitOfQty; }
		}

		public ZDecimal CustomsVFD
		{
			get { return InventoryDocketLine.CustomsData.WB_ValueForDuty; }
		}

		public ZDecimal CustomsTILV
		{
			get { return InventoryDocketLine.CustomsData.WB_TILV; }
		}

		public ZString CustomsCtryOfOrigin
		{
			get { return InventoryDocketLine.CustomsData.WB_RN_NKCountryOfOrigin; }
		}

		public ZString CustomsAddInfo
		{
			get { return InventoryDocketLine.CustomsData.WB_AddInfo; }
		}

		#endregion

		#region Custom Attributes

		public ZString CustomAttrib1
		{
			get { return InventoryDocketLine.WE_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return InventoryDocketLine.WE_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return InventoryDocketLine.WE_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return InventoryDocketLine.WE_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return InventoryDocketLine.WE_CustomAttrib5; }
		}

		public ZString CustomAttrib6
		{
			get { return InventoryDocketLine.WE_CustomAttrib6; }
		}

		public ZDateTime CustomDate1
		{
			get { return InventoryDocketLine.WE_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return InventoryDocketLine.WE_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return InventoryDocketLine.WE_CustomDate3; }
		}

		public ZDateTime CustomDate4
		{
			get { return InventoryDocketLine.WE_CustomDate4; }
		}

		public ZDateTime CustomDate5
		{
			get { return InventoryDocketLine.WE_CustomDate5; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return InventoryDocketLine.WE_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return InventoryDocketLine.WE_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return InventoryDocketLine.WE_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return InventoryDocketLine.WE_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return InventoryDocketLine.WE_CustomDecimal5; }
		}

		public ZBool CustomFlag1
		{
			get { return InventoryDocketLine.WE_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return InventoryDocketLine.WE_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return InventoryDocketLine.WE_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return InventoryDocketLine.WE_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return InventoryDocketLine.WE_CustomFlag5; }
		}

		public ZString CustomTextBlob1
		{
			get { return InventoryDocketLine.WE_CustomTextBlob1; }
		}

		#endregion

		#region Implementation

		WhsDocketLine InventoryDocketLine
		{
			get { return (WhsDocketLine)WrappedObject; }
		}

		ZString BuildAttribute(ZString attributeValue, ZString attributeName, ZString alternateAttributeName)
		{
			if (attributeName.IsEmpty)
			{
				attributeName = alternateAttributeName;
			}

			return attributeValue.IsEmpty ? ZString.Empty : new ZString(attributeName.ToString() + ": " + attributeValue.ToString());
		}

		#endregion
	}
}
