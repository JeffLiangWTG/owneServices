using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class TranslationDataSource : IContentTranslationDataSource
	{
		public IEnumerable<IContentTranslationDataItem> GetTranslatableResources()
		{
			var items = new List<IContentTranslationDataItem>();

			var supportedLanguages = EDIDataRegistry.Instance.BillingTranslationExportSupportedLanguages.Value;

			if (supportedLanguages == null || !supportedLanguages.Any())
			{
				return items;
			}

			//registry items
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.StlDiscountTypes));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.StlMonthlyUsageInvoiceDescription));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.MonthlyUsageInvoiceComment));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment));
			items.Add(TranslationDataItem.New(EDIDataRegistry.Instance.PrepaymentInvoiceComment));

			//orgs
			var phlQuery = new ZDBOnlySubQuery(typeof(EdiPriceHeaderLink), EdiPriceHeaderLinkSchema.PHL_LD);
			phlQuery.AddToFilter(EdiPriceHeaderLinkSchema.PHL_ValidFrom, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			var validToQuery = new ZQuery(EdiPriceHeaderLinkSchema.PHL_ValidTo, null);
			validToQuery.AddToFilter(JoinCondition.Or, EdiPriceHeaderLinkSchema.PHL_ValidTo, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			phlQuery.AddToFilter(validToQuery);

			var ldQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_LE);
			ldQuery.AddSubQuery(phlQuery, JoinCondition.And);

			var leQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);
			leQuery.AddSubQuery(ldQuery, JoinCondition.And);

			var lcQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			lcQuery.AddSubQuery(LicenceCompanySchema.LC_LE, leQuery, JoinCondition.And);

			var orgQuery = new ZDBOnlyQuery(typeof(EDIOrgHeader));
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Language, supportedLanguages);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			orgQuery.AddSubQuery(lcQuery, JoinCondition.And);

			var priceHeaderSet = new HashSet<ZGuid>();

			var reader = new FilteredBusinessObjectReader<EDIOrgHeader>(orgQuery, new BusinessObjectFactory());
			foreach (EDIOrgHeader org in reader)
			{
				//org comment / fees
				var item = new TranslationDataItem(FormattableString.Invariant($"Org. - {org.OH_Code}"));
				item.AddSafe(org.LicCompany?.SelfBilling?.L4_InvoiceCommentInfo);
				item.AddRangeSafe(org.LicCompany?.Fees.Select(x => x.L8_DescriptionInfo));
				items.Add(item);

				//price items
				var priceHeaders = org.LicEnterprise?.Databases.OfType<LicenceDatabase>()
					.Select(x => x.PriceHeaderLinkForDate(ZDateTime.Now)?.PriceHeader)
					.Where(x => x != null).Distinct() ?? Enumerable.Empty<ClientLicencePriceHeader>();

				foreach (var priceHeader in priceHeaders)
				{
					if (!priceHeaderSet.Contains(priceHeader.PK))
					{
						priceHeaderSet.Add(priceHeader.PK);
						items.Add(TranslationDataItem.New(priceHeader));
					}
				}
			}

			return items.Where(x => x.Resources.Any()).ToArray();
		}
	}

	public class TranslationDataItem : IContentTranslationDataItem
	{
		public TranslationDataItem(string id)
		{
			Id = id;
			Resources = new Dictionary<string, string>();
		}

		public string Id { get; }
		public IDictionary<string, string> Resources { get; }

		void AddSafe(string key, string value)
		{
			if (Resources.ContainsKey(key))
			{
				return;
			}

			Resources.Add(key, value);
		}

		public void AddSafe(ZPropertyInfo info)
		{
			if (info == null)
			{
				return;
			}

			if (info.Value is ZString)
			{
				var value = (ZString)info.Value;
				if (value.IsEmpty)
				{
					return;
				}

				string key = info.CustomizableDataResourceStrings.Source.GetKey(info.BizObj, value);
				AddSafe(key, value);
			}

			if (info.Value is ResourceString)
			{
				var resString = (ResourceString)info.Value;
				if (string.IsNullOrEmpty(resString.EnglishText))
				{
					return;
				}

				AddSafe(resString.ResourceKey, resString.EnglishText);
			}
		}

		public void AddRangeSafe(IEnumerable<ZPropertyInfo> infos)
		{
			foreach (var info in infos ?? Enumerable.Empty<ZPropertyInfo>())
			{
				AddSafe(info);
			}
		}

		public static TranslationDataItem New(MultilingualStringRegistryItem registryItem)
		{
			var item = new TranslationDataItem(FormattableString.Invariant($"Registry - {registryItem.Caption}"));

			if (registryItem.Value is ResourceString)
			{
				var res = (ResourceString)registryItem.Value;
				item.AddSafe(res.ResourceKey, res.EnglishText);
			}

			return item;
		}

		public static TranslationDataItem New(CodeDescriptionBoolRegistryItem registryItem)
		{
			var item = new TranslationDataItem(FormattableString.Invariant($"Registry - {registryItem.Caption}"));
			item.AddRangeSafe(registryItem.Value.OfType<CodeDescriptionBool>().Select(x => x.DescriptionInfo));
			return item;
		}

		public static TranslationDataItem New(ClientLicencePriceHeader priceHeader)
		{
			var item = new TranslationDataItem(FormattableString.Invariant($"Price Header - {priceHeader.L6_PricelistVersion}"));

			item.AddRangeSafe(priceHeader.Items.Select(x => x.L7_DescriptionInfo));
			item.AddRangeSafe(priceHeader.Items.Select(x => x.L7_ChargeBasisInfo));

			var chargeCodes = priceHeader.Items.SelectMany(x => new[] { x.L7_ChargeCode, x.L7_DepositChargeCode, x.L7_DiscountChargeCode })
			.Where(x => !x.IsEmpty).Distinct().ToArray();

			var genericCharges = priceHeader.Factory.Load<GenericCharge>(new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodes));
			item.AddRangeSafe(genericCharges.Select(x => x.VC_DescriptionInfo));

			return item;
		}
	}
}


