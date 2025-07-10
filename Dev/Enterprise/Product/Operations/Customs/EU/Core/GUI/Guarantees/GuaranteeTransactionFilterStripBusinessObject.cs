using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI
{
	public class GuaranteeTransactionFilterStripBusinessObject : Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class EUFilterConstants
		{
			public const string TypeGuarantee = "Type";
			public const string ReferenceGuarantee = "Reference";
			public const string ValueGuarantee = "Value";
			public const string CommentGuarantee = "Comment";
			public const string StatusGuarantee = "Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddTypeFilter(filters);
			AddReferenceFilter(filters);
			AddValueFilter(filters);
			AddCommentFilter(filters);
			AddStatusFilter(filters);
			return filters;
		}

		#region Type Filter

		void AddTypeFilter(ModuleFilterCollection filters)
		{
			var transactionTypeFilter = filters.AddTextFilter(EUFilterConstants.TypeGuarantee, GetMovementTypeQuery, GuaranteesTypeList)
				.WithMaxLengthOf<ModuleTextFilter>(CusPermitLineTransactionSchema.CPL_TransactionType);
			transactionTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			transactionTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			transactionTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			transactionTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			transactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("8A8BAACD-FEC8-454E-AE7A-41DEE76F6B83", EUFilterConstants.TypeGuarantee);
			transactionTypeFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetMovementTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionType, comparisonOperator, value);
		}

		public CodeDescriptionPairList GuaranteesTypeList
		{
			get { return Factory.GetCachedValue<PermitTransactionTypeList>() + Factory.GetCachedValue<GuaranteeTransactionTypeList>(); }
		}

		#endregion

		#region Reference Filter

		void AddReferenceFilter(ModuleFilterCollection filters)
		{
			var referenceFilter = filters.AddTextFilter(EUFilterConstants.ReferenceGuarantee, CusPermitLineTransactionSchema.CPL_Reference);
			referenceFilter.MultilingualDescription = ResString.GetMultilingualString("7F091E92-2E91-4311-9E6F-95EB7A2FFC4C", EUFilterConstants.ReferenceGuarantee);
			referenceFilter.Category = FilterCategories.TextSearch;
		}

		#endregion

		#region Value Filter

		void AddValueFilter(ModuleFilterCollection filters)
		{
			var valueFilter = filters.AddNumberRangeFilter(EUFilterConstants.ValueGuarantee, CusPermitLineTransactionSchema.CPL_TranValue);
			valueFilter.MultilingualDescription = ResString.GetMultilingualString("1679650C-D1AF-4D28-9911-A6F147CB58C6", EUFilterConstants.ValueGuarantee);
			valueFilter.Category = FilterCategories.NumbersAndReferences;
			valueFilter.Decimals = 2;
		}

		#endregion

		#region Comment Filter

		void AddCommentFilter(ModuleFilterCollection filters)
		{
			var referenceFilter = filters.AddTextFilter(EUFilterConstants.CommentGuarantee, CusPermitLineTransactionSchema.CPL_Comment);
			referenceFilter.MultilingualDescription = ResString.GetMultilingualString("303EF30F-F8F5-4264-8BAB-DA75F411ED01", EUFilterConstants.CommentGuarantee);
			referenceFilter.Category = FilterCategories.TextSearch;
		}

		#endregion

		#region Status Filter

		void AddStatusFilter(ModuleFilterCollection filters)
		{
			var transactionStatusFilter = filters.AddTextFilter(EUFilterConstants.StatusGuarantee, GetStatusQuery, GuaranteesStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(CusPermitLineTransactionSchema.CPL_TransactionStatus);
			transactionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			transactionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			transactionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			transactionStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			transactionStatusFilter.MultilingualDescription = ResString.GetMultilingualString("3BFE3B58-3A1A-471C-970E-FD91B71D1E8E", EUFilterConstants.StatusGuarantee);
			transactionStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionStatus, comparisonOperator, value);
		}

		public CodeDescriptionPairList GuaranteesStatusList => Factory.GetCachedValue<PermitTransactionStatusList>();

		#endregion
	}
}
