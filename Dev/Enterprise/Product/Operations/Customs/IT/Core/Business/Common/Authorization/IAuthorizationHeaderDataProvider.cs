using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IAuthorizationHeaderDataProvider
{
	ZString AuthorizationNumber { get; }
	IEnumerable<ZString> AuthorizationTypes { get; }
	ZGuid HolderPk { get; }
}
