using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public abstract class MessageProvider : IHeader
	{
		protected MessageProvider()
		{
			PreparationDateAndTime = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, true);
		}

		public DateTime PreparationDateAndTime { get; private set; }
	}
}
