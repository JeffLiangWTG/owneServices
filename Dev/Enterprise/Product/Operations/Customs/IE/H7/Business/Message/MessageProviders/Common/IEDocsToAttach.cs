using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.H7.Business
{
	public interface IEDocsToAttach
	{
		IReadOnlyDictionary<ZGuid, string> EDocsToAttach { get; }
	}
}
