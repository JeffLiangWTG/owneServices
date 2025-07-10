using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Messages.APERAK;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class APERAKGetter
	{
		public APERAKGetter(EDIMessage message)
		{
			this.message = message;
			isTranslated = false;
		}
		readonly EDIMessage message;
		bool isTranslated;

		APERAKMessage fAPERAK;
		public APERAKMessage APERAK
		{
			get
			{
				if (!isTranslated && message != null)
				{
					SegmentGroup baseMessage = null;
					try
					{
						baseMessage = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());
					}
					catch (InvalidFormatException)
					{
						// Ignore the message
					}
					fAPERAK = baseMessage as APERAKMessage;
					isTranslated = true;
				}
				return fAPERAK;
			}
		}
	}
}
