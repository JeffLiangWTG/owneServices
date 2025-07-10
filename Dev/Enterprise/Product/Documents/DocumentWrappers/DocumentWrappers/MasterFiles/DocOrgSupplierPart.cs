using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgSupplierPart : DocBaseWrapper
	{
		DocOrgSupplierPart(OrgSupplierPart orgSupplierPart, BusinessObjectFactory factoryToWrap)
			: base(orgSupplierPart, factoryToWrap)
		{
		}

		public static DocOrgSupplierPart New(OrgSupplierPart orgSupplierPart, BusinessObjectFactory factoryToWrap)
		{
			if (orgSupplierPart == null)
			{
				return null;
			}
			else
			{
				return new DocOrgSupplierPart(orgSupplierPart, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return "";
		}

		OrgSupplierPart OrgSupplierPart
		{
			get { return (OrgSupplierPart)WrappedObject; }
		}

		public ZString Code
		{
			get { return OrgSupplierPart.OP_PartNum; }
		}

		public ZString Description
		{
			get { return OrgSupplierPart.OP_Desc; }
		}

		public ZDecimal Cubic
		{
			get { return OrgSupplierPart.OP_Cubic; }
		}

		public ZString CubicUQ
		{
			get { return OrgSupplierPart.OP_CubicUQ; }
		}

		public ZString CustomAttrib1
		{
			get { return OrgSupplierPart.OP_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return OrgSupplierPart.OP_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return OrgSupplierPart.OP_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return OrgSupplierPart.OP_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return OrgSupplierPart.OP_CustomAttrib5; }
		}

		public ZDateTime CustomDate1
		{
			get { return OrgSupplierPart.OP_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return OrgSupplierPart.OP_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return OrgSupplierPart.OP_CustomDate3; }
		}

		public ZDateTime CustomDate4
		{
			get { return OrgSupplierPart.OP_CustomDate4; }
		}

		public ZDateTime CustomDate5
		{
			get { return OrgSupplierPart.OP_CustomDate5; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return OrgSupplierPart.OP_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal1AsPercent
		{
			get { return CustomDecimal1 / 100; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return OrgSupplierPart.OP_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return OrgSupplierPart.OP_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return OrgSupplierPart.OP_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return OrgSupplierPart.OP_CustomDecimal5; }
		}

		public ZBool CustomFlag1
		{
			get { return OrgSupplierPart.OP_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return OrgSupplierPart.OP_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return OrgSupplierPart.OP_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return OrgSupplierPart.OP_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return OrgSupplierPart.OP_CustomFlag5; }
		}

		public ZString Department
		{
			get { return OrgSupplierPart.OP_Department; }
		}

		public ZDecimal Depth
		{
			get { return OrgSupplierPart.OP_Depth; }
		}

		public ZString Desc
		{
			get { return OrgSupplierPart.OP_Desc; }
		}

		public ZString Division
		{
			get { return OrgSupplierPart.OP_Division; }
		}

		public ZDecimal Height
		{
			get { return OrgSupplierPart.OP_Height; }
		}

		public ZDecimal LastCost
		{
			get { return OrgSupplierPart.OP_LastCost; }
		}

		public ZString MeasureUQ
		{
			get { return OrgSupplierPart.OP_MeasureUQ; }
		}

		public DocOrganisation BuyerOrSupplier
		{
			get
			{
				if (OrgSupplierPart.RelatedOrganisations.Count > 0)
				{
					return DocOrganisation.New(OrgSupplierPart.Factory, OrgSupplierPart.RelatedOrganisations[0].OU_OH);
				}
				else
				{
					return null;
				}
			}
		}

		public ZDecimal OrderMultipleQty
		{
			get { return OrgSupplierPart.OP_OrderMultipleQty; }
		}

		public ZString OrderMultipleUnit
		{
			get { return OrgSupplierPart.OP_OrderMultipleUnit; }
		}

		public ZString PartNum
		{
			get { return OrgSupplierPart.OP_PartNum; }
		}

		public ZDecimal QtyInStock
		{
			get { return OrgSupplierPart.OP_QtyInStock; }
		}

		public ZString StockKeepingUnit
		{
			get { return OrgSupplierPart.OP_StockKeepingUnit; }
		}

		public ZDecimal VendorPackQty
		{
			get { return OrgSupplierPart.OP_VendorPackQty; }
		}

		public ZString VendorPackUnit
		{
			get { return OrgSupplierPart.OP_F3_NKPackType; }
		}

		public ZDecimal Weight
		{
			get { return OrgSupplierPart.OP_Weight; }
		}

		public ZDecimal WeightedCost
		{
			get { return OrgSupplierPart.OP_WeightedCost; }
		}

		public ZString WeightUQ
		{
			get { return OrgSupplierPart.OP_WeightUQ; }
		}

		public ZDecimal Width
		{
			get { return OrgSupplierPart.OP_Width; }
		}

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return PartNum; }
		}

		#endregion
	}
}
