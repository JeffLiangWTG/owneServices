namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class PasswordRequestMessage : ShortMessage
	{
		readonly string newPassword;
		public PasswordRequestMessage(string newPassword)
		{
			this.newPassword = newPassword;
		}

		protected override string ShortMessageBody
		{
			get
			{
				Password password = new Password();
				return password.Existing + password.Format(this.newPassword);
			}
		}

		protected override string ShortMessageNumber
		{
			get { return ShortMessageTypeCodes.Codes.PasswordChangeRequest; }
		}
	}
}
