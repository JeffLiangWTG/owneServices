using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class B2InvoiceHeaderComparer : PropertyComparer
	{
		public B2InvoiceHeaderComparer(Type businessObjectType, string propertyName, ListSortDirection direction)
			: base(businessObjectType, propertyName, direction)
		{
			cache = new Dictionary<ZGuid, ZString>();
		}

		public B2InvoiceHeaderComparer(PropertyDescriptor property, ListSortDirection direction)
			: base(property, direction)
		{
			cache = new Dictionary<ZGuid, ZString>();
		}

		public override int Compare(BusinessObject x, BusinessObject y)
		{
			var invoiceNumber1 = GetOrAddFromCache((JobComInvoiceHeader)x);
			var invoiceNumber2 = GetOrAddFromCache((JobComInvoiceHeader)y);

			return Direction == ListSortDirection.Ascending ? Compare(invoiceNumber1, invoiceNumber2) : Compare(invoiceNumber2, invoiceNumber1);
		}

		ZString GetOrAddFromCache(JobComInvoiceHeader invoiceHeader)
		{
			if (!cache.ContainsKey(invoiceHeader.PK))
			{
				cache.Add(invoiceHeader.PK, invoiceHeader.JZ_InvoiceNumber);
			}

			return cache[invoiceHeader.PK];
		}
		readonly Dictionary<ZGuid, ZString> cache;

		int Compare(ZString invoiceNumber1, ZString invoiceNumber2)
		{
			int result;
			decimal invoiceNo1, invoiceNo2;
			var parseSuccess1 = decimal.TryParse(invoiceNumber1, out invoiceNo1);
			var parseSuccess2 = decimal.TryParse(invoiceNumber2, out invoiceNo2);
			if (parseSuccess1 && parseSuccess2)
			{
				if (invoiceNo1 != invoiceNo2)
				{
					result = invoiceNo1.CompareTo(invoiceNo2);
				}
				else
				{
					result = invoiceNumber1.Length.CompareTo(invoiceNumber2.Length);
				}
			}
			else if (parseSuccess1)
			{
				result = -1;
			}
			else if (parseSuccess2)
			{
				result = 1;
			}
			else
			{
				result = invoiceNumber1.CompareTo(invoiceNumber2);
			}
			return result;
		}
	}
}
