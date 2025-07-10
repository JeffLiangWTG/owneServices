using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	/// <summary>
	/// By using this interface you are bypassing all of the generic event handling code that causes Workflow to Fire.
	/// Use at own risk. (Not recommended for business logic)
	/// </summary>
	public interface ICustomProcessTaskHandlerProvider
	{
		IProcessTaskHandler GetHandler(IStmALog log);
	}
}
