using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class CHInterchangeProvider : InterchangeProviderBase
{
	public CHInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
		: base(messageCollection)
	{
	}

	protected override string InstructionHowToSetInterchangeSenderID => string.Empty;

	protected override System.Type InterchangeType => typeof(CHEDIInterchange);

	public static CHInterchangeProvider New(NonDependentEDIMessageCollection messages)
	{
		CHInterchangeProvider interchangeProvider = null;
		if (messages.Count > 0)
		{
			switch (messages[0].EM_ApplicationCode)
			{
				case ApplicationCodeList.Codes.CHCustomsEdec:
					interchangeProvider = new CHCInterchangeProvider(messages);
					break;
				case ApplicationCodeList.Codes.CHCustomsPassar:
					interchangeProvider = new CHPInterchangeProvider(messages);
					break;
				case ApplicationCodeList.Codes.CHCustomsCharteraOutput:
					interchangeProvider = new CHOInterchangeProvider(messages);
					break;
			}
		}
		return interchangeProvider;
	}

	protected sealed override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		var message = messages[0];
		var to = GetEI_ToFromMessageType(message);

		var logMessage = ValidateMessage(message, to);
		if (!logMessage.IsEmpty)
		{
			message.Notes.AddNew(true, (NoResString)"Processing Log", logMessage);
			message.EM_Status = EDIMessageStatusList.Codes.Failed;
			interchange.Delete();
			return;
		}

		interchange.EI_ApplicationCode = message.EM_ApplicationCode;
		interchange.EI_TransportType = EDIInterchange.TransportType.xT;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_GB = message.EM_GB;
		SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, to, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
		SetInterchangeValues(interchange, message);
	}

	protected abstract ZString GetEI_ToFromMessageType(EDIMessage message);

	protected abstract void SetInterchangeValues(EDIInterchange interchange, EDIMessage message);

	ZString ValidateMessage(EDIMessage message, ZString to)
	{
		if (message.EM_LinkedObject == null)
		{
			return NoLinkedObjectErrorMessage;
		}

		if (message.EM_MessageText.IsEmpty)
		{
			return NoMessageTextErrorMessage;
		}

		if (to.IsEmpty)
		{
			return NoToErrorMessage;
		}

		return ZString.Empty;
	}

	protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

	protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

	protected override ZString ProcessedMessageStatusCode(EDIInterchange interchange) => EDIMessageStatusList.Codes.Sent;

	protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

	ZString NoToErrorMessage => Res.GetString("671A5201-6F4D-4202-B53B-1020902187EC", "EDI Interchange EI_To is not define so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

	ZString NoLinkedObjectErrorMessage => Res.GetString("DCF7049C-692A-497D-9ED5-1A72140F6E0F", "Message's Linked Object is null so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

	ZString NoMessageTextErrorMessage => Res.GetString("ABF33B10-B6EA-48AD-94AB-35099C381240", "Message's Message Text is empty so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

	ZString MessageSetToFailedErrorMessage => Res.GetString("538D11FD-D82E-4094-BD63-465C80B80D89", "The message's status has been set to 'Failed'.");
}
