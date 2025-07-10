using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public static class CustomsMessageHelper
{
	public static Dictionary<string, string> CreateHeaderAttributes() => new Dictionary<string, string>();

	public static Dictionary<string, string> AddBpId(this Dictionary<string, string> dictionary) => dictionary.AddOrUpdate(MessagingConstants.CustomMsgAttributes.BpId, EnvironmentHelper.GetBusinessPartnerId());

	public static Dictionary<string, string> AddMessageId(this Dictionary<string, string> dictionary, ZString messageId) => dictionary.AddOrUpdate(MessagingConstants.CustomMsgAttributes.MessageID, messageId);

	public static Dictionary<string, string> AddLastMessageId(this Dictionary<string, string> dictionary, ZString messageId) => dictionary.AddOrUpdate(MessagingConstants.CustomMsgAttributes.LastMessageID, messageId);

	public static Dictionary<string, string> AddMessageType(this Dictionary<string, string> dictionary, ZString messageType) => dictionary.AddOrUpdate(MessagingConstants.CustomMsgAttributes.MessageType, messageType);

	public static Dictionary<string, string> AddPartnerTopicIfEnabled(this Dictionary<string, string> dictionary) => CHCustomsDataRegistry.Instance.EnablePartnerTopic.Value ? dictionary.AddOrUpdate(MessagingConstants.CustomMsgAttributes.PartnerTopic, PartnerTopic) : dictionary;

	public const string PartnerTopicPrefix = "CW-";

	public static string PartnerTopic => $"{PartnerTopicPrefix}{GlbCompany.CurrentCompany.LicenceKeyIdentifier}";

	static Dictionary<string, string> AddOrUpdate(this Dictionary<string, string> dictionary, string key, string value)
	{
		dictionary[key] = value;
		return dictionary;
	}

	public static void CreateEDIInterchange(this EDIMessage message) => CreateEDIInterchanges(message);

	public static EDIInterchange[] CreateEDIInterchanges(params EDIMessage[] messages)
	{
		EDIInterchange[] interchanges = null;

		if (messages.Length > 0)
		{
			var messageCollection = new NonDependentEDIMessageCollection(messages[0].Factory);
			messageCollection.AddRange(messages);

			var provider = CHInterchangeProvider.New(messageCollection);
			if (provider != null)
			{
				provider.PackCollatedMessagesIntoInterchanges();
				interchanges = provider.Interchanges;
			}
		}
		return interchanges;
	}
}
