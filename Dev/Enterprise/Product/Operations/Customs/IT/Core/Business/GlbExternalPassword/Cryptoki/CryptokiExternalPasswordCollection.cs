using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class CryptokiExternalPasswordCollection : OneItemPasswordCollection<CryptokiExternalPassword>
{
	public CryptokiExternalPasswordCollection(GlbStaff staff)
		: base(staff, ValidationCaptions.CryptokiExternalPassword.OnlyOneXadesCertificateIsAllowed, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ITX))
	{
	}

	public CryptokiExternalPassword GetCryptokiCertificate() => (CryptokiExternalPassword)this.FirstOrDefault();
}
