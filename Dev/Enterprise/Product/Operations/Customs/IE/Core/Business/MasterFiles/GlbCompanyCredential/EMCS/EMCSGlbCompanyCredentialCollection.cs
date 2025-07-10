using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class EMCSGlbCompanyCredentialCollection : DependentBusinessObjectCollection<EMCSGlbCompanyCredential, GlbCompany>, IGlbExternalPasswordCollection_IEEMCS
	{
		public EMCSGlbCompanyCredentialCollection(GlbCompany company) : base(company, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.IEM))
		{
		}
	}
}
