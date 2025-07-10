namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public abstract class ShortMessage : ServiceMessage
	{
		protected sealed override string ServiceMessageBody
		{
			get
			{
				return ShortMessageBody;
			}
		}

		protected abstract string ShortMessageBody { get; }

		protected abstract string ShortMessageNumber { get; }

		protected sealed override string MessageTypeNumber
		{
			get { return ShortMessageNumber; }
		}
	}
}
