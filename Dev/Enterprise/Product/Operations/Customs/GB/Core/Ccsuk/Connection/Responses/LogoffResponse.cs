namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class LogoffResponse : ShortMessageResponse
	{
		public LogoffResponse(string bodyText) : base(bodyText)
		{ }

		public override string ReasonForFailure
		{
			get
			{
				switch (ResponseCode)
				{
					case "01":
						return "Host Identity different to logged-on Host Identity on this circuit";
					case "02":
						return "No Logon Session current on this circuit";
					default:
						throw new Exceptions.ShortMessage.UnknownResponseCode(ResponseCode);
				}
			}
		}
	}
}
