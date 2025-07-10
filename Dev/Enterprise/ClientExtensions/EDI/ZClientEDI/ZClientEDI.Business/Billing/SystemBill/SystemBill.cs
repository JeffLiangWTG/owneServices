using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Usages and amounts for a single billing system on a single bill (invoice).
	/// Part of a complete invoice (OrganisationBill class).
	/// There is at most one SystemBill per system in an OrganisationBill.
	/// The usages may come from different periods and different organizations,
	/// but they are to be on the same invoice.
	/// PeriodStart is the maximum from all usages.
	/// Note, Odpl system usages will all have the same PeriodStart
	/// since Odpl billing does not include previous months.
	/// </summary>
	public class SystemBill : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SystemBill(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Main Properties

		public ZGuid OrganisationPK { get; protected set; }

		/// <summary>
		/// The period being billed
		/// </summary>
		public ZDateTime BillingPeriod { get; set; }

		/// <summary>
		/// The most recent PeriodStart from all the usages.
		/// </summary>
		public ZDateTime PeriodStart { get; protected set; }

		public ZString SystemCode { get; protected set; }

		/// <summary>
		/// UsageCode to use when populating EdiBilledUsage.BU9_UsageCode
		/// If not set, the SystemCode will be used.
		/// </summary>
		public ZString UsageCodeForBilledUsage { get; set; }

		public ZBool IsBilled
		{
			get { return SystemUsages.Count > 0 ? SystemUsages[0].IsBilled : ZBool.True; }
		}

		public ZBool IsBillable
		{
			get { return SystemUsages.Count > 0 ? SystemUsages[0].IsBillable : ZBool.True; }
		}

		#endregion

		#region Organisation

		public EDIOrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.Load<EDIOrgHeader>(OrganisationPK)); }
		}
		EDIOrgHeader organisation;

		[BusinessObjectTestExclude] // since it can return null
		public ClientLicenceBillingDiscountCollection Discounts
		{
			get { return Organisation != null && Organisation.LicCompany != null ? Organisation.LicCompany.SelfBilling.BillingDiscounts : null; }
		}

		public List<ClientLicenceBillingDiscount> GetDiscountsForMonth(ClientLicencePriceHeader prices, ZDateTime firstDayOfMonth)
		{
			var result = new List<ClientLicenceBillingDiscount>();
			ZDateTime lastDayOfMonth = new ZDateTime(firstDayOfMonth).AddMonths(1).AddDays(-1);
			if (prices != null)
			{
				result.AddRange(prices.Discounts.Where(x => x.L5_SystemCode == SystemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)));
			}

			var localDiscounts = Discounts;
			if (localDiscounts != null)
			{
				result.AddRange(localDiscounts.Where(x => x.L5_SystemCode == SystemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)));
			}

			return result;
		}

		#endregion

		#region Amount Properties

		public ZDecimal Amount { get; protected set; }
		public ZDecimal AmountExemptProcessingFee { get; protected set; }
		public ZDecimal DiscountAmount { get; protected set; }
		public ZDecimal SurchargeAmount { get; protected set; }

		public ZDecimal TotalAmount
		{
			get { return Amount + SurchargeAmount - DiscountAmount; }
		}

		#endregion

		#region Currency / Exchange Rate

		public ZString CurrencyCode
		{
			get { return SystemUsages.Count > 0 ? SystemUsages[0].CurrencyCode : ZString.Empty; }
		}

		#endregion

		#region Calculate Group Amounts

		protected virtual void CalculateGroupAmounts()
		{
			Amount = SystemUsages.Sum(x => x.Amount);
			AmountExemptProcessingFee = SystemUsages.Sum(x => x.AmountExemptProcessingFee);
			DiscountAmount = 0m;
			SurchargeAmount = 0m;
		}

		#endregion

		#region Free Trials

		public virtual CodeDescriptionPairList GetFreeTrials()
		{
			return null;
		}

		#endregion

		#region Charge Codes

		public virtual ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
		}

		public virtual ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			return EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
		}

		public ZString GetSurchargeChargeCodeName()
		{
			return EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value;
		}

		#endregion

		#region Text Properties

		public ZString OrganisationCode
		{
			get { return Organisation != null ? Organisation.OH_Code : ZString.Empty; }
		}

		[ReadOnly(true)]
		public ZString Information { get; set; }

		public virtual ZString SystemDescription
		{
			get { return BillingConstants.BillingSystemList.GetDescriptionFromCode(SystemCode); }
		}

		#endregion

		#region System Usages

		public List<SystemUsage> SystemUsages
		{
			get { return systemUsages ?? (systemUsages = new List<SystemUsage>()); }
		}
		List<SystemUsage> systemUsages;

		public void PopulateFromSystemUsages(SystemUsage[] systemUsagesToAdd)
		{
			OrganisationPK = ZGuid.Empty;
			organisation = null;
			var mostRecentPeriodStart = systemUsagesToAdd.Length > 0 ? systemUsagesToAdd[0].PeriodStart : ZDateTime.Empty;

			SystemUsages.Clear();
			foreach (SystemUsage systemUsage in systemUsagesToAdd)
			{
				SystemUsages.Add(systemUsage);
				if (OrganisationPK.IsEmpty)
				{
					OrganisationPK = systemUsage.InvoicedOrganisationPK;
					SystemCode = systemUsage.SystemCode;
				}

				if (systemUsage.PeriodStart > mostRecentPeriodStart)
				{
					mostRecentPeriodStart = systemUsage.PeriodStart;
				}
			}

			PeriodStart = mostRecentPeriodStart;
			if (BillingPeriod.IsEmpty)
			{
				BillingPeriod = PeriodStart;
			}
			CalculateGroupAmounts();
		}

		public InvoiceGroup InvoiceGroupKey { get; set; }

		protected SummaryOptions CalculateSummaryStyle(IEnumerable<SystemUsage> usages)
		{
			SummaryOptions result = 0;
			if (HasMultipleLicences(usages))
			{
				result |= SummaryOptions.ShowLicenceCode;
			}

			if (HasEarlierPeriods(usages))
			{
				result |= SummaryOptions.ShowPeriod;
			}

			return result;
		}

		bool HasEarlierPeriods(IEnumerable<SystemUsage> usages)
		{
			return usages.Any(x => x.PeriodStart != this.BillingPeriod);
		}

		public bool HasUsagesForEarlierPeriods
		{
			get { return HasEarlierPeriods(SystemUsages); }
		}

		bool HasMultipleLicences(IEnumerable<SystemUsage> usages)
		{
			bool result = false;
			var iter = usages.GetEnumerator();
			if (iter.MoveNext())
			{
				var first = iter.Current;
				while (iter.MoveNext())
				{
					var usage = iter.Current;
					if (first.EnterpriseCode != usage.EnterpriseCode ||
						first.CompanyCode != usage.CompanyCode ||
						first.ServerCode != usage.ServerCode)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		protected SummaryOptions GetSummaryOptions()
		{
			return CalculateSummaryStyle(SystemUsages);
		}

		protected SummaryOptions GetGroupSummaryOptions()
		{
			return CalculateSummaryStyle(SystemUsages) | SummaryOptions.ShowLicenceCode;
		}

		#endregion

		#region General Summary

		public virtual SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return BuildGeneralSummarySections(organisationPK, false);
		}

		protected void BuildSummaryLine(SummaryLine line, SummaryOptions options, SystemUsage usage)
		{
			if ((options & SummaryOptions.ShowSubCode) != 0 && !usage.SubCode.IsEmpty)
			{
				line.MainDescription += " [" + usage.SubCode + "]";
			}
			if ((options & SummaryOptions.ShowLicenceCode) != 0)
			{
				line.MainDescription += " (" + usage.LicenceNineCode + ")";
			}
			if ((options & SummaryOptions.ShowPeriod) != 0)
			{
				line.MainDescription += " " + usage.PeriodStartAsText;
			}
		}

		protected SummarySection BuildSingleSummarySection(SystemUsage[] organisationUsages, bool checkForSubCode, Predicate<SystemUsage> usageFilter = null, bool includeLicenceUnits = false)
		{
			var usages = organisationUsages.Where(x => usageFilter == null || usageFilter(x));
			if (!usages.Any())
			{
				return null;
			}
			else
			{
				var options = CalculateSummaryStyle(usages);
				if (checkForSubCode)
				{
					options |= SummaryOptions.ShowSubCode;
				}

				SummarySection section = new SummarySection(Factory);
				var header = section.Header;

				List<SummaryLine> lines = new List<SummaryLine>();
				foreach (var usage in usages)
				{
					SummarySection currentSummary = usage.GetGeneralSummarySections().First();
					header.CopyMissingHeaderLabelsFrom(currentSummary.Header);

					foreach (SummaryLine line in currentSummary.Lines.Cast<SummaryLine>())
					{
						BuildSummaryLine(line, options, usage);
						lines.Add(line);
					}
				}

				bool showSpaceBetweenLines = lines.Any(x => x.MainDescription.Length > 50 || x.MainDescription.IndexOf('\n') >= 0);

				foreach (var line in lines)
				{
					if (showSpaceBetweenLines && section.Lines.Count > 0)
					{
						section.Lines.AddNew();
					}
					section.Lines.Add(line);
				}

				header.TotalAmount = usages.Sum(x => x.Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				if (includeLicenceUnits && HasLicenceUnits)
				{
					header.LicenceUnits = "Licence Units";
					header.LicenceUnitsAmount = "Total Licence Units";
					header.TotalLicenceUnits = usages.Sum(x => x.LicenceUnitsAmount).ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);

					// Group summary only
					header.LicenceUnitsAmountDescription = "Total Licence Units";
				}

				var companyDescriptions = usages.Select(x => x.ClientCompanyDescription).Distinct();
				if (companyDescriptions.Count() == 1)
				{
					header.ClientCompanyDescription = companyDescriptions.First();
				}

				return section;
			}
		}

		[Flags]
		public enum SummaryOptions
		{
			None = 0,
			ShowPeriod = 1,
			ShowLicenceCode = 2,
			ShowSubCode = 4,
			ShowSpaceBetweenLines = 8
		}

		protected void AddSummarySection(List<SummarySection> sections, SystemUsage[] organisationUsages, Predicate<SystemUsage> usageFilter = null)
		{
			var section = BuildSingleSummarySection(organisationUsages, false, usageFilter);
			if (section != null)
			{
				sections.Add(section);
			}
		}

		protected SummarySection[] BuildGeneralSummarySections(ZGuid organisationPK, bool asSingleSection, bool includeLicenceUnits = false)
		{
			SystemUsage[] organisationUsages = SystemUsages.Where(x => x.OrganisationPK == organisationPK).ToArray();
			if (organisationUsages.Length == 0)
			{
				return Array.Empty<SummarySection>();
			}

			SummarySection[] result;

			if (asSingleSection)
			{
				SummarySection resultSection = BuildSingleSummarySection(organisationUsages, true, null, includeLicenceUnits);
				result = new SummarySection[] { resultSection };
			}
			else
			{
				var options = CalculateSummaryStyle(organisationUsages);
				List<SummarySection> allSections = new List<SummarySection>();
				foreach (SystemUsage systemUsage in organisationUsages)
				{
					SummarySection[] orgSections = systemUsage.GetGeneralSummarySections();
					if (options != 0)
					{
						foreach (var section in orgSections)
						{
							BuildSummaryLine(section.Header, options, systemUsage);
						}
					}

					if (!systemUsage.ClientCompanyDescription.IsEmpty)
					{
						foreach (var section in orgSections)
						{
							section.Header.ClientCompanyDescription = systemUsage.ClientCompanyDescription;
						}
					}

					allSections.AddRange(orgSections);
				}

				result = allSections.ToArray();
			}

			return result;
		}

		#endregion

		#region Group Summary

		public virtual SummarySection[] GetGroupSummarySections()
		{
			SummarySection result = GetGroupSummarySection(SystemDescription + " Group Summary", delegate(SystemUsage x) { return x.Amount; });
			return new SummarySection[] { result };
		}

		protected SummarySection GetGroupSummarySection(ZString headerDescription, Func<SystemUsage, ZDecimal> getAmount)
		{
			SummarySection result = new SummarySection(Factory);
			result.Header.MainDescription = headerDescription;

			var options = GetGroupSummaryOptions();

			ZDecimal totalAmount = 0m;
			ZDecimal totalLicenceUnitsAmount = 0;
			foreach (SystemUsage systemUsage in SystemUsages.OrderBy(x => x.Organisation.OH_Code).ThenBy(x => x.PeriodStart))
			{
				ZDecimal usageAmount = getAmount(systemUsage);
				ZDecimal licenceUnitsAmount = systemUsage.LicenceUnitsAmount;
				if (usageAmount != 0m)
				{
					SummaryLine groupLine = result.Lines.AddNew();
					groupLine.MainDescription = systemUsage.Organisation.OH_Code;
					groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

					BuildSummaryLine(groupLine, options, systemUsage);

					groupLine.TaxCode = TaxCode(systemUsage.InvoiceDelivery);
					groupLine.TotalLicenceUnits = licenceUnitsAmount.ToString();

					totalAmount += usageAmount;
					totalLicenceUnitsAmount += licenceUnitsAmount;
				}
			}

			result.Header.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			result.Header.AmountDescription = ZString.Format("Total Price ({0})", CurrencyCode);
			result.Header.TotalDescription = HasLicenceUnits ? new ZString("Total") : ZString.Format("Total ({0})", CurrencyCode);
			result.Header.TotalLicenceUnits = totalLicenceUnitsAmount.ToString();
			result.Header.LicenceUnitsAmountDescription = "Total Licence Units";

			return result;
		}

		protected SummarySection BuildGroupSummarySectionOneLinePerOrg(bool includeLicenceUnits = false)
		{
			SummarySection result = new SummarySection(Factory);
			result.Header.MainDescription = SystemDescription + " Group Summary";

			var options = GetGroupSummaryOptions();

			ZDecimal totalAmount = 0m;
			ZDecimal totalLicenceUnitsAmount = 0;
			foreach (var group in SystemUsages.OrderBy(x => x.Organisation.OH_Code)
				.ThenBy(x => x.PeriodStart)
				.GroupBy(x => new { x.Organisation.OH_Code, x.PeriodStartAsText, x.LicenceNineCode }))
			{
				ZDecimal usageAmount = group.Sum(x => x.Amount);
				if (usageAmount != 0m)
				{
					SummaryLine groupLine = result.Lines.AddNew();
					groupLine.MainDescription = group.Key.OH_Code;
					groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					BuildSummaryLine(groupLine, options, group.First());
					if (includeLicenceUnits)
					{
						ZDecimal licenceUnitsAmount = group.Sum(x => x.LicenceUnitsAmount);
						groupLine.TotalLicenceUnits = licenceUnitsAmount.ToString();
						totalLicenceUnitsAmount += licenceUnitsAmount;
					}

					totalAmount += usageAmount;
				}
			}

			result.Header.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			result.Header.AmountDescription = ZString.Format("Total Price ({0})", CurrencyCode);
			result.Header.TotalDescription = ZString.Format("Total ({0})", CurrencyCode);
			if (includeLicenceUnits)
			{
				result.Header.TotalLicenceUnits = totalLicenceUnitsAmount.ToString();
				result.Header.LicenceUnitsAmountDescription = "Total Licence Units";
			}

			return result;
		}

		protected ZString TaxCode(ClientInvoiceDelivery delivery)
		{
			ZString result = ZString.Empty;

			if (delivery != null)
			{
				if (delivery.TaxId != null)
				{
					result = delivery.TaxId.AT_Code;
				}
				else if (delivery.SalesTaxChargeCode != null)
				{
					result = delivery.SalesTaxChargeCode.AC_Code;
				}
			}

			return result;
		}

		#endregion

		#region Discount Summary

		public virtual SummarySection[] GetDiscountSummarySections()
		{
			return Array.Empty<SummarySection>();
		}

		protected SummarySection[] GetMultiPeriodDiscountSummarySections(Dictionary<ZDateTime, DiscountCalculation> discountCalculations)
		{
			if (discountCalculations.Count > 0 && discountCalculations.Values.Any(x => x.DiscountDescriptions.Any()))
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (var periodDiscountCalculation in discountCalculations)
				{
					foreach (ZString discountDescription in periodDiscountCalculation.Value.DiscountDescriptions)
					{
						SummaryLine summaryLine = result.Lines.AddNew();
						summaryLine.MainDescription = discountDescription;

						if (HasUsagesForEarlierPeriods)
						{
							ZString periodAsString = periodDiscountCalculation.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture);
							summaryLine.MainDescription = periodAsString + " " + summaryLine.MainDescription;
						}
					}
				}

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		protected SummarySection[] GetSinglePeriodDiscountSummarySections(DiscountCalculation discountCalculation)
		{
			if (discountCalculation.DiscountDescriptions.Any())
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (ZString discountDescription in discountCalculation.DiscountDescriptions)
				{
					SummaryLine summaryLine = result.Lines.AddNew();
					summaryLine.MainDescription = discountDescription;
				}

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion

		#region Surcharge Summary

		public virtual SummarySection[] GetSurchargeSummarySections()
		{
			return Array.Empty<SummarySection>();
		}

		#endregion

		#region Add Amount Lines To Invoice

		public class TaxGroup
		{
			public TaxGroup(AccTaxRate taxId, AccChargeCode salesTax)
			{
				this.TaxId = taxId;
				this.SalesTax = salesTax;
			}

			public TaxGroup(ClientInvoiceDelivery delivery)
				: this(delivery?.TaxId, delivery?.SalesTaxChargeCode)
			{
			}

			public AccTaxRate TaxId;
			public AccChargeCode SalesTax;
			public ZString Code
			{
				get
				{
					return CombineCodes(
						TaxId != null ? TaxId.AT_Code : null,
						SalesTax != null ? SalesTax.AC_Code : null);
				}
			}

			public decimal? TaxPercentage
			{
				get
				{
					if (TaxId != null)
					{
						return TaxId.GetRate(ZDate.Today);
					}
					else if (SalesTax != null)
					{
						return SalesTaxPercentage;
					}
					else
					{
						return null;
					}
				}
			}

			static string CombineCodes(string a, string b)
			{
				bool bEmpty = string.IsNullOrEmpty(b);
				if (!string.IsNullOrEmpty(a))
				{
					return !bEmpty ? a + " " + b : a;
				}
				else
				{
					return !bEmpty ? b : string.Empty;
				}
			}

			public static decimal GetSalesTaxPercentage(AccChargeCode salesTax)
			{
				decimal result = 0;
				if (salesTax != null)
				{
					string rateText = EDIDataRegistry.Instance.SalesTaxRates.Value.GetDescriptionFromCode(salesTax.AC_Code);
					if (string.IsNullOrEmpty(rateText) || !decimal.TryParse(rateText, out result))
					{
						throw new InvalidOperationException("Invalid or missing sales tax rate for " + salesTax.AC_Code + " in registry " +
							((IRegistryItemInternals)EDIDataRegistry.Instance.SalesTaxRates).Location);
					}
				}

				return result;
			}

			public decimal SalesTaxPercentage
			{
				get
				{
					if (salesTaxPercentage == -1)
					{
						salesTaxPercentage = GetSalesTaxPercentage(SalesTax);
					}

					return salesTaxPercentage;
				}
			}
			decimal salesTaxPercentage = -1;

			public override bool Equals(object obj)
			{
				return Equals((TaxGroup)obj);
			}

			public bool Equals(TaxGroup other)
			{
				return TaxId == other.TaxId
					&& SalesTax == other.SalesTax;
			}

			public override int GetHashCode()
			{
				return (TaxId != null ? TaxId.PK.GetHashCode() : 0)
					^ (SalesTax != null ? SalesTax.PK.GetHashCode() : 0);
			}
		}

		public class BillLine
		{
			public BillLine(ZDecimal amount,
				TaxGroup tax,
				ZString currencyCode,
				ZString chargeCodeName,
				ZString description,
				int sequence,
				ZString systemCode,
				ZString subCode,
				ZGuid departmentPK,
				ZDateTime taxDate)
			{
				Amount = amount;
				Tax = tax;
				CurrencyCode = currencyCode;
				ChargeCodeName = chargeCodeName;
				Description = description;
				Sequence = sequence;
				SystemCode = systemCode;
				SubCode = subCode;
				DepartmentPK = departmentPK;
				TaxDate = taxDate;
			}

			public BillLine(ZDecimal amount,
				TaxGroup tax,
				ZString currencyCode,
				ZString chargeCodeName,
				ZString description,
				int sequence,
				ZString systemCode,
				ZGuid departmentPK)
				: this(amount, tax, currencyCode, chargeCodeName, description, sequence, systemCode, ZString.Empty, departmentPK, ZDateTime.Empty)
			{
			}

			public BillLine(ZDecimal amount,
				TaxGroup tax,
				ZString currencyCode,
				ZString chargeCodeName,
				ZString description,
				int sequence,
				ZString systemCode,
				ZString subCode)
				: this(amount, tax, currencyCode, chargeCodeName, description, sequence, systemCode, subCode, ZGuid.Empty, ZDateTime.Empty)
			{
			}

			public BillLine(ZDecimal amount,
				TaxGroup tax,
				ZString currencyCode,
				ZString chargeCodeName,
				ZString description,
				int sequence,
				ZString systemCode)
				: this(amount, tax, currencyCode, chargeCodeName, description, sequence, systemCode, ZString.Empty, ZGuid.Empty, ZDateTime.Empty)
			{
			}

			public ZDecimal Amount;
			public ZDecimal AmountInInvoiceCurrency;
			public TaxGroup Tax;
			public ZString CurrencyCode;
			public ZString ChargeCodeName;
			public ZString Description;
			public int Sequence;
			public ZString SystemCode;
			public ZString SubCode;
			public ZGuid DepartmentPK;
			public ZDateTime TaxDate;
			public bool IsProcessingFeeExempt;
			public bool RequireCommentLineAfter = true;
			public bool IsIncludedInPrepay;
			public ARInvoiceLine InvoiceLine;
		}

		public void CreateInvoiceLines(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice = null)
		{
			CreateInvoiceLinesCore(lines, dateForExchangeRate, invoice);

			var processingFeeExemptBillingSystems = EDIDataRegistry.Instance.ProcessingFeeExemptBillingSystems.Value;
			foreach (var line in lines)
			{
				if (processingFeeExemptBillingSystems.ContainsCode(line.SystemCode))
				{
					line.IsProcessingFeeExempt = true;
				}
			}
		}

		protected virtual void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var chargeCodes = SystemUsages.Select(x => (string)GetAmountChargeCodeName(x)).Distinct();
			if (chargeCodes.Count() > 1)
			{
				throw new InvalidOperationException("Multipe charge codes are defined for " + Organisation.OH_Code + " " + SystemDescription + " (" + SystemCode + "): "
						+ string.Join(", ", chargeCodes));
			}

			var totalAmountIsZero = SystemUsages.Sum(x => x.Amount) == 0m;
			var groupWeights = BuildTaxGroupWeights(SystemUsages, totalAmountIsZero);
			decimal totalWeight = groupWeights.Sum(x => x.Value);
			CreateProRataLines(lines, groupWeights, totalWeight);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected static List<KeyValuePair<TaxGroup, decimal>> BuildTaxGroupWeights(IEnumerable<SystemUsage> usages, bool totalAmountIsZero = false)
		{
			List<KeyValuePair<TaxGroup, decimal>> groupWeights = new List<KeyValuePair<TaxGroup, decimal>>();
			foreach (var usageTaxGroup in usages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				decimal groupWeight = totalAmountIsZero
					? usageTaxGroup.Count()
					: usageTaxGroup.Sum(x => x.Amount);
				if (groupWeight != 0)
				{
					groupWeights.Add(new KeyValuePair<TaxGroup, decimal>(usageTaxGroup.Key, groupWeight));
				}
			}

			return groupWeights;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void CreateProRataLines(List<BillLine> lines, List<KeyValuePair<TaxGroup, decimal>> groupWeights, decimal totalWeight)
		{
			CreateProRataLines(lines, groupWeights, totalWeight, Amount, GetAmountChargeCodeName(SystemUsages.FirstOrDefault()), BillingSystemDescription + " Usage");

			if (DiscountAmount != 0)
			{
				CreateProRataLines(lines, groupWeights, totalWeight, -DiscountAmount, GetDiscountChargeCodeName(SystemUsages.FirstOrDefault()), BillingSystemDescription + " Discount");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void CreateProRataLines(List<BillLine> lines,
			List<KeyValuePair<TaxGroup, decimal>> groupWeights,
			decimal totalWeight,
			decimal amount,
			ZString chargeCodeName,
			ZString description)
		{
			if (amount == 0)
			{
				return;
			}

			decimal amountRemaining = amount;
			List<KeyValuePair<TaxGroup, decimal>> nonZeroGroupWeights = groupWeights.FindAll(x => x.Value != 0);
			for (int i = 0; i < nonZeroGroupWeights.Count; ++i)
			{
				KeyValuePair<TaxGroup, decimal> groupWeight = nonZeroGroupWeights[i];
				decimal groupAmount = (i != nonZeroGroupWeights.Count - 1)
					? Utilities.Round(amount * groupWeight.Value / totalWeight, BillingConstants.RoundingDecimals)
					: amountRemaining;

				if (groupAmount != 0)
				{
					amountRemaining -= groupAmount;
					TaxGroup tax = groupWeight.Key;
					lines.Add(new BillLine(groupAmount, tax, CurrencyCode, chargeCodeName, description, lines.Count, SystemCode, ZGuid.Empty));
				}
			}
		}

		protected ZString BillingSystemDescription
		{
			get { return BillingConstants.BillingSystemList.GetDescriptionFromCode(SystemCode); }
		}

		#endregion

		#region Create Revenue Breakdown

		public virtual void CreateRevenueBreakdown(ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			CreatePriceItemRevenueBreakdown(invoice, signedProcessingFeePercentage);
		}

		protected void CreatePriceItemRevenueBreakdown(ARInvoice invoice, decimal signedProcessingFeePercentage)
		{
			decimal totalPreDiscount = Amount;
			decimal totalUsageAmount = SystemUsages.Sum(x => x.Amount);
			if (totalPreDiscount == 0 || totalUsageAmount == 0 || SystemUsages.Count == 0)
			{
				return;
			}

			decimal total = TotalAmount;
			decimal totalProcessingFee = signedProcessingFeePercentage / 100m * total;
			decimal totalPostDiscount = TotalAmount + totalProcessingFee;

			foreach (var usage in SystemUsages.Where(x => x.Amount != 0))
			{
				if (usage.CurrencyCode != CurrencyCode)
				{
					throw new InvalidOperationException("Revenue breakdown has multiple currencies for " + Organisation.OH_Code + " " + SystemDescription + " "
						+ string.Join(", ", SystemUsages.Select(x => x.CurrencyCode).Distinct()));
				}
				var billed = invoice.Factory.New<EdiBilledUsage>();
				PopulateBilledUsage(billed, invoice, usage, totalUsageAmount, totalPreDiscount, totalPostDiscount, totalProcessingFee);

				var priceItem = (usage as IPriceItemUsage)?.PriceItem;
				if (priceItem != null)
				{
					billed.BU9_L7 = priceItem.PK;
					billed.BU9_PriceCode = priceItem.L7_Code;
				}

				EdiBilledDiscount.CreateBilledDiscounts(billed, DiscountDetailedInfos, SurchargeDetailedInfos);
			}
		}

		protected void PopulateBilledUsage(EdiBilledUsage billed, ARInvoice invoice, SystemUsage usage,
			decimal totalUsageAmount, decimal totalPreDiscount, decimal totalPostDiscount, decimal totalProcessingFee)
		{
			decimal weight = usage.Amount / totalUsageAmount;
			decimal preDiscount = weight * totalPreDiscount;
			decimal postDiscount = weight * totalPostDiscount;
			decimal processingFee = weight * totalProcessingFee;
			var currency = usage.CurrencyCode;
			var dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(invoice);

			billed.BU9_AC_AmountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, GetAmountChargeCodeName(usage));
			if (preDiscount != postDiscount)
			{
				billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, GetDiscountChargeCodeName(usage));
				if (billed.BU9_AC_DiscountChargeCode.IsEmpty)
				{
					billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
				}
			}
			billed.BU9_AH_Invoice = invoice.PK;
			billed.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			billed.BU9_LC = usage.User.LicenceCompanyPK;
			billed.BU9_LCC = usage.User.ClientCompanyPK;
			billed.BU9_LD = usage.User.DatabasePK;
			billed.BU9_LocalAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_LocalAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_LocalProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_PeriodStart = PeriodStart.Date;
			billed.BU9_PriceCurrency = currency;
			billed.BU9_TransactionAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_TransactionAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_TransactionProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_UnitCount = new ZDecimal(usage.UnitCount);
			billed.BU9_UnitPrice = usage.UnitPrice;
			billed.BU9_UsageCode = !UsageCodeForBilledUsage.IsEmpty ? UsageCodeForBilledUsage : SystemCode;
			billed.BU9_UsageSubCode = usage.SubCode;
		}

		protected virtual IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => Enumerable.Empty<DiscountDetailedInfo>();
		protected virtual IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => Enumerable.Empty<DiscountDetailedInfo>();

		#endregion

		#region On Invoice Saving

		public void OnInvoiceFactorySaving(ARInvoice invoice)
		{
			Argument.NotNull(invoice, "invoice");

			SetChargeableUsagesInvoice(invoice);
			OnInvoiceFactorySavingCore(invoice);
		}

		void SetChargeableUsagesInvoice(ARInvoice invoice)
		{
			List<ZGuid> chargeableUsagePKs = new List<ZGuid>();
			foreach (SystemUsage systemUsage in SystemUsages)
			{
				chargeableUsagePKs.AddRange(systemUsage.ChargeableUsagePKs);
			}

			if (chargeableUsagePKs.Count > 0)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
				query.AddToFilter(ClientChargeableUsageSchema.PK, chargeableUsagePKs);

				var usages = invoice.Factory.Load<ClientChargeableUsage>(query);
				var usageByPk = usages.ToDictionary(x => x.PK);

				foreach (var chargeableUsage in usages)
				{
					chargeableUsage.U1_AH_Invoice = invoice.PK;
					chargeableUsage.U1_InvoicedUnitCount = chargeableUsage.U1_UnitCount;
				}

				foreach (SystemUsage systemUsage in SystemUsages)
				{
					var licenceCompanyPK = systemUsage.User.LicenceCompanyPK;
					if (!licenceCompanyPK.IsEmpty)
					{
						foreach (var usagePk in systemUsage.ChargeableUsagePKs)
						{
							ClientChargeableUsage chargeableUsage;
							if (usageByPk.TryGetValue(usagePk, out chargeableUsage)
								&& chargeableUsage.U1_LC != licenceCompanyPK)
							{
								chargeableUsage.U1_LC = licenceCompanyPK;
							}
						}
					}
				}
			}

			SetChargeableUsagesInvoiceCore(invoice);
		}

		protected virtual void SetChargeableUsagesInvoiceCore(ARInvoice invoice)
		{
		}

		protected virtual void OnInvoiceFactorySavingCore(ARInvoice invoice)
		{
		}

		#endregion

		#region Validation

		protected override ZString HumanReadableNameCore
		{
			get { return "System Bill"; }
		}

		public void ValidateAll(BusinessObject notificationOwner)
		{
			ValidateCurrencyCode(notificationOwner);
			ValidateUnitPrice(notificationOwner);
			ValidateStandardDiscounts(notificationOwner);
			ValidateAllCore(notificationOwner);

			if (TotalAmount < 0 && Amount > 0)
			{
				notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: Discount over 100%", BillingSystemDescription));
			}
		}

		protected virtual bool CanHaveDifferentCurrenciesInTheGroup
		{
			get { return false; }
		}

		void ValidateCurrencyCode(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				foreach (SystemUsage systemUsage in SystemUsages.Where(x => x.CurrencyCode.IsEmpty))
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: No currency specified for {1}", BillingSystemDescription, systemUsage.Organisation.OH_Code));
				}

				if (!CanHaveDifferentCurrenciesInTheGroup)
				{
					ZString expectedCurrencyCode = SystemUsages[0].CurrencyCode;
					if (SystemUsages.Any(x => x.CurrencyCode != expectedCurrencyCode))
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0}: Different price currencies exist for the billing group.", BillingSystemDescription));
					}
				}
			}
		}

		protected virtual void ValidateUnitPrice(BusinessObject notificationOwner)
		{
		}

		protected virtual void ValidateAllCore(BusinessObject notificationOwner)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateStandardDiscounts(BusinessObject notificationOwner)
		{
			foreach (var usagesGroupedByMonth in SystemUsages.GroupBy(x => x.PeriodStart))
			{
				bool checkDiscountTypes = true;

				var usagesWithPrices = usagesGroupedByMonth.Where(x => x.PriceHeader != null && x.PriceHeader.IsMainPriceListType);
				var firstUsage = usagesWithPrices.FirstOrDefault();
				var firstPriceHeader = firstUsage?.PriceHeader;
				if (firstPriceHeader != null)
				{
					var usageMismatch = usagesWithPrices.Skip(1).FirstOrDefault(x => x.PriceHeader.L6_DiscountCode != firstPriceHeader.L6_DiscountCode);
					if (usageMismatch != null)
					{
						var priceHeader = usageMismatch.PriceHeader;
						notificationOwner.AddRowError(
							BillingSystemDescription + ": Multiple standard discounts - "
							+ firstUsage.Organisation.OH_Code + " has price version " + firstPriceHeader.L6_PricelistVersion + " and discount " + firstPriceHeader.L6_DiscountCode
							+ " vs "
							+ usageMismatch.Organisation.OH_Code + " has price version " + priceHeader.L6_PricelistVersion + " and discount " + priceHeader.L6_DiscountCode);
					}
				}

				if (checkDiscountTypes && firstPriceHeader != null && !firstPriceHeader.L6_DiscountCode.IsEmpty)
				{
					var stdDiscounts = firstPriceHeader.Discounts;
					var orgDiscounts = Discounts;
					if (stdDiscounts.Count != 0 && orgDiscounts != null && orgDiscounts.Count != 0)
					{
						var firstDayOfMonth = usagesGroupedByMonth.Key;
						var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
						var stdDiscountTypes = stdDiscounts.Where(x => x.L5_SystemCode == SystemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)).Select(x => x.L5_Type).Distinct().ToList();
						var orgDiscountTypes = orgDiscounts.Where(x => x.L5_SystemCode == SystemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)).Select(x => x.L5_Type).Distinct().ToList();
						if (stdDiscountTypes.Any(x => x == BillingConstants.DiscountType.Volume || x == BillingConstants.DiscountType.IncrementalVolume) &&
							orgDiscountTypes.Any(x => x == BillingConstants.DiscountType.Volume || x == BillingConstants.DiscountType.IncrementalVolume))
						{
							notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: standard and organization discounts both contain Volume discounts", BillingSystemDescription));
						}
					}
				}
			}
		}

		public virtual bool HasLicenceUnits
		{
			get { return SystemUsages.Any(x => x.HasLicenceUnits); }
		}

		#endregion
	}
}

