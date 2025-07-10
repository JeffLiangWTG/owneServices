using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Summary"), WrapperTypeName("PackageOrderReferenceWrapper")]
	public class PackageOrderReferenceWrapper : GenericWrapper
	{
		public PackageOrderReferenceWrapper(PkgPackageOrderReference packageOrderReference, BusinessObjectFactory factory)
			: base(packageOrderReference, factory)
		{
			PackageOrderReference = packageOrderReference ?? factory.GetNull<PkgPackageOrderReference>();
		}

		readonly PkgPackageOrderReference PackageOrderReference;

		#region Properties

		public ZString BatchNumber
		{
			get { return PackageOrderReference.KPO_BatchNumber; }
		}

		public ZString CommercialInvoiceNumber
		{
			get { return PackageOrderReference.KPO_CommercialInvoiceNumber; }
		}

		public ZDate ExpiryDate
		{
			get { return PackageOrderReference.KPO_ExpiryDate; }
		}

		public ZString LineReference
		{
			get { return PackageOrderReference.KPO_LineReference; }
		}

		public ZString OrderNumber
		{
			get { return PackageOrderReference.KPO_OrderNumber; }
		}

		public ZString SerialNumber
		{
			get { return PackageOrderReference.KPO_SerialNumber; }
		}

		public ZString SKUPartNumber
		{
			get { return PackageOrderReference.KPO_SKUPartNumber; }
		}

		#region Summary

		public ZString Summary
		{
			get
			{
				ZString result = ZString.Empty;

				if (!BatchNumber.IsEmpty)
				{
					result += BatchNumber;
				}

				if (!CommercialInvoiceNumber.IsEmpty)
				{
					result += ", " + CommercialInvoiceNumber;
				}

				if (!ExpiryDate.IsEmpty)
				{
					result += ", " + ExpiryDate.ToString();
				}

				if (!LineReference.IsEmpty)
				{
					result += ", " + LineReference;
				}

				if (!OrderNumber.IsEmpty)
				{
					result += ", " + OrderNumber;
				}

				if (!SerialNumber.IsEmpty)
				{
					result += ", " + SerialNumber;
				}

				if (!SKUPartNumber.IsEmpty)
				{
					result += ", " + SKUPartNumber;
				}

				return result;
			}
		}

		#endregion

		#endregion
	}
}
