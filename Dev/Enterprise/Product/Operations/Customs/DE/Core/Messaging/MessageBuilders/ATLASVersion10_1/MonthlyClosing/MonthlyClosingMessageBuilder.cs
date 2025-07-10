using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public abstract class MonthlyClosingMessageBuilder<T> : MessageBuilder<T> where T : class
	{
		public List<int> LineNumbersInMessage = new List<int>();

		protected void AddToLineNumbersInMessage(int lineNumber) => LineNumbersInMessage.Add(lineNumber);
	}
}
