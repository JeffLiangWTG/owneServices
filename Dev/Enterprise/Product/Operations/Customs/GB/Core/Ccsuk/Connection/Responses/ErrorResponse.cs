namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class ErrorResponse : ShortMessageResponse
	{
		public ErrorResponse(string bodyText)
			: base(bodyText)
		{ }

		public override bool WasOperationSuccessful
		{
			get { return false; }  // duh
		}

		public override string ReasonForFailure
		{
			get
			{
				switch (ResponseCode)
				{
					case "01":
						return "Expecting a Logon Request Message.";
					case "02":
						return "Receive message greater than max message length";
					case "03":
						return "Unknown Service Message";
					case "04":
						return "Error in HTH Header";
					case "05":
						return "Protocol Violation";
					default:
						throw new Exceptions.ShortMessage.UnknownResponseCode(ResponseCode);
				}
			}
		}
	}
}
