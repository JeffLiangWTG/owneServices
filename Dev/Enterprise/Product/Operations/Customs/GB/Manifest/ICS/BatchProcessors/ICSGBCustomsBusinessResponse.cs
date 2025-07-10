using CargoWise.Types;
using Enterprise.Customs.GB.CDS;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSGBCustomsBusinessResponse : GBCustomsBusinessResponse
	{
		public ICSGBCustomsBusinessResponse(ZString xml) : base(xml)
		{
		}

		public override ZString ResponseBodyXml
		{
			get
			{
				return SelectSingleNode(NotificationResponseNodeXPath, OutcomeResponseNodeXPath)?.InnerXml ?? base.ResponseBodyXml;
			}
		}

		const string ResponseBodyNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']";
		const string NotificationResponseNodeXPath = $"{ResponseBodyNodeXPath}/*[local-name()='notificationResponse']/*[local-name()='response']";
		const string OutcomeResponseNodeXPath = $"{ResponseBodyNodeXPath}/*[local-name()='outcomeResponse']/*[local-name()='response']";
	}
}
