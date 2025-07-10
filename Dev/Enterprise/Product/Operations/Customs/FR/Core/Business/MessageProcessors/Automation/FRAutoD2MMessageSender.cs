using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRAutoD2MMessageSender : FRAutoMessageSender
	{
		public FRAutoD2MMessageSender(ICommonLogger logger) : base(logger)
		{
		}

		public int DaysWaitBeforeSendingDeltaDStep2 => FRCustomsDataRegistry.Instance.NbDaysWaitBeforeSendingDeltaDStep2.Value;

		protected override bool RunInFirstActiveBranch => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.D2M;

		protected override bool IsAutomationTurnedOn()
		{
			return DaysWaitBeforeSendingDeltaDStep2 > 0;
		}

		protected override IEnumerable<ZGuid> GetCandidateCusEntryHeaderPKsPerBranch(BusinessObjectFactory factory)
		{
			var query = GetCandidateCusEntryNumberFilter();

			var cusEntryHeaderPKs = new DynamicBusinessObjectCollection(factory);
			cusEntryHeaderPKs.Load(FormattableString.Invariant($"SELECT CE_ParentID FROM dbo.CusEntryNum {query.GetAsWhereAndOrderByClause(false)}"), query.Params);
			return cusEntryHeaderPKs.Select(x => new ZGuid(x[CusEntryNumSchema.Constants.CE_ParentID]));
		}

		ZQuery GetCandidateCusEntryNumberFilter()
		{
			var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.PK, Environment.Env.CurrentCompanyPK);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryNumSchema.CE_ParentID);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, EntryStatusDescriptionCodeList.Codes.ES100);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, MessageStatusCodeList.Codes.AWR);
			entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);

			var entryNumberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.France.Import);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today.AddDays(-DaysWaitBeforeSendingDeltaDStep2));
			entryNumberQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			entryNumberQuery.OrderBy = CusEntryNumSchema.Constants.CE_IssueDate;

			return entryNumberQuery;
		}

		protected override bool ReadyToSend(Declaration.CusEntryHeader entry) => entry.IsDeltaDStepOneSentOK && !entry.IsDeltaDStepTwoSentOK;
	}
}
