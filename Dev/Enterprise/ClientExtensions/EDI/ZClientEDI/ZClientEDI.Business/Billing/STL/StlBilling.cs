using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.Billing.BorderWise;
using Enterprise.Client.EDI.Billing.Fee;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public class StlBilling : UsageBilling, IRelatedModuleFilterSupportable
	{
		public StlBilling(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Bills

		public StlBillCollection Bills
		{
			get { return bills ?? (bills = new StlBillCollection(Factory)); }
		}
		StlBillCollection bills;

		public StlBillValidationResultCollection ValidationResults
		{
			get { return validationResults ?? (validationResults = new StlBillValidationResultCollection()); }
		}
		StlBillValidationResultCollection validationResults;

		#endregion

		#region Generate Report

		public ZBool IncludeOnlySiteLive
		{
			get { return includeOnlySiteLive; }
			set
			{
				SetNonPersistentPropertyValue(IncludeOnlySiteLiveInfo, ref includeOnlySiteLive, value);
			}
		}
		ZBool includeOnlySiteLive = true;

		public ZPropertyInfo IncludeOnlySiteLiveInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeOnlySiteLive)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public void GenerateReport(IEdiProgress progress)
		{
			if (!Globals.IsTest)
			{
				FactoryForGenerate = null;
			}

			Bills.RemoveAll();
			ValidationResults.RemoveAll();

			var factory = FactoryForGenerate;
			factory.Saving -= factory_Saving;
			factory.Saving += factory_Saving;

			var context = new BillingRunContext(factory, ZDateTime.Today, DateTo, OrganisationPK, EnterpriseCode);
			context.IncludeOdpl = false;
			var codesFilter = ParseCodesText();
			var priceItemPKFilter = GetPriceItemPKFilter();
			IsPriceItemFilterApplicableOnReport = priceItemPKFilter != null;
			var includeFees = (codesFilter.Length == 0 || codesFilter.Any(x => string.Equals(x, BillingConstants.BillingSystem.Fee, StringComparison.OrdinalIgnoreCase))) && !IsPriceItemFilterApplicableOnReport;
			ClientLicenceFee[] feeList = null;
			IEnumerable<Guid> feeDatabasePks = null;
			if (includeFees)
			{
				var feeQuery = FeeBillingSystem.BuildFeesDueQuery(context.PeriodStart, context, false, true);
				feeList = factory.Load<ClientLicenceFee>(feeQuery);
				feeDatabasePks = feeList.Where(x => !x.L8_LD.IsEmpty).Select(x => x.L8_LD.ToGuid()).Distinct();
			}

			var databaseUsageSet = new DatabaseUsageSet(context, IncludeOnlySiteLive, codesFilter, AccumulateMonths, feeDatabasePks, new GenericUsageSetFactory());
			if (progress.SafeIsCancelled())
			{
				return;
			}

			StlFees stlFees = null;
			if (includeFees)
			{
				stlFees = new StlFees(context, feeList);
				if (!stlFees.Any())
				{
					stlFees = null;
				}
			}

			if (stlFees == null
				&& !databaseUsageSet.DatabaseUsages.Any()
				&& !databaseUsageSet.GenericUsageSets.Values.Any(x => x.HasUsage))
			{
				return;
			}

			var monthlyUsageList = MatchUsageToPrice(context, databaseUsageSet, priceItemPKFilter);
			var monthlyUsagesWithBranch = monthlyUsageList.Where(x => !x.InvoiceGroup.BranchPK.IsEmpty);

			var allDepositBalances = DepositBalanceCollection.LoadFromDb(monthlyUsagesWithBranch.Where(x => !x.InvoicedOrganisationPK.IsEmpty).Select(x => x.InvoicedOrganisationPK.ToGuid()), true);
			var orgPkToPositiveDepositBalances = allDepositBalances.Where(x => x.Amount > 0).GroupBy(x => x.OrgPk.ToGuid()).ToDictionary(x => x.Key, y => (IEnumerable<DepositBalance>)y);

			// ProtoBills
			var invoiceGroupToFees = stlFees != null ? stlFees.FeeUsages.GroupBy(x => x.OwnerDelivery.InvoiceGroup).ToDictionary(x => x.Key, y => y.ToArray()) : null;
			var invoiceGroupToMonthlyUsage = monthlyUsageList.GroupBy(x => x.InvoiceGroup).ToDictionary(x => x.Key, y => y.ToArray());
			var protoBills = new List<ProtoBill>((invoiceGroupToFees?.Count ?? 0) + invoiceGroupToMonthlyUsage.Count);
			foreach (var invoiceGroupUsagePair in invoiceGroupToMonthlyUsage)
			{
				var protoBill = new ProtoBill();
				protoBills.Add(protoBill);
				protoBill.MonthlyUsages = invoiceGroupUsagePair.Value;
				FeeUsage[] fees;
				if (invoiceGroupToFees != null && invoiceGroupToFees.TryGetValue(invoiceGroupUsagePair.Key, out fees))
				{
					protoBill.FeeUsages = fees;
					invoiceGroupToFees.Remove(invoiceGroupUsagePair.Key);
				}
			}
			if (invoiceGroupToFees != null)
			{
				foreach (var invoiceGroupFeesPair in invoiceGroupToFees)
				{
					var protoBill = new ProtoBill();
					protoBills.Add(protoBill);
					protoBill.FeeUsages = invoiceGroupFeesPair.Value;
				}
			}

			AssignFeesToDatabase(protoBills);

			var protoBillsWithBranch = protoBills.Where(x => !x.InvoiceGroup.BranchPK.IsEmpty);

			var branches = factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK,
				protoBillsWithBranch.Select(x => x.InvoiceGroup.BranchPK.ToGuid()).Distinct()));
			var companies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, branches.Select(x => x.GB_GC.ToGuid()).Distinct()));
			var companyPkToCompany = companies.ToDictionary(x => x.PK.ToGuid());
			var branchPkToCompany = branches.ToDictionary(x => x.PK.ToGuid(), y => companyPkToCompany[y.GB_GC.ToGuid()]);

			UpdateIsBackPostAvailable(branches);
			context.IsBackPost = IsBackPostAvailable && IsBackPostAllowed;
			var dateForExchangeRate = context.DateForExchangeRate;

			PrepareToGetCurrencyExchangeRates(context.CurrencyExchange, protoBills, branchPkToCompany);

			var databasePks = monthlyUsagesWithBranch.Where(x => x.Database != null).Select(x => x.Database.PK.ToGuid()).Distinct().ToArray();
			CalculatePrepaid(context, protoBillsWithBranch, branchPkToCompany, orgPkToPositiveDepositBalances);

			context.ProductBundles.Init(monthlyUsageList);
			CalculateDiscounts(context, monthlyUsageList);

			// Min spend uses the post discount amount - so must come after CalculateDiscounts
			ApplyAllMinSpendAdjustments(monthlyUsageList, priceItemPKFilter);

			CalculateFeeDiscounts(protoBills, databaseUsageSet);
			if (!(codesFilter.Length == 1 && codesFilter[0] == BillingConstants.BillingSystem.ODM))
			{
				CalculateCommitmentAmount(monthlyUsagesWithBranch, context.PeriodStart);
				CalculateMinimumFees(context.CurrencyExchange, monthlyUsagesWithBranch, branchPkToCompany, priceItemPKFilter);
			}

			using (Bills.SuspendListChanged())
			{
				foreach (var protoBill in protoBills)
				{
					var invoiceGroup = protoBill.InvoiceGroup;
					var bill = new StlBill(factory,
						factory.Load<GlbBranch>(invoiceGroup.BranchPK),
						factory.Load<EDIOrgHeader>(invoiceGroup.InvoicedOrganisationPK),
						invoiceGroup.Currency,
						context.PeriodStart,
						dateForExchangeRate,
						invoiceGroup.IsBilled);
					bill.AddMonthlyUsages(protoBill.MonthlyUsages);
					bill.AddFees(protoBill.FeeUsages);
					bill.HasPrepaid |= protoBill.HasPrepaid;
					bill.Prepaid = protoBill.Prepaid;
					bill.OutstandingBalanceExDepositLocal = protoBill.OutstandingBalanceExDepositLocal;
					bill.DepositAvailableLocal = protoBill.DepositAvailableLocal;
					Bills.Add(bill);
				}
			}

			ValidateBills(stlFees);
			AddBillsForBorderWiseErrors(context, dateForExchangeRate, databaseUsageSet.GetGenericUsageSet(ProductTypes.Codes.BorderWise) as IBorderWiseUsageSet);
			var allBills = Bills.Cast<StlBill>();
			CalculateCurrencyAndSurchargeAmounts(context, allBills);
			CalculateAlreadyInvoiced(allBills);
			CalculateLastMonthInvoice(allBills, databasePks, context);
			PreFetch(factory, allBills);

			ValidationResults.PopulateResults(Bills);
		}

		void AddBillsForBorderWiseErrors(BillingRunContext context, ZDateTime dateForExchangeRate, IBorderWiseUsageSet borderWiseUsages)
		{
			if (borderWiseUsages == null)
			{
				return;
			}

			foreach (EDIOrgHeader org in borderWiseUsages.GetUsingOrgsWithMissingDatabase(context))
			{
				var bill = new StlBill(FactoryForGenerate, null, org, "", context.PeriodStart, dateForExchangeRate);
				bill.AddRowError(org.OH_Code + (NoResString)" missing BorderWise Licence Database");
				Bills.Add(bill);
			}
		}

		void ValidateBills(StlFees stlFees)
		{
			ReadOnlyCodeDescriptionPairList feeDiscountChargeCodes = null;
			if (stlFees != null)
			{
				feeDiscountChargeCodes = EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.Value;
			}

			foreach (StlBill bill in Bills)
			{
				if (bill.InvoicingBranch == null)
				{
					bill.AddRowError(BillRecipient.ValidationMessages.NoBranch);
				}

				if (bill.Organisation != null && !bill.Organisation.OH_IsActive)
				{
					bill.AddRowError(BillRecipient.ValidationMessages.InactiveOrg);
				}

				foreach (var feeChargeCode in bill.FeeUsages.Where(x => x.DiscountAmount != 0).Select(x => (string)x.Fee.L8_ChargeCode).Distinct())
				{
					if (string.IsNullOrEmpty(feeDiscountChargeCodes.GetDescriptionFromCode(feeChargeCode)))
					{
						var regItem = EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes;
						bill.AddRowError((NoResString)"Fee charge code " + feeChargeCode + (NoResString)" missing discount charge code in registry "
							+ string.Join((NoResString)" > ", regItem.Categories) + (NoResString)" > " + regItem.Caption);
					}
				}
			}

			ValidateIsDebtor();
		}

		void ValidateIsDebtor()
		{
			foreach (var billsByInvoicingCompany in Bills.Cast<StlBill>()
				.Where(x => x.InvoicingBranch != null)
				.GroupBy(x => x.InvoicingBranch.GB_GC.ToGuid()))
			{
				var orgPkToIsDebtor = InvoicedOrgPkToIsDebtor(billsByInvoicingCompany.Select(x => x.Organisation.PK.ToGuid()).Distinct(), billsByInvoicingCompany.Key);

				foreach (var bill in billsByInvoicingCompany)
				{
					bool isDebtor;
					if (!orgPkToIsDebtor.TryGetValue(bill.Organisation.PK.ToGuid(), out isDebtor)
						|| !isDebtor)
					{
						bill.AddRowError(BillRecipient.ValidationMessages.NotDebtor);
					}
				}
			}
		}

		public static Dictionary<Guid, bool> InvoicedOrgPkToIsDebtor(IEnumerable<Guid> invoicedOrgPks, Guid companyPk)
		{
			var result = new Dictionary<Guid, bool>(invoicedOrgPks.Count());

			var paramName = invoicedOrgPks.Count() < 5 ? "@FewPKs" : "@ManyPKs";

			string sql = "select OB_OH, OB_IsDebtor from dbo.OrgCompanyData where OB_GC = " + companyPk.ToSqlGuid() + " and OB_OH in (select Value from " + paramName + ")";

			using (var pkParam = new GuidTableValuedParameter(invoicedOrgPks))
			using (var cmd = Db.Connection.Command(sql))
			{
				pkParam.AddTo(cmd, paramName);
				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var orgPk = reader.GetGuid(0);
						var isDebtor = reader.GetBoolean(1);
						result.Add(orgPk, isDebtor);
					}
				}
			}

			return result;
		}

		class ProtoBill
		{
			public FeeUsage[] FeeUsages { get; internal set; }
			public StlMonthlyUsage[] MonthlyUsages { get; internal set; }

			public InvoiceGroup InvoiceGroup { get { return MonthlyUsages != null ? MonthlyUsages[0].InvoiceGroup : FeeUsages[0].OwnerDelivery.InvoiceGroup; } }
			public ZGuid InvoicedOrganisationPK { get { return InvoiceGroup.InvoicedOrganisationPK; } }

			public bool HasPrepaid { get; internal set; }
			public bool HasFees { get { return FeeUsages != null; } }
			public bool HasMonthlyUsage { get { return MonthlyUsages != null; } }

			public decimal OutstandingBalanceExDepositLocal { get; set; }
			public decimal DepositAvailableLocal { get; set; }

			public StlBill.PrepaidDetail Prepaid = new StlBill.PrepaidDetail();
		}

		void AssignFeesToDatabase(List<ProtoBill> protoBills)
		{
			foreach (var bill in protoBills.Where(x => x.HasMonthlyUsage && x.HasFees))
			{
				var feeUsages = bill.FeeUsages;

				if (bill.MonthlyUsages.Length == 1)
				{
					bill.MonthlyUsages.First().AddFeeUsages(feeUsages);
				}
				else
				{
					// one bill is paying for multiple databases
					bool first = true;
					var databasePks = new HashSet<Guid>(bill.MonthlyUsages.Where(x => x.Database != null).Select(x => x.Database.PK.ToGuid()));
					foreach (var monthlyUsage in bill.MonthlyUsages)
					{
						monthlyUsage.AddFeeUsages(feeUsages
							.Where(x => (monthlyUsage.Database != null && x.Fee.L8_LD == monthlyUsage.Database.PK)
								|| (first && (x.Fee.L8_LD.IsEmpty || !databasePks.Contains(x.Fee.L8_LD.ToGuid()))))
							.ToArray());
						first = false;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void CalculateCommitmentAmount(IEnumerable<StlMonthlyUsage> monthlyUsagesWithBranch, ZDateTime periodStart)
		{
			CalculateSharedCommitments(monthlyUsagesWithBranch);

			foreach (var monthlyUsage in monthlyUsagesWithBranch.Where(x => x.Commitment != null))
			{
				if (monthlyUsage.Database?.LD_IsBilledPerCompany ?? false)
				{
					monthlyUsage.AddRowError((NoResString)"Commitment discount combined with per company billing is not supported.");
				}

				var commitment = monthlyUsage.Commitment;
				var commitmentGroup = monthlyUsage.CommitmentGroup;
				var usageUnits = commitmentGroup?.TotalUsedLicenceUnits ?? monthlyUsage.TotalUsedLicenceUnits;
				if (usageUnits > 0 && usageUnits < commitment.LicenceUnits)
				{
					var usageLinesToInclude = monthlyUsage.UsageLines.Where(x => x.LicenceUnits > 0 && !x.PriceCurrency.IsEmpty);
					if (usageLinesToInclude.Any())
					{
						string adjustmentCurrency = monthlyUsage.DatabaseUsage?.PriceHeaderLink?.PHL_RX_NKCurrency;
						if (adjustmentCurrency == null || !usageLinesToInclude.Any(x => x.PriceCurrency == adjustmentCurrency))
						{
							// choose currency with most licence units
							adjustmentCurrency = usageLinesToInclude
								.GroupBy(x => x.PriceCurrency)
								.OrderByDescending(x => x.Sum(y => y.LicenceUnits * y.TotalUnitCount))
								.First()
								.Key;
						}
						UsageLine firstUsageLineInAdjustmentCurrency = null;

						decimal adjustmentFactor = commitment.LicenceUnits / usageUnits;
						decimal totalLicenceUnitsInAdjustmentCurrency = 0;
						decimal totalPreDiscountInAdjustmentCurrency = 0;
						decimal totalPostDiscountInAdjustmentCurrency = 0;
						foreach (var usageLine in usageLinesToInclude)
						{
							if (usageLine.PriceCurrency == adjustmentCurrency)
							{
								if (firstUsageLineInAdjustmentCurrency == null)
								{
									firstUsageLineInAdjustmentCurrency = usageLine;
								}
								totalLicenceUnitsInAdjustmentCurrency += usageLine.LicenceUnits * usageLine.TotalUnitCount;
								totalPreDiscountInAdjustmentCurrency += usageLine.PreDiscountAmount;
								totalPostDiscountInAdjustmentCurrency += usageLine.PostDiscountAmount;
							}

							usageLine.AdjustAmounts(adjustmentFactor);
						}

						var priceItem = monthlyUsage.Factory.New<ClientLicencePriceItem>();
						var extraLicenceUnits = commitmentGroup?.Adjustment ?? (commitment.LicenceUnits - usageUnits);
						priceItem.L7_LicenceUnits = extraLicenceUnits;
						priceItem.SetPriceItemDescription(ResString.GetMultilingualString("b9d2b912-b152-4c5d-aa8f-15bfcd4d2825", "Adjustment To Reach Committed Discount Units"));
						priceItem.L7_Order = short.MaxValue;

						var roleItem = monthlyUsage.GetOrCreateCommitmentRoleItem();

						var preDiscountLicenceUnitRate = totalPreDiscountInAdjustmentCurrency / totalLicenceUnitsInAdjustmentCurrency;
						var postDiscountLicenceUnitRate = totalPostDiscountInAdjustmentCurrency / totalLicenceUnitsInAdjustmentCurrency;

						var commitmentLine = new UsageLine(monthlyUsage.Factory, periodStart, periodStart, "", priceItem, roleItem, null, null);
						commitmentLine.FormatOptions = FormatOptions.GetOptionsByProduct(monthlyUsage.Database?.LD_Product, monthlyUsage.Factory);
						commitmentLine.RoleIndex = UsageLine.CommitmentRoleIndex;
						commitmentLine.Discounts = firstUsageLineInAdjustmentCurrency.Discounts;
						commitmentLine.TotalUnitCount = 1;
						commitmentLine.Price = Utilities.Round(extraLicenceUnits * preDiscountLicenceUnitRate, BillingConstants.RoundingDecimals);
						commitmentLine.PriceCurrency = adjustmentCurrency;
						commitmentLine.DiscountedPrice = Utilities.Round(extraLicenceUnits * postDiscountLicenceUnitRate, BillingConstants.RoundingDecimals);
						commitmentLine.SetAmounts(commitmentLine.Price, commitmentLine.DiscountedPrice);
						monthlyUsage.AddCommitment(commitmentLine);
					}
				}
			}
		}

		static void CalculateSharedCommitments(IEnumerable<StlMonthlyUsage> monthlyUsagesWithBranch)
		{
			foreach (var commitmentGroup in monthlyUsagesWithBranch
				.Where(x => !string.IsNullOrEmpty(x.Commitment?.LS9_Name))
				.GroupBy(x => (string)x.Commitment.LS9_Name, StringComparer.OrdinalIgnoreCase))
			{
				var firstCommitment = commitmentGroup.First().Commitment;
				var commitmentLicenceUnits = firstCommitment.LicenceUnits;
				var groupTotal = commitmentGroup.Sum(x => x.TotalUsedLicenceUnits);
				var memberCount = commitmentGroup.Count();
				var totalAdjustment = Math.Max(0, commitmentLicenceUnits - groupTotal);
				var roundedAdjustmentPerMember = Utilities.Round(totalAdjustment / memberCount, 1);
				var adjustmentRemaining = totalAdjustment;
				int membersRemaining = memberCount;
				foreach (var monthlyUsage in commitmentGroup.OrderBy(x => x.EnterpriseCode).ThenBy(x => x.ServerCode))
				{
					--membersRemaining;
					var adjustment = membersRemaining != 0 ? roundedAdjustmentPerMember : adjustmentRemaining;
					adjustmentRemaining -= adjustment;
					monthlyUsage.CommitmentGroup = new SharedCommitment()
					{
						TotalUsedLicenceUnits = groupTotal,
						Adjustment = adjustment
					};
					if (commitmentLicenceUnits != monthlyUsage.Commitment.LicenceUnits)
					{
						var db = monthlyUsage.Database;
						var msg = FormattableString.Invariant($"Shared Commitment '{commitmentGroup.Key}': licence units of {monthlyUsage.Commitment.LicenceUnits} does not match {commitmentLicenceUnits} on Database Code {db.LD_ServerCode} of Enterprise Code {db.EnterpriseCode}");
						monthlyUsage.AddRowError(msg);
					}
				}
			}
		}

		void factory_Saving(BusinessObjectFactory factory)
		{
			throw new InvalidOperationException("Attempt to Save the report factory");
		}

		/// <summary>
		/// Calculate monthlyUsage.HasPrepaid, i.e., current balance >= previous month's undiscounted amount
		/// Determines if prepayment discount applies.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		static void CalculatePrepaid(BillingRunContext context,
			IEnumerable<ProtoBill> protoBillsWithBranch,
			Dictionary<Guid, GlbCompany> branchPkToCompany,
			Dictionary<Guid, IEnumerable<DepositBalance>> orgPkToPositiveDepositBalances)
		{
			decimal prepaymentMargin = EDIDataRegistry.Instance.PrepaymentMargin.Value;

			Guid[] invoicedOrgPks = protoBillsWithBranch
				.Where(x => !x.InvoicedOrganisationPK.IsEmpty)
				.Select(x => x.InvoicedOrganisationPK.ToGuid())
				.Distinct()
				.ToArray();

			var invoicedOrgPkToPrepayAmounts = InvoicedOrgPkToPrepayAmounts(invoicedOrgPks, context.PeriodStart.AddMonths(-1).ToDateTime());

			foreach (var protoBillByGlbCompany in protoBillsWithBranch
				.Where(x => !x.InvoicedOrganisationPK.IsEmpty)
				.GroupBy(x => branchPkToCompany[x.InvoiceGroup.BranchPK.ToGuid()]))
			{
				var localCompany = protoBillByGlbCompany.Key;

				var orgPkToOutstandingBalance = ARBalance.GetOutstandingBalance(
					protoBillByGlbCompany.Select(x => x.InvoicedOrganisationPK.ToGuid()),
					localCompany.PK.ToGuid(),
					ZDateTime.Today.ToDateTime());

				foreach (var protoBillByInvoicingOrg in protoBillByGlbCompany.GroupBy(x => x.InvoicedOrganisationPK.ToGuid()))
				{
					decimal outstandingBalanceIncTaxLocal;
					orgPkToOutstandingBalance.TryGetValue(protoBillByInvoicingOrg.Key, out outstandingBalanceIncTaxLocal);

					IEnumerable<DepositBalance> depositBalances;
					orgPkToPositiveDepositBalances.TryGetValue(protoBillByInvoicingOrg.Key, out depositBalances);

					decimal depositIncTaxLocalCurrency = 0;
					var monthlyUsages = protoBillByInvoicingOrg.Where(x => x.HasMonthlyUsage).SelectMany(x => x.MonthlyUsages);

					if (depositBalances != null)
					{
						if (monthlyUsages.Any())
						{
							HashSet<string> depositChargeCodes = new HashSet<string>();

							foreach (var usageLineWithDepositChargeCode in monthlyUsages
								.SelectMany(m => m.UsageLines.Where(x => x.PriceItem != null && !x.PriceItem.L7_DepositChargeCode.IsEmpty)))
							{
								depositChargeCodes.Add(usageLineWithDepositChargeCode.PriceItem.L7_DepositChargeCode);
							}

							foreach (var monthlyUsage in monthlyUsages.Where(x => x.ConversionCredit != null))
							{
								depositChargeCodes.Add(monthlyUsage.ConversionCredit.CreditChargeCode);
							}

							foreach (var depositChargeCode in depositChargeCodes)
							{
								var depositBalance = depositBalances.FirstOrDefault(x => x.ChargeCode == depositChargeCode);
								if (depositBalance != null)
								{
									var amountIncTaxLocalCurrency = depositBalance.Amount + depositBalance.Tax;
									if (depositBalance.CurrencyCode != localCompany.GC_RX_NKLocalCurrency)
									{
										amountIncTaxLocalCurrency = context.CurrencyExchange.GetAmount(depositBalance.CurrencyCode, localCompany.GC_RX_NKLocalCurrency, localCompany, amountIncTaxLocalCurrency);
									}
									depositIncTaxLocalCurrency += amountIncTaxLocalCurrency;
								}
							}
						}
					}

					string prepayCurrency = null;
					decimal prepayAmountRequired = 0;
					var prepaidBalanceIncTaxLocal = outstandingBalanceIncTaxLocal + depositIncTaxLocalCurrency;
					decimal previousLocalToPrepayExchangeRateMultiplier = 0;

					List<UsageInvoiceSummary> previousInvoiceSummaries;
					if (invoicedOrgPkToPrepayAmounts.TryGetValue(protoBillByInvoicingOrg.Key, out previousInvoiceSummaries))
					{
						var previousInvoiceSummary = previousInvoiceSummaries
							.FirstOrDefault(x => x.DatabasePk.IsEmpty
								|| monthlyUsages.Any(m => m.Database != null && m.Database.PK == x.DatabasePk)
								|| protoBillByInvoicingOrg.Any(b => b.FeeUsages != null && b.FeeUsages.Any(f => f.Fee.L8_LD == x.DatabasePk)));
						if (previousInvoiceSummary != null)
						{
							prepayCurrency = previousInvoiceSummary.PrepayCurrency;
							var lastAmountIncTax = previousInvoiceSummary.PrepayAmountIncTax;
							prepayAmountRequired = lastAmountIncTax;
							previousLocalToPrepayExchangeRateMultiplier = previousInvoiceSummary.IsReciprocal ? 1.0m / previousInvoiceSummary.ExchangeRate : previousInvoiceSummary.ExchangeRate;
						}
					}

					if (prepayCurrency == null)
					{
						// if there's no record of last months prepayment predictions, use half this months amount
						prepayCurrency = localCompany.GC_RX_NKLocalCurrency;
						var preDiscountUsageInPrepayCurrency = monthlyUsages.Sum(x => x.UsageLines.Sum(u => context.CurrencyExchange.GetAmount(u.PriceCurrency, prepayCurrency, localCompany, u.PreDiscountAmount)));
						var preDiscountFeeUsageInPrepayCurrency = protoBillByInvoicingOrg
								.Where(b => b.FeeUsages != null)
								.SelectMany(b => b.FeeUsages)
								.Sum(feeUsage => context.CurrencyExchange.GetAmount(feeUsage.Currency, prepayCurrency, localCompany, feeUsage.PreDiscountAmount));
						prepayAmountRequired = (preDiscountUsageInPrepayCurrency + preDiscountFeeUsageInPrepayCurrency) / 2;
					}

					if (prepayCurrency != null)
					{
						var prepaidBalanceIncTaxPrepayCurrency = context.CurrencyExchange.GetAmount(localCompany.GC_RX_NKLocalCurrency, prepayCurrency, localCompany, prepaidBalanceIncTaxLocal);
						if (previousLocalToPrepayExchangeRateMultiplier != 0)
						{
							var balanceUsingPreviousExchangeRate = prepaidBalanceIncTaxLocal * previousLocalToPrepayExchangeRateMultiplier;
							if (prepaidBalanceIncTaxPrepayCurrency < balanceUsingPreviousExchangeRate)
							{
								prepaidBalanceIncTaxPrepayCurrency = balanceUsingPreviousExchangeRate;
							}
						}

						bool hasPrepaid = prepaidBalanceIncTaxPrepayCurrency > 0 && prepaidBalanceIncTaxPrepayCurrency > prepayAmountRequired - prepaymentMargin;
						foreach (var protoBill in protoBillByInvoicingOrg)
						{
							protoBill.HasPrepaid = hasPrepaid;
							protoBill.Prepaid.PrepayCurrency = prepayCurrency;
							protoBill.Prepaid.BalanceActualIncTax = prepaidBalanceIncTaxPrepayCurrency;
							protoBill.Prepaid.BalanceRequiredIncTax = prepayAmountRequired;
							protoBill.OutstandingBalanceExDepositLocal = outstandingBalanceIncTaxLocal;
							protoBill.DepositAvailableLocal = depositIncTaxLocalCurrency;
						}

						foreach (var monthlyUsage in monthlyUsages)
						{
							monthlyUsage.HasPrepaid = hasPrepaid;
						}
					}

					if (context.PeriodStart >= new ZDateTime(2017, 7, 1))
					{
						ApplyPrepaymentDiscountAfter20170701(context, protoBillByInvoicingOrg.Key, protoBillByInvoicingOrg, monthlyUsages, localCompany, orgPkToPositiveDepositBalances);
					}
					else
					{
						ApplyPrepaymentDiscountBefore20170701(context, protoBillByInvoicingOrg);
					}
				}
			}
		}

		/// <summary>
		/// New Prepayment Policy (201707)
		/// If the prepaid balance equals or exceed the predetermined prepaid balance OR ManualOverride then the discount applies.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void ApplyPrepaymentDiscountAfter20170701(BillingRunContext context,
			Guid invoicingOrgPK,
			IEnumerable<ProtoBill> protoBills,
			IEnumerable<StlMonthlyUsage> monthlyUsages,
			GlbCompany localCompany,
			Dictionary<Guid, IEnumerable<DepositBalance>> orgPkToPositiveDepositBalances)
		{
			Func<string, decimal, decimal> getAmountInLocalCurrency = (sourceCurrency, sourceAmount) =>
			{
				return (sourceAmount == 0m || sourceCurrency == localCompany.GC_RX_NKLocalCurrency) ?
					sourceAmount :
					context.CurrencyExchange.GetAmount(sourceCurrency, localCompany.GC_RX_NKLocalCurrency, localCompany, sourceAmount);
			};

			var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
			var invoicingCompany = context.CompanyDeliveries.GetCompanyByOrgPk(invoicingOrgPK);
			var licenceBilling = invoicingCompany?.ReadonlySelfBilling;

			var isManualOverride = monthlyUsages.Any(x => x.DatabaseUsage?.HasManualPrepaymentDiscountSetting ?? false);

			var predeterminedPrepaidBalance = licenceBilling?.L4_PredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var predeterminedPrepaidCurrency = licenceBilling?.L4_RX_NKPredeterminedPrepaidBalanceCurrency ?? ZString.Empty;
			var futurePredeterminedPrepaidBalance = licenceBilling?.L4_FuturePredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var futurePredeterminedPrepaidCurrency = licenceBilling?.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency ?? ZString.Empty;

			var prepaidDepositBalance = 0m;
			var prepaidDepositCurrency = ZString.Empty;
			IEnumerable<DepositBalance> prepaidBalances;
			if (orgPkToPositiveDepositBalances.TryGetValue(invoicingOrgPK, out prepaidBalances))
			{
				var prepaidBalance = prepaidBalances.FirstOrDefault(x => x.ChargeCode == prepaidBalanceChargeCode);
				if (prepaidBalance != null)
				{
					prepaidDepositBalance = prepaidBalance.Amount + prepaidBalance.Tax;
					prepaidDepositCurrency = prepaidBalance.CurrencyCode;
				}
			}
			var isPrepaidGreaterThanPredetermined = false;

			if (predeterminedPrepaidBalance > 0m && prepaidDepositBalance > 0m)
			{
				if (predeterminedPrepaidCurrency == prepaidDepositCurrency)
				{
					isPrepaidGreaterThanPredetermined = prepaidDepositBalance >= predeterminedPrepaidBalance;
				}
				else
				{
					var predeterminedPrepaidBalanceLocal = getAmountInLocalCurrency(predeterminedPrepaidCurrency, predeterminedPrepaidBalance);
					var prepaidDepositBalanceLocal = getAmountInLocalCurrency(prepaidDepositCurrency, prepaidDepositBalance);
					isPrepaidGreaterThanPredetermined = prepaidDepositBalanceLocal >= predeterminedPrepaidBalanceLocal;
				}
			}

			var hasPrepaid = (isPrepaidGreaterThanPredetermined || isManualOverride);

			foreach (var protoBill in protoBills)
			{
				protoBill.HasPrepaid = hasPrepaid;
				protoBill.Prepaid.ShouldUseOutstandingBalanceAsPrepaidDepositBalance = false;
				protoBill.Prepaid.PredeterminedPrepaidBalanceCurrency = predeterminedPrepaidCurrency;
				protoBill.Prepaid.PredeterminedPrepaidBalance = predeterminedPrepaidBalance;
				protoBill.Prepaid.FuturePredeterminedPrepaidBalanceCurrency = futurePredeterminedPrepaidCurrency;
				protoBill.Prepaid.FuturePredeterminedPrepaidBalance = futurePredeterminedPrepaidBalance;
				protoBill.Prepaid.PrepaidDepositBalanceCurrency = prepaidDepositCurrency;
				protoBill.Prepaid.PrepaidDepositBalance = prepaidDepositBalance;
			}

			foreach (var monthlyUsage in monthlyUsages)
			{
				monthlyUsage.HasPrepaid = hasPrepaid;
			}
		}

		//TO BE DELETED AFTER 20170701
		static void ApplyPrepaymentDiscountBefore20170701(BillingRunContext context, IGrouping<Guid, ProtoBill> protoBillByInvoicingOrg)
		{
			var invoicingOrg = context.Factory.Load<EDIOrgHeader>(protoBillByInvoicingOrg.Key);
			var predeterminedBalance = invoicingOrg.LicCompany?.SelfBilling?.L4_PredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var predeterminedCurrency = invoicingOrg.LicCompany?.SelfBilling?.L4_RX_NKPredeterminedPrepaidBalanceCurrency ?? ZString.Empty;
			var futurePredeterminedPrepaidBalance = invoicingOrg.LicCompany?.SelfBilling?.L4_FuturePredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var futurePredeterminedPrepaidCurrency = invoicingOrg.LicCompany?.SelfBilling?.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency ?? ZString.Empty;

			foreach (var protoBill in protoBillByInvoicingOrg)
			{
				protoBill.Prepaid.ShouldUseOutstandingBalanceAsPrepaidDepositBalance = true;
				protoBill.Prepaid.PrepaidDepositBalanceCurrency = "";
				protoBill.Prepaid.PrepaidDepositBalance = 0m;
				protoBill.Prepaid.PredeterminedPrepaidBalanceCurrency = predeterminedCurrency;
				protoBill.Prepaid.PredeterminedPrepaidBalance = predeterminedBalance;
				protoBill.Prepaid.FuturePredeterminedPrepaidBalanceCurrency = futurePredeterminedPrepaidCurrency;
				protoBill.Prepaid.FuturePredeterminedPrepaidBalance = futurePredeterminedPrepaidBalance;
			}
		}

		static void PrepareToGetCurrencyExchangeRates(CurrencyExchangeService currencyExchange, IEnumerable<ProtoBill> protoBills, Dictionary<Guid, GlbCompany> branchPkToCompany)
		{
			foreach (var protoBill in protoBills)
			{
				var invoiceGroup = protoBill.InvoiceGroup;
				if (invoiceGroup != null && !invoiceGroup.BranchPK.IsEmpty && !invoiceGroup.Currency.IsEmpty)
				{
					var company = branchPkToCompany[invoiceGroup.BranchPK.ToGuid()];
					currencyExchange.PrepareToGetRateFor(company.GC_RX_NKLocalCurrency, invoiceGroup.Currency, company);

					if (protoBill.HasMonthlyUsage)
					{
						foreach (var monthlyUsage in protoBill.MonthlyUsages)
						{
							foreach (var priceCurrency in monthlyUsage.PriceCurrencies)
							{
								currencyExchange.PrepareToGetRateFor(priceCurrency, invoiceGroup.Currency, company);
								currencyExchange.PrepareToGetRateFor(priceCurrency, company.GC_RX_NKLocalCurrency, company);
							}
						}
					}

					if (protoBill.HasFees)
					{
						foreach (var feeUsage in protoBill.FeeUsages)
						{
							currencyExchange.PrepareToGetRateFor(feeUsage.Fee.L8_RX_NKCurrency, invoiceGroup.Currency, company);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static void CalculateCurrencyAndSurchargeAmounts(BillingRunContext context, IEnumerable<StlBill> bills)
		{
			var versionHistory = new VersionHistory();

			var currencyExchange = context.CurrencyExchange;

			Dictionary<string, HashSet<string>> missingExchangeRates = new Dictionary<string, HashSet<string>>();
			var billsGroupedByInvoicingCompany = bills.Where(x => x.InvoicingBranch != null && !x.InvoiceCurrencyCode.IsEmpty).GroupBy(x => x.InvoicingBranch.Company);
			foreach (var billsPerInvoicingCompany in billsGroupedByInvoicingCompany)
			{
				GlbCompany invoicingCompany = billsPerInvoicingCompany.Key;
				string localCurrency = invoicingCompany.GC_RX_NKLocalCurrency;

				foreach (var billsPerInvoiceCurrency in billsPerInvoicingCompany.GroupBy(x => x.InvoiceCurrencyCode))
				{
					string invoiceCurrency = billsPerInvoiceCurrency.Key;
					var invoiceCurrencyDecimals = currencyExchange.GetCurrencyByCode(invoiceCurrency).Decimals;
					decimal localExchangeRate = currencyExchange.GetRate(localCurrency, invoiceCurrency, invoicingCompany);

					foreach (var bill in billsPerInvoiceCurrency)
					{
						decimal preDiscountTotal = 0;
						decimal postDiscountTotal = 0;
						decimal versionSurchargeTotal = 0;

						missingExchangeRates.Clear();

						bill.LocalExchangeRate = invoicingCompany.GC_IsReciprocal && localExchangeRate != 0 ? Utilities.Round(1 / localExchangeRate, 5) : localExchangeRate;

						if (localExchangeRate == 0)
						{
							AddCurrencyExchange(missingExchangeRates, localCurrency, invoiceCurrency);
						}

						foreach (StlMonthlyUsage monthlyUsage in bill.MonthlyUsages)
						{
							CalculateVersionSurchargePercent(versionHistory, monthlyUsage);

							decimal databaseUsagePostDiscountAmountInInvoiceCurrency = 0;

							foreach (var priceCurrency in monthlyUsage.PriceCurrencies)
							{
								decimal localCurrencyRateAsMultiplier = 1m;
								if (!string.IsNullOrEmpty(priceCurrency) && priceCurrency != localCurrency)
								{
									localCurrencyRateAsMultiplier = currencyExchange.GetRate(priceCurrency, localCurrency, invoicingCompany);
									if (localCurrencyRateAsMultiplier == 0)
									{
										AddCurrencyExchange(missingExchangeRates, priceCurrency, localCurrency);
									}
								}

								decimal invoiceCurrencyRateAsMultiplier = 1m;
								if (priceCurrency != invoiceCurrency)
								{
									invoiceCurrencyRateAsMultiplier = currencyExchange.GetRate(priceCurrency, invoiceCurrency, invoicingCompany);
									if (invoiceCurrencyRateAsMultiplier == 0)
									{
										AddCurrencyExchange(missingExchangeRates, priceCurrency, invoiceCurrency);
									}
									else
									{
										bill.AddPriceToInvoiceExchangeRate(priceCurrency, invoiceCurrencyRateAsMultiplier);
									}
								}

								foreach (var usageLine in monthlyUsage.UsageLines.Where(x => x.PriceCurrency == priceCurrency))
								{
									// price currency amounts
									decimal preDiscountAmount = usageLine.PreDiscountAmount;
									decimal postDiscountAmount = usageLine.PostDiscountAmount;

									// local currency amounts
									usageLine.LocalCurrency = localCurrency;
									usageLine.LocalPreDiscountAmount = preDiscountAmount * localCurrencyRateAsMultiplier;
									usageLine.LocalPostDiscountAmount = postDiscountAmount * localCurrencyRateAsMultiplier;
									usageLine.LocalDiscountAmount = usageLine.LocalPostDiscountAmount - usageLine.LocalPreDiscountAmount;

									usageLine.InvoiceCurrencyRateAsMultiplier = invoiceCurrencyRateAsMultiplier;
									usageLine.InvoiceCurrencyDecimals = invoiceCurrencyDecimals;

									// Non-Current Version surcharge for each line
									if (monthlyUsage.VersionSurchargePercent != 0)
									{
										usageLine.VersionSurchargeAmount = usageLine.PostDiscountAmount * monthlyUsage.VersionSurchargePercent / 100m;
										usageLine.LocalVersionSurchargeAmount = usageLine.VersionSurchargeAmount * localCurrencyRateAsMultiplier;
									}
								}

								// invoice currency amounts
								foreach (var usageLinesByChargeCodes in monthlyUsage.UsageLines.Where(x => x.PriceCurrency == priceCurrency)
									.GroupBy(x => new
									{
										ChargeCode = (string)x.PriceItem.L7_ChargeCode,
										DepositChargeCode = (string)x.PriceItem.L7_DepositChargeCode, // TODO use blank if no balance
										DiscountChargeCode = x.PreDiscountAmount != x.PostDiscountAmount ? "DISCSTL" : ""
									}))
								{
									decimal preDiscount = usageLinesByChargeCodes.Sum(x => x.PreDiscountAmount);
									decimal postDiscount = usageLinesByChargeCodes.Sum(x => x.PostDiscountAmount);
									preDiscount = Utilities.Round(preDiscount * invoiceCurrencyRateAsMultiplier, invoiceCurrencyDecimals);
									postDiscount = Utilities.Round(postDiscount * invoiceCurrencyRateAsMultiplier, invoiceCurrencyDecimals);
									preDiscountTotal += preDiscount;
									postDiscountTotal += postDiscount;
									databaseUsagePostDiscountAmountInInvoiceCurrency += postDiscount;
								}
							}

							if (monthlyUsage.VersionSurchargePercent != 0)
							{
								// Note, not including fees in Non-Current Version Surcharge. Can't tell if they belong to the surcharge agreement or a separate agreement.
								var unrounded = databaseUsagePostDiscountAmountInInvoiceCurrency * monthlyUsage.VersionSurchargePercent / 100m;
								monthlyUsage.VersionSurchargeAmountInInvoiceCurrency = Utilities.Round(unrounded, invoiceCurrencyDecimals);
								versionSurchargeTotal += monthlyUsage.VersionSurchargeAmountInInvoiceCurrency;
							}
						}

						foreach (var feeUsageByCurrency in bill.FeeUsages.GroupBy(x => x.Currency))
						{
							var invoiceCurrencyRateAsMultiplier = currencyExchange.GetRate(feeUsageByCurrency.Key, invoiceCurrency, invoicingCompany);

							if (invoiceCurrencyRateAsMultiplier == 0)
							{
								AddCurrencyExchange(missingExchangeRates, feeUsageByCurrency.Key, invoiceCurrency);
							}
							else
							{
								bill.AddPriceToInvoiceExchangeRate(feeUsageByCurrency.Key, invoiceCurrencyRateAsMultiplier);

								foreach (var feeUsage in feeUsageByCurrency)
								{
									preDiscountTotal += Utilities.Round(feeUsage.PreDiscountAmount * invoiceCurrencyRateAsMultiplier, invoiceCurrencyDecimals);
									postDiscountTotal += Utilities.Round(feeUsage.PostDiscountAmount * invoiceCurrencyRateAsMultiplier, invoiceCurrencyDecimals);
								}
							}
						}

						// Process fee surcharge applies to an amount that includes the Non-Current Version surcharge since it also has to be processed...
						var amountForProcessingSurcharge = postDiscountTotal + versionSurchargeTotal;
						ZDecimal processingSurchargeTotal = Utilities.Round(amountForProcessingSurcharge * bill.SurchargePercent / 100m, invoiceCurrencyDecimals);
						bill.InvoicePreDiscountTotal = preDiscountTotal;
						bill.InvoicePostDiscountTotal = amountForProcessingSurcharge + processingSurchargeTotal;
						bill.InvoiceSurchargeTotal = processingSurchargeTotal;
						bill.VersionSurchargeTotal = versionSurchargeTotal;
						bill.AudInvoicePostDiscountTotal = currencyExchange.GetAmount(invoiceCurrency, "AUD", invoicingCompany, bill.InvoicePostDiscountTotal);

						var dateForExchangeRate = context.DateForExchangeRate;
						foreach (var pair in missingExchangeRates)
						{
							foreach (var dest in pair.Value)
							{
								bill.AddRowError((NoResString)"Login company " + invoicingCompany.GC_Code + (NoResString)": exchange rate for " + dateForExchangeRate.ToShortDateString() + (NoResString)" not found for " + pair.Key + (NoResString)" to " + dest);
							}
						}
					}
				}
			}
		}

		static void CalculateVersionSurchargePercent(VersionHistory versionHistory, StlMonthlyUsage monthlyUsage)
		{
			var db = monthlyUsage.Factory.Load<LicenceDatabase>(monthlyUsage.Database.PK);
			if (db != null)
			{
				var versionSurchargeSetting = monthlyUsage.DatabaseUsage.GetVersionSurchargeLicenceSetting();
				var versionSurcharge = versionHistory.GetVersionSurcharge(db, versionSurchargeSetting, monthlyUsage.PeriodStart.ToDateTime());
				if (versionSurcharge != null && versionSurcharge.SurchargePercent > 0)
				{
					monthlyUsage.VersionSurchargePercent = versionSurcharge.SurchargePercent;
					monthlyUsage.VersionSurchargeNonCurrentVersion = versionSurcharge.NonCurrentVersion;
					monthlyUsage.Bill.AddRowWarning($"Non-Current Version Surcharge: ({versionSurcharge.NonCurrentVersion}) ({monthlyUsage.VersionSurchargePercent}%)");
				}
			}
		}

		static void AddCurrencyExchange(Dictionary<string, HashSet<string>> sourceToDestination, string sourceCurrency, string destinationCurrency)
		{
			if (sourceCurrency != destinationCurrency)
			{
				HashSet<string> destinationSet;
				if (!sourceToDestination.TryGetValue(sourceCurrency, out destinationSet))
				{
					destinationSet = new HashSet<string>();
					sourceToDestination.Add(sourceCurrency, destinationSet);
				}
				destinationSet.Add(destinationCurrency);
			}
		}

		static void PreFetch(BusinessObjectFactory factory, IEnumerable<StlBill> bills)
		{
			// A/R contact
			var orgPkToContact = ARContactHelper.GetContacts(factory, bills.Select(x => x.Organisation.PK.ToGuid()));
			foreach (StlBill bill in bills)
			{
				OrgContact contact;
				orgPkToContact.TryGetValue(bill.Organisation.PK.ToGuid(), out contact);
				bill.Contact = contact;
			}
		}

		static void CalculateAlreadyInvoiced(IEnumerable<StlBill> bills)
		{
			var billInvoiceList = bills.Select(b =>
			new
			{
				Bill = b,
				InvoicesPKs = b.Usages
						.Select(x => x.InvoicePk)
						.Concat(b.FeeUsages.Select(x => x.InvoicePk))
						.Where(x => !x.IsEmpty)
						.Distinct()
						.Select(x => x.ToGuid())
						.ToArray()
			}).ToArray();

			var nonCancelledInvoicePkToSummary = GetNonCancelledInvoicePkToSummary(billInvoiceList.SelectMany(x => x.InvoicesPKs).Distinct());

			foreach (var bill in billInvoiceList)
			{
				var summaries = bill.InvoicesPKs.Select(x => nonCancelledInvoicePkToSummary.TryGetValue(x, out var summary) ? summary : null)
				.Where(x => x != null)
				.OrderBy(x => x.AH_TransactionNum);
				bill.Bill.SetInvoicesForThisMonth(summaries);
			}
		}

		static void CalculateLastMonthInvoice(IEnumerable<StlBill> bills, Guid[] databasePks, BillingRunContext context)
		{
			var databasePkToInvoicedOrgPkToInvoice = GetDatabasePkToInvoicedOrgPkToInvoice(databasePks, context.PeriodStart.AddMonths(-1).ToDateTime());

			foreach (StlBill bill in bills)
			{
				foreach (var monthlyUsage in bill.MonthlyUsages.Where(x => x.Database != null && !x.InvoicedOrganisationPK.IsEmpty))
				{
					if (databasePkToInvoicedOrgPkToInvoice.TryGetValue(monthlyUsage.Database.PK.ToGuid(), out var invoicedOrgPkToInvoice)
						&& invoicedOrgPkToInvoice.TryGetValue(monthlyUsage.InvoicedOrganisationPK.ToGuid(), out var invoiceSummary))
					{
						bill.SetInvoicesForLastMonth(invoiceSummary);
						break;
					}
				}
			}
		}

		class UsageInvoiceSummary
		{
			public UsageInvoiceSummary(ZGuid databasePk, string prepayCurrency, decimal prepayAmount, string localCurrency, decimal exchangeRate, bool isReciprocal)
			{
				DatabasePk = databasePk;
				PrepayCurrency = prepayCurrency;
				PrepayAmountIncTax = prepayAmount;
				ExchangeRate = exchangeRate;
				LocalCurrency = localCurrency;
				IsReciprocal = isReciprocal;
			}
			public ZGuid DatabasePk { get; private set; }
			public string PrepayCurrency { get; private set; }
			public string LocalCurrency { get; private set; }
			public decimal PrepayAmountIncTax { get; private set; }
			public decimal ExchangeRate { get; private set; }
			public bool IsReciprocal { get; private set; }
		}

		static Dictionary<Guid, List<UsageInvoiceSummary>> InvoicedOrgPkToPrepayAmounts(IEnumerable<Guid> invoicedOrgPks, DateTime periodStart)
		{
			var orgPkToAmounts = new Dictionary<Guid, List<UsageInvoiceSummary>>();
			if (invoicedOrgPks.Any())
			{
				var paramName = invoicedOrgPks.Count() < 5 ? "@FewPKs" : "@ManyPKs";

				string sqlTemplate = @"
select BU9_LD, AH_OH
	, EUI_PrepayAmountIncTax
	, TransactionCurrency = AH_RX_NKTransactionCurrency
	, AH_ExchangeRate
	, LocalCurrency = GC_RX_NKLocalCurrency
	, IsReciprocal = GC_IsReciprocal
from dbo.EdiUsageInvoice
join dbo.AccTransactionHeader on EUI_AH_Invoice = AH_PK
join dbo.GlbCompany on AH_GC = GC_PK
join
(
	select BU9_LD, BU9_AH_Invoice
	from dbo.EdiBilledUsage
	group by BU9_AH_Invoice, BU9_LD
) b on b.BU9_AH_Invoice = EUI_AH_Invoice
where AH_IsCancelled = 0 and EUI_PeriodStart = '{0}' 
	and AH_OH in (select Value from " + paramName + @")";

				string sql = string.Format(CultureInfo.InvariantCulture, sqlTemplate, periodStart.ToString("yyyy-M-dd", CultureInfo.InvariantCulture));

				using (var pkParam = new GuidTableValuedParameter(invoicedOrgPks))
				using (var cmd = Db.Connection.Command(sql))
				{
					pkParam.AddTo(cmd, paramName);
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
					{
						while (reader.Read())
						{
							object dbpk = reader[0];
							var databasePk = dbpk == DBNull.Value ? ZGuid.Empty : new ZGuid(dbpk);
							var orgPk = reader.GetGuid(1);
							var amount = reader.GetDecimal(2);
							var transactionCurrency = reader.GetString(3);
							var exchangeRate = reader.GetDecimal(4);
							var localCurrency = reader.GetString(5);
							var isReciprocal = reader.GetBoolean(6);
							List<UsageInvoiceSummary> amountList;
							if (!orgPkToAmounts.TryGetValue(orgPk, out amountList))
							{
								amountList = new List<UsageInvoiceSummary>(2);
								orgPkToAmounts.Add(orgPk, amountList);
							}
							amountList.Add(new UsageInvoiceSummary(databasePk, transactionCurrency, amount, localCurrency, exchangeRate, isReciprocal));
						}
					}
				}
			}

			return orgPkToAmounts;
		}

		static Dictionary<Guid, Dictionary<Guid, List<StlBill.InvoiceSummary>>> GetDatabasePkToInvoicedOrgPkToInvoice(Guid[] databasePks, DateTime periodStart)
		{
			var result = new Dictionary<Guid, Dictionary<Guid, List<StlBill.InvoiceSummary>>>();
			if (databasePks.Length > 0)
			{
				var paramName = databasePks.Length < 5 ? "@FewPKs" : "@ManyPKs";
				var postDateStart = periodStart.AddMonths(1).AddDays(-1);
				var postDateEnd = periodStart.AddMonths(2).AddDays(-2);

				string sql = @"
select distinct U1_LD, AH_OH, U1_AH_Invoice, AH_TransactionNum, AH_OSExTaxAmount = ROUND(AH_OSTotal - AH_GSTAmount * AH_ExchangeRate, 2)
from dbo.ClientChargeableUsage
join dbo.AccTransactionHeader on U1_AH_Invoice = AH_PK
where AH_IsCancelled = 0
	and U1_AH_Invoice is not null
	and u1_PeriodStart = @PeriodStart
	and u1_ld in (select Value from " + paramName + @")
	and u1_Code not in ('PUR')
	and (AH_PostDate between @PostDateStart and @PostDateEnd);  
";
				using (var pkParam = new GuidTableValuedParameter(databasePks))
				using (var cmd = Db.Connection.Command(sql))
				{
					pkParam.AddTo(cmd, paramName);
					cmd.AddParameter("@PeriodStart", SqlDbType.SmallDateTime, periodStart.Date);
					cmd.AddParameter("@PostDateStart", SqlDbType.SmallDateTime, postDateStart.Date);
					cmd.AddParameter("@PostDateEnd", SqlDbType.SmallDateTime, postDateEnd.Date);
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
					{
						while (reader.Read())
						{
							int index = 0;
							Guid databasePk = reader.GetGuid(index++);
							var orgPk = reader.GetGuid(index++);

							if (!result.TryGetValue(databasePk, out var invoicedOrgPkToInvoice))
							{
								invoicedOrgPkToInvoice = new Dictionary<Guid, List<StlBill.InvoiceSummary>>(1);
								result.Add(databasePk, invoicedOrgPkToInvoice);
							}

							if (!invoicedOrgPkToInvoice.TryGetValue(orgPk, out var invoiceSummaries))
							{
								invoiceSummaries = new List<StlBill.InvoiceSummary>();
								invoicedOrgPkToInvoice.Add(orgPk, invoiceSummaries);
							}

							var summary = new StlBill.InvoiceSummary();
							summary.PK = reader.GetGuid(index++);
							summary.AH_TransactionNum = reader.GetString(index++);
							summary.AH_OSExTaxAmount = reader.GetDecimal(index++);
							invoiceSummaries.Add(summary);
						}
					}
				}
			}

			return result;
		}

		static Dictionary<Guid, StlBill.InvoiceSummary> GetNonCancelledInvoicePkToSummary(IEnumerable<Guid> invoicePks)
		{
			var result = new Dictionary<Guid, StlBill.InvoiceSummary>();
			if (invoicePks.Any())
			{
				string sql = @"select " + AccTransactionHeaderSchema.Constants.PK
					+ ", " + AccTransactionHeaderSchema.Constants.AH_TransactionNum
					+ ", " + AccTransactionHeaderSchema.Constants.AH_InvoiceAmount
					+ " from " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName
					+ " where " + AccTransactionHeaderSchema.Constants.AH_IsCancelled + " = 0"
					+ " and " + AccTransactionHeaderSchema.Constants.PK + " in ('" + string.Join("', '", invoicePks) + "')";

				using (var cmd = Db.Connection.Command(sql))
				{
					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
					{
						while (reader.Read())
						{
							var summary = new StlBill.InvoiceSummary();
							summary.PK = reader.GetGuid(0);
							summary.AH_TransactionNum = reader.GetString(1);
							summary.AH_OSExTaxAmount = reader.GetDecimal(2);
							result.Add(summary.PK.ToGuid(), summary);
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public static void CalculateDiscounts(BillingRunContext context, IEnumerable<StlMonthlyUsage> monthlyUsageList)
		{
			// buying groups
			Dictionary<string, BuyingGroupVolume> buyingGroups = new Dictionary<string, BuyingGroupVolume>(StringComparer.OrdinalIgnoreCase);
			foreach (var monthlyUsage in monthlyUsageList)
			{
				var buyingGroup = monthlyUsage.DatabaseUsage.GetBuyingGroupSetting();
				if (buyingGroup != null && !buyingGroup.LS9_Name.IsEmpty)
				{
					BuyingGroupVolume groupVolume;
					if (!buyingGroups.TryGetValue(buyingGroup.LS9_Name, out groupVolume))
					{
						groupVolume = new BuyingGroupVolume();
						buyingGroups.Add(buyingGroup.LS9_Name, groupVolume);
					}
					monthlyUsage.BuyingGroup = groupVolume;
					groupVolume.TotalLicenceUnits += monthlyUsage.TotalLicenceUnits;
				}
			}

			var availableDiscountsForDatabase = new List<IStlDiscount>();
			var availableDiscountsForStructure = new List<IStlDiscount>();
			var discountsForPriceItem = new List<IStlDiscount>();
			var discountSetsForDatabase = new Dictionary<ulong, DiscountSet>();
			var discountFinder = new StlDiscountFinder();

			foreach (var monthlyUsage in monthlyUsageList)
			{
				foreach (var usageLineByDiscountVersion in monthlyUsage.UsageLines
					.Where(x => x.ShouldApplyDiscounts
						&& x.PriceItem != null
						&& !x.PriceItem.L7_PGM_DiscountGroupCode.IsEmpty)
					.GroupBy(x => x.DiscountVersionCode))
				{
					var discountVersion = usageLineByDiscountVersion.Key;
					if (!discountVersion.IsEmpty)
					{
						var usageDiscounts = monthlyUsage.DatabaseUsage.GetDiscountVersion(discountVersion);
						var priceHeaderLink = monthlyUsage.DatabaseUsage.PriceHeaderLink;
						availableDiscountsForDatabase.Clear();
						BuildAvailableDiscountsForDatabase(context, availableDiscountsForDatabase, monthlyUsage, usageDiscounts, discountFinder);

						if (availableDiscountsForDatabase.Count > 64)
						{
							throw new InvalidOperationException("Limit of 64 discounts per customer exceeded: "
								+ availableDiscountsForDatabase.Count.ToString(CultureInfo.InvariantCulture)
								+ " (DB# " + ((int)monthlyUsage.Database.LD_DatabaseNumber).ToString(CultureInfo.InvariantCulture) + ")");
						}

						discountSetsForDatabase.Clear();
						foreach (var usageLineByDiscountStructure in usageLineByDiscountVersion.GroupBy(x => (string)x.PriceItem.L7_PGM_DiscountGroupCode, StringComparer.OrdinalIgnoreCase))
						{
							var discountStructure = usageLineByDiscountStructure.Key;
							availableDiscountsForStructure.Clear();

							for (int i = 0; i < availableDiscountsForDatabase.Count; ++i)
							{
								var discount = availableDiscountsForDatabase[i];
								availableDiscountsForStructure.Add(usageDiscounts.GroupContains(discountStructure, discount.HeaderDiscount)
									? discount
									: null);
							}

							foreach (var usageLine in usageLineByDiscountStructure)
							{
								discountsForPriceItem.Clear();
								ulong discountMask = 0;
								for (int i = 0; i < availableDiscountsForStructure.Count; ++i)
								{
									var discount = availableDiscountsForStructure[i];
									if (discount != null)
									{
										var shouldInclude = priceHeaderLink?.ShouldIncludeDiscount(usageLine.PriceItem, usageLine.CountryCodeForDiscount, discount) ?? true;
										if (shouldInclude)
										{
											discountMask |= 1ul << i;
											discountsForPriceItem.Add(discount);
										}
									}
								}

								if (discountMask != 0)
								{
									DiscountSet discountSet;
									if (!discountSetsForDatabase.TryGetValue(discountMask, out discountSet))
									{
										discountSet = new DiscountSet(discountsForPriceItem.ToArray());
										discountSetsForDatabase.Add(discountMask, discountSet);
										discountFinder.AddDiscounts(discountsForPriceItem.Where(x => x.RequiredByDependentDiscount));
									}

									usageLine.Discounts = discountSet;
								}
							}
						}
					}
				}
			}

			foreach (var monthlyUsage in monthlyUsageList)
			{
				int discountKeyIndex = 0;
				foreach (UsageLine usageLine in monthlyUsage.UsageLinesInOrder)
				{
					var discountSet = usageLine.Discounts;
					if (discountSet != null && discountSet.EffectiveMultiplier != 1 && discountSet.Key == null)
					{
						discountSet.Key = IndexToKey(discountKeyIndex++);
					}
					ApplyDiscounts(usageLine);
				}
			}
		}

		static List<string> GetCountryCodesWithSingleCountryDiscount(DatabaseUsage dbUsage, PriceList priceList, ClientLicencePriceItem priceItem)
		{
			var result = new List<string>();
			var discountVersionCode = priceItem.Parent?.L6_DiscountCode ?? ZString.Empty;
			if (priceList.IncludeCargoWiseOneDiscounts && dbUsage.StlPriceList != null)
			{
				discountVersionCode = dbUsage.StlPriceList.Header.L6_DiscountCode;
			}
			var usageDiscounts = dbUsage.GetDiscountVersion(discountVersionCode);
			string discountStructure = priceItem.L7_PGM_DiscountGroupCode;

			foreach (var headerDiscount in usageDiscounts.Discounts)
			{
				if (headerDiscount.PHD_Type == BillingConstants.DiscountCalculator.SingleCountry &&
					usageDiscounts.GroupContains(discountStructure, headerDiscount))
				{
					var setting = dbUsage.GetDiscountSetting(headerDiscount.PHD_Name);
					if ((setting != null && setting.LS9_IsActive) || (setting == null && headerDiscount.PHD_IsDefaultEnabled))
					{
						var config = (SingleCountryDiscount)headerDiscount.Config;
						var country = config.Country;
						if (!country.IsEmpty && !result.Contains(country))
						{
							result.Add(country);
						}
					}
				}
			}
			return result;
		}

		static void BuildAvailableDiscountsForDatabase(BillingRunContext context, List<IStlDiscount> availableDiscountsForDatabase, StlMonthlyUsage monthlyUsage, DiscountVersion usageDiscounts, StlDiscountFinder discountFinder)
		{
			var suspensionPolicyCode = monthlyUsage.DatabaseUsage.GetDiscountSuspensionPolicyLicenceSetting()?.PolicyCode ?? ZString.Empty;
			if (suspensionPolicyCode.IsEmpty)
			{
				suspensionPolicyCode = DiscountSuspensionPolicyList.Codes.NeverSuspend;
				var productCode = monthlyUsage.Database?.LD_Product ?? ZString.Empty;
				if (!productCode.IsEmpty)
				{
					if (context.Factory.GetCachedValue("StlBilling.StlDiscountSuspensionPolicyDefault", () =>
							EDIDataRegistry.Instance.StlDiscountSuspensionPolicyDefault.Value.OfType<DiscountSuspensionPolicy>()
								.ToDictionary(k => k.ProductCode, v => v.PolicyCode)).TryGetValue(productCode, out var defaultPolicyCode))
					{
						suspensionPolicyCode = defaultPolicyCode;
					}
				}
			}

			if (suspensionPolicyCode == DiscountSuspensionPolicyList.Codes.AlwaysSuspend)
			{
				return;
			}

			StlDiscountCalculatorFactory discountFactory = new StlDiscountCalculatorFactory();
			DiscountInfo theInfo = new DiscountInfo();

			var headerDiscounts = usageDiscounts.Discounts;
			List<IStlDiscount> potentialDiscountsForDatabase = new List<IStlDiscount>();

			foreach (var headerDiscount in headerDiscounts)
			{
				var setting = monthlyUsage.DatabaseUsage.GetDiscountSetting(headerDiscount.PHD_Name);

				if ((setting != null && setting.LS9_IsActive) || (setting == null && headerDiscount.PHD_IsDefaultEnabled))
				{
					theInfo.Init(headerDiscount, setting, monthlyUsage, context.DatabaseCountryUsers, discountFinder, context);
					var discount = discountFactory.New(theInfo);
					potentialDiscountsForDatabase.Add(discount);
				}
			}

			StlDiscount.BuildAvailableDiscounts(potentialDiscountsForDatabase, availableDiscountsForDatabase);

			if (potentialDiscountsForDatabase.Any(x => x is PrepaymentStlDiscount))
			{
				monthlyUsage.IsPrepaymentDiscountAvailable = true;
			}
		}

		static void ApplyAllMinSpendAdjustments(IEnumerable<StlMonthlyUsage> monthlyUsageList, HashSet<ZGuid> priceItemPKFilter)
		{
			foreach (var monthlyUsagesPerDatabase in monthlyUsageList
				.Where(x => x.DatabaseUsage != null && x.DatabaseUsage.HasMinSpendSetting)
				.GroupBy(x => x.DatabaseUsage))
			{
				var databaseUsage = monthlyUsagesPerDatabase.Key;
				foreach (var setting in databaseUsage.GetMinSpendSettings())
				{
					ApplyMinSpendAdjustment(databaseUsage, monthlyUsagesPerDatabase, setting, priceItemPKFilter);
				}
			}
		}

		static void ApplyMinSpendAdjustment(DatabaseUsage databaseUsage, IEnumerable<StlMonthlyUsage> monthlyUsageList, MinSpendLicenceSetting setting, HashSet<ZGuid> priceItemPKFilter)
		{
			var settingPriceKey = setting.PriceKey;
			var matchingUsageLines = GetUsageLinesWithPriceKey(monthlyUsageList, settingPriceKey);
			StlMonthlyUsage monthlyUsageToAdjust = null;
			ClientLicencePriceItem priceItemToAdjust = null;
			decimal adjustment = 0;

			if (matchingUsageLines.Count > 0)
			{
				var usedAmount = matchingUsageLines.Sum(x => x.usageLine.PostDiscountAmount);
				if (usedAmount < setting.LS9_Price)
				{
					adjustment = setting.LS9_Price - usedAmount;
					var maxBreakOrAmountLine = FindMaxBreakOrAmountLine(matchingUsageLines);
					priceItemToAdjust = maxBreakOrAmountLine.usageLine.PriceItem;
					monthlyUsageToAdjust = maxBreakOrAmountLine.monthlyUsage;
				}
			}
			else
			{
				// They had no usage at all.
				// The price item has the charge code and description
				monthlyUsageToAdjust = monthlyUsageList.First();
				(var priceList, var priceNode) = GetBestPriceNode(databaseUsage, settingPriceKey);
				if (priceNode != null)
				{
					adjustment = setting.LS9_Price;
					// priceNode will be the item with the highest break.
					// Find the lowest break instead since it will have a description more likely to match the case of no usage, e.g., "eCommerce first 50000".
					if (priceNode.Item.L7_UnitBreak > 0)
					{
						var priceItem = priceNode.Item;
						priceNode = priceList.ItemSet.AllNodesWithCode.Where(x =>
								x.Item.L7_Category == priceItem.L7_Category &&
								x.Item.L7_Code == priceItem.L7_Code)
							.OrderByDescending(item => item.Item.L7_UnitBreak)
							.First();
					}

					priceItemToAdjust = priceNode.Item;
				}
				else
				{
					var msg = BillingConstants.LicenceSetting.Descriptions.MinSpend + (NoResString)" " + setting.Summary + (NoResString)" has no matching price code on current pricelists";
					monthlyUsageToAdjust.AddRowWarning(msg);
				}
			}

			if (adjustment != 0 && priceItemToAdjust != null && monthlyUsageToAdjust != null && (priceItemPKFilter?.Contains(priceItemToAdjust.PK) ?? true))
			{
				var line = new UsageLine(monthlyUsageToAdjust.Factory, monthlyUsageToAdjust.PeriodStart, monthlyUsageToAdjust.PeriodStart, "", priceItemToAdjust,
					monthlyUsageToAdjust.GetOrCreateCommitmentRoleItem(), null, null);
				line.FormatOptions = FormatOptions.GetOptionsByProduct(monthlyUsageToAdjust.Database?.LD_Product, monthlyUsageToAdjust.Factory);
				var minSpendAdditionalDescription = ResString.GetMultilingualString("B7E57797-FA5B-4797-BE5C-56B3EFC923F8", "Adjustment To Reach Minimum Spend");
				PopulateSimpleAmountUsageLine(line, adjustment, databaseUsage.PriceCurrency, minSpendAdditionalDescription);
				line.SetLicenceUnits(0);
				line.RoleIndex = UsageLine.CommitmentRoleIndex;
				line.FeeBasisText = ZString.Empty;
				monthlyUsageToAdjust.AddUsageLine(line);
			}
		}

		static List<(StlMonthlyUsage monthlyUsage, UsageLine usageLine)> GetUsageLinesWithPriceKey(IEnumerable<StlMonthlyUsage> monthlyUsageList, UsageCodeKey priceKey)
		{
			var matchingUsageLines = new List<(StlMonthlyUsage monthlyUsage, UsageLine usageLine)>();
			foreach (var monthlyUsage in monthlyUsageList)
			{
				foreach (var usageLine in monthlyUsage.UsageLines)
				{
					var priceItem = usageLine.PriceItem;
					if (priceItem != null && priceItem.L7_Category == priceKey.Category && priceItem.L7_Code == priceKey.Code)
					{
						matchingUsageLines.Add((monthlyUsage, usageLine));
					}
				}
			}

			return matchingUsageLines;
		}

		static (StlMonthlyUsage monthlyUsage, UsageLine usageLine) FindMaxBreakOrAmountLine(List<(StlMonthlyUsage monthlyUsage, UsageLine usageLine)> matchingUsageLines)
		{
			(StlMonthlyUsage monthlyUsage, UsageLine usageLine) maxBreakOrAmountLine = (null, null);
			foreach (var match in matchingUsageLines)
			{
				var maxUsageLine = maxBreakOrAmountLine.usageLine;
				if (maxUsageLine == null ||
					(
						(maxUsageLine.PriceItem.L7_UnitBreak < match.usageLine.PriceItem.L7_UnitBreak)
						||
						(
							maxUsageLine.PriceItem.L7_UnitBreak == match.usageLine.PriceItem.L7_UnitBreak
							&&
							maxUsageLine.UnitCount < match.usageLine.UnitCount
						)
					))
				{
					maxBreakOrAmountLine = match;
				}
			}

			return maxBreakOrAmountLine;
		}

		static (PriceList priceList, PriceNode priceNode) GetBestPriceNode(DatabaseUsage databaseUsage, UsageCodeKey key)
		{
			var priceNode = databaseUsage.StlPriceList?.GetPriceNode(key);
			if (priceNode != null)
			{
				return (databaseUsage.StlPriceList, priceNode);
			}

			foreach (var priceList in databaseUsage.AdditionalPriceLists)
			{
				if (priceList.Header.L6_SystemCode != BillingConstants.PriceHeaderType.EHub
					&& priceList.Header.L6_SystemCode != BillingConstants.PriceHeaderType.CargoWiseNext)
				{
					priceNode = priceList.GetPriceNode(key);
					if (priceNode != null)
					{
						return (priceList, priceNode);
					}
				}
			}
			return (null, null);
		}

		static void CalculateFeeDiscounts(IEnumerable<ProtoBill> protoBills, DatabaseUsageSet databaseUsageSet)
		{
			foreach (var protoBill in protoBills.Where(x => x.HasFees && x.HasPrepaid))
			{
				if (protoBill.HasMonthlyUsage)
				{
					foreach (var monthlyUsage in protoBill.MonthlyUsages)
					{
						var discountableFees = monthlyUsage.FeeUsages.Where(y => y.Fee.L8_IsDiscountable);
						if (discountableFees.Any())
						{
							CalculatePrepayDiscount(databaseUsageSet, discountableFees, monthlyUsage.Database.PK.ToGuid());
						}
					}
				}
				else
				{
					// fee only bill
					var discountableFees = protoBill.FeeUsages.Where(y => y.Fee.L8_IsDiscountable && !y.Fee.L8_LD.IsEmpty);
					foreach (var databaseFeeGroup in discountableFees.GroupBy(x => x.Fee.L8_LD.ToGuid()))
					{
						var databasePk = databaseFeeGroup.Key;
						CalculatePrepayDiscount(databaseUsageSet, databaseFeeGroup, databasePk);
					}
				}
			}
		}

		static void CalculatePrepayDiscount(DatabaseUsageSet databaseUsageSet, IEnumerable<FeeUsage> feeUsages, Guid databasePk)
		{
			var priceList = databaseUsageSet.AllStlPrices.GetPriceListByDatabasePk(databasePk);
			if (priceList != null && priceList.Discounts != null)
			{
				var prepayHeaderDiscount = priceList.Discounts.Discounts.FirstOrDefault(x => x.PHD_Type == BillingConstants.DiscountCalculator.Prepayment);
				if (prepayHeaderDiscount != null)
				{
					var prepaySetting = databaseUsageSet.GetSettingByDatabasePk(databasePk, prepayHeaderDiscount);
					if ((prepaySetting != null && prepaySetting.LS9_IsActive) || (prepaySetting == null && prepayHeaderDiscount.PHD_IsDefaultEnabled))
					{
						decimal percent = prepaySetting != null && prepaySetting.LS9_Percent != 0m
							? prepaySetting.LS9_Percent
							: prepayHeaderDiscount.PHD_Percent;

						foreach (var feeUsage in feeUsages)
						{
							feeUsage.PrepayDiscount = prepayHeaderDiscount;
							feeUsage.SetAmounts(feeUsage.Fee.L8_Amount,
								Utilities.Round(feeUsage.Fee.L8_Amount * (100 - percent) / 100m, BillingConstants.RoundingDecimals));
						}
					}
				}
			}
		}

		static string IndexToKey(int index)
		{
			string result = "";
			do
			{
				result = (char)('A' + (index % 26)) + result;
				index /= 26;
			} while (index != 0);
			return result;
		}

		public class BuyingGroupVolume
		{
			public decimal TotalLicenceUnits { get; set; }
		}

		/// <summary>
		/// Each StlMonthlyUsage with a shared commitment has their own instance of this class.
		/// The adjustment can vary amongst group members if the unrounded amount is not a whole number.
		/// E.g., if the group has three members and the group usage needs to be bumped by 100 overall
		/// then one member is bumped by 33, and two by 34.
		/// </summary>
		public class SharedCommitment
		{
			public decimal TotalUsedLicenceUnits { get; set; }
			public decimal Adjustment { get; set; }
		}

		public class DiscountSet
		{
			public DiscountSet(IStlDiscount[] discounts)
			{
				this.discounts = discounts;

				EffectiveMultiplier = 1;
				foreach (var discount in discounts)
				{
					EffectiveMultiplier *= 1 - discount.Percentage / 100m;
				}

				EffectiveMultiplier = Utilities.Round(EffectiveMultiplier, EffectiveMultiplierDecimals);
			}

			readonly IStlDiscount[] discounts;
			public decimal EffectiveMultiplier { get; private set; }
			public decimal EffectivePercent { get { return 100 * (1 - EffectiveMultiplier); } }
			public string EffectivePercentText
			{
				get
				{
					return EffectivePercent.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
				}
			}
			public string Key { get; set; }

			public const int EffectiveMultiplierDecimals = 3;

			public string CombinedDescription
			{
				get
				{
					var desc = new StringBuilder();
					foreach (var discount in discounts)
					{
						var percentage = discount.Percentage;
						if (percentage != 0m)
						{
							if (desc.Length > 0)
							{
								desc.Append(", ");
							}

							desc.Append(discount.HeaderDiscount.NameDescription);
							desc.Append(" (");
							desc.Append(Utilities.Round(percentage, 2).ToString("0.##", CultureInfo.InvariantCulture));
							desc.Append("%)");
						}
					}

					return desc.ToString();
				}
			}

			public IStlDiscount[] ToArray() => discounts;
		}

		public const int DiscountedPriceDecimals = 3;
		public const string DiscountedPriceFormat = "#,##0.000";

		public static void ApplyDiscounts(UsageLine usageLine)
		{
			decimal discountAsMultiplier = usageLine.Discounts != null ? usageLine.Discounts.EffectiveMultiplier : 1m;
			usageLine.DiscountedPrice = Utilities.Round(discountAsMultiplier * usageLine.Price, DiscountedPriceDecimals);

			ZDecimal rawAmount = usageLine.IsDisbursement ? usageLine.TotalPrice : usageLine.UnitCount * usageLine.Price;
			var unroundedPreDiscountAmount = rawAmount;
			var unroundedPostDiscountAmount = discountAsMultiplier * rawAmount;

			usageLine.SetAmounts(unroundedPreDiscountAmount, unroundedPostDiscountAmount);
		}

		/// <summary>
		/// Assign prices
		/// </summary>
		static List<StlMonthlyUsage> MatchUsageToPrice(BillingRunContext context, DatabaseUsageSet databaseUsageSet, HashSet<ZGuid> priceItemPKFilter)
		{
			var result = new List<StlMonthlyUsage>();

			// We need to bill CW1 first.
			// Because some products (WTA) may require discounts (MDC) based on CW1 discount.
			var usages = databaseUsageSet.DatabaseUsages
				.OrderBy(x => (x.Database?.IsEnterpriseFamilyDatabase ?? false) ? 0 : 1)
				.ThenBy(x => x.Database?.LD_Product ?? "").ToArray();

			foreach (var databaseUsage in usages)
			{
				result.AddRange(MatchUsageToPrice(context.Factory, context.PeriodStart, databaseUsage, priceItemPKFilter));
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public static List<StlMonthlyUsage> MatchUsageToPrice(BusinessObjectFactory factory, ZDateTime periodStart, DatabaseUsage databaseUsage, HashSet<ZGuid> priceItemPKFilter = null)
		{
			var cwnPriceListUsageMap = databaseUsage.AdditionalPriceLists?
						.FirstOrDefault(x => x.Header.L6_SystemCode == BillingConstants.BillingSystem.CargoWiseNext)?
						.Header.UsageMaps?.Select(x => new { x.PUM_UsageCategory, x.PUM_UsageCode });

			//the CWN usages are from STL collector (SHD), so the U1_Code can only be STL, we need to check the mapping here.
			bool IsCw1NextUsage(Usage usage)
			{
				return usage.Code == BillingConstants.BillingSystem.CargoWiseNext
					|| (cwnPriceListUsageMap != null && cwnPriceListUsageMap.Any(x => x.PUM_UsageCategory == usage.Code && x.PUM_UsageCode == usage.SubCode));
			}

			var result = new List<StlMonthlyUsage>();

			var invoiceGroupToMonthlyUsage = new Dictionary<InvoiceGroup, StlMonthlyUsage>(1);
			bool isDatabaseBilledPerCompany = databaseUsage.Database?.LD_IsBilledPerCompany ?? false;
			var borderWiseUsageLines = new List<UsageLine>();

			foreach (var usageByPriceNode in databaseUsage.Usages
				.Where(x => x.Code != BillingConstants.BillingSystem.ClientMapping && !IsCw1NextUsage(x))
				.GroupBy(x => GetBestPriceNodeForNonHubUsage(databaseUsage, x))
				.Where(x => x.Key.priceNode != null)
				.Where(x => priceItemPKFilter?.Contains(x.Key.priceNode.Item.PK) ?? true)
				.OrderBy(x => x.Key.priceNode.Item.L7_UnitBreakParentCode.IsEmpty ? 0 : 1)
				.ThenBy(x => (short)x.Key.priceNode.Item.L7_Order))
			{
				var priceList = usageByPriceNode.Key.priceList;
				var priceNode = usageByPriceNode.Key.priceNode;
				var newUsageLines = priceNode.Item.L7_Category == BillingConstants.BillingSystem.BorderWise
					? borderWiseUsageLines
					: null;
				CalculatePricingPerPriceNode(factory, priceNode, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, usageByPriceNode, priceList, newUsageLines);
			}

			ApplyBorderWisePurchasedLicences(databaseUsage, borderWiseUsageLines);

			// eHub has additional complexity
			IEnumerable<Usage> hubUsages = databaseUsage.Usages
				.Where(x => x.Code == BillingConstants.BillingSystem.ClientMapping);
			if (hubUsages.Any())
			{
				CalculateHubPricing(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, hubUsages, isDatabaseBilledPerCompany, priceItemPKFilter);
			}

			// CWN
			var cwnUsages = databaseUsage.Usages
				.Where(x => IsCw1NextUsage(x));
			if (cwnUsages.Any())
			{
				CalculateDisbursements(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, cwnUsages, isDatabaseBilledPerCompany, priceItemPKFilter);
			}

			var systemCode = databaseUsage.Database?.LD_ServerCode ?? "N/A";
			var billedDeliveries = databaseUsage.Usages.Select(x => x.OwnerDelivery?.Delivery).Distinct()
				.Where(y => y != null && y.L9_IsBilled && !y.L9_RX_NKInvoiceCurrency.IsEmpty && y.InvoicingBranch != null).ToArray();
			var deliveryCurrencies = new Dictionary<ZString, ZString[]>(1);
			var deliveryBranches = new Dictionary<ZString, ZString[]>(1);
			var deliveryInvoiceToOrgs = new Dictionary<ZString, ZString[]>(1);
			if (!isDatabaseBilledPerCompany)
			{
				deliveryCurrencies.Add(systemCode, billedDeliveries.Select(x => x.L9_RX_NKInvoiceCurrency).Distinct().ToArray());
				deliveryBranches.Add(systemCode, billedDeliveries.Select(x => x.InvoicingBranch.GB_Code).Distinct().ToArray());
				deliveryInvoiceToOrgs.Add(systemCode, billedDeliveries.Select(x => x.InvoiceToText).Distinct().ToArray());
			}
			else
			{
				foreach (var companyDeliveries in billedDeliveries.GroupBy(x => x.Company))
				{
					var companyCode = companyDeliveries.Key.LC_CompanyCode;
					deliveryCurrencies.Add(companyCode, companyDeliveries.Select(x => x.L9_RX_NKInvoiceCurrency).Distinct().ToArray());
					deliveryBranches.Add(companyCode, companyDeliveries.Select(x => x.InvoicingBranch.GB_Code).Distinct().ToArray());
					deliveryInvoiceToOrgs.Add(companyCode, companyDeliveries.Select(x => x.InvoiceToText).Distinct().ToArray());
				}
			}

			foreach (var monthlyUsage in invoiceGroupToMonthlyUsage.Values)
			{
				var usagesWithDelivery = monthlyUsage.Usages.Where(x => x.OwnerDelivery?.Delivery != null);
				var mainTaxes = usagesWithDelivery.Where(x => !x.IsSpecialTaxUsage).Select(x => new SystemBill.TaxGroup(x.OwnerDelivery.Delivery)).Distinct().ToArray();
				var specialTaxes = usagesWithDelivery.Where(x => x.IsSpecialTaxUsage).Select(x => new SystemBill.TaxGroup(x.OwnerDelivery.Delivery)).Distinct().ToArray();
				if (mainTaxes.Length > 1 || specialTaxes.Length > 1)
				{
					string taxesAsString = string.Empty;
					if (mainTaxes.Length > 1)
					{
						taxesAsString += string.Join(", ", mainTaxes.Select(x => x.Code));
					}
					if (specialTaxes.Length > 1)
					{
						taxesAsString += string.Join(", ", specialTaxes.Select(x => x.Code));
					}
					monthlyUsage.AddRowError((NoResString)"Database " + systemCode + (NoResString)" has multiple taxes " + taxesAsString);
				}

				if (isDatabaseBilledPerCompany)
				{
					var owners = monthlyUsage.Usages.Where(x => x.OwnerDelivery.InvoiceGroup == monthlyUsage.InvoiceGroup).Select(x => x.OwnerDelivery.Owner).Distinct().ToArray();
					foreach (var missingCurrencyOwner in owners.Where(x => string.IsNullOrEmpty(x.PerCompanyBillingPriceCurrency)))
					{
						monthlyUsage.AddRowError((NoResString)"Org " + missingCurrencyOwner.Company.Header.OH_Code + (NoResString)" database " + systemCode + (NoResString)" has no per company billing price currency");
					}
					var priceCurrencies = owners.Where(x => !string.IsNullOrEmpty(x.PerCompanyBillingPriceCurrency)).Select(x => x.PerCompanyBillingPriceCurrency).Distinct().ToArray();
					if (priceCurrencies.Length > 1)
					{
						monthlyUsage.AddRowError((NoResString)"Database " + systemCode + (NoResString)" has multiple per company billing price currencies " + string.Join((NoResString)", ", priceCurrencies));
					}
					foreach (var owner in owners)
					{
						var companyCode = owner.Company.LC_CompanyCode;
						if (deliveryCurrencies.TryGetValue(companyCode, out var currenciesForCompany) && currenciesForCompany.Length > 1)
						{
							monthlyUsage.AddRowError((NoResString)"Company " + companyCode + (NoResString)" on Database " + systemCode + (NoResString)" has multiple invoicing currencies " + string.Join((NoResString)", ", currenciesForCompany));
						}
						if (deliveryBranches.TryGetValue(companyCode, out var branchesForCompany) && branchesForCompany.Length > 1)
						{
							monthlyUsage.AddRowError((NoResString)"Company " + companyCode + (NoResString)" on Database " + systemCode + (NoResString)" has multiple invoicing branches " + string.Join((NoResString)", ", branchesForCompany));
						}
						if (deliveryInvoiceToOrgs.TryGetValue(companyCode, out var invoiceToOrgsForCompany) && invoiceToOrgsForCompany.Length > 1)
						{
							monthlyUsage.AddRowError((NoResString)"Company " + companyCode + (NoResString)" on Database " + systemCode + (NoResString)" has multiple invoicing organisations " + string.Join((NoResString)", ", invoiceToOrgsForCompany));
						}
					}
				}
				else
				{
					if (deliveryCurrencies[systemCode].Length > 1)
					{
						monthlyUsage.AddRowError((NoResString)"Database " + systemCode + (NoResString)" has multiple invoicing currencies " + string.Join((NoResString)", ", deliveryCurrencies[systemCode]));
					}
					if (deliveryBranches[systemCode].Length > 1)
					{
						monthlyUsage.AddRowError((NoResString)"Database " + systemCode + (NoResString)" has multiple invoicing branches " + string.Join((NoResString)", ", deliveryBranches[systemCode]));
					}
					if (deliveryInvoiceToOrgs[systemCode].Length > 1)
					{
						monthlyUsage.AddRowError((NoResString)"Database " + systemCode + (NoResString)" has multiple invoicing organisations " + string.Join((NoResString)", ", deliveryInvoiceToOrgs[systemCode]));
					}
				}

				if (monthlyUsage.HasRowErrors || monthlyUsage.UsageLines.Any())
				{
					monthlyUsage.DatabaseUsage.AddValidationNotifications(monthlyUsage);
					CheckCountryTierUsage(monthlyUsage);

					result.Add(monthlyUsage);
				}
			}

			return result;
		}

		static void CheckCountryTierUsage(StlMonthlyUsage monthlyUsage)
		{
			var hasCountryTierPriceItem = false;
			if (monthlyUsage.DatabaseUsage.StlPriceList != null && monthlyUsage.DatabaseUsage.StlPriceList.HasAnyCountryTierPriceItems())
			{
				hasCountryTierPriceItem = true;
				CheckForMissingPriceItem(monthlyUsage, monthlyUsage.DatabaseUsage.StlPriceList);
			}

			foreach (var priceList in monthlyUsage.DatabaseUsage.AdditionalPriceLists)
			{
				if (priceList.HasAnyCountryTierPriceItems())
				{
					hasCountryTierPriceItem = true;
					CheckForMissingPriceItem(monthlyUsage, priceList);
				}
			}

			if (!hasCountryTierPriceItem)
			{
				return;
			}

			var countryTierBilledUsageLines = monthlyUsage.UsageLines.Where(x => x.PriceItem.L7_FeeType.EqualsIgnoringCase(BillingConstants.FeeType.CountryTier)).ToArray();

			monthlyUsage.DatabaseUsage.Usages.ForEach(usage =>
			{
				var servicePriceHeaderCode = usage.ServicePriceHeaderCode;

				if (servicePriceHeaderCode != null)
				{
					var priceList = monthlyUsage.DatabaseUsage.GetPriceList(servicePriceHeaderCode);
					if (priceList != null && priceList.HasPriceCodeCountryTierRegistryMapping(usage.SubCode))
					{
						CheckForMissingRegistryMappingLine(monthlyUsage, priceList, usage);
						return;
					}
				}
				else
				{
					var stlPriceListHasMapping = monthlyUsage.DatabaseUsage.StlPriceList?.HasPriceCodeCountryTierRegistryMapping(usage.SubCode);
					if (stlPriceListHasMapping.HasValue && stlPriceListHasMapping.Value)
					{
						CheckForMissingRegistryMappingLine(monthlyUsage, monthlyUsage.DatabaseUsage.StlPriceList, usage);
						return;
					}
				}

				foreach (var priceList in monthlyUsage.DatabaseUsage.AdditionalPriceLists)
				{
					if (priceList.Header.L6_SystemCode != BillingConstants.PriceHeaderType.EHub)
					{
						if (priceList.HasPriceCodeCountryTierRegistryMapping(usage.SubCode))
						{
							CheckForMissingRegistryMappingLine(monthlyUsage, priceList, usage);
							return;
						}
					}
				}

				return;
			});

			void CheckForMissingRegistryMappingLine(StlMonthlyUsage mUsage, PriceList priceList, Usage usageLine)
			{
				var countryTierCode = priceList.GetCountryTierCode(usageLine.SubCode, usageLine.Company.LCC_RN_NKCountryCode);

				if (countryTierBilledUsageLines.Any(x => x.PriceItem.Parent.L6_SystemCode.EqualsIgnoringCase(priceList.Header.L6_SystemCode) && x.PriceItem.L7_Code.EqualsIgnoringCase(usageLine.SubCode) && x.PriceItem.L7_CountryTierCode.EqualsIgnoringCase(countryTierCode)))
				{
					return;
				}

				if (countryTierCode.Length == 0)
				{
					mUsage.AddRowError((NoResString)"System code " + priceList.Header.L6_SystemCode + (NoResString)" and Price Code " + usageLine.SubCode + (NoResString)" have no mapping for country code " + usageLine.Company.LCC_RN_NKCountryCode + (NoResString)" in the " + EDIDataRegistry.Instance.CountryTierPriceCodeMappings.Caption + (NoResString)" registry");
				}
			}

			void CheckForMissingPriceItem(StlMonthlyUsage mUsage, PriceList priceList)
			{
				var missingPriceItems = priceList.GetMissingCountryTierPriceItems();
				foreach (var missingPriceItem in missingPriceItems)
				{
					mUsage.AddRowError((NoResString)"Country Tier Code " + missingPriceItem.Item2 + (NoResString)" is not mapped to any price items for system code " + priceList.Header.L6_SystemCode + (NoResString)" and Price Code " + missingPriceItem.Item1);
				}
			}
		}

		/// <summary>
		/// Search all pricelists for a price matching the usage, preferring STL pricelist.
		/// </summary>
		static (PriceList priceList, PriceNode priceNode) GetBestPriceNodeForNonHubUsage(DatabaseUsage databaseUsage, Usage usage)
		{
			var servicePriceHeaderCode = usage.ServicePriceHeaderCode;
			var key = GetUsageKey(usage.Code, usage.SubCode);
			var usageCountry = usage.Company?.LCC_RN_NKCountryCode;
			PriceNode priceNode;

			if (servicePriceHeaderCode != null)
			{
				var priceList = databaseUsage.GetPriceList(servicePriceHeaderCode);
				if (priceList != null)
				{
					priceNode = priceList.GetPriceNodeFromUsageKey(key, usageCountry);
					if (priceNode != null)
					{
						return (priceList, priceNode);
					}
				}
			}
			else
			{
				priceNode = databaseUsage.StlPriceList?.GetPriceNodeFromUsageKey(key, usageCountry);
				if (priceNode != null)
				{
					return (databaseUsage.StlPriceList, priceNode);
				}
			}

			foreach (var priceList in databaseUsage.AdditionalPriceLists)
			{
				if (priceList.Header.L6_SystemCode != BillingConstants.PriceHeaderType.EHub)
				{
					priceNode = priceList.GetPriceNodeFromUsageKey(key, usageCountry);
					if (priceNode != null)
					{
						return (priceList, priceNode);
					}
				}
			}
			return (null, null);
		}

		static void CalculateHubPricing(
			BusinessObjectFactory factory,
			ZDateTime periodStart,
			DatabaseUsage databaseUsage,
			Dictionary<InvoiceGroup, StlMonthlyUsage> invoiceGroupToMonthlyUsage,
			IEnumerable<Usage> hubUsages,
			bool isDatabaseBilledPerCompany,
			HashSet<ZGuid> priceItemPKFilter)
		{
			// Legacy method had a price for each interface on a special HUB pricelist on the customers organization.
			// New method has a common price for all interfaces on the standard STL pricelist.
			var hubPriceList = databaseUsage.GetPriceList(BillingConstants.PriceHeaderType.EHub);
			var stlPriceList = databaseUsage.StlPriceList;

			foreach (var usageByParentPrice in hubUsages
				.GroupBy(x => FindPriceNodeWithRefCode(hubPriceList, stlPriceList, x.Code, x.SubCode))
				.Where(x => priceItemPKFilter?.Contains(x.Key.priceNode?.Item.PK ?? ZGuid.Empty) ?? true)
				.OrderBy(x => x.Key.priceNode != null ? (short)x.Key.priceNode.Item.L7_Order : short.MaxValue))
			{
				var priceNode = usageByParentPrice.Key.priceNode;
				if (priceNode != null)
				{
					var priceList = usageByParentPrice.Key.priceList;
					var currency = priceNode.Item.L7_RX_NKCurrency;

					if (priceList.Header.L6_SystemCode == BillingConstants.PriceHeaderType.STL)
					{
						foreach (var usage in usageByParentPrice)
						{
							usage.AdditionalDescription = usage.ChargeableUsage?.U1_SubCode ?? ZString.Empty;
						}

						if (currency.IsEmpty)
						{
							currency = isDatabaseBilledPerCompany
								? usageByParentPrice.First().OwnerDelivery?.Owner.PerCompanyBillingPriceCurrency
								: databaseUsage.PriceCurrency;
						}
					}
					else
					{
						if (currency.IsEmpty)
						{
							currency = priceList.Header.L6_RX_NKCurrency;
						}
					}

					Dictionary<Guid, decimal> itemPkToPrice = priceList.GetItemPkToPrice(currency);
					foreach (var usageByPeriodStart in usageByParentPrice.GroupBy(x => x.PeriodStart))
					{
						foreach (var usageByAdditionalDescription in usageByPeriodStart.GroupBy(x => x.AdditionalDescription))
						{
							// following similar logic to the ClientMappingBillingSystem, the biggest user pays for all usage
							var biggestInvoiceGroup = usageByAdditionalDescription.GroupBy(usage => usage.OwnerDelivery.InvoiceGroup).OrderByDescending(grp => grp.Sum(usage => usage.UnitCount)).First();
							var monthlyUsage = FindOrCreateMonthlyUsage(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, biggestInvoiceGroup.Key, biggestInvoiceGroup.First().OwnerDelivery.Delivery);

							CalculatePricing(factory, periodStart, monthlyUsage, currency, priceNode, usageByAdditionalDescription, priceList, itemPkToPrice, null, false);
						}
					}
				}
				else
				{
					var first = usageByParentPrice.First();
					var monthlyUsage = FindOrCreateMonthlyUsage(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, first.OwnerDelivery.InvoiceGroup, first.OwnerDelivery.Delivery);
					var msg = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} [{1}]: No price found. Price must be on the STL price list or, for older pricing, the HUB pricelist on the database usage owner (defaults to first live licence), or whoever their invoice is delivered to.",
						"HUB",
						first.Code + '.' + first.SubCode);
					monthlyUsage.AddRowError(msg);
				}
			}
		}

		static void CalculateDisbursements(
			BusinessObjectFactory factory,
			ZDateTime periodStart,
			DatabaseUsage databaseUsage,
			Dictionary<InvoiceGroup, StlMonthlyUsage> invoiceGroupToMonthlyUsage,
			IEnumerable<Usage> cwnUsages,
			bool isDatabaseBilledPerCompany,
			HashSet<ZGuid> priceItemPKFilter)
		{
			var cwnPriceList = databaseUsage.GetPriceList(BillingConstants.PriceHeaderType.CargoWiseNext);

			foreach (var usageByParentPrice in cwnUsages
				.GroupBy(x => Tuple.Create(x.ChargeableUsage.U1_SubCode, x.ChargeableUsage.U1_RX_NKCurrency, x.ChargeableUsage.U1_Direction)))
			{
				var priceCode = usageByParentPrice.Key.Item1;
				var currency = usageByParentPrice.Key.Item2;
				var direction = usageByParentPrice.Key.Item3;
				var priceNode = cwnPriceList.ItemSet.AllNodesWithCode.FirstOrDefault(x => x.Item.L7_Code.EqualsIgnoringCase(priceCode));

				foreach (var usageByPeriodStart in usageByParentPrice.GroupBy(x => x.PeriodStart))
				{
					foreach (var usageByAdditionalDescription in usageByPeriodStart.GroupBy(x => x.AdditionalDescription))
					{
						// following similar logic to the ClientMappingBillingSystem, the biggest user pays for all usage
						var biggestInvoiceGroup = usageByAdditionalDescription
							.GroupBy(usage => usage.OwnerDelivery.InvoiceGroup)
							.OrderByDescending(grp => grp.Sum(usage => usage.UnitCount)).First();
						var monthlyUsage = FindOrCreateMonthlyUsage(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, biggestInvoiceGroup.Key, biggestInvoiceGroup.First().OwnerDelivery.Delivery);

						CalculateDisbursement(factory, periodStart, monthlyUsage, currency, priceNode, usageByAdditionalDescription, cwnPriceList);
					}
				}
			}
		}

		/// <summary>
		/// For a single database and some of it's usage, match usage to price. 
		/// For each match, create UsageLine and add to a MonthlyUsage.
		/// Creates MonthlyUsage(s) as necessary.
		/// Usage that doesn't match any price item is ignored.
		/// </summary>
		/// <param name="factory">factory for loading anything else needed</param>
		/// <param name="periodStart">Period being billed. Usage may be from an earlier period if it was late or is being accumulated.</param>
		/// <param name="databaseUsage">DatabaseUsage being matched</param>
		/// <param name="invoiceGroupToMonthlyUsage">Dictionary of invoice to MonthlyUsage to contain results</param>
		/// <param name="usages">usages to match</param>
		/// <param name="priceList">pricelist to use for matching</param>
		/// <param name="priceHeaderType">header type of priceList</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void CalculatePricingPerPriceNode(
			BusinessObjectFactory factory,
			PriceNode priceNode,
			ZDateTime periodStart,
			DatabaseUsage databaseUsage,
			Dictionary<InvoiceGroup, StlMonthlyUsage> invoiceGroupToMonthlyUsage,
			IEnumerable<Usage> usageByPriceNode,
			PriceList priceList,
			List<UsageLine> newUsageLines)
		{
			var priceItem = priceNode.Item;
			var priceKey = priceItem.CodeKey;
			var customPriceSetting = databaseUsage.GetCustomPriceSetting(priceKey);
			var hasHighVolumeFeatureSetting = databaseUsage.HasCustomHighVolumeFeatureSetting(priceKey);

			var countriesWithDiscount = GetCountryCodesWithSingleCountryDiscount(databaseUsage, priceList, priceItem);

			foreach (var usageByPeriodStart in usageByPriceNode.GroupBy(x => x.PeriodStart))
			{
				StlMonthlyUsage firstMonthlyUsage = null;

				foreach (var usage in usageByPeriodStart)
				{
					var usageCountryCode = usage.Company?.LCC_RN_NKCountryCode.ToString();
					if (countriesWithDiscount.Contains(usageCountryCode))
					{
						usage.CountryCodeForDiscount = usageCountryCode;
						usage.AdditionalDescription += (!usage.AdditionalDescription.IsEmpty ? " " : string.Empty) + usageCountryCode;
					}
				}

				foreach (var usageByAdditionalDescription in usageByPeriodStart.GroupBy(x => x.AdditionalDescription))
				{
					UsageOwnerDelivery perDatabaseFeeOwner = null;
					if (BillingConstants.FeeType.IsPerDatabase(priceNode.Item.L7_FeeType))
					{
						perDatabaseFeeOwner = FindPerDatabaseChargeOwner(factory, databaseUsage, usageByAdditionalDescription);
					}

					foreach (var usagesPerInvoice in usageByAdditionalDescription.GroupBy(x => (perDatabaseFeeOwner ?? x.OwnerDelivery).InvoiceGroup))
					{
						var mainTaxOwnerDelivery = perDatabaseFeeOwner ?? usagesPerInvoice.First().OwnerDelivery;
						var specialTaxOwnerDelivery = perDatabaseFeeOwner ?? usagesPerInvoice.FirstOrDefault(x => x.IsSpecialTaxUsage)?.OwnerDelivery;
						bool isDatabaseBilledPerCompany = databaseUsage.Database?.LD_IsBilledPerCompany ?? false;
						var currency = isDatabaseBilledPerCompany
							? mainTaxOwnerDelivery.Owner.PerCompanyBillingPriceCurrency
							: databaseUsage.PriceCurrency;

						var monthlyUsage = FindOrCreateMonthlyUsage(factory, periodStart, databaseUsage, invoiceGroupToMonthlyUsage, usagesPerInvoice.Key, mainTaxOwnerDelivery.Delivery, specialTaxOwnerDelivery?.Delivery);
						if (firstMonthlyUsage == null)
						{
							firstMonthlyUsage = monthlyUsage;
						}

						if (isDatabaseBilledPerCompany && perDatabaseFeeOwner == null &&
							usagesPerInvoice.Any(x => x.OwnerDelivery.Owner.PerCompanyBillingPriceCurrency != currency))
						{
							monthlyUsage.AddRowError((NoResString)"Database " + databaseUsage.Database.LD_ServerCode + (NoResString)" has multiple price currencies for price code " + priceNode.Item.CodeAndSubCodeForDisplay
								+ (NoResString)". Check the per-company billing price currency for orgs "
								+ string.Join((NoResString)", ", usagesPerInvoice.Select(x => x.OwnerDelivery.Owner).Distinct().Select(x => x.Company.Header.OH_Code + (NoResString)" " + x.PerCompanyBillingPriceCurrency)));
						}

						var itemPkToPrice = priceList.GetItemPkToPrice(currency);

						var usageLine = CalculatePricing(factory, periodStart, monthlyUsage, currency, priceNode, usagesPerInvoice, priceList, itemPkToPrice, customPriceSetting, hasHighVolumeFeatureSetting);
						if (usageLine != null && newUsageLines != null)
						{
							newUsageLines.Add(usageLine);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void ApplyBorderWisePurchasedLicences(DatabaseUsage databaseUsage, List<UsageLine> newUsageLines)
		{
			if (newUsageLines.Any())
			{
				var purchasedSettings = databaseUsage.GetBorderWisePurchasedLicenceSettings();
				if (purchasedSettings?.Any() ?? false)
				{
					var groups = EDIDataRegistry.Instance.BorderWisePurchasedGroups.Value;

					foreach (var linesPerPeriod in newUsageLines.GroupBy(x => x.PeriodStart))
					{
						var priceCodeToUsageLines = linesPerPeriod
							.GroupBy(x => (string)x.PriceItemCode)
							.ToDictionary(x => x.Key,
								y => y.OrderByDescending(z => z.TotalUnitCount).ToList(),
								StringComparer.OrdinalIgnoreCase);

						foreach (var setting in purchasedSettings
							// Ordered so single code purchases come before group purchases
							.OrderBy(x => groups.ContainsCode(x.PriceCode) ? 1 : 0))
						{
							int purchasedCount = setting.LicenceCount;

							var groupMemberText = groups.GetDescriptionFromCode(setting.PriceCode);
							var priceCodes = !string.IsNullOrEmpty(groupMemberText)
								? groupMemberText.Split(',').Select(x => x.Trim())
								: new string[] { setting.PriceCode };

							UsageLine firstLine = null;
							foreach (var priceCode in priceCodes)
							{
								if (priceCodeToUsageLines.TryGetValue(priceCode, out var linesOrdered))
								{
									foreach (var line in linesOrdered)
									{
										if (firstLine == null)
										{
											firstLine = line;
										}

										var lineIncludedUnitCount = Math.Min(purchasedCount, (int)line.UnitCount);
										line.IncludedUnitCount += lineIncludedUnitCount;
										purchasedCount -= lineIncludedUnitCount;
									}
								}
							}

							if (purchasedCount != 0 && firstLine != null)
							{
								// Show leftovers
								firstLine.IncludedUnitCount += purchasedCount;
							}
						}
					}
				}
			}
		}

		static UsageOwnerDelivery FindPerDatabaseChargeOwner(BusinessObjectFactory factory, DatabaseUsage databaseUsage, IEnumerable<Usage> usages)
		{
			UsageOwnerDelivery ownerDelivery;
			var owners = usages.Select(x => x.OwnerDelivery.Owner).Distinct().ToArray();
			if (owners.Length == 1)
			{
				ownerDelivery = usages.First().OwnerDelivery;
			}
			else if (owners.Any(x => x == databaseUsage.OwnerDelivery.Owner))
			{
				ownerDelivery = databaseUsage.OwnerDelivery;
			}
			else
			{
				var firstGroup = usages.Where(x => !x.ClientCompanyPk.IsEmpty)
					.GroupBy(x => x.ClientCompanyPk.ToGuid())
					.OrderBy(x => factory.Load<ClientCompany>(x.Key).LCC_CreateTimeUtc)
					.First();

				ownerDelivery = firstGroup.First().OwnerDelivery;
			}

			return ownerDelivery;
		}

		static StlMonthlyUsage FindOrCreateMonthlyUsage(BusinessObjectFactory factory, ZDateTime periodStart, DatabaseUsage databaseUsage, Dictionary<InvoiceGroup, StlMonthlyUsage> invoiceGroupToMonthlyUsage, InvoiceGroup invoiceGroup, ClientInvoiceDelivery mainTaxDelivery, ClientInvoiceDelivery specialTaxDelivery = null)
		{
			StlMonthlyUsage monthlyUsage;
			if (!invoiceGroupToMonthlyUsage.TryGetValue(invoiceGroup, out monthlyUsage))
			{
				var mainTaxGroup = new SystemBill.TaxGroup(mainTaxDelivery);
				monthlyUsage = new StlMonthlyUsage(factory, databaseUsage, periodStart, invoiceGroup, mainTaxGroup);
				invoiceGroupToMonthlyUsage.Add(invoiceGroup, monthlyUsage);
			}

			if (monthlyUsage.SpecialTaxGroup == null && specialTaxDelivery != null)
			{
				var specialTaxGroup = new SystemBill.TaxGroup(specialTaxDelivery);
				if (!specialTaxGroup.Equals(monthlyUsage.MainTaxGroup))
				{
					monthlyUsage.SpecialTaxGroup = specialTaxGroup;
				}
			}

			return monthlyUsage;
		}

		/// <summary>
		/// PriceNode lookup - looks in first price list for an item with refCode in Ref4, falling back to a lookup by code only in another pricelist.
		/// Used for Client Mapping with old style pricing where the subcode is the interface name and each interface can have it's own price
		/// or new style pricing on the main STL pricelist.
		/// </summary>
		/// <returns>null if code(s) not found</returns>
		static (PriceList priceList, PriceNode priceNode) FindPriceNodeWithRefCode(PriceList priceListWithSubCode, PriceList fallBackPriceListNoSubCode, string code, string refCode)
		{
			var priceNode = FindPriceNodeWithRefCode(priceListWithSubCode?.ItemSet, code, refCode);
			if (priceNode != null)
			{
				return (priceListWithSubCode, priceNode);
			}

			priceNode = fallBackPriceListNoSubCode?.GetPriceNodeFromUsageKey(new UsageCodeKey(BillingConstants.BillingSystem.ClientMapping, code));
			if (priceNode != null)
			{
				return (fallBackPriceListNoSubCode, priceNode);
			}

			return (null, null);
		}

		/// <summary>
		/// PriceNode lookup where the sub code can be in L7_Ref4.
		/// Used for Client Mapping with old style pricing where the subcode is the interface name and each interface can have it's own price.
		/// Category is expected to always be "CMP".
		/// </summary>
		static PriceNode FindPriceNodeWithRefCode(PriceItemSet priceItemSet, string code, string refCode)
		{
			PriceNode node = null;

			if (priceItemSet != null)
			{
				const string category = BillingConstants.BillingSystem.ClientMapping;
				var fullCode = code;
				if (refCode.Length != 0)
				{
					fullCode += '.' + refCode;
				}
				if (!priceItemSet.KeyToHighestBreakPriceNode.TryGetValue(new UsageCodeKey(category, fullCode), out node))
				{
					priceItemSet.KeyToHighestBreakPriceNode.TryGetValue(new UsageCodeKey(category, code), out node);
				}
				if (node != null && !node.Item.L7_ParentCode.IsEmpty)
				{
					PriceNode parent;
					if (!priceItemSet.KeyToHighestBreakPriceNode.TryGetValue(new UsageCodeKey(category, code + '.' + node.Item.L7_ParentCode), out parent))
					{
						priceItemSet.KeyToHighestBreakPriceNode.TryGetValue(new UsageCodeKey(category, node.Item.L7_ParentCode), out parent);
					}
					node = parent;
				}
			}

			return node;
		}

		static void CalculateMinimumFees(CurrencyExchangeService currencyExchange, IEnumerable<StlMonthlyUsage> monthlyUsageList, Dictionary<Guid, GlbCompany> branchPkToCompany, HashSet<ZGuid> priceItemPKFilter)
		{
			foreach (var monthlyUsagesPerDatabase in monthlyUsageList.Where(x => x.PriceHeader != null && x.Database != null).GroupBy(x => x.Database))
			{
				CalculateMinimumFeesForDatabase(currencyExchange, monthlyUsagesPerDatabase, branchPkToCompany, priceItemPKFilter);
			}

			foreach (var monthlyUsage in monthlyUsageList.Where(x => x.PriceHeader != null))
			{
				CalculateMinimumFeesPerReference(monthlyUsage, currencyExchange, branchPkToCompany, priceItemPKFilter);
			}
		}

		static void CalculateMinimumFeesPerReference(StlMonthlyUsage monthlyUsage, CurrencyExchangeService currencyExchange, Dictionary<Guid, GlbCompany> branchPkToCompany, HashSet<ZGuid> priceItemPKFilter)
		{
			var priceItemSet = monthlyUsage.PriceList.ItemSet;

			foreach (var lineGroup in monthlyUsage
				.UsageLines
				.Where(x => x.PriceItem != null && !x.PriceItem.L7_ParentCode.IsEmpty)
				.GroupBy(x => new UsageCodeKey(x.PriceItem.L7_ParentCategory, (string)x.PriceItem.L7_ParentCode)))
			{
				var parentKey = lineGroup.Key;

				PriceNode priceNode;
				if (priceItemSet.KeyToHighestBreakPriceNode.TryGetValue(parentKey, out priceNode)
					&& priceNode.Item.L7_FeeType == BillingConstants.FeeType.MinimumFeePerReference
					&& (priceItemPKFilter?.Contains(priceNode.Item.PK) ?? true))
				{
					var customPrice = monthlyUsage.DatabaseUsage.GetCustomPriceSetting(parentKey);
					var hasHighVolumeFeatureSetting = monthlyUsage.DatabaseUsage.HasCustomHighVolumeFeatureSetting(parentKey);
					if (!ValidatePriceSettings(monthlyUsage, monthlyUsage.PriceList.Header, priceNode.Item, customPrice, hasHighVolumeFeatureSetting))
					{
						return;
					}

					foreach (var linesPerReference in lineGroup.GroupBy(x => x.AdditionalDescription).OrderBy(x => x.Key))
					{
						var currencyToAmount = CalculateTotalPerCurrency(linesPerReference);
						var minFeeCurrency = CalculateMinimumFeeCurrency(currencyToAmount, monthlyUsage, monthlyUsage.PriceHeader, priceNode, hasHighVolumeFeatureSetting);
						if (minFeeCurrency != null)
						{
							var amountUsed = CalculateTotalInOneCurrency(currencyToAmount, minFeeCurrency, currencyExchange, branchPkToCompany[monthlyUsage.InvoiceGroup.BranchPK.ToGuid()]);
							decimal minimumAmount = CalculatePriceInCurrency(monthlyUsage, monthlyUsage.DatabaseUsage, customPrice, hasHighVolumeFeatureSetting, monthlyUsage.PriceList, priceNode, minFeeCurrency);
							if (minimumAmount <= 0)
							{
								return;
							}

							var feeAmount = minimumAmount - amountUsed;
							if (feeAmount > 0)
							{
								AddSimpleAmountUsageLine(feeAmount, minFeeCurrency, priceNode, monthlyUsage, linesPerReference.Key);
							}
						}
					}
				}
			}
		}

		static Dictionary<string, decimal> CalculateTotalPerCurrency(IEnumerable<UsageLine> usageLines)
		{
			var currencyToAmount = new Dictionary<string, decimal>(1);
			foreach (var usageLine in usageLines)
			{
				decimal total;
				if (!currencyToAmount.TryGetValue(usageLine.PriceCurrency, out total))
				{
					total = 0;
				}
				total += usageLine.PostDiscountAmount;
				currencyToAmount[usageLine.PriceCurrency] = total;
			}
			return currencyToAmount;
		}

		static decimal CalculateTotalInOneCurrency(
			Dictionary<string, decimal> currencyToAmount,
			string currencyToUse,
			CurrencyExchangeService currencyExchange,
			GlbCompany companyForCurrencyExchange)
		{
			decimal groupTotal = 0;
			foreach (var currencyAmountPair in currencyToAmount)
			{
				groupTotal += currencyExchange.GetAmount(currencyAmountPair.Key, currencyToUse, companyForCurrencyExchange, currencyAmountPair.Value);
			}

			return groupTotal;
		}

		static bool CanCalculateVariablePriceAndLicenceUnits(ClientLicencePriceHeader priceHeader, ClientLicencePriceItem item, bool hasHighVolumeFeatureSetting)
			=> (priceHeader.L6_HasExchangeRates && !IsFixedCurrency(priceHeader, item))
				|| hasHighVolumeFeatureSetting;

		/// <summary>
		/// Returns currency with largest amount if no amount exceeded the minimum, otherwise null
		/// </summary>
		static string CalculateMinimumFeeCurrency(
			Dictionary<string, decimal> currencyToAmount,
			StlMonthlyUsage owner,
			ClientLicencePriceHeader priceHeader,
			PriceNode minFeePriceNode,
			bool hasHighVolumeFeatureSetting)
		{
			string currencyToUse = null;
			decimal highestFractionUsed = -1;

			foreach (var currencyAmountPair in currencyToAmount)
			{
				decimal minimumAmount = 0;
				var isMinimumAmountValid = false;

				if (CanCalculateVariablePriceAndLicenceUnits(priceHeader, minFeePriceNode.Item, hasHighVolumeFeatureSetting))
				{
					decimal licUnits = 0;
					if (!CalculatePriceAndLicenceUnits(owner, priceHeader, minFeePriceNode.Item, currencyAmountPair.Key, hasHighVolumeFeatureSetting, out minimumAmount, out licUnits))
					{
						return null;
					}
					isMinimumAmountValid = true;
				}
				else
				{
					var itemPkToPrice = owner.PriceList.GetItemPkToPrice(currencyAmountPair.Key);
					isMinimumAmountValid = itemPkToPrice != null && itemPkToPrice.TryGetValue(minFeePriceNode.Item.PK.ToGuid(), out minimumAmount);
				}

				if (isMinimumAmountValid)
				{
					var fractionUsed = currencyAmountPair.Value / minimumAmount;
					if (highestFractionUsed < fractionUsed)
					{
						highestFractionUsed = fractionUsed;
						currencyToUse = currencyAmountPair.Key;
					}
				}
			}

			if (highestFractionUsed < 1 && currencyToUse != null)
			{
				return currencyToUse;
			}
			else
			{
				if (highestFractionUsed < 1)
				{
					owner.AddRowError(string.Format(CultureInfo.InvariantCulture, (NoResString)"Minimum fee {0} missing price for currency: {1}",
						minFeePriceNode.Item.L7_Code, currencyToAmount.First().Key));
				}

				return null;
			}
		}

		static decimal CalculatePriceInCurrency(
			BusinessObject notificationOwner,
			DatabaseUsage dbUsage,
			PriceLicenceSetting customPrice,
			bool hasHighVolumeFeatureSetting,
			PriceList priceList,
			PriceNode priceNode,
			string currencyToUse)
		{
			decimal price = 0;

			if (customPrice != null)
			{
				price = customPrice.LS9_Price;
			}
			else if (CanCalculateVariablePriceAndLicenceUnits(priceList.Header, priceNode.Item, hasHighVolumeFeatureSetting))
			{
				decimal licUnits = 0;
				if (!CalculatePriceAndLicenceUnits(notificationOwner, dbUsage, priceList.Header, priceNode.Item, currencyToUse, hasHighVolumeFeatureSetting, out price, out licUnits))
				{
					price = 0;
				}
			}
			else
			{
				price = priceList.GetItemPkToPrice(currencyToUse)[priceNode.Item.PK.ToGuid()];
			}

			return price;
		}

		static void AddSimpleAmountUsageLine(
			decimal amount,
			string currencyToUse,
			PriceNode priceNode,
			StlMonthlyUsage payer,
			string additionalDescription)
		{
			var usageLine = CreateSimpleAmountUsageLine(amount, currencyToUse, priceNode, payer, additionalDescription);
			payer.AddUsageLine(usageLine);
		}

		static UsageLine CreateSimpleAmountUsageLine(decimal amount, string currencyToUse, PriceNode priceNode, StlMonthlyUsage payer, string additionalDescription)
		{
			var usageLine = new UsageLine(payer.Factory, payer.PeriodStart, payer.PeriodStart, "", priceNode);
			usageLine.FormatOptions = FormatOptions.GetOptionsByProduct(payer.Database?.LD_Product, payer.Factory);
			PopulateSimpleAmountUsageLine(usageLine, amount, currencyToUse, additionalDescription);
			return usageLine;
		}

		static void PopulateSimpleAmountUsageLine(UsageLine usageLine, decimal amount, string currencyToUse, string additionalDescription)
		{
			usageLine.Price = amount;
			usageLine.PriceCurrency = currencyToUse;
			usageLine.TotalUnitCount = 1;
			usageLine.DiscountedPrice = amount;
			usageLine.SetAmounts(amount, amount);
			usageLine.AdditionalDescription = additionalDescription;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void CalculateMinimumFeesForDatabase(CurrencyExchangeService currencyExchange, IEnumerable<StlMonthlyUsage> monthlyUsagesPerDatabase, Dictionary<Guid, GlbCompany> branchPkToCompany, HashSet<ZGuid> priceItemPKFilter)
		{
			var owner = monthlyUsagesPerDatabase.FirstOrDefault(monthly => monthly.Usages.Any(usage => usage.OwnerDelivery.Owner == monthly.DatabaseUsage.OwnerDelivery.Owner))
				?? monthlyUsagesPerDatabase.First();

			var codeToHighestBreakPriceNode = owner.PriceList.ItemSet.KeyToHighestBreakPriceNode;
			var parentKeyToUsage = new Dictionary<UsageCodeKey, Dictionary<StlMonthlyUsage, List<UsageLine>>>();

			foreach (var monthlyUsage in monthlyUsagesPerDatabase)
			{
				foreach (var usageLine in monthlyUsage.UsageLines)
				{
					if (usageLine.PriceItem != null)
					{
						var parentCode = usageLine.PriceItem.L7_ParentCode;
						if (!parentCode.IsEmpty)
						{
							var parentKey = new UsageCodeKey(usageLine.PriceItem.L7_ParentCategory, parentCode);
							if (codeToHighestBreakPriceNode.TryGetValue(parentKey, out var priceNode)
								&& priceNode.Item.L7_FeeType == BillingConstants.FeeType.MinimumFee
								&& (priceItemPKFilter?.Contains(priceNode.Item.PK) ?? true))
							{
								if (!parentKeyToUsage.TryGetValue(parentKey, out var usageMap))
								{
									usageMap = new Dictionary<StlMonthlyUsage, List<UsageLine>>();
									parentKeyToUsage.Add(parentKey, usageMap);
								}

								List<UsageLine> usageLineList = null;
								if (!usageMap.TryGetValue(monthlyUsage, out usageLineList))
								{
									usageLineList = new List<UsageLine>(20);
									usageMap.Add(monthlyUsage, usageLineList);
								}

								usageLineList.Add(usageLine);
							}
						}
					}
				}
			}

			foreach (var parentKeyUsagePair in parentKeyToUsage)
			{
				var parentKey = parentKeyUsagePair.Key;
				var priceNode = codeToHighestBreakPriceNode[parentKey];
				var usageMap = parentKeyUsagePair.Value;
				IEnumerable<StlMonthlyUsage> monthlyUsagesContributing = usageMap.Keys;

				var customPrice = owner.DatabaseUsage.GetCustomPriceSetting(parentKey);
				var hasHighVolumeFeatureSetting = owner.DatabaseUsage.HasCustomHighVolumeFeatureSetting(parentKey);

				if (!ValidatePriceSettings(owner, owner.PriceHeader, priceNode.Item, customPrice, hasHighVolumeFeatureSetting))
				{
					continue;
				}

				var currencyToAmount = CalculateTotalPerCurrency(usageMap.Values.SelectMany(x => x));
				var minFeeCurrency = CalculateMinimumFeeCurrency(currencyToAmount, owner, owner.PriceHeader, priceNode, hasHighVolumeFeatureSetting);
				if (minFeeCurrency != null)
				{
					var payingMonthlyUsage = monthlyUsagesContributing.Contains(owner)
						? owner
						: usageMap.Where(x => x.Value.Any(y => y.PriceCurrency == minFeeCurrency)).Select(x => x.Key).First();

					var amountUsed = CalculateTotalInOneCurrency(currencyToAmount, minFeeCurrency, currencyExchange, branchPkToCompany[owner.InvoiceGroup.BranchPK.ToGuid()]);
					decimal minimumAmount = CalculatePriceInCurrency(payingMonthlyUsage, payingMonthlyUsage.DatabaseUsage, customPrice, hasHighVolumeFeatureSetting, payingMonthlyUsage.PriceList, priceNode, minFeeCurrency);
					if (minimumAmount <= 0)
					{
						return;
					}

					var feeAmount = minimumAmount - amountUsed;
					if (feeAmount > 0)
					{
						AddSimpleAmountUsageLine(feeAmount, minFeeCurrency, priceNode, payingMonthlyUsage, string.Empty);
					}
				}
			}
		}

		static UsageLine CalculateDisbursement(BusinessObjectFactory factory, ZDateTime periodStart, StlMonthlyUsage monthlyUsage, string currency, PriceNode priceNode,
			IEnumerable<Usage> usages,
			PriceList priceList)
		{
			var priceItemSet = priceList.ItemSet;
			var priceHeader = priceList.Header;
			var priceItem = priceNode.Item;
			var feeType = priceItem.L7_FeeType;
			if (feeType.IsEmpty)
			{
				feeType = BillingConstants.FeeType.Transactional;
			}

			var firstUsage = usages.First();
			var rawUnitCount = usages.Sum(x => x.UnitCount);
			var rawTotalPrice = usages.Sum(x => x.ChargeableUsage.U1_TotalPrice);
			decimal breakUnitCount = rawUnitCount;

			IEnumerable<PriceNode> nodes = new[] { priceNode };

			UsageLine mainUsageLine = null;
			var newUsageLines = new List<UsageLine>();
			foreach (var node in nodes)
			{
				var item = node.Item;
				mainUsageLine = new UsageLine(factory, periodStart, firstUsage.PeriodStart, firstUsage.AdditionalDescription, node);
				mainUsageLine.FormatOptions = FormatOptions.GetOptionsByProduct(monthlyUsage.Database?.LD_Product, factory);
				mainUsageLine.CountryCodeForDiscount = firstUsage.CountryCodeForDiscount;

				if (priceList.IncludeCargoWiseOneDiscounts && monthlyUsage.DatabaseUsage.StlPriceList != null)
				{
					mainUsageLine.DiscountVersionCode = monthlyUsage.DatabaseUsage.StlPriceList.Header.L6_DiscountCode;
				}

				mainUsageLine.TotalUnitCount = rawUnitCount;

				decimal licenceUnits = 0m;
				decimal unitPrice = 0m;
				if (!CalculatePriceAndLicenceUnits(monthlyUsage, priceHeader, item, currency, hasHighVolumeFeatureSetting: false, out unitPrice, out licenceUnits))
				{
					return null;
				}
				mainUsageLine.Price = 0m;
				mainUsageLine.TotalPrice = rawTotalPrice;
				mainUsageLine.Direction = firstUsage.ChargeableUsage.U1_Direction;
				mainUsageLine.IsDisbursement = true;
				mainUsageLine.PriceCurrency = currency;
				mainUsageLine.SetLicenceUnits(licenceUnits);

				if (item.L7_ChargeCode.IsEmpty)
				{
					monthlyUsage.AddRowError((NoResString)"No charge code for item " + item.CodeAndSubCodeForDisplay);
				}

				monthlyUsage.AddUsageLine(mainUsageLine);
				newUsageLines.Add(mainUsageLine);
			}

			if (mainUsageLine != null)
			{
				mainUsageLine.AddUsages(usages);
				foreach (var line in newUsageLines.Where(x => x != mainUsageLine))
				{
					line.ParentUsageLine = mainUsageLine;
				}
			}

			return mainUsageLine;
		}

		static bool ValidatePriceSettings(StlMonthlyUsage monthlyUsage, ClientLicencePriceHeader priceHeader, ClientLicencePriceItem priceItem, PriceLicenceSetting customPriceSetting, bool hasHighVolumeFeatureSetting)
		{
			if (customPriceSetting != null && hasHighVolumeFeatureSetting)
			{
				monthlyUsage.AddRowError(FormattableString.Invariant($"Price code {priceItem.CodeAndSubCodeForDisplay} has price setting and high volume setting in the same period."));
				return false;
			}

			var priceLink = monthlyUsage.DatabaseUsage.PriceHeaderLink;
			if (priceLink != null && priceHeader != null
				&& priceLink.PriceHeaderVersion == priceHeader.L6_PricelistVersion
				&& !priceLink.PHL_VolumeCode.EqualsIgnoringCase(EdiPriceHeaderLinkVolumeCodeList.Codes.STD)
				&& !priceHeader.L6_HasExchangeRates)
			{
				monthlyUsage.AddRowError(FormattableString.Invariant($"Price list {priceHeader.L6_PricelistVersion} is missing exchange rates."));
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode")]
		static UsageLine CalculatePricing(BusinessObjectFactory factory, ZDateTime periodStart, StlMonthlyUsage monthlyUsage, string currency, PriceNode priceNode, IEnumerable<Usage> usages,
			PriceList priceList,
			Dictionary<Guid, decimal> itemPkToPrice,
			PriceLicenceSetting customPriceSetting,
			bool hasHighVolumeFeatureSetting)
		{
			var priceItemSet = priceList.ItemSet;
			var priceHeader = priceList.Header;
			var priceItem = priceNode.Item;
			var feeType = priceItem.L7_FeeType;
			if (feeType.IsEmpty)
			{
				feeType = BillingConstants.FeeType.Transactional;
			}

			if (!ValidatePriceSettings(monthlyUsage, priceHeader, priceItem, customPriceSetting, hasHighVolumeFeatureSetting))
			{
				return null;
			}

			bool hasNamedUser = usages.Any(x => x.Code == BillingConstants.BillingSystem.ODM);

			// filter out named user usage unless this is charged per-database
			if (hasNamedUser &&
				!BillingConstants.FeeType.IsPerDatabaseInstance(feeType) &&
				feeType != BillingConstants.FeeType.Country &&
				feeType != BillingConstants.FeeType.UsersPerCountryVolumeBreak)
			{
				usages = usages.Where(x => x.Code != BillingConstants.BillingSystem.ODM);
				if (!usages.Any())
				{
					return null;
				}
			}

			var databaseCountryLanguageBillingHelper = factory.GetCachedValue("StlBilling.CalculatePricing.DatabaseCountryLanguageBillingHelper", () => new DatabaseCountryLanguageBillingHelper());
			if (!databaseCountryLanguageBillingHelper.ShouldBillPriceItem(priceItem, monthlyUsage.DatabaseUsage.CountryUsers))
			{
				return null;
			}

			var firstUsage = usages.First();
			var rawUnitCount = usages.Sum(x => x.UnitCount);
			if (ShouldConvertToHostingPriceUnits(firstUsage))
			{
				rawUnitCount = ConvertToHostingPriceUnits(feeType, priceItem.L7_Code, (int)rawUnitCount, monthlyUsage.PeriodStart);
			}
			var totalUnitCount = CalculateUnitCountAdjustedByFeeType(feeType, priceItem.L7_Code, rawUnitCount, monthlyUsage);
			decimal breakUnitCount = totalUnitCount;

			if (!priceItem.L7_UnitBreakParentCode.IsEmpty)
			{
				breakUnitCount = monthlyUsage.UsageLines
				   .Where(x => x.PriceItemCode == priceItem.L7_UnitBreakParentCode
						&& x.PriceItemCategory == priceItem.L7_Category
						&& firstUsage.PeriodStart == x.PeriodStart)
				   .Sum(x => (decimal)x.UnitCount);
			}

			IEnumerable<PriceNode> nodes = priceItem.L7_UnitBreak > 0
				? priceItemSet.AllNodesWithCode.Where(x => x.Item.L7_Category == priceItem.L7_Category &&
						x.Item.L7_Code == priceItem.L7_Code &&
						x.Item.L7_Ref4 == priceItem.L7_Ref4 &&
						x.Item.L7_UnitBreak < breakUnitCount)
					.OrderByDescending(item => item.Item.L7_UnitBreak)
				: (new[] { priceNode });

			if (BillingConstants.FeeType.IsVolumeBreak(priceItem.L7_FeeType))
			{
				if (nodes.Any(x => x.Item.L7_FeeType != priceItem.L7_FeeType))
				{
					monthlyUsage.AddRowError((NoResString)"Price code " + priceItem.CodeAndSubCodeForDisplay + (NoResString)" has multiple fee types including " + BillingConstants.GetCachedStlFeeTypeList(factory).GetDescriptionFromCode(priceItem.L7_FeeType));
					return null;
				}
			}

			// Custom price overrides volume price and only needs a single node - use the node with the lowest break.
			if (customPriceSetting != null && !customPriceSetting.SupportUnitBreak && nodes.Count() > 1)
			{
				nodes = new[] { nodes.Last() };
			}

			UsageLine mainUsageLine = null;
			var newUsageLines = new List<UsageLine>();
			foreach (var node in nodes)
			{
				if (totalUnitCount == 0)
				{
					break;
				}

				var item = node.Item;

				var unitCount = BillingConstants.FeeType.IsVolumeBreak(item.L7_FeeType)
					? totalUnitCount
					: Math.Max(0, totalUnitCount - item.L7_UnitBreak);

				if (item.L7_FeeType != priceItem.L7_FeeType)
				{
					unitCount = CalculateUnitCountAdjustedByFeeType(item.L7_FeeType, item.L7_Code, unitCount, monthlyUsage);
				}
				mainUsageLine = new UsageLine(factory, periodStart, firstUsage.PeriodStart, firstUsage.AdditionalDescription, node);
				mainUsageLine.FormatOptions = FormatOptions.GetOptionsByProduct(monthlyUsage.Database?.LD_Product, factory);
				mainUsageLine.CountryCodeForDiscount = firstUsage.CountryCodeForDiscount;
				if (priceList.IncludeCargoWiseOneDiscounts && monthlyUsage.DatabaseUsage.StlPriceList != null)
				{
					mainUsageLine.DiscountVersionCode = monthlyUsage.DatabaseUsage.StlPriceList.Header.L6_DiscountCode;
				}

				if (priceItem.L7_FeeType == BillingConstants.FeeType.UsersPerCountryVolumeBreak)
				{
					// UsersPerCountryVolumeBreak is derived from usage with a unit count of 1 for each country, i.e. ODM-GPC.
					// Originally this item was charged per country.
					// Now it is charged per country on a scale based on the average users per country.
					// So the charged units are the number of countries (i.e., usages.Sum(x => x.UnitCount))
					// and the break scale is users per country.
					mainUsageLine.TotalUnitCount = rawUnitCount;
				}
				else
				{
					mainUsageLine.TotalUnitCount = unitCount;
				}

				totalUnitCount -= unitCount;

				decimal price;
				if (customPriceSetting != null)
				{
					var priceSettingResult = customPriceSetting.SupportUnitBreak ? customPriceSetting.GetPriceAndUnits(item.L7_UnitBreak) : customPriceSetting.GetPriceAndUnits();
					if (priceSettingResult.Price.HasValue && priceSettingResult.Units.HasValue)
					{
						mainUsageLine.Price = priceSettingResult.Price.Value;
						if (customPriceSetting.EnableUnits)
						{
							mainUsageLine.SetLicenceUnits(priceSettingResult.Units.Value);
						}
					}
					else
					{
						mainUsageLine.Price = 0;
						monthlyUsage.AddRowError($"Price (Tier) Setting is missing for Price {item.L7_Code}, Unit Break {item.L7_UnitBreak}");
					}
					
					if (BillingConstants.PriceHeaderType.IsGlobal(priceHeader.L6_SystemCode) && !item.L7_RX_NKCurrency.IsEmpty)
					{
						mainUsageLine.PriceCurrency = item.L7_RX_NKCurrency;
					}
					else
					{
						mainUsageLine.PriceCurrency = currency;
					}
					mainUsageLine.ShouldApplyDiscounts = customPriceSetting.LS9_ApplyDiscounts;
				}
				else if (CanCalculateVariablePriceAndLicenceUnits(priceHeader, item, hasHighVolumeFeatureSetting))
				{
					decimal licenceUnits = 0;
					if (!CalculatePriceAndLicenceUnits(monthlyUsage, priceHeader, item, currency, hasHighVolumeFeatureSetting, out price, out licenceUnits))
					{
						return null;
					}
					mainUsageLine.Price = price;
					mainUsageLine.PriceCurrency = currency;
					mainUsageLine.SetLicenceUnits(licenceUnits);
				}
				else if (itemPkToPrice != null && itemPkToPrice.TryGetValue(item.PK.ToGuid(), out price))
				{
					mainUsageLine.Price = price;
					mainUsageLine.PriceCurrency = currency;
				}
				else if (priceList.HasSingleCurrency(item))
				{
					mainUsageLine.Price = item.L7_Price;
					mainUsageLine.PriceCurrency = !item.L7_RX_NKCurrency.IsEmpty ? item.L7_RX_NKCurrency : priceHeader.L6_RX_NKCurrency;
					if (mainUsageLine.Price == 0m && mainUsageLine.PriceCurrency != currency && item.L7_RX_NKCurrency.IsEmpty)
					{
						mainUsageLine.PriceCurrency = currency;
					}
				}
				else
				{
					monthlyUsage.AddRowError((NoResString)"No price for item " + item.CodeAndSubCodeForDisplay + (NoResString)" and currency " + currency);
				}

				if (item.L7_ChargeCode.IsEmpty)
				{
					monthlyUsage.AddRowError((NoResString)"No charge code for item " + item.CodeAndSubCodeForDisplay);
				}

				if (monthlyUsage.DatabaseUsage?.PriceHeaderLink?.PHL_CorePackCode.ToString() == EdiPriceHeaderLinkCorePackCodeList.Codes.EX
					&& item.L7_Code == DatabaseUsage.ActiveUsersUsageCode)
				{
					mainUsageLine.SetLicenceUnits(0);
				}

				monthlyUsage.AddUsageLine(mainUsageLine);
				newUsageLines.Add(mainUsageLine);
			}

			if (mainUsageLine != null)
			{
				mainUsageLine.AddUsages(usages);
				foreach (var line in newUsageLines.Where(x => x != mainUsageLine))
				{
					line.ParentUsageLine = mainUsageLine;
				}
			}

			return mainUsageLine;
		}

		static bool IsFixedCurrency(ClientLicencePriceHeader priceHeader, ClientLicencePriceItem item)
		{
			return !item.L7_RX_NKCurrency.IsEmpty
				&& item.L7_ExchangeRateGroupCode.IsEmpty
				&& priceHeader.L6_HasExchangeRates
				&& null == priceHeader.ExchangeRates.FindByGroupCodeAndCurrency(ZString.Empty, item.L7_RX_NKCurrency);
		}

		static int ConvertToHostingPriceUnits(string feeType, string priceCode, int rawUnitCount, ZDateTime periodStart)
		{
			if (priceCode == BillingConstants.Hosting.DataStorageCode
				|| priceCode == BillingConstants.Hosting.eDocsStorageCode
				|| priceCode == BillingConstants.Hosting.UltraFastStorageCode
				|| priceCode == BillingConstants.Hosting.NonProductionStorageCode)
			{
				rawUnitCount = Hosting.HostingStorageBillingSystem.SizeWithBuffer(rawUnitCount,
					periodStart,
					EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value,
					Hosting.HostingStorageBillingSystem.GetBufferPeriodStart());
			}

			return Hosting.HostingUsage.ConvertToFeeTypeUnits(feeType, rawUnitCount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static decimal CalculateUnitCountAdjustedByFeeType(string feeType, string priceCode, decimal rawUnitCount, StlMonthlyUsage stlMonthlyUsage)
		{
			decimal unitCount = 0;

			switch (feeType)
			{
				case BillingConstants.FeeType.Database:
				case BillingConstants.FeeType.DatabaseCountry:
				case BillingConstants.FeeType.DatabaseLanguage:
				case BillingConstants.FeeType.DatabaseLanguageZ:
					{
						unitCount = Math.Sign(rawUnitCount);
						break;
					}
				case BillingConstants.FeeType.Per10GBPerMonthMin1GB:
				case BillingConstants.FeeType.PerMBPerMonthMin1GB:
				case BillingConstants.FeeType.PerGBPerMonthMin1GB:
				case BillingConstants.FeeType.PerGBPerMonth:
				case BillingConstants.FeeType.Transactional:
				case BillingConstants.FeeType.TransactionalOneVolumeBreak:
				case BillingConstants.FeeType.PerDevicePerMonth:
				case BillingConstants.FeeType.MinimumFee:
				case BillingConstants.FeeType.MinimumFeePerReference:
				case BillingConstants.FeeType.Country:
				case BillingConstants.FeeType.CountryTier:
					{
						unitCount = rawUnitCount;
						break;
					}
				case BillingConstants.FeeType.UsersPerCountryVolumeBreak:
					{
						unitCount = rawUnitCount == 0 ? 0 : stlMonthlyUsage.DatabaseUsage.CountryUsers.DomesticCountryUserGroup.AverageUsersPerCountry;
						break;
					}
				default:
					{
						stlMonthlyUsage.AddRowError((NoResString)"Unknown fee type for price code " + priceCode + (NoResString)": " + feeType);
						break;
					}
			}

			return unitCount;
		}

		public static UsageCodeKey GetUsageKey(string usageCode, string usageSubCode)
		{
			string subCode = usageSubCode.Length != 0
				? usageSubCode
				: usageCode;

			return new UsageCodeKey(usageCode, subCode);
		}

		static bool ShouldConvertToHostingPriceUnits(Usage usage)
			=> !usage.Database.Factory.GetCachedValue("StlBilling.BillingUnitCountAdjustments", () =>
					EDIDataRegistry.Instance.BillingUnitCountAdjustments.Value.OfType<BillingUnitCountAdjustment>().Select(x => x.PriceCode).ToHashSet())
						.Contains(usage.SubCode);

		#endregion

		#region Properties

		#region Codes

		[BusinessObjectMaxLengthTestExclude()]
		[MaxLength(200)]
		public ZString CodesText
		{
			get { return codesText; }
			set
			{
				SetNonPersistentPropertyValue(CodesTextInfo, ref codesText, value);
				if (!IsValidationSuspended)
				{
					ValidateCodesText();
				}
			}
		}
		ZString codesText;

		public ZPropertyInfo CodesTextInfo
		{
			get { return GetZPropertyInfo(nameof(CodesText)); }
		}

		public void ValidateCodesText()
		{
			CodesTextInfo.ClearAllNotifications();
		}

		string[] ParseCodesText()
		{
			return ((string)CodesText).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
		}

		#endregion

		#region Accumulate

		public ZInt AccumulateMonths
		{
			get { return accumulateMonths; }
			set
			{
				SetNonPersistentPropertyValue(AccumulateMonthsInfo, ref accumulateMonths, value);
			}
		}
		ZInt accumulateMonths = 1;

		public ZPropertyInfo AccumulateMonthsInfo
		{
			get { return GetZPropertyInfo(nameof(AccumulateMonths)); }
		}

		#endregion

		#endregion

		#region Create Invoices

		public IEnumerable<InvoicingBase> CreateInvoices(StlBill bill)
		{
			return bill.CreateInvoicesWithoutSave(PostAndInvoiceDateOverride);
		}

		ZDateTime PostAndInvoiceDateOverride
		{
			get
			{
				ZDateTime postAndInvoiceDateOverride = ZDateTime.Empty;

				var backPost = IsBackPostAvailable && IsBackPostAllowed;

				if (backPost)
				{
					var today = ZDateTime.Today;
					postAndInvoiceDateOverride = today.AddDays(-today.Day);
				}

				return postAndInvoiceDateOverride;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Scope = "member", Target = "Enterprise.Client.EDI.Billing.Business.#CreateInvoices(StlBill[], IEdiProgress)")]
		public void CreateInvoices(StlBill[] selectedBills, IEdiProgress progress)
		{
			var postAndInvoiceDateOverride = PostAndInvoiceDateOverride;
			var invoicesCount = 0;

			using (var usSalesTaxCalculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				foreach (var branchBills in selectedBills.GroupBy(s => s.InvoicingBranch.PK))
				{
					using (BillingInvoicingHelper.BranchContext(branchBills.Key.ToGuid()))
					{
						foreach (var bill in branchBills)
						{
							if (progress.SafeIsCancelled())
							{
								return;
							}
							progress.SafeUpdateCurrentCount((NoResString)"Processing " + bill.OrganisationCode);

							bill.CreateInvoices(postAndInvoiceDateOverride, usSalesTaxCalculator: usSalesTaxCalculator);

							if (++invoicesCount % MaxInvoicesCountPerGC == 0)
							{
								GC.Collect();
							}
						}
					}
				}
			}
		}

		const int MaxInvoicesCountPerGC = 20;

		#endregion

		#region Price Header ExchangeRate

		static bool CalculatePriceAndLicenceUnits(
			StlMonthlyUsage monthlyUsage,
			ClientLicencePriceHeader priceHeader,
			ClientLicencePriceItem priceItem,
			ZString currency,
			bool hasHighVolumeFeatureSetting,
			out decimal price,
			out decimal licenceUnits)
		{
			return CalculatePriceAndLicenceUnits(monthlyUsage, monthlyUsage.DatabaseUsage, priceHeader, priceItem, currency, hasHighVolumeFeatureSetting, out price, out licenceUnits);
		}

		static bool CalculatePriceAndLicenceUnits(
			BusinessObject notificationOwner,
			DatabaseUsage dbUsage,
			ClientLicencePriceHeader priceHeader,
			ClientLicencePriceItem priceItem,
			ZString currency,
			bool hasHighVolumeFeatureSetting,
			out decimal price,
			out decimal licenceUnits)
		{
			var result = false;
			price = 0;
			licenceUnits = 0;

			if (priceHeader.L6_HasExchangeRates)
			{
				var priceHeaderLink = dbUsage.PriceHeaderLink;

				if (priceHeaderLink == null && !PriceHeaderType.IsGlobal(priceHeader.L6_SystemCode))
				{
					notificationOwner.AddRowError((NoResString)"Price Header Link is missing.");
				}
				else
				{
					if (hasHighVolumeFeatureSetting && priceHeaderLink?.PHL_VolumeCode.ToString() != EdiPriceHeaderLinkVolumeCodeList.Codes.HV)
					{
						notificationOwner.AddRowError((NoResString)"Can't have a High Volume feature without a price list link of type \"HV\" - \"High Volume\" in the same period.");
					}
					else
					{
						var priceHeaderExchangeRate = priceHeader.ExchangeRates.FindByGroupCodeAndCurrency(priceItem.L7_ExchangeRateGroupCode, currency);

						if (priceHeaderExchangeRate == null)
						{
							notificationOwner.AddRowError(FormattableString.Invariant($"Price list {priceHeader.L6_PricelistVersion} is missing exchange rate for {currency}, group {priceItem.L7_ExchangeRateGroupCode}."));
						}
						else
						{
							priceHeaderExchangeRate.CalculatePriceAndLicenceUnits(priceItem, priceHeaderLink, hasHighVolumeFeatureSetting, out price, out licenceUnits);
							result = true;
						}
					}
				}
			}
			else
			{
				notificationOwner.AddRowError(FormattableString.Invariant($"Price list {priceHeader.L6_PricelistVersion} is missing exchange rates."));
			}

			return result;
		}

		#endregion

		#region PriceItemFilter

		public bool IsPriceItemFilterApplicableOnReport { get; private set; }

		public FilterRuleProvider FilterProvider => filterProvider ?? (filterProvider = new FilterRuleProvider(ClientModuleRegistration.BillingPrices, this));
		FilterRuleProvider filterProvider;

		public StmModuleFilter PriceItemFilter => FilterProvider.GetOrCreateAndCacheFilter();

		public void ValidateFilterStrips() => RelatedModuleFiltersHelper.ValidateFilterStrips(PriceItemFilter, ClientModuleRegistration.BillingPrices);

		HashSet<ZGuid> GetPriceItemPKFilter()
		{
			var query = RelatedModuleFiltersHelper.GetFilterQuerySafe(PriceItemFilter);
			if (!query.IsNoResultQuery && !query.IsEmpty)
			{
				var whereClause = query.GetAsWhereClause(true);
				if (!string.IsNullOrEmpty(whereClause))
				{
					var sql = $@"SELECT {ClientLicencePriceItemSchema.Constants.PK} FROM {ClientLicencePriceItemSchema.Constants.TableName} {whereClause};";
					return DataUtils.GetListOfValuesFromQuery(Db.Connection, sql).Select(x => new ZGuid(x)).ToHashSet();
				}
			}
			return null;
		}

		#endregion
	}
}
