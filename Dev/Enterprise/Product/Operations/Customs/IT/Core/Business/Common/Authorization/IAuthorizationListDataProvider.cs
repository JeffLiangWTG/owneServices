using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IAuthorizationListDataProvider
{
	IEnumerable<ZString> AuthorizationTypes { get; }
	IEnumerable<ZGuid> GetEligibleHolders();
}
