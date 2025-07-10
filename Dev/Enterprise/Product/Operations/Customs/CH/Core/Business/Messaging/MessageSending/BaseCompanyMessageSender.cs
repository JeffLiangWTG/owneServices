using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseCompanyMessageSender<TCredentials> where TCredentials : GlbExternalPassword
{
	protected BaseCompanyMessageSender(LoggingInformation logger)
	{
		this.logger = Argument.NotNull(logger, "logger");
	}

	protected readonly LoggingInformation logger;

	protected abstract string FriendlyName { get; }

	protected abstract ZString ApplicationCode { get; }

	protected abstract ZString MessageType { get; }
	protected abstract ZString MessageSubType { get; }

	public void Send(CancellationToken cancellationToken = new CancellationToken())
	{
		var factory = new BusinessObjectFactory();
		if (GlbCompany.GetCurrentCompany(factory) is GlbCompany company)
		{
			if (GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company) is GlbCompanyWrapper wrapper
				&& GetCompanyCredentials(wrapper) is TCredentials credentials
				&& CanSend(credentials))
			{
				var count = SendCore(cancellationToken, company, credentials);
				logger.Log($"{count} {FriendlyName}(s) for company {company.GC_Code} has been processed.");
			}
			else
			{
				logger.DebugLog($"No {FriendlyName} for company {company.GC_Code} has been processed.");
			}
		}
	}

	protected abstract TCredentials GetCompanyCredentials(GlbCompanyWrapper companyWrapper);

	protected abstract int SendCore(CancellationToken cancellationToken, GlbCompany company, TCredentials tokenCredentials);

	protected virtual bool CanSend(TCredentials tokenCredentials) => true;

	protected EDIMessage CreateEDIMessage(BusinessObjectFactory factory, BusinessObject linkedObject, ZString requestBody, ZString applicationReference, ZGuid? credentialsPK = null, ZByte? retryCount = null, ZDateTime? heldUntilDate = null)
	{
		var message = factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = ApplicationCode;
		message.EM_MessageType = MessageType;
		message.EM_MessageSubType = MessageSubType;
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageText = requestBody;
		message.EM_LinkedObject = linkedObject;
		message.EM_ApplicationReference = applicationReference.Left(message.EM_ApplicationReferenceInfo.MaxLength);
		message.EM_GP = credentialsPK ?? ZGuid.Empty;
		if (retryCount != null)
		{
			message.EM_RetryCount = retryCount.Value;
		}
		if (heldUntilDate != null)
		{
			message.EM_HeldUntilDate = heldUntilDate.Value;
		}
		return message;
	}

	protected EDIInterchange[] CreateEDIInterchanges(params EDIMessage[] messages) => CustomsMessageHelper.CreateEDIInterchanges(messages);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	protected const string MessagePlaceHolder = "<<MESSAGE PLACEHOLDER>>";
}
