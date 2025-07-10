using System;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public abstract class MessageProcessor<T> : BranchCustomsApplicationTypeMessageProcessor
	{
		protected MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		public const int RetryCount = 2;
		const int RetryDelayed = 1;

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.IC2;
		protected virtual bool FindManifestHeaderWithFallbackCusEntryNumber => false;

		#region PreProcessMessage

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			var messageObject = InitializeMessageObject(message);

			if (message.EM_LinkedObject == null)
			{
				var manifestHeader = FindManifestHeader(message, messageObject);

				if (manifestHeader != null)
				{
					message.EM_GB = manifestHeader.AMA_GB;
					message.EM_LinkedObject = manifestHeader;
					message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
					message.EM_RetryCount = 0;
				}
				else if (message.EM_RetryCount == RetryCount)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Discarded;
					message.EM_RetryCount = 0;
					var discardedNote = Res.GetString("A8FF1E53-876B-4B06-8EF7-6486B00E6EB3", "Unable to find manifest header for message (Number:{0}, Type:{1}); message status set to DISCARDED.", message.EM_MessageNum, message.EM_MessageType);
					Logger.LogWarning(discardedNote);
					message.Notes.AddNew(true, Res.GetString("c72a752b-ff7e-489c-bec8-bc4bf56c3bce", "Customs Message Error"), discardedNote);
					NotifyPreProcessFailure(message, messageObject);
				}
				else
				{
					message.EM_RetryCount++;
					message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(RetryDelayed);
				}
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			}
		}

		T InitializeMessageObject(EDIMessage message) => XmlObjectSerializer.Deserialize<T>(message.EM_MessageText);

		AsycudaManifestHeader FindManifestHeader(EDIMessage message, T messageObject)
		{
			var manifestHeader = FindManifestHeaderBySessionGUID(message);

			if (manifestHeader == null && GetLocalReferenceNumber != null)
			{
				var localReferenceNumber = GetLocalReferenceNumber(messageObject);
				manifestHeader = FindManifestHeaderByCusEntryNumber(localReferenceNumber, CusEntryNumberTypes.Standard.LocalReferenceNumber, message);
			}

			if (manifestHeader == null && GetMasterReferenceNumber != null)
			{
				var masterReferenceNumber = GetMasterReferenceNumber(messageObject);
				manifestHeader = FindManifestHeaderByCusEntryNumber(masterReferenceNumber, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, message);
			}

			if (manifestHeader == null && FindManifestHeaderFromSpecificContext != null)
			{
				manifestHeader = FindManifestHeaderFromSpecificContext(messageObject, message);
			}

			return manifestHeader;
		}

		AsycudaManifestHeader FindManifestHeaderBySessionGUID(EDIMessage message)
		{
			var result = default(AsycudaManifestHeader);

			var sessionGUID = message.Interchange?.EI_SessionGUID ?? ZGuid.Empty;
			if (!sessionGUID.IsEmpty)
			{
				var interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionGUID);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.EUICS2);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_TransportType, message.Interchange.EI_TransportType);

				var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
				messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				messageQuery.AddToFilter(EDIMessageSchema.EM_Status, StatusOfOutgoingEDIMessage);
				messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IC2);
				messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeQuery, JoinCondition.And);
				messageQuery.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " desc";

				var transmitMessage = message.Factory.LoadTop1<EDIMessage>(messageQuery);
				result = transmitMessage?.EM_LinkedObject as AsycudaManifestHeader;
			}

			return result;
		}

		protected AsycudaManifestHeader FindManifestHeaderByCusEntryNumber(string cusEntryNumber, string entryType, EDIMessage message)
		{
			AsycudaManifestHeader manifestHeader = null;

			if (!string.IsNullOrEmpty(cusEntryNumber))
			{
				var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, cusEntryNumber);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ManifestBase.AutoAsycudaManifestHeader.Schema.TableName);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);

				var manifestHeaderQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				manifestHeaderQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, entryNumQuery, JoinCondition.And);
				manifestHeaderQuery.OrderBy = AsycudaManifestHeaderSchema.Constants.AMA_SystemCreateTimeUtc + OrderByClause.Descending;

				manifestHeader = message.Factory.LoadTop1<AsycudaManifestHeader>(manifestHeaderQuery);
			}

			return manifestHeader;
		}

		protected virtual Func<T, string> GetLocalReferenceNumber => null;

		protected virtual Func<T, string> GetMasterReferenceNumber => null;

		protected virtual Func<T, EDIMessage, AsycudaManifestHeader> FindManifestHeaderFromSpecificContext => null;

		protected virtual string StatusOfOutgoingEDIMessage => EDIMessage.Status.Sent;

		protected virtual void NotifyPreProcessFailure(EDIMessage message, T messageObject)
		{
		}

		#endregion

		#region ProcessMessage

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message.EM_Status == EDIMessageStatusList.Codes.PreProcessedOK)
			{
				var messageObject = InitializeMessageObject(message);
				ProcessMessageCore(message, messageObject);
			}
		}

		protected abstract void ProcessMessageCore(EDIMessage message, T messageObject);

		#endregion

		protected EDIMessage GetOriginalMessage(AsycudaManifestHeader manifestHeader)
		{
			return manifestHeader?.Messages.Where(m => m.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit).OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();
		}
	}
}
