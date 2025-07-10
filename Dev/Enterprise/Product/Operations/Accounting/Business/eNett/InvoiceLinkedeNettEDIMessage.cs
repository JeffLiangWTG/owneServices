using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class InvoiceLinkedeNettEDIMessage : LinkedeNettEDIMessage
	{
		public InvoiceLinkedeNettEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type DataAdapterType
		{
			get { return ObjectFactory.GetType<Integration.IeNettInboundInvoiceDataAdapter>(); }
		}
	}
}