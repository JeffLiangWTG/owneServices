using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class AutomaticSignatureExternalPasswordCollection : OneItemPasswordCollection<AutomaticSignatureExternalPassword>
{
	public AutomaticSignatureExternalPasswordCollection(GlbStaff staff)
		: base(staff, ValidationCaptions.AutomaticSignatureExternalPassword.OnlyOneAutomaticSignatureEntryIsAllowed, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ITA))
	{
	}
}
