using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public static class RequestedProcedureHelper
	{
		public static CodeDescriptionPairList GetCachedRequestedProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString messageType, ZString declarationType) => GetCachedRequestedProcedureCodeList(factory, dataGroupingCode, new ZString[] { messageType }, declarationType);

		public static CodeDescriptionPairList GetCachedRequestedProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] messageTypes, ZString declarationType)
		{
			var messageTypeKey = string.Join("_", messageTypes);
			var today = ZDateTime.Today;
			var key = $"ProcedureCodeListCore_{dataGroupingCode}_{messageTypeKey}_{declarationType}_{today}";

			return factory.GetCachedValue(key, delegate
			{
				var result = new CodeDescriptionPairList();

				var query = new ZDBOnlyQuery(typeof(RefCusCodeList));
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode);
				query.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				query.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				var subQuery = new ZDBOnlySubQuery(typeof(RefCusProcedure), RefCusProcedureSchema.ZZ6_ProcedureCode);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, SQLComparisonOperator.StartsWith, dataGroupingCode);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
				if (messageTypes.Length > 0)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Equal, messageTypes);
				}
				if (!declarationType.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_Group, SQLComparisonOperator.Contains, declarationType);
				}

				query.AddSubQuery(RefCusCodeListSchema.ZZD_Code, subQuery, JoinCondition.And);

				var procedureCodes = factory.Load<RefCusCodeList>(query).Distinct().OrderBy(p => p.ZZD_Code);
				if (procedureCodes != null)
				{
					foreach (var code in procedureCodes)
					{
						result.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
					}
				}

				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedPreviousProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString messageType, ZString declarationType, ZString procedureCode) => GetCachedPreviousProcedureCodeList(factory, dataGroupingCode, new ZString[] { messageType }, declarationType, procedureCode);

		public static CodeDescriptionPairList GetCachedPreviousProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] messageTypes, ZString declarationType, ZString procedureCode)
		{
			var messageTypeKey = string.Join("_", messageTypes);
			var today = ZDateTime.Today;
			var key = $"PreviousProcedureCodeListCore_{dataGroupingCode}_{procedureCode}_{messageTypeKey}_{declarationType}_{today}";

			return factory.GetCachedValue(key, delegate
			{
				var result = new CodeDescriptionPairList();

				var query = new ZDBOnlyQuery(typeof(RefCusCodeList));
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode);
				query.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				query.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				var subQuery = new ZDBOnlySubQuery(typeof(RefCusProcedure), RefCusProcedureSchema.ZZ6_PreviousProcedureCode);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, SQLComparisonOperator.StartsWith, dataGroupingCode);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
				if (!procedureCode.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
				}
				if (messageTypes.Length > 0)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Equal, messageTypes);
				}
				if (!declarationType.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_Group, SQLComparisonOperator.Contains, declarationType);
				}

				query.AddSubQuery(RefCusCodeListSchema.ZZD_Code, subQuery, JoinCondition.And);

				var procedureCodes = factory.Load<RefCusCodeList>(query).Distinct().OrderBy(p => p.ZZD_Code);
				if (procedureCodes != null)
				{
					foreach (var code in procedureCodes)
					{
						result.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
					}
				}

				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedAdditionalProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString messageType, ZString declarationType, ZString procedureCode, ZString previousProcedure) => GetCachedAdditionalProcedureCodeList(factory, dataGroupingCode, new ZString[] { messageType }, declarationType, procedureCode, previousProcedure);

		public static CodeDescriptionPairList GetCachedAdditionalProcedureCodeList(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] messageTypes, ZString declarationType, ZString procedureCode, ZString previousProcedure)
		{
			var messageTypeKey = string.Join("_", messageTypes);
			var today = ZDateTime.Today;
			var key = $"AdditionalProcedureCodeList_{dataGroupingCode}_{previousProcedure}_{procedureCode}_{messageTypeKey}_{declarationType}_{today}";

			return factory.GetCachedValue(key, delegate
			{
				var result = new CodeDescriptionPairList();

				var query = new ZDBOnlyQuery(typeof(RefCusCodeList));
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, dataGroupingCode);
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode);
				query.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				query.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				var subQuery = new ZDBOnlySubQuery(typeof(RefCusProcedure), RefCusProcedureSchema.ZZ6_Concession);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, SQLComparisonOperator.Contains, dataGroupingCode);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
				if (!procedureCode.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
				}
				if (!previousProcedure.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedure);
				}
				if (messageTypes.Length > 0)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Equal, messageTypes);
				}
				if (!declarationType.IsEmpty)
				{
					subQuery.AddToFilter(RefCusProcedureSchema.ZZ6_Group, SQLComparisonOperator.Contains, declarationType);
				}

				query.AddSubQuery(RefCusCodeListSchema.ZZD_Code, subQuery, JoinCondition.And);

				var procedureCodes = factory.Load<RefCusCodeList>(query).Distinct().OrderBy(p => p.ZZD_Code);
				if (procedureCodes != null)
				{
					foreach (var code in procedureCodes)
					{
						result.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
					}
				}

				return result;
			});
		}

		public static void ApplyAllowedComparisonOperatorList(ModuleTextFilter textFilter)
		{
			textFilter.ComparisonOperator_List.Clear();
			textFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
		}

		public static ZDBOnlyQuery GetJobDeclarationFromRequestedProcedureQuery(ZString filterText)
		{
			var jobComInvoiceLineSubQuery = GetClusterKeySubQueryOfJobComInvoiceLine();
			jobComInvoiceLineSubQuery.AddToFilter(JobComInvoiceLineSchema.JI_Procedure, SQLComparisonOperator.Like, filterText);

			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, jobComInvoiceLineSubQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		public static ZDBOnlyQuery GetCusEntryHeaderFromRequestedProcedureQuery(ZString filterText)
		{
			var jobComInvoiceLineSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
			jobComInvoiceLineSubQuery.AddToFilter(JobComInvoiceLineSchema.JI_Procedure, SQLComparisonOperator.Like, filterText);

			var cusEntryLineSubQuery = new ZDBOnlySubQuery(typeof(CusEntryLine), CusEntryLineSchema.CL_CH);
			cusEntryLineSubQuery.AddSubQuery(CusEntryLineSchema.PK, jobComInvoiceLineSubQuery, JoinCondition.And);

			var cusEntryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			cusEntryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.CH_ClusterKey, GetClusterKeySubQueryOfJobComInvoiceLine(), JoinCondition.And);
			cusEntryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.PK, cusEntryLineSubQuery, JoinCondition.And);

			return cusEntryHeaderQuery;
		}

		static ZDBOnlySubQuery GetClusterKeySubQueryOfJobComInvoiceLine()
		{
			var jobComInvoiceLineSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey);
			jobComInvoiceLineSubQuery.AddToFilter(JobComInvoiceLineSchema.JI_CL, SQLComparisonOperator.NotEqual, null);
			return jobComInvoiceLineSubQuery;
		}

		public static ZString GetRequestedProcedureFilterTextWithWildcards(ZString requestedProcedure) => requestedProcedure + Wildcard_AnyCharacters;

		public static ZString GetPreviousProcedureFilterTextWithWildcards(ZString previousProcedure) => Wildcard_2Characters + previousProcedure + Wildcard_AnyCharacters;

		public static ZString GetAdditionalProcedureFilterTextWithWildcards(ZString aditionalProcedure) => Wildcard_2Characters + Wildcard_2Characters + aditionalProcedure;

		const string Wildcard_2Characters = "__";
		const string Wildcard_AnyCharacters = "%";
	}
}
