using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageSenderPairList : MessagePartyPairList
	{
		public new class Codes : MessagePartyPairList.Codes
		{
			public const string OneStop = "1STOP";
			public const string CSXResponse = "CSXWTADL";
		}

		public new class Descriptions : MessagePartyPairList.Descriptions
		{
			public const string OneStop = "1-Stop Message Processing Centre";
		}

		public MessageSenderPairList()
			: base()
		{
			AddPair(Codes.OneStop, Descriptions.OneStop);
			AddPair(Codes.CSXResponse, Descriptions.CSXAdelaide);
		}
	}
}
