using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickOrderedInventoryWrapper : WarehouseGenericLineWrapper
	{
		public WarehousePickOrderedInventoryWrapper(WhsPickOrderedInventory orderedInventory, BusinessObjectFactory factoryToWrap, ZDecimal? unitOrderedOverride = null, ZDecimal? unitsPickedOverride = null)
			: base(orderedInventory, factoryToWrap)
		{
			UnitOrderedOverride = unitOrderedOverride;
			UnitsPickedOverride = unitsPickedOverride;
		}

		#region New

		public static WarehousePickOrderedInventoryWrapper New(WhsPickOrderedInventory orderedInventory, BusinessObjectFactory factoryToWrap)
		{
			return (orderedInventory == null) ? null : new WarehousePickOrderedInventoryWrapper(orderedInventory, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

		WhsPickOrderedInventory OrderedInventory => (WhsPickOrderedInventory)WrappedObject;

		#endregion

		#region Properties

		#region PartAttribute1

		protected override ZString PartAttribute1Core => (OrderedInventory == null) ? ZString.Empty : OrderedInventory.PartAttrib1;

		#endregion

		#region PartAttribute1Name

		protected override MultilingualString PartAttribute1NameCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (OrderedInventory != null && OrderedInventory.Client != null)
				{
					if (OrderedInventory.Client.MiscServ != null)
					{
						attributeName = OrderedInventory.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return attributeName;
			}
		}

		#endregion

		#region PartAttribute2WithLabel

		protected override MultilingualString PartAttribute2WithLabelCore
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (OrderedInventory != null)
				{
					MultilingualString attributeName = (NoResString)ZString.Empty;
					if (OrderedInventory.Client != null && OrderedInventory.Client.MiscServ != null)
					{
						attributeName = OrderedInventory.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}

					result = BuildAttribute(OrderedInventory.PartAttrib2, attributeName, ResString.GetMultilingualString("100ca460-b67f-4074-a0c2-d5fbd165bb7e", "Attribute 2"));
				}
				return result;
			}
		}

		#endregion

		#region PartAttribute3WithLabel

		protected override MultilingualString PartAttribute3WithLabelCore
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (OrderedInventory != null)
				{
					MultilingualString attributeName = (NoResString)ZString.Empty;
					if (OrderedInventory.Client != null && OrderedInventory.Client.MiscServ != null)
					{
						attributeName = OrderedInventory.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}

					result = BuildAttribute(OrderedInventory.PartAttrib3, attributeName, ResString.GetMultilingualString("a9ba9e12-6f29-4ac6-b1d1-d47f4f56413c", "Attribute 3"));
				}
				return result;
			}
		}

		#endregion

		#region TrackedSerialNumber

		protected override ZString TrackedSerialNumberCore => (OrderedInventory == null) ? ZString.Empty : OrderedInventory.SerialNumber;

		protected override ZString TrackedSerialWithLabelCore => BuildAttribute(TrackedSerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), string.Empty);

		#endregion

		#region PackingDateWithLabel

		protected override ZString PackingDateWithLabelCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null)
				{
					result = BuildAttribute(OrderedInventory.PackingDate.ToShortDateString(), Res.GetString("58a3e18c-f9df-477c-b2c3-05b6e7009bbd", "Packing Date"), "");
				}
				return result;
			}
		}

		#endregion

		#region ExpiryDateWithLabel

		protected override ZString ExpiryDateWithLabelCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null)
				{
					result = BuildAttribute(OrderedInventory.ExpiryDate.ToShortDateString(), Res.GetString("05a00ddf-67fe-4554-9ce2-52fba06d3fde", "Expiry Date"), "");
				}
				return result;
			}
		}

		#endregion

		#region UnitsUQ

		protected override ZString UnitsUQCore => (OrderedInventory == null) ? ZString.Empty : OrderedInventory.UnitsUQ;

		#endregion

		#region ClientName

		protected override ZString ClientNameCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null && OrderedInventory.Client != null)
				{
					result = OrderedInventory.Client.OH_FullName;
				}

				return result;
			}
		}

		#endregion

		#region ClientCode

		protected override ZString ClientCodeCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null && OrderedInventory.Client != null)
				{
					result = OrderedInventory.Client.OH_Code;
				}
				return result;
			}
		}

		#endregion

		#region ProductCode

		protected override ZString ProductCodeCore => (OrderedInventory == null) ? ZString.Empty : OrderedInventory.ProductCode;

		#endregion

		#region ProductDesc

		protected override ZString ProductDescriptionCore => (OrderedInventory == null) ? ZString.Empty : OrderedInventory.ProductDesc;

		#endregion

		#region ProductBrandName

		protected override ZString ProductBrandNameCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null && OrderedInventory.SupplierPart != null)
				{
					result = OrderedInventory.SupplierPart.OP_Brand;
				}
				return result;
			}
		}

		#endregion

		#region ProductModel

		protected override ZString ProductModelCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrderedInventory != null && OrderedInventory.SupplierPart != null)
				{
					result = OrderedInventory.SupplierPart.OP_Model;
				}
				return result;
			}
		}

		#endregion

		#region UnitsOrdered

		protected override LabelValuePairWrapper UnitsOrderedCore
		{
			get
			{
				LabelValuePairWrapper result = LabelValuePairWrapper.Empty;
				if (OrderedInventory != null)
				{
					result = new LabelValuePairWrapper(OrderedInventory.UnitsUQ, UnitOrderedOverride ?? OrderedInventory.QuantityOrdered, Factory);
				}
				return result;
			}
		}

		readonly ZDecimal? UnitOrderedOverride;

		#endregion

		#region UnitsPicked

		protected override LabelValuePairWrapper UnitsPickedCore
		{
			get
			{
				LabelValuePairWrapper result = LabelValuePairWrapper.Empty;
				if (OrderedInventory != null)
				{
					result = new LabelValuePairWrapper(OrderedInventory.UnitsUQ, UnitsPickedOverride ?? OrderedInventory.PickLineQuantity, Factory);
				}
				return result;
			}
		}

		readonly ZDecimal? UnitsPickedOverride;

		#endregion

		#region UnitsShort

		protected override LabelValuePairWrapper UnitsShortCore
		{
			get
			{
				LabelValuePairWrapper result = LabelValuePairWrapper.Empty;
				if (OrderedInventory != null)
				{
					result = new LabelValuePairWrapper(OrderedInventory.UnitsUQ, OrderedInventory.QuantityShort, Factory);
				}
				return result;
			}
		}

		#endregion

		#endregion

	}
}
