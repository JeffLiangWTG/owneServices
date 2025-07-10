using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPackageLabel : DocWhsLabel
	{
		#region Constructors

		DocWhsPackageLabel(WhsLabel label, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
		}

		DocWhsPackageLabel(WhsLabel label, DocWhsPickableDocketLine orderLine, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
			SelectedOrderLine = orderLine;
		}

		#endregion

		#region New

		public new static DocWhsPackageLabel New(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsPackageLabel(label, factoryToWrap) : null);
		}

		public static DocWhsPackageLabel New(WhsLabel label, DocWhsPickableDocketLine orderLine, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsPackageLabel(label, orderLine, factoryToWrap) : null);
		}

		#endregion

		#region Related Business Objects

		public DocWhsPickableDocketLine SelectedOrderLine
		{
			get
			{
				return selectedOrderLine;
			}
			set
			{
				selectedOrderLine = value;
			}
		}

		DocWhsPickableDocketLine selectedOrderLine;

		#endregion

		#region Properties

		#region ZString  Fields

		#region ProductCode

		public ZString ProductCodeBarcode
		{
			get
			{
				var barcode = new TextBarcode(this.ProductCode);
				return barcode.TextAs128sFontString;
			}
		}

		public ZString ProductCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null && SelectedOrderLine.Product != null)
				{
					WhsPickableDocket order = ((WhsPickableDocketLine)SelectedOrderLine.WrappedObject).PickableDocket;
					OrgSupplierPart part = (OrgSupplierPart)SelectedOrderLine.Product.WrappedObject;
					if (part != null && order != null)
					{
						result = part.GetLocalPartNumForWarehouseConsignee(order.Consignee);
					}
					else
					{
						result = SelectedOrderLine.Product.PartNum;
					}
				}
				return result;
			}
		}

		#endregion

		public ZString ProductDesc
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null && SelectedOrderLine.Product != null)
				{
					WhsPickableDocket order = ((WhsPickableDocketLine)SelectedOrderLine.WrappedObject).PickableDocket;
					OrgSupplierPart part = (OrgSupplierPart)SelectedOrderLine.Product.WrappedObject;
					if (part != null && order != null)
					{
						result = part.GetLocalPartDescForWarehouseConsignee(order.Consignee);
					}
					else
					{
						result = SelectedOrderLine.Product.Desc;
					}
				}
				return result;
			}
		}

		public ZString ComponentFlag
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null)
				{
					WhsPickableDocketLine docketLine = (WhsPickableDocketLine)SelectedOrderLine.WrappedObject;
					WhsWorkOrderLine workOrderLine = docketLine as WhsWorkOrderLine;
					if (workOrderLine != null)
					{
						result = Res.GetString("d6c4f292-5211-4bd5-960e-25115eea6831", "BOM Component Part");
					}
				}

				return result;
			}
		}

		public ZString LeftOverAttributes
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null)
				{
					if (!SelectedOrderLine.PartAttribute2.IsEmpty)
					{
						result = SelectedOrderLine.PartAttribute2Name + ": " + SelectedOrderLine.PartAttribute2;
					}

					if (!SelectedOrderLine.PartAttribute3.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = SelectedOrderLine.PartAttribute3Name + ": " + SelectedOrderLine.PartAttribute3;
						}
						else
						{
							result += ", " + SelectedOrderLine.PartAttribute3Name + ": " + SelectedOrderLine.PartAttribute3;
						}
					}
				}
				return result;
			}
		}

		public ZString FirstDate
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null)
				{
					if (!SelectedOrderLine.ExpiryDate.IsEmpty)
					{
						result = Res.GetString("1c9ecefc-81ef-49cd-b4df-010df87116c7", "Expiry Date:");
					}
					else
					{
						if (!SelectedOrderLine.PackingDate.IsEmpty)
						{
							result = Res.GetString("6ec38c13-4800-4fac-a3ac-de36273ad1e1", "Packing Date:");
						}
					}
				}
				return result;
			}
		}

		public ZString SecondDate
		{
			get
			{
				ZString result = ZString.Empty;
				if (SelectedOrderLine != null)
				{
					if (!SelectedOrderLine.ExpiryDate.IsEmpty && !SelectedOrderLine.PackingDate.IsEmpty)
					{
						result = Res.GetString("6ec38c13-4800-4fac-a3ac-de36273ad1e1", "Packing Date:");
					}
				}
				return result;
			}
		}

		#endregion

		#region ZDateTime  Fields

		public ZDateTime ExpiryDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (SelectedOrderLine != null)
				{
					result = SelectedOrderLine.ExpiryDate;
					if (result.IsEmpty)
					{
						result = SelectedOrderLine.PackingDate;
					}
				}
				return result;
			}
		}

		public ZDateTime PackingDateTemporary
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (SelectedOrderLine != null)
				{
					if (!SelectedOrderLine.ExpiryDate.IsEmpty)
					{
						result = SelectedOrderLine.PackingDate;
					}
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
