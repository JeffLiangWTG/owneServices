using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsStocktakeLine : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsStocktakeLine(WhsStocktakeLine stocktakeLine, BusinessObjectFactory factoryToWrap)
			: base(stocktakeLine, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsStocktakeLine New(WhsStocktakeLine stocktakeLine, BusinessObjectFactory factoryToWrap)
		{
			return (stocktakeLine == null) ? null : new DocWhsStocktakeLine(stocktakeLine, factoryToWrap);
		}

		#endregion
		#region Properties

		#region ProductCode

		public ZString ProductCode
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductDesc

		public ZString ProductDesc
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Brand : ZString.Empty;
			}
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Model : ZString.Empty;
			}
		}

		#endregion

		public ZString ClientCode
		{
			get { return (StocktakeLine.Client != null) ? StocktakeLine.Client.OH_Code : ZString.Empty; }
		}

		public ZString LocationString
		{
			get { return StocktakeLine.LocationString; }
		}

		public ZString PackingDateWithLabel
		{
			get { return BuildAttribute(StocktakeLine.WU_PackingDate.ToShortDateString(), Res.GetString("f471ee75-eeda-4530-8c16-01ed5928ef45", "Packing Date"), ""); }
		}

		public ZString ExpiryDateWithLabel
		{
			get { return BuildAttribute(StocktakeLine.WU_ExpiryDate.ToShortDateString(), Res.GetString("0dedcc55-94c9-4c4a-b1f6-e12a3635ada0", "Expiry Date"), ""); }
		}

		public ZString PartAttribute1
		{
			get { return StocktakeLine.WU_PartAttrib1; }
		}

		public ZString PartAttribute1Name
		{
			get
			{
				ZString result = ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						result = StocktakeLine.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return result;
			}
		}

		public ZString PartAttribute1WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib1, attributeName, Res.GetString("f8f5fc8c-80e0-4281-bdac-ce46758aeaf2", "Attribute 1"));
			}
		}

		public ZString PartAttribute2WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib2, attributeName, Res.GetString("9d92643f-885e-4245-a41c-dba30982e954", "Attribute 2"));
			}
		}

		public ZString PartAttribute3WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib3, attributeName, Res.GetString("7a5ae716-cc75-440f-8689-55ea49079e46", "Attribute 3"));
			}
		}

		public ZString TrackedSerialNumber => StocktakeLine.WU_SerialNumber;

		public ZString TrackedSerialNumberName => Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number");

		public ZString TrackedSerialWithLabel => BuildAttribute(TrackedSerialNumber, TrackedSerialNumberName, string.Empty);

		public ZDecimal SystemUnits
		{
			get { return StocktakeLine.WU_SystemUnits; }
		}

		public ZDecimal LastCount
		{
			get { return StocktakeLine.CurrentCount; }
		}

		public ZDecimal Variance
		{
			get { return (LastCount - StocktakeLine.WU_SystemUnits); }
		}

		public ZString Status
		{
			get { return StocktakeLine.WU_Status; }
		}

		public ZString PalletID
		{
			get { return StocktakeLine.WU_PalletID; }
		}

		#endregion

		#region Implementation

		WhsStocktakeLine StocktakeLine
		{
			get { return (WhsStocktakeLine)WrappedObject; }
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
