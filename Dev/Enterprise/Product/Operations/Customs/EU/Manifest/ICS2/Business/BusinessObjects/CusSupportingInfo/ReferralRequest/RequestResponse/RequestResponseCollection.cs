using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestResponseCollection : CusSupportingInfoCollection<RequestResponse>
	{
		public RequestResponseCollection(RequestHeader parent)
			: base(parent, CusSupportingInfoTypeList.Codes.RequestResponse)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var requestResponse = (RequestResponse)child;

			var header = Master as RequestHeader;
			if (header != null && header.EUS_Type == EUICS2ReferralRequestTypeList.Codes.CL735_RFS)
			{
				requestResponse.CSI_SubType = EUICS2HRCMAdditionalInfoTypes.Codes.CL703_R4;
			}
		}
	}
}
