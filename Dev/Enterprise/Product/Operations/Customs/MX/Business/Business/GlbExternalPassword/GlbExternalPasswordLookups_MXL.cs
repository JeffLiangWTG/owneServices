using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Business
{
	public class GlbExternalPasswordLookups_MXL : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups_MXL(GlbExternalPassword_MXL parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_MXL Parent => (GlbExternalPassword_MXL)base.Parent;

		public ZZRefCusCodeListCombinedCollection CustomsFacilities => MXRefCusCodeListTypes.GetCustomsFacilities(Factory);
	}
}
