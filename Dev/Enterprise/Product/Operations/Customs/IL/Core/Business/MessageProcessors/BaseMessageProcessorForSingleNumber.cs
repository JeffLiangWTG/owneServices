using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public abstract class BaseMessageProcessorForSingleNumber<TResponse> : ILBranchCustomsApplicationTypeMessageProcessorBase where TResponse : class
	{
		protected BaseMessageProcessorForSingleNumber(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ILCustoms;

		protected abstract ZString CouldNotLocateMessage { get; }

		protected abstract ZString MoreThanOneMessage { get; }

		protected abstract ZString EntryType { get; }

		protected abstract ZString GetMessageReferenceNumber(TResponse responseMessage);

		protected abstract ZDateTimeOffset GetResponseDateTime(TResponse responseMessage);

		protected abstract bool SupportsConsol { get; }

		protected abstract ElectronicFormEventLogManager<TResponse> GetElectronicFormEventLogManager();

		protected abstract void ClearMessageReference(EnterpriseBusinessObject enterpriseBusinessObject);

		protected abstract ZString GetApplicationId(TResponse responseMessage);

		protected abstract ZString GetStatusName(BusinessObjectFactory factory, TResponse responseMessage);

		#region UCK
		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var result = ProcessMessageCommonLogic(message);
			if (result.EnterpriseBusinessObject != null)
			{
				return (message.EM_GB, result.EnterpriseBusinessObject, (NoResString)ZString.Empty);
			}
			return GetDiscardResult(message.EM_GB, result);
		}
		#endregion

		#region UCQ
		protected override void ProcessMessageCore(EDIMessage message)
		{
			ProcessMessageInternal(message);
			base.ProcessMessageCore(message);
		}

		EnterpriseBusinessObject ProcessMessageInternal(EDIMessage message)
		{
			var result = ProcessMessageCommonLogic(message);
			if (result.EnterpriseBusinessObject != null)
			{
				ProcessSingleEntry(result.IlEDIMessage, result.ResponseMessage, result.ReferenceNumber, result.EnterpriseBusinessObject);
			}
			else
			{
				HandleMessageFailureBasedOnResult(result);
			}
			return result.EnterpriseBusinessObject;
		}

		void ProcessSingleEntry<TBizObject>(ILEDIMessage ilEDIMessage, TResponse response, ZString number, TBizObject enterpriseBusinessObject)
			where TBizObject : EnterpriseBusinessObject
		{
			var eventType = GetElectronicFormEventLogManager()?.DetermineEventAndLog(response, enterpriseBusinessObject);
			if (eventType == Events.MessageWithdrawCancelAccepted && enterpriseBusinessObject is EnterpriseBusinessObject electronicMessageProvider)
			{
				ClearMessageReference(electronicMessageProvider);
			}

			var statusName = GetStatusName(ilEDIMessage.Factory, response);
			enterpriseBusinessObject.Logs.AddNew(
				eventType: Events.CustomsEntryStatus,
				reference: $"{number} {statusName}",
				dateTime: GetResponseDateTime(response));

			ilEDIMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			enterpriseBusinessObject.Logs.AddNew(Events.MessageReceived, ilEDIMessage.EM_MessageType);
		}

		void HandleMessageFailureBasedOnResult((EnterpriseBusinessObject EnterpriseBusinessObject, ILEDIMessage IlEDIMessage, TResponse ResponseMessage, int LoadedEntryNumbersLength, ZString ReferenceNumber) result)
		{
			if (result.IlEDIMessage != null)
			{
				var (_, _, discardReason) = GetDiscardResult(default, result);
				switch (discardReason.ToString())
				{
					case var s when s == CouldNotLocateMessage:
						DiscardMessage(result.IlEDIMessage, CouldNotLocateMessage);
						break;
					case var s when s == MoreThanOneMessage:
						DiscardMessage(result.IlEDIMessage, MoreThanOneMessage);
						break;
					case var s when s == Constants.MessageProcessors.CouldNotLocateByOriginalSentMessageMessage:
						HandleExceptionMessage(result.IlEDIMessage, result.ResponseMessage);
						break;
					case var s when s == Constants.MessageProcessors.ApplicationIDIsExceptionMessage:
						HandleExceptionMessage(result.IlEDIMessage, result.ResponseMessage);
						break;
				}
			}
		}

		(ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) GetDiscardResult(ZGuid branchPK, (EnterpriseBusinessObject EnterpriseBusinessObject, ILEDIMessage IlEDIMessage, TResponse ResponseMessage, int LoadedEntryNumbersLength, ZString ReferenceNumber) result)
		{
			if (result.IlEDIMessage != null)
			{
				string discardReasonText;
				if (GetApplicationId(result.ResponseMessage) != CustomsResponseHeader.ExceptionApplicationCode)
				{
					if (result.LoadedEntryNumbersLength == 0)
					{
						discardReasonText = CouldNotLocateMessage;
					}
					else
					{
						discardReasonText = MoreThanOneMessage;
					}
				}
				else
				{
					if (result.IlEDIMessage.EM_LinkedObject == null)
					{
						discardReasonText = Constants.MessageProcessors.CouldNotLocateByOriginalSentMessageMessage;
					}
					else
					{
						discardReasonText = Constants.MessageProcessors.ApplicationIDIsExceptionMessage;
					}
				}
				return (branchPK, null, (NoResString)discardReasonText);
			}
			return (branchPK, null, Constants.MessageProcessors.UnexpectedMessage);
		}

		void DiscardMessage(ILEDIMessage ilMessage, string noteText)
		{
			ilMessage.EM_Status = EDIMessage.Status.Discarded;
			var note = ilMessage.Notes.AddNew();
			note.ST_NoteText = noteText;
		}

		void HandleExceptionMessage(ILEDIMessage ilEDIMessage, TResponse response)
		{
			if (ilEDIMessage.EM_LinkedObject == null)
			{
				DiscardMessage(ilEDIMessage, Constants.MessageProcessors.CouldNotLocateByOriginalSentMessageMessage);
				return;
			}

			ilEDIMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			var linkedEnterpriseObject = (EnterpriseBusinessObject)ilEDIMessage.EM_LinkedObject;
			linkedEnterpriseObject.Logs.AddNew(Events.MessageReceived, ilEDIMessage.EM_MessageType);

			GetElectronicFormEventLogManager()?.LogEventWithParameters(response, linkedEnterpriseObject, Events.MessageRejected);
		}
		#endregion

		(EnterpriseBusinessObject EnterpriseBusinessObject, ILEDIMessage IlEDIMessage, TResponse ResponseMessage, int LoadedEntryNumbersLength, ZString ReferenceNumber) ProcessMessageCommonLogic(EDIMessage message)
		{
			if (message is ILEDIMessage ilEDIMessage
				&& ilEDIMessage.MessageDataObject is MessageDataObject<TResponse> messageDataObject)
			{
				var responseMessage = messageDataObject.MessageData;
				if (GetApplicationId(responseMessage) != CustomsResponseHeader.ExceptionApplicationCode)
				{
					var factory = ilEDIMessage.Factory;
					var number = GetMessageReferenceNumber(responseMessage);
					var countryCode = ilEDIMessage.GetCountryCodeSafe();

					var linkedObjectTableName = AutoJobShipment.Schema.TableName;
					var loadedEntryNumbers = LoadEntryNumber(factory, EntryType, number, countryCode, linkedObjectTableName);
					if (SupportsConsol && loadedEntryNumbers.Length == 0)
					{
						linkedObjectTableName = AutoJobConsol.Schema.TableName;
						loadedEntryNumbers = LoadEntryNumber(factory, EntryType, number, countryCode, linkedObjectTableName);
					}

					switch (loadedEntryNumbers.Length)
					{
						case 0:
							return (null, ilEDIMessage, responseMessage, 0, number);
						case > 1:
							return (null, ilEDIMessage, responseMessage, loadedEntryNumbers.Length, number);
						case 1:
							var loadedEntryNumber = loadedEntryNumbers.Single();
							EnterpriseBusinessObject enterpriseBusinessObject = null;

							switch (linkedObjectTableName)
							{
								case AutoJobShipment.Schema.TableName:
									enterpriseBusinessObject = GetEnterpriseBusinessObject<ForwardingShipment>(ilEDIMessage, loadedEntryNumber);
									break;
								case AutoJobConsol.Schema.TableName:
									enterpriseBusinessObject = GetEnterpriseBusinessObject<ForwardingConsol>(ilEDIMessage, loadedEntryNumber);
									break;
							}

							return (enterpriseBusinessObject, ilEDIMessage, responseMessage, 1, number);
					}
				}

				return (null, ilEDIMessage, responseMessage, 0, ZString.Empty);
			}

			return (null, null, default, 0, ZString.Empty);
		}

		CusEntryNumber[] LoadEntryNumber(BusinessObjectFactory factory, ZString entryType, ZString entryNumber, ZString countryCode, string parentTable)
		{
			var zQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
			zQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			zQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			zQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, parentTable);
			return factory.Load<CusEntryNumber>(zQuery);
		}

		EnterpriseBusinessObject GetEnterpriseBusinessObject<TBizObject>(ILEDIMessage ilEDIMessage, CusEntryNumber cusEntryNumber)
			where TBizObject : EnterpriseBusinessObject
		{
			var factory = ilEDIMessage.Factory;
			return factory.Load<TBizObject>(cusEntryNumber.CE_ParentID);
		}
	}
}
