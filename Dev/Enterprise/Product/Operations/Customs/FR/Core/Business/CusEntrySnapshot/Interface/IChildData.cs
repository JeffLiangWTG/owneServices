using Enterprise.Customs.FR.Business.Snapshot;

namespace Enterprise.Customs.FR.Business.Interfaces.Snapshot
{
	public interface IChildData
	{
		ChildType Type { get; }
		string Code { get; }
		string Reference { get; }
	}
}
