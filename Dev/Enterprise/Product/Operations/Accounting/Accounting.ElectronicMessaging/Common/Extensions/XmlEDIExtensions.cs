using CargoWise.EntityFramework;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class XmlEDIExtensions
	{
		public static XmlEDIMessage[] LoadMessages(this IXmlEDIInterchange interchange)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			return interchange.Factory.Load<XmlEDIMessage>(query);
		}
	}
}