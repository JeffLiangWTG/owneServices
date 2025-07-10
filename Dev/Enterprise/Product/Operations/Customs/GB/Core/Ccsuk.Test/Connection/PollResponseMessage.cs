using System;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	public class PollResponseMessage : PollRequestMessage
	{
		public PollResponseMessage(ZString bodyText)
		{
			if (!bodyText.StartsWith(ShortMessage.ServiceMessageIdentifier + ShortMessageTypeCodes.Codes.ConnectionPoll))
			{
				throw new ArgumentException("That is not a poll response");
			}
			this.bodyText = bodyText;
		}

		protected override string ShortMessageNumber
		{
			get { return ShortMessageTypeCodes.Codes.ConnectionPoll; }
		}

		protected override string ShortMessageBody  // e.g. 0100
		{
			get { return bodyText.Right(4); }
		}

		readonly ZString bodyText;
	}
}
