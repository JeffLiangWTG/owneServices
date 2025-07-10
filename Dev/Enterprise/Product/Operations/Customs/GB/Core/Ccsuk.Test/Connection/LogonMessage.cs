namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	public class LogonMessage : Connection.LogonMessage
	{
		public LogonMessage(string body)
		{
			this.body = body;
		}

		protected override string ShortMessageBody
		{
			get
			{
				return body;
			}
		}
		readonly string body;
	}
}
