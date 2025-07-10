using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Module
{
	public class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
	{
		public class DeclarationFilterConstants : EU.Module.DeclarationFilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
			public const string CustomsDocsReqdByDate = "Customs Docs. Req. By";
			public static ResourceString CustomsDocsReqdByDateMultilingualDescription => ResString.GetMultilingualString("8DF18683-2B94-452A-B989-1F559189DDAB", DeclarationFilterConstants.CustomsDocsReqdByDate);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
			public const string CustomsDocsReqd = "Customs Docs. Req.";
			public static ResourceString CustomsDocsReqdMultilingualDescription => ResString.GetMultilingualString("26A88216-1BD6-4DD0-B9F2-E963F357EB35", DeclarationFilterConstants.CustomsDocsReqd);
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
			public const string CustomsDocsReqdFilterPrompt = "At least one Customs Docs. Req. open or pending";

			public const string LocalReferenceNumber = "LRN";
			public static ResourceString LocalReferenceNumberMultilingualDescription => ResString.GetMultilingualString("BF94FFD7-95A5-4F76-ABFC-324DB5E0B97A", DeclarationFilterConstants.LocalReferenceNumber);

			public static ResourceString CustomsRegistrationNumberMultilingualDescription => ResString.GetMultilingualString("34215CB8-866F-42F7-9C2C-4AEFAB3BA301", DeclarationFilterConstants.CustomsRegistrationNumber);
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
			public const string CustomsRegistrationNumber = "Customs Registration Number";
		}

		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

		protected override bool SupportRequestedProcedureCore => true;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var customsDocsReqdByDateFilter = filters.AddDateFilter(DeclarationFilterConstants.CustomsDocsReqdByDate, GetCustomsDocsReqdByDateQuery);
			customsDocsReqdByDateFilter.Category = FilterCategories.Dates;
			customsDocsReqdByDateFilter.MultilingualDescription = DeclarationFilterConstants.CustomsDocsReqdByDateMultilingualDescription;

			var customsDocsReqdFilter = filters.AddFlagsFilter(DeclarationFilterConstants.CustomsDocsReqd, new string[] { DeclarationFilterConstants.CustomsDocsReqdFilterPrompt }, new GetFlagsQuery[] { GetCustomsDocsReqdQuery });
			customsDocsReqdFilter.Category = FilterCategories.StatusAndFlags;
			customsDocsReqdFilter.MultilingualDescription = DeclarationFilterConstants.CustomsDocsReqdMultilingualDescription;

			var localReferenceNumberFilter = filters.AddTextFilter(DeclarationFilterConstants.LocalReferenceNumber, GetLRNumberQuery);
			localReferenceNumberFilter.WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_BGMReference);
			localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			localReferenceNumberFilter.MultilingualDescription = DeclarationFilterConstants.LocalReferenceNumberMultilingualDescription;

			var customsRegistrationNumberFilter = filters.AddTextFilter(DeclarationFilterConstants.CustomsRegistrationNumber, GetCustomsRegistrationNumberQuery);
			customsRegistrationNumberFilter.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			customsRegistrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			customsRegistrationNumberFilter.MultilingualDescription = DeclarationFilterConstants.CustomsRegistrationNumberMultilingualDescription;

			return filters;
		}

		protected override void AddApplicationCodeFilter(ModuleFilterCollection filters)
		{
			var applicationCodeFilter = new ModuleTextFilter(DeclarationFilterConstants.SubmitType, JobDeclarationSchema.JE_ApplicationCode, Lookups.ApplicationCodeSearchFilterList);
			RemoveCodesExcept(applicationCodeFilter.ComparisonOperator_List, new[] { ModuleTextFilter.ComparisonConstants.Exact });

			applicationCodeFilter.Category = FilterCategories.ModesAndTypes;
			applicationCodeFilter.MultilingualDescription = ApplicationCodeFilterCaption;
			filters.AddFilter(applicationCodeFilter);
		}

		void RemoveCodesExcept(CodeDescriptionPairList list, IEnumerable<string> codes)
		{
			foreach (var code in list.GetAllCodes().Except(codes))
			{
				list.RemoveCode(code);
			}
		}

		protected override bool SupportsExitControlCore => true;

		ZQuery GetLRNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var chQ = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			chQ.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, comparisonOperator, value);
			var mainQ = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQ.AddSubQuery(chQ, JoinCondition.And);
			return mainQ;
		}

		ZQuery GetCustomsDocsReqdByDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var requestedDocumentSubQuery = new ZDBOnlySubQuery(typeof(EU.Business.RequestedDocument), CusSupportingInfoSchema.CSI_ParentID);
			requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
			AddDateTimeRange(requestedDocumentSubQuery, comparisonOperator, JoinCondition.And, CusSupportingInfoSchema.CSI_DateOfExpiry, fromDate, toDate);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			entryInstructionSubQuery.AddSubQuery(CusEntryInstructionSchema.PK, requestedDocumentSubQuery, JoinCondition.And);

			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryInstructionSubQuery, JoinCondition.And);

			return jobDeclarationQuery;
		}

		ZQuery GetCustomsDocsReqdQuery(ZBool value)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (value)
			{
				var requestedDocumentSubQuery = new ZDBOnlySubQuery(typeof(EU.Business.RequestedDocument), CusSupportingInfoSchema.CSI_ParentID);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.NotEqual, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.NotEqual, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived);

				var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				entryInstructionSubQuery.AddSubQuery(CusEntryInstructionSchema.PK, requestedDocumentSubQuery, JoinCondition.And);

				jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryInstructionSubQuery, JoinCondition.And);
			}

			return jobDeclarationQuery;
		}

		ZQuery GetCustomsRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString filterText)
		{
			var customsRegistrationNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			customsRegistrationNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CustomsReleaseNumber);
			customsRegistrationNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, filterText);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.PK, customsRegistrationNumberQuery, JoinCondition.And);

			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.PK, entryHeaderQuery, JoinCondition.And);

			return jobDeclarationQuery;
		}
	}
}
