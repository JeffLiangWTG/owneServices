using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class TaxChangeAssessmentFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Type = "Type";
			public const string ReferenceNumber = "Reference Number";
			public const string LRN = "LRN";
			public const string IssueDate = "Issue Date";
			public const string MaturityDate = "Maturity Date";
			public const string Branch = "Branch";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var typeFilter = result.AddTextFilter(Schema.Type, GetTypeQuery);
			typeFilter.Category = FilterCategories.ModesAndTypes;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("4FB620F3-CE97-4F9E-9B99-790ACFC0FFBB", Schema.Type);

			var referenceNumberFilter = result.AddTextFilter(Schema.ReferenceNumber, CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			referenceNumberFilter.SupportsBlankComparisonOperators = false;
			referenceNumberFilter.SubGroup = new MRNCusEntryNumberSubGroup();
			referenceNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("8B6BA63F-A8A8-428D-8BDE-1B613DED7189", Schema.ReferenceNumber);

			var lrnFilter = result.AddTextFilter(Schema.LRN, GetLRNQuery);
			lrnFilter.Category = FilterCategories.NumbersAndReferences;
			lrnFilter.MultilingualDescription = ResString.GetMultilingualString("B484BBE2-8E86-4A7C-9648-BA09561C191B", Schema.LRN);

			var issueDateFilter = result.AddDateFilter(Schema.IssueDate, CusEntryNumSchema.CE_IssueDate);
			issueDateFilter.Category = FilterCategories.Dates;
			issueDateFilter.SubGroup = new MRNCusEntryNumberSubGroup();
			issueDateFilter.MultilingualDescription = ResString.GetMultilingualString("DEBFA551-22C9-43C7-8784-99E0FF9D5D11", Schema.IssueDate);

			var maturityDateFilter = result.AddDateFilter(Schema.MaturityDate, CusEntryNumSchema.CE_ExpiryDate);
			maturityDateFilter.Category = FilterCategories.Dates;
			maturityDateFilter.SubGroup = new MRNCusEntryNumberSubGroup();
			maturityDateFilter.MultilingualDescription = ResString.GetMultilingualString("1D00303F-A11A-404F-A0CA-99C3ED1AC393", Schema.MaturityDate);

			var branchFilter = result.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, EDIMessageSchema.EM_GB, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("0DE39F33-286E-4DC0-B55A-9E1558232929", Schema.Branch);
			branchFilter.SupportsBlankComparisonOperators = false;

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

		ZQuery GetTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetStmNoteQuery(comparisonOperator, TaxChangeAssessment.Schema.TaxChangeAssessmentType, value);
		}

		ZQuery GetLRNQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetStmNoteQuery(comparisonOperator, TaxChangeAssessment.Schema.LocalReferenceNumber, value);
		}

		ZQuery GetStmNoteQuery(SQLComparisonOperator comparisonOperator, ZString description, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			var stmNoteQuery = new ZDBOnlySubQuery(typeof(StmNote), StmNoteSchema.ST_ParentID);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.INT));
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Table, AutoEDIMessage.Schema.TableName);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Description, description);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_NoteText, comparisonOperator, value);
			query.AddSubQuery(stmNoteQuery, JoinCondition.And);
			return query;
		}

		class MRNCusEntryNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AutoEDIMessage.Schema.TableName);
				mrnQuery.AddToFilter(filter);
				query.AddSubQuery(mrnQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
