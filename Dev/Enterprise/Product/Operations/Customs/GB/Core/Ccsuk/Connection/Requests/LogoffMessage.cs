namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class LogoffMessage : ShortMessage
	{
		protected override string ShortMessageBody
		{
			get { return string.Empty; }
		}
		protected override string ShortMessageNumber
		{
			get { return ShortMessageTypeCodes.Codes.LogoffRequest; }
		}
	}
}
