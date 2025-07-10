using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	static class DCUniqueInvoiceBuilder
	{
		[ThreadStatic]
		internal static Dictionary<ZString, DCUniqueInvoice> UniqueInvoices;

		internal static void AddNewHeaderOnly(ZString invoiceKey, DecInvoiceHeaderDataRow invoiceHeader)
		{
			if (UniqueInvoices != null && !UniqueInvoices.ContainsKey(invoiceKey))
			{
				UniqueInvoices.Add(invoiceKey, new DCUniqueInvoice(invoiceHeader));
			}
		}

		internal static void AddLineOnly(ZString invoiceKey, DecInvoiceLineDataRow invoiceLine)
		{
			if (UniqueInvoices != null)
			{
				if (!UniqueInvoices.ContainsKey(invoiceKey))
				{
					throw new WCBException(WCBException.WCBExceptionType.NoInvoiceHeaderFound, invoiceKey);
				}

				DCUniqueInvoice uniqueInvoice = UniqueInvoices[invoiceKey];
				if (uniqueInvoice != null)
				{
					uniqueInvoice.InvoiceLineCollection.Add(invoiceLine);
				}
				else
				{
					throw new WCBException(WCBException.WCBExceptionType.NoInvoiceHeaderFound, invoiceKey);
				}
			}
		}
	}
}
