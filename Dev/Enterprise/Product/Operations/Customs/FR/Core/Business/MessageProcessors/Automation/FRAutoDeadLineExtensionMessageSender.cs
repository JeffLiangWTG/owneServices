using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRAutoDeadLineExtensionMessageSender : FRAutoMessageSender
	{
		public FRAutoDeadLineExtensionMessageSender(ICommonLogger logger, ZString deltaMode) : base(logger)
		{
			this.deltaMode = deltaMode;
		}
		readonly ZString deltaMode;

		protected override bool RunInFirstActiveBranch => false;

		protected override ZString MessageType => ZString.Empty;

		protected override bool IsAutomationTurnedOn()
		{
			return FRCustomsDataRegistry.Instance.FRAutomatedModification.Value.EnableAutomatedModification && IsRegistryTimeSuperiorToActualTime();
		}

		bool IsRegistryTimeSuperiorToActualTime()
		{
			var registryTime = new ZDateTime(2023, 01, 01, FRCustomsDataRegistry.Instance.FRAutomatedModification.Value.TimeByDefault.Hour, FRCustomsDataRegistry.Instance.FRAutomatedModification.Value.TimeByDefault.Minute, 00);
			var currentTime = new ZDateTime(2023, 01, 01, ZDateTime.Now.Hour, ZDateTime.Now.Minute, 00);
			return registryTime <= currentTime;
		}

		protected override IEnumerable<ZGuid> GetCandidateCusEntryHeaderPKsPerBranch(BusinessObjectFactory factory)
		{
			var query = GetCandidateForCusEntryNumberFilter(Common.EU.EUJobMessageTypeList.Codes.Import);
			var query2 = GetCandidateForCusEntryNumberFilter(Common.EU.EUJobMessageTypeList.Codes.Export);

			var cusEntryHeaderPKs = new DynamicBusinessObjectCollection(factory);
			cusEntryHeaderPKs.Load(FormattableString.Invariant($"SELECT CE_ParentID FROM dbo.CusEntryNum {query.GetAsWhereAndOrderByClause(false)}"), query.Params);
			var pKImport = cusEntryHeaderPKs.Select(x => new ZGuid(x[CusEntryNumSchema.Constants.CE_ParentID]));

			var cusEntryHeaderPKs2 = new DynamicBusinessObjectCollection(factory);
			cusEntryHeaderPKs2.Load(FormattableString.Invariant($"SELECT CE_ParentID FROM dbo.CusEntryNum {query2.GetAsWhereAndOrderByClause(false)}"), query2.Params);
			var pKIExport = cusEntryHeaderPKs2.Select(x => new ZGuid(x[CusEntryNumSchema.Constants.CE_ParentID]));

			return pKImport.Concat(pKIExport);
		}

		ZDBOnlyQuery GetCandidateForCusEntryNumberFilter(ZString messageType)
		{
			var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, messageType);

			var referenceDataSubQuery = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryHandlingBlanks(JobDeclaration.Schema.JE_DeltaMode, SQLComparisonOperator.Equal, deltaMode);
			declarationQuery.AddToFilter(referenceDataSubQuery, JoinCondition.And);

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(GlbBranchSchema.PK, Environment.Env.CurrentBranchPK);
			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(Declaration.CusEntryHeader), CusEntryNumSchema.CE_ParentID);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, MessageStatusCodeList.Codes.AWR);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, CandidateEntriesRequiredStatus);
			entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);

			var entryNumberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, messageType);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today.AddDays(-60));
			entryNumberQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return entryNumberQuery;
		}

		protected override void DoExtendThingsBeforeSendingMessage(Declaration.CusEntryHeader entry)
		{
			var cei = entry.EntryInstruction;
			if (cei != null)
			{
				cei.CEI_DateForDuty = cei.CEI_DateForDuty.AddDays(1);
				entry.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			}
		}

		protected override bool ReadyToSend(Declaration.CusEntryHeader entry) => entry.EntryInstruction?.CEI_DateForDuty.IsToday ?? false;

		protected virtual ZString CandidateEntriesRequiredStatus => ZString.Empty;
	}
}
