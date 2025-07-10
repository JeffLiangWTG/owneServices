using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Integration
{
	public interface ITaskStatusFilter
	{
		ZString TaskAggregator { get; set; }
		ZBoolDescriptionPairList TaskTypeCheckList { get; }
		ZBoolDescriptionPairList TaskStatusCheckList { get; }
	}
}
