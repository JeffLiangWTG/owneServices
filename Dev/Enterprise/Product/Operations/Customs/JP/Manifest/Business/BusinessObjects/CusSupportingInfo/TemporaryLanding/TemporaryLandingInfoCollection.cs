using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class TemporaryLandingInfoCollection : CusSupportingInfoCollection<TemporaryLandingInfo>
	{
		public TemporaryLandingInfoCollection(AsycudaBill bill) : base(bill, CusSupportingInfoTypeList.Codes.ApprovalCertificate)
		{
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < 1;
	}
}
