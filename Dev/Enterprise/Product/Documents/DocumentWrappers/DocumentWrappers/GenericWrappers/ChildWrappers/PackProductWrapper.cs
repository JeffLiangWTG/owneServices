using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("ProductCode"), WrapperTypeName("PackProductWrapper")]
	public class PackProductWrapper : GenericWrapper
	{
		public PackProductWrapper(PackProduct product, BusinessObjectFactory factory)
			: base(product, factory)
		{
			ProductBO = product ?? factory.GetNull<PackProduct>();
		}

		readonly PackProduct ProductBO;

		#region Related Business Objects

		OrgSupplierPart RelatedPart
		{
			get { return relatedPart ?? (relatedPart = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, ProductCode))); }
		}
		OrgSupplierPart relatedPart;

		OrderLine OrderLine
		{
			get { return orderLine ?? (orderLine = Factory.LoadTop1<OrderLine>(new ZQuery(JobOrderLineSchema.PK, ProductBO.D2_JO))); }
		}
		OrderLine orderLine;

		#endregion

		#region Properties

		public ZString ProductCode
		{
			get { return ProductBO.D2_ProductCode; }
		}

		public ZDecimal ProductQuantity
		{
			get { return ProductBO.D2_ProductQuantity; }
		}

		public ZString ProductQuantityUnit
		{
			get { return ProductBO.D2_ProductUnitOfQty; }
		}

		public ZString ProductDescription
		{
			get { return RelatedPart != null ? RelatedPart.OP_Desc : ZString.Empty; }
		}

		public ZString ProductOrderLineNo
		{
			get { return OrderLine != null ? OrderLine.OrderAndOrderLineNumber : ZString.Empty; }
		}

		#region Summary

		public ZString Summary
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ProductCode.IsEmpty)
				{
					result += ProductCode;
				}

				if (!ProductQuantity.IsEmpty)
				{
					result += ", " + ProductQuantity.ToStringTrimZeros();
				}

				if (!ProductQuantityUnit.IsEmpty)
				{
					result += " " + ProductQuantityUnit;
				}

				if (!ProductDescription.IsEmpty)
				{
					result += ", " + ProductDescription;
				}

				if (!ProductOrderLineNo.IsEmpty)
				{
					result += ", " + ProductOrderLineNo;
				}

				return result;
			}
		}

		#endregion

		#endregion
	}
}
