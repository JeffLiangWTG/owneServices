using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class MatchEPaymentRecipientsFilterBusinessObject : FilterStripBusinessObject
	{
		public MatchEPaymentRecipientsFilterBusinessObject()
		{
		}

		public MatchEPaymentRecipientsFilterBusinessObject(MatchEPaymentRecipients parent)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "MatchEPaymentRecipientsForm";
			Parent = parent;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new MatchEPaymentRecipientsFilterBusinessObject(Parent);

		readonly MatchEPaymentRecipients Parent;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var recipientNameFilter = filters.AddTextFilter("Recipient Name", AccEPaymentBeneficiarySchema.ABF_BeneficiaryFullName);
			recipientNameFilter.MultilingualDescription = ResString.GetMultilingualString("6DA1DD4B-E05B-49E9-B913-B8E92083D5B1", "Recipient Name");
			recipientNameFilter.MaxLength = AccEPaymentBeneficiarySchema.ABF_BeneficiaryFullName.MaxLength;
			recipientNameFilter.Visibility = FilterVisibility.AlwaysVisible;

			var nickNameFilter = filters.AddTextFilter("Nickname", AccEPaymentBeneficiarySchema.ABF_BeneficiaryNickName);
			nickNameFilter.MultilingualDescription = ResString.GetMultilingualString("8F2F6254-F679-45BA-BA10-6B0CD027781A", "Nickname");
			nickNameFilter.MaxLength = AccEPaymentBeneficiarySchema.ABF_BeneficiaryNickName.MaxLength;
			nickNameFilter.Visibility = FilterVisibility.Visible;

			var countryFilter = filters.AddNkFilter("Country", AccEPaymentBeneficiarySchema.ABF_RN_NKCountryCode, ModuleIDs.RefCountry, CountryList);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("9E18F438-F96D-4FB4-B48E-98ED72100A63", "Country/Region");
			countryFilter.Visibility = FilterVisibility.Visible;

			var currencyFilter = filters.AddNkFilter("Currency", AccEPaymentBeneficiarySchema.ABF_RX_NKAccountCurrency, ModuleIDs.RefCurrency, CurrencyList);
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("869FDFFC-D027-470D-9425-09D8C8742868", "Currency");
			currencyFilter.Visibility = FilterVisibility.Visible;

			var lastUpdatedFilter = filters.AddDateFilter("Last Updated", AccEPaymentBeneficiarySchema.ABF_SystemLastEditTimeUtc);
			lastUpdatedFilter.MultilingualDescription = ResString.GetMultilingualString("448D49E2-7346-4829-BCFC-AAE2B68A5A16", "Last Updated");
			lastUpdatedFilter.Visibility = FilterVisibility.Visible;

			var matchedFilter = filters.AddTextFilter("Matched/Unmatched Recipients", MatchReceipientsStatusQuery, MatchRecipientsStatusList);
			matchedFilter.Category = FilterCategories.StatusAndFlags;
			matchedFilter.DefaultProperty = MatchRecipientsStatusTypes.All;
			matchedFilter.MultilingualDescription = ResString.GetMultilingualString("ED8442B3-4016-4012-B808-AA14E912E5AD", "Matched/Unmatched Recipients");
			matchedFilter.Visibility = FilterVisibility.AlwaysVisible;

			return filters;
		}

		#region Match Receipients Status Query

		ZQuery MatchReceipientsStatusQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccEPaymentBeneficiary));
			ZDBOnlySubQuery subQuery = null;
			if (value == MatchRecipientsStatusTypes.UnmatchedOnly)
			{
				subQuery = new ZDBOnlySubQuery(typeof(AccAPAccountDetails), AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, true);
			}
			else if (value == MatchRecipientsStatusTypes.MatchedOnly)
			{
				subQuery = new ZDBOnlySubQuery(typeof(AccAPAccountDetails), AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, false);
			}
			if (subQuery != null)
			{
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			return query;
		}

		CodeDescriptionPairList MatchRecipientsStatusList
		{
			get
			{
				if (fMatchRecipientsStatusList == null)
				{
					fMatchRecipientsStatusList = new CodeDescriptionPairList();

					fMatchRecipientsStatusList.AddPair(MatchRecipientsStatusTypes.All, Res.GetString("E415105D-EC34-40C5-88B4-FFA874C81D47", "Show all recipients"));
					fMatchRecipientsStatusList.AddPair(MatchRecipientsStatusTypes.MatchedOnly, Res.GetString("13B4A166-D188-4AB2-B3FF-666BE1E2C05F", "Show matched recipients only"));
					fMatchRecipientsStatusList.AddPair(MatchRecipientsStatusTypes.UnmatchedOnly, Res.GetString("D33BD0D8-E592-45BB-B394-C074518415DF", "Show unmatched recipients only"));
				}

				return fMatchRecipientsStatusList;
			}
		}
		CodeDescriptionPairList fMatchRecipientsStatusList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a status type only")]
		public static class MatchRecipientsStatusTypes
		{
			public const string All = "ALL";
			public const string MatchedOnly = "Matched Only";
			public const string UnmatchedOnly = "Unmatched Only";
		}

		#endregion

		RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(Factory);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;

		RefCurrencyCollection CurrencyList
		{
			get
			{
				if (currencyList == null)
				{
					currencyList = new RefCurrencyCollection(Factory);
				}
				return currencyList;
			}
		}
		RefCurrencyCollection currencyList;
	}
}
