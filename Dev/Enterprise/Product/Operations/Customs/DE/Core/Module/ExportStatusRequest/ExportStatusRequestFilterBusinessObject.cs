using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class ExportStatusRequestFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Module = "Module";
			public const string DateOfRequest = "Date of Request";
			public const string MRN = "Movement Reference Number";
			public const string HasResponse = "Has Response";
			public const string CreatedUser = "Creating User";
			public const string RequestTime = "Request Time";
			public const string LastEditTime = "Last Edit Time";
			public const string LastEditUser = "Last Edit User";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var moduleTextFilter = result.AddTextFilter(Schema.Module, GetModuleQuery, Lookups.ModuleCodeList);
			moduleTextFilter.Category = FilterCategories.ModesAndTypes;
			moduleTextFilter.MultilingualDescription = ResString.GetMultilingualString("acd934c9-fb64-4e53-a180-a656c2e9a36c", Schema.Module);

			var dateOfRequestDateFilter = result.AddDateFilter(Schema.DateOfRequest, EDIMessageSchema.EM_SystemCreateTimeUtc, true);
			dateOfRequestDateFilter.Category = FilterCategories.Dates;
			dateOfRequestDateFilter.MultilingualDescription = ResString.GetMultilingualString("a9530102-d9b6-4791-ae78-98e131695891", Schema.DateOfRequest);

			var mrnFilter = result.AddTextFilter(Schema.MRN, CusEntryNumSchema.CE_EntryNum);
			mrnFilter.Category = FilterCategories.NumbersAndReferences;
			mrnFilter.SupportsBlankComparisonOperators = false;
			mrnFilter.SubGroup = new MRNSubGroup();
			mrnFilter.MultilingualDescription = ResString.GetMultilingualString("766428bb-ee2e-463e-aa97-32bb2fb34752", Schema.MRN);

			var hasResponseFlagsFilter = result.AddFlagsFilter(Schema.HasResponse,
				new string[] { Res.GetString("8AA4719A-FD18-435A-9457-8C88F91F572E", "Ticked for yes, unticked for no") },
				new GetFlagsQuery[] { GetHasResponseQuery });
			hasResponseFlagsFilter.SubGroup = new HasResponseSubGroup();
			hasResponseFlagsFilter.MultilingualDescription = ResString.GetMultilingualString("b0c09e31-7ef7-45a0-a5df-e05627eba2a9", Schema.HasResponse);

			return result;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			if (filters[FilterDescriptions.CreatedTime] is ModuleDateFilter createdTimeFilter)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}

		ZQuery GetHasResponseQuery(ZBool value)
		{
			var comparisonOperator = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(EDIMessageSchema.EM_Status, comparisonOperator, EDIMessage.Status.Acknowledged);
		}

		ZQuery GetModuleQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var moduleQuery = new ZQuery();
			string applicationCodeToQuery;
			switch (value)
			{
				case ExportStatusRequestModuleCodeList.Codes.AES:
					applicationCodeToQuery = EDIMessage.ApplicationCodes.DECustomsAesSystem;
					break;
				case ExportStatusRequestModuleCodeList.Codes.NCTS:
					applicationCodeToQuery = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
					break;
				default:
					return moduleQuery;
			}
			moduleQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, comparisonOperator, applicationCodeToQuery);
			return moduleQuery;
		}

		ExportStatusRequestFilterStripBusinessObjectLookups lookups;
		ExportStatusRequestFilterStripBusinessObjectLookups Lookups => lookups ?? (lookups = new ExportStatusRequestFilterStripBusinessObjectLookups(this));

		class MRNSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var qry = new ZDBOnlyQuery(typeof(EDIMessage));
				var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, EDIMessage.Schema.TableName);
				mrnQuery.AddToFilter(filter);
				qry.AddSubQuery(mrnQuery, JoinCondition.And);
				return qry;
			}
		}

		class HasResponseSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var queries = filter.GetCompositeParts();
				var distinctQueryStrings = queries.Select(x => x.FilterString).Distinct().ToArray();
				var result = new ZDBOnlyQuery(typeof(EDIMessage));
				if (distinctQueryStrings.Length == 1)
				{
					result.AddToFilter(queries[0]);
				}
				return result;
			}
		}
	}
}
