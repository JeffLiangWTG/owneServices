using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.Module
{
	public class EntryHeaderFilterBusinessObject : EU.Module.EntryHeaderFilterBusinessObject
	{
		public static class FilterConstants
		{
			public const string DateOfExit = "Date of Exit";
			public const string ActualOfficeOfExit = "Actual Office of Exit";
			public const string InventoryConsignmentReference = "Inventory Consignment Reference (MUCR)";
			public const string DUCR = "DUCR (Declaration Unique Consignment Reference)";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			var exitDateFilter = filters.AddSingleDateFilter(FilterConstants.DateOfExit, GetExitDateQuery);
			exitDateFilter.Category = FilterCategories.Dates;
			exitDateFilter.MultilingualDescription = ResString.GetMultilingualString("49197E7E-3EC6-448D-B468-129C01082956", FilterConstants.DateOfExit);

			var actualOfficeOfExitFilter = GetAddInfoTextFilter(FilterConstants.ActualOfficeOfExit, EUAddInfoSchema.ZG_ExitActualOffice.Name.Substring(3));
			actualOfficeOfExitFilter.MultilingualDescription = ResString.GetMultilingualString("6547DDC7-D12A-471A-823E-D687BE357587", FilterConstants.ActualOfficeOfExit);
			actualOfficeOfExitFilter.Category = FilterCategories.TextSearch;
			actualOfficeOfExitFilter.MaxLength = EUAddInfoSchema.ZG_ExitActualOffice.MaxLength;
			filters.AddFilter(actualOfficeOfExitFilter);

			var invRefFilter = filters.AddTextFilter(FilterConstants.InventoryConsignmentReference, GetInventoryConsignmentReferenceQuery);
			invRefFilter.MultilingualDescription = ResString.GetMultilingualString("316CDAE6-E26A-4692-A21C-22DBCBF13E91", FilterConstants.InventoryConsignmentReference);
			invRefFilter.Category = FilterCategories.NumbersAndReferences;

			var ducrFilter = filters.AddTextFilter(FilterConstants.DUCR, GetDUCRQuery);
			ducrFilter.MultilingualDescription = ResString.GetMultilingualString("C7D2E5F8-AB12-4B88-932E-987654321DEF", FilterConstants.DUCR);
			ducrFilter.Category = FilterCategories.NumbersAndReferences;

			return filters;
		}

		ZQuery GetExitDateQuery(ZDateTime value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				result = Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, value.ToISO8601ShortDateString(), CusEntryHeaderSchema.CH_AddInfo, EUAddInfoSchema.ZG_ExitDate.Name.Substring(3));
			}

			return result;
		}

		ZQuery GetDUCRQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
				var sub = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
				sub.AddToFilter(JobDeclarationSchema.JE_UCR, comparisonOperator, value);
				query.AddSubQuery(sub, JoinCondition.And);
				result = query;
			}

			return result;
		}

		ModuleTextFilter GetAddInfoTextFilter(ZString description, string addInfoPropertyName)
		{
			return new AddInfoModuleTextFilter(description, CusEntryHeaderSchema.CH_AddInfo, addInfoPropertyName);
		}

		ZQuery GetInventoryConsignmentReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var cusEntryNumQ = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQ.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
				cusEntryNumQ.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
				var mainQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
				mainQuery.AddSubQuery(cusEntryNumQ, JoinCondition.And);
				return mainQuery;
			}

			return new ZQuery();
		}
	}
}
