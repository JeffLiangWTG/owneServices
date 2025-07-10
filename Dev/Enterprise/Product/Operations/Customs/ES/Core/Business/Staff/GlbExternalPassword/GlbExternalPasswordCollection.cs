using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPasswordCollection : DependentBusinessObjectCollection<GlbExternalPassword, GlbStaff>, Enterprise.MasterFiles.Integration.Customs.ES.IGlbExternalPasswordCollection
	{
		public GlbExternalPasswordCollection(GlbStaff staff)
			: base(staff, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ESB))
		{
		}
	}
}
