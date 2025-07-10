using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMHeaderCompanyRegisterWrapper : IIMHeaderCompanyRegister
{
	public ZString Number => ZString.Empty;

	public ZString Series => ZString.Empty;

	public ZDate Date => ZDate.Empty;
}
