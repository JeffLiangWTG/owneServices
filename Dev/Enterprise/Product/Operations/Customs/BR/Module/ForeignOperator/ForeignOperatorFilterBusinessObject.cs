using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using FilterConstants = Enterprise.Customs.BR.Business.Constants.FilterConstants;

namespace Enterprise.Customs.BR.Module
{
	public class ForeignOperatorFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Owner = "Owner";
			public const string Status = "Status";
			public const string MessageStatus = "Message Status";
			public const string AuthorityIdentifier = "Authority Identifier";
			public const string ForeignOperator = "Foreign Operator";
			public const string Name = "Name";
			public const string Country = "Country";
			public const string Tin = "TIN";
		}

		public ForeignOperatorLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new ForeignOperatorLookups(this);
				}
				return lookups;
			}
		}
		ForeignOperatorLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var messageStatus = result.AddTextFilter(FilterConstants.ForeignOperator.MessageStatus, GetMessageStatusQuery, Lookups.MessageStatusList);
			messageStatus.Category = FilterCategories.StatusAndFlags;
			messageStatus.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_MessageStatus", FilterConstants.ForeignOperator.MessageStatus);

			var status = result.AddTextFilter(FilterConstants.ForeignOperator.Status, CusBRForeignOperatorSchema.BFR_CustomsStatus, Lookups.CustomsStatusTypeList);
			status.Category = FilterCategories.StatusAndFlags;
			status.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_CustomsStatus", FilterConstants.ForeignOperator.Status);

			var authorityIdentifier = result.AddTextFilter(FilterConstants.ForeignOperator.AuthorityIdentifier, CusBRForeignOperatorSchema.BFR_AuthorityIdentifier);
			authorityIdentifier.Category = FilterCategories.NumbersAndReferences;
			authorityIdentifier.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_AuthorityIdentifier", FilterConstants.ForeignOperator.AuthorityIdentifier);

			var owner = result.AddGuidFilter(FilterConstants.ForeignOperator.Owner, ModuleIDs.Organisation, CusBRForeignOperatorSchema.BFR_OH_Owner, new OrgHeaderCollection(Factory));
			owner.Category = FilterCategories.Organisations;
			owner.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_OH_Owner", FilterConstants.ForeignOperator.Owner);

			var foreignOperator = result.AddGuidFilter(Schema.ForeignOperator, ModuleIDs.Organisation, CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, new OrgHeaderCollection(Factory));
			foreignOperator.Category = FilterCategories.Organisations;
			foreignOperator.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_OH_ForeignOperator", Schema.ForeignOperator);

			var name = result.AddTextFilter(Schema.Name, GetNameQuery);
			name.Category = FilterCategories.TextSearch;
			name.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|BFR_Name", Schema.Name);

			var country = result.AddNkFilter(Schema.Country, GetCountryQuery, ModuleIDs.RefCountry, Lookups.Countries).WithMaxLengthOf<ModuleNkFilter>(OrgAddressSchema.OA_RN_NKCountryCode);
			country.Category = FilterCategories.Locations;
			country.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|ForeignOperatorCountry", Schema.Country);

			var tin = result.AddTextFilter(Schema.Tin, GetTinQuery);
			tin.Category = FilterCategories.TextSearch;
			tin.MultilingualDescription = ResString.GetMultilingualString("ForeignOperatorFilterBusinessObject|Tin", Schema.Tin);

			return result;
		}

		protected ZQuery GetTinQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isBlank = comparisonOperator == SQLComparisonOperator.IsBlank;
			var isBlankOrNegative = comparisonOperator.IsNegativeSQLOperator() || isBlank;

			var subQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH, isBlankOrNegative);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, BrazilOrgCusCodeInfo.OrgCusCodes.TIN);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, isBlank ? SQLComparisonOperator.IsNotBlank : comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);

			var result = new ZDBOnlyQuery(typeof(CusBRForeignOperator));
			if (isBlankOrNegative)
			{
				result.AddToFilter(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, SQLComparisonOperator.Equal, null);
			}
			result.AddSubQuery(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, subQuery, JoinCondition.Or);
			return result;
		}

		protected ZQuery GetCountryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryWithForeignOperator(comparisonOperator, CusBRForeignOperatorSchema.BFR_RN_NKCountryCode, typeof(OrgAddress), OrgAddressSchema.OA_OH, OrgAddressSchema.OA_RN_NKCountryCode, value);
		}

		protected ZQuery GetNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryWithForeignOperator(comparisonOperator, CusBRForeignOperatorSchema.BFR_Name, typeof(OrgHeader), OrgHeaderSchema.PK, OrgHeaderSchema.OH_FullName, value);
		}

		ZQuery GetQueryWithForeignOperator(SQLComparisonOperator comparisonOperator, SchemaColumn column, Type subQueryTableType, SchemaColumn subQueryKeyColumn, SchemaColumn subQueryColumn, ZString value)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			var isBlankOrNegative = isNegative || comparisonOperator == SQLComparisonOperator.IsBlank;

			var subQuery = new ZDBOnlySubQuery(subQueryTableType, subQueryKeyColumn, isNegative);
			subQuery.AddToFilter(subQueryColumn, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
			if (subQueryColumn == OrgAddressSchema.OA_RN_NKCountryCode)
			{
				var addressCapabilityQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				addressCapabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, SQLComparisonOperator.Equal, true);
				subQuery.AddSubQuery(addressCapabilityQuery, JoinCondition.And);
			}

			var result = new ZDBOnlyQuery(typeof(CusBRForeignOperator));
			var mainTableQuery = new ZQuery(column, comparisonOperator, value);
			if (isNegative)
			{
				mainTableQuery.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, null);
			}
			if (isBlankOrNegative)
			{
				result.AddToFilter(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, SQLComparisonOperator.Equal, null);
			}
			result.AddSubQuery(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, subQuery, JoinCondition.Or);
			result.AddToFilter(mainTableQuery, isBlankOrNegative ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		ZQuery GetMessageStatusQuery(ZString value)
		{
			if (value == "NOT")
			{
				value = ZString.Empty;
			}
			return new ZQuery(CusBRForeignOperatorSchema.BFR_MessageStatus, value);
		}
	}
}
