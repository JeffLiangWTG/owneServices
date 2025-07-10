namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class LogonResponse : ShortMessageResponse
	{
		public LogonResponse(string bodyText)
			: base(bodyText)
		{ }

		public override string ReasonForFailure
		{
			get
			{
				switch (ResponseCode)
				{
					case "01":
						return "Host Identity Invalid";
					case "02":
						return "Host Barred";
					case "03":
						return "Password Invalid";
					case "04":
						return "Host Password Expired";
					case "05":
						return "Exceeded Max Circuits per host";
					case "06":
						return "Circuit already logged-on";
					default:
						throw new Exceptions.ShortMessage.UnknownResponseCode(ResponseCode);
				}
			}
		}
	}
}
