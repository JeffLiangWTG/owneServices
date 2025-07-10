using CargoWise.Customs.DE.MessageContracts.AES;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public sealed class EXTCTLTypeOfControls : IEXTCTLTypeOfControls
	{
		public EXTCTLTypeOfControls(string type, string text)
		{
			Type = type;
			Text = text;
		}

		public string Type { get; }
		public string Text { get; }
	}
}
