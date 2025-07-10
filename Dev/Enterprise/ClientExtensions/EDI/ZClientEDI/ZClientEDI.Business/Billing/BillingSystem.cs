using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class BillingSystem
	{
		protected BillingSystem()
		{
			IsEnabled = true;
		}

		public const string MessageTimeFormat = BillingConstants.MessageTimeFormat;
		public static string ToMessageTimeFormat(DateTime messageTimeUtc) => messageTimeUtc.ToString(MessageTimeFormat, CultureInfo.InvariantCulture);
		public static string ToMessageTimeFormat(ZDateTime messageTimeUtc) => messageTimeUtc.ToString(MessageTimeFormat, CultureInfo.InvariantCulture);

		public bool IsEnabled { get; set; }

		#region System Code

		public abstract string SystemCode { get; }

		#endregion

		#region Billing Context

		protected BillingRunContext Context { get; private set; }

		void SetContext(BillingRunContext context)
		{
			Argument.NotNull(context, "context");
			Context = context;

			if (SystemUsage.MiscOrganisationPK.IsEmpty)
			{
				// Loading MiscOrganisationPK. Exception during reading from DataReader if not pre-loaded
			}

			UpdateProgressStatus("Loading usage...", 0);
		}

		#endregion

		#region Load Raw Usage

		public abstract SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context);

		public abstract StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context);

		protected ClientChargeableUsage[] LoadRawChargeableUsages(BillingLoadRawUsageContext context)
		{
			var query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			query.AddToFilter(ClientChargeableUsageSchema.U1_Code, SystemCode);
			query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, context.Period);

			var ownerQuery = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			ownerQuery.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_LCC, context.ClientCompanyPK);
			ownerQuery.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_LC, context.LicenceCompanyPK);
			ownerQuery.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_LD, context.DatabasePK);
			query.AddToFilter(ownerQuery, JoinCondition.And);

			return context.Factory.Load<ClientChargeableUsage>(query);
		}

		public abstract void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action);

		public abstract void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer);

		#endregion

		#region Load System Bills

		public SystemBill[] LoadSystemBills(BillingRunContext context)
		{
			SetContext(context);
			UpdateProgressStatus("Loading usage...", 20);
			ClientChargeableUsage[] chargeableUsages = LoadChargeableUsages();

			AddFetchHintsForLoadSystemBills(chargeableUsages);

			SystemUsage[] systemUsages = CreateSystemUsages(chargeableUsages);
			systemUsages = GetBillableSystemUsages(systemUsages);

			UpdateProgressStatus("Calculating amounts...", 40);
			foreach (SystemUsage systemUsage in systemUsages)
			{
				if (context.IsPreviewOnly)
				{
					systemUsage.SetPreviewOnly(context.PreviewPriceHeader);
				}
				systemUsage.CalculateAmount();
			}

			UpdateProgressStatus("Calculating groups...", 70);
			SystemBill[] systemBills = CreateSystemBills(systemUsages);

			return systemBills;
		}

		void AddFetchHintsForLoadSystemBills(IEnumerable<ClientChargeableUsage> chargeableUsages)
		{
			var factory = Context.Factory;
			foreach (var databasePk in chargeableUsages.Where(x => !x.U1_LD.IsEmpty).Select(x => x.U1_LD).Distinct())
			{
				factory.AddFetchHint(LicenceDatabaseSchema.Constants.TableName, databasePk);
			}
			foreach (var clientCompanyPk in chargeableUsages.Where(x => !x.U1_LCC.IsEmpty).Select(x => x.U1_LCC).Distinct())
			{
				factory.AddFetchHint(ClientCompanySchema.Constants.TableName, clientCompanyPk);
			}
			foreach (var licenceCompanyPk in chargeableUsages.Where(x => !x.U1_LC.IsEmpty).Select(x => x.U1_LC).Distinct())
			{
				factory.AddFetchHint(LicenceCompanySchema.Constants.TableName, licenceCompanyPk);
			}
		}

		protected virtual SystemUsage[] GetBillableSystemUsages(SystemUsage[] systemUsages)
		{
			foreach (var usage in systemUsages)
			{
				usage.LicenceMode = CalculateLicenceMode(Context.Factory, usage.User, Context.PeriodStart);
			}

			if (!Context.IncludeStl || !Context.IncludeOdpl)
			{
				return systemUsages.Where(x => IsValidLicenceMode(x.LicenceMode)).ToArray();
			}

			return systemUsages;
		}

		internal static string CalculateLicenceMode(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			string mode = "";
			var lic = user.UsageOwnerLicence;
			if (lic != null)
			{
				mode = lic.LA_LicenceAdvStdOth == "STL" || lic.Database.PriceHeaderLinkForDate(periodStart.AddDays(15)) != null
					? MonthlyUsageBilling.LicenceModeConstants.Codes.STL
					: MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
			}
			else if (!user.LicenceCompanyPK.IsEmpty)
			{
				var licCompany = factory.Load<LicenceCompany>(user.LicenceCompanyPK);
				if (licCompany != null)
				{
					bool? allStl = null;
					foreach (LicenceHeader licHeader in licCompany.LicHeadersForAllDatabases.Where(x => x.LA_IsActive))
					{
						var db = licHeader.Database;
						if (db.LD_IsActive && db.LD_LicenceType == DatabaseTypes.Codes.Production
							&& db.IsEnterpriseFamilyDatabase)
						{
							bool isStl = licHeader.LA_LicenceAdvStdOth == "STL"
								|| licHeader.Database.PriceHeaderLinkForDate(periodStart.AddDays(15)) != null;
							if (!allStl.HasValue)
							{
								allStl = isStl;
							}
							else if (allStl.Value != isStl)
							{
								mode = MonthlyUsageBilling.LicenceModeConstants.Codes.ALL;
								allStl = null;
								break;
							}
						}
					}

					if (allStl.HasValue)
					{
						mode = allStl.Value ? MonthlyUsageBilling.LicenceModeConstants.Codes.STL : MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
					}
				}
			}

			return mode;
		}

		protected bool IsValidLicenceMode(string licenceMode)
		{
			bool isOdpl = licenceMode != MonthlyUsageBilling.LicenceModeConstants.Codes.STL;
			bool isStl = licenceMode == MonthlyUsageBilling.LicenceModeConstants.Codes.STL || licenceMode == MonthlyUsageBilling.LicenceModeConstants.Codes.ALL;
			return (Context.IncludeStl && isStl)
				|| (Context.IncludeOdpl & isOdpl);
		}

		protected virtual bool AccumulateUnbilledMonths
		{
			get { return !Context.IsPreviewOnly; }
		}

		protected virtual ZDateTime EarliestUsageToAccumulate
		{
			get { return Context?.PeriodStart.AddMonths(-12) ?? new ZDateTime(2010, 11, 1); }// Usage before this date was billed using other methods
		}

		protected virtual void AddPeriodStartFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			ZDateTime dateToExclusive = Context.DateToInclusive.AddDays(1);
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, SQLComparisonOperator.LessThan, dateToExclusive);

			ZDateTime dateFrom = Context.PeriodStart;

			if (AccumulateUnbilledMonths && dateFrom > EarliestUsageToAccumulate)
			{
				dateFrom = EarliestUsageToAccumulate;
			}

			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, SQLComparisonOperator.GreaterThanOrEqualTo, dateFrom);
		}

		protected virtual void AddAccumulateUnbilledMonthsFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			if (AccumulateUnbilledMonths)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
				query.AddToFilter(ClientChargeableUsageSchema.U1_AH_Invoice, DBNull.Value);

				// always include the current month, even if invoiced
				ZDateTime currentPeriodStart = Context.PeriodStart;
				query.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_PeriodStart, currentPeriodStart);

				ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), ClientChargeableUsageSchema.U1_AH_Invoice);
				invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, true);
				query.AddSubQuery(invoiceSubQuery, JoinCondition.Or);

				chargeableUsageQuery.AddToFilter(query);
			}
		}

		protected virtual void AddAdditionalFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
		}

		protected virtual ClientChargeableUsage[] LoadChargeableUsages()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			query.AddToFilter(ClientChargeableUsageSchema.U1_ManuallyProcessed, false);
			AddCodeFilter(query);

			AddPeriodStartFilter(query);
			AddAccumulateUnbilledMonthsFilter(query);
			AddAdditionalFilter(query);

			if (Context.IsPreviewOnly)
			{
				query.AddToFilter(ClientChargeableUsageSchema.U1_LD, Context.BillingGroupDatabasePKs);
			}
			else if (!Context.OrganisationPK.IsEmpty)
			{
				var orgQuery = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
				var licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientChargeableUsageSchema.U1_LC);
				licenceCompanySubQuery.AddToFilter(JoinCondition.Or, LicenceCompanySchema.LC_OH, SQLComparisonOperator.Equal, Context.BillingGroupOrgPKs);

				orgQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.Or);
				orgQuery.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_LD, Context.BillingGroupDatabasePKs);

				query.AddToFilter(orgQuery, JoinCondition.And);
			}

			var licDatabaseQuery = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			licDatabaseQuery.AddToFilter(JoinCondition.Or, ClientChargeableUsageSchema.U1_LD, null);
			var licDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
			licDatabaseQuery.AddSubQuery(ClientChargeableUsageSchema.U1_LD, licDatabaseSubQuery, JoinCondition.Or);

			if (ShouldLoadInvoicedUsagesForInactiveDatabases)
			{
				var invoicedUsageQuery = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
				invoicedUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_AH_Invoice, SQLComparisonOperator.NotEqual, null);
				var inactiveDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
				inactiveDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, false);
				invoicedUsageQuery.AddSubQuery(ClientChargeableUsageSchema.U1_LD, inactiveDatabaseSubQuery, JoinCondition.And);
				licDatabaseQuery.AddToFilter(invoicedUsageQuery, JoinCondition.Or);
			}

			query.AddToFilter(licDatabaseQuery, JoinCondition.And);

			return Context.Factory.Load<ClientChargeableUsage>(query);
		}

		protected virtual void AddCodeFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, SystemCode);
		}

		protected abstract SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages);

		SystemBill[] CreateSystemBills(SystemUsage[] systemUsages)
		{
			List<SystemBill> result = new List<SystemBill>(systemUsages.Length);

			foreach (var currencyGroup in systemUsages.GroupBy(x => ShouldCreateSystemBillPerCurrency ? x.CurrencyCode : ZString.Empty))
			{
				foreach (var usagesGroupedBy in currencyGroup.GroupBy(x => ClientInvoiceDelivery.CreateGroup(x.InvoiceDelivery, x.InvoicedOrganisationPK, x.LicCompany, x.ServerCode, x.IsBillable)))
				{
					SystemBill systemBill = CreateSystemBill();
					systemBill.BillingPeriod = Context.PeriodStart;
					systemBill.PopulateFromSystemUsages(usagesGroupedBy
						.OrderBy(s => s.PeriodStart)
						.ThenBy(s => SortOrder(s.User.UsageOwnerLicence))
						.ToArray());
					systemBill.InvoiceGroupKey = usagesGroupedBy.Key;
					result.Add(systemBill);
				}
			}

			return result.ToArray();
		}

		static int SortOrder(LicenceHeader lic)
		{
			return lic != null && lic.Database.LD_LicenceType == DatabaseTypes.Codes.Production
				? 0
				: 1;
		}

		protected abstract SystemBill CreateSystemBill();

		protected virtual bool ShouldCreateSystemBillPerCurrency
		{
			get { return false; }
		}

		protected virtual bool ShouldLoadInvoicedUsagesForInactiveDatabases => false;

		#endregion

		#region Implementation

		protected ZDateTime GetStartDate(ZDateTime dateToInclusive)
		{
			return dateToInclusive.AddDays(1).AddMonths(-1);
		}

		protected virtual void UpdateProgressStatus(string status, int percentComplete)
		{
			if (Context != null && Context.Progress != null)
			{
				Context.Progress.SetStatusAndPercentComplete(SystemDescription + ": " + status, percentComplete);
			}
		}

		public string SystemDescription
		{
			get { return systemDescription ?? (systemDescription = BillingConstants.BillingSystemList.GetDescriptionFromCode(SystemCode)); }
		}
		string systemDescription;

		#endregion

		protected static Dictionary<string, ClientLicencePriceItem> BuildCodeToPriceItemMap(ClientLicencePriceItemCollection items, bool mapIncludedToParent)
		{
			Dictionary<string, ClientLicencePriceItem> result = new Dictionary<string, ClientLicencePriceItem>();

			// Non-included items. Item with highest break will be chosen.
			foreach (var priceItem in items.Where(x => !x.L7_Code.IsEmpty
				&& !x.L7_FeeType.IsEmpty
				&& (!mapIncludedToParent || x.L7_FeeType != BillingConstants.FeeType.Included)
				).OrderBy(x => x.L7_UnitBreak))
			{
				result[priceItem.L7_Code] = priceItem;
			}

			// included items
			if (mapIncludedToParent)
			{
				foreach (var priceItem in items.Where(x => !x.L7_Code.IsEmpty
					&& !x.L7_FeeType.IsEmpty
					&& x.L7_FeeType == BillingConstants.FeeType.Included))
				{
					ClientLicencePriceItem mainPriceItem;
					if (result.TryGetValue(priceItem.L7_ParentCode, out mainPriceItem))
					{
						result[priceItem.L7_Code] = mainPriceItem;
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected static void BuildPriceMaps(IEnumerable<SystemUsage> systemUsages, Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>> priceMaps, bool mapIncludedToParent)
		{
			foreach (var usage in systemUsages.Where(x => x.PriceHeader != null))
			{
				var items = usage.PriceHeader.LocalOrStandardItems;
				var masterPriceHeader = items.Master;
				if (!priceMaps.ContainsKey(masterPriceHeader.PK))
				{
					priceMaps[masterPriceHeader.PK] = BuildCodeToPriceItemMap(items, mapIncludedToParent);
				}
			}
		}

		public virtual void OnChargeableUsageUpdate(BillingPeriod billingPeriod, ILogger logger)
		{
		}
	}
}

