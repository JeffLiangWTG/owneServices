using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.Business;

public class BEInterchangeProvider : InterchangeProviderBase
{
	public BEInterchangeProvider(NonDependentEDIMessageCollection readyMessages) : base(readyMessages)
	{
	}

	protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

	protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		var requestMessage = messages.Cast<BEMessage>().Single();
		var externalPassword = GlbCompanyWrapper.GetWrapper<BEGlbCompanyWrapper>(interchange.Company).Credential;
		var messageType = requestMessage.EM_MessageType;
		var messageTo = requestMessage.EM_IsTestMessage ? (BECustomsRegistry.Instance.DetermineTestSystem.Value ? Constants.InterchangeRecievers.PRE : Constants.InterchangeRecievers.TEST) : Constants.InterchangeRecievers.LIVE;
		SetInterchangeValuesForTransmit(interchange, messages, messageType, messageTo, requestMessage.Company.LicenceKeyIdentifier);
		if (IsTADOrFOL(requestMessage))
		{
			SetMessageInterchangeReceiverToCC015CReceiver(requestMessage, interchange);
		}
		interchange.EI_TransportType = EDIInterchange.TransportType.xT;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.BECustoms;
		interchange.EI_Priority = ZString.Empty;
		interchange.EI_GP = externalPassword?.PK ?? ZGuid.Empty;

		var dictionary = BECustomsRegistry.Instance.SendUserAndSecretInHeader.Value && externalPassword != null
			? externalPassword.GetMessageAttrDictionary()
			: new Dictionary<string, string>();
		dictionary.Add(xTMessaging.Shared.Constants.CustomMsgAttributes.MessageSubType, requestMessage.EM_MessageSubType);

		if (GetCustomsReferenceValue(requestMessage) is ZString customsReference && !customsReference.IsEmpty)
		{
			dictionary.Add(Constants.CustomMsgAttributes.CustomsReference, customsReference);
		}

		if (IsTADOrFOL(requestMessage))
		{
			var language = SharedConstants.Languages.English;
			if (requestMessage.EM_MessageSubType == Constants.BECMessageSubtypes.Outgoing.FOL)
			{
				var communicationLanguage = ((Customs.Business.CusInBondMoveHeader)requestMessage.EM_LinkedObject).Header.BH_CommunicationLanguage.ToUpper();
				if (communicationLanguage == CountryCodes.Netherlands || communicationLanguage == CountryCodes.France || communicationLanguage == CountryCodes.Germany)
				{
					language = communicationLanguage;
				}
			}
			dictionary.Add(Constants.CustomMsgAttributes.Language, language.ToLower());
		}

		interchange.SetHeaderTextWithAttributeDictionary(dictionary, true);
	}

	protected void SetMessageInterchangeReceiverToCC015CReceiver(BEMessage message, EDIInterchange interchange)
	{
		var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, message.EM_LinkUniqueID);
		query.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageSubType, BETSDOutgoingMessageTypes.Codes.TS015);
		query.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_EI, SQLComparisonOperator.NotEqual, null);

		var messages015 = message.Factory.Load<EDIMessage>(query);
		var latestMessage015 = messages015.OrderByDescending(x => int.TryParse(x.EM_MessageNum, out var value) ? value : 0).FirstOrDefault();

		if(latestMessage015 != null)
		{
			interchange.EI_To = latestMessage015.EM_InterchangeReceiver;
		}
	}

	ZString GetCustomsReferenceValue(BEMessage message)
	{
		var messageType = message.EM_MessageType;
		var messageSubType = message.EM_MessageSubType;

		if (messageType == SendMessageTypes.Codes.TSD && messageSubType == BETSDOutgoingMessageTypes.Codes.TS414)
		{
			return ((CusEntryHeader)message.EM_LinkedObject).CRN;
		}
		else if ((messageType == SendMessageTypes.Codes.TSD && (messageSubType == BETSDOutgoingMessageTypes.Codes.TS207 || messageSubType == BETSDOutgoingMessageTypes.Codes.TS215))
			|| (messageType == SendMessageTypes.Codes.REN && messageSubType == BERENOutgoingMessageTypes.Codes._614))
		{
			return ((CusEntryHeader)message.EM_LinkedObject).MovementReferenceNumber;
		}
		else if (IsTADOrFOL(message))
		{
			var cusInBondHeader = ((Customs.Business.CusInBondMoveHeader)message.EM_LinkedObject).Header;
			var movementReferenceNumber = CusEntryNumber.Load(cusInBondHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, cusInBondHeader.Company.GC_RN_NKCountryCode);

			return movementReferenceNumber?.CE_EntryNum ?? ZString.Empty;
		}

		return ZString.Empty;
	}

	protected override string GetCollationKey(EDIMessage message) => message.PK.ToString();

	protected override ZString GetInterchangeFooter(int messageCount) => ZString.Empty;

	static bool IsTADOrFOL(BEMessage mesage) => mesage.EM_MessageSubType == Constants.BECMessageSubtypes.Outgoing.TransitAccompanyingDocument || mesage.EM_MessageSubType == Constants.BECMessageSubtypes.Outgoing.FOL;
}
