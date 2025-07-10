using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class PaymentLinkedeNettEDIMessage : LinkedeNettEDIMessage
	{
		public PaymentLinkedeNettEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type DataAdapterType
		{
			get { return ObjectFactory.GetType<Integration.IeNettInboundPaymentDataAdapter>(); }
		}
	}
}