namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class PollRequestMessage : ShortMessage
	{
		protected override string ShortMessageNumber
		{
			get { return ShortMessageTypeCodes.Codes.ConnectionPoll; }
		}

		protected override string ShortMessageBody
		{
			get { return PollCode + ResponseCode; }
		}

		string PollCode
		{
			get
			{
				return "00";
			}
		}

		string ResponseCode
		{
			get
			{
				return "00";
			}
		}
	}
}
