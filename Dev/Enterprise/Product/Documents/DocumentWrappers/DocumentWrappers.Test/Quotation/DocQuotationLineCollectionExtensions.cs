using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	public static class DocQuotationLineCollectionExtensions
	{
		public static List<DocRateLineItem> Find(this DocQuotationLineCollection collection, RateLine line)
		{
			List<DocRateLineItem> result = new List<DocRateLineItem>();

			foreach (DocRateLineItem item in collection)
			{
				if (item.QuotationLine.Master.PK == line.PK)
				{
					result.Add(item);
				}
			}

			return result;
		}

		public static DocRateLineItem FindByDescAndAmount(this DocQuotationLineCollection collection, ZGuid rateLinePK, ZString description, ZString amount)
		{
			DocRateLineItem result = null;

			foreach (DocRateLineItem item in collection)
			{
				if (rateLinePK != ZGuid.Empty)
				{
					if (item.QuotationLine.Master.PK == rateLinePK && item.Description.ToUpper().Contains(description.ToUpper()) && item.Amount.Contains(amount))
					{
						result = item;
					}
					if (result == null && (item.Description.ToUpper().Contains(description.ToUpper()) && item.Amount.Contains(amount)))
					{
						result = item;
					}
				}
				else
				{
					if (item.Description.ToUpper().Contains(description.ToUpper()) && item.Amount.Contains(amount))
					{
						return item;
					}
				}
			}
			return result;
		}
	}
}
