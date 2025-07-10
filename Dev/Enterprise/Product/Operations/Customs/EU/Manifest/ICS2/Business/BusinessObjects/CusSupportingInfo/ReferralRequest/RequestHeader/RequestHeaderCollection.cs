using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestHeaderCollection : EUMemberStateCommunicationCollection<RequestHeader>
	{
		public RequestHeaderCollection(BusinessObject parent)
			: base(parent)
		{
			MaxCountValidationEnable(99);
		}
	}
}
