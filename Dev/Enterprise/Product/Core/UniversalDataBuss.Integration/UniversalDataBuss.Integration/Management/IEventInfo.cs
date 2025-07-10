using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEventInfo
	{
		ZString EventReference { get; }
		IUser EventUser { get; }
		IBranch EventBranch { get; }
		IDepartment EventDepartment { get; }
	}
}
