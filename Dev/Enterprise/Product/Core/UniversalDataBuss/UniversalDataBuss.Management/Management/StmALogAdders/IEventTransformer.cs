using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public interface IEventTransformer
	{
		EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent);
	}
}
