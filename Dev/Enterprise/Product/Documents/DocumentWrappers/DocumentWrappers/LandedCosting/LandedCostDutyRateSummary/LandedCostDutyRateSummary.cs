using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class LandedCostDutyRateSummary : DocBaseWrapper
	{
		LandedCostDutyRateSummary(ZDecimal dutyPercent, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fDutyPercent = dutyPercent;
		}
		readonly ZDecimal fDutyPercent;
		public static LandedCostDutyRateSummary New(ZDecimal dutyPercent, BusinessObjectFactory factory)
		{
			return new LandedCostDutyRateSummary(dutyPercent, factory);
		}

		public void AddHistoryLine(LandedCostHistory historyLine)
		{
			totalLineCostInLocalCurrency += historyLine.TotalCost;
			totalInvoicePriceInOSCurrency += historyLine.LinePriceInInvoiceCurrency;
		}
		Decimal totalLineCostInLocalCurrency;
		ZDecimal totalInvoicePriceInOSCurrency;

		public ZDecimal Factor
		{
			get { return totalInvoicePriceInOSCurrency.IsEmpty ? 0m : (totalLineCostInLocalCurrency / totalInvoicePriceInOSCurrency); }
		}

		public ZDecimal DutyPercent
		{
			get { return fDutyPercent; }
		}

		public ZString DutyPercentString
		{
			get { return DutyPercent.IsEmpty ? Res.GetString("51d839e9-ac6e-454e-aeb3-b1800e14a087", "Free") : DutyPercent.ToString(2) + "%"; }
		}

		public ZDecimal TotalInvoicePriceInOSCurrency
		{
			get { return totalInvoicePriceInOSCurrency; }
		}

		public ZDecimal TotalLineCostInLocalCurrency
		{
			get { return totalLineCostInLocalCurrency; }
		}
	}
}
