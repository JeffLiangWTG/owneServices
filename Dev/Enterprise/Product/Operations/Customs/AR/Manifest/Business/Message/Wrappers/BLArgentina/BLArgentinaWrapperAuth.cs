using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	class BLArgentinaWrapperAuth : IAuth
	{
		string IAuth.CompanyCUIT => ZString.Empty;

		string IAuth.CompanyRol => ZString.Empty;

		string IAuth.AgentType => ZString.Empty;
	}
}
