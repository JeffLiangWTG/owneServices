namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class LogonMessage : ShortMessage
	{
		protected override string ShortMessageNumber
		{
			get { return ShortMessageTypeCodes.Codes.LogonRequest; }
		}

		protected override string ShortMessageBody
		{
			get { return Password; }
		}

		string Password
		{
			get
			{
				return new Password().Existing;
			}
		}
	}
}
