using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Fee
{
	public class FeeBillingSystem : BillingSystem
	{
		public FeeBillingSystem()
			: base()
		{
		}

		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.Fee; }
		}

		#endregion

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new FeeBill(Context.Factory);
		}

		protected override bool ShouldCreateSystemBillPerCurrency
		{
			get { return true; }
		}

		protected override bool ShouldLoadInvoicedUsagesForInactiveDatabases => true;

		#endregion

		#region Load Raw Usage

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			return new FeeRawUsage(context);
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context);
			rawUsage.Summary.Header.Column1 = "Error";
			var line = rawUsage.Summary.Lines.AddNew();
			line.Column1 = "Usage report is not available for STL billing";
			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			if (isStlBilling)
			{
				var dataCsvLine = new OCsvLine(new string[] { "Error", "Usage report is not available for STL billing" });
				action(dataCsvLine.ToString());
			}
			else
			{
				var headerCsvLine = new OCsvLine(new string[] { "Product Fees", "Amount" });
				action(headerCsvLine.ToString());

				var rawUsage = new FeeRawUsage(context);
				var summarySection = rawUsage.GetRawUsageSummarySections()[0];

				foreach (SummaryLine line in summarySection.Lines)
				{
					var dataValues = new string[] { line.MainDescription, line.Amount };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			if (!isStlBilling)
			{
				var rawUsage = new FeeRawUsage(context);
				var summarySection = rawUsage.GetRawUsageSummarySections()[0];

				foreach (SummaryLine line in summarySection.Lines)
				{
					writer.WriteCsvUsageReport(context.PeriodStartTimeUtc, context.CompanyCode, "", "", string.Concat(line.MainDescription, ' ', line.Amount).Trim(), context.PriceItemCode, context.PriceItemDescription, 1);
				}
			}
		}

		#endregion

		#region Load Usages

		/// <summary>
		/// No need to accumulate unbilled months since we assume fees will be above the minimum bill amount.
		/// </summary>
		protected override bool AccumulateUnbilledMonths
		{
			get { return false; }
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<FeeSystemUsage> result = new List<FeeSystemUsage>();

			var feeToUsageMap = BuildFeeToUsageMap(chargeableUsages);

			var dateFrom = Context.PeriodStart;
			ZDBOnlyQuery feeQuery = BuildFeesDueQuery(dateFrom, Context, false, false);
			ClientLicenceFee[] allFeesDue = Context.Factory.Load<ClientLicenceFee>(feeQuery);
			PopulateFeeAndRemitToFeeUsages(Context, result, dateFrom, allFeesDue, feeToUsageMap);

			if (!Context.OrganisationPK.IsEmpty)
			{
				ZDBOnlyQuery remitTofeeQuery = BuildFeesDueQuery(dateFrom, Context, true, false);
				ClientLicenceFee[] allRemitToFeesDue = Context.Factory.Load<ClientLicenceFee>(remitTofeeQuery);
				PopulateFeeAndRemitToFeeUsages(Context, result, dateFrom, allRemitToFeesDue, feeToUsageMap);
			}

			return result.ToArray();
		}

		internal static Dictionary<Guid, Guid> BuildFeeToUsageMap(ClientChargeableUsage[] chargeableUsages)
		{
			Dictionary<Guid, Guid> feeToUsageMap = new Dictionary<Guid, Guid>();
			foreach (var usage in chargeableUsages)
			{
				if (!usage.U1_Parent.IsEmpty)
				{
					feeToUsageMap.Add(usage.U1_Parent.ToGuid(), usage.PK.ToGuid());
				}
			}

			return feeToUsageMap;
		}

		internal static void PopulateFeeAndRemitToFeeUsages(BillingRunContext context, List<FeeSystemUsage> result, ZDateTime dateFrom, ClientLicenceFee[] feesDue, Dictionary<Guid, Guid> feeToUsageMap)
		{
			foreach (var orgGroup in feesDue.GroupBy(s => s.L8_LC))
			{
				var licCompany = context.Factory.Load<LicenceCompany>(orgGroup.Key);
				PopulateFeeUsage(context, result, dateFrom, feeToUsageMap, orgGroup, licCompany, false);
			}

			foreach (var orgGroup in feesDue.Where(s => !s.L8_OH_RemitToOrg.IsEmpty).GroupBy(s => s.L8_OH_RemitToOrg))
			{
				LicenceCompany licCompany = null;
				var remitToOrg = orgGroup.First().RemitToOrg as EDIOrgHeader;
				if (remitToOrg != null && remitToOrg.LicCompany != null)
				{
					licCompany = remitToOrg.LicCompany;
				}
				if (licCompany != null)
				{
					PopulateFeeUsage(context, result, dateFrom, feeToUsageMap, orgGroup, licCompany, true);
				}
			}
		}

		static void PopulateFeeUsage(BillingRunContext context,
			List<FeeSystemUsage> result,
			ZDateTime dateFrom,
			Dictionary<Guid, Guid> feeToUsageMap,
			IGrouping<ZGuid, ClientLicenceFee> orgGroup,
			LicenceCompany licCompany,
			bool isRemitUsage)
		{
			foreach (var currencyGroup in orgGroup.GroupBy(s => s.L8_RX_NKCurrency))
			{
				var fees = currencyGroup.ToArray();
				FeeSystemUsage feeUsage = new FeeSystemUsage(context.Factory, new UsingParty(licCompany.Header), dateFrom, fees, isRemitUsage);
				foreach (var fee in fees)
				{
					Guid usagePk;
					if (feeToUsageMap.TryGetValue(fee.PK.ToGuid(), out usagePk))
					{
						feeUsage.ChargeableUsagePKs.Add(usagePk);
					}
				}

				result.Add(feeUsage);
			}
		}

		static ZDBOnlyQuery BuildFeeDateRangeQuery(ZDateTime firstDayOfMonth)
		{
			ZDateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			ZDBOnlyQuery feeQuery = new ZDBOnlyQuery(typeof(ClientLicenceFee));
			feeQuery.AddToFilter(ClientLicenceFeeSchema.L8_SystemCode, BillingConstants.BillingSystem.ODM);
			ZQuery startDateQuery = new ZQuery(ClientLicenceFeeSchema.L8_StartDate, SQLComparisonOperator.LessThan, lastDayOfMonth);

			ZString sql = "(DATEDIFF(MONTH, L8_StartDate, @DateFrom) = (DATEDIFF(MONTH, L8_StartDate, @DateFrom) / L8_RenewalMonths * L8_RenewalMonths))";
			ZSqlParameterCollection paramList = new ZSqlParameterCollection();
			paramList.Add("@DateFrom", firstDayOfMonth, ClientLicenceFeeSchema.L8_StartDate);
			startDateQuery.AddFilterAndZSQLParameterCollection(sql, paramList);

			// include empty start dates only if the fee is every month
			ZQuery emptyStartDateQuery = new ZQuery(ClientLicenceFeeSchema.L8_StartDate, ZDateTime.Empty);
			emptyStartDateQuery.AddToFilter(ClientLicenceFeeSchema.L8_RenewalMonths, 1);

			startDateQuery.AddToFilter(emptyStartDateQuery, JoinCondition.Or);

			ZQuery endDateQuery = new ZQuery(ClientLicenceFeeSchema.L8_EndDate, ZDateTime.Empty);
			endDateQuery.AddToFilter(JoinCondition.Or, ClientLicenceFeeSchema.L8_EndDate, SQLComparisonOperator.GreaterThan, firstDayOfMonth);

			feeQuery.AddToFilter(startDateQuery);
			feeQuery.AddToFilter(endDateQuery);

			return feeQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public static ZDBOnlyQuery BuildFeesDueQuery(ZDateTime firstDayOfMonth, BillingRunContext context, bool isRemitTo, bool isStl)
		{
			ZDBOnlyQuery feeQuery = BuildFeeDateRangeQuery(firstDayOfMonth);

			if (isRemitTo)
			{
				if (!context.OrganisationPK.IsEmpty)
				{
					feeQuery.AddToFilter(ClientLicenceFeeSchema.L8_OH_RemitToOrg, context.OrganisationPK);
				}
				else
				{
					feeQuery.AddToFilter(ClientLicenceFeeSchema.L8_OH_RemitToOrg, SQLComparisonOperator.NotEqual, null);
				}
			}

			if ((!isRemitTo && !context.OrganisationPK.IsEmpty)
				|| !string.IsNullOrEmpty(context.EnterpriseCode))
			{
				ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientLicenceFeeSchema.L8_LC);

				if (!isRemitTo && !context.OrganisationPK.IsEmpty)
				{
					licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_OH, context.BillingGroupOrgPKs);
				}

				if (!string.IsNullOrEmpty(context.EnterpriseCode))
				{
					var entSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
					entSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, context.EnterpriseCode);
					licenceCompanySubQuery.AddSubQuery(entSubQuery, JoinCondition.And);
				}

				feeQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
			}

			var midMonthAsText = firstDayOfMonth.AddDays(15).SqlFormat;
			string sql = @"
