using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ISimpleLogResult
	{
		IEnumerable<ISimpleLog> Logs { get; }
	}
}
