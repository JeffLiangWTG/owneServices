using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class LicenceUsageFilter : ModuleDateFilter
	{
		#region Properties

		readonly BusinessObjectFactory factory;
		public readonly bool IsBilledFilter;

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string PriceHeaderCode = "PriceHeaderCode";
			public const string UsageCountComparisonOperator = "UsageCountComparisonOperator";
			public const string UsageCount = "UsageCount";
			public const string PriceItemCode = "PriceItemCode";
			public const string CountryCode = "CountryCode";
		}

		#endregion

		public LicenceUsageFilter(ZString description, string parentSchemaColumn, bool isBilledFilter)
			: base(description, false, false)
		{
			this.ParentSchemaColumn = parentSchemaColumn;
			SetDefaultFilterValues();
			factory = new BusinessObjectFactory();
			this.IsBilledFilter = isBilledFilter;
		}

		readonly string ParentSchemaColumn;

		void SetDefaultFilterValues()
		{
			PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			HideFutureDates = true;
			UsageCount = 1;
			UsageCountComparisonOperator = CountComparisonConstants.GreaterThanOrEqualTo;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			SetDefaultFilterValues();
		}

		protected override bool IsEmptyCore => base.IsEmptyCore || PriceHeaderCodeInfo.HasErrors() || UsageCountInfo.HasErrors() || UsageCountComparisonOperatorInfo.HasErrors();

		public bool IsDateEmpty
		{
			get { return base.IsEmpty; }
		}

		#region Price List

		[List("ClientLicencePriceHeaderList")]
		public ZString PriceHeaderCode
		{
			get { return priceHeaderCode; }
			set
			{
				if (priceHeaderCode != value)
				{
					priceHeaderCode = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidatePriceHeaderCode();
					}
					PriceHeaderCodeInfo.RefreshBinding();
					PriceItemCode = "";
					InvalidateCachedQuery();
				}
			}
		}
		ZString priceHeaderCode;

		[List("ClientLicencePriceItemList")]
		public ZString PriceItemCode
		{
			get { return priceItemCode; }
			set
			{
				if (priceItemCode != value)
				{
					priceItemCode = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidatePriceItemCode();
					}
					PriceItemCodeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString priceItemCode;

		[List("CountryCodeList")]
		public ZString CountryCode
		{
			get { return countryCode; }
			set
			{
				if (countryCode != value)
				{
					countryCode = value;
					CountryCodeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString countryCode;

		public ZPropertyInfo PriceHeaderCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PriceHeaderCode); }
		}

		public ZPropertyInfo PriceItemCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PriceItemCode); }
		}
		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}
		public CodeDescriptionPairList ClientLicencePriceHeaderList
		{
			get
			{
				return BillingConstants.PriceHeaderType.GetPriceHeaderTypeList();
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public CodeDescriptionPairList ClientLicencePriceItemList
		{
			get
			{
				if (IsBilledFilter)
				{
					return factory.GetCachedValue("LicenceUsageFilter.ClientLicencePriceItemList." + PriceHeaderCode,
				() =>
				{
					CodeDescriptionPairList result;

					stlPriceHeaderPK = Guid.Empty;
					ZGuid standardPricesCompanyPK = ZGuid.Empty;
					if (LicenceCompany.StandardPricesCompany != null)
					{
						standardPricesCompanyPK = LicenceCompany.StandardPricesCompany.PK;
					}

					var priceList = new DynamicBusinessObjectCollection(factory);
					string sqlText = $@"SELECT DISTINCT L7_Code, L7_Description, L7_Order, a.L6_SystemCode AS L6_SystemCode, a.L6_PK AS L6_PK FROM dbo.ClientLicencePriceItem, ClientLicencePriceHeader a
 INNER JOIN(
	 SELECT L6_SystemCode, MAX(L6_SystemCreateTimeUtc) AS Most_Recent
	 FROM  dbo.ClientLicencePriceHeader WHERE L6_LC = '{standardPricesCompanyPK}'
	 GROUP BY L6_SystemCode) GroupedHeader
 ON a.L6_SystemCode = GroupedHeader.L6_SystemCode
 WHERE  a.L6_SystemCreateTimeUtc = Most_Recent AND a.L6_SystemCode = '{PriceHeaderCode}' AND L7_L6 = a.L6_PK
 AND a.L6_LC = '{standardPricesCompanyPK}' AND L7_Code != ''
 ORDER BY L7_Order, L7_Code";

					priceList.Load(sqlText);
					result = new CodeDescriptionPairList();
					foreach (DynamicBusinessObject priceItem in priceList)
					{
						result.AddPairIfNotExist(priceItem[ClientLicencePriceItemSchema.Constants.L7_Code].ToString(), priceItem[ClientLicencePriceItemSchema.Constants.L7_Description].ToString());
						if (priceItem[ClientLicencePriceHeaderSchema.Constants.L6_SystemCode].ToString().Equals("STL"))
						{
							stlPriceHeaderPK = Guid.Parse(priceItem[ClientLicencePriceHeaderSchema.Constants.PK].ToString());
						}
					}
					return result;
				});
				}
				else
				{
					var result = LicenceModuleList.Instance.Names;
					result.SortByDescription();
					return result;
				}
			}
		}

		#endregion

		Guid stlPriceHeaderPK;

		public RefCountryCollection CountryCodeList
		{
			get
			{
				return factory.GetCachedValue("LicenceUsageFilter.CountryCodeList",
				() =>
				{
					return new RefCountryCollection(factory);
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		#region Usage Count

		public ZInt UsageCount
		{
			get { return usageCount; }
			set
			{
				if (UsageCount != value)
				{
					InvalidateCachedQuery();
				}

				usageCount = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateUsageCount();
				}
				UsageCountInfo.RefreshBinding();
			}
		}
		ZInt usageCount;

		public ZPropertyInfo UsageCountInfo
		{
			get { return GetZPropertyInfo(Schema.UsageCount); }
		}

		#endregion

		#region Usage Count Comparison Operator

		[List("UsageCountComparisonOperatorList")]
		public ZString UsageCountComparisonOperator
		{
			get { return usageCountComparisonOperator; }
			set
			{
				if (usageCountComparisonOperator != value)
				{
					usageCountComparisonOperator = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateUsageCountComparisonOperator();
					}
					UsageCountComparisonOperatorInfo.RefreshBinding();

					if ((usageCountComparisonOperator == CountComparisonConstants.LessThanOrEqualTo || usageCountComparisonOperator == CountComparisonConstants.GreaterThanOrEqualTo)
						&& UsageCount == 0)
					{
						UsageCount = 1;
					}
					else if (usageCountComparisonOperator == CountComparisonConstants.IsBlank)
					{
						UsageCount = 0;
					}
					else if (usageCountComparisonOperator == CountComparisonConstants.IsNotBlank)
					{
						UsageCount = 1;
					}

					InvalidateCachedQuery();
				}
			}
		}
		ZString usageCountComparisonOperator;

		public ZPropertyInfo UsageCountComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(Schema.UsageCountComparisonOperator); }
		}

		public CodeDescriptionPairList UsageCountComparisonOperatorList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				result.AddPair(CountComparisonConstants.Exact, "Search for an Exact match");
				result.AddPair(CountComparisonConstants.LessThanOrEqualTo, "Search for fields that are Less Than or Equal to the supplied value");
				result.AddPair(CountComparisonConstants.GreaterThanOrEqualTo, "Search for fields that are Greater Than or Equal to the supplied value");

				return result;
			}
		}

		public SQLComparisonOperator UsageCountSqlComparisonOperator
		{
			get
			{
				switch (UsageCountComparisonOperator)
				{
					case CountComparisonConstants.Exact:
					case CountComparisonConstants.IsBlank:
						return SQLComparisonOperator.Equal;
					case CountComparisonConstants.GreaterThanOrEqualTo:
					case CountComparisonConstants.IsNotBlank:
						return SQLComparisonOperator.GreaterThanOrEqualTo;
					case CountComparisonConstants.LessThanOrEqualTo:
						return SQLComparisonOperator.LessThanOrEqualTo;
					default:
						return SQLComparisonOperator.Equal;
				}
			}
		}

		public static class CountComparisonConstants
		{
			public const string Exact = "Equals";
			public const string LessThanOrEqualTo = "Less than or equal to";
			public const string GreaterThanOrEqualTo = "Greater than or equal to";
			public const string IsBlank = "Has no usage";
			public const string IsNotBlank = "Has usage";
		}

		#endregion

		#region Validation

		public new LicenceUsageFilterValidation Validation
		{
			get { return (LicenceUsageFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new LicenceUsageFilterValidation(this);
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddFilterAndZSQLParameterCollection(GetUsageSQL(), new ZSqlParameterCollection());
				return query;
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string GetUsageSQL()
		{
			var fromDate = FromDate.IsValid ? new ZDateTime(FromDate.Year, FromDate.Month, 1) : new ZDateTime(1900, 1, 1);
			var toDate = ToDate.IsValid ? new ZDateTime(ToDate.Year, ToDate.Month, 1) : ZDateTime.UtcNow;
			string usageCodeColumnName1 = IsBilledFilter ? EdiBilledUsageSchema.Constants.BU9_UsageCode : ClientChargeableUsageSchema.Constants.U1_Code;
			string usageCodeColumnName2 = IsBilledFilter ? EdiBilledUsageSchema.Constants.BU9_UsageSubCode : ClientChargeableUsageSchema.Constants.U1_SubCode;
			var usageCountType = IsBilledFilter ? EdiBilledUsageSchema.Constants.BU9_UnitCount : ClientChargeableUsageSchema.Constants.U1_UnitCount;

			bool isZeroUsageCountQuery = UsageCountSqlComparisonOperator == SQLComparisonOperator.Equal && UsageCount == 0;
			string usageCountClause = !isZeroUsageCountQuery
										? $"HAVING SUM({usageCountType}) " + UsageCountSqlComparisonOperator.ComparisonText(UsageCount) + " " + UsageCount
										: $"HAVING SUM({usageCountType}) > 0";

			var whereClause1 = new StringBuilder();
			var whereClause2 = new StringBuilder();

			if (!(countryCode.IsEmpty || countryCode.Equals("")))
			{
				whereClause1.Append($"AND (ClientCompany.LCC_RN_NKCountryCode = '{countryCode}' OR LicenceCompany.LC_CompanyCountry = '{countryCode}' )");
			}

			if (!(priceHeaderCode.IsEmpty || priceHeaderCode.Equals("")) && IsBilledFilter)
			{
				if (priceHeaderCode.Equals(BillingConstants.PriceHeaderType.Other))
				{
					whereClause2.Append($"{usageCodeColumnName1} = '{BillingConstants.BillingSystem.AirlineMessaging}'");
				}
				else if (priceHeaderCode.Equals(BillingConstants.PriceHeaderType.LDaaS))
				{
					whereClause2.Append($"{usageCodeColumnName1} = '{BillingConstants.BillingSystem.Service}'");
				}
				else
				{
					whereClause2.Append($"{usageCodeColumnName1} = '{priceHeaderCode}'");
				}

				if (!(priceItemCode.IsEmpty || priceItemCode.Equals("")))
				{
					whereClause2.Append($" AND {usageCodeColumnName2} = '{priceItemCode}'");
				}
			}
			else if (!(priceItemCode.IsEmpty || priceItemCode.Equals("")))
			{
				whereClause2.Append($" AND {usageCodeColumnName2} = '{priceItemCode}'");
			}

			string usageSQL = IsBilledFilter ? GetBilledUsageSql(fromDate, toDate, whereClause1, whereClause2, usageCountClause) : GetModuleUsageSql(fromDate, toDate, whereClause1, whereClause2, usageCountClause);

			return ParentSchemaColumn + (isZeroUsageCountQuery ? " NOT IN" : " IN") + usageSQL;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "SQL string")]
		string GetBilledUsageSql(ZDateTime fromDate, ZDateTime toDate, StringBuilder whereClause1, StringBuilder whereClause2, string usageCountClause)
		{
			string usageSQL = $@"(
	SELECT
		ISNULL(LCC_OH, LC_OH)
	FROM
		dbo.ediBilledUsage
		LEFT JOIN dbo.ClientCompany ON BU9_LCC = LCC_PK
		LEFT JOIN dbo.LicenceCompany ON BU9_LC = LC_PK
	WHERE
		1 = 1
		{whereClause1}
		AND (
			({whereClause2})
			OR
			(BU9_UsageCode = 'STL' AND EXISTS (SELECT 1 FROM dbo.EdiPriceUsageMapping WHERE PUM_L6 = '{stlPriceHeaderPK.ToString()}'
				AND PUM_UsageCategory = 'STL'
				AND PUM_UsageCode = BU9_UsageSubCode
				AND PUM_PriceCode = '{priceItemCode}'))
			OR
			(BU9_UsageCode = '{priceItemCode}' AND BU9_PriceCode = '{priceItemCode}')
			OR
			(BU9_UsageCode = '{BillingConstants.BillingSystem.Service}' AND BU9_PriceCode = '{priceItemCode}')
			OR
			(BU9_UsageCode = '' AND BU9_PriceCode = '{priceItemCode}' AND BU9_PriceCode LIKE 'G%')
		)
		AND BU9_PeriodStart >= '{fromDate.SqlFormat}'
		AND BU9_PeriodStart <= '{toDate.SqlFormat}'
	GROUP BY
		ISNULL(LCC_OH, LC_OH)
	{usageCountClause}
)";

			return usageSQL;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "SQL string")]
		string GetModuleUsageSql(ZDateTime fromDate, ZDateTime toDate, StringBuilder whereClause1, StringBuilder whereClause2, string usageCountClause)
		{
			string usageSQL = $@"(
	SELECT
		ISNULL(LCC_OH, LC_OH)
	FROM
		dbo.ClientChargeableUsage
		LEFT JOIN dbo.ClientCompany ON U1_LCC = LCC_PK
		LEFT JOIN dbo.LicenceCompany ON U1_LC = LC_PK
	WHERE
		1 = 1
		{whereClause1}
		AND U1_Code = 'ODM'
		{whereClause2}
		AND U1_PeriodStart >= '{fromDate.SqlFormat}'
		AND U1_PeriodStart <= '{toDate.SqlFormat}'
	GROUP BY
		ISNULL(LCC_OH, LC_OH)
	{usageCountClause}
)";

			return usageSQL;
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.PriceHeaderCode, PriceHeaderCode);
			writer.WriteElementString(Schema.PriceItemCode, PriceItemCode);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			writer.WriteElementString(Schema.UsageCountComparisonOperator, UsageCountComparisonOperator);
			writer.WriteElementString(Schema.UsageCount, UsageCount.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			PriceHeaderCode = reader.ReadElementString(Schema.PriceHeaderCode);
			PriceItemCode = reader.ReadElementString(Schema.PriceItemCode);
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			UsageCountComparisonOperator = reader.ReadElementString(Schema.UsageCountComparisonOperator);
			UsageCount = ZInt.Parse(reader.ReadElementString(Schema.UsageCount));
		}

		#endregion

		#region static create control function
		static public void AddLicenceUsageFilters(ModuleFilterCollection filters, string parentSchemaColumn, bool isBilledFilter)
		{
			var licenceUsageCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("e4762569-34eb-4329-93bf-f520eea757ba", "License Usage"));

			var hasModuleLicenceUsagefilter = isBilledFilter ? new LicenceUsageFilter("Billed Usage Analysis (UTC)", parentSchemaColumn, isBilledFilter) : new LicenceUsageFilter("Module Usage Analysis (UTC)", parentSchemaColumn, isBilledFilter);
			hasModuleLicenceUsagefilter.Category = licenceUsageCategory;
			filters.AddFilter(hasModuleLicenceUsagefilter);
		}
		#endregion
	}
}
