namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class PasswordResponse : ShortMessageResponse
	{
		public PasswordResponse(string bodyText)
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
						return "Existing Password Incorrect";
					case "04":
						return "New password same as existing password";
					case "05":
						return "New password too long";
					case "06":
						return "Host does not use password (Call Logon)";
					case "07":
						return "Host Identity in message differs from Host Mnemonic indicated by Handshake Message ";
					case "08":
						return "New password same as previous password";
					case "09":
						return "Password contains illegal characters";

					default:
						throw new Exceptions.ShortMessage.UnknownResponseCode(ResponseCode);
				}
			}
		}

		public void SaveIfSuccessful(string newPassword)
		{
			if (this.WasOperationSuccessful)
			{
				new Password().Save(newPassword);
			}
		}
	}
}
