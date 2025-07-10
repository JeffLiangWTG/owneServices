using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	abstract class PrintFromMessageProvider
	{
		public PrintFromMessageProvider(EDIMessage baseMessage, ILogger logger)
		{
			gbEdiMessage = baseMessage.Factory.Load<GbEDIMessage>(baseMessage.PK);
			this.logger = logger;
		}

		ICcsukCusAwb awb;
		protected ICcsukCusAwb Awb
		{
			get
			{
				if (awb == null)
				{
					var hawb = gbEdiMessage.Factory.Load<CusHAWB>(gbEdiMessage.EM_LinkUniqueID);
					if (hawb != null)
					{
						awb = hawb.CS_IsMasterHouse ? hawb.MAWB : hawb;
					}
					if (awb != null && awb.HasSplits && !gbEdiMessage.EM_ApplicationReference.IsEmpty)
					{
						awb = awb.Splits[gbEdiMessage.EM_ApplicationReference];
					}
				}
				return awb;
			}
		}

		public abstract void DoPrinting();

		protected GbEDIMessage gbEdiMessage;
		protected ILogger logger;
	}
}
