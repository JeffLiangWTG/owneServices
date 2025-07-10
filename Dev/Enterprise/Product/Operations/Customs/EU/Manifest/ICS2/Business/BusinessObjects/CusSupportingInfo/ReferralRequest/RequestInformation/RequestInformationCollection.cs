using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestInformationCollection : CusSupportingInfoCollection<RequestInformation>
	{
		public RequestInformationCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.RequestInformation)
		{
		}
	}
}
