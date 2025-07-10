using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.Business
{
	[ImmutableObject(true)]
	public class eNettEDIMessageTypeDecider : TypeDecider, IeNettEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;
			string messageSubType = row[EDIMessage.Schema.EM_MessageSubType].ToString();

			switch (messageSubType)
			{
				case eNettMessageSubTypeList.Codes.CancelInvoice:
				case eNettMessageSubTypeList.Codes.GetNewInvoices:
					result = typeof(InvoiceLinkedeNettEDIMessage);
					break;
				case eNettMessageSubTypeList.Codes.GetNewPayments:
					result = typeof(PaymentLinkedeNettEDIMessage);
					break;
				default:
					result = typeof(eNettEDIMessage);
					break;
			}
			return result;
		}

		public override Type GetTypeForNew()
		{
			return typeof(eNettEDIMessage);
		}
	}
}
