using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Fee;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// A ClientLicenceFee plus the ClientChargeableUsage that indicates if it has already been invoiced.
	/// </summary>
	public class FeeUsage
	{
		public FeeUsage(ClientLicenceFee fee, ClientChargeableUsage chargeable = null)
		{
			this.fee = fee;
			this.chargeable = chargeable;
			SetAmounts(fee.L8_Amount, fee.L8_Amount);
		}

		readonly ClientChargeableUsage chargeable;
		readonly ClientLicenceFee fee;

		public string Currency { get { return fee.L8_RX_NKCurrency; } }
		public UsageOwnerDelivery OwnerDelivery { get; set; }

		public ClientLicenceFee Fee { get { return fee; } }
		public ZGuid ChargeableUsagePk { get { return chargeable != null ? chargeable.PK : ZGuid.Empty; } }

		public void CreateOrUpdateChargeableUsageForInvoice(ARInvoice invoice, ZDateTime periodStart)
		{
			FeeBill.CreateOrUpdateChargeableUsage(invoice, fee, ChargeableUsagePk, periodStart, false);
		}

		public ZGuid InvoicePk { get { return (chargeable != null ? chargeable.U1_AH_Invoice : ZGuid.Empty); } }

		public EdiPriceHeaderDiscount PrepayDiscount { get; set; }

		public ZDecimal PreDiscountAmount { get; private set; }
		public ZDecimal PostDiscountAmount { get; private set; }
		public ZDecimal DiscountAmount { get { return PostDiscountAmount - PreDiscountAmount; } }

		public void SetAmounts(decimal preDiscount, decimal postDiscount)
		{
			PreDiscountAmount = preDiscount;
			PostDiscountAmount = postDiscount;
		}
	}

	public class StlFees
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public StlFees(BillingRunContext context, ClientLicenceFee[] feeList)
		{
			fees = feeList;
			var factory = context.Factory;
			var companyDeliveries = context.CompanyDeliveries;
			var feeToChargeableUsage = GetFeeChargeableUsages(context, fees)
				.ToDictionary(x => x.U1_Parent.ToGuid());

			feeUsages = new List<FeeUsage>(fees.Length);
			foreach (var fee in fees)
			{
				ClientChargeableUsage usage;
				feeToChargeableUsage.TryGetValue(fee.PK.ToGuid(), out usage);
				feeUsages.Add(new FeeUsage(fee, usage));
			}

			var feeUsagesByCompany = feeUsages.GroupBy(x => x.Fee.L8_LC.ToGuid());
			var companyPks = fees.Select(x => x.L8_LC.ToGuid()).Distinct();
			companyDeliveries.AddOwnerCompanyPks(companyPks);

			var databasePks = fees.Where(x => !x.L8_LD.IsEmpty).Select(x => x.L8_LD.ToGuid()).Distinct().ToArray();
			var databasePkMap = factory.Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.PK, databasePks)).ToDictionary(x => x.PK.ToGuid());

			foreach (var companyFees in feeUsagesByCompany)
			{
				var licCompany = companyDeliveries.GetCompany(companyFees.Key);
				IEnumerable<ClientInvoiceDelivery> deliveries = companyDeliveries.GetDeliveries(companyFees.Key);
				foreach (var companyAndDatabaseFees in companyFees.GroupBy(x => x.Fee.L8_LD))
				{
					LicenceDatabase db = !companyAndDatabaseFees.Key.IsEmpty ? databasePkMap[companyAndDatabaseFees.Key.ToGuid()] : null;
					var delivery = deliveries != null
						? ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, db?.LD_ServerCode, BillingConstants.BillingSystem.Fee)
						: null;
					var owner = new UsageOwnerDelivery(new UsageOwner(db, licCompany, null), delivery, companyDeliveries.GetInvoicedCompany(delivery));

					foreach (var fee in companyAndDatabaseFees)
					{
						fee.OwnerDelivery = owner;
					}
				}
			}
		}

		readonly List<FeeUsage> feeUsages;
		readonly ClientLicenceFee[] fees;

		public IEnumerable<FeeUsage> FeeUsages {  get { return feeUsages; } }

		public bool Any()
		{
			return fees.Length > 0;
		}

		static ClientChargeableUsage[] GetFeeChargeableUsages(BillingRunContext context, ClientLicenceFee[] fees)
		{
			ClientChargeableUsage[] result;
			var feePks = fees.Select(x => x.PK).ToArray();
			if (feePks.Length == 0)
			{
				result = Array.Empty<ClientChargeableUsage>();
			}
			else
			{
				var query = new ZQuery(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.Fee);
				query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, context.PeriodStart);
				query.AddToFilter(ClientChargeableUsageSchema.U1_Parent, feePks);
				result = context.Factory.Load<ClientChargeableUsage>(query);
			}

			return result;
		}
	}
}
