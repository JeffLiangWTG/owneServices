using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers
{
	[DefaultField("Exists")]
	public class DocInvoiceHasChargeCode : GenericWrapper
	{
		DocInvoiceHasChargeCode(InvoiceHasChargeCode invoiceHasChargeCodeObject, BusinessObjectFactory factory)
			: base(invoiceHasChargeCodeObject, factory)
		{
			this.invoiceHasChargeCodeObject = invoiceHasChargeCodeObject;
		}

		public static DocInvoiceHasChargeCode New(InvoiceHasChargeCode invoiceHasChargeCodeObject, BusinessObjectFactory factory)
		{
			return invoiceHasChargeCodeObject != null ? new DocInvoiceHasChargeCode(invoiceHasChargeCodeObject, factory) : null;
		}

		public ZString ChargeCode
		{
			get
			{
				return invoiceHasChargeCodeObject.ChargeCode;
			}
		}

		public ZBool Exists
		{
			get
			{
				return invoiceHasChargeCodeObject.Exists;
			}
		}

		readonly InvoiceHasChargeCode invoiceHasChargeCodeObject;
	}

	public class InvoiceHasChargeCode : NonPersistentBusinessObject
	{
		public InvoiceHasChargeCode()
		{ }

		public InvoiceHasChargeCode(ZString chargeCode, ZBool exists)
		{
			this.ChargeCode = chargeCode;
			this.Exists = exists;
		}

		public ZString ChargeCode
		{
			get; set;
		}

		public ZBool Exists
		{
			get; set;
		}
	}
}
