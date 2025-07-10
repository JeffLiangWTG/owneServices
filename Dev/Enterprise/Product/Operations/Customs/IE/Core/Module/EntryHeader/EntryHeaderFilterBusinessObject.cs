using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Module
{
	public class EntryHeaderFilterBusinessObject : EU.Module.EntryHeaderFilterBusinessObject
	{
		public static class FilterConstants
		{
			public const string CustomsDocsReqdByDate = JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqdByDate;
			public static ResourceString CustomsDocsReqdByDateMultilingualDescription => JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqdByDateMultilingualDescription;

			public const string CustomsDocsReqd = JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqd;
			public static ResourceString CustomsDocsReqdMultilingualDescription => JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqdMultilingualDescription;
			public const string CustomsDocsReqdFilterPrompt = JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsDocsReqdFilterPrompt;

			public static ResourceString CustomsRegistrationNumberMultilingualDescription => JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsRegistrationNumberMultilingualDescription;
			public const string CustomsRegistrationNumber = JobDeclarationFilterBusinessObject.DeclarationFilterConstants.CustomsRegistrationNumber;
		}

		protected override bool SupportRequestedProcedureCore => true;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var customsDocsReqdByDateFilter = filters.AddDateFilter(FilterConstants.CustomsDocsReqdByDate, GetCustomsDocsReqdByDateQuery);
			customsDocsReqdByDateFilter.Category = FilterCategories.Dates;
			customsDocsReqdByDateFilter.MultilingualDescription = FilterConstants.CustomsDocsReqdByDateMultilingualDescription;

			var customsDocsReqdFilter = filters.AddFlagsFilter(FilterConstants.CustomsDocsReqd, new string[] { FilterConstants.CustomsDocsReqdFilterPrompt }, new GetFlagsQuery[] { GetCustomsDocsReqdQuery });
			customsDocsReqdFilter.Category = FilterCategories.StatusAndFlags;
			customsDocsReqdFilter.MultilingualDescription = FilterConstants.CustomsDocsReqdMultilingualDescription;

			var customsRegistrationNumberFilter = filters.AddTextFilter(FilterConstants.CustomsRegistrationNumber, GetCustomsRegistrationNumberQuery);
			customsRegistrationNumberFilter.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			customsRegistrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			customsRegistrationNumberFilter.MultilingualDescription = FilterConstants.CustomsRegistrationNumberMultilingualDescription;

			return filters;
		}

		ZQuery GetCustomsDocsReqdByDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var requestedDocumentSubQuery = new ZDBOnlySubQuery(typeof(RequestedDocument), CusSupportingInfoSchema.CSI_ParentID);
			requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
			AddDateTimeRange(requestedDocumentSubQuery, comparisonOperator, JoinCondition.And, CusSupportingInfoSchema.CSI_DateOfExpiry, fromDate, toDate);

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, requestedDocumentSubQuery, JoinCondition.And);

			return entryHeaderQuery;
		}

		ZQuery GetCustomsDocsReqdQuery(ZBool value)
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));

			if (value)
			{
				var requestedDocumentSubQuery = new ZDBOnlySubQuery(typeof(RequestedDocument), CusSupportingInfoSchema.CSI_ParentID);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.NotEqual, RequestedDocumentStatusList.Codes.RequestCancelled);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.NotEqual, RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived);

				entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, requestedDocumentSubQuery, JoinCondition.And);
			}

			return entryHeaderQuery;
		}

		ZQuery GetCustomsRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString filterText)
		{
			var customsRegistrationNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			customsRegistrationNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CustomsReleaseNumber);
			customsRegistrationNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, filterText);

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.PK, customsRegistrationNumberQuery, JoinCondition.And);

			return entryHeaderQuery;
		}
	}
}
