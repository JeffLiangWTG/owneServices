using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Preview;
using Enterprise.Client.EDI.Billing.Fee;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlPreviewLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlBillForBindingOnly StlBillForBinding { get; private set; }
		public ZString OrganisationCode { get; private set; }
		public ZString OrganisationName { get; private set; }
		public ZString EnterpriseCode { get; private set; }
		public ZString ServerCode { get; private set; }
		public ZDateTime PeriodStart { get; private set; }
		public ZString InvoiceCurrencyCode { get; private set; }
		public ZDecimal OdplPreDiscount { get; private set; }
		public ZDecimal OdplTotalDue { get; private set; }
		public ZDecimal StlPreDiscount { get; private set; }
		public ZDecimal StlPostDiscount { get; private set; }

		readonly StlPreviewLicenceDatabaseOwner DatabaseOwner;
		readonly StlPreviewContext PreviewContext;

		StlBill StlBillForSummary;
		OrganisationBill OrganisationBillForSummary;

		public StlPreviewLine(BusinessObjectFactory billingFactory, StlPreviewLicenceDatabaseOwner databaseOwner, ZDateTime periodStart, StlPreviewContext previewContext)
		{
			if (billingFactory == null || databaseOwner == null || periodStart.IsEmpty || previewContext == null)
			{
				return;
			}

			DatabaseOwner = databaseOwner;
			PeriodStart = periodStart;
			PreviewContext = previewContext;

			var runningContext = GetCachedRunningContext(billingFactory);

			var stlBill = GenerateStlBill(periodStart, runningContext);
			var odplBill = GenerateOdplBill(periodStart, runningContext);

			OrganisationCode = runningContext.OrganisationCode;
			OrganisationName = runningContext.OrganisationName;
			EnterpriseCode = stlBill?.EnterpriseCode ?? odplBill?.EnterpriseCode ?? "";
			ServerCode = runningContext.LicHeader.DatabaseCode;
			InvoiceCurrencyCode = stlBill?.InvoiceCurrencyCode ?? odplBill?.InvoiceCurrencyCode ?? "";

			OdplPreDiscount = odplBill?.Amount ?? 0;
			OdplTotalDue = odplBill?.TotalDue ?? 0;
			StlPreDiscount = stlBill?.InvoicePreDiscountTotal ?? 0;
			StlPostDiscount = stlBill?.InvoicePostDiscountTotal ?? 0;

			StlBillForBinding = new StlBillForBindingOnly(stlBill);

			CopyNotificationsToRowNotifications(this, stlBill);
			CopyNotificationsToRowNotifications(this, odplBill);
		}

		public StlBill GetStlBillForSummary(BusinessObjectFactory billingFactory)
		{
			return StlBillForSummary ?? (StlBillForSummary = GenerateStlBill(PeriodStart, GetCachedRunningContext(billingFactory)));
		}

		public OrganisationBill GetOrganisationBillForSummary(BusinessObjectFactory billingFactory)
		{
			return OrganisationBillForSummary ?? (OrganisationBillForSummary = GenerateOdplBill(PeriodStart, GetCachedRunningContext(billingFactory)));
		}

		StlPreviewBillRunningContext GetCachedRunningContext(BusinessObjectFactory billingFactory)
		{
			return billingFactory.GetCachedValue(ZString.Format("StlPreviewLine.StlPreviewBillRunningContext.{0}.{1}", PreviewContext.StlPreviewContextPk, DatabaseOwner.PK),
					() => new StlPreviewBillRunningContext(billingFactory, DatabaseOwner, PreviewContext));
		}

		#region Implement

		protected override ZString HumanReadableNameCore => "Preview Line";

		class StlPreviewBillRunningContext
		{
			public readonly StlPreviewContext PreviewContext;
			public readonly BusinessObjectFactory Factory;
			public readonly LicenceDatabase LicDatabase;
			public readonly LicenceHeader LicHeader;
			public readonly IEnumerable<string> OnDemandUsageCodesForPreview;
			public readonly ClientLicencePriceHeader StlPriceHeader;
			public readonly DiscountVersionSet DiscountVersions;
			public readonly ClientInvoiceDelivery Delivery;
			public readonly PriceList StlPriceList;
			public readonly EdiLicenceSetting[] EdiLicenceSettings;
			public readonly ClientLicencePriceHeader OdplPriceHeader;
			public readonly ZGuid OrganisationPk;
			public readonly ZString OrganisationCode;
			public readonly ZString OrganisationName;
			public readonly ZString CurrencyCode;

			public StlPreviewBillRunningContext(BusinessObjectFactory factory, StlPreviewLicenceDatabaseOwner databaseOwner, StlPreviewContext billPreviewContext)
			{
				Factory = factory;
				PreviewContext = billPreviewContext;
				LicDatabase = Factory.Load<LicenceDatabase>(databaseOwner.LD_PK);
				LicHeader = Factory.Load<LicenceHeader>(databaseOwner.LA_PK);

				// Apply temporary settings in the bill factory to override the existing ones
				ClientInvoiceDelivery oldDelivery = LicHeader.Company.InvoiceDeliveries.FindByServerAndSystem(LicDatabase.LD_ServerCode, BillingConstants.BillingSystem.ODM);
				OrganisationPk = oldDelivery != null && !oldDelivery.L9_OH_InvoiceTo.IsEmpty ? oldDelivery.L9_OH_InvoiceTo : LicHeader.Company.LC_OH;
				Delivery = null;
				CurrencyCode = new[] { billPreviewContext.CurrencyCode, oldDelivery?.L9_RX_NKInvoiceCurrency, (ZString)"USD" }.First(x => x.HasValue && !x.Value.IsEmpty).Value;

				foreach (LicenceHeader tmp in LicDatabase.LicHeadersForAllCompanies)
				{
					var org = tmp.Company.Header;
					foreach (ClientInvoiceDelivery deliveryToRemove in org.LicCompany.InvoiceDeliveries.ToArray())
					{
						if (deliveryToRemove.L9_IsBilled)
						{
							deliveryToRemove.Delete();
						}
					}
					ClientInvoiceDelivery delivery = org.LicCompany.InvoiceDeliveries.AddNew();
					delivery.L9_GB_InvoicingBranch = Env.CurrentBranchPK;
					delivery.L9_RX_NKInvoiceCurrency = CurrencyCode;
					delivery.L9_SystemCode = BillingConstants.BillingSystem.All;
					if (tmp.PK == LicHeader.PK)
					{
						Delivery = delivery;
					}

					if (OrganisationPk != org.PK)
					{
						delivery.L9_OH_InvoiceTo = OrganisationPk;
					}
				}

				DiscountVersions = new DiscountVersionSet(Factory);
				StlPriceHeader = Factory.Load<ClientLicencePriceHeader>(billPreviewContext.StlPriceHeaderPk);
				OdplPriceHeader = LoadOdplPriceHeaderWithExpectedCurrencyCode(billPreviewContext.OdplPriceHeaderPk);

				if (StlPriceHeader != null)
				{
					var stlPriceListSet = PriceListSet.NewWithRates(Factory, new[] { StlPriceHeader }, DiscountVersions);
					StlPriceList = stlPriceListSet.GetPriceList(billPreviewContext.StlPriceHeaderPk.ToGuid());
					OnDemandUsageCodesForPreview = StlPriceList.GetOnDemandUsageCodesForPreview();
					EdiLicenceSettings = BuildDiscountSettings(StlPriceList, LicHeader);
				}

				var orgHeader = Factory.Load<EDIOrgHeader>(OrganisationPk);
				orgHeader.LicCompany.SelfBilling.BillingDiscounts.DeleteAll();
				OrganisationCode = orgHeader.OH_Code;
				OrganisationName = orgHeader.OH_FullName;
			}

			EdiLicenceSetting[] BuildDiscountSettings(PriceList stlPriceList, LicenceHeader licHeader)
			{
				var settings = new List<EdiLicenceSetting>();
				foreach (var d in PreviewContext.StlDiscounts)
				{
					var headerDiscount = stlPriceList.Discounts.Discounts.FirstOrDefault(x => x.PHD_Name == d.Name);
					if (headerDiscount.PHD_IsDefaultEnabled != d.IsActive
						|| (d.IsActive && headerDiscount.PHD_Percent != d.Percent))
					{
						var setting = Factory.New<DiscountLicenceSetting>();
						setting.LS9_IsActive = d.IsActive;
						setting.LS9_LD = licHeader.LA_LD;
						setting.LS9_Name = d.Name;
						setting.LS9_Percent = d.Percent;
						setting.LS9_ValidFrom = new ZDateTime(2010, 1, 1);
						settings.Add(setting);
					}
				}

				return settings.ToArray();
			}

			ClientLicencePriceHeader LoadOdplPriceHeaderWithExpectedCurrencyCode(ZGuid odplPriceHeaderPk)
			{
				ClientLicencePriceHeader result = null;

				if (!odplPriceHeaderPk.IsEmpty)
				{
					result = Factory.Load<ClientLicencePriceHeader>(odplPriceHeaderPk);

					if (result != null && result.L6_RX_NKCurrency != CurrencyCode)
					{
						result = LicenceCompany.GetStandardPriceHeaders(Factory)?.FirstOrDefault(x =>
								x.L6_SystemCode == BillingConstants.BillingSystem.ODM &&
								x.L6_RN_NKCountry.IsEmpty &&
								x.L6_PricelistVersion == result.L6_PricelistVersion &&
								x.L6_RX_NKCurrency == CurrencyCode);
					}
				}

				return result;
			}
		}

		static StlBill GenerateStlBill(ZDateTime periodStart, StlPreviewBillRunningContext billContext)
		{
			if (billContext.PreviewContext.StlPriceHeaderPk.IsEmpty)
			{
				return null;
			}

			var factory = billContext.Factory;
			var org = billContext.LicHeader.Company.Header;
			var context = new BillingRunContext(factory, billContext.PreviewContext.GenerateDate, periodStart.AddMonths(1).AddDays(-1), org.PK);
			context.IncludeOdpl = true;
			context.IncludeStl = false;
			context.IsBackPost = true;
			Guid databasePk = billContext.LicHeader.LA_LD.ToGuid();
			Guid[] databasePks = new[] { databasePk };

			var feeQuery = FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false);
			var feeList = factory.Load<ClientLicenceFee>(feeQuery)
				.Where(x => x.L8_LD.IsEmpty || x.L8_LD == billContext.LicHeader.LA_LD)
				.ToArray();
			StlFees stlFees = null;
			if (feeList.Length > 0)
			{
				stlFees = new StlFees(context, feeList);
				if (!stlFees.Any())
				{
					stlFees = null;
				}
			}

			var chargeableUsages = DatabaseUsageSet.GetDatabaseUsages(factory, periodStart, databasePk, billContext.OnDemandUsageCodesForPreview).ToList();

			var mainDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			mainDatabasePkMap.Add(databasePk, billContext.LicHeader.Database);
			var usageDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			usageDatabasePkMap.Add(databasePk, billContext.LicHeader.Database);
			var borderWiseUsageSet = new BorderWise.BorderWiseUsageSetProvider().Create(context, mainDatabasePkMap);
			borderWiseUsageSet.AppendTo(usageDatabasePkMap, mainDatabasePkMap, chargeableUsages);

			var services = PremiumServiceBillingSystem.GetDue(factory, periodStart, databasePks, null);
			var clientCompanies = factory.Load<ClientCompany>(new ZQuery(ClientCompanySchema.LCC_LD, billContext.LicHeader.LA_LD));
			var clientCompanyPkMap = clientCompanies.ToDictionary(x => x.PK.ToGuid());
			var usageSet = new UsageSet(chargeableUsages, services, null, periodStart, usageDatabasePkMap, clientCompanyPkMap);
			var usages = usageSet.Usages.ToArray();

			var usageOwnerDelivery = new UsageOwnerDelivery(new UsageOwner(billContext.LicHeader), billContext.Delivery, billContext.LicHeader.Company);
			foreach (var usage in usages)
			{
				usage.OwnerDelivery = usageOwnerDelivery;
			}

			var hubPriceListSet = new DatabasePriceListSet(context, new[] { usageOwnerDelivery }, BillingConstants.PriceHeaderType.EHub, billContext.DiscountVersions);
			PriceList hubPriceList = hubPriceListSet.GetPriceListByDatabasePk(billContext.LicHeader.LA_LD.ToGuid());
			var additionalPriceLists = DatabaseUsageSet.BuildGlobalPriceListDictionary(context, billContext.DiscountVersions);
			additionalPriceLists[BillingConstants.PriceHeaderType.EHub] = hubPriceList;
			PriceList borderwisePriceList = null;
			if (borderWiseUsageSet.HasUsage)
			{
				borderwisePriceList = DatabaseUsageSet.BuildGlobalPriceList(BillingConstants.PriceHeaderType.BorderWise, context, billContext.DiscountVersions, isMultiCurrencies: true, includeCargoWiseOneDiscounts: true);
				additionalPriceLists[BillingConstants.PriceHeaderType.BorderWise] = borderwisePriceList;
			}

			var dbUsage = new DatabaseUsage(usageOwnerDelivery, usages, billContext.CurrencyCode, billContext.StlPriceList, billContext.EdiLicenceSettings, periodStart, true, additionalPriceLists, clientCompanies, billContext.PreviewContext.PriceHeaderLink);
			var monthlyUsageList = StlBilling.MatchUsageToPrice(factory, periodStart, dbUsage);
			foreach (var monthlyUsage in monthlyUsageList)
			{
				monthlyUsage.HasPrepaid = true;
			}

			var bill = new StlBill(factory,
				factory.Load<GlbBranch>(Env.CurrentBranchPK),
				org,
				billContext.CurrencyCode,
				periodStart,
				context.DateForExchangeRate);
			bill.AddMonthlyUsages(monthlyUsageList);
			bill.HasPrepaid = true;

			if (stlFees != null)
			{
				bill.AddFees(stlFees.FeeUsages);
				monthlyUsageList.FirstOrDefault()?.AddFeeUsages(stlFees.FeeUsages);
			}

			var billAsArray = new[] { bill };

			StlBilling.CalculateDiscounts(context, monthlyUsageList);
			StlBilling.CalculateCurrencyAndSurchargeAmounts(context, billAsArray);
			return bill;
		}

		static OrganisationBill GenerateOdplBill(ZDateTime periodStart, StlPreviewBillRunningContext billContext)
		{
			if (billContext.OdplPriceHeader == null)
			{
				return null;
			}

			var context = new BillingRunContext(billContext.Factory, billContext.PreviewContext.GenerateDate, periodStart.AddMonths(1).AddDays(-1), billContext.OrganisationPk, billContext.PreviewContext.EnterpriseCode);
			context.IsBackPost = true;
			context.IncludeStl = false;
			context.IncludeOdpl = true;
			context.SetPreviewOnly(billContext.LicHeader, billContext.OdplPriceHeader);

			var billingSystems = new BillingSystemList().ToArray();
			var systemBills = MonthlyUsageBilling.LoadSystemBills(null, billingSystems, context);
			OrganisationBill[] bills = new MonthlyUsageBilling(billContext.Factory).CreateOrganisationBills(systemBills, null, billingSystems, context);
			return bills.Length > 0 ? bills[0] : null;
		}

		static void CopyNotificationsToRowNotifications(NonPersistentBusinessObject target, BusinessObject source)
		{
			if (target != null && source != null)
			{
				var notifications = source.Notifications.ToArray();

				foreach (var error in notifications.Where(x => x.Type == NotificationType.Error))
				{
					target.AddRowError(error.Message);
				}

				foreach (var warning in notifications.Where(x => x.Type == NotificationType.Warning))
				{
					target.AddRowWarning(warning.Message);
				}
			}
		}

		#endregion
	}

	public class StlPreviewLineCollection : NonPersistentBusinessObjectCollection<StlPreviewLine>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlPreviewLine(null, null, ZDateTime.Empty, null);
		}
	}
}
