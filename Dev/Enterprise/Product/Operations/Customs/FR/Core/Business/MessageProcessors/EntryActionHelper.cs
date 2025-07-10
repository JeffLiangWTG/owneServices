using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public static class EntryActionHelper
	{
		public static CusEntryHeader GetEntryHeaderFromEntryNumber(BusinessObjectFactory factory, ZString entryNumber, ZString country)
		{
			if (!entryNumber.IsEmpty)
			{
				var subQuery = GetBasicEntryQuery(country);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);

				var matchingEntryTypes = new string[] { EU.Business.MessageTypeList.Codes.Import, EU.Business.MessageTypeList.Codes.Export };
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, matchingEntryTypes);

				var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
				query.AddSubQuery(CusEntryHeaderSchema.PK, subQuery, JoinCondition.And);

				return factory.LoadTop1<CusEntryHeader>(query);
			}

			return null;
		}

		public static CusEntryHeader GetEntryHeaderFromMessage(BusinessObjectFactory factory, IResponseDataProvider dataProvider, ZString country)
		{
			if (!dataProvider.Refdos.IsEmpty)
			{
				var subQuery = GetBasicEntryQuery(country);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, dataProvider.TransactionID.Right(10));
				var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
				query.AddSubQuery(CusEntryHeaderSchema.PK, subQuery, JoinCondition.And);

				return factory.LoadTop1<CusEntryHeader>(query);
			}

			return null;
		}

		static ZDBOnlySubQuery GetBasicEntryQuery(ZString country)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);
			return subQuery;
		}

		public static FREDIMessage GetOutgoingMessage(CusEntryHeader entry, FREDIMessage incomingMessage)
		{
			var incomingDeltaStatus = GetDeltaStatus(incomingMessage);
			FREDIMessage lastOutgoingMessage = null;
			var interchangeNumber = incomingMessage.EM_InterchangeNumber;
			foreach (var outgoingMessage in entry.Messages.OfType<FREDIMessage>().Where(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit))
			{
				if (interchangeNumber.StartsWith(outgoingMessage.EM_InterchangeNumber + "."))
				{
					return outgoingMessage;
				}
				else if (lastOutgoingMessage == null || lastOutgoingMessage.EM_SystemCreateTimeUtc < outgoingMessage.EM_SystemCreateTimeUtc)
				{
					lastOutgoingMessage = outgoingMessage;
				}
			}
			return lastOutgoingMessage;
		}

		static ZString GetDeltaStatus(FREDIMessage incomingMessage)
		{
			if (incomingMessage.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
			{
				return (incomingMessage.MessageDataObject as IResponseDataProvider)?.Etat ?? ZString.Empty;
			}

			throw new NotSupportedException("Should only expect to get status from RCV message.");
		}

		static ZString GetMessageEvent(FREDIMessage incomingMessage)
		{
			if (incomingMessage.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
			{
				return (incomingMessage.MessageDataObject as IResponseDataProvider)?.Evenement ?? ZString.Empty;
			}

			return ZString.Empty;
		}

		public static ZString GetEntryStatus(FREDIMessage incomingMessage)
		{
			var deltaStatus = GetDeltaStatus(incomingMessage);
			return DeltaStatusToEntryStatusMap.GetCodeFromDescription(deltaStatus) ?? ZString.Empty;
		}

		public static ZString GetEntryAction(CusEntryHeader entry, FREDIMessage message)
		{
			if (message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit)
			{
				return message.EM_MessageSubType;
			}

			if (message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
			{
				var outgoingMessage = GetOutgoingMessage(entry, message);
				return GetEntryAction(entry, outgoingMessage);
			}

			throw new NotSupportedException("Only TRX & RCV messages are supported.");
		}

		public static bool HasError(FREDIMessage message)
		{
			if (message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
			{
				return (message.MessageDataObject as IResponseDataProvider)?.HasErrors ?? false;
			}

			throw new NotSupportedException("Only RCV messages are supported.");
		}

		public static bool IsCustomsPendingAcknowledgementRequestStatus(ZString status)
		{
			return status == EntryStatusDescriptionCodeList.Codes.ES114 || status == EntryStatusDescriptionCodeList.Codes.ES115;
		}

		public static bool IsCustomsAcknowledgedRequestStatus(ZString status)
		{
			return status == EntryStatusDescriptionCodeList.Codes.ES116 || status == EntryStatusDescriptionCodeList.Codes.ES117;
		}

		public static bool IsCustomsDecisionStatus(ZString status)
		{
			return status == EntryStatusDescriptionCodeList.Codes.ES118 || status == EntryStatusDescriptionCodeList.Codes.ES119;
		}

		public static bool IsUserDecisionStatus(ZString status)
		{
			return status == EntryStatusDescriptionCodeList.Codes.ES120;
		}

		public static bool IsDecisionStatus(ZString status)
		{
			return IsCustomsDecisionStatus(status) || IsUserDecisionStatus(status);
		}

		public static bool IsIntermediateStatus(ZString status)
		{
			return IsCustomsPendingAcknowledgementRequestStatus(status) || IsCustomsAcknowledgedRequestStatus(status) || IsDecisionStatus(status) || status == EntryStatusDescriptionCodeList.Codes.ES010;
		}

		public static bool IsOverridableRequestStatus(ZString status)
		{
			return IsCustomsAcknowledgedRequestStatus(status) || IsCustomsDecisionStatus(status) || IsUserDecisionStatus(status);
		}

		public static bool IsGreaterThanOrEqualToBAE(ZString status)
		{
			return int.TryParse(status, out var weight) && int.TryParse(EntryStatusDescriptionCodeList.Codes.ES100, out var baeWeight) && weight >= baeWeight;
		}

		public static bool HasVariousBAEStatus(CusEntryHeader entryHeader)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, GetVariousBAEStatuses());
			return entryHeader.Logs.Find(query).Any();
		}

		public static bool IsVariousBAEStatus(ZString status)
		{
			return GetVariousBAEStatuses().Contains(status);
		}

		public static ZString[] GetVariousBAEStatuses()
		{
			return new ZString[]
			{
				EntryStatusDescriptionCodeList.Codes.ES100,
				EntryStatusDescriptionCodeList.Codes.ES101
			};
		}

		static bool MustNotUpdateEntryStatus(ZString responseStatus, ZString entryStatus)
		{
			bool result = false;

			if (IsCustomsDecisionStatus(responseStatus) && !IsCustomsAcknowledgedRequestStatus(entryStatus))
			{
				result = true;
			}
			else if (responseStatus == EntryStatusDescriptionCodeList.Codes.ES113)
			{
				result = true;
			}

			return result;
		}

		static bool IsResponseWeightStatusSuperiorToEntryStatus(string responseStatus, string entryStatus)
		{
			int.TryParse(entryStatus, out int entryStatusWeight);
			int.TryParse(responseStatus, out int responseStatusWeight);
			return responseStatusWeight > entryStatusWeight;
		}

		static bool MustUpdateEntryStatus(ZString responseStatus, ZString entryStatus)
		{
			bool result = false;

			switch (responseStatus)
			{
				case EntryStatusDescriptionCodeList.Codes.ES060:
				case EntryStatusDescriptionCodeList.Codes.ES061:
					if (entryStatus == EntryStatusDescriptionCodeList.Codes.ES062 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES063 || EntryActionHelper.IsOverridableRequestStatus(entryStatus))
					{
						result = true;
					}
					break;
				case EntryStatusDescriptionCodeList.Codes.ES070:
				case EntryStatusDescriptionCodeList.Codes.ES083:
					if (EntryActionHelper.IsOverridableRequestStatus(entryStatus))
					{
						result = true;
					}
					break;
				case EntryStatusDescriptionCodeList.Codes.ES080:
					if (entryStatus == EntryStatusDescriptionCodeList.Codes.ES081 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES082 || EntryActionHelper.IsOverridableRequestStatus(entryStatus))
					{
						result = true;
					}
					break;
				case EntryStatusDescriptionCodeList.Codes.ES090:
					result = true;
					break;
				case EntryStatusDescriptionCodeList.Codes.ES100:
				case EntryStatusDescriptionCodeList.Codes.ES101:
					if (EntryActionHelper.IsVariousBAEStatus(entryStatus) || EntryActionHelper.IsOverridableRequestStatus(entryStatus) ||
						entryStatus == EntryStatusDescriptionCodeList.Codes.ES111 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES112 ||
						entryStatus == EntryStatusDescriptionCodeList.Codes.ES113)
					{
						result = true;
					}
					break;
				case EntryStatusDescriptionCodeList.Codes.ES130:
					if (entryStatus == EntryStatusDescriptionCodeList.Codes.ES131 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES132)
					{
						result = true;
					}
					break;
				default:
					result = false;
					break;
			}
			return result;
		}

		public static bool ShouldUpdateEntryStatus(ZString responseStatus, ZString entryStatus)
		{
			if (MustUpdateEntryStatus(responseStatus, entryStatus))
			{
				return true;
			}

			if (MustNotUpdateEntryStatus(responseStatus, entryStatus))
			{
				return false;
			}

			return IsResponseWeightStatusSuperiorToEntryStatus(responseStatus, entryStatus);
		}

		public static bool ShouldRevertEntryStatus(ZString responseStatus)
		{
			return responseStatus == InboundDeltaStatusToEntryStatusMap.Codes.RS120_1;
		}

		public static bool TransactionMustBeDeleted(ZString responseStatus, CusEntryHeader entryHeader, FREDIMessage incomingMessage)
		{
			return (IsRectificationRefused(incomingMessage))
				|| responseStatus == EntryStatusDescriptionCodeList.Codes.ES090
				|| responseStatus == EntryStatusDescriptionCodeList.Codes.ES120
				|| IsWithdrawnRefused(entryHeader, incomingMessage);
		}

		public static bool TransactionMustBeConfirmed(ZString responseStatus, CusEntryHeader entryHeader, FREDIMessage incomingMessage)
		{
			return IsOriginalClear(responseStatus, GetEntryAction(entryHeader, incomingMessage))
				   || IsRectificationAccepted(incomingMessage)
				   || IsWithdrawnAccepted(responseStatus);
		}

		public static bool IsOriginalClear(ZString responseStatus, ZString entryAction)
		{
			return IsVariousBAEStatus(responseStatus) && (entryAction == EntryActionCodeList.Codes.VAA || entryAction == EntryActionCodeList.Codes.EAV || entryAction == EntryActionCodeList.Codes.VAL);
		}

		public static bool IsWithdrawnAccepted(ZString responseStatus)
		{
			return responseStatus == EntryStatusDescriptionCodeList.Codes.ES150;
		}

		public static bool IsWithdrawnRefused(CusEntryHeader entryHeader, FREDIMessage incomingMessage)
		{
			var entryAction = GetEntryAction(entryHeader, incomingMessage);
			var hasError = HasError(incomingMessage);
			return entryAction == EntryActionCodeList.Codes.INV && (hasError || IsInvalidationRefused(incomingMessage));
		}

		public static bool IsInvalidationRefused(FREDIMessage incomingMessage) => GetMessageEvent(incomingMessage) == RefusedInvalidationMessage;

		public static bool IsRectificationAccepted(FREDIMessage incomingMessage) => GetMessageEvent(incomingMessage) == AcceptedAmendmentMessage;

		public static bool IsRectificationRefused(FREDIMessage incomingMessage) => GetMessageEvent(incomingMessage) == RefusedAmendmentMessage;

		static ZString RefusedInvalidationMessage => (NoResString)"invalidation d'une déclaration refusée par la douane";

		static ZString RefusedAmendmentMessage => (NoResString)"Rectification d'une déclaration refusée par la douane";

		static ZString AcceptedAmendmentMessage => (NoResString)"Rectification d'une déclaration acceptée par la douane";

		static InboundDeltaStatusToEntryStatusMap DeltaStatusToEntryStatusMap => deltaStatusToEntryStatusMap ?? (deltaStatusToEntryStatusMap = new InboundDeltaStatusToEntryStatusMap());
		[ThreadStatic]
		static InboundDeltaStatusToEntryStatusMap deltaStatusToEntryStatusMap;

		public static bool EntryVALPendingSnapshotsMustBeConfirmed(FREDIMessage incomingMessage)
		{
			var result = false;
			if (incomingMessage != null)
			{
				var entryStatus = GetEntryStatus(incomingMessage);
				result = entryStatus == EntryStatusDescriptionCodeList.Codes.ES060 || entryStatus == EntryStatusDescriptionCodeList.Codes.ES061;
			}

			return result;
		}

		public static bool EntrySnapshotMustBeUpdated(FREDIMessage incomingMessage, string entryStatus)
		{
			int.TryParse(entryStatus, out int entryStatusWeight);
			return entryStatusWeight < maxStatusWeightBeforeD2M && incomingMessage != null && IsRectificationAccepted(incomingMessage);
		}

		public static bool IsStatusClear(ZString status)
		{
			return status == EntryStatusDescriptionCodeList.Codes.ES100 || status == EntryStatusDescriptionCodeList.Codes.ES101 || status == EntryStatusDescriptionCodeList.Codes.ES130;
		}

		const int maxStatusWeightBeforeD2M = 130;
	}
}
