using CargoWise.Common;
using CargoWise.Customs.IN.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public abstract class BaseManifestMessageProcessor<T, U> : IEmailMessageProcessor where T : class, new() where U : AsycudaManifestHeader
{
	public bool CanProcess(EmailInfo emailInfo) => emailInfo.MessageIdOnAttachment == MessageID;

	public BusinessObject GetLinkedObject(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger = null)
	{
		var deserializeResponse = FlatFileMessage.DeserializeObject<T>(emailInfo.AttachmentText);
		var manifestHeader = FindManifestHeader(message.Factory, deserializeResponse);
		if (manifestHeader == null)
		{
			logger?.Log($"Failed to find matching Manifest by {GetManifestMatchingCriteriaForLog(deserializeResponse)}.", Integration.LogType.Warning);
		}

		return manifestHeader;
	}

	public bool Process(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger = null)
	{
		logger?.DebugLog($"Message processing by {GetType().Name}");

		var deserializeResponse = FlatFileMessage.DeserializeObject<T>(emailInfo.AttachmentText);
		if (!ResponseDataIsValid(deserializeResponse))
		{
			logger?.Log($"Failed to deserialize the message text to a valid {typeof(T).Name}.", Integration.LogType.Error);
			return false;
		}

		message.EM_MessageType = MessageType;
		message.EM_MessageSubType = GetMessageSubType(deserializeResponse);

		SetMessageStatusAndCustomsStatus(message.EM_LinkedObject, deserializeResponse);
		return true;
	}

	protected abstract string MessageID { get; }
	protected abstract string ManifestApplicationCode { get; }
	protected abstract string ManifestType { get; }
	protected abstract string TransportMode { get; }
	protected abstract string MessageType { get; }

	protected abstract string GetMessageSubType(T responseData);
	protected abstract bool IsPositive(T responseData);

	protected abstract string GetManifestMatchingCriteriaForLog(T responseData);
	protected virtual bool ResponseDataIsValid(T responseData) => true;

	protected virtual ZQuery GetManifestHeaderQuery(T responseData)
	{
		var headerQuery = (ZDBOnlyQuery)new ZDBOnlyQuery(typeof(U))
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ManifestApplicationCode)
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, ManifestType)
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_TransportMode, TransportMode)
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.India);

		headerQuery.AddSubQuery(GetMasterBillQuery(responseData), JoinCondition.And);
		headerQuery.OrderBy = AsycudaManifestHeaderSchema.Constants.AMA_SystemCreateTimeUtc + OrderByClause.Descending;
		return headerQuery;
	}

	protected virtual ZDBOnlySubQuery GetMasterBillQuery(T responseData)
	{
		return (ZDBOnlySubQuery)new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA)
			.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
	}

	protected virtual void SetBillStatus(BusinessObject linkObject, T responseData)
	{
	}

	U FindManifestHeader(BusinessObjectFactory factory, T responseData)
	{
		return ResponseDataIsValid(responseData) ? factory.LoadTop1<U>(GetManifestHeaderQuery(responseData)) : null;
	}

	void SetMessageStatusAndCustomsStatus(BusinessObject linkObject, T responseData)
	{
		var messageAttachee = linkObject as IMessageAttachee;
		if (messageAttachee == null)
		{
			ErrorReporter.ReportOnce(message: $"LinkObject is not IMessageAttachee, type is {linkObject.GetType().FullName}");
		}
		else
		{
			if (IsPositive(responseData))
			{
				messageAttachee.MessageStatus = MessageStatusList.Codes.MessageAccepted;
				messageAttachee.CustomsStatus = RegistrationStatusList.Codes.ManifestRegistered;
			}
			else
			{
				messageAttachee.MessageStatus = MessageStatusList.Codes.ErrorResponseReceived;
			}

			SetBillStatus(linkObject, responseData);
		}
	}
}
