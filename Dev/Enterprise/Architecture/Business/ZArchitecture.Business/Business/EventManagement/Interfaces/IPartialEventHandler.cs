using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface IPartialEventHandler
	{
		void Handle(IStmALog log, IStmALogParent master);
	}
}
