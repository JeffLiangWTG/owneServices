using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IXmlRootNameMappingProvider
	{
		IReadOnlyDictionary<ZString, (ZString ApplicationCode, ZString MessageType)> GetXmlRootNameMapping();
	}
}
