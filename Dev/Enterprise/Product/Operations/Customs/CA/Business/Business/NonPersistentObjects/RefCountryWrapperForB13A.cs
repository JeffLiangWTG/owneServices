using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business;

public class RefCountryWrapperForB13A : NonPersistentBusinessObject
{
	public RefCountryWrapperForB13A(string rN_Desc = "")
	{
		RN_Desc = rN_Desc;
	}

	public ZString RN_Desc { get; }
}