((
	-- fees with no database:
	--		linked to companies with no active database are considered to be STL
	--		linked to companies with all active databases STL are considered to be STL
	--		otherwise they are linked to a company with at least one non-STL database and are considered to be ODPL
	L8_LD is null
	and
	L8_LC " + (isStl ? "not " : "") + @"in
	(
		-- all companies that have at least one active non-STL database
		select LA_LC
		from
		(
			select LA_LC
				, HasNonStl = MAX(case when LA_LicenceAdvStdOth != 'STL' and PHL_PK is null then 1 else 0 end)
			from dbo.LicenceHeader
			join dbo.LicenceDatabase on LA_LD = LD_PK
			left join dbo.EdiPriceHeaderLink on PHL_LD = LD_PK and PHL_ValidFrom < '" + midMonthAsText + @"'
			where LA_IsActive = 1 and LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'PRW') and LD_IsActive = 1 and LD_LicenceType = 'PRD'
			group by LA_LC
		) a
		where HasNonStl = 1
	)
)
or
(
	L8_LD is not null
	and L8_LD " + (isStl ? "" : "not ") + @"in
	(
		select LA_LD
		from
		(
			select LA_LD
				, HasStl = MAX(case when LA_LicenceAdvStdOth = 'STL' or PHL_PK is not null then 1 else 0 end)
				, HasActiveNonStl = MAX(case when LA_IsActive = 1 and LD_IsActive = 1 and LA_LicenceAdvStdOth != 'STL' and PHL_PK is null then 1 else 0 end)
			from dbo.LicenceHeader
			join dbo.LicenceDatabase on LA_LD = LD_PK
			left join dbo.EdiPriceHeaderLink on PHL_LD = LD_PK and PHL_ValidFrom < '" + midMonthAsText + @"'
			group by LA_LD
		) b
		where HasStl = 1 and HasActiveNonStl = 0
	)
))";

			feeQuery.AddFilterAndZSQLParameterCollection(sql, null);
			return feeQuery;
		}

		#endregion
	}
}

