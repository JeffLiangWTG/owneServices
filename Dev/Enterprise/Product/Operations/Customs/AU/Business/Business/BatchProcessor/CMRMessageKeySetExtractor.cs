using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMessageKeySetExtractor : IMessageKeySetExtractor
	{
		public MessageKeySet ExtractKeys(EDIMessage message)
		{
			var messageKeys = new List<string>();

			if (message is CMRCUSRESMessage cusResMessage)
			{
				if (message is CMRCARSTMessage carstMessage)
				{
					var ob = carstMessage.OceanBillNumber.Trim();
					var mawb = carstMessage.MAWB.Trim();

					if (!mawb.IsEmpty)
					{
						messageKeys.Add("MB:" + mawb);
					}

					if (!ob.IsEmpty)
					{
						messageKeys.Add("OB:" + ob);
					}

					if (mawb.IsEmpty && ob.IsEmpty)
					{
						messageKeys.Add("MB/OB:EMPTY");
					}
				}
				else
				{
					messageKeys.Add(FormattableString.Invariant($"SendersReference:{cusResMessage.SendersReference}"));
				}
			}

			if (message is ICMRDepotMessage depotMessage)
			{
				// this is the key used in CMRSeaDepotMessageProcessor.LockOutturnHeaderOrThrowConcurrencyException
				messageKeys.Add(string.Format(
					CultureInfo.InvariantCulture,
					"CusOutturnHeader:{0}:{1}:{2}",
					depotMessage.LloydsNumber,
					depotMessage.OurPremiseID,
					depotMessage.VoyageNumber
				));
			}

			if (messageKeys.Count == 0)
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Message (PK={message.PK}, Type='{message.EM_MessageType}') has no associated keys."));
			}

			return new MessageKeySet(messageKeys);
		}
	}
}
