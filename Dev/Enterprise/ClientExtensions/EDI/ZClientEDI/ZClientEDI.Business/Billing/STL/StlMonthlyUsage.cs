using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// STL usage for a single database, billing period, invoiced organization and tax ID.
	/// With per-company billing there are multiple invoiced organizations
	/// and multiple of these per database.
	/// Without per-company billing, there is only one per database.
	/// </summary>
	public class StlMonthlyUsage : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		public StlMonthlyUsage(BusinessObjectFactory factory, DatabaseUsage databaseUsage, ZDateTime periodStart, InvoiceGroup invoiceGroup, SystemBill.TaxGroup mainTaxGroup)
			: base(factory)
		{
			if (databaseUsage == null)
			{
				throw new ArgumentNullException(nameof(databaseUsage));
			}
			if (invoiceGroup == null)
			{
				throw new ArgumentNullException(nameof(invoiceGroup));
			}
			if (mainTaxGroup == null)
			{
				throw new ArgumentNullException(nameof(mainTaxGroup));
			}

			DatabaseUsage = databaseUsage;
			PeriodStart = periodStart;
			InvoiceGroup = invoiceGroup;
			MainTaxGroup = mainTaxGroup;
		}

		public StlMonthlyUsage(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DatabaseUsage DatabaseUsage { get; private set; }
		public ZDateTime PeriodStart { get; private set; }
		public InvoiceGroup InvoiceGroup { get; private set; }
		public SystemBill.TaxGroup MainTaxGroup { get; private set; }
		public SystemBill.TaxGroup SpecialTaxGroup { get; set; }

		public StlBill Bill { get; set; }
		public IBilledDatabase Database { get { return DatabaseUsage.Database; } }

		public ZString EnterpriseCode
		{
			get { return Database?.EnterpriseCode ?? DatabaseUsage.OwnerDelivery.OwnerCompany.LicEnterprise.LE_EnterpriseCode; }
		}

		public ZString ServerCode
		{
			get { return Database?.LD_ServerCode ?? "N/A"; }
		}

		public void AddUsageLine(UsageLine usageLine)
		{
			UsageLinesInternal.Add(usageLine);
			usingClientCompanies = null;

			if (!usageLine.PriceCurrency.IsEmpty)
			{
				priceCurrencies.Add(usageLine.PriceCurrency);
			}
		}

		ClientCompany[] usingClientCompanies;

		public IEnumerable<ClientCompany> UsingClientCompanies
		{
			get
			{
				if (usingClientCompanies == null)
				{
					usingClientCompanies = Usages.Where(x => x.ChargeableUsage != null && x.ChargeableUsage.ClientCompany != null)
						.Select(x => x.ChargeableUsage.ClientCompany)
						.Distinct()
						.OrderBy(x => x.LCC_Code)
						.ToArray();
				}

				return usingClientCompanies;
			}
		}

		public ZString PriceListVersion
		{
			get { return PriceHeader != null ? PriceHeader.L6_PricelistVersion : ZString.Empty; }
		}

		public ZBool IsSiteLive { get { return DatabaseUsage.IsSiteLive; } }
		public ZDateTime SiteLiveDate { get { return DatabaseUsage.SiteLiveDate; } }

		/// <summary>
		/// One UsageLine for each price item.
		/// The contributing subusages will be in UsageLine.SubUsages, e.g., one for each company
		/// </summary>
		public IEnumerable<UsageLine> UsageLines
		{
			get { return UsageLinesInternal.Cast<UsageLine>(); }
		}

		public IEnumerable<UsageLine> UsageLinesInOrder
		{
			get
			{
				return UsageLinesInternal.Cast<UsageLine>()
					.OrderBy(x => x.RoleIndex)
					.ThenBy(x => x.Sequence)
					.ThenByDescending(x => x.PeriodStart)
					.ThenBy(x => x.AdditionalDescription)
					.ThenBy(x => x.PriceItem?.L7_UnitBreak ?? 0);
			}
		}

		public UsageLineCollection UsageLinesForBindingOnly
		{
			get { return UsageLinesInternal; }
		}

		UsageLineCollection UsageLinesInternal
		{
			get { return usageLines ?? (usageLines = new UsageLineCollection(Factory)); }
		}
		UsageLineCollection usageLines;

		#region Commitment

		/// <summary>
		/// Dynamically created price item for the "Commitment" role section in the summary document.
		/// The section appears after all regular price item usage sections so it is given the highest role index.
		/// E.g.
		///		Commitment
		///					{Commitment 1 price/amount/etc details}
		///					...
		/// </summary>
		/// <returns></returns>
		public ClientLicencePriceItem GetOrCreateCommitmentRoleItem()
		{
			if (commitmentRoleItem == null)
			{
				commitmentRoleItem = Factory.New<ClientLicencePriceItem>();
				commitmentRoleItem.SetPriceItemDescription(ResString.GetMultilingualString("14bd8b26-566b-44b0-86c8-03da1f896d4c", "Commitment"));
			}
			return commitmentRoleItem;
		}
		ClientLicencePriceItem commitmentRoleItem;

		/// <summary>
		/// As of 2021, these are used for the summary document only, i.e., for display purposes only.
		/// The actual commitment adjustment is distributed over all the usage as a proportional adjustment.
		/// </summary>
		public IEnumerable<UsageLine> CommitmentLines
		{
			get { return CommitmentLinesInternal.Cast<UsageLine>(); }
		}

		public void AddCommitment(UsageLine commitmentLine)
		{
			CommitmentLinesInternal.Add(commitmentLine);
		}

		UsageLineCollection CommitmentLinesInternal
		{
			get { return commitmentLines ?? (commitmentLines = new UsageLineCollection(Factory)); }
		}
		UsageLineCollection commitmentLines;

		#endregion

		#region Fees

		public IEnumerable<FeeUsage> FeeUsages
		{
			get
			{
				return feeUsages ?? (feeUsages = Enumerable.Empty<FeeUsage>());
			}
		}

		public void AddFeeUsages(IEnumerable<FeeUsage> feeUsagesToAdd)
		{
			if (this.feeUsages != null)
			{
				throw new InvalidOperationException("AddFeeUsages was called more than once.");
			}

			this.feeUsages = feeUsagesToAdd;
			foreach (var feeUsage in feeUsagesToAdd)
			{
				if (!feeUsage.Fee.L8_RX_NKCurrency.IsEmpty)
				{
					priceCurrencies.Add(feeUsage.Fee.L8_RX_NKCurrency);
				}
			}
		}

		IEnumerable<FeeUsage> feeUsages;

		#endregion

		public IEnumerable<string> PriceCurrencies
		{
			get
			{
				return priceCurrencies;
			}
		}
		readonly HashSet<string> priceCurrencies = new HashSet<string>();

		public ZDecimal TotalUsedLicenceUnits => usageLines?.Cast<UsageLine>().Sum(x => x.LicenceUnits * x.TotalUnitCount) ?? 0;

		public ZDecimal TotalLicenceUnits
		{
			get
			{
				var result = TotalUsedLicenceUnits;

				if (CommitmentGroup != null)
				{
					result += CommitmentGroup.Adjustment;
				}
				else if (Commitment != null)
				{
					if (result < Commitment.LicenceUnits)
					{
						result = Commitment.LicenceUnits;
					}
				}

				return result;
			}
		}

		public IEnumerable<Usage> Usages
		{
			get
			{
				foreach (var usageLine in UsageLines)
				{
					foreach (var usage in usageLine.Usages)
					{
						yield return usage;
					}
				}
			}
		}

		public ClientLicencePriceHeader PriceHeader { get { return PriceList?.Header; } }
		public PriceList PriceList { get { return DatabaseUsage.StlPriceList; } }

		public string SingleDomesticEntityCountry { get { return DatabaseUsage.CountryUsers.DomesticCountryUserGroup.SingleDomesticEntityCountry; } }

		public StlBilling.BuyingGroupVolume BuyingGroup { get; set; }
		public StlBilling.SharedCommitment CommitmentGroup { get; set; }

		public int DatabaseCompanyCount
		{
			get { return DatabaseUsage.CompanyCount; }
		}

		public int DatabaseActiveUserCount
		{
			get { return DatabaseUsage.ActiveUserCount; }
		}

		public ZGuid InvoicedOrganisationPK { get { return InvoiceGroup.InvoicedOrganisationPK; } }

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new StlSummaryDocumentSupporter(this); }
		}

		#endregion

		public DepositUseSummary DepositSummary { get; set; }

		// Used to determine if prepayment discount is granted
		public ZBool HasPrepaid { get; set; }

		public bool IsPrepaymentDiscountAvailable { get; set; }

		public ConversionCreditLicenceSetting ConversionCredit { get { return DatabaseUsage.ConversionCredit; } }
		public CommitmentLicenceSetting Commitment { get { return DatabaseUsage.Commitment; } }

		public class CurrencyAmounts
		{
			public decimal UsagePreDiscountAmount { get; internal set; }
			public decimal UsagePostDiscountAmount { get; internal set; }
			public decimal FeePreDiscountAmount { get; internal set; }
			public decimal FeePostDiscountAmount { get; internal set; }
			public decimal TotalPreDiscountAmount => UsagePreDiscountAmount + FeePreDiscountAmount;
			public decimal TotalPostDiscountAmount => UsagePostDiscountAmount + FeePostDiscountAmount;

			public decimal VersionSurchargeAmount { get; internal set; }
			public decimal BillSurchargeAmount { get; internal set; }
		}

		public CurrencyAmounts GetAmountsForPriceCurrency(string priceCurrency)
		{
			var result = new CurrencyAmounts();

			foreach (var usageLine in UsageLines.Where(x => x.PriceCurrency == priceCurrency))
			{
				result.UsagePreDiscountAmount += usageLine.PreDiscountAmount;
				result.UsagePostDiscountAmount += usageLine.PostDiscountAmount;
			}

			foreach (var fee in FeeUsages.Where(x => x.Currency == priceCurrency))
			{
				result.FeePreDiscountAmount += fee.PreDiscountAmount;
				result.FeePostDiscountAmount += fee.PostDiscountAmount;
			}

			if (VersionSurchargePercent != 0)
			{
				result.VersionSurchargeAmount = result.UsagePostDiscountAmount * VersionSurchargePercent / 100m;
			}

			// Process fee surcharge applies to an amount that includes the Non-Current Version surcharge since it also has to be processed...
			var amountForProcessingSurcharge = result.TotalPostDiscountAmount + result.VersionSurchargeAmount;
			result.BillSurchargeAmount = amountForProcessingSurcharge * Bill.SurchargePercent / 100m;

			return result;
		}

		#region Non-Current Version surcharge

		/// <summary>
		/// Calculated Non-Current Version surcharge. Will be zero if database was not on Non-Current Version or surcharge is not enabled.
		/// </summary>
		public decimal VersionSurchargePercent { get; internal set; }
		public VersionNumber VersionSurchargeNonCurrentVersion { get; internal set; }
		public decimal VersionSurchargeAmountInInvoiceCurrency { get; internal set; }

		#endregion
	}
}

