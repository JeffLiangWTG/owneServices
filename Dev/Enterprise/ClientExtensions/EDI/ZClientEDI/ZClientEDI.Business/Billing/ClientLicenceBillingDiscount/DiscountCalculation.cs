using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DiscountCalculation
	{
		public DiscountCalculation()
		{
			Amount = 0m;
			DiscountAmount = 0m;
			UnitCount = 0;
			InvoiceDescriptions = System.Array.Empty<ZString>();
			ErrorDescriptions = System.Array.Empty<ZString>();
			details = new List<DiscountDetailedInfo>();
		}

		public ZDecimal Amount { get; set; }
		public ZDecimal DiscountAmount { get; set; }
		public ZInt UnitCount { get; set; }

		public IEnumerable<DiscountDetailedInfo> Details => details;
		readonly List<DiscountDetailedInfo> details;

		public void MergeDiscountDetails(DiscountDetailedInfo info)
		{
			details.Add(info);
		}

		public void MergeDiscountDetails(ZString discountDescription, ZString discountType, ZDecimal discountAmount, ZDecimal discountPercent)
		{
			MergeDiscountDetails(new DiscountDetailedInfo(discountDescription, discountType, discountAmount, discountPercent));
		}

		public void SetDiscountDetails(ZString discountDescription, ZString discountType, ZDecimal discountAmount, ZDecimal discountPercent)
		{
			details.Clear();
			MergeDiscountDetails(new DiscountDetailedInfo(discountDescription, discountType, discountAmount, discountPercent));
		}

		public IEnumerable<ZString> DiscountDescriptions
		{
			get
			{
				return Details.Select(x => x.DiscountDescription);
			}
		}
		public IEnumerable<ZString> InvoiceDescriptions { get; set; }
		public IEnumerable<ZString> ErrorDescriptions { get; set; }

		public ZDecimal TotalAmount
		{
			get { return Amount - DiscountAmount; }
		}

		public void MergeDiscountWith(DiscountCalculation anotherCalculation)
		{
			if (anotherCalculation != null)
			{
				DiscountAmount += anotherCalculation.DiscountAmount;
				details.AddRange(anotherCalculation.Details);

				var uniqueInvoiceDescriptions = anotherCalculation.InvoiceDescriptions.Where(x => !InvoiceDescriptions.Contains(x)).ToList();
				uniqueInvoiceDescriptions.AddRange(InvoiceDescriptions);
				InvoiceDescriptions = uniqueInvoiceDescriptions;

				if (anotherCalculation.ErrorDescriptions.Any())
				{
					ErrorDescriptions = ErrorDescriptions.Concat(anotherCalculation.ErrorDescriptions).ToArray();
				}
			}
		}
	}

	public class DiscountDetailedInfo
	{
		public DiscountDetailedInfo(ZString discountDescription, ZString discountType, ZDecimal discountAmount, ZDecimal discountPercent)
		{
			DiscountDescription = discountDescription;
			DiscountType = discountType;
			DiscountAmount = discountAmount;
			DiscountPercent = discountPercent;
		}

		public readonly ZString DiscountDescription;
		public readonly ZString DiscountType;
		public readonly ZDecimal DiscountAmount;
		public readonly ZDecimal DiscountPercent;
	}
}

