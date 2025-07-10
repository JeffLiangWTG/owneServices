using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public class GenericMessageNumberStrategy : IMessageNumberStrategy
	{
		readonly BusinessObjectFactory factory;
		public string InterchangeNumber
		{
			get;
			private set;
		}

		public GenericMessageNumberStrategy(BusinessObjectFactory factory, string interchangeNumber)
		{
			this.factory = factory;
			InterchangeNumber = interchangeNumber;
		}

		public string GetMessageReferenceNumber()
		{
			return InterchangeNumber.IsNullOrEmpty() ? Environment.Env.Instance.NumberFountains.XmlEDIInterchangeNumber.GetNextFormatted(factory) : InterchangeNumber;
		}
	}
}
