using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class GlbExternalPasswordCollection_GB : DependentBusinessObjectCollection<GlbExternalPassword_GB, GlbCompany>, Enterprise.MasterFiles.Integration.Customs.GB.IGlbExternalPasswordCollection_GB
	{
		public GlbExternalPasswordCollection_GB(GlbCompany company)
			: base(company, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CDS))
		{
		}
	}
}
