using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class APInvoiceChargesApprovalRequestChargeDetailsCollection : NonPersistentBusinessObjectCollection<APInvoiceChargesApprovalRequestChargeDetails>
	{
		[Obsolete("For serializer only")]
		public APInvoiceChargesApprovalRequestChargeDetailsCollection()
			: base(new BusinessObjectFactory())
		{
		}

		public APInvoiceChargesApprovalRequestChargeDetailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new APInvoiceChargesApprovalRequestChargeDetails(Factory);
		}
	}
}
