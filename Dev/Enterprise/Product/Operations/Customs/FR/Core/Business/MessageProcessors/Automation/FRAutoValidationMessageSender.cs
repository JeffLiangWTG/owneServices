using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRAutoValidationMessageSender : FRAutoMessageSender
	{
		public FRAutoValidationMessageSender(ICommonLogger logger) : base(logger)
		{
		}

		protected override bool RunInFirstActiveBranch => false;

		protected override ZString MessageType => ZString.Empty;

		protected override bool IsAutomationTurnedOn()
		{
			return FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.Value.EnableAutomatedValidation;
		}

		protected override IEnumerable<ZGuid> GetCandidateCusEntryHeaderPKsPerBranch(BusinessObjectFactory factory)
		{
			var query = GetCandidateCusEntryHeaderFilter();

			var cusEntryHeaderPKs = new DynamicBusinessObjectCollection(factory);
			cusEntryHeaderPKs.Load(FormattableString.Invariant($"SELECT CH_PK FROM dbo.CusEntryHeader {query.GetAsWhereAndOrderByClause(false)}"), query.Params);
			return cusEntryHeaderPKs.Select(x => new ZGuid(x[CusEntryHeaderSchema.Constants.PK]));
		}

		ZQuery GetCandidateCusEntryHeaderFilter()
		{
			var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_ContainerMode, Core.Constants.ContainerModes.FCL);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_GB, GlbBranch.CurrentBranch.PK);

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, CandidateEntriesRequiredStatus);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, MessageStatusCodeList.Codes.AWR);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, MessageStatusCodeList.Codes.Error);

			var frCusEntryHeaderSQL = @"CH_PK IN (SELECT CH_PK FROM dbo.FRCusEntryHeader WHERE CH_TriggeringPointForValidation <> '' AND CH_TriggeringPointForValidation <> 'NUL')";
			entryHeaderQuery.AddFilterAndZSQLParameterCollection(frCusEntryHeaderSQL, new ZSqlParameterCollection(), JoinCondition.And);

			entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);

			return entryHeaderQuery;
		}

		protected override bool ReadyToSend(CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			var shipment = declaration.Shipment;
			if (shipment != null)
			{
				var entryContainers = entry.Containers;
				var hasNonTrackableContainers = entryContainers.Any(x => x.CO_FCL_LCL_AIR != Core.Constants.ContainerModes.FCL && x.CO_FCL_LCL_AIR != Core.Constants.ContainerModes.FCLMixedShipper);
				if (!hasNonTrackableContainers)
				{
					var containerNumbers = entryContainers.Select(x => x.CO_ContainerNumber);
					if (containerNumbers.Any())
					{
						var eventCode = string.Empty;

						switch (entry.CH_TriggeringPointForValidation)
						{
							case TriggerPointsCodeList.Codes.PAB:
								eventCode = Events.ArrivalCode;
								break;
							case TriggerPointsCodeList.Codes.VAQ:
								eventCode = Events.FreightUnloadedCode;
								break;
							case TriggerPointsCodeList.Codes.REC:
								eventCode = Events.GateInCode;
								break;
						}

						if (!eventCode.IsNullOrEmpty())
						{
							var pcs = GetInterestedPCS(entry);
							var bizOCollection = eventCode == Events.ArrivalCode ? (IEnumerable<EnterpriseBusinessObject>)shipment.TransportsIncludingRelated.Cast<Transport>() ?? Enumerable.Empty<Transport>()
								: (IEnumerable<EnterpriseBusinessObject>)shipment.Containers ?? Enumerable.Empty<CommonContainer>();
							if (containerNumbers.All(x => BizOCollectionHasEventFromPCS(declaration, bizOCollection, eventCode, pcs, x)))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		protected virtual ZString CandidateEntriesRequiredStatus => ZString.Empty;

		ZBool BizOCollectionHasEventFromPCS(JobDeclaration declaration, IEnumerable<EnterpriseBusinessObject> bizOCollection, ZString eventCode, ZString pcs, ZString containerNumber)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			if (eventCode == Events.ArrivalCode)
			{
				var exportDate = declaration.JE_ExportDate;
				if (exportDate.IsEmpty)
				{
					query.IsNoResultQuery = true;
				}
				else
				{
					query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, exportDate);
				}
			}

			return bizOCollection.Any(bizO => bizO.Logs.Find(query).Any(x => EventHasMessageFromPCS(x, pcs) && EventHasContainerNumber(x, containerNumber)));
		}

		ZBool EventHasMessageFromPCS(StmALog log, ZString pcs)
		{
			return log.RelatedEDIMessage is MessageValueObject obj && obj.Sender == pcs;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter string")]
		ZBool EventHasContainerNumber(StmALog log, ZString containerNumber)
		{
			return log.SourceInfoItems.Any(x => x.Key == "Container Number" && x.Data == containerNumber);
		}

		ZString GetInterestedPCS(CusEntryHeader entry)
		{
			var declaration = entry.Declaration;

			var operationalPort = ZString.Empty;
			if (declaration.IsImport)
			{
				operationalPort = declaration.JE_RL_NKPortOfArrival;
			}
			else if (declaration.IsExport)
			{
				operationalPort = declaration.JE_RL_NKPortOfLoading;
			}

			var communitySystemCodesOfForwarderAndAgent = PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent
				.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
				.OfType<CommunitySystemCodesOfForwarderAndAgent>()
				.FirstOrDefault(x => x.Port == operationalPort)?.PCS;

			return communitySystemCodesOfForwarderAndAgent ?? ZString.Empty;
		}
	}
}
