using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickOrderedInventory : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsPickOrderedInventory(WhsPickOrderedInventory itemToPick, BusinessObjectFactory factoryToWrap)
			: base(itemToPick, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsPickOrderedInventory New(WhsPickOrderedInventory itemToPick, BusinessObjectFactory factoryToWrap)
		{
			return (itemToPick == null) ? null : new DocWhsPickOrderedInventory(itemToPick, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

		WhsPickOrderedInventory ItemToPick
		{
			get { return (WhsPickOrderedInventory)WrappedObject; }
		}

		#endregion

		#region Properties

		#region ZString Fields

		public ZString PartAttribute1
		{
			get { return ItemToPick.PartAttrib1; }
		}

		public ZString PartAttribute1Name
		{
			get
			{
				var attributeName = ZString.Empty;
				if (ItemToPick.Client != null)
				{
					if (ItemToPick.Client.MiscServ != null)
					{
						attributeName = ItemToPick.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return attributeName;
			}
		}

		public ZString PartAttribute2WithLabel
		{
			get
			{
				var attributeName = ZString.Empty;
				if (ItemToPick.Client != null)
				{
					if (ItemToPick.Client.MiscServ != null)
					{
						attributeName = ItemToPick.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(ItemToPick.PartAttrib2, attributeName, Res.GetString("100ca460-b67f-4074-a0c2-d5fbd165bb7e", "Attribute 2"));
			}
		}

		public ZString PartAttribute3WithLabel
		{
			get
			{
				var attributeName = ZString.Empty;
				if (ItemToPick.Client != null)
				{
					if (ItemToPick.Client.MiscServ != null)
					{
						attributeName = ItemToPick.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(ItemToPick.PartAttrib3, attributeName, Res.GetString("8ad157ca-b7d8-438b-8b0d-de172f3ba725", "Attribute 3"));
			}
		}

		public ZString TrackedSerialWithLabel => BuildAttribute(ItemToPick.SerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), string.Empty);

		public ZString UnitsUQ
		{
			get { return ItemToPick.UnitsUQ; }
		}

		public ZString ClientName
		{
			get
			{
				var result = ZString.Empty;
				if (ItemToPick.Client != null)
				{
					result = ItemToPick.Client.OH_FullName;
				}
				return result;
			}
		}

		public ZString ClientCode
		{
			get
			{
				var result = ZString.Empty;
				if (ItemToPick.Client != null)
				{
					result = ItemToPick.Client.OH_Code;
				}
				return result;
			}
		}

		#region ProductCode

		public ZString ProductCode
		{
			get { return ItemToPick.ProductCode; }
		}

		#endregion

		#region ProductDesc

		public ZString ProductDesc
		{
			get { return ItemToPick.ProductDesc; }
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get
			{
				var supplierPart = ItemToPick.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Brand : ZString.Empty;
			}
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get
			{
				var supplierPart = ItemToPick.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Model : ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region ZDecimal Fields

		public ZDecimal QuantityOrdered
		{
			get { return ItemToPick.QuantityOrdered; }
		}

		public ZDecimal QuantityPicked
		{
			get { return ItemToPick.PickLineQuantity; }
		}

		public ZDecimal QuantityShort
		{
			get { return ItemToPick.QuantityShort; }
		}

		#endregion

		#endregion

		#region Implementation

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
