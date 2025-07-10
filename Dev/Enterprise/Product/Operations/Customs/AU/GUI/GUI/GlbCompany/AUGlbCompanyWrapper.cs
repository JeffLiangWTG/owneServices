using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUGlbCompanyWrapper : GlbCompanyWrapper, MasterFiles.Integration.Customs.AU.IAUGlbCompanyWrapper
	{
		public AUGlbCompanyWrapper(GlbCompany company) : base(company)
		{
		}

		public override bool IsValidWrapper => true;
	}
}
