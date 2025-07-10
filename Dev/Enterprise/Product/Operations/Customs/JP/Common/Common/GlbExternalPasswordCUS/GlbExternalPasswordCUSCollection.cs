using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public class GlbExternalPasswordCUSCollection : DependentBusinessObjectCollection<GlbExternalPasswordCUS, GlbStaff>
	{
		public GlbExternalPasswordCUSCollection(GlbStaff staff)
			: base(staff, GetExternalPasswordFilter(GlbCompany.CurrentCompany.PK))
		{
		}

		static ZQuery GetExternalPasswordFilter(ZGuid companyPk)
		{
			var passwordTypesQuery = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, JPPasswordType.Codes.CUS);

			var result = new ZQuery(GlbExternalPasswordSchema.GP_GC, companyPk);
			result.AddToFilter(passwordTypesQuery);
			return result;
		}
	}
}
