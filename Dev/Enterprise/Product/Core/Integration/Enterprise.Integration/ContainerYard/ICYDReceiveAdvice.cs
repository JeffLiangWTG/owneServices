using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDReceiveAdvice
	{
		ZGuid PK { get; }
		KeyValuePair<string, string> MappedCommunityCode { get; }
	}
}
