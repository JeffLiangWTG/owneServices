using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class ClientLicenceBillingDiscountCollection : ActiveBusinessObjectCollection<ClientLicenceBillingDiscount>
	{
		public ClientLicenceBillingDiscountCollection(ClientLicenceBilling master)
			: base(master.Factory, master, new ZQuery(), ClientLicenceBillingDiscountSchema.L5_L4)
		{
			Master = master;
			ApplySort(ClientLicenceBillingDiscountSchema.L5_Type.Name, System.ComponentModel.ListSortDirection.Ascending);
		}

		public ClientLicenceBillingDiscountCollection(BusinessObjectFactory factory, ClientLicenceBilling master, ZQuery filter)
			: base(factory, master, filter)
		{
		}

		readonly ClientLicenceBilling Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientLicenceBillingDiscount newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (Master != null)
			{
				newElement.L5_L4 = Master.PK;
			}
		}

		static ClientLicenceBillingDiscount Find(IList<ClientLicenceBillingDiscount> discounts, string systemCode, string discountType, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			return discounts.FirstOrDefault(x => x.L5_SystemCode == systemCode && (x.L5_SubCode == discountSubCode || x.L5_SubCode.IsEmpty) && x.L5_Type == discountType && x.IsDateRangeMatched(startDate, endDate));
		}

		protected override bool AllowNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed; }
		}

		#endregion

		#region Calculation

		public static DiscountCalculation CalculateTransactionalDiscount(List<ClientLicenceBillingDiscount> discounts, IDiscountable discountable, int unitCount, ZDecimal unitPrice, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			DiscountCalculation result = new DiscountCalculation();
			result.Amount = discountable.AmountToDiscount;
			result.UnitCount = unitCount;

			ClientLicenceBillingDiscount commitmentDiscount = Find(discounts, discountable.SystemCode, BillingConstants.DiscountType.Commitment, startDate, endDate, discountSubCode);
			if (commitmentDiscount != null)
			{
				if (unitCount < commitmentDiscount.L5_Units)
				{
					unitCount = commitmentDiscount.L5_Units;
					result.Amount = unitCount * unitPrice;
					result.UnitCount = unitCount;
				}
			}

			int unitCountRemaining = unitCount;
			foreach (ClientLicenceBillingDiscount billingDiscount in GetIncrementalVolumeDiscounts(discounts, discountable.SystemCode, unitCount, startDate, endDate, discountSubCode))
			{
				result.MergeDiscountWith(billingDiscount.CalculateIncrementalVolume(unitCountRemaining - billingDiscount.L5_Units, unitPrice));
				unitCountRemaining = billingDiscount.L5_Units;
			}

			if (commitmentDiscount != null)
			{
				DiscountCalculation commitmentDiscountCalculation = commitmentDiscount.CalculateTransactionalCommitment(result.Amount - result.DiscountAmount);
				result.MergeDiscountWith(commitmentDiscountCalculation);
			}

			ClientLicenceBillingDiscount minimumFeeDiscount = Find(discounts, discountable.SystemCode, BillingConstants.DiscountType.MinimumFee, startDate, endDate, discountSubCode);
			if (minimumFeeDiscount != null)
			{
				DiscountCalculation minimumFeeCalculation = minimumFeeDiscount.CalculateMinimumFee(unitCount, unitPrice);
				result.Amount = minimumFeeCalculation.Amount;
				result.MergeDiscountWith(minimumFeeCalculation);
			}

			ClientLicenceBillingDiscount volumeDiscount = GetVolumeDiscount(discounts, discountable.SystemCode, unitCount, 0, startDate, endDate, discountSubCode);
			if (volumeDiscount != null)
			{
				result.MergeDiscountWith(volumeDiscount.Calculate(result.Amount));
			}

			foreach (ClientLicenceBillingDiscount billingDiscount in GetSpecialDiscounts(discounts, discountable.SystemCode, startDate, endDate, discountSubCode))
			{
				result.MergeDiscountWith(billingDiscount.Calculate(result.Amount));
			}

			return result;
		}

		public DiscountCalculation CalculateTransactionalDiscount(IDiscountable discountable, int unitCount, ZDecimal unitPrice, ZDateTime startDate, ZDateTime endDate)
		{
			return CalculateTransactionalDiscount(this.ToList(), discountable, unitCount, unitPrice, startDate, endDate);
		}

		internal static ClientLicenceBillingDiscount GetVolumeDiscount(IList<ClientLicenceBillingDiscount> discounts, ZString systemCode, ZDecimal amount, ZDecimal licenceUnitsAmount, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			ClientLicenceBillingDiscount result = null;
			ZDecimal breakAmount = 0m;
			foreach (ClientLicenceBillingDiscount billingDiscount in discounts.Where(x => x.L5_SystemCode == systemCode && (x.L5_SubCode == discountSubCode || x.L5_SubCode.IsEmpty)))
			{
				ZDecimal currentAmount = (billingDiscount.L5_BreakUnits == BillingConstants.DiscountBreakUnit.LicenceUnits) ? licenceUnitsAmount : amount;
				if (billingDiscount.IsVolume
					&& billingDiscount.IsDateRangeMatched(startDate, endDate)
					&& currentAmount >= billingDiscount.L5_BreakAmount
					&& billingDiscount.L5_BreakAmount > breakAmount)
				{
					breakAmount = billingDiscount.L5_BreakAmount;
					result = billingDiscount;
				}
			}

			return result;
		}

		internal static ClientLicenceBillingDiscount GetVolumeDiscount(IEnumerable<ClientLicenceBillingDiscount> discounts, ZDecimal amount, ZDecimal licenceUnitsAmount)
		{
			ClientLicenceBillingDiscount result = null;
			ZDecimal breakAmount = 0m;
			foreach (ClientLicenceBillingDiscount billingDiscount in discounts)
			{
				ZDecimal currentAmount = (billingDiscount.L5_BreakUnits == BillingConstants.DiscountBreakUnit.LicenceUnits) ? licenceUnitsAmount : amount;
				if (billingDiscount.IsVolume
					&& currentAmount >= billingDiscount.L5_BreakAmount
					&& billingDiscount.L5_BreakAmount > breakAmount)
				{
					breakAmount = billingDiscount.L5_BreakAmount;
					result = billingDiscount;
				}
			}

			return result;
		}

		static IEnumerable<ClientLicenceBillingDiscount> GetIncrementalVolumeDiscounts(IEnumerable<ClientLicenceBillingDiscount> discounts, ZString systemCode, int unitCount, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			return discounts.Where(x =>
				x.L5_SystemCode == systemCode
				&& (x.L5_SubCode == discountSubCode || x.L5_SubCode.IsEmpty)
				&& x.IsIncrementalVolume
				&& x.IsDateRangeMatched(startDate, endDate)
				&& unitCount > x.L5_Units)
				.OrderByDescending(x => x.L5_Units);
		}

		static IEnumerable<ClientLicenceBillingDiscount> GetSpecialDiscounts(IList<ClientLicenceBillingDiscount> discounts, ZString systemCode, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			return discounts.Where(x => x.L5_SystemCode == systemCode && (x.L5_SubCode == discountSubCode || x.L5_SubCode.IsEmpty) && x.IsSpecial && x.IsDateRangeMatched(startDate, endDate));
		}

		public static DiscountCalculation CalculateTransactionalSurcharge(List<ClientLicenceBillingDiscount> discounts, IDiscountable discountable, ZDecimal amountToAddSurcharge, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			DiscountCalculation result = new DiscountCalculation();
			result.Amount = amountToAddSurcharge;

			foreach (ClientLicenceBillingDiscount surcharge in GetSurcharges(discounts, discountable.SystemCode, startDate, endDate, discountSubCode))
			{
				result.MergeDiscountWith(surcharge.Calculate(result.Amount));
			}

			return result;
		}

		static IEnumerable<ClientLicenceBillingDiscount> GetSurcharges(IList<ClientLicenceBillingDiscount> discounts, ZString systemCode, ZDateTime startDate, ZDateTime endDate, string discountSubCode = "")
		{
			return discounts.Where(x => x.L5_SystemCode == systemCode && (x.L5_SubCode == discountSubCode || x.L5_SubCode.IsEmpty) && x.IsSurcharge && x.IsDateRangeMatched(startDate, endDate));
		}

		#endregion

		#region Update Capped Discount

		public void UpdateCappedDiscount(BusinessObjectFactory factory, ZGuid invoicePK)
		{
			ClientLicenceBillingDiscount cappedDiscount = this.FirstOrDefault(x => x.L5_SystemCode == BillingConstants.BillingSystem.ODM && x.L5_Type == BillingConstants.DiscountType.Capped);
			if (cappedDiscount != null && cappedDiscount.CappedDiscountAmount > 0m)
			{
				cappedDiscount.CreateCappedDiscountUsageLog(factory, invoicePK);
			}
		}

		#endregion

		#region Init Date Range

		public void InitDateRange(ZString systemCode, BusinessObjectFactory factory)
		{
			var discountsToInitDateRange = this.Where(x => x.L5_SystemCode == systemCode && !x.L5_Duration.IsEmpty && x.L5_StartDate.IsEmpty && x.L5_EndDate.IsEmpty);
			if (discountsToInitDateRange.Any())
			{
				foreach (ClientLicenceBillingDiscount discount in discountsToInitDateRange)
				{
					ClientLicenceBillingDiscount discountInAnotherFactory = factory.Load<ClientLicenceBillingDiscount>(discount.PK);
					if (discountInAnotherFactory != null)
					{
						discountInAnotherFactory.InitDateRange();
					}
				}
			}
		}

		#endregion
	}
}

