using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing
{
	public class PremiumServiceBillingSystem : BillingSystem
	{
		public PremiumServiceBillingSystem()
		{
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.Service; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<PremiumServiceUsage> result = new List<PremiumServiceUsage>();

			var dateFrom = GetStartDate(Context.DateToInclusive);
			ZDBOnlyQuery query = BuildDueQuery(dateFrom, Context);
			var allDue = Context.Factory.Load<ClientPremiumService>(query);

			foreach (var dbGroup in allDue.GroupBy(s => s.CPS_LD))
			{
				var db = Context.Factory.Load<LicenceDatabase>(dbGroup.Key);
				var licOwner = db.UsageOwnerOrFirstLicence;

				if (licOwner != null)
				{
					foreach (var serviceTypeGroup in dbGroup.GroupBy(x => new { x.UsageOwner, x.CPS_PriceHeaderCode, x.CPS_Type }))
					{
						var usingParty = (serviceTypeGroup.Key.UsageOwner?.Org) == null ? new UsingParty(licOwner) : new UsingParty(serviceTypeGroup.Key.UsageOwner);
						var licenceCompanyPK = new[] { usingParty.LicenceCompanyPK, licOwner.LA_LC }.First(x => !x.IsEmpty);

						ClientPremiumService[] services = serviceTypeGroup.ToArray();
						var usages = chargeableUsages.Where(x => services.Any(s => s.PK == x.U1_Parent)
										&& x.U1_LC == licenceCompanyPK);

						var usage = new PremiumServiceUsage(Context.Factory, usingParty, dateFrom, services);
						usage.SubCode = serviceTypeGroup.Key.CPS_Type;

						if (Context.IsPreviewOnly)
						{
							usage.SetPreviewOnly(Context.PreviewPriceHeader);
						}
						usage.ChargeableUsagePKs.AddRange(usages.Select(x => x.PK));
						var priceItem = usage.PriceItem;
						if (priceItem != null && BillingConstants.FeeType.IsPerDatabaseUser(priceItem.L7_FeeType))
						{
							var dbUsers = Context.DatabaseUsersService;
							int userCount = dbUsers.DatabaseMonthlyUserCount(usage.DatabasePK, dateFrom);
							if (userCount != 0)
							{
								usage.FeeTypeUnitCount = userCount;
								result.Add(usage);
							}
						}
						else
						{
							result.Add(usage);
						}
					}
				}
			}

			return result.Where(x => x.FeeTypeUnitCount != 0).ToArray();
		}

		protected override bool ShouldLoadInvoicedUsagesForInactiveDatabases => true;

		public static ClientPremiumService[] GetDue(BusinessObjectFactory factory, ZDateTime firstDayOfMonth, Guid[] databasePks, string[] codesFilter)
		{
			ZDateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientPremiumService));
			if (codesFilter != null && codesFilter.Length > 0)
			{
				query.AddToFilter(ClientPremiumServiceSchema.CPS_Type, codesFilter);
			}
			ZQuery startDateQuery = new ZQuery(ClientPremiumServiceSchema.CPS_StartDate, SQLComparisonOperator.LessThan, lastDayOfMonth);

			ZQuery endDateQuery = new ZQuery(ClientPremiumServiceSchema.CPS_EndDate, ZDateTime.Empty);
			endDateQuery.AddToFilter(JoinCondition.Or, ClientPremiumServiceSchema.CPS_EndDate, SQLComparisonOperator.GreaterThan, firstDayOfMonth);

			query.AddToFilter(startDateQuery);
			query.AddToFilter(endDateQuery);

			ZDBOnlySubQuery dbSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientPremiumServiceSchema.CPS_LD);
			dbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);
			dbSubQuery.AddToFilter(LicenceDatabaseSchema.PK, databasePks);
			query.AddSubQuery(dbSubQuery, JoinCondition.And);

			if (EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.Count > 0)
			{
				var dummyCodes = EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.GetAllCodes();
				query.AddToFilter(ClientPremiumServiceSchema.CPS_Type, SQLComparisonOperator.NotEqual, dummyCodes);
			}

			return factory.Load<ClientPremiumService>(query);
		}

		ZDBOnlyQuery BuildDueQuery(ZDateTime firstDayOfMonth, BillingRunContext context)
		{
			ZDateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientPremiumService));
			ZQuery startDateQuery = new ZQuery(ClientPremiumServiceSchema.CPS_StartDate, SQLComparisonOperator.LessThan, lastDayOfMonth);

			ZQuery endDateQuery = new ZQuery(ClientPremiumServiceSchema.CPS_EndDate, ZDateTime.Empty);
			endDateQuery.AddToFilter(JoinCondition.Or, ClientPremiumServiceSchema.CPS_EndDate, SQLComparisonOperator.GreaterThan, firstDayOfMonth);

			query.AddToFilter(startDateQuery);
			query.AddToFilter(endDateQuery);

			ZDBOnlySubQuery dbSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientPremiumServiceSchema.CPS_LD);
			dbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);

			if (!context.OrganisationPK.IsEmpty)
			{
				dbSubQuery.AddToFilter(LicenceDatabaseSchema.PK, context.BillingGroupDatabasePKs);
			}

			ZDBOnlySubQuery headerSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LD);
			headerSubQuery.AddToFilter(LicenceHeaderSchema.LA_IsActive, ZBool.True);
			dbSubQuery.AddSubQuery(headerSubQuery, JoinCondition.And);
			query.AddSubQuery(dbSubQuery, JoinCondition.And);

			if (EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.Count > 0)
			{
				var dummyCodes = EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.GetAllCodes();
				query.AddToFilter(ClientPremiumServiceSchema.CPS_Type, SQLComparisonOperator.NotEqual, dummyCodes);
			}

			return query;
		}

		#region Load Raw Usage

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			var org = context.Factory.Load<EDIOrgHeader>(context.OrganisationPK);
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, SystemCode);
			rawUsage.Summary.Header.Column1 = "Service";
			rawUsage.Summary.Header.Column2 = "Reference";
			rawUsage.Summary.Header.Column3 = "Unit Count";

			var chargeableUsages = LoadRawChargeableUsages(context);
			var serviceQuery = new ZQuery(ClientPremiumServiceSchema.PK, chargeableUsages.Select(x => x.U1_Parent));
			var matchedPremiumServices = context.Factory.Load<ClientPremiumService>(serviceQuery);

			foreach (var servicesPerDb in matchedPremiumServices.GroupBy(x => x.Database).OrderBy(x => x.Key.LD_ServerCode))
			{
				LicenceDatabase db = servicesPerDb.Key;
				var priceHeader = OdplUsage.PriceHeaderForDate(context.Period, org, org.LicCompany.InvoiceDeliveries.FindByServerAndSystem(db.LD_ServerCode, SystemCode));

				foreach (var servicesPerType in servicesPerDb.GroupBy(x => (x.CPS_PriceHeaderCode, x.CPS_Type)).OrderBy(y => y.Min(z => z.CPS_DisplayOrder)))
				{
					ClientLicencePriceItem priceItem = null;
					if (servicesPerType.Key.CPS_PriceHeaderCode.IsEmpty)
					{
						priceItem = priceHeader?.LocalOrStandardItems.FindByCode(servicesPerType.Key.CPS_Type);
					}
					else
					{
						priceItem = LicenceCompany.StandardPricesCompany?.PriceHeaderForDate(chargeableUsages[0].U1_PeriodStart, servicesPerType.Key.CPS_PriceHeaderCode)
							?.Items.FindByCode(servicesPerType.Key.CPS_Type);
					}

					string priceDescription = priceItem != null ? priceItem.L7_DescriptionLocalized : ZString.Empty;

					bool isPerDatabaseUser = priceItem != null && BillingConstants.FeeType.IsPerDatabaseUser(priceItem.L7_FeeType);
					IEnumerable<string> userList = isPerDatabaseUser ? context.DatabaseUsersService.DatabaseMonthlyUserList(db.PK, context.Period) : null;
					int feeUnits = userList != null ? userList.Count() : 1;
					if (isPerDatabaseUser)
					{
						rawUsage.Summary.Header.Column2 = "Reference / Staff Name";
					}

					foreach (ClientPremiumService service in servicesPerType.OrderBy(x => x.CPS_DisplayOrder))
					{
						SummaryLine line = rawUsage.Summary.Lines.AddNew();
						line.Column1 = priceDescription;
						line.Column2 = service.CPS_ClientRef;
						line.Column3 = (service.CPS_Units * feeUnits).ToString(CultureInfo.InvariantCulture);
					}

					if (userList != null)
					{
						foreach (var user in userList)
						{
							var line = rawUsage.Summary.Lines.AddNew();
							line.Column1 = "        " + user;
						}
					}
				}
			}

			return rawUsage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			rawUsage.Summary.Header.Column1 = "Service";
			rawUsage.Summary.Header.Column2 = "Reference";
			rawUsage.Summary.Header.Column3 = "Unit Count";

			var chargeableUsages = LoadRawChargeableUsages(context);
			if (chargeableUsages.Length > 0)
			{
				var db = chargeableUsages[0].Database;
				if (db != null)
				{
					var services = Enumerable.Empty<ClientPremiumService>();

					//usages created from temporary service (#NP, #HP) by Enterprise.Client.EDI.Billing.Business.SystemLicenceBilling
					var usageFromAdditionalService = chargeableUsages.FirstOrDefault(u => u.U1_SubCode == context.PriceItemCode
															&& !u.U1_Parent.IsEmpty
															&& (u.U1_Parent == u.U1_LD || context.Factory.ExistsInDatabase(LicenceDatabaseSchema.Constants.TableName, new ZQuery(LicenceDatabaseSchema.PK, u.U1_Parent))));
					if (usageFromAdditionalService != null)
					{
						var additionalService = new BusinessObjectFactory().New<ClientPremiumService>();
						additionalService.CPS_Type = context.PriceItemCode;
						additionalService.CPS_Units = decimal.ToInt16(usageFromAdditionalService.U1_UnitCount);
						additionalService.CPS_ClientRef = $"Server {context.Factory.Load<LicenceDatabase>(usageFromAdditionalService.U1_Parent).LD_ServerCode}";
						services = new[] { additionalService };
					}
					else
					{
						services = context.Factory.Load<ClientPremiumService>(new ZQuery(ClientPremiumServiceSchema.PK, chargeableUsages.Select(x => x.U1_Parent)));
					}

					var priceLink = db.PriceHeaderLinkForDate(chargeableUsages[0].U1_PeriodStart);

					foreach (var servicesPerType in services.GroupBy(x => (x.CPS_PriceHeaderCode, x.CPS_Type)).OrderBy(y => y.Min(z => z.CPS_DisplayOrder)))
					{
						var prices = !servicesPerType.Key.CPS_PriceHeaderCode.IsEmpty
							? LicenceCompany.StandardPricesCompany?.PriceHeaderForDate(chargeableUsages[0].U1_PeriodStart, servicesPerType.Key.CPS_PriceHeaderCode)
							: priceLink?.PriceHeader;

						var priceItem = prices?.Items.FindByCode(servicesPerType.Key.CPS_Type);
						string priceDescription = priceItem != null ? priceItem.L7_DescriptionLocalized : ZString.Empty;

						foreach (var service in servicesPerType.OrderBy(x => x.CPS_DisplayOrder))
						{
							int feeUnits = 1;
							SummaryLine line = rawUsage.Summary.Lines.AddNew();
							line.Column1 = priceDescription;
							line.Column2 = service.CPS_ClientRef;
							line.Column3 = (service.CPS_Units * feeUnits).ToString(CultureInfo.InvariantCulture);
						}
					}
				}
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			SummarySection summarySection;
			if (isStlBilling)
			{
				var stlRawUsage = LoadStlRawUsage(context);
				summarySection = stlRawUsage.Summary;
			}
			else
			{
				var odplRawUsage = LoadOdplRawUsage(context) as SystemCodeRawUsage;
				summarySection = odplRawUsage.Summary;
			}

			var headerColumns = new string[] { summarySection.Header.Column1, summarySection.Header.Column2, summarySection.Header.Column3 };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			foreach (SummaryLine line in summarySection.Lines)
			{
				string[] dataValues = new string[] { line.Column1, line.Column2, line.Column3 };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			SummarySection summarySection;
			if (isStlBilling)
			{
				var stlRawUsage = LoadStlRawUsage(context);
				summarySection = stlRawUsage.Summary;
			}
			else
			{
				var odplRawUsage = LoadOdplRawUsage(context) as SystemCodeRawUsage;
				summarySection = odplRawUsage.Summary;
			}

			foreach (SummaryLine line in summarySection.Lines)
			{
				writer.WriteCsvUsageReport(context.PeriodStartTimeUtc, context.CompanyCode, "", "", string.Concat(line.Column1, ' ', line.Column2, ' ', line.Column3).Trim(), context.PriceItemCode, context.PriceItemDescription, 1);
			}
		}

		#endregion

		#region System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new PremiumServiceBill(Context.Factory);
		}

		#endregion
	}
}

