using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IParentEventDataContextManager : IEventDataContextManager
	{
		IEnumerable<IEventDataContextManager> ChildContextManagers { get; }
	}
}