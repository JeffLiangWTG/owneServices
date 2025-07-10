using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class GlbMauExternalPasswordCollection : DependentBusinessObjectCollection<GlbMauExternalPassword, GlbCompany>, MasterFiles.Integration.CustomsIntegration.IT.IGlbMauExternalPasswordCollection
{
	public GlbMauExternalPasswordCollection(GlbCompany company) : base(company, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ITM))
	{
	}
}
