using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public static class NctsMessageHelper
	{
		public static BusinessObject LocateLinkedObjectByEdiInterchange(EDIInterchange interchange)
		{
			return LocateOriginalOutgoingEdiMessage(interchange).EM_LinkedObject;
		}

		public static EDIMessage LocateOriginalOutgoingEdiMessage(EDIInterchange interchange)
		{
			var originalOutgoingInterchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID);
			originalOutgoingInterchangeQuery.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			originalOutgoingInterchangeQuery.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_Status, MessageStatusList.Codes.Sent));
			originalOutgoingInterchangeQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			var originalOutgoingInterchange = interchange.Factory.LoadTop1<BECInterchange>(originalOutgoingInterchangeQuery);

			var originalOutgoingEdiMessageQuery = new ZQuery(EDIMessageSchema.EM_EI, originalOutgoingInterchange.PK);
			originalOutgoingEdiMessageQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			originalOutgoingEdiMessageQuery.AddToFilter(new ZQuery(EDIMessageSchema.EM_Status, MessageStatusList.Codes.Sent));
			return interchange.Factory.LoadTop1<BEMessage>(originalOutgoingEdiMessageQuery);
		}

		public static CusInBondHeader LocateHeaderByLRNOrMRN(BusinessObjectFactory factory, IInboundProvider messageDataProvider, string subApplicationCode = null, string[] messageStatusArray = null, string headerType = null) =>
			LocateHeaderByLRN(factory, messageDataProvider, subApplicationCode) ?? LocateHeaderByMRN(factory, messageDataProvider, subApplicationCode, messageStatusArray, headerType);

		public static CusInBondHeader LocateHeaderByLRNOrMRNFallbackInterchange(BusinessObjectFactory factory, IInboundProvider messageDataProvider, EDIInterchange interchange, string subApplicationCode = null, string[] messageStatusArray = null, string headerType = null)
		{
			var header = LocateHeaderByLRNOrMRN(factory, messageDataProvider, subApplicationCode, messageStatusArray, headerType);
			if (header == null && interchange != null)
			{
				var linkedObject = LocateLinkedObjectByEdiInterchange(interchange);
				if (linkedObject is NctsHeader nctsHeader)
				{
					header = nctsHeader;
				}
				if (linkedObject is NctsDepartureMovementHeader movementHeader)
				{
					header = movementHeader.Header;
				}
			}
			return header;
		}

		public static CusInBondHeader LocateHeaderByLRN(BusinessObjectFactory factory, IInboundProvider messageDataProvider, string subApplicationCode = "")
		{
			var lrn = messageDataProvider.LRN;
			CusInBondHeader result = null;
			if (!string.IsNullOrEmpty(lrn))
			{
				var entryQuery = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				entryQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, lrn);
				if (!string.IsNullOrEmpty(subApplicationCode))
				{
					entryQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
				}
				var moveHeader = factory.LoadTop1<CusInBondMoveHeader>(entryQuery);

				if (moveHeader != null)
				{
					result = moveHeader.Header;
				}
			}

			return result;
		}

		public static CusInBondHeader LocateHeaderByMRN(BusinessObjectFactory factory, IInboundProvider messageDataProvider, string subApplicationCode = "", string[] messageStatusArray = null, string headerType = null)
		{
			var mrn = messageDataProvider.MRN;
			CusInBondHeader result = null;

			if (!string.IsNullOrEmpty(mrn))
			{
				var hasSubApplicationCode = !string.IsNullOrEmpty(subApplicationCode);
				var hasMessageStatusArray = messageStatusArray != null && messageStatusArray.Length > 0;
				var hasHeaderType = headerType != null;

				var cusInBondHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				if (hasHeaderType)
				{
					cusInBondHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, headerType);
				}
				if (hasMessageStatusArray)
				{
					var inBondHeaderArray = factory.Load<CusInBondHeader>(cusInBondHeaderQuery);
					if (!inBondHeaderArray.IsNullOrEmpty() && inBondHeaderArray.Length > 1)
					{
						var cusInBoundHeaderStatusQuery = NctsHeader.GetEffectiveMessageStatusSubQuery(messageStatusArray);
						cusInBondHeaderQuery.AddSubQuery(cusInBoundHeaderStatusQuery, JoinCondition.And);
					}
				}
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);
				cusInBondHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
				if (hasSubApplicationCode)
				{
					var cusInBondMoveHeader = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
					if (hasSubApplicationCode)
					{
						cusInBondMoveHeader.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
					}
					cusInBondHeaderQuery.AddSubQuery(cusInBondMoveHeader, JoinCondition.And);
				}
				result = factory.LoadTop1<CusInBondHeader>(cusInBondHeaderQuery);
			}

			if (result == null)
			{
				result = LocateHeaderByMRNLinkedToCusInBondMovementHeader(factory, messageDataProvider, subApplicationCode, messageStatusArray);
			}

			return result;
		}

		static CusInBondHeader LocateHeaderByMRNLinkedToCusInBondMovementHeader(BusinessObjectFactory factory, IInboundProvider messageDataProvider, string subApplicationCode = "", string[] messageStatusArray = null, string headerType = null)
		{
			var mrn = messageDataProvider.MRN;
			CusInBondHeader result = null;

			if (!string.IsNullOrEmpty(mrn))
			{
				var hasSubApplicationCode = !string.IsNullOrEmpty(subApplicationCode);
				var hasMessageStatusArray = messageStatusArray != null && messageStatusArray.Length > 0;
				var hasHeaderType = headerType != null;

				var cusInBondHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				if (hasHeaderType)
				{
					cusInBondHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, headerType);
				}
				if (hasMessageStatusArray)
				{
					var inBondHeaderArray = factory.Load<CusInBondHeader>(cusInBondHeaderQuery);
					if (!inBondHeaderArray.IsNullOrEmpty() && inBondHeaderArray.Length > 1)
					{
						var cusInBoundHeaderStatusQuery = NctsHeader.GetEffectiveMessageStatusSubQuery(messageStatusArray);
						cusInBondHeaderQuery.AddSubQuery(cusInBoundHeaderStatusQuery, JoinCondition.And);
					}
				}

				var cusInBondMoveHeader = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				if (hasSubApplicationCode)
				{
					cusInBondMoveHeader.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
				}

				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn);
				cusInBondMoveHeader.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

				cusInBondHeaderQuery.AddSubQuery(cusInBondMoveHeader, JoinCondition.And);
				result = factory.LoadTop1<CusInBondHeader>(cusInBondHeaderQuery);
			}

			return result;
		}

		public static CusInvPack RetrievePackageBySequencesForArrivalDeclaration(NctsHeader header, int houseConsignmentSequence, int houseConsignmentItemDeclarationSequence, int packageSequence)
		{
			var billSequence = houseConsignmentSequence.ToString();
			return header
				.Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == billSequence)
				?.ArrivalGoodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == houseConsignmentItemDeclarationSequence)
				?.Packages.Cast<NctsPackage>().FirstOrDefault(p => p.B5_SequenceNumber == packageSequence);
		}

		public static void RequestTADFromCustoms(NctsHeader header)
		{
			var ediMessage = (NCTSMessage)header.MovementHeader.Messages.AddNew(typeof(NCTSMessage));
			ediMessage.EM_MessageSubType = BE.Business.Constants.BECMessageSubtypes.Outgoing.TransitAccompanyingDocument;
		}

		public static void RequestFOLFromCustoms(NctsHeader header)
		{
			var ediMessage = (NCTSMessage)header.MovementHeader.Messages.AddNew(typeof(NCTSMessage));
			ediMessage.EM_MessageSubType = BE.Business.Constants.BECMessageSubtypes.Outgoing.FOL;
		}

		public static NctsHeader GetNctsHeaderFromLinkedObject(BEMessage message)
		{
			var linkedObject = message.EM_LinkedObject;
			NctsHeader returnValue = null;

			if (linkedObject is NctsHeader header)
			{
				returnValue = header;
			}
			else if (linkedObject is NctsDepartureMovementHeader movementHeader)
			{
				returnValue = movementHeader.Header;
			}

			return returnValue;
		}

		public static ZString GetUtcDateTimeToLocalBEBranchString(DateTime dateTimeUtc)
		{
			var result = ZString.Empty;
			if (dateTimeUtc.Kind == DateTimeKind.Utc)
			{
				var zDateTimeUtc = new ZDateTime(dateTimeUtc);
				if (zDateTimeUtc.IsValid)
				{
					result = zDateTimeUtc.ToLocalBranchTimeOffset().ToString("dd-MM-yyyy HH:mm:ss \"UTC\"z");
				}
			}
			return result;
		}
	}
}
