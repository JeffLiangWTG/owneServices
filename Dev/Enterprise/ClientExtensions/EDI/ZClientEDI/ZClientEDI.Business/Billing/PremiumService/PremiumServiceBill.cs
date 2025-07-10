using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Bill for all premium services for an organisation.
	/// Separate usages may refer to different price items and charge codes.
	/// </summary>
	public class PremiumServiceBill : SystemBill, ISystemMinimumFeeContributionBill
	{
		public PremiumServiceBill(BusinessObjectFactory factory)
			: base(factory)
		{
			this.SystemCode = BillingConstants.BillingSystem.Service;
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (PremiumServiceUsage)systemUsage;
			var priceItem = usage.PriceItem;
			if (priceItem != null && !priceItem.L7_ChargeCode.IsEmpty)
			{
				return priceItem.L7_ChargeCode;
			}
			return "";
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (PremiumServiceUsage)systemUsage;
			var priceItem = usage.PriceItem;
			if (priceItem != null && !priceItem.L7_DiscountChargeCode.IsEmpty)
			{
				return priceItem.L7_DiscountChargeCode;
			}
			return "";
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return BuildGeneralSummarySections(organisationPK, true);
		}

		#endregion

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				foreach (SystemUsage systemUsage in SystemUsages)
				{
					var itemUsage = systemUsage as PremiumServiceUsage;
					if (itemUsage != null)
					{
						if (!itemUsage.HasPriceItem)
						{
							notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No price with valid fee type found for {1}", BillingSystemDescription, itemUsage.Organisation.OH_Code));
						}
						else if (itemUsage.UnitPrice == 0m)
						{
							if (itemUsage.UseGlobalPrice)
							{
								notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No global price defined for {1}, price code {2} in currency {3}"
									, BillingSystemDescription, itemUsage.Organisation.OH_Code, itemUsage.PriceItemCode, itemUsage.CurrencyCode));
							}
							else
							{
								notificationOwner.AddRowWarning(string.Format(CultureInfo.InvariantCulture, "{0}: Zero price for {1}", BillingSystemDescription, itemUsage.Organisation.OH_Code));
							}
						}
					}
				}
			}
		}

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			foreach (var usagesByChargeCode in SystemUsages.GroupBy(x => (string)GetAmountChargeCodeName(x)))
			{
				var totalAmount = usagesByChargeCode.Sum(x => x.Amount);
				var totalAmountIsZero = totalAmount == 0m;
				var groupWeights = BuildTaxGroupWeights(usagesByChargeCode, totalAmountIsZero);
				decimal totalWeight = groupWeights.Sum(x => x.Value);

				var firstUsage = (PriceItemUsage)usagesByChargeCode.First();
				CreateProRataLines(lines, groupWeights, totalWeight, totalAmount, usagesByChargeCode.Key, firstUsage.PriceItem.L7_DescriptionLocalized.Trim());
			}
		}

		protected override void SetChargeableUsagesInvoiceCore(ARInvoice invoice)
		{
			foreach (PremiumServiceUsage systemUsage in SystemUsages)
			{
				ClientChargeableUsage[] chargeableUsages = null;
				if (systemUsage.ChargeableUsagePKs.Count > 0)
				{
					chargeableUsages = invoice.Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.PK, systemUsage.ChargeableUsagePKs));

					foreach (var chargeableUsage in chargeableUsages)
					{
						chargeableUsage.U1_UnitCount = 0;
						chargeableUsage.U1_InvoicedUnitCount = 0;
					}
				}

				foreach (var service in systemUsage.Services)
				{
					ClientChargeableUsage chargeableUsage = chargeableUsages != null ? chargeableUsages.FirstOrDefault(s => s.U1_Parent == service.PK) : null;
					ZGuid chargeableUsagePk = chargeableUsage != null ? chargeableUsage.PK : ZGuid.Empty;
					CreateOrUpdateChargeableUsage(invoice, systemUsage, service, chargeableUsagePk, systemUsage.PeriodStart);
				}
			}
		}

		internal static ZGuid CreateOrUpdateChargeableUsage(InvoicingBase invoice,
			PremiumServiceUsage systemUsage,
			ClientPremiumService service,
			ZGuid chargeableUsagePk,
			ZDateTime periodStart)
		{
			return CreateOrUpdateChargeableUsage(
				invoice,
				service,
				chargeableUsagePk,
				periodStart,
				systemUsage.FeeTypeUnitCount * service.CPS_Units,
				systemUsage.DatabasePK,
				systemUsage.UnitPrice,
				systemUsage.LicCompany != null ? systemUsage.LicCompany.PK : ZGuid.Empty).PK;
		}

		internal static ClientChargeableUsage CreateOrUpdateChargeableUsage(InvoicingBase invoice,
			ClientPremiumService service,
			ZGuid chargeableUsagePk,
			ZDateTime periodStart,
			int unitCount,
			ZGuid databasePk,
			ZDecimal price,
			ZGuid licenceCompanyPk)
		{
			ClientChargeableUsage usage = null;
			if (!chargeableUsagePk.IsEmpty)
			{
				usage = invoice.Factory.Load<ClientChargeableUsage>(chargeableUsagePk);
			}

			return CreateOrUpdateChargeableUsage(invoice, service, usage, periodStart, unitCount, databasePk, price, licenceCompanyPk);
		}

		internal static ClientChargeableUsage CreateOrUpdateChargeableUsage(InvoicingBase invoice,
			ClientPremiumService service,
			ClientChargeableUsage usage,
			ZDateTime periodStart,
			int unitCount,
			ZGuid databasePk,
			ZDecimal price,
			ZGuid licenceCompanyPk)
		{
			if (usage == null)
			{
				usage = invoice.Factory.New<ClientChargeableUsage>();
			}
			else
			{
				usage = usage.GetInAnotherFactory(invoice.Factory);
			}

			usage.U1_AH_Invoice = invoice.PK;
			usage.U1_Code = BillingConstants.BillingSystem.Service;
			usage.U1_InvoicedUnitCount = unitCount;
			usage.U1_LC = licenceCompanyPk;
			usage.U1_LCC = ZGuid.Empty;
			usage.U1_Parent = service.IsInDatabase ? service.PK : service.CPS_LD;
			usage.U1_PeriodStart = periodStart;
			usage.U1_SubCode = service.CPS_Type;
			usage.U1_UnitCount = unitCount;
			usage.U1_UnitPrice = price;
			usage.U1_UpdateTime = ZDateTime.UtcNow;
			usage.U1_LD = databasePk;
			return usage;
		}

		public IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			if (Amount > 0)
			{
				foreach (var usagesGroup in SystemUsages.Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => new { x.PeriodStart, x.User.DatabasePK }))
				{
					var ratio = usagesGroup.Sum(x => x.Amount) / Amount;
					var dbContribution = (Amount - DiscountAmount + SurchargeAmount) * ratio;

					var minimumFee = new SystemMinimumFee(usagesGroup.Key.DatabasePK, usagesGroup.Key.PeriodStart, dbContribution, CurrencyCode, "");
					result.Add(minimumFee);
				}
			}

			return result;
		}
	}
}

